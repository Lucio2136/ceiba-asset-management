using CeibaAssets.Domain.Assignments;
using FluentValidation;

namespace CeibaAssets.Application.Assignments;

internal sealed class AssignAssetCommandValidator : AbstractValidator<AssignAssetCommand>
{
    public AssignAssetCommandValidator()
    {
        RuleFor(x => x.AssetId).NotEmpty().WithMessage("El activo es obligatorio.");
        RuleFor(x => x.ResponsiblePersonId).NotEmpty().WithMessage("El responsable es obligatorio.");
        RuleFor(x => x.Notes)
            .MaximumLength(AssetAssignment.NotesMaxLength)
            .WithMessage($"Las notas no pueden exceder {AssetAssignment.NotesMaxLength} caracteres.");
    }
}

internal sealed class ReturnAssetCommandValidator : AbstractValidator<ReturnAssetCommand>
{
    public ReturnAssetCommandValidator()
    {
        RuleFor(x => x.AssetId).NotEmpty().WithMessage("El activo es obligatorio.");
        RuleFor(x => x.Notes)
            .MaximumLength(AssetAssignment.NotesMaxLength)
            .WithMessage($"Las notas no pueden exceder {AssetAssignment.NotesMaxLength} caracteres.");
    }
}

internal sealed class MoveAssetCommandValidator : AbstractValidator<MoveAssetCommand>
{
    public MoveAssetCommandValidator()
    {
        RuleFor(x => x.AssetId).NotEmpty().WithMessage("El activo es obligatorio.");
        RuleFor(x => x.ToLocationId).NotEmpty().WithMessage("La ubicación de destino es obligatoria.");
        RuleFor(x => x.Reason)
            .MaximumLength(AssetMovement.ReasonMaxLength)
            .WithMessage($"El motivo no puede exceder {AssetMovement.ReasonMaxLength} caracteres.");
    }
}
