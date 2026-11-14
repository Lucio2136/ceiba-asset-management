namespace CeibaAssets.Application.Common;

/// <summary>
/// Resultado de un caso de uso: un valor o un error de negocio (sin excepciones para el flujo normal).
/// </summary>
public sealed class Result<T>
{
    private readonly T? _value;

    internal Result(T value)
    {
        _value = value;
        IsSuccess = true;
    }

    internal Result(ApplicationError error)
    {
        Error = error;
    }

    public bool IsSuccess { get; }

    public ApplicationError? Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("El resultado es un error y no tiene valor.");
}

public static class Result
{
    public static Result<T> Success<T>(T value) => new(value);

    public static Result<T> Failure<T>(ApplicationError error) => new(error);

    public static Result<T> NotFound<T>(string detail) => new(new ApplicationError(ErrorKind.NotFound, detail));

    public static Result<T> Conflict<T>(string detail) => new(new ApplicationError(ErrorKind.Conflict, detail));

    public static Result<T> Invalid<T>(string field, string message) => new(new ApplicationError(
        ErrorKind.Validation,
        "Uno o más campos no son válidos.",
        new Dictionary<string, string[]> { [field] = [message] }));
}
