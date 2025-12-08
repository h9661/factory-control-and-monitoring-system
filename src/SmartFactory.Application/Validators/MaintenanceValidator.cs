using FluentValidation;
using SmartFactory.Application.DTOs.Maintenance;

namespace SmartFactory.Application.Validators;

/// <summary>
/// Validator for MaintenanceCreateDto.
/// </summary>
public class MaintenanceCreateValidator : AbstractValidator<MaintenanceCreateDto>
{
    public MaintenanceCreateValidator()
    {
        RuleFor(x => x.EquipmentId)
            .NotEmpty()
            .WithMessage("Equipment is required.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid maintenance type.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.ScheduledDate)
            .NotEmpty()
            .WithMessage("Scheduled date is required.");

        RuleFor(x => x.TechnicianId)
            .MaximumLength(100)
            .WithMessage("Technician ID cannot exceed 100 characters.");

        RuleFor(x => x.TechnicianName)
            .MaximumLength(200)
            .WithMessage("Technician name cannot exceed 200 characters.");

        RuleFor(x => x.EstimatedCost)
            .GreaterThanOrEqualTo(0)
            .When(x => x.EstimatedCost.HasValue)
            .WithMessage("Estimated cost cannot be negative.");
    }
}

/// <summary>
/// Validator for MaintenanceCompleteDto.
/// </summary>
public class MaintenanceCompleteValidator : AbstractValidator<MaintenanceCompleteDto>
{
    public MaintenanceCompleteValidator()
    {
        RuleFor(x => x.ActualCost)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ActualCost.HasValue)
            .WithMessage("Actual cost cannot be negative.");

        RuleFor(x => x.DowntimeMinutes)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DowntimeMinutes.HasValue)
            .WithMessage("Downtime cannot be negative.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes cannot exceed 2000 characters.");

        RuleFor(x => x.PartsUsed)
            .MaximumLength(1000)
            .WithMessage("Parts used cannot exceed 1000 characters.");
    }
}

/// <summary>
/// Validator for MaintenanceRescheduleDto.
/// </summary>
public class MaintenanceRescheduleValidator : AbstractValidator<MaintenanceRescheduleDto>
{
    public MaintenanceRescheduleValidator()
    {
        RuleFor(x => x.NewScheduledDate)
            .NotEmpty()
            .WithMessage("New scheduled date is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters.");
    }
}
