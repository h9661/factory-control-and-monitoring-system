using FluentValidation;
using SmartFactory.Application.DTOs.Alarm;

namespace SmartFactory.Application.Validators;

/// <summary>
/// Validator for AlarmCreateDto.
/// </summary>
public class AlarmCreateValidator : AbstractValidator<AlarmCreateDto>
{
    public AlarmCreateValidator()
    {
        RuleFor(x => x.EquipmentId)
            .NotEmpty()
            .WithMessage("Equipment is required.");

        RuleFor(x => x.AlarmCode)
            .NotEmpty()
            .WithMessage("Alarm code is required.")
            .MaximumLength(50)
            .WithMessage("Alarm code cannot exceed 50 characters.");

        RuleFor(x => x.Severity)
            .IsInEnum()
            .WithMessage("Invalid alarm severity.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required.")
            .MaximumLength(500)
            .WithMessage("Message cannot exceed 500 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters.");
    }
}

/// <summary>
/// Validator for AlarmAcknowledgeDto.
/// </summary>
public class AlarmAcknowledgeValidator : AbstractValidator<AlarmAcknowledgeDto>
{
    public AlarmAcknowledgeValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.")
            .MaximumLength(100)
            .WithMessage("User ID cannot exceed 100 characters.");
    }
}

/// <summary>
/// Validator for AlarmResolveDto.
/// </summary>
public class AlarmResolveValidator : AbstractValidator<AlarmResolveDto>
{
    public AlarmResolveValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.")
            .MaximumLength(100)
            .WithMessage("User ID cannot exceed 100 characters.");

        RuleFor(x => x.ResolutionNotes)
            .MaximumLength(2000)
            .WithMessage("Resolution notes cannot exceed 2000 characters.");
    }
}
