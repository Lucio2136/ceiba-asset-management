using CeibaAssets.Application.Abstractions;
using CeibaAssets.Application.Audit;
using CeibaAssets.Application.Common;
using CeibaAssets.Domain.Common;
using CeibaAssets.Domain.Locations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CeibaAssets.Application.Locations;

internal sealed class LocationsService(
    IApplicationDbContext db,
    IAuditTrail audit,
    IValidator<CreateLocationCommand> createValidator,
    IValidator<UpdateLocationCommand> updateValidator) : ILocationsService
{
    public async Task<IReadOnlyList<LocationModel>> ListAsync(LocationQuery query, CancellationToken cancellationToken)
    {
        var locations = db.Locations.AsNoTracking();

        if (!query.IncludeInactive)
        {
            locations = locations.Where(l => l.IsActive);
        }

        if (query.FloorId is { } floorId)
        {
            locations = locations.Where(l => l.FloorId == floorId);
        }

        if (query.BuildingId is { } buildingId)
        {
            locations = locations.Where(l => l.Floor!.BuildingId == buildingId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = SearchPattern.Contains(query.Search);
            locations = locations.Where(l => EF.Functions.Like(l.Name, pattern) || EF.Functions.Like(l.Code, pattern));
        }

        return await Project(locations)
            .OrderBy(l => l.BuildingName)
            .ThenBy(l => l.LevelNumber)
            .ThenBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<LocationModel>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var location = await Project(db.Locations.AsNoTracking().Where(l => l.Id == id))
            .FirstOrDefaultAsync(cancellationToken);

        return location is null
            ? Result.NotFound<LocationModel>("La ubicación no existe.")
            : Result.Success(location);
    }

    public async Task<Result<LocationModel>> CreateAsync(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        if (await createValidator.ValidateCommandAsync(command, cancellationToken) is { } invalid)
        {
            return Result.Failure<LocationModel>(invalid);
        }

        var floor = await db.Floors.AsNoTracking()
            .Where(f => f.Id == command.FloorId)
            .Select(f => new { f.IsActive })
            .FirstOrDefaultAsync(cancellationToken);
        if (floor is null)
        {
            return Result.Invalid<LocationModel>("floorId", "El piso no existe.");
        }

        if (!floor.IsActive)
        {
            return Result.Invalid<LocationModel>("floorId", "El piso está inactivo.");
        }

        var code = Location.NormalizeCode(command.Code);
        if (await db.Locations.AnyAsync(l => l.Code == code, cancellationToken))
        {
            return Result.Conflict<LocationModel>($"Ya existe una ubicación con el código {code}.");
        }

        Location location;
        try
        {
            location = Location.Create(command.FloorId, command.Code, command.Name, command.Description);
        }
        catch (DomainException ex)
        {
            return Result.Conflict<LocationModel>(ex.Message);
        }

        db.Locations.Add(location);
        audit.Record("location.created", nameof(Location), location.Id.ToString(), $"Ubicación {location.Code} creada: {location.Name}.");

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result.Conflict<LocationModel>($"Ya existe una ubicación con el código {code}.");
        }

        return await GetAsync(location.Id, cancellationToken);
    }

    public async Task<Result<LocationModel>> UpdateAsync(UpdateLocationCommand command, CancellationToken cancellationToken)
    {
        if (await updateValidator.ValidateCommandAsync(command, cancellationToken) is { } invalid)
        {
            return Result.Failure<LocationModel>(invalid);
        }

        var location = await db.Locations.FirstOrDefaultAsync(l => l.Id == command.Id, cancellationToken);
        if (location is null)
        {
            return Result.NotFound<LocationModel>("La ubicación no existe.");
        }

        _ = RowVersionCodec.TryDecode(command.RowVersion, out var rowVersion);
        db.Entry(location).Property(l => l.RowVersion).OriginalValue = rowVersion;

        try
        {
            location.Update(command.Name, command.Description, command.IsActive);
        }
        catch (DomainException ex)
        {
            return Result.Conflict<LocationModel>(ex.Message);
        }

        audit.Record("location.updated", nameof(Location), location.Id.ToString(), $"Ubicación {location.Code} actualizada.");

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict<LocationModel>("La ubicación fue modificada por otro usuario. Recargue e intente de nuevo.");
        }

        return await GetAsync(location.Id, cancellationToken);
    }

    private static IQueryable<LocationModel> Project(IQueryable<Location> locations) =>
        locations.Select(l => new LocationModel(
            l.Id,
            l.FloorId,
            l.Floor!.Name,
            l.Floor.LevelNumber,
            l.Floor.BuildingId,
            l.Floor.Building!.Name,
            l.Code,
            l.Name,
            l.Description,
            l.IsActive,
            RowVersionCodec.Encode(l.RowVersion)));
}
