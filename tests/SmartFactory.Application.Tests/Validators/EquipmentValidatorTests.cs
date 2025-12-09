using FluentAssertions;
using SmartFactory.Application.DTOs.Equipment;
using SmartFactory.Application.Validators;
using SmartFactory.Domain.Enums;
using Xunit;

namespace SmartFactory.Application.Tests.Validators;

/// <summary>
/// Unit tests for EquipmentCreateValidator and EquipmentUpdateValidator.
/// </summary>
public class EquipmentValidatorTests
{
    private readonly EquipmentCreateValidator _createValidator;
    private readonly EquipmentUpdateValidator _updateValidator;

    public EquipmentValidatorTests()
    {
        _createValidator = new EquipmentCreateValidator();
        _updateValidator = new EquipmentUpdateValidator();
    }

    #region EquipmentCreateValidator Tests

    [Fact]
    public async Task CreateValidator_WithValidDto_PassesValidation()
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto();

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateValidator_WithEmptyProductionLineId_FailsValidation()
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { ProductionLineId = Guid.Empty };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductionLineId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateValidator_WithInvalidCode_FailsValidation(string? code)
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { Code = code! };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code");
    }

    [Fact]
    public async Task CreateValidator_WithCodeExceeding50Characters_FailsValidation()
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { Code = new string('A', 51) };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage.Contains("50"));
    }

    [Theory]
    [InlineData("EQ@001")]
    [InlineData("EQ#001")]
    [InlineData("EQ 001")]
    [InlineData("EQ.001")]
    public async Task CreateValidator_WithInvalidCodeFormat_FailsValidation(string code)
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { Code = code };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage.Contains("letters, numbers"));
    }

    [Theory]
    [InlineData("EQ001")]
    [InlineData("EQ-001")]
    [InlineData("EQ_001")]
    [InlineData("Equipment123")]
    public async Task CreateValidator_WithValidCodeFormat_PassesValidation(string code)
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { Code = code };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateValidator_WithInvalidName_FailsValidation(string? name)
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { Name = name! };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task CreateValidator_WithNameExceeding200Characters_FailsValidation()
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { Name = new string('A', 201) };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("200"));
    }

    [Fact]
    public async Task CreateValidator_WithDescriptionExceeding1000Characters_FailsValidation()
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { Description = new string('A', 1001) };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("255.255.255.255")]
    public async Task CreateValidator_WithValidIpAddress_PassesValidation(string ipAddress)
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { IpAddress = ipAddress };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("192.168.1")]
    [InlineData("192.168.1.1.1")]
    [InlineData("abc.def.ghi.jkl")]
    [InlineData("invalid")]
    public async Task CreateValidator_WithInvalidIpAddress_FailsValidation(string ipAddress)
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { IpAddress = ipAddress };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "IpAddress");
    }

    [Fact]
    public async Task CreateValidator_WithNullIpAddress_PassesValidation()
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { IpAddress = null };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateValidator_WithZeroMaintenanceInterval_FailsValidation()
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { MaintenanceIntervalDays = 0 };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MaintenanceIntervalDays");
    }

    [Fact]
    public async Task CreateValidator_WithNegativeMaintenanceInterval_FailsValidation()
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { MaintenanceIntervalDays = -5 };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MaintenanceIntervalDays");
    }

    [Fact]
    public async Task CreateValidator_WithNullMaintenanceInterval_PassesValidation()
    {
        // Arrange
        var dto = CreateValidEquipmentCreateDto() with { MaintenanceIntervalDays = null };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region EquipmentUpdateValidator Tests

    [Fact]
    public async Task UpdateValidator_WithValidDto_PassesValidation()
    {
        // Arrange
        var dto = CreateValidEquipmentUpdateDto();

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateValidator_WithInvalidName_FailsValidation(string? name)
    {
        // Arrange
        var dto = CreateValidEquipmentUpdateDto() with { Name = name! };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData(null)]
    [InlineData("")]
    public async Task UpdateValidator_WithValidOrEmptyIpAddress_PassesValidation(string? ipAddress)
    {
        // Arrange
        var dto = CreateValidEquipmentUpdateDto() with { IpAddress = ipAddress };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateValidator_WithInvalidIpAddress_FailsValidation()
    {
        // Arrange
        var dto = CreateValidEquipmentUpdateDto() with { IpAddress = "invalid.ip" };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "IpAddress");
    }

    #endregion

    #region Helper Methods

    private static EquipmentCreateDto CreateValidEquipmentCreateDto()
    {
        return new EquipmentCreateDto
        {
            ProductionLineId = Guid.NewGuid(),
            Code = "EQ-001",
            Name = "Test Equipment",
            Type = EquipmentType.SMTMachine,
            Description = "Test description",
            IpAddress = "192.168.1.100",
            Manufacturer = "Test Manufacturer",
            Model = "Model X",
            SerialNumber = "SN-12345",
            MaintenanceIntervalDays = 30
        };
    }

    private static EquipmentUpdateDto CreateValidEquipmentUpdateDto()
    {
        return new EquipmentUpdateDto
        {
            Name = "Updated Equipment",
            Type = EquipmentType.SMTMachine,
            IsActive = true,
            Description = "Updated description",
            IpAddress = "192.168.1.101",
            Manufacturer = "Updated Manufacturer",
            Model = "Model Y",
            SerialNumber = "SN-12346",
            MaintenanceIntervalDays = 60
        };
    }

    #endregion
}
