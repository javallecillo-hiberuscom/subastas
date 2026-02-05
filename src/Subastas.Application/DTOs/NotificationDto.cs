using System;

namespace Subastas.Application.Dtos
{
 public class NotificationDto
 {
 public int Id { get; set; }
 public string Tipo { get; set; } = null!; // "usuario" or custom type for admin
 public string? Titulo { get; set; }
 public string Mensaje { get; set; } = null!;
 public DateTime Fecha { get; set; }
 public bool Leida { get; set; }
 public int? UsuarioId { get; set; }
 public string? UsuarioNombre { get; set; }
 public int? SubastaId { get; set; }
 public bool EsAdmin { get; set; }
 }
}
