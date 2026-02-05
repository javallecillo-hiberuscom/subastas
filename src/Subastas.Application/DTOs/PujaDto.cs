using System;
using System.Collections.Generic;

namespace Subastas.Application.Dtos
{
 public class PujaDto
 {
 public int IdPuja { get; set; }
 public int IdSubasta { get; set; }
 public int IdUsuario { get; set; }
 public decimal Cantidad { get; set; }
 public DateTime FechaPuja { get; set; }
 public object Usuario { get; set; } = null!;
 public object Subasta { get; set; } = null!;
 public bool EstaGanando { get; set; }
 }
}