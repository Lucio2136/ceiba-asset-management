using CeibaAssets.Api.Security;
using CeibaAssets.Application.Abstractions;
using CeibaAssets.Application.Security;
using CeibaAssets.Infrastructure.Identity;
using CeibaAssets.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;

namespace CeibaAssets.Api.Configuration;

internal static class AuthServiceCollectionExtensions
{
    /// <summary>
    /// Identity con cookie HttpOnly, SameSite=Strict, antiforgery (XSRF-TOKEN / X-XSRF-TOKEN) y autorización por roles (ADR-0007).
    /// </summary>
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IHostEnvironment environment)
    {
        // Producción y Development usan solo HTTPS; otros entornos (Testing) aceptan el esquema de la solicitud.
        var securePolicy = environment.IsDevelopment() || environment.IsProduction()
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<CeibaDbContext>();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = ".Ceiba.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = securePolicy;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;

            // Es una API: no hay redirecciones a páginas de inicio de sesión.
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddAntiforgery(options =>
        {
            options.HeaderName = XsrfTokenCookie.HeaderName;
            options.Cookie.Name = ".Ceiba.Antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = securePolicy;
        });

        services.AddAuthorizationBuilder()
            // Todo endpoint exige sesión salvo que declare [AllowAnonymous].
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(Policies.Write, policy => policy.RequireRole(AppRoles.Administrator, AppRoles.AssetManager))
            .AddPolicy(Policies.Audit, policy => policy.RequireRole(AppRoles.Administrator, AppRoles.Auditor));

        return services;
    }
}
