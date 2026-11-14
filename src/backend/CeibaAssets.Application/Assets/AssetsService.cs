using System.Text.Json;
using CeibaAssets.Application.Abstractions;
using CeibaAssets.Application.Assignments;
using CeibaAssets.Application.Audit;
using CeibaAssets.Application.Common;
using CeibaAssets.Domain.Assets;
using CeibaAssets.Domain.Assignments;
using CeibaAssets.Domain.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CeibaAssets.Application.Assets;

internal sealed class AssetsService(
    IApplicationDbContext db,
    IAuditTrail audit,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    IValidator<CreateAssetCommand> createValidator,
    IValidator<UpdateAssetCommand> updateValidator) : IAssetsService
{
    public async Task<PagedModel<AssetListItemModel>> ListAsync(AssetListQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        var assets = db.Assets.AsNoTracking();

        if (query.AssetType is { } type)
        {
            assets = assets.Where(a => a.AssetType == type);
        }

        if (query.Status is { } status)
        {
            assets = assets.Where(a => a.Status == status);
        }

        if (query.LocationId is { } locationId)
        {
            assets = assets.Where(a => a.LocationId == locationId);
        }

        if (query.AssignedTo is { } personId)
        {
            assets = assets.Where(a => db.AssetAssignments.Any(
                x => x.AssetId == a.Id && x.ReturnedAtUtc == null && x.ResponsiblePersonId == personId));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = SearchPattern.Contains(query.Search);
            assets = assets.Where(a =>
                EF.Functions.Like(a.Name, pattern)
                || (a.Manufacturer != null && EF.Functions.Like(a.Manufacturer, pattern))
                || (a.Model != null && EF.Functions.Like(a.Model, pattern))
                || (a.SerialNumber != null && EF.Functions.Like(a.SerialNumber, pattern))
                || a.Identifiers.Any(i => EF.Functions.Like(i.NormalizedValue, pattern)));
        }

        var total = await assets.CountAsync(cancellationToken);

        var rows = await assets
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.AssetType,
                a.Status,
                a.Name,
                a.Manufacturer,
                a.Model,
                a.SerialNumber,
                a.LocationId,
                LocationName = a.Location == null ? null : a.Location.Name,
                FloorName = a.Location == null ? null : a.Location.Floor!.Name,
                BuildingName = a.Location == null ? null : a.Location.Floor!.Building!.Name,
            })
            .ToListAsync(cancellationToken);

        var pageIds = rows.Select(r => r.Id).ToList();
        var assignees = await db.AssetAssignments
            .AsNoTracking()
            .Where(x => x.ReturnedAtUtc == null && pageIds.Contains(x.AssetId))
            .Join(
                db.ResponsiblePeople,
                assignment => assignment.ResponsiblePersonId,
                person => person.Id,
                (assignment, person) => new { assignment.AssetId, PersonId = person.Id, person.DisplayName })
            .ToListAsync(cancellationToken);
        var assigneeByAsset = assignees
            .GroupBy(x => x.AssetId)
            .ToDictionary(group => group.Key, group => group.First());

        var items = rows.Select(r =>
        {
            assigneeByAsset.TryGetValue(r.Id, out var assignee);
            return new AssetListItemModel(
                r.Id,
                r.AssetType,
                r.Status,
                r.Name,
                r.Manufacturer,
                r.Model,
                r.SerialNumber,
                r.LocationId,
                r.LocationName,
                BuildPath(r.BuildingName, r.FloorName, r.LocationName),
                assignee?.PersonId,
                assignee?.DisplayName);
        }).ToList();

        return new PagedModel<AssetListItemModel>(items, total, page, pageSize);
    }

    public async Task<Result<AssetModel>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var row = await db.Assets
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.AssetType,
                a.Status,
                a.Name,
                a.Manufacturer,
                a.Model,
                a.SerialNumber,
                a.Notes,
                a.LocationId,
                LocationName = a.Location == null ? null : a.Location.Name,
                FloorName = a.Location == null ? null : a.Location.Floor!.Name,
                BuildingName = a.Location == null ? null : a.Location.Floor!.Building!.Name,
                a.CreatedAtUtc,
                a.UpdatedAtUtc,
                a.RowVersion,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return Result.NotFound<AssetModel>("El activo no existe.");
        }

        var identifiers = await db.AssetIdentifiers
            .AsNoTracking()
            .Where(i => i.AssetId == id)
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.Kind)
            .ThenBy(i => i.Value)
            .Select(i => new AssetIdentifierModel(i.Id, i.Kind, i.Value, i.IsPrimary))
            .ToListAsync(cancellationToken);

        var assignment = await db.AssetAssignments
            .AsNoTracking()
            .Where(x => x.AssetId == id && x.ReturnedAtUtc == null)
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
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new AssetModel(
            row.Id,
            row.AssetType,
            row.Status,
            row.Name,
            row.Manufacturer,
            row.Model,
            row.SerialNumber,
            row.Notes,
            row.LocationId,
            row.LocationName,
            BuildPath(row.BuildingName, row.FloorName, row.LocationName),
            row.CreatedAtUtc,
            row.UpdatedAtUtc,
            RowVersionCodec.Encode(row.RowVersion),
            identifiers,
            assignment));
    }

    public async Task<Result<AssetModel>> CreateAsync(CreateAssetCommand command, CancellationToken cancellationToken)
    {
        if (await createValidator.ValidateCommandAsync(command, cancellationToken) is { } invalid)
        {
            return Result.Failure<AssetModel>(invalid);
        }

        if (command.LocationId is { } locationId && await CheckLocationAsync(locationId, cancellationToken) is { } locationError)
        {
            return Result.Failure<AssetModel>(locationError);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        Asset asset;
        try
        {
            asset = Asset.Create(
                command.AssetType,
                command.Name,
                command.Manufacturer,
                command.Model,
                command.SerialNumber,
                command.Notes,
                command.LocationId,
                ToInputs(command.Identifiers),
                now);
        }
        catch (DomainException ex)
        {
            return Result.Conflict<AssetModel>(ex.Message);
        }

        db.Assets.Add(asset);

        if (asset.LocationId is { } initialLocation)
        {
            db.AssetMovements.Add(AssetMovement.Create(
                asset.Id,
                null,
                initialLocation,
                now,
                currentUser.UserId,
                "Ubicación inicial al registrar el activo."));
        }

        audit.Record("asset.created", nameof(Asset), asset.Id.ToString(), $"Activo {asset.Name} ({asset.AssetType}) registrado.");
        await db.SaveChangesAsync(cancellationToken);

        return await GetAsync(asset.Id, cancellationToken);
    }

    public async Task<Result<AssetModel>> UpdateAsync(UpdateAssetCommand command, CancellationToken cancellationToken)
    {
        if (await updateValidator.ValidateCommandAsync(command, cancellationToken) is { } invalid)
        {
            return Result.Failure<AssetModel>(invalid);
        }

        var asset = await db.Assets
            .Include(a => a.Identifiers)
            .FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken);
        if (asset is null)
        {
            return Result.NotFound<AssetModel>("El activo no existe.");
        }

        _ = RowVersionCodec.TryDecode(command.RowVersion, out var rowVersion);
        db.Entry(asset).Property(a => a.RowVersion).OriginalValue = rowVersion;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var identifiers = command.Identifiers is null
            ? asset.Identifiers
                .Where(i => i.Kind != AssetIdentifierKind.SerialNumber)
                .Select(i => new AssetIdentifierInput(i.Kind, i.Value, i.IsPrimary))
                .ToList()
            : ToInputs(command.Identifiers);

        try
        {
            asset.UpdateDetails(
                command.AssetType,
                command.Name,
                command.Manufacturer,
                command.Model,
                command.SerialNumber,
                command.Notes,
                identifiers,
                now);

            if (command.Status is { } status && status != asset.Status)
            {
                if (status == AssetStatus.InMaintenance)
                {
                    asset.SendToMaintenance(now);
                }
                else
                {
                    asset.ReleaseFromMaintenance(now);
                }
            }
        }
        catch (DomainException ex)
        {
            return Result.Conflict<AssetModel>(ex.Message);
        }

        audit.Record("asset.updated", nameof(Asset), asset.Id.ToString(), $"Activo {asset.Name} actualizado.");

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict<AssetModel>("El activo fue modificado por otro usuario. Recargue e intente de nuevo.");
        }

        return await GetAsync(asset.Id, cancellationToken);
    }

    public async Task<Result<AssetModel>> RetireAsync(RetireAssetCommand command, CancellationToken cancellationToken)
    {
        if (command.Reason?.Length > AssetAssignment.NotesMaxLength)
        {
            return Result.Invalid<AssetModel>("reason", $"El motivo no puede exceder {AssetAssignment.NotesMaxLength} caracteres.");
        }

        var asset = await db.Assets.FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken);
        if (asset is null)
        {
            return Result.NotFound<AssetModel>("El activo no existe.");
        }

        try
        {
            asset.Retire(timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (DomainException ex)
        {
            return Result.Conflict<AssetModel>(ex.Message);
        }

        var details = string.IsNullOrWhiteSpace(command.Reason)
            ? null
            : JsonSerializer.Serialize(new { reason = command.Reason.Trim() });
        audit.Record("asset.retired", nameof(Asset), asset.Id.ToString(), $"Activo {asset.Name} dado de baja.", details);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict<AssetModel>("El activo fue modificado por otro usuario. Recargue e intente de nuevo.");
        }

        return await GetAsync(asset.Id, cancellationToken);
    }

    private static string? BuildPath(string? building, string? floor, string? location) =>
        location is null ? null : $"{building} / {floor} / {location}";

    private static List<AssetIdentifierInput> ToInputs(IReadOnlyList<AssetIdentifierCommand>? identifiers) =>
        (identifiers ?? [])
            .Select(i => new AssetIdentifierInput(i.Kind, i.Value, i.IsPrimary))
            .ToList();

    private async Task<ApplicationError?> CheckLocationAsync(Guid locationId, CancellationToken cancellationToken)
    {
        var isActive = await db.Locations
            .AsNoTracking()
            .Where(l => l.Id == locationId)
            .Select(l => (bool?)l.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        return isActive switch
        {
            null => Result.Invalid<AssetModel>("locationId", "La ubicación no existe.").Error,
            false => Result.Invalid<AssetModel>("locationId", "La ubicación está inactiva.").Error,
            _ => null,
        };
    }
}
