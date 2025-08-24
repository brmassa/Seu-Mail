using Seu.Mail.Core.Models.Calendar;

namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Service interface for managing calendar subscriptions and external calendar feeds
/// </summary>
/// <summary>
/// Service interface for managing calendar subscriptions and external calendar feeds.
/// </summary>
public interface ICalendarSubscriptionService
{
    /// <summary>
    /// Creates a new calendar subscription.
    /// </summary>
    /// <param name="subscription">The subscription to create.</param>
    /// <returns>The created subscription with assigned ID.</returns>
    Task<CalendarSubscription> CreateSubscriptionAsync(CalendarSubscription subscription);

    /// <summary>
    /// Updates an existing calendar subscription.
    /// </summary>
    /// <param name="subscription">The subscription to update.</param>
    /// <returns>The updated subscription.</returns>
    Task<CalendarSubscription> UpdateSubscriptionAsync(CalendarSubscription subscription);

    /// <summary>
    /// Deletes a calendar subscription and optionally its events.
    /// </summary>
    /// <param name="subscriptionId">ID of the subscription to delete.</param>
    /// <param name="deleteEvents">Whether to delete associated events.</param>
    Task DeleteSubscriptionAsync(int subscriptionId, bool deleteEvents = true);

    /// <summary>
    /// Gets a calendar subscription by ID.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <returns>The calendar subscription or null if not found.</returns>
    Task<CalendarSubscription?> GetSubscriptionAsync(int subscriptionId);

    /// <summary>
    /// Gets all subscriptions for a specific account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>List of calendar subscriptions.</returns>
    Task<List<CalendarSubscription>> GetSubscriptionsAsync(int accountId);

    /// <summary>
    /// Gets all active subscriptions for a specific account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>List of active calendar subscriptions.</returns>
    Task<List<CalendarSubscription>> GetActiveSubscriptionsAsync(int accountId);

    /// <summary>
    /// Gets subscriptions that are due for synchronization.
    /// </summary>
    /// <param name="accountId">The account ID (optional, null for all accounts).</param>
    /// <returns>List of subscriptions due for sync.</returns>
    Task<List<CalendarSubscription>> GetSubscriptionsDueForSyncAsync(int? accountId = null);

    /// <summary>
    /// Synchronizes a specific subscription by downloading and parsing its feed.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to sync.</param>
    /// <returns>Sync result with details.</returns>
    Task<SubscriptionSyncResult> SyncSubscriptionAsync(int subscriptionId);

    /// <summary>
    /// Synchronizes all active subscriptions for an account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>List of sync results.</returns>
    Task<List<SubscriptionSyncResult>> SyncAllSubscriptionsAsync(int accountId);

    /// <summary>
    /// Tests a subscription URL to validate it can be accessed and parsed.
    /// </summary>
    /// <param name="url">The calendar feed URL.</param>
    /// <param name="credentials">Optional authentication credentials.</param>
    /// <returns>Test result with validation details.</returns>
    Task<SubscriptionTestResult> TestSubscriptionAsync(string url, SubscriptionCredentials? credentials = null);

    /// <summary>
    /// Discovers calendar subscription information from a URL.
    /// </summary>
    /// <param name="url">The URL to discover calendar information from.</param>
    /// <returns>Discovered subscription information.</returns>
    Task<DiscoveredSubscription?> DiscoverSubscriptionAsync(string url);

    /// <summary>
    /// Imports events from an iCalendar (.ics) string.
    /// </summary>
    /// <param name="iCalendarData">The iCalendar data as string.</param>
    /// <param name="accountId">The account ID to import events for.</param>
    /// <param name="subscriptionId">Optional subscription ID for tracking.</param>
    /// <returns>Import result with statistics.</returns>
    Task<ImportResult> ImportICalendarAsync(string iCalendarData, int accountId, int? subscriptionId = null);

    /// <summary>
    /// Exports events to iCalendar format.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="startDate">Start date for export range.</param>
    /// <param name="endDate">End date for export range.</param>
    /// <param name="includePrivate">Whether to include private events.</param>
    /// <returns>iCalendar formatted string.</returns>
    Task<string> ExportToICalendarAsync(int accountId, DateTime startDate, DateTime endDate,
        bool includePrivate = true);

    /// <summary>
    /// Imports events from a CSV file.
    /// </summary>
    /// <param name="csvData">The CSV data as string.</param>
    /// <param name="accountId">The account ID to import events for.</param>
    /// <param name="mappings">Column mappings for CSV fields.</param>
    /// <returns>Import result with statistics.</returns>
    Task<ImportResult> ImportCsvAsync(string csvData, int accountId, CsvColumnMappings mappings);

    /// <summary>
    /// Exports events to CSV format.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="startDate">Start date for export range.</param>
    /// <param name="endDate">End date for export range.</param>
    /// <returns>CSV formatted string.</returns>
    Task<string> ExportToCsvAsync(int accountId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets subscription statistics.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <returns>Subscription statistics.</returns>
    Task<SubscriptionStatistics> GetSubscriptionStatisticsAsync(int subscriptionId);

    /// <summary>
    /// Validates subscription credentials.
    /// </summary>
    /// <param name="subscription">The subscription to validate.</param>
    /// <returns>Validation result.</returns>
    Task<ValidationResult> ValidateCredentialsAsync(CalendarSubscription subscription);

    /// <summary>
    /// Refreshes subscription metadata (name, description, etc.) from the feed.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <returns>Updated subscription.</returns>
    Task<CalendarSubscription> RefreshSubscriptionMetadataAsync(int subscriptionId);
}

/// <summary>
/// Represents the result of a subscription synchronization operation
/// </summary>
/// <summary>
/// Represents the result of a subscription synchronization operation.
/// </summary>
public record SubscriptionSyncResult
{
    /// <summary>
    /// ID of the synchronized subscription.
    /// </summary>
    public int SubscriptionId { get; init; }

    /// <summary>
    /// Whether the sync was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Number of events added during sync.
    /// </summary>
    public int EventsAdded { get; init; }

    /// <summary>
    /// Number of events updated during sync.
    /// </summary>
    public int EventsUpdated { get; init; }

    /// <summary>
    /// Number of events removed during sync.
    /// </summary>
    public int EventsRemoved { get; init; }

    /// <summary>
    /// Error message if sync failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Duration of the sync operation.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// When the sync was performed.
    /// </summary>
    public DateTime SyncDateTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// ETag from the response for conditional requests.
    /// </summary>
    public string? ETag { get; init; }

    /// <summary>
    /// Last-Modified header from the response.
    /// </summary>
    public DateTime? LastModified { get; init; }
}

/// <summary>
/// Represents the result of testing a subscription URL
/// </summary>
public record SubscriptionTestResult
{
    /// <summary>
    /// Whether the test was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// HTTP status code received
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Content type of the response
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Size of the response in bytes
    /// </summary>
    public long ContentLength { get; init; }

    /// <summary>
    /// Number of events found in the feed
    /// </summary>
    public int EventCount { get; init; }

    /// <summary>
    /// Calendar name from the feed (if available)
    /// </summary>
    public string? CalendarName { get; init; }

    /// <summary>
    /// Calendar description from the feed (if available)
    /// </summary>
    public string? CalendarDescription { get; init; }

    /// <summary>
    /// Error message if test failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Time taken to perform the test
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Whether authentication is required
    /// </summary>
    public bool RequiresAuthentication { get; init; }

    /// <summary>
    /// Detected subscription type
    /// </summary>
    public SubscriptionType DetectedType { get; init; }
}

/// <summary>
/// Represents discovered subscription information
/// </summary>
public record DiscoveredSubscription
{
    /// <summary>
    /// Calendar feed URL
    /// </summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// Calendar name
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Calendar description
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Detected subscription type
    /// </summary>
    public SubscriptionType Type { get; init; }

    /// <summary>
    /// Whether authentication is required
    /// </summary>
    public bool RequiresAuthentication { get; init; }

    /// <summary>
    /// Suggested color for the calendar
    /// </summary>
    public string? SuggestedColor { get; init; }

    /// <summary>
    /// Time zone of the calendar
    /// </summary>
    public string? TimeZone { get; init; }

    /// <summary>
    /// Number of events in the calendar
    /// </summary>
    public int EventCount { get; init; }
}

/// <summary>
/// Represents authentication credentials for a subscription
/// </summary>
public record SubscriptionCredentials
{
    /// <summary>
    /// Username for basic authentication
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Password for basic authentication
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// API key for API-based authentication
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// OAuth token for OAuth authentication
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    /// Additional headers for authentication
    /// </summary>
    public Dictionary<string, string> Headers { get; init; } = new();
}

/// <summary>
/// Represents the result of an import operation
/// </summary>
public record ImportResult
{
    /// <summary>
    /// Whether the import was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Number of events successfully imported
    /// </summary>
    public int EventsImported { get; init; }

    /// <summary>
    /// Number of events that failed to import
    /// </summary>
    public int EventsFailed { get; init; }

    /// <summary>
    /// Number of events that were duplicates and skipped
    /// </summary>
    public int EventsSkipped { get; init; }

    /// <summary>
    /// Total number of events processed
    /// </summary>
    public int TotalEvents { get; init; }

    /// <summary>
    /// Error messages for failed imports
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Warning messages
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Duration of the import operation
    /// </summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Represents CSV column mappings for import
/// </summary>
public record CsvColumnMappings
{
    /// <summary>
    /// Column index or name for event title
    /// </summary>
    public string Title { get; init; } = "Title";

    /// <summary>
    /// Column index or name for event description
    /// </summary>
    public string? Description { get; init; } = "Description";

    /// <summary>
    /// Column index or name for start date/time
    /// </summary>
    public string StartDateTime { get; init; } = "Start";

    /// <summary>
    /// Column index or name for end date/time
    /// </summary>
    public string EndDateTime { get; init; } = "End";

    /// <summary>
    /// Column index or name for location
    /// </summary>
    public string? Location { get; init; } = "Location";

    /// <summary>
    /// Column index or name for all-day flag
    /// </summary>
    public string? IsAllDay { get; init; } = "AllDay";

    /// <summary>
    /// Date format to use when parsing dates
    /// </summary>
    public string DateFormat { get; init; } = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// Whether the CSV has headers
    /// </summary>
    public bool HasHeaders { get; init; } = true;

    /// <summary>
    /// CSV delimiter character
    /// </summary>
    public char Delimiter { get; init; } = ',';
}

/// <summary>
/// Represents subscription statistics
/// </summary>
public record SubscriptionStatistics
{
    /// <summary>
    /// Total number of events in the subscription
    /// </summary>
    public int TotalEvents { get; init; }

    /// <summary>
    /// Number of upcoming events
    /// </summary>
    public int UpcomingEvents { get; init; }

    /// <summary>
    /// Number of past events
    /// </summary>
    public int PastEvents { get; init; }

    /// <summary>
    /// Date range of events in the subscription
    /// </summary>
    public DateRange EventDateRange { get; init; } = new();

    /// <summary>
    /// Last sync date
    /// </summary>
    public DateTime? LastSyncDate { get; init; }

    /// <summary>
    /// Next scheduled sync date
    /// </summary>
    public DateTime? NextSyncDate { get; init; }

    /// <summary>
    /// Average events per month
    /// </summary>
    public double AverageEventsPerMonth { get; init; }

    /// <summary>
    /// Size of the subscription feed
    /// </summary>
    public long FeedSizeBytes { get; init; }
}

/// <summary>
/// Represents a date range
/// </summary>
public record DateRange
{
    /// <summary>
    /// Start date of the range
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// End date of the range
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Gets the duration of the range
    /// </summary>
    public TimeSpan? Duration => EndDate.HasValue && StartDate.HasValue
        ? EndDate.Value - StartDate.Value
        : null;
}