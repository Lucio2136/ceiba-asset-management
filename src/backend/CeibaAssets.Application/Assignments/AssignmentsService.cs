using CeibaAssets.Application.Abstractions;
using CeibaAssets.Application.Audit;
using CeibaAssets.Application.Common;
using CeibaAssets.Domain.Assets;
using CeibaAssets.Domain.Assignments;
using CeibaAssets.Domain.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CeibaAssets.Application.Assignments;

internal sealed class AssignmentsService(
    IApplicationDbContext db,
    IAuditTrail audit,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    IValidator<AssignAssetCommand> assignValidator,
    IValidator<ReturnAssetCommand> returnValidator,
    IValidator<MoveAssetCommand> moveValidator) : IAssignmentsService
{
    private const string ConcurrencyMessage =
        "El activo fue modificado por otro usuario. Recargue e intente de nuevo.";

    public async Task<Result<AssignmentModel>> AssignAsync(AssignAssetCommand command, CancellationToken cancellationToken)
    {
        if (await assignValidator.ValidateCommandAsync(command, cancellationToken) is { } invalid)
        {
            return Result.Failure<AssignmentModel>(invalid);
        }

        var asset = await db.Assets.FirstOrDefaultAsync(a => a.Id == command.AssetId, cancellationToken);
        if (asset is null)
        {
            return Result.NotFound<AssignmentModel>("El activo no existe.");
        }

        var person = await db.ResponsiblePeople
            .AsNoTracking()
            .Where(p => p.Id == command.ResponsiblePersonId)
            .Select(p => new { p.DisplayName, p.IsActive })
            .FirstOrDefaultAsync(cancellationToken);
        if (person is null)
        {
            return Result.Invalid<AssignmentModel>("responsiblePersonId", "El responsable no existe.");
        }

        if (!person.IsActive)
        {
            return Result.Invalid<AssignmentModel>("responsiblePersonId", "El responsable está inactivo.");
        }

        if (await db.AssetAssignments.AnyAsync(a => a.AssetId == asset.Id && a.ReturnedAtUtc == null, cancellationToken))
        {
            return Result.Conflict<AssignmentModel>("El activo ya tiene una asignación vigente.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        AssetAssignment assignment;
        try
        {
            asset.MarkAssigned(now);
            assignment = AssetAssignment.Create(asset.Id, command.ResponsiblePersonId, now, currentUser.UserId, command.Notes);
        }
        catch (DomainException ex)
        {
            return Result.Conflict<AssignmentModel>(ex.Message);
        }

        db.AssetAssignments.Add(assignment);
        audit.Record(
            "asset.assigned",
            nameof(Asset),
            asset.Id.ToString(),
            $"Activo {asset.Name} asignado a {person.DisplayName}.");

        if (await TrySaveAsync(cancellationToken) is { } conflict)
        {
            return Result.Failure<AssignmentModel>(conflict);
        }

        return Result.Success(ToModel(assignment, person.DisplayName));
    }

    public async Task<Result<AssignmentModel>> ReturnAsync(ReturnAssetCommand command, CancellationToken cancellationToken)
    {
        if (await returnValidator.ValidateCommandAsync(command, cancellationToken) is { } invalid)
        {
            return Result.Failure<AssignmentModel>(invalid);
        }

        var asset = await db.Assets.FirstOrDefaultAsync(a => a.Id == command.AssetId, cancellationToken);
        if (asset is null)
        {
            return Result.NotFound<AssignmentModel>("El activo no existe.");
        }

        var assignment = await db.AssetAssignments
            .FirstOrDefaultAsync(a => a.AssetId == asset.Id && a.ReturnedAtUtc == null, cancellationToken);
        if (assignment is null)
        {
            return Result.Conflict<AssignmentModel>("El activo no tiene una asignación vigente.");
        }

        var personName = await db.ResponsiblePeople
            .AsNoTracking()
            .Where(p => p.Id == assignment.ResponsiblePersonId)
            .Select(p => p.DisplayName)
            .FirstAsync(cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        try
        {
            assignment.Return(now, command.Notes);
            asset.MarkReturned(now);
        }
        catch (DomainException ex)
        {
            return Result.Conflict<AssignmentModel>(ex.Message);
        }

        audit.Record(
            "asset.returned",
            nameof(Asset),
            asset.Id.ToString(),
            $"Activo {asset.Name} devuelto por {personName}.");

        if (await TrySaveAsync(cancellationToken) is { } conflict)
        {
            return Result.Failure<AssignmentModel>(conflict);
        }

        return Result.Success(ToModel(assignment, personName));
    }

    public async Task<Result<MovementModel>> MoveAsync(MoveAssetCommand command, CancellationToken cancellationToken)
    {
        if (await moveValidator.ValidateCommandAsync(command, cancellationToken) is { } invalid)
        {
            return Result.Failure<MovementModel>(invalid);
        }

        var asset = await db.Assets.FirstOrDefaultAsync(a => a.Id == command.AssetId, cancellationToken);
        if (asset is null)
        {
            return Result.NotFound<MovementModel>("El activo no existe.");
        }

        var destination = await db.Locations
            .AsNoTracking()
            .Where(l => l.Id == command.ToLocationId)
            .Select(l => new { l.Name, l.IsActive })
            .FirstOrDefaultAsync(cancellationToken);
        if (destination is null)
        {
            return Result.Invalid<MovementModel>("toLocationId", "La ubicación de destino no existe.");
        }

        if (!destination.IsActive)
        {
            return Result.Invalid<MovementModel>("toLocationId", "La ubicación de destino está inactiva.");
        }

        var fromName = asset.LocationId is { } fromId
            ? await db.Locations.AsNoTracking().Where(l => l.Id == fromId).Select(l => l.Name).FirstOrDefaultAsync(cancellationToken)
            : null;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        AssetMovement movement;
        try
        {
            var previous = asset.MoveTo(command.ToLocationId, now);
            movement = AssetMovement.Create(asset.Id, previous, command.ToLocationId, now, currentUser.UserId, command.Reason);
        }
        catch (DomainException ex)
        {
            return Result.Conflict<MovementModel>(ex.Message);
        }

        db.AssetMovements.Add(movement);
        audit.Record(
            "asset.moved",
            nameof(Asset),
            asset.Id.ToString(),
            $"Activo {asset.Name} movido a {destination.Name}.");

        if (await TrySaveAsync(cancellationToken) is { } conflict)
        {
            return Result.Failure<MovementModel>(conflict);
        }

        return Result.Success(new MovementModel(
            movement.Id,
            movement.AssetId,
            movement.FromLocationId,
            fromName,
            movement.ToLocationId,
            destination.Name,
            movement.MovedAtUtc,
            movement.MovedByUserId,
            movement.Reason));
    }

    public async Task<Result<IReadOnlyList<AssignmentModel>>> ListAssignmentsAsync(Guid assetId, CancellationToken cancellationToken)
    {
        if (!await db.Assets.AnyAsync(a => a.Id == assetId, cancellationToken))
        {
            return Result.NotFound<IReadOnlyList<AssignmentModel>>("El activo no existe.");
        }

        var items = await db.AssetAssignments
            .AsNoTracking()
            .Where(a => a.AssetId == assetId)
            .Join(
                db.ResponsiblePeople,
                a => a.ResponsiblePersonId,
                p => p.Id,
                (a, p) => new AssignmentModel(
                    a.Id,
                    a.AssetId,
                    a.ResponsiblePersonId,
                    p.DisplayName,
                    a.AssignedAtUtc,
                    a.ReturnedAtUtc,
                    a.AssignedByUserId,
                    a.Notes))
            .OrderByDescending(a => a.AssignedAtUtc)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<AssignmentModel>>(items);
    }

    public async Task<Result<IReadOnlyList<MovementModel>>> ListMovementsAsync(Guid assetId, CancellationToken cancellationToken)
    {
        if (!await db.Assets.AnyAsync(a => a.Id == assetId, cancellationToken))
        {
            return Result.NotFound<IReadOnlyList<MovementModel>>("El activo no existe.");
        }

        var rows = await db.AssetMovements
            .AsNoTracking()
            .Where(m => m.AssetId == assetId)
            .OrderByDescending(m => m.MovedAtUtc)
            .ToListAsync(cancellationToken);

        var locationIds = rows
            .SelectMany(m => new[] { m.FromLocationId, m.ToLocationId })
            .OfType<Guid>()
            .Distinct()
            .ToList();
        var names = await db.Locations
            .AsNoTracking()
            .Where(l => locationIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.Name, cancellationToken);

        var items = rows.Select(m => new MovementModel(
            m.Id,
            m.AssetId,
            m.FromLocationId,
            m.FromLocationId is { } from && names.TryGetValue(from, out var fromName) ? fromName : null,
            m.ToLocationId,
            names.GetValueOrDefault(m.ToLocationId, string.Empty),
            m.MovedAtUtc,
            m.MovedByUserId,
            m.Reason)).ToList();

        return Result.Success<IReadOnlyList<MovementModel>>(items);
    }

    private static AssignmentModel ToModel(AssetAssignment assignment, string personName) => new(
        assignment.Id,
        assignment.AssetId,
        assignment.ResponsiblePersonId,
        personName,
        assignment.AssignedAtUtc,
        assignment.ReturnedAtUtc,
        assignment.AssignedByUserId,
        assignment.Notes);

    private async Task<ApplicationError?> TrySaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return null;
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ApplicationError(ErrorKind.Conflict, ConcurrencyMessage);
        }
        catch (DbUpdateException)
        {
            // Índice filtrado: un activo solo puede tener una asignación vigente.
            return new ApplicationError(ErrorKind.Conflict, "La operación entra en conflicto con otra en curso. Intente de nuevo.");
        }
    }
}
