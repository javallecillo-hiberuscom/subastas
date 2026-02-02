using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Subastas.Domain.Entities;

/// <summary>
/// Representa una notificación administrativa del sistema.
/// </summary>
[Table("NotificacionAdmin")]
public class NotificacionAdmin
{
    /// <summary>
    /// Identificador único de la notificación administrativa.
    /// </summary>
    [Key]
    public int IdNotificacion { get; set; }

    /// <summary>
    /// Título de la notificación.
    /// </summary>
    [Required]
    [StringLength(200)]
    [Unicode(false)]
    public string Titulo { get; set; } = null!;

    /// <summary>
    /// Mensaje descriptivo de la notificación.
    /// </summary>
    [Required]
    [StringLength(500)]
    [Unicode(false)]
    public string Mensaje { get; set; } = null!;

    /// <summary>
    /// Tipo de notificación (registro, documento_subido, puja, otro).
    /// </summary>
    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string Tipo { get; set; } = null!;

    /// <summary>
    /// Identificador del usuario relacionado (opcional).
    /// </summary>
    public int? IdUsuario { get; set; }

    /// <summary>
    /// Indica si la notificación ha sido leída (0 = no, 1 = sí).
    /// </summary>
    public byte Leida { get; set; } = 0;

    /// <summary>
    /// Fecha de creación de la notificación.
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    /// <summary>
    /// Datos adicionales en formato JSON si es necesario.
    /// </summary>
    [StringLength(500)]
    [Unicode(false)]
    public string? DatosAdicionales { get; set; }

    // Relaciones
    [ForeignKey(nameof(IdUsuario))]
    public virtual Usuario? Usuario { get; set; }
}
