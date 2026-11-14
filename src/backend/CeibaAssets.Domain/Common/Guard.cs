namespace CeibaAssets.Domain.Common;

/// <summary>
/// Validaciones reutilizables para las invariantes de las entidades.
/// </summary>
internal static class Guard
{
    public static string RequiredText(string? value, string fieldName, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw new DomainException($"{fieldName} es obligatorio.");
        }

        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{fieldName} no puede exceder {maxLength} caracteres.");
        }

        return trimmed;
    }

    public static string? OptionalText(string? value, string fieldName, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{fieldName} no puede exceder {maxLength} caracteres.");
        }

        return trimmed;
    }

    public static Guid RequiredId(Guid value, string fieldName)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException($"{fieldName} es obligatorio.");
        }

        return value;
    }
}
