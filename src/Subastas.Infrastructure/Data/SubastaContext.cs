using Microsoft.EntityFrameworkCore;
using Subastas.Domain.Entities;

namespace Subastas.Infrastructure.Data;

/// <summary>
/// Contexto principal de la base de datos para la aplicación de subastas.
/// Implementa el patrón Repository usando Entity Framework Core.
/// </summary>
public class SubastaContext : DbContext
{
    /// <summary>
    /// Constructor para inyección de dependencias.
    /// </summary>
    public SubastaContext(DbContextOptions<SubastaContext> options)
        : base(options)
    {
    }

    #region DbSets

    public virtual DbSet<Empresa> Empresas { get; set; }
    public virtual DbSet<ImagenVehiculo> ImagenesVehiculo { get; set; }
    public virtual DbSet<Notificacion> Notificaciones { get; set; }
    public virtual DbSet<NotificacionAdmin> NotificacionesAdmin { get; set; }
    public virtual DbSet<Pago> Pagos { get; set; }
    public virtual DbSet<Puja> Pujas { get; set; }
    public virtual DbSet<Subasta> Subastas { get; set; }
    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<Vehiculo> Vehiculos { get; set; }

    #endregion

    /// <summary>
    /// Configuración del modelo y relaciones entre entidades.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Empresa
        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.IdEmpresa).HasName("PK__Empresa__75D2CED44086D3C7");
        });

        // ImagenVehiculo
        modelBuilder.Entity<ImagenVehiculo>(entity =>
        {
            entity.HasKey(e => e.IdImagen).HasName("PK__ImagenVe__EA9A7136BBBF0FF8");
            entity.Property(e => e.Activo).HasDefaultValue((byte)1);
            entity.HasOne(d => d.Vehiculo)
                .WithMany(p => p.ImagenesVehiculo)
                .HasForeignKey(d => d.IdVehiculo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImagenVehiculo_Vehiculo");
        });

        // Notificacion
        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.ToTable("Notificacion");
            entity.HasKey(e => e.IdNotificacion).HasName("PK__Notifica__AFE1D7E48069B189");
            entity.HasOne(d => d.Subasta)
                .WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdSubasta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notificacion_Subasta");
            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notificacion_Usuario");
        });

        // NotificacionAdmin
        modelBuilder.Entity<NotificacionAdmin>(entity =>
        {
            entity.ToTable("NotificacionAdmin");
            entity.HasKey(e => e.IdNotificacion);
            entity.HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Pago
        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PK__Pago__BD2295AD68E6A9EE");
            entity.HasOne(d => d.Puja)
                .WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdPuja)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_Puja");
        });

        // Puja
        modelBuilder.Entity<Puja>(entity =>
        {
            entity.HasKey(e => e.IdPuja).HasName("PK__Puja__B4598EC6C86DB51A");
            entity.HasOne(d => d.Subasta)
                .WithMany(p => p.Pujas)
                .HasForeignKey(d => d.IdSubasta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Puja_Subasta");
            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.Pujas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Puja_Usuario");
        });

        // Subasta
        modelBuilder.Entity<Subasta>(entity =>
        {
            entity.HasKey(e => e.IdSubasta).HasName("PK__Subasta__710C9B2556003E5C");
            entity.Property(e => e.Estado).HasDefaultValue("activa");
            entity.HasOne(d => d.Vehiculo)
                .WithMany(p => p.Subastas)
                .HasForeignKey(d => d.IdVehiculo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Subasta_Vehiculo");
        });

        // Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__645723A6CA1F7D0A");
            entity.Property(e => e.Activo).HasDefaultValue((byte)1);
            entity.Property(e => e.Rol).HasDefaultValue("registrado");
            entity.HasOne(d => d.Empresa)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Usuario_Empresa")
                .IsRequired(false);
        });

        // Vehiculo
        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasKey(e => e.IdVehiculo).HasName("PK__Vehiculo__4868297046AD87EF");
            entity.Property(e => e.Estado).HasDefaultValue("registrado");
        });
    }
}
