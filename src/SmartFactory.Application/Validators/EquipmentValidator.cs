using FluentValidation;
using SmartFactory.Application.DTOs.Equipment;

namespace SmartFactory.Application.Validators;

/// <summary>
/// Validator for EquipmentCreateDto.
/// </summary>
public class EquipmentCreateValidator : AbstractValidator<EquipmentCreateDto>
{
    public EquipmentCreateValidator()
    {
        RuleFor(x => x.ProductionLineId)
            .NotEmpty()
            .WithMessage("Production line is required.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Equipment code is required.")
            .MaximumLength(50)
            .WithMessage("Equipment code cannot exceed 50 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$")
            .WithMessage("Equipment code can only contain letters, numbers, hyphens, and underscores.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Equipment name is required.")
            .MaximumLength(200)
            .WithMessage("Equipment name cannot exceed 200 characters.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid equipment type.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.OpcNodeId)
            .MaximumLength(500)
            .WithMessage("OPC Node ID cannot exceed 500 characters.");

        RuleFor(x => x.IpAddress)
            .MaximumLength(50)
            .WithMessage("IP address cannot exceed 50 characters.")
            .Matches(@"^(\d{1,3}\.){3}\d{1,3}$|^$")
            .When(x => !string.IsNullOrEmpty(x.IpAddress))
            .WithMessage("Invalid IP address format.");

        RuleFor(x => x.Manufacturer)
            .MaximumLength(200)
            .WithMessage("Manufacturer cannot exceed 200 characters.");

        RuleFor(x => x.Model)
            .MaximumLength(200)
            .WithMessage("Model cannot exceed 200 characters.");

        RuleFor(x => x.SerialNumber)
            .MaximumLength(100)
            .WithMessage("Serial number cannot exceed 100 characters.");

        RuleFor(x => x.MaintenanceIntervalDays)
            .GreaterThan(0)
            .When(x => x.MaintenanceIntervalDays.HasValue)
            .WithMessage("Maintenance interval must be greater than 0 days.");
    }
}

/// <summary>
/// Validator for EquipmentUpdateDto.
/// </summary>
public class EquipmentUpdateValidator : AbstractValidator<EquipmentUpdateDto>
{
    public EquipmentUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Equipment name is required.")
            .MaximumLength(200)
            .WithMessage("Equipment name cannot exceed 200 characters.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid equipment type.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.OpcNodeId)
            .MaximumLength(500)
            .WithMessage("OPC Node ID cannot exceed 500 characters.");

        RuleFor(x => x.IpAddress)
            .MaximumLength(50)
            .WithMessage("IP address cannot exceed 50 characters.")
            .Matches(@"^(\d{1,3}\.){3}\d{1,3}$|^$")
            .When(x => !string.IsNullOrEmpty(x.IpAddress))
            .WithMessage("Invalid IP address format.");

        RuleFor(x => x.Manufacturer)
            .MaximumLength(200)
            .WithMessage("Manufacturer cannot exceed 200 characters.");

        RuleFor(x => x.Model)
            .MaximumLength(200)
            .WithMessage("Model cannot exceed 200 characters.");

        RuleFor(x => x.SerialNumber)
            .MaximumLength(100)
            .WithMessage("Serial number cannot exceed 100 characters.");

        RuleFor(x => x.MaintenanceIntervalDays)
            .GreaterThan(0)
            .When(x => x.MaintenanceIntervalDays.HasValue)
            .WithMessage("Maintenance interval must be greater than 0 days.");
    }
}
