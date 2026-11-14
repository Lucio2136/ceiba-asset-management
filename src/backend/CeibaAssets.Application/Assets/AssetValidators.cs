using CeibaAssets.Application.Common;
using CeibaAssets.Domain.Assets;
using FluentValidation;

namespace CeibaAssets.Application.Assets;

internal sealed class AssetIdentifierCommandValidator : AbstractValidator<AssetIdentifierCommand>
{
    public AssetIdentifierCommandValidator()
    {
        RuleFor(x => x.Kind).IsInEnum().WithMessage("La clase de identificador no es válida.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("El valor del identificador es obligatorio.")
            .MaximumLength(AssetIdentifier.ValueMaxLength)
            .WithMessage($"El identificador no puede exceder {AssetIdentifier.ValueMaxLength} caracteres.");
    }
}

internal static class AssetRuleExtensions
{
    public const int MaxIdentifiers = 20;

    public static IRuleBuilderOptions<T, IReadOnlyList<AssetIdentifierCommand>?> ValidIdentifiers<T>(
        this IRuleBuilder<T, IReadOnlyList<AssetIdentifierCommand>?> rule)
    {
        return rule
            .Must(list => list is null || list.Count <= MaxIdentifiers)
            .WithMessage($"Se permiten como máximo {MaxIdentifiers} identificadores.")
            .Must(list => list is null || !list
                .Where(item => item.IsPrimary)
                .GroupBy(item => item.Kind)
                .Any(group => group.Count() > 1))
            .WithMessage("Solo puede haber un identificador principal por clase.");
    }
}

internal sealed class CreateAssetCommandValidator : AbstractValidator<CreateAssetCommand>
{
    public CreateAssetCommandValidator()
    {
        RuleFor(x => x.AssetType).IsInEnum().WithMessage("El tipo de activo no es válido.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(Asset.NameMaxLength).WithMessage($"El nombre no puede exceder {Asset.NameMaxLength} caracteres.");

        RuleFor(x => x.Manufacturer)
            .MaximumLength(Asset.ManufacturerMaxLength)
            .WithMessage($"El fabricante no puede exceder {Asset.ManufacturerMaxLength} caracteres.");

        RuleFor(x => x.Model)
            .MaximumLength(Asset.ModelMaxLength)
            .WithMessage($"El modelo no puede exceder {Asset.ModelMaxLength} caracteres.");

        RuleFor(x => x.SerialNumber)
            .MaximumLength(Asset.SerialNumberMaxLength)
            .WithMessage($"El número de serie no puede exceder {Asset.SerialNumberMaxLength} caracteres.");

        RuleFor(x => x.Notes)
            .MaximumLength(Asset.NotesMaxLength)
            .WithMessage($"Las notas no pueden exceder {Asset.NotesMaxLength} caracteres.");

        RuleFor(x => x.Identifiers).ValidIdentifiers();
        RuleForEach(x => x.Identifiers).SetValidator(new AssetIdentifierCommandValidator());
    }
}

internal sealed class UpdateAssetCommandValidator : AbstractValidator<UpdateAssetCommand>
{
    public UpdateAssetCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");
        RuleFor(x => x.AssetType).IsInEnum().WithMessage("El tipo de activo no es válido.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(Asset.NameMaxLength).WithMessage($"El nombre no puede exceder {Asset.NameMaxLength} caracteres.");

        RuleFor(x => x.Manufacturer)
            .MaximumLength(Asset.ManufacturerMaxLength)
            .WithMessage($"El fabricante no puede exceder {Asset.ManufacturerMaxLength} caracteres.");

        RuleFor(x => x.Model)
            .MaximumLength(Asset.ModelMaxLength)
            .WithMessage($"El modelo no puede exceder {Asset.ModelMaxLength} caracteres.");

        RuleFor(x => x.SerialNumber)
            .MaximumLength(Asset.SerialNumberMaxLength)
            .WithMessage($"El número de serie no puede exceder {Asset.SerialNumberMaxLength} caracteres.");

        RuleFor(x => x.Notes)
            .MaximumLength(Asset.NotesMaxLength)
            .WithMessage($"Las notas no pueden exceder {Asset.NotesMaxLength} caracteres.");

        RuleFor(x => x.Status)
            .Must(status => status is null or AssetStatus.Available or AssetStatus.InMaintenance)
            .WithMessage("Solo se puede establecer el estado Available o InMaintenance; use asignaciones y baja para los demás.");

        RuleFor(x => x.RowVersion)
            .Must(value => RowVersionCodec.TryDecode(value, out _))
            .WithMessage("La versión de fila es obligatoria y debe estar en Base64.");

        RuleFor(x => x.Identifiers).ValidIdentifiers();
        RuleForEach(x => x.Identifiers).SetValidator(new AssetIdentifierCommandValidator());
    }
}
