using System.ComponentModel.DataAnnotations;

namespace Seu.Mail.Core.Models.Calendar;

/// <summary>
/// Represents a recurrence rule for repeating calendar events
/// </summary>
public record RecurrenceRule
{
    /// <summary>
    /// Unique identifier for the recurrence rule.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The frequency of recurrence
    /// </summary>
    public RecurrenceFrequency Frequency { get; init; }

    /// <summary>
    /// Interval between recurrences (e.g., every 2 weeks)
    /// </summary>
    public int Interval { get; init; } = 1;

    /// <summary>
    /// Number of occurrences before stopping (null for infinite)
    /// </summary>
    public int? Count { get; init; }

    /// <summary>
    /// End date for the recurrence (null for infinite)
    /// </summary>
    public DateTime? Until { get; init; }

    /// <summary>
    /// Days of the week for weekly recurrence (comma-separated)
    /// </summary>
    [StringLength(20)]
    public string? ByDayOfWeek { get; init; }

    /// <summary>
    /// Days of the month for monthly recurrence (comma-separated)
    /// </summary>
    [StringLength(100)]
    public string? ByDayOfMonth { get; init; }

    /// <summary>
    /// Months of the year for yearly recurrence (comma-separated)
    /// </summary>
    [StringLength(50)]
    public string? ByMonth { get; init; }

    /// <summary>
    /// Week of the month for monthly recurrence (-1 for last week)
    /// </summary>
    public int? ByWeekOfMonth { get; init; }

    /// <summary>
    /// Associated calendar event
    /// </summary>
    public int CalendarEventId { get; init; }

    /// <summary>
    /// Navigation property to the calendar event
    /// </summary>
    public CalendarEvent CalendarEvent { get; init; } = null!;

    /// <summary>
    /// Exception dates where recurrence should be skipped (JSON array)
    /// </summary>
    [StringLength(2000)]
    public string? ExceptionDates { get; init; }

    /// <summary>
    /// Creates a list of DateTime objects from a comma-separated string
    /// </summary>
    /// <param name="commaSeparated">Comma-separated string of values</param>
    /// <returns>List of integers</returns>
    public static List<int> ParseCommaSeparatedInts(string? commaSeparated)
    {
        if (string.IsNullOrWhiteSpace(commaSeparated))
            return new List<int>();

        return commaSeparated
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var result) ? result : 0)
            .Where(i => i > 0)
            .ToList();
    }

    /// <summary>
    /// Creates a list of DayOfWeek from a comma-separated string
    /// </summary>
    /// <param name="commaSeparated">Comma-separated string of day names</param>
    /// <returns>List of DayOfWeek</returns>
    public static List<DayOfWeek> ParseDaysOfWeek(string? commaSeparated)
    {
        if (string.IsNullOrWhiteSpace(commaSeparated))
            return new List<DayOfWeek>();

        return commaSeparated
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Enum.TryParse<DayOfWeek>(s.Trim(), true, out var result) ? result : (DayOfWeek?)null)
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .ToList();
    }

    /// <summary>
    /// Gets the days of week as a list
    /// </summary>
    public List<DayOfWeek> GetDaysOfWeek() => ParseDaysOfWeek(ByDayOfWeek);

    /// <summary>
    /// Gets the days of month as a list
    /// </summary>
    public List<int> GetDaysOfMonth() => ParseCommaSeparatedInts(ByDayOfMonth);

    /// <summary>
    /// Gets the months as a list
    /// </summary>
    public List<int> GetMonths() => ParseCommaSeparatedInts(ByMonth);
}

/// <summary>
/// Recurrence frequency options
/// </summary>
public enum RecurrenceFrequency
{
    /// <summary>
    /// Daily recurrence.
    /// </summary>
    Daily = 1,
    /// <summary>
    /// Weekly recurrence.
    /// </summary>
    Weekly = 2,
    /// <summary>
    /// Monthly recurrence.
    /// </summary>
    Monthly = 3,
    /// <summary>
    /// Yearly recurrence.
    /// </summary>
    Yearly = 4
}
