using SmartFactory.Domain.Enums;

namespace SmartFactory.Domain.ValueObjects;

/// <summary>
/// Represents a measurement value with unit and quality.
/// </summary>
public record Measurement
{
    public double Value { get; init; }
    public string Unit { get; init; }
    public DataQuality Quality { get; init; }
    public DateTime Timestamp { get; init; }

    public Measurement(double value, string unit, DataQuality quality = DataQuality.Good)
    {
        Value = value;
        Unit = unit;
        Quality = quality;
        Timestamp = DateTime.UtcNow;
    }

    public Measurement(double value, string unit, DataQuality quality, DateTime timestamp)
    {
        Value = value;
        Unit = unit;
        Quality = quality;
        Timestamp = timestamp;
    }

    public bool IsValid => Quality == DataQuality.Good;

    public override string ToString() => $"{Value} {Unit}";
}

/// <summary>
/// Represents a temperature measurement.
/// </summary>
public record Temperature : Measurement
{
    public Temperature(double celsius) : base(celsius, "°C") { }

    public double Fahrenheit => Value * 9 / 5 + 32;
    public double Kelvin => Value + 273.15;
}

/// <summary>
/// Represents a percentage value (0-100).
/// </summary>
public record Percentage
{
    public double Value { get; init; }

    public Percentage(double value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(value), "Percentage must be between 0 and 100.");

        Value = value;
    }

    public static implicit operator double(Percentage p) => p.Value;
    public static implicit operator Percentage(double d) => new(d);

    public override string ToString() => $"{Value:F1}%";
}
