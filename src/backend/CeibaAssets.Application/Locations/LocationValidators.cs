using CeibaAssets.Application.Common;
using CeibaAssets.Domain.Locations;
using FluentValidation;

namespace CeibaAssets.Application.Locations;

internal sealed class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.FloorId).NotEmpty().WithMessage("El piso es obligatorio.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es obligatorio.")
            .MaximumLength(Location.CodeMaxLength).WithMessage($"El código no puede exceder {Location.CodeMaxLength} caracteres.")
            .Matches("^[A-Za-z0-9][A-Za-z0-9._-]*$")
            .WithMessage("El código solo admite letras, números, punto, guion y guion bajo.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(Location.NameMaxLength).WithMessage($"El nombre no puede exceder {Location.NameMaxLength} caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(Location.DescriptionMaxLength)
            .WithMessage($"La descripción no puede exceder {Location.DescriptionMaxLength} caracteres.");
    }
}

internal sealed class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(Location.NameMaxLength).WithMessage($"El nombre no puede exceder {Location.NameMaxLength} caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(Location.DescriptionMaxLength)
            .WithMessage($"La descripción no puede exceder {Location.DescriptionMaxLength} caracteres.");

        RuleFor(x => x.RowVersion)
            .Must(value => RowVersionCodec.TryDecode(value, out _))
            .WithMessage("La versión de fila es obligatoria y debe estar en Base64.");
    }
}
