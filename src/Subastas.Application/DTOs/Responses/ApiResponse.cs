namespace Subastas.Application.DTOs.Responses;

/// <summary>
/// DTO de respuesta genérica para operaciones.
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Datos de respuesta.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Lista de errores (si los hay).
    /// </summary>
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "Operación exitosa")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
