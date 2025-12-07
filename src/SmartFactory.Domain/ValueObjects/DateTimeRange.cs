namespace SmartFactory.Domain.ValueObjects;

/// <summary>
/// Represents a range between two dates/times.
/// </summary>
public record DateTimeRange
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }

    public DateTimeRange(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date.");

        Start = start;
        End = end;
    }

    public TimeSpan Duration => End - Start;

    public bool Contains(DateTime dateTime) => dateTime >= Start && dateTime <= End;

    public bool Overlaps(DateTimeRange other) =>
        Start < other.End && End > other.Start;

    public static DateTimeRange Today() =>
        new(DateTime.Today, DateTime.Today.AddDays(1).AddTicks(-1));

    public static DateTimeRange ThisWeek()
    {
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        return new(startOfWeek, startOfWeek.AddDays(7).AddTicks(-1));
    }

    public static DateTimeRange ThisMonth()
    {
        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        return new(startOfMonth, startOfMonth.AddMonths(1).AddTicks(-1));
    }

    public static DateTimeRange Last24Hours() =>
        new(DateTime.UtcNow.AddHours(-24), DateTime.UtcNow);

    public static DateTimeRange LastDays(int days) =>
        new(DateTime.UtcNow.AddDays(-days), DateTime.UtcNow);
}
