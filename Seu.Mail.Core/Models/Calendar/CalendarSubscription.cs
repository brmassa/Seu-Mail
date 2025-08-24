using System.ComponentModel.DataAnnotations;

namespace Seu.Mail.Core.Models.Calendar;

/// <summary>
/// Represents a subscription to an external calendar feed (e.g., iCal, Google Calendar)
/// </summary>
public class CalendarSubscription
{
    /// <summary>
    /// Unique identifier for the calendar subscription
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name/title of the calendar subscription
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL of the external calendar feed
    /// </summary>
    [Required]
    [StringLength(2000)]
    [Url]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Type of calendar subscription
    /// </summary>
    public SubscriptionType Type { get; set; } = SubscriptionType.ICalendar;

    /// <summary>
    /// Color to use for events from this subscription
    /// </summary>
    [StringLength(7)] // Hex color format
    public string Color { get; set; } = "#28a745";

    /// <summary>
    /// Whether this subscription is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether events from this subscription are read-only
    /// </summary>
    public bool IsReadOnly { get; set; } = true;

    /// <summary>
    /// How often to sync this subscription (in minutes)
    /// </summary>
    public int SyncIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// When the subscription was last synchronized
    /// </summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>
    /// Status of the last sync attempt
    /// </summary>
    public SyncStatus LastSyncStatus { get; set; } = SyncStatus.Success;

    /// <summary>
    /// Error message from the last sync attempt (if failed)
    /// </summary>
    [StringLength(1000)]
    public string? LastSyncError { get; set; }

    /// <summary>
    /// Number of events imported from this subscription
    /// </summary>
    public int EventCount { get; set; }

    /// <summary>
    /// ID of the associated email account
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// Navigation property to the email account
    /// </summary>
    public EmailAccount Account { get; set; } = null!;

    /// <summary>
    /// Events imported from this subscription
    /// </summary>
    public ICollection<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();

    /// <summary>
    /// Username for authenticated calendar feeds
    /// </summary>
    [StringLength(256)]
    public string? Username { get; set; }

    /// <summary>
    /// Password for authenticated calendar feeds (encrypted)
    /// </summary>
    [StringLength(512)]
    public string? Password { get; set; }

    /// <summary>
    /// API key for calendar services that require it
    /// </summary>
    [StringLength(512)]
    public string? ApiKey { get; set; }

    /// <summary>
    /// ETag from the last successful sync for conditional requests
    /// </summary>
    [StringLength(256)]
    public string? ETag { get; set; }

    /// <summary>
    /// Last-Modified header from the last successful sync
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Whether to automatically sync this subscription
    /// </summary>
    public bool AutoSync { get; set; } = true;

    /// <summary>
    /// Time zone identifier for this calendar (e.g., "America/New_York")
    /// </summary>
    [StringLength(100)]
    public string? TimeZone { get; set; }

    /// <summary>
    /// Optional description of the calendar subscription
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Date when the subscription was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the subscription was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Determines if the subscription is due for sync
    /// </summary>
    public bool IsDueForSync()
    {
        if (!IsActive || !AutoSync)
            return false;

        if (!LastSyncAt.HasValue)
            return true;

        return DateTime.UtcNow >= LastSyncAt.Value.AddMinutes(SyncIntervalMinutes);
    }

    /// <summary>
    /// Gets a user-friendly sync interval description
    /// </summary>
    public string GetSyncIntervalDescription()
    {
        return SyncIntervalMinutes switch
        {
            < 60 => $"{SyncIntervalMinutes} minutes",
            60 => "1 hour",
            < 1440 => $"{SyncIntervalMinutes / 60} hours",
            1440 => "Daily",
            < 10080 => $"{SyncIntervalMinutes / 1440} days",
            10080 => "Weekly",
            _ => $"{SyncIntervalMinutes / 1440} days"
        };
    }
}

/// <summary>
/// Types of calendar subscriptions
/// </summary>
public enum SubscriptionType
{
    /// <summary>
    /// iCalendar (.ics) format
    /// </summary>
    ICalendar = 1,

    /// <summary>
    /// Google Calendar
    /// </summary>
    GoogleCalendar = 2,

    /// <summary>
    /// Microsoft Outlook/Exchange
    /// </summary>
    Outlook = 3,

    /// <summary>
    /// Apple iCloud Calendar
    /// </summary>
    ICloud = 4,

    /// <summary>
    /// CalDAV protocol
    /// </summary>
    CalDAV = 5,

    /// <summary>
    /// Other/custom calendar format
    /// </summary>
    Other = 99
}

/// <summary>
/// Status of a calendar sync operation
/// </summary>
public enum SyncStatus
{
    /// <summary>
    /// Sync completed successfully
    /// </summary>
    Success = 1,

    /// <summary>
    /// Sync failed due to an error
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Sync is currently in progress
    /// </summary>
    InProgress = 3,

    /// <summary>
    /// Sync was cancelled
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Calendar not modified since last sync
    /// </summary>
    NotModified = 5
}