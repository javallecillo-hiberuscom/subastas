namespace Subastas.Domain.Exceptions;

/// <summary>
/// Excepción base para la capa de dominio.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Excepción cuando no se encuentra una entidad.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} con ID {id} no encontrado")
    {
    }

    public EntityNotFoundException(string message) : base(message)
    {
    }
}

/// <summary>
/// Excepción cuando se viola una regla de negocio.
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}

/// <summary>
/// Excepción cuando ya existe una entidad duplicada.
/// </summary>
public class DuplicateEntityException : DomainException
{
    public DuplicateEntityException(string message) : base(message)
    {
    }
}

/// <summary>
/// Excepción cuando las credenciales son inválidas.
/// </summary>
public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException() : base("Credenciales inválidas")
    {
    }
}
