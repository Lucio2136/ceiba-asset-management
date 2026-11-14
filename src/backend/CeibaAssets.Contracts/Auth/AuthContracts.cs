namespace CeibaAssets.Contracts.Auth;

/// <summary>Credenciales de inicio de sesión.</summary>
public sealed record LoginRequest
{
    public required string Email { get; init; }

    public required string Password { get; init; }
}

/// <summary>Usuario autenticado. Se devuelve al iniciar sesión y en <c>/v1/auth/me</c>.</summary>
public sealed record MeResponse
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public required string DisplayName { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }
}
