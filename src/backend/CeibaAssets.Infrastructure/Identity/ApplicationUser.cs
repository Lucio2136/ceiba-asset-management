using Microsoft.AspNetCore.Identity;

namespace CeibaAssets.Infrastructure.Identity;

/// <summary>
/// Cuenta de acceso a la aplicación (esquema <c>security</c>).
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public const int DisplayNameMaxLength = 150;

    public string DisplayName { get; set; } = string.Empty;
}
