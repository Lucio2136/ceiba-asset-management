namespace CeibaAssets.Domain.Common;

/// <summary>
/// Se lanza cuando una operación viola una invariante del dominio.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException()
    {
    }

    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
