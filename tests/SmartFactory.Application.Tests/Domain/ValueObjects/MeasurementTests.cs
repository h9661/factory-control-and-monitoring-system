using FluentAssertions;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.ValueObjects;
using Xunit;

namespace SmartFactory.Application.Tests.Domain.ValueObjects;

/// <summary>
/// Unit tests for Measurement, Temperature, and Percentage value objects.
/// </summary>
public class MeasurementTests
{
    #region Measurement Tests

    [Fact]
    public void Measurement_WithValidValues_CreatesInstance()
    {
        // Arrange & Act
        var measurement = new Measurement(25.5, "°C");

        // Assert
        measurement.Value.Should().Be(25.5);
        measurement.Unit.Should().Be("°C");
        measurement.Quality.Should().Be(DataQuality.Good);
        measurement.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Measurement_WithQuality_SetsQualityCorrectly()
    {
        // Arrange & Act
        var measurement = new Measurement(100.0, "bar", DataQuality.Bad);

        // Assert
        measurement.Quality.Should().Be(DataQuality.Bad);
    }

    [Fact]
    public void Measurement_WithTimestamp_SetsTimestampCorrectly()
    {
        // Arrange
        var timestamp = new DateTime(2024, 6, 15, 10, 30, 0);

        // Act
        var measurement = new Measurement(50.0, "rpm", DataQuality.Good, timestamp);

        // Assert
        measurement.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void Measurement_IsValid_ReturnsTrueForGoodQuality()
    {
        // Arrange
        var measurement = new Measurement(25.0, "°C", DataQuality.Good);

        // Act & Assert
        measurement.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(DataQuality.Bad)]
    [InlineData(DataQuality.Uncertain)]
    public void Measurement_IsValid_ReturnsFalseForNonGoodQuality(DataQuality quality)
    {
        // Arrange
        var measurement = new Measurement(25.0, "°C", quality);

        // Act & Assert
        measurement.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Measurement_ToString_ReturnsFormattedString()
    {
        // Arrange
        var measurement = new Measurement(25.5, "°C");

        // Act
        var result = measurement.ToString();

        // Assert
        result.Should().Be("25.5 °C");
    }

    [Fact]
    public void Measurement_Equality_TwoEqualMeasurements_AreEqual()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var measurement1 = new Measurement(25.0, "°C", DataQuality.Good, timestamp);
        var measurement2 = new Measurement(25.0, "°C", DataQuality.Good, timestamp);

        // Act & Assert
        measurement1.Should().Be(measurement2);
    }

    [Fact]
    public void Measurement_Equality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var measurement1 = new Measurement(25.0, "°C", DataQuality.Good, timestamp);
        var measurement2 = new Measurement(26.0, "°C", DataQuality.Good, timestamp);

        // Act & Assert
        measurement1.Should().NotBe(measurement2);
    }

    #endregion

    #region Temperature Tests

    [Fact]
    public void Temperature_WithCelsius_CreatesInstance()
    {
        // Arrange & Act
        var temperature = new Temperature(25.0);

        // Assert
        temperature.Value.Should().Be(25.0);
        temperature.Unit.Should().Be("°C");
    }

    [Theory]
    [InlineData(0, 32)]
    [InlineData(100, 212)]
    [InlineData(-40, -40)]
    [InlineData(25, 77)]
    public void Temperature_Fahrenheit_ReturnsCorrectConversion(double celsius, double expectedFahrenheit)
    {
        // Arrange
        var temperature = new Temperature(celsius);

        // Act
        var fahrenheit = temperature.Fahrenheit;

        // Assert
        fahrenheit.Should().Be(expectedFahrenheit);
    }

    [Theory]
    [InlineData(0, 273.15)]
    [InlineData(100, 373.15)]
    [InlineData(-273.15, 0)]
    [InlineData(25, 298.15)]
    public void Temperature_Kelvin_ReturnsCorrectConversion(double celsius, double expectedKelvin)
    {
        // Arrange
        var temperature = new Temperature(celsius);

        // Act
        var kelvin = temperature.Kelvin;

        // Assert
        kelvin.Should().BeApproximately(expectedKelvin, 0.01);
    }

    [Fact]
    public void Temperature_ToString_ContainsValueAndUnit()
    {
        // Arrange
        var temperature = new Temperature(25.5);

        // Act
        var result = temperature.ToString();

        // Assert - Record types return full representation, verify it contains key info
        result.Should().Contain("25.5");
        result.Should().Contain("°C");
    }

    #endregion
}

/// <summary>
/// Unit tests for the Percentage value object.
/// </summary>
public class PercentageTests
{
    #region Constructor Tests

    [Fact]
    public void Percentage_WithValidValue_CreatesInstance()
    {
        // Arrange & Act
        var percentage = new Percentage(50.0);

        // Assert
        percentage.Value.Should().Be(50.0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Percentage_WithBoundaryValues_CreatesInstance(double value)
    {
        // Arrange & Act
        var percentage = new Percentage(value);

        // Assert
        percentage.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(-50)]
    [InlineData(100.1)]
    [InlineData(200)]
    public void Percentage_WithOutOfRangeValue_ThrowsArgumentOutOfRangeException(double value)
    {
        // Act
        var act = () => new Percentage(value);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Percentage must be between 0 and 100*");
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void Percentage_ImplicitConversionToDouble_ReturnsValue()
    {
        // Arrange
        var percentage = new Percentage(75.5);

        // Act
        double value = percentage;

        // Assert
        value.Should().Be(75.5);
    }

    [Fact]
    public void Percentage_ImplicitConversionFromDouble_CreatesPercentage()
    {
        // Arrange & Act
        Percentage percentage = 75.5;

        // Assert
        percentage.Value.Should().Be(75.5);
    }

    [Fact]
    public void Percentage_ImplicitConversionFromInvalidDouble_ThrowsException()
    {
        // Act
        var act = () => { Percentage percentage = 150.0; };

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region ToString Tests

    [Theory]
    [InlineData(0, "0.0%")]
    [InlineData(50, "50.0%")]
    [InlineData(75.5, "75.5%")]
    [InlineData(100, "100.0%")]
    public void Percentage_ToString_ReturnsFormattedString(double value, string expected)
    {
        // Arrange
        var percentage = new Percentage(value);

        // Act
        var result = percentage.ToString();

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Percentage_Equality_TwoEqualPercentages_AreEqual()
    {
        // Arrange
        var percentage1 = new Percentage(50.0);
        var percentage2 = new Percentage(50.0);

        // Act & Assert
        percentage1.Should().Be(percentage2);
    }

    [Fact]
    public void Percentage_Equality_DifferentPercentages_AreNotEqual()
    {
        // Arrange
        var percentage1 = new Percentage(50.0);
        var percentage2 = new Percentage(60.0);

        // Act & Assert
        percentage1.Should().NotBe(percentage2);
    }

    #endregion

    #region Usage in Calculations Tests

    [Fact]
    public void Percentage_InCalculation_WorksCorrectly()
    {
        // Arrange
        var percentage = new Percentage(50.0);
        var total = 200.0;

        // Act
        double result = total * percentage / 100;

        // Assert
        result.Should().Be(100.0);
    }

    [Fact]
    public void Percentage_Comparison_WorksCorrectly()
    {
        // Arrange
        var percentage1 = new Percentage(50.0);
        var percentage2 = new Percentage(75.0);

        // Act & Assert
        ((double)percentage1 < (double)percentage2).Should().BeTrue();
    }

    #endregion
}
