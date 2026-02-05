using System;

namespace Subastas.Application.Dtos
{
 public class SubastaDto
 {
 public string? Titulo { get; set; }
 public DateTime Fecha { get; set; }
 public decimal Precio { get; set; }
 // Additional fields for richer PDF
 public string? Descripcion { get; set; }
 // Base64 encoded image (optional)
 public string? ImagenBase64 { get; set; }
 }
}
