namespace CeibaAssets.Application.Abstractions;

/// <summary>
/// Usuario de sistema para procesos sin sesión (por ejemplo, el worker de sincronización).
/// </summary>
internal sealed class SystemCurrentUser : ICurrentUser
{
    public Guid? UserId => null;

    public string? DisplayName => "Sistema";
}
