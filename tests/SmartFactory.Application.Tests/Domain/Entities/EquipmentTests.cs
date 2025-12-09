using FluentAssertions;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using Xunit;

namespace SmartFactory.Application.Tests.Domain.Entities;

/// <summary>
/// Unit tests for the Equipment entity covering construction, status management, and maintenance tracking.
/// </summary>
public class EquipmentTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesEquipment()
    {
        // Arrange
        var productionLineId = Guid.NewGuid();
        var code = "EQ-001";
        var name = "Test Equipment";
        var type = EquipmentType.SMTMachine;

        // Act
        var equipment = new Equipment(productionLineId, code, name, type);

        // Assert
        equipment.ProductionLineId.Should().Be(productionLineId);
        equipment.Code.Should().Be("EQ-001");
        equipment.Name.Should().Be(name);
        equipment.Type.Should().Be(type);
        equipment.Status.Should().Be(EquipmentStatus.Offline);
        equipment.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithLowercaseCode_ConvertsToUppercase()
    {
        // Arrange
        var productionLineId = Guid.NewGuid();

        // Act
        var equipment = new Equipment(productionLineId, "eq-001", "Test Equipment", EquipmentType.SMTMachine);

        // Assert
        equipment.Code.Should().Be("EQ-001");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyCode_ThrowsArgumentException(string? code)
    {
        // Arrange
        var productionLineId = Guid.NewGuid();

        // Act
        var act = () => new Equipment(productionLineId, code!, "Test Equipment", EquipmentType.SMTMachine);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyName_ThrowsArgumentException(string? name)
    {
        // Arrange
        var productionLineId = Guid.NewGuid();

        // Act
        var act = () => new Equipment(productionLineId, "EQ-001", name!, EquipmentType.SMTMachine);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithNewValues_UpdatesProperties()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        var newName = "Updated Equipment";
        var newDescription = "New description";
        var newType = EquipmentType.AOIMachine;

        // Act
        equipment.Update(newName, newDescription, newType);

        // Assert
        equipment.Name.Should().Be(newName);
        equipment.Description.Should().Be(newDescription);
        equipment.Type.Should().Be(newType);
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void SetOpcConfiguration_WithValidNodeId_SetsOpcNodeId()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        var nodeId = "ns=2;s=Machine1.Temperature";

        // Act
        equipment.SetOpcConfiguration(nodeId);

        // Assert
        equipment.OpcNodeId.Should().Be(nodeId);
    }

    [Fact]
    public void SetNetworkConfiguration_WithValidIpAddress_SetsIpAddress()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        var ipAddress = "192.168.1.100";

        // Act
        equipment.SetNetworkConfiguration(ipAddress);

        // Assert
        equipment.IpAddress.Should().Be(ipAddress);
    }

    [Fact]
    public void SetManufacturerInfo_WithValidInfo_SetsAllProperties()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        var manufacturer = "ABB";
        var model = "IRB 6700";
        var serialNumber = "SN-12345";

        // Act
        equipment.SetManufacturerInfo(manufacturer, model, serialNumber);

        // Assert
        equipment.Manufacturer.Should().Be(manufacturer);
        equipment.Model.Should().Be(model);
        equipment.SerialNumber.Should().Be(serialNumber);
    }

    [Fact]
    public void SetInstallationDate_WithValidDate_SetsInstallationDate()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        var installDate = new DateTime(2023, 1, 15);

        // Act
        equipment.SetInstallationDate(installDate);

        // Assert
        equipment.InstallationDate.Should().Be(installDate);
    }

    [Fact]
    public void SetMaintenanceSchedule_WithValidInterval_SetsMaintenanceIntervalDays()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        var intervalDays = 30;

        // Act
        equipment.SetMaintenanceSchedule(intervalDays);

        // Assert
        equipment.MaintenanceIntervalDays.Should().Be(intervalDays);
    }

    #endregion

    #region Status Management Tests

    [Fact]
    public void UpdateStatus_WithDifferentStatus_UpdatesStatusAndRecordsHeartbeat()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        equipment.UpdateStatus(EquipmentStatus.Running);

        // Assert
        equipment.Status.Should().Be(EquipmentStatus.Running);
        equipment.LastHeartbeat.Should().NotBeNull();
        equipment.LastHeartbeat.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void UpdateStatus_WithSameStatus_DoesNotUpdateHeartbeat()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        equipment.UpdateStatus(EquipmentStatus.Running);
        var heartbeatAfterFirstUpdate = equipment.LastHeartbeat;

        // Act - Update to same status
        equipment.UpdateStatus(EquipmentStatus.Running);

        // Assert - Heartbeat should not change
        equipment.LastHeartbeat.Should().Be(heartbeatAfterFirstUpdate);
    }

    [Fact]
    public void RecordHeartbeat_UpdatesLastHeartbeat()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        var beforeRecord = DateTime.UtcNow;

        // Act
        equipment.RecordHeartbeat();

        // Assert
        equipment.LastHeartbeat.Should().NotBeNull();
        equipment.LastHeartbeat.Should().BeOnOrAfter(beforeRecord);
    }

    #endregion

    #region Activation Tests

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        equipment.Deactivate();

        // Act
        equipment.Activate();

        // Assert
        equipment.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        var equipment = CreateTestEquipment();

        // Act
        equipment.Deactivate();

        // Assert
        equipment.IsActive.Should().BeFalse();
    }

    #endregion

    #region Maintenance Tests

    [Fact]
    public void RecordMaintenance_UpdatesLastMaintenanceDate()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        var beforeMaintenance = DateTime.UtcNow;

        // Act
        equipment.RecordMaintenance();

        // Assert
        equipment.LastMaintenanceDate.Should().NotBeNull();
        equipment.LastMaintenanceDate.Should().BeOnOrAfter(beforeMaintenance);
    }

    [Fact]
    public void IsMaintenanceDue_WithNoMaintenanceInterval_ReturnsFalse()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        equipment.RecordMaintenance();
        // No maintenance interval set

        // Act
        var result = equipment.IsMaintenanceDue();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMaintenanceDue_WithNoLastMaintenanceDate_ReturnsFalse()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        equipment.SetMaintenanceSchedule(30);
        // No maintenance recorded

        // Act
        var result = equipment.IsMaintenanceDue();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMaintenanceDue_WithMaintenanceNotDue_ReturnsFalse()
    {
        // Arrange
        var equipment = CreateTestEquipment();
        equipment.SetMaintenanceSchedule(30);
        equipment.RecordMaintenance(); // Just maintained

        // Act
        var result = equipment.IsMaintenanceDue();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Online/Operational Status Tests

    [Theory]
    [InlineData(EquipmentStatus.Running, true)]
    [InlineData(EquipmentStatus.Idle, true)]
    [InlineData(EquipmentStatus.Warning, true)]
    [InlineData(EquipmentStatus.Error, true)]
    [InlineData(EquipmentStatus.Maintenance, true)]
    [InlineData(EquipmentStatus.Setup, true)]
    [InlineData(EquipmentStatus.Offline, false)]
    public void IsOnline_ReturnsCorrectValue(EquipmentStatus status, bool expectedOnline)
    {
        // Arrange
        var equipment = CreateTestEquipment();
        equipment.UpdateStatus(status);

        // Act & Assert
        equipment.IsOnline.Should().Be(expectedOnline);
    }

    [Theory]
    [InlineData(EquipmentStatus.Running, true)]
    [InlineData(EquipmentStatus.Idle, true)]
    [InlineData(EquipmentStatus.Warning, false)]
    [InlineData(EquipmentStatus.Error, false)]
    [InlineData(EquipmentStatus.Maintenance, false)]
    [InlineData(EquipmentStatus.Setup, false)]
    [InlineData(EquipmentStatus.Offline, false)]
    public void IsOperational_ReturnsCorrectValue(EquipmentStatus status, bool expectedOperational)
    {
        // Arrange
        var equipment = CreateTestEquipment();
        equipment.UpdateStatus(status);

        // Act & Assert
        equipment.IsOperational.Should().Be(expectedOperational);
    }

    #endregion

    #region Helper Methods

    private static Equipment CreateTestEquipment()
    {
        return new Equipment(
            Guid.NewGuid(),
            "EQ-001",
            "Test Equipment",
            EquipmentType.SMTMachine);
    }

    #endregion
}
