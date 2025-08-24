using System.ComponentModel.DataAnnotations;

namespace Seu.Mail.Core.Models.Calendar;

/// <summary>
/// Represents user-specific calendar settings and preferences
/// </summary>
public class CalendarSettings
{
    /// <summary>
    /// Unique identifier for the calendar settings
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the associated email account
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// Navigation property to the email account
    /// </summary>
    public EmailAccount Account { get; set; } = null!;

    /// <summary>
    /// First day of the week (0 = Sunday, 1 = Monday, etc.)
    /// </summary>
    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;

    /// <summary>
    /// Whether to show week numbers in calendar views
    /// </summary>
    public bool ShowWeekNumbers { get; set; } = true;

    /// <summary>
    /// Default view when opening the calendar
    /// </summary>
    public CalendarViewType DefaultView { get; set; } = CalendarViewType.Month;

    /// <summary>
    /// Time zone identifier (e.g., "America/New_York")
    /// </summary>
    [StringLength(100)]
    public string TimeZone { get; set; } = "UTC";

    /// <summary>
    /// Default start time for new events (in minutes from midnight)
    /// </summary>
    public int DefaultEventStartTime { get; set; } = 540; // 9:00 AM

    /// <summary>
    /// Default duration for new events (in minutes)
    /// </summary>
    public int DefaultEventDuration { get; set; } = 60; // 1 hour

    /// <summary>
    /// Whether to show all-day events at the top of day views
    /// </summary>
    public bool ShowAllDayEventsAtTop { get; set; } = true;

    /// <summary>
    /// Whether to show weekends in calendar views
    /// </summary>
    public bool ShowWeekends { get; set; } = true;

    /// <summary>
    /// Start hour for day/week views (0-23)
    /// </summary>
    public int DayViewStartHour { get; set; } = 6; // 6:00 AM

    /// <summary>
    /// End hour for day/week views (0-23)
    /// </summary>
    public int DayViewEndHour { get; set; } = 22; // 10:00 PM

    /// <summary>
    /// Time interval for day/week view slots (in minutes)
    /// </summary>
    public int TimeSlotInterval { get; set; } = 30; // 30 minutes

    /// <summary>
    /// Default color for new events
    /// </summary>
    [StringLength(7)] // Hex color format
    public string DefaultEventColor { get; set; } = "#007bff";

    /// <summary>
    /// Whether to confirm before deleting events
    /// </summary>
    public bool ConfirmEventDeletion { get; set; } = true;

    /// <summary>
    /// Whether to show event details in tooltips
    /// </summary>
    public bool ShowEventTooltips { get; set; } = true;

    /// <summary>
    /// Default reminder time before events (in minutes)
    /// </summary>
    public int DefaultReminderMinutes { get; set; } = 15; // 15 minutes

    /// <summary>
    /// Whether to enable automatic event reminders
    /// </summary>
    public bool EnableReminders { get; set; } = true;

    /// <summary>
    /// Date format string for displaying dates
    /// </summary>
    [StringLength(50)]
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Time format string for displaying times
    /// </summary>
    [StringLength(50)]
    public string TimeFormat { get; set; } = "HH:mm";

    /// <summary>
    /// Whether to use 24-hour time format
    /// </summary>
    public bool Use24HourFormat { get; set; } = true;

    /// <summary>
    /// Number of months to show in month view navigation
    /// </summary>
    public int MonthViewNavigationRange { get; set; } = 12;

    /// <summary>
    /// Whether to highlight today in calendar views
    /// </summary>
    public bool HighlightToday { get; set; } = true;

    /// <summary>
    /// Color for highlighting today
    /// </summary>
    [StringLength(7)] // Hex color format
    public string TodayHighlightColor { get; set; } = "#ffc107";

    /// <summary>
    /// Whether to show declined events
    /// </summary>
    public bool ShowDeclinedEvents { get; set; } = false;

    /// <summary>
    /// Maximum number of events to show in a day cell (month view)
    /// </summary>
    public int MaxEventsPerDayCell { get; set; } = 3;

    /// <summary>
    /// Whether to automatically sync calendar subscriptions
    /// </summary>
    public bool AutoSyncSubscriptions { get; set; } = true;

    /// <summary>
    /// How often to auto-sync subscriptions (in minutes)
    /// </summary>
    public int AutoSyncIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Date when the settings were created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the settings were last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the default start time as a TimeSpan
    /// </summary>
    public TimeSpan GetDefaultStartTimeSpan()
    {
        return TimeSpan.FromMinutes(DefaultEventStartTime);
    }

    /// <summary>
    /// Gets the default duration as a TimeSpan
    /// </summary>
    public TimeSpan GetDefaultDurationTimeSpan()
    {
        return TimeSpan.FromMinutes(DefaultEventDuration);
    }

    /// <summary>
    /// Gets the day view start time as a TimeSpan
    /// </summary>
    public TimeSpan GetDayViewStartTimeSpan()
    {
        return TimeSpan.FromHours(DayViewStartHour);
    }

    /// <summary>
    /// Gets the day view end time as a TimeSpan
    /// </summary>
    public TimeSpan GetDayViewEndTimeSpan()
    {
        return TimeSpan.FromHours(DayViewEndHour);
    }

    /// <summary>
    /// Gets the time slot interval as a TimeSpan
    /// </summary>
    public TimeSpan GetTimeSlotIntervalTimeSpan()
    {
        return TimeSpan.FromMinutes(TimeSlotInterval);
    }

    /// <summary>
    /// Gets the auto-sync interval as a TimeSpan
    /// </summary>
    public TimeSpan GetAutoSyncIntervalTimeSpan()
    {
        return TimeSpan.FromMinutes(AutoSyncIntervalMinutes);
    }
}

/// <summary>
/// Available calendar view types
/// </summary>
public enum CalendarViewType
{
    /// <summary>
    /// Monthly calendar view
    /// </summary>
    Month = 1,

    /// <summary>
    /// Weekly calendar view
    /// </summary>
    Week = 2,

    /// <summary>
    /// Daily calendar view
    /// </summary>
    Day = 3,

    /// <summary>
    /// Agenda/list view
    /// </summary>
    Agenda = 4
}