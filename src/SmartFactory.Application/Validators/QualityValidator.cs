using FluentValidation;
using SmartFactory.Application.DTOs.Quality;

namespace SmartFactory.Application.Validators;

/// <summary>
/// Validator for QualityRecordCreateDto.
/// </summary>
public class QualityRecordCreateValidator : AbstractValidator<QualityRecordCreateDto>
{
    public QualityRecordCreateValidator()
    {
        RuleFor(x => x.EquipmentId)
            .NotEmpty()
            .WithMessage("Equipment is required.");

        RuleFor(x => x.InspectionType)
            .IsInEnum()
            .WithMessage("Invalid inspection type.");

        RuleFor(x => x.Result)
            .IsInEnum()
            .WithMessage("Invalid inspection result.");

        RuleFor(x => x.DefectType)
            .IsInEnum()
            .When(x => x.DefectType.HasValue)
            .WithMessage("Invalid defect type.");

        RuleFor(x => x.DefectDescription)
            .MaximumLength(1000)
            .WithMessage("Defect description cannot exceed 1000 characters.");

        RuleFor(x => x.InspectorId)
            .MaximumLength(100)
            .WithMessage("Inspector ID cannot exceed 100 characters.");

        RuleFor(x => x.InspectorName)
            .MaximumLength(200)
            .WithMessage("Inspector name cannot exceed 200 characters.");

        RuleFor(x => x.SampleSize)
            .GreaterThan(0)
            .When(x => x.SampleSize.HasValue)
            .WithMessage("Sample size must be greater than 0.");

        RuleFor(x => x.DefectCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DefectCount.HasValue)
            .WithMessage("Defect count cannot be negative.")
            .LessThanOrEqualTo(x => x.SampleSize ?? int.MaxValue)
            .When(x => x.DefectCount.HasValue && x.SampleSize.HasValue)
            .WithMessage("Defect count cannot exceed sample size.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes cannot exceed 2000 characters.");
    }
}
