using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Subastas.Infrastructure.Data;
using Subastas.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Habilitar logging detallado para Azure
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// En Azure App Service, esto enviará logs a la consola
if (builder.Environment.IsProduction())
{
    builder.Logging.AddApplicationInsights();
}

try
{
    // Configuración de controladores con manejo de ciclos
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Permitir cualquier case
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // Convertir a camelCase para JavaScript
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true; // Desactivar validación automática para manejarla manualmente
        });

    builder.Services.AddEndpointsApiExplorer();

    // Configuración de Swagger con autenticación JWT
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Subastas API",
            Description = "API para gestión de subastas de vehículos - Arquitectura Limpia",
            Contact = new OpenApiContact
            {
                Name = "Equipo de Desarrollo",
                Email = "dev@subastas.com"
            }
        });

        // Incluir comentarios XML
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        options.CustomSchemaIds(type => type.FullName);

        // Mapear IFormFile para Swagger
        options.MapType<IFormFile>(() => new OpenApiSchema
        {
            Type = "string",
            Format = "binary"
        });

        // Configurar JWT en Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // Configuración de Base de Datos con retry logic
    var connectionString = builder.Configuration.GetConnectionString("SubastaConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("La cadena de conexión 'SubastaConnection' no está configurada.");
    }

    builder.Services.AddDbContext<SubastaContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

    // Configuración de JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];

    if (string.IsNullOrEmpty(secretKey))
    {
        throw new InvalidOperationException("JWT SecretKey no configurada. Configure la variable de entorno 'JwtSettings__SecretKey' en Azure App Service.");
    }

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero,
            // Configurar validación de roles case-insensitive
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        // Política para administradores (case-insensitive)
        options.AddPolicy("AdminPolicy", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole("Admin") || context.User.IsInRole("admin") || context.User.IsInRole("ADMIN")
            )
        );
    });

    // Registrar servicios de Infrastructure (Repositorios y Servicios)
    builder.Services.AddInfrastructure();

    // Configuración de CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:4200", "http://localhost:4201" };

            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    var app = builder.Build();

    // Log de inicio
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Iniciando aplicación Subastas API en entorno: {Environment}", app.Environment.EnvironmentName);

    // Configuración del pipeline HTTP
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    // Habilitar Swagger
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Subastas API v1");
        options.RoutePrefix = string.Empty; // Swagger en la raíz
    });

    // Servir archivos estáticos con manejo de errores
    try
    {
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
            logger.LogInformation("Directorio Uploads creado en: {Path}", uploadsPath);
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPath),
            RequestPath = "/uploads"
        });

        var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
        if (!Directory.Exists(imgPath))
        {
            Directory.CreateDirectory(imgPath);
            logger.LogInformation("Directorio img creado en: {Path}", imgPath);
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(imgPath),
            RequestPath = "/img"
        });
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "No se pudieron crear directorios para archivos estáticos. Continuando sin ellos.");
    }

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Endpoint de manejo de errores global
    app.MapGet("/error", () => Results.Problem("Ha ocurrido un error en el servidor."))
        .ExcludeFromDescription();

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
        .ExcludeFromDescription();

    logger.LogInformation("Aplicación configurada correctamente. Iniciando servidor...");

    app.Run();
}
catch (Exception ex)
{
    // Log crítico si falla el startup
    var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogCritical(ex, "Error fatal durante el inicio de la aplicación");
    throw;
}
