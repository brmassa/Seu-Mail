using Seu.Mail.Core.Models.Calendar;

namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Service interface for managing calendar settings and user preferences
/// </summary>
/// <summary>
/// Service interface for managing calendar settings and user preferences.
/// </summary>
public interface ICalendarSettingsService
{
    /// <summary>
    /// Gets calendar settings for a specific account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>Calendar settings or null if not found.</returns>
    Task<CalendarSettings?> GetSettingsAsync(int accountId);

    /// <summary>
    /// Creates or updates calendar settings for an account.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    /// <returns>The saved settings.</returns>
    Task<CalendarSettings> SaveSettingsAsync(CalendarSettings settings);

    /// <summary>
    /// Gets calendar settings for an account, creating default settings if none exist.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>Calendar settings.</returns>
    Task<CalendarSettings> GetOrCreateSettingsAsync(int accountId);

    /// <summary>
    /// Updates a specific setting for an account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="settingName">Name of the setting to update.</param>
    /// <param name="value">New value for the setting.</param>
    /// <returns>Updated settings.</returns>
    Task<CalendarSettings> UpdateSettingAsync(int accountId, string settingName, object value);

    /// <summary>
    /// Resets calendar settings to default values for an account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>Default settings.</returns>
    Task<CalendarSettings> ResetToDefaultsAsync(int accountId);

    /// <summary>
    /// Gets the default calendar settings template.
    /// </summary>
    /// <param name="accountId">The account ID to associate with the settings.</param>
    /// <returns>Default calendar settings.</returns>
    CalendarSettings GetDefaultSettings(int accountId);

    /// <summary>
    /// Validates calendar settings before saving.
    /// </summary>
    /// <param name="settings">Settings to validate.</param>
    /// <returns>Validation result with any errors.</returns>
    Task<ValidationResult> ValidateSettingsAsync(CalendarSettings settings);

    /// <summary>
    /// Gets time zone information for display.
    /// </summary>
    /// <returns>List of available time zones.</returns>
    Task<List<TimeZoneInfo>> GetAvailableTimeZonesAsync();

    /// <summary>
    /// Converts a date/time to the user's preferred time zone.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="utcDateTime">UTC date/time to convert.</param>
    /// <returns>Date/time in user's time zone.</returns>
    Task<DateTime> ConvertToUserTimeZoneAsync(int accountId, DateTime utcDateTime);

    /// <summary>
    /// Converts a date/time from user's time zone to UTC.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="localDateTime">Local date/time to convert.</param>
    /// <returns>Date/time in UTC.</returns>
    Task<DateTime> ConvertFromUserTimeZoneAsync(int accountId, DateTime localDateTime);

    /// <summary>
    /// Gets the working hours for an account based on settings.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>Working hours information.</returns>
    Task<WorkingHours> GetWorkingHoursAsync(int accountId);

    /// <summary>
    /// Exports calendar settings to a file format.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="format">Export format (json, xml).</param>
    /// <returns>Exported settings as string.</returns>
    Task<string> ExportSettingsAsync(int accountId, string format = "json");

    /// <summary>
    /// Imports calendar settings from a file.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="settingsData">Settings data to import.</param>
    /// <param name="format">Import format (json, xml).</param>
    /// <returns>Imported settings.</returns>
    Task<CalendarSettings> ImportSettingsAsync(int accountId, string settingsData, string format = "json");
}

/// <summary>
/// Represents working hours configuration
/// </summary>
/// <summary>
/// Represents working hours configuration.
/// </summary>
public record WorkingHours
{
    /// <summary>
    /// Start time of working hours.
    /// </summary>
    public TimeSpan StartTime { get; init; } = TimeSpan.FromHours(9); // 9:00 AM

    /// <summary>
    /// End time of working hours.
    /// </summary>
    public TimeSpan EndTime { get; init; } = TimeSpan.FromHours(17); // 5:00 PM

    /// <summary>
    /// Working days of the week.
    /// </summary>
    public List<DayOfWeek> WorkingDays { get; init; } = new()
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    };

    /// <summary>
    /// Time zone for working hours.
    /// </summary>
    public string TimeZone { get; init; } = "UTC";

    /// <summary>
    /// Checks if a given date/time falls within working hours.
    /// </summary>
    /// <param name="dateTime">Date/time to check.</param>
    /// <returns>True if within working hours.</returns>
    public bool IsWorkingTime(DateTime dateTime)
    {
        if (!WorkingDays.Contains(dateTime.DayOfWeek))
            return false;

        var timeOfDay = dateTime.TimeOfDay;
        return timeOfDay >= StartTime && timeOfDay <= EndTime;
    }
}

/// <summary>
/// Represents a validation result for settings
/// </summary>
/// <summary>
/// Represents a validation result for settings.
/// </summary>
public record ValidationResult
{
    /// <summary>
    /// Whether the validation passed.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// List of validation error messages.
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// List of validation warning messages.
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful <see cref="ValidationResult"/>.</returns>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    /// <param name="errors">Error messages.</param>
    /// <returns>A failed <see cref="ValidationResult"/>.</returns>
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };

    /// <summary>
    /// Creates a validation result with warnings.
    /// </summary>
    /// <param name="warnings">Warning messages.</param>
    /// <returns>A <see cref="ValidationResult"/> with warnings.</returns>
    public static ValidationResult WithWarnings(params string[] warnings) => new()
    {
        IsValid = true,
        Warnings = warnings.ToList()
    };
}
