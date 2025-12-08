using FluentValidation;
using SmartFactory.Application.DTOs.WorkOrder;

namespace SmartFactory.Application.Validators;

/// <summary>
/// Validator for WorkOrderCreateDto.
/// </summary>
public class WorkOrderCreateValidator : AbstractValidator<WorkOrderCreateDto>
{
    public WorkOrderCreateValidator()
    {
        RuleFor(x => x.FactoryId)
            .NotEmpty()
            .WithMessage("Factory is required.");

        RuleFor(x => x.OrderNumber)
            .NotEmpty()
            .WithMessage("Order number is required.")
            .MaximumLength(50)
            .WithMessage("Order number cannot exceed 50 characters.");

        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .WithMessage("Product code is required.")
            .MaximumLength(50)
            .WithMessage("Product code cannot exceed 50 characters.");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.TargetQuantity)
            .GreaterThan(0)
            .WithMessage("Target quantity must be greater than 0.");

        RuleFor(x => x.ScheduledStart)
            .NotEmpty()
            .WithMessage("Scheduled start date is required.");

        RuleFor(x => x.ScheduledEnd)
            .NotEmpty()
            .WithMessage("Scheduled end date is required.")
            .GreaterThan(x => x.ScheduledStart)
            .WithMessage("Scheduled end date must be after scheduled start date.");

        RuleFor(x => x.CustomerName)
            .MaximumLength(200)
            .WithMessage("Customer name cannot exceed 200 characters.");

        RuleFor(x => x.CustomerOrderRef)
            .MaximumLength(100)
            .WithMessage("Customer order reference cannot exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes cannot exceed 2000 characters.");
    }
}

/// <summary>
/// Validator for WorkOrderUpdateDto.
/// </summary>
public class WorkOrderUpdateValidator : AbstractValidator<WorkOrderUpdateDto>
{
    public WorkOrderUpdateValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.TargetQuantity)
            .GreaterThan(0)
            .WithMessage("Target quantity must be greater than 0.");

        RuleFor(x => x.ScheduledStart)
            .NotEmpty()
            .WithMessage("Scheduled start date is required.");

        RuleFor(x => x.ScheduledEnd)
            .NotEmpty()
            .WithMessage("Scheduled end date is required.")
            .GreaterThan(x => x.ScheduledStart)
            .WithMessage("Scheduled end date must be after scheduled start date.");

        RuleFor(x => x.CustomerName)
            .MaximumLength(200)
            .WithMessage("Customer name cannot exceed 200 characters.");

        RuleFor(x => x.CustomerOrderRef)
            .MaximumLength(100)
            .WithMessage("Customer order reference cannot exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes cannot exceed 2000 characters.");
    }
}

/// <summary>
/// Validator for WorkOrderProgressDto.
/// </summary>
public class WorkOrderProgressValidator : AbstractValidator<WorkOrderProgressDto>
{
    public WorkOrderProgressValidator()
    {
        RuleFor(x => x.CompletedQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Completed quantity cannot be negative.");

        RuleFor(x => x.DefectQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Defect quantity cannot be negative.")
            .LessThanOrEqualTo(x => x.CompletedQuantity)
            .WithMessage("Defect quantity cannot exceed completed quantity.");
    }
}
