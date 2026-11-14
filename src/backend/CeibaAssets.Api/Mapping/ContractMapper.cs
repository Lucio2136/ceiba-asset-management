using CeibaAssets.Application.Assets;
using CeibaAssets.Application.Assignments;
using CeibaAssets.Application.Audit;
using CeibaAssets.Application.Common;
using CeibaAssets.Application.Locations;
using CeibaAssets.Application.People;
using CeibaAssets.Contracts.Assets;
using CeibaAssets.Contracts.Assignments;
using CeibaAssets.Contracts.Audit;
using CeibaAssets.Contracts.Common;
using CeibaAssets.Contracts.Locations;
using CeibaAssets.Contracts.People;
using DomainAssets = CeibaAssets.Domain.Assets;

namespace CeibaAssets.Api.Mapping;

/// <summary>
/// Traduce los modelos internos de Application a los contratos públicos y viceversa.
/// Los enums públicos reproducen los del dominio por nombre.
/// </summary>
internal static class ContractMapper
{
    public static TTo ConvertEnum<TFrom, TTo>(TFrom value)
        where TFrom : struct, Enum
        where TTo : struct, Enum => Enum.Parse<TTo>(value.ToString(), ignoreCase: false);

    public static PagedResult<TDto> ToDto<TModel, TDto>(this PagedModel<TModel> page, Func<TModel, TDto> map) => new()
    {
        Items = page.Items.Select(map).ToList(),
        TotalCount = page.TotalCount,
        Page = page.Page,
        PageSize = page.PageSize,
    };

    public static BuildingDto ToDto(this BuildingModel model) => new()
    {
        Id = model.Id,
        Code = model.Code,
        Name = model.Name,
        LevelCount = model.LevelCount,
        IsActive = model.IsActive,
    };

    public static FloorDto ToDto(this FloorModel model) => new()
    {
        Id = model.Id,
        BuildingId = model.BuildingId,
        LevelNumber = model.LevelNumber,
        Name = model.Name,
        IsActive = model.IsActive,
    };

    public static LocationDto ToDto(this LocationModel model) => new()
    {
        Id = model.Id,
        FloorId = model.FloorId,
        FloorName = model.FloorName,
        LevelNumber = model.LevelNumber,
        BuildingId = model.BuildingId,
        BuildingName = model.BuildingName,
        Code = model.Code,
        Name = model.Name,
        Description = model.Description,
        IsActive = model.IsActive,
        RowVersion = model.RowVersion,
    };

    public static AssetDto ToDto(this AssetModel model) => new()
    {
        Id = model.Id,
        AssetType = ConvertEnum<DomainAssets.AssetType, AssetType>(model.AssetType),
        Status = ConvertEnum<DomainAssets.AssetStatus, AssetStatus>(model.Status),
        Name = model.Name,
        Manufacturer = model.Manufacturer,
        Model = model.Model,
        SerialNumber = model.SerialNumber,
        Notes = model.Notes,
        LocationId = model.LocationId,
        LocationName = model.LocationName,
        LocationPath = model.LocationPath,
        CreatedAtUtc = model.CreatedAtUtc,
        UpdatedAtUtc = model.UpdatedAtUtc,
        RowVersion = model.RowVersion,
        Identifiers = model.Identifiers.Select(ToDto).ToList(),
        CurrentAssignment = model.CurrentAssignment?.ToDto(),
    };

    public static AssetIdentifierDto ToDto(this AssetIdentifierModel model) => new()
    {
        Id = model.Id,
        Kind = ConvertEnum<DomainAssets.AssetIdentifierKind, AssetIdentifierKind>(model.Kind),
        Value = model.Value,
        IsPrimary = model.IsPrimary,
    };

    public static AssetListItemDto ToDto(this AssetListItemModel model) => new()
    {
        Id = model.Id,
        AssetType = ConvertEnum<DomainAssets.AssetType, AssetType>(model.AssetType),
        Status = ConvertEnum<DomainAssets.AssetStatus, AssetStatus>(model.Status),
        Name = model.Name,
        Manufacturer = model.Manufacturer,
        Model = model.Model,
        SerialNumber = model.SerialNumber,
        LocationId = model.LocationId,
        LocationName = model.LocationName,
        LocationPath = model.LocationPath,
        AssignedToId = model.AssignedToId,
        AssignedToName = model.AssignedToName,
    };

    public static AssignmentDto ToDto(this AssignmentModel model) => new()
    {
        Id = model.Id,
        AssetId = model.AssetId,
        ResponsiblePersonId = model.ResponsiblePersonId,
        ResponsiblePersonName = model.ResponsiblePersonName,
        AssignedAtUtc = model.AssignedAtUtc,
        ReturnedAtUtc = model.ReturnedAtUtc,
        AssignedByUserId = model.AssignedByUserId,
        Notes = model.Notes,
    };

    public static MovementDto ToDto(this MovementModel model) => new()
    {
        Id = model.Id,
        AssetId = model.AssetId,
        FromLocationId = model.FromLocationId,
        FromLocationName = model.FromLocationName,
        ToLocationId = model.ToLocationId,
        ToLocationName = model.ToLocationName,
        MovedAtUtc = model.MovedAtUtc,
        MovedByUserId = model.MovedByUserId,
        Reason = model.Reason,
    };

    public static ResponsiblePersonDto ToDto(this ResponsiblePersonModel model) => new()
    {
        Id = model.Id,
        EmployeeNumber = model.EmployeeNumber,
        DisplayName = model.DisplayName,
        Email = model.Email,
        Department = model.Department,
        IsActive = model.IsActive,
    };

    public static AuditEventDto ToDto(this AuditEventModel model) => new()
    {
        Id = model.Id,
        OccurredAtUtc = model.OccurredAtUtc,
        ActorUserId = model.ActorUserId,
        ActorName = model.ActorName,
        Action = model.Action,
        EntityType = model.EntityType,
        EntityId = model.EntityId,
        Summary = model.Summary,
        DetailsJson = model.DetailsJson,
    };

    public static CreateAssetCommand ToCommand(this CreateAssetRequest request) => new(
        ConvertEnum<AssetType, DomainAssets.AssetType>(request.AssetType),
        request.Name,
        request.Manufacturer,
        request.Model,
        request.SerialNumber,
        request.Notes,
        request.LocationId,
        request.Identifiers?.Select(ToCommand).ToList());

    public static UpdateAssetCommand ToCommand(this UpdateAssetRequest request, Guid id) => new(
        id,
        ConvertEnum<AssetType, DomainAssets.AssetType>(request.AssetType),
        request.Name,
        request.Manufacturer,
        request.Model,
        request.SerialNumber,
        request.Notes,
        request.Status is { } status ? ConvertEnum<AssetStatus, DomainAssets.AssetStatus>(status) : null,
        request.Identifiers?.Select(ToCommand).ToList(),
        request.RowVersion);

    public static AssetIdentifierCommand ToCommand(this AssetIdentifierRequest request) => new(
        ConvertEnum<AssetIdentifierKind, DomainAssets.AssetIdentifierKind>(request.Kind),
        request.Value,
        request.IsPrimary);

    public static AssignAssetCommand ToCommand(this AssignAssetRequest request) =>
        new(request.AssetId, request.ResponsiblePersonId, request.Notes);

    public static ReturnAssetCommand ToCommand(this ReturnAssetRequest request) =>
        new(request.AssetId, request.Notes);

    public static MoveAssetCommand ToCommand(this MoveAssetRequest request) =>
        new(request.AssetId, request.ToLocationId, request.Reason);

    public static CreateLocationCommand ToCommand(this CreateLocationRequest request) =>
        new(request.FloorId, request.Code, request.Name, request.Description);

    public static UpdateLocationCommand ToCommand(this UpdateLocationRequest request, Guid id) =>
        new(id, request.Name, request.Description, request.IsActive, request.RowVersion);
}
