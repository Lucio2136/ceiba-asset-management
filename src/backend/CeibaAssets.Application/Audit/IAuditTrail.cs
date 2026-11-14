namespace CeibaAssets.Application.Audit;

/// <summary>
/// Registra eventos de auditoría. <see cref="Record"/> los agrega al contexto actual para que se
/// persistan en la misma operación que el cambio de negocio.
/// </summary>
public interface IAuditTrail
{
    void Record(string action, string entityType, string entityId, string summary, string? detailsJson = null);

    /// <summary>
    /// Registra y guarda un evento en una operación aparte (por ejemplo, intentos de inicio de sesión).
    /// </summary>
    Task RecordAndSaveAsync(
        string action,
        string entityType,
        string entityId,
        string summary,
        Guid? actorUserId,
        string? actorName,
        CancellationToken cancellationToken);
}
