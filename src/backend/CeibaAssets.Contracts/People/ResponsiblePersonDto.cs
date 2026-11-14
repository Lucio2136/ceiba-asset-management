namespace CeibaAssets.Contracts.People;

/// <summary>Persona responsable de activos.</summary>
public sealed record ResponsiblePersonDto
{
    public required Guid Id { get; init; }

    public required string EmployeeNumber { get; init; }

    public required string DisplayName { get; init; }

    public required string Email { get; init; }

    public string? Department { get; init; }

    public required bool IsActive { get; init; }
}
