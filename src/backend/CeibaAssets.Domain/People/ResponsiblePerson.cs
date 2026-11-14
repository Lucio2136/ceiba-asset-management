using CeibaAssets.Domain.Common;

namespace CeibaAssets.Domain.People;

/// <summary>
/// Persona responsable de activos (esquema <c>people</c>). Es independiente de las cuentas de acceso.
/// </summary>
public sealed class ResponsiblePerson
{
    public const int EmployeeNumberMaxLength = 32;
    public const int DisplayNameMaxLength = 150;
    public const int EmailMaxLength = 254;
    public const int DepartmentMaxLength = 100;

    private ResponsiblePerson()
    {
    }

    public Guid Id { get; private set; }

    public string EmployeeNumber { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? Department { get; private set; }

    public bool IsActive { get; private set; } = true;

    public byte[] RowVersion { get; private set; } = [];

    public static ResponsiblePerson Create(
        string employeeNumber,
        string displayName,
        string email,
        string? department) => new()
        {
            Id = Guid.CreateVersion7(),
            EmployeeNumber = Guard.RequiredText(employeeNumber, "El número de empleado", EmployeeNumberMaxLength).ToUpperInvariant(),
            DisplayName = Guard.RequiredText(displayName, "El nombre", DisplayNameMaxLength),
            Email = NormalizeEmail(email),
            Department = Guard.OptionalText(department, "El departamento", DepartmentMaxLength),
        };

    public void Update(string displayName, string email, string? department)
    {
        DisplayName = Guard.RequiredText(displayName, "El nombre", DisplayNameMaxLength);
        Email = NormalizeEmail(email);
        Department = Guard.OptionalText(department, "El departamento", DepartmentMaxLength);
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    private static string NormalizeEmail(string email)
    {
        var normalized = Guard.RequiredText(email, "El correo electrónico", EmailMaxLength).ToLowerInvariant();
        if (!normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new DomainException("El correo electrónico no es válido.");
        }

        return normalized;
    }
}
