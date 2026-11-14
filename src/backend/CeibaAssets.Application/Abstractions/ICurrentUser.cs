namespace CeibaAssets.Application.Abstractions;

/// <summary>
/// Usuario que ejecuta la operación actual. La API lo resuelve desde la sesión; fuera de una
/// solicitud HTTP se usa el usuario de sistema.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    string? DisplayName { get; }
}
