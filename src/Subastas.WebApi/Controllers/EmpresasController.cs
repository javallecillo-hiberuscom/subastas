using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subastas.Infrastructure.Data;
using Subastas.Domain.Entities;

namespace Subastas.WebApi.Controllers;

/// <summary>
/// Controlador API para CRUD sobre la entidad Empresa.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class EmpresasController : ControllerBase
{
    private readonly SubastaContext _context;

    public EmpresasController(SubastaContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene la lista de todas las empresas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Empresa>>> GetEmpresas()
        => await _context.Empresas.ToListAsync();

    /// <summary>
    /// Obtiene una empresa por su identificador.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Empresa>> GetEmpresa(int id)
    {
        var empresa = await _context.Empresas.FindAsync(id);
        return empresa == null ? NotFound() : empresa;
    }

    /// <summary>
    /// Crea una nueva empresa.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostEmpresa([FromBody] Empresa empresa)
    {
        _context.Empresas.Add(empresa);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEmpresa), new { id = empresa.IdEmpresa }, empresa);
    }

    /// <summary>
    /// Actualiza una empresa existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutEmpresa(int id, [FromBody] Empresa empresa)
    {
        if (id != empresa.IdEmpresa) return BadRequest();

        _context.Entry(empresa).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Empresas.Any(e => e.IdEmpresa == id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Elimina una empresa por su identificador.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmpresa(int id)
    {
        var empresa = await _context.Empresas.FindAsync(id);
        if (empresa == null) return NotFound();

        _context.Empresas.Remove(empresa);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
