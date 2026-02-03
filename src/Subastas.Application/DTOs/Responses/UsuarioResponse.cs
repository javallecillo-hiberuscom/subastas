namespace Subastas.Application.DTOs.Responses;

/// <summary>
/// DTO de respuesta con información de usuario.
/// </summary>
public class UsuarioResponse
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Apellidos { get; set; }
    public string Email { get; set; } = null!;
    public string Rol { get; set; } = null!;
    public bool Activo { get; set; }
    public bool Validado { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public int? IdEmpresa { get; set; }
    public string? NombreEmpresa { get; set; }
    public string? FotoPerfilBase64 { get; set; }
    public string? DocumentoIAE { get; set; }
}
