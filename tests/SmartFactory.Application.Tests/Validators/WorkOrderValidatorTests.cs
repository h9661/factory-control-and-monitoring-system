using FluentAssertions;
using SmartFactory.Application.DTOs.WorkOrder;
using SmartFactory.Application.Validators;
using SmartFactory.Domain.Enums;
using Xunit;

namespace SmartFactory.Application.Tests.Validators;

/// <summary>
/// Unit tests for WorkOrderCreateValidator, WorkOrderUpdateValidator, and WorkOrderProgressValidator.
/// </summary>
public class WorkOrderValidatorTests
{
    private readonly WorkOrderCreateValidator _createValidator;
    private readonly WorkOrderUpdateValidator _updateValidator;
    private readonly WorkOrderProgressValidator _progressValidator;

    public WorkOrderValidatorTests()
    {
        _createValidator = new WorkOrderCreateValidator();
        _updateValidator = new WorkOrderUpdateValidator();
        _progressValidator = new WorkOrderProgressValidator();
    }

    #region WorkOrderCreateValidator Tests

    [Fact]
    public async Task CreateValidator_WithValidDto_PassesValidation()
    {
        // Arrange
        var dto = CreateValidWorkOrderCreateDto();

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateValidator_WithEmptyFactoryId_FailsValidation()
    {
        // Arrange
        var dto = CreateValidWorkOrderCreateDto() with { FactoryId = Guid.Empty };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FactoryId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateValidator_WithInvalidOrderNumber_FailsValidation(string? orderNumber)
    {
        // Arrange
        var dto = CreateValidWorkOrderCreateDto() with { OrderNumber = orderNumber! };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OrderNumber");
    }

    [Fact]
    public async Task CreateValidator_WithOrderNumberExceeding50Characters_FailsValidation()
    {
        // Arrange
        var dto = CreateValidWorkOrderCreateDto() with { OrderNumber = new string('A', 51) };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OrderNumber" && e.ErrorMessage.Contains("50"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateValidator_WithInvalidProductCode_FailsValidation(string? productCode)
    {
        // Arrange
        var dto = CreateValidWorkOrderCreateDto() with { ProductCode = productCode! };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductCode");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateValidator_WithInvalidProductName_FailsValidation(string? productName)
    {
        // Arrange
        var dto = CreateValidWorkOrderCreateDto() with { ProductName = productName! };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductName");
    }

    [Fact]
    public async Task CreateValidator_WithProductNameExceeding200Characters_FailsValidation()
    {
        // Arrange
        var dto = CreateValidWorkOrderCreateDto() with { ProductName = new string('A', 201) };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductName" && e.ErrorMessage.Contains("200"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateValidator_WithInvalidTargetQuantity_FailsValidation(int targetQuantity)
    {
        // Arrange
        var dto = CreateValidWorkOrderCreateDto() with { TargetQuantity = targetQuantity };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TargetQuantity");
    }

    [Fact]
    public async Task CreateValidator_WithEndDateBeforeStartDate_FailsValidation()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var dto = CreateValidWorkOrderCreateDto() with
        {
            ScheduledStart = now,
            ScheduledEnd = now.AddDays(-1)
        };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ScheduledEnd" && e.ErrorMessage.Contains("after"));
    }

    [Fact]
    public async Task CreateValidator_WithEndDateEqualToStartDate_FailsValidation()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var dto = CreateValidWorkOrderCreateDto() with
        {
            ScheduledStart = now,
            ScheduledEnd = now
        };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ScheduledEnd");
    }

    [Fact]
    public async Task CreateValidator_WithCustomerNameExceeding200Characters_FailsValidation()
    {
        // Arrange
        var dto = CreateValidWorkOrderCreateDto() with { CustomerName = new string('A', 201) };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerName");
    }

    [Fact]
    public async Task CreateValidator_WithNotesExceeding2000Characters_FailsValidation()
    {
        // Arrange
        var dto = CreateValidWorkOrderCreateDto() with { Notes = new string('A', 2001) };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Notes");
    }

    #endregion

    #region WorkOrderUpdateValidator Tests

    [Fact]
    public async Task UpdateValidator_WithValidDto_PassesValidation()
    {
        // Arrange
        var dto = CreateValidWorkOrderUpdateDto();

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateValidator_WithInvalidProductName_FailsValidation(string? productName)
    {
        // Arrange
        var dto = CreateValidWorkOrderUpdateDto() with { ProductName = productName! };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductName");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateValidator_WithInvalidTargetQuantity_FailsValidation(int targetQuantity)
    {
        // Arrange
        var dto = CreateValidWorkOrderUpdateDto() with { TargetQuantity = targetQuantity };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TargetQuantity");
    }

    [Fact]
    public async Task UpdateValidator_WithEndDateBeforeStartDate_FailsValidation()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var dto = CreateValidWorkOrderUpdateDto() with
        {
            ScheduledStart = now,
            ScheduledEnd = now.AddDays(-1)
        };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ScheduledEnd");
    }

    #endregion

    #region WorkOrderProgressValidator Tests

    [Fact]
    public async Task ProgressValidator_WithValidDto_PassesValidation()
    {
        // Arrange
        var dto = new WorkOrderProgressDto
        {
            CompletedQuantity = 100,
            DefectQuantity = 5
        };

        // Act
        var result = await _progressValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ProgressValidator_WithNegativeCompletedQuantity_FailsValidation()
    {
        // Arrange
        var dto = new WorkOrderProgressDto
        {
            CompletedQuantity = -1,
            DefectQuantity = 0
        };

        // Act
        var result = await _progressValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CompletedQuantity");
    }

    [Fact]
    public async Task ProgressValidator_WithNegativeDefectQuantity_FailsValidation()
    {
        // Arrange
        var dto = new WorkOrderProgressDto
        {
            CompletedQuantity = 100,
            DefectQuantity = -1
        };

        // Act
        var result = await _progressValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DefectQuantity");
    }

    [Fact]
    public async Task ProgressValidator_WithDefectQuantityExceedingCompleted_FailsValidation()
    {
        // Arrange
        var dto = new WorkOrderProgressDto
        {
            CompletedQuantity = 100,
            DefectQuantity = 101
        };

        // Act
        var result = await _progressValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DefectQuantity" && e.ErrorMessage.Contains("cannot exceed"));
    }

    [Fact]
    public async Task ProgressValidator_WithDefectQuantityEqualToCompleted_PassesValidation()
    {
        // Arrange
        var dto = new WorkOrderProgressDto
        {
            CompletedQuantity = 100,
            DefectQuantity = 100
        };

        // Act
        var result = await _progressValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ProgressValidator_WithZeroValues_PassesValidation()
    {
        // Arrange
        var dto = new WorkOrderProgressDto
        {
            CompletedQuantity = 0,
            DefectQuantity = 0
        };

        // Act
        var result = await _progressValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static WorkOrderCreateDto CreateValidWorkOrderCreateDto()
    {
        var now = DateTime.UtcNow;
        return new WorkOrderCreateDto
        {
            FactoryId = Guid.NewGuid(),
            OrderNumber = "WO-2024-001",
            ProductCode = "PROD-001",
            ProductName = "Test Product",
            TargetQuantity = 1000,
            Priority = WorkOrderPriority.Normal,
            ScheduledStart = now,
            ScheduledEnd = now.AddDays(7),
            CustomerName = "Test Customer",
            CustomerOrderRef = "CUST-REF-001",
            Notes = "Test notes"
        };
    }

    private static WorkOrderUpdateDto CreateValidWorkOrderUpdateDto()
    {
        var now = DateTime.UtcNow;
        return new WorkOrderUpdateDto
        {
            ProductName = "Updated Product",
            TargetQuantity = 2000,
            Priority = WorkOrderPriority.High,
            ScheduledStart = now,
            ScheduledEnd = now.AddDays(14),
            CustomerName = "Updated Customer",
            CustomerOrderRef = "CUST-REF-002",
            Notes = "Updated notes"
        };
    }

    #endregion
}
