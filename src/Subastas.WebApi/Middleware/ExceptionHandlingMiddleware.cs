using System.Net;
using System.Text.Json;
using Subastas.Application.DTOs.Responses;
using Subastas.Domain.Exceptions;

namespace Subastas.WebApi.Middleware;

/// <summary>
/// Middleware para manejo global de excepciones.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            EntityNotFoundException notFoundEx => new
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Response = ApiResponse<object>.ErrorResult(notFoundEx.Message)
            },
            DuplicateEntityException duplicateEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = ApiResponse<object>.ErrorResult(duplicateEx.Message)
            },
            BusinessRuleException businessEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = ApiResponse<object>.ErrorResult(businessEx.Message)
            },
            InvalidCredentialsException credEx => new
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Response = ApiResponse<object>.ErrorResult(credEx.Message)
            },
            _ => new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Response = ApiResponse<object>.ErrorResult(
                    "Error interno del servidor",
                    new List<string> { exception.Message })
            }
        };

        context.Response.StatusCode = response.StatusCode;

        // Log del error
        if (response.StatusCode >= 500)
        {
            _logger.LogError(exception, "Error interno del servidor: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Error de cliente: {Message}", exception.Message);
        }

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response.Response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}
