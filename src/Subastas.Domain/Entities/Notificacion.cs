using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Subastas.Domain.Entities;

/// <summary>
/// Representa una notificación enviada a un usuario sobre una subasta.
/// </summary>
[Table("Notificacion")]
public class Notificacion
{
    /// <summary>
    /// Identificador único de la notificación.
    /// </summary>
    [Key]
    public int IdNotificacion { get; set; }

    /// <summary>
    /// Identificador del usuario que recibe la notificación.
    /// </summary>
    public int IdUsuario { get; set; }

    /// <summary>
    /// Identificador de la subasta relacionada.
    /// </summary>
    public int IdSubasta { get; set; }

    /// <summary>
    /// Mensaje de la notificación.
    /// </summary>
    public string Mensaje { get; set; } = null!;

    /// <summary>
    /// Fecha y hora de envío de la notificación.
    /// </summary>
    [Precision(0)]
    public DateTime FechaEnvio { get; set; }

    /// <summary>
    /// Indica si la notificación ha sido leída (0 = no, 1 = sí).
    /// </summary>
    public byte Leida { get; set; }

    // Relaciones
    [ForeignKey(nameof(IdSubasta))]
    [InverseProperty("Notificaciones")]
    public virtual Subasta Subasta { get; set; } = null!;

    [ForeignKey(nameof(IdUsuario))]
    [InverseProperty("Notificaciones")]
    public virtual Usuario Usuario { get; set; } = null!;
}
