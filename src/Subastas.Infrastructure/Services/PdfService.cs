using System;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Subastas.Application.Dtos;
using Subastas.Application.Interfaces.Services;

namespace Subastas.Infrastructure.Services
{
 public class PdfService : IPdfService
 {
 public byte[] GenerateSubastasPdf(IEnumerable<SubastaDto> items, DateTime from, DateTime to)
 {
 var document = Document.Create(container =>
 {
 container.Page(page =>
 {
 page.Margin(20);
 page.Size(PageSizes.A4);

 page.Header().Row(row =>
 {
 row.RelativeColumn().Column(col =>
 {
 col.Item().Text("Informe de Subastas").FontSize(20).SemiBold();
 col.Item().Text($"Periodo: {from:yyyy-MM-dd} ? {to:yyyy-MM-dd}").FontSize(10).FontColor(Colors.Grey.Darken2);
 });

 row.ConstantColumn(100).AlignRight().Column(c =>
 {
 c.Item().Text("Subastas").FontSize(14).SemiBold();
 });
 });

 page.Content().PaddingVertical(10).Column(col =>
 {
 foreach (var it in items)
 {
 col.Item().Padding(8).Border(1).BorderColor(Colors.Grey.Lighten3).Background(Colors.White).Column(card =>
 {
 card.Item().Text(it.Titulo ?? string.Empty).FontSize(14).SemiBold();

 // Image (if present)
 if (!string.IsNullOrWhiteSpace(it.ImagenBase64))
 {
 try
 {
 var bytes = Convert.FromBase64String(it.ImagenBase64);
 card.Item().Height(80).Image(bytes, ImageScaling.FitArea);
 }
 catch
 {
 card.Item().Height(80).Background(Colors.Grey.Lighten3).AlignCenter().Text("No image");
 }
 }
 else
 {
 card.Item().Height(80).Background(Colors.Grey.Lighten3).AlignCenter().Text("No image");
 }

 if (!string.IsNullOrWhiteSpace(it.Descripcion))
 {
 card.Item().PaddingTop(4).Text(it.Descripcion).FontSize(10).FontColor(Colors.Grey.Darken2);
 }

 card.Item().Row(r =>
 {
 r.RelativeColumn().Text(it.Fecha.ToString("yyyy-MM-dd")).FontSize(10).FontColor(Colors.Grey.Darken2);
 r.ConstantColumn(120).AlignRight().Text(it.Precio.ToString("C")).FontSize(12).SemiBold();
 });
 });

 // spacer between cards
 col.Item().Height(6);
 }
 });

 page.Footer().AlignCenter().Text(x => x.CurrentPageNumber().FontSize(9));
 });
 });

 return document.GeneratePdf();
 }
 }
}
