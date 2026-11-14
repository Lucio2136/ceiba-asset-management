using FluentValidation;
using FluentValidation.Results;

namespace CeibaAssets.Application.Common;

internal static class ValidationExtensions
{
    /// <summary>
    /// Valida el comando y devuelve el error de validación (campos en camelCase) o <see langword="null"/>.
    /// </summary>
    public static async Task<ApplicationError?> ValidateCommandAsync<T>(
        this IValidator<T> validator,
        T command,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(command, cancellationToken);
        return result.IsValid ? null : ToError(result);
    }

    private static ApplicationError ToError(ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(failure => ToCamelCase(failure.PropertyName), StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray(),
                StringComparer.Ordinal);

        return new ApplicationError(ErrorKind.Validation, "Uno o más campos no son válidos.", errors);
    }

    private static string ToCamelCase(string propertyName) =>
        string.Join('.', propertyName.Split('.').Select(segment =>
            segment.Length == 0 ? segment : char.ToLowerInvariant(segment[0]) + segment[1..]));
}
