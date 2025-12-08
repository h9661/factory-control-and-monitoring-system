using FluentValidation;
using SmartFactory.Application.DTOs.Factory;

namespace SmartFactory.Application.Validators;

/// <summary>
/// Validator for FactoryCreateDto.
/// </summary>
public class FactoryCreateValidator : AbstractValidator<FactoryCreateDto>
{
    public FactoryCreateValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Factory code is required.")
            .MaximumLength(20)
            .WithMessage("Factory code cannot exceed 20 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$")
            .WithMessage("Factory code can only contain letters, numbers, hyphens, and underscores.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Factory name is required.")
            .MaximumLength(200)
            .WithMessage("Factory name cannot exceed 200 characters.");

        RuleFor(x => x.Location)
            .MaximumLength(200)
            .WithMessage("Location cannot exceed 200 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("Address cannot exceed 500 characters.");

        RuleFor(x => x.TimeZone)
            .NotEmpty()
            .WithMessage("Time zone is required.")
            .MaximumLength(50)
            .WithMessage("Time zone cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.ContactEmail)
            .MaximumLength(200)
            .WithMessage("Contact email cannot exceed 200 characters.")
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.ContactEmail))
            .WithMessage("Invalid email address format.");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(50)
            .WithMessage("Contact phone cannot exceed 50 characters.");
    }
}

/// <summary>
/// Validator for FactoryUpdateDto.
/// </summary>
public class FactoryUpdateValidator : AbstractValidator<FactoryUpdateDto>
{
    public FactoryUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Factory name is required.")
            .MaximumLength(200)
            .WithMessage("Factory name cannot exceed 200 characters.");

        RuleFor(x => x.Location)
            .MaximumLength(200)
            .WithMessage("Location cannot exceed 200 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("Address cannot exceed 500 characters.");

        RuleFor(x => x.TimeZone)
            .NotEmpty()
            .WithMessage("Time zone is required.")
            .MaximumLength(50)
            .WithMessage("Time zone cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.ContactEmail)
            .MaximumLength(200)
            .WithMessage("Contact email cannot exceed 200 characters.")
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.ContactEmail))
            .WithMessage("Invalid email address format.");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(50)
            .WithMessage("Contact phone cannot exceed 50 characters.");
    }
}

/// <summary>
/// Validator for ProductionLineCreateDto.
/// </summary>
public class ProductionLineCreateValidator : AbstractValidator<ProductionLineCreateDto>
{
    public ProductionLineCreateValidator()
    {
        RuleFor(x => x.FactoryId)
            .NotEmpty()
            .WithMessage("Factory is required.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Production line code is required.")
            .MaximumLength(50)
            .WithMessage("Production line code cannot exceed 50 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$")
            .WithMessage("Production line code can only contain letters, numbers, hyphens, and underscores.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Production line name is required.")
            .MaximumLength(200)
            .WithMessage("Production line name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.DesignedCapacity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Designed capacity cannot be negative.");
    }
}

/// <summary>
/// Validator for ProductionLineUpdateDto.
/// </summary>
public class ProductionLineUpdateValidator : AbstractValidator<ProductionLineUpdateDto>
{
    public ProductionLineUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Production line name is required.")
            .MaximumLength(200)
            .WithMessage("Production line name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.DesignedCapacity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Designed capacity cannot be negative.");
    }
}
