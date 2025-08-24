using System.ComponentModel.DataAnnotations;

namespace Seu.Mail.Core.Models.Calendar;

/// <summary>
/// Represents a calendar event with support for recurring events, reminders, and attendees
/// </summary>
public class CalendarEvent
{
    /// <summary>
    /// Unique identifier for the calendar event
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The title/summary of the event
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the event
    /// </summary>
    [StringLength(4000)]
    public string? Description { get; set; }

    /// <summary>
    /// Start date and time of the event
    /// </summary>
    public DateTime StartDateTime { get; set; }

    /// <summary>
    /// End date and time of the event
    /// </summary>
    public DateTime EndDateTime { get; set; }

    /// <summary>
    /// Indicates if this is an all-day event
    /// </summary>
    public bool IsAllDay { get; set; }

    /// <summary>
    /// Time zone identifier for the event (e.g., "America/New_York")
    /// </summary>
    [StringLength(100)]
    public string? TimeZone { get; set; }

    /// <summary>
    /// Physical or virtual location of the event
    /// </summary>
    [StringLength(500)]
    public string? Location { get; set; }

    /// <summary>
    /// Color category for visual organization (hex format)
    /// </summary>
    [StringLength(7)]
    public string Color { get; set; } = "#007bff";

    /// <summary>
    /// Event priority level
    /// </summary>
    public EventPriority Priority { get; set; } = EventPriority.Normal;

    /// <summary>
    /// Current status of the event
    /// </summary>
    public EventStatus Status { get; set; } = EventStatus.Confirmed;

    /// <summary>
    /// Visibility/privacy level of the event
    /// </summary>
    public EventVisibility Visibility { get; set; } = EventVisibility.Default;

    /// <summary>
    /// How busy this event makes the user
    /// </summary>
    public EventBusyStatus BusyStatus { get; set; } = EventBusyStatus.Busy;

    /// <summary>
    /// ID of the associated email account/calendar
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// Calendar category or folder name
    /// </summary>
    [StringLength(100)]
    public string Category { get; set; } = "Default";

    /// <summary>
    /// Event organizer information
    /// </summary>
    [StringLength(320)]
    public string? Organizer { get; set; }

    /// <summary>
    /// URL associated with the event
    /// </summary>
    [StringLength(500)]
    public string? Url { get; set; }

    /// <summary>
    /// Recurrence pattern for repeating events
    /// </summary>
    public RecurrenceRule? Recurrence { get; set; }

    /// <summary>
    /// If this is a recurring event instance, reference to the master event
    /// </summary>
    public int? MasterEventId { get; set; }

    /// <summary>
    /// For recurring event instances, the original start time
    /// </summary>
    public DateTime? OriginalStartTime { get; set; }

    /// <summary>
    /// Indicates if this instance has been modified from the recurrence pattern
    /// </summary>
    public bool IsModified { get; set; } = false;

    /// <summary>
    /// External calendar system ID for synchronization
    /// </summary>
    [StringLength(255)]
    public string? ExternalId { get; set; }

    /// <summary>
    /// External calendar system this event belongs to
    /// </summary>
    [StringLength(100)]
    public string? ExternalSource { get; set; }

    /// <summary>
    /// iCalendar UID for RFC compliance
    /// </summary>
    [StringLength(255)]
    public string? ICalUid { get; set; }

    /// <summary>
    /// Sequence number for iCalendar updates
    /// </summary>
    public int Sequence { get; set; } = 0;

    /// <summary>
    /// Date and time when the event was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the event was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last synchronization timestamp
    /// </summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>
    /// Event tags for categorization
    /// </summary>
    [StringLength(500)]
    public string? Tags { get; set; }

    /// <summary>
    /// Custom event properties as JSON
    /// </summary>
    public string? CustomProperties { get; set; }

    // Navigation properties
    /// <summary>
    /// Navigation property to the associated email account.
    /// </summary>
    public virtual EmailAccount? Account { get; set; }
    /// <summary>
    /// Navigation property to the master event for recurring event instances.
    /// </summary>
    public virtual CalendarEvent? MasterEvent { get; set; }
    /// <summary>
    /// Navigation property to child recurring event instances.
    /// </summary>
    public virtual ICollection<CalendarEvent> RecurrenceInstances { get; set; } = new List<CalendarEvent>();
    /// <summary>
    /// Navigation property to event reminders.
    /// </summary>
    public virtual ICollection<EventReminder> Reminders { get; set; } = new List<EventReminder>();
    /// <summary>
    /// Navigation property to event attendees.
    /// </summary>
    public virtual ICollection<EventAttendee> Attendees { get; set; } = new List<EventAttendee>();

    // Compatibility properties for service interfaces
    /// <summary>
    /// Alias for Recurrence property to match service interface expectations
    /// </summary>
    public RecurrenceRule? RecurrenceRule
    {
        get => Recurrence;
        set => Recurrence = value;
    }

    /// <summary>
    /// Alias for MasterEventId property to match service interface expectations
    /// </summary>
    public int? ParentEventId
    {
        get => MasterEventId;
        set => MasterEventId = value;
    }

    /// <summary>
    /// Alias for RecurrenceInstances property to match service interface expectations
    /// </summary>
    public ICollection<CalendarEvent> ChildEvents
    {
        get => RecurrenceInstances;
        set => RecurrenceInstances = value;
    }

    /// <summary>
    /// Subscription ID for calendar synchronization (compatibility property)
    /// </summary>
    public int? SubscriptionId { get; set; }

    /// <summary>
    /// Gets the duration of the event
    /// </summary>
    public TimeSpan Duration => EndDateTime - StartDateTime;

    /// <summary>
    /// Determines if the event is currently happening
    /// </summary>
    public bool IsHappening
    {
        get
        {
            var now = DateTime.Now;
            return StartDateTime <= now && EndDateTime >= now;
        }
    }

    /// <summary>
    /// Determines if the event is in the past
    /// </summary>
    public bool IsPast => EndDateTime < DateTime.Now;

    /// <summary>
    /// Determines if the event is in the future
    /// </summary>
    public bool IsFuture => StartDateTime > DateTime.Now;

    /// <summary>
    /// Gets the event duration as a formatted string
    /// </summary>
    public string FormattedDuration
    {
        get
        {
            var duration = Duration;
            if (IsAllDay)
            {
                var days = (EndDateTime.Date - StartDateTime.Date).Days;
                return days == 1 ? "All day" : $"{days} days";
            }

            if (duration.TotalDays >= 1)
                return $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            return $"{duration.Minutes}m";
        }
    }

    /// <summary>
    /// Validates the event data
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Title) &&
               EndDateTime > StartDateTime &&
               AccountId > 0;
    }

    /// <summary>
    /// Creates a copy of this event for modification
    /// </summary>
    public CalendarEvent Clone()
    {
        return new CalendarEvent
        {
            Title = Title,
            Description = Description,
            StartDateTime = StartDateTime,
            EndDateTime = EndDateTime,
            IsAllDay = IsAllDay,
            TimeZone = TimeZone,
            Location = Location,
            Color = Color,
            Priority = Priority,
            Status = Status,
            Visibility = Visibility,
            BusyStatus = BusyStatus,
            AccountId = AccountId,
            Category = Category,
            Organizer = Organizer,
            Url = Url,
            Tags = Tags,
            CustomProperties = CustomProperties
        };
    }
}

/// <summary>
/// Event priority levels following RFC 5545
/// </summary>
public enum EventPriority
{
    /// <summary>
    /// Undefined priority
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Highest priority (1-3)
    /// </summary>
    High = 1,

    /// <summary>
    /// Medium priority (4-6)
    /// </summary>
    Normal = 5,

    /// <summary>
    /// Low priority (7-9)
    /// </summary>
    Low = 9
}

/// <summary>
/// Event status options following RFC 5545
/// </summary>
public enum EventStatus
{
    /// <summary>
    /// Event is tentative
    /// </summary>
    Tentative = 1,

    /// <summary>
    /// Event is confirmed
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// Event has been cancelled
    /// </summary>
    Cancelled = 3
}

/// <summary>
/// Event visibility/privacy levels
/// </summary>
public enum EventVisibility
{
    /// <summary>
    /// Use default visibility
    /// </summary>
    Default = 0,

    /// <summary>
    /// Event is public
    /// </summary>
    Public = 1,

    /// <summary>
    /// Event is private
    /// </summary>
    Private = 2,

    /// <summary>
    /// Event is confidential
    /// </summary>
    Confidential = 3
}

/// <summary>
/// Free/busy status for scheduling
/// </summary>
public enum EventBusyStatus
{
    /// <summary>
    /// Time is free
    /// </summary>
    Free = 0,

    /// <summary>
    /// Time is busy
    /// </summary>
    Busy = 1,

    /// <summary>
    /// Time is tentatively busy
    /// </summary>
    Tentative = 2,

    /// <summary>
    /// User is out of office
    /// </summary>
    OutOfOffice = 3
}
