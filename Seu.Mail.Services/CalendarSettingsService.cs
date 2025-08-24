using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Seu.Mail.Data.Context;
using Seu.Mail.Core.Models.Calendar;
using Seu.Mail.Contracts.Services;

namespace Seu.Mail.Services;

/// <summary>
/// Provides services for managing calendar settings and user preferences.
/// </summary>
public class CalendarSettingsService : ICalendarSettingsService
{
    private readonly EmailDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarSettingsService"/> class.
    /// </summary>
    /// <param name="context">The database context for calendar settings.</param>
    public CalendarSettingsService(EmailDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets calendar settings for a specific account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>Calendar settings or null if not found.</returns>
    public async Task<CalendarSettings?> GetSettingsAsync(int accountId)
    {
        return await _context.CalendarSettings
            .FirstOrDefaultAsync(s => s.AccountId == accountId);
    }

    /// <summary>
    /// Saves calendar settings for a specific account, creating new settings or updating existing ones.
    /// </summary>
    /// <param name="settings">The calendar settings to save.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the saved calendar settings.</returns>
    public async Task<CalendarSettings> SaveSettingsAsync(CalendarSettings settings)
    {
        var existingSettings = await GetSettingsAsync(settings.AccountId);

        if (existingSettings != null)
        {
            settings.Id = existingSettings.Id;
            settings.CreatedAt = existingSettings.CreatedAt;
            settings.ModifiedAt = DateTime.UtcNow;

            _context.CalendarSettings.Update(settings);
        }
        else
        {
            settings.CreatedAt = DateTime.UtcNow;
            settings.ModifiedAt = DateTime.UtcNow;

            _context.CalendarSettings.Add(settings);
        }

        await _context.SaveChangesAsync();
        return await GetSettingsAsync(settings.AccountId) ?? settings;
    }

    /// <summary>
    /// Gets existing calendar settings for an account or creates default settings if none exist.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the calendar settings.</returns>
    public async Task<CalendarSettings> GetOrCreateSettingsAsync(int accountId)
    {
        var settings = await GetSettingsAsync(accountId);
        if (settings != null)
            return settings;

        var defaultSettings = GetDefaultSettings(accountId);
        return await SaveSettingsAsync(defaultSettings);
    }

    /// <summary>
    /// Updates a specific calendar setting for an account.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="settingKey">The key of the setting to update.</param>
    /// <param name="value">The new value for the setting.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated calendar settings.</returns>
    public async Task<CalendarSettings> UpdateSettingAsync(int accountId, string settingKey, object value)
    {
        var settings = await GetOrCreateSettingsAsync(accountId);

        switch (settingKey.ToLower())
        {
            case "firstdayofweek":
                settings.FirstDayOfWeek = (DayOfWeek)value;
                break;
            case "showweeknumbers":
                settings.ShowWeekNumbers = (bool)value;
                break;
            case "defaultview":
                settings.DefaultView = (CalendarViewType)value;
                break;
            case "timezone":
                settings.TimeZone = (string)value;
                break;
            case "defaulteventstarttime":
                settings.DefaultEventStartTime = (int)value;
                break;
            case "defaulteventduration":
                settings.DefaultEventDuration = (int)value;
                break;
            case "showalldayeventsattop":
                settings.ShowAllDayEventsAtTop = (bool)value;
                break;
            case "showweekends":
                settings.ShowWeekends = (bool)value;
                break;
            case "dayviewstarthour":
                settings.DayViewStartHour = (int)value;
                break;
            case "dayviewendhour":
                settings.DayViewEndHour = (int)value;
                break;
            case "timeslotinterval":
                settings.TimeSlotInterval = (int)value;
                break;
            case "defaulteventcolor":
                settings.DefaultEventColor = (string)value;
                break;
            case "confirmeventdeletion":
                settings.ConfirmEventDeletion = (bool)value;
                break;
            case "showeventtooltips":
                settings.ShowEventTooltips = (bool)value;
                break;
            case "defaultreminderminutes":
                settings.DefaultReminderMinutes = (int)value;
                break;
            case "enablereminders":
                settings.EnableReminders = (bool)value;
                break;
            case "dateformat":
                settings.DateFormat = (string)value;
                break;
            case "timeformat":
                settings.TimeFormat = (string)value;
                break;
            case "use24hourformat":
                settings.Use24HourFormat = (bool)value;
                break;
            case "monthviewnavigationrange":
                settings.MonthViewNavigationRange = (int)value;
                break;
            case "highlighttoday":
                settings.HighlightToday = (bool)value;
                break;
            case "todayhighlightcolor":
                settings.TodayHighlightColor = (string)value;
                break;
            case "showdeclinedevents":
                settings.ShowDeclinedEvents = (bool)value;
                break;
            case "maxeventsperday":
                settings.MaxEventsPerDayCell = (int)value;
                break;
            case "autosyncsubscriptions":
                settings.AutoSyncSubscriptions = (bool)value;
                break;
            case "autosyncintervalminutes":
                settings.AutoSyncIntervalMinutes = (int)value;
                break;
            default:
                throw new ArgumentException($"Unknown setting: {settingKey}");
        }

        return await SaveSettingsAsync(settings);
    }

    /// <summary>
    /// Resets calendar settings for an account to default values.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the reset calendar settings.</returns>
    public async Task<CalendarSettings> ResetToDefaultsAsync(int accountId)
    {
        var defaultSettings = GetDefaultSettings(accountId);
        return await SaveSettingsAsync(defaultSettings);
    }

    /// <summary>
    /// Gets the default calendar settings template.
    /// </summary>
    /// <param name="accountId">The account ID to associate with the settings.</param>
    /// <returns>Default calendar settings.</returns>
    public CalendarSettings GetDefaultSettings(int accountId)
    {
        return new CalendarSettings
        {
            AccountId = accountId,
            FirstDayOfWeek = DayOfWeek.Monday,
            ShowWeekNumbers = true,
            DefaultView = CalendarViewType.Month,
            TimeZone = "UTC",
            DefaultEventStartTime = 540,
            DefaultEventDuration = 60,
            ShowAllDayEventsAtTop = true,
            ShowWeekends = true,
            DayViewStartHour = 6,
            DayViewEndHour = 22,
            TimeSlotInterval = 30,
            DefaultEventColor = "#007bff",
            ConfirmEventDeletion = true,
            ShowEventTooltips = true,
            DefaultReminderMinutes = 15,
            EnableReminders = true,
            DateFormat = "yyyy-MM-dd",
            TimeFormat = "HH:mm",
            Use24HourFormat = true,
            MonthViewNavigationRange = 12,
            HighlightToday = true,
            TodayHighlightColor = "#ffc107",
            ShowDeclinedEvents = false,
            MaxEventsPerDayCell = 3,
            AutoSyncSubscriptions = true,
            AutoSyncIntervalMinutes = 60,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Validates calendar settings and corrects any invalid values.
    /// </summary>
    /// <param name="settings">The calendar settings to validate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains validation results.</returns>
    public async Task<ValidationResult> ValidateSettingsAsync(CalendarSettings settings)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate time zone
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZone);
        }
        catch
        {
            errors.Add($"Invalid time zone: {settings.TimeZone}");
        }

        // Validate time ranges
        if (settings.DayViewStartHour < 0 || settings.DayViewStartHour > 23)
            errors.Add("Day view start hour must be between 0 and 23");

        if (settings.DayViewEndHour < 0 || settings.DayViewEndHour > 23)
            errors.Add("Day view end hour must be between 0 and 23");

        if (settings.DayViewStartHour >= settings.DayViewEndHour)
            errors.Add("Day view start hour must be before end hour");

        // Validate event duration
        if (settings.DefaultEventDuration <= 0)
            errors.Add("Default event duration must be greater than 0");

        if (settings.DefaultEventDuration > 1440) // 24 hours
            warnings.Add("Default event duration is longer than 24 hours");

        // Validate time slot interval
        if (settings.TimeSlotInterval <= 0 || settings.TimeSlotInterval > 240) // 4 hours
            errors.Add("Time slot interval must be between 1 and 240 minutes");

        // Validate colors
        if (!IsValidHexColor(settings.DefaultEventColor))
            errors.Add("Default event color must be a valid hex color");

        if (!IsValidHexColor(settings.TodayHighlightColor))
            errors.Add("Today highlight color must be a valid hex color");

        // Validate reminder settings
        if (settings.DefaultReminderMinutes < 0)
            errors.Add("Default reminder minutes cannot be negative");

        // Validate sync settings
        if (settings.AutoSyncIntervalMinutes < 1)
            errors.Add("Auto sync interval must be at least 1 minute");

        if (settings.AutoSyncIntervalMinutes > 10080) // 1 week
            warnings.Add("Auto sync interval is longer than 1 week");

        // Validate max events per day
        if (settings.MaxEventsPerDayCell < 1 || settings.MaxEventsPerDayCell > 20)
            errors.Add("Max events per day cell must be between 1 and 20");

        // Check if account exists
        var accountExists = await _context.EmailAccounts.AnyAsync(a => a.Id == settings.AccountId);
        if (!accountExists)
            errors.Add($"Account with ID {settings.AccountId} does not exist");

        return errors.Any()
            ? ValidationResult.Failure(errors.ToArray())
            : warnings.Any()
                ? ValidationResult.WithWarnings(warnings.ToArray())
                : ValidationResult.Success();
    }

    /// <summary>
    /// Gets time zone information for display.
    /// </summary>
    /// <returns>List of available time zones.</returns>
    public async Task<List<TimeZoneInfo>> GetAvailableTimeZonesAsync()
    {
        return await Task.Run(() => TimeZoneInfo.GetSystemTimeZones().ToList());
    }

    /// <summary>
    /// Converts a UTC datetime to the user's configured timezone.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="utcDateTime">The UTC datetime to convert.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the datetime in the user's timezone.</returns>
    public async Task<DateTime> ConvertToUserTimeZoneAsync(int accountId, DateTime utcDateTime)
    {
        var settings = await GetOrCreateSettingsAsync(accountId);

        try
        {
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, userTimeZone);
        }
        catch
        {
            // Fall back to local time if user's time zone is invalid
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.Local);
        }
    }

    /// <summary>
    /// Converts a datetime from the user's timezone to UTC.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="userDateTime">The datetime in the user's timezone to convert.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the UTC datetime.</returns>
    public async Task<DateTime> ConvertFromUserTimeZoneAsync(int accountId, DateTime userDateTime)
    {
        var settings = await GetOrCreateSettingsAsync(accountId);

        try
        {
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZone);
            return TimeZoneInfo.ConvertTimeToUtc(userDateTime, userTimeZone);
        }
        catch
        {
            // Fallback to system default timezone
            return TimeZoneInfo.ConvertTimeToUtc(userDateTime, TimeZoneInfo.Local);
        }
    }

    /// <summary>
    /// Gets the working hours configuration for a specific account.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the working hours configuration.</returns>
    public async Task<WorkingHours> GetWorkingHoursAsync(int accountId)
    {
        var settings = await GetOrCreateSettingsAsync(accountId);

        return new WorkingHours
        {
            StartTime = settings.GetDayViewStartTimeSpan(),
            EndTime = settings.GetDayViewEndTimeSpan(),
            WorkingDays = settings.ShowWeekends
                ? Enum.GetValues<DayOfWeek>().ToList()
                : new List<DayOfWeek>
                {
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday
                },
            TimeZone = settings.TimeZone
        };
    }

    /// <summary>
    /// Exports calendar settings for an account in the specified format.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="format">The export format (default: "json").</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the exported settings as a string.</returns>
    public async Task<string> ExportSettingsAsync(int accountId, string format = "json")
    {
        var settings = await GetOrCreateSettingsAsync(accountId);

        return format.ToLower() switch
        {
            "json" => JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            "xml" => SerializeToXml(settings),
            _ => throw new ArgumentException($"Unsupported export format: {format}")
        };
    }

    /// <summary>
    /// Imports calendar settings for an account from the specified data and format.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="settingsData">The settings data to import.</param>
    /// <param name="format">The format of the import data (default: "json").</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the imported calendar settings.</returns>
    public async Task<CalendarSettings> ImportSettingsAsync(int accountId, string settingsData, string format = "json")
    {
        CalendarSettings importedSettings = format.ToLower() switch
        {
            "json" => JsonSerializer.Deserialize<CalendarSettings>(settingsData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? throw new ArgumentException("Invalid JSON format"),
            "xml" => DeserializeFromXml(settingsData),
            _ => throw new ArgumentException($"Unsupported import format: {format}")
        };

        // Ensure the settings are associated with the correct account
        importedSettings.AccountId = accountId;

        // Validate the imported settings
        var validation = await ValidateSettingsAsync(importedSettings);
        if (!validation.IsValid)
        {
            throw new ArgumentException($"Invalid settings: {string.Join(", ", validation.Errors)}");
        }

        return await SaveSettingsAsync(importedSettings);
    }

    // Private helper methods
    private static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        if (!color.StartsWith("#"))
            return false;

        if (color.Length != 7)
            return false;

        return color[1..].All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
    }

    private static string SerializeToXml(CalendarSettings settings)
    {
        var xmlSettings = new System.Xml.XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  "
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = System.Xml.XmlWriter.Create(stringWriter, xmlSettings);

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("CalendarSettings");

        xmlWriter.WriteElementString("AccountId", settings.AccountId.ToString());
        xmlWriter.WriteElementString("FirstDayOfWeek", settings.FirstDayOfWeek.ToString());
        xmlWriter.WriteElementString("ShowWeekNumbers", settings.ShowWeekNumbers.ToString());
        xmlWriter.WriteElementString("DefaultView", settings.DefaultView.ToString());
        xmlWriter.WriteElementString("TimeZone", settings.TimeZone);
        xmlWriter.WriteElementString("DefaultEventStartTime", settings.DefaultEventStartTime.ToString());
        xmlWriter.WriteElementString("DefaultEventDuration", settings.DefaultEventDuration.ToString());
        xmlWriter.WriteElementString("ShowAllDayEventsAtTop", settings.ShowAllDayEventsAtTop.ToString());
        xmlWriter.WriteElementString("ShowWeekends", settings.ShowWeekends.ToString());
        xmlWriter.WriteElementString("DayViewStartHour", settings.DayViewStartHour.ToString());
        xmlWriter.WriteElementString("DayViewEndHour", settings.DayViewEndHour.ToString());
        xmlWriter.WriteElementString("TimeSlotInterval", settings.TimeSlotInterval.ToString());
        xmlWriter.WriteElementString("DefaultEventColor", settings.DefaultEventColor);
        xmlWriter.WriteElementString("ConfirmEventDeletion", settings.ConfirmEventDeletion.ToString());
        xmlWriter.WriteElementString("ShowEventTooltips", settings.ShowEventTooltips.ToString());
        xmlWriter.WriteElementString("DefaultReminderMinutes", settings.DefaultReminderMinutes.ToString());
        xmlWriter.WriteElementString("EnableReminders", settings.EnableReminders.ToString());
        xmlWriter.WriteElementString("DateFormat", settings.DateFormat);
        xmlWriter.WriteElementString("TimeFormat", settings.TimeFormat);
        xmlWriter.WriteElementString("Use24HourFormat", settings.Use24HourFormat.ToString());
        xmlWriter.WriteElementString("MonthViewNavigationRange", settings.MonthViewNavigationRange.ToString());
        xmlWriter.WriteElementString("HighlightToday", settings.HighlightToday.ToString());
        xmlWriter.WriteElementString("TodayHighlightColor", settings.TodayHighlightColor);
        xmlWriter.WriteElementString("ShowDeclinedEvents", settings.ShowDeclinedEvents.ToString());
        xmlWriter.WriteElementString("MaxEventsPerDayCell", settings.MaxEventsPerDayCell.ToString());
        xmlWriter.WriteElementString("AutoSyncSubscriptions", settings.AutoSyncSubscriptions.ToString());
        xmlWriter.WriteElementString("AutoSyncIntervalMinutes", settings.AutoSyncIntervalMinutes.ToString());

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        return stringWriter.ToString();
    }

    private static CalendarSettings DeserializeFromXml(string xmlData)
    {
        var doc = new System.Xml.XmlDocument();
        doc.LoadXml(xmlData);

        var root = doc.DocumentElement ?? throw new ArgumentException("Invalid XML format");

        return new CalendarSettings
        {
            AccountId = int.Parse(root.SelectSingleNode("AccountId")?.InnerText ?? "0"),
            FirstDayOfWeek = Enum.Parse<DayOfWeek>(root.SelectSingleNode("FirstDayOfWeek")?.InnerText ?? "Monday"),
            ShowWeekNumbers = bool.Parse(root.SelectSingleNode("ShowWeekNumbers")?.InnerText ?? "true"),
            DefaultView = Enum.Parse<CalendarViewType>(root.SelectSingleNode("DefaultView")?.InnerText ?? "Month"),
            TimeZone = root.SelectSingleNode("TimeZone")?.InnerText ?? "UTC",
            DefaultEventStartTime = int.Parse(root.SelectSingleNode("DefaultEventStartTime")?.InnerText ?? "540"),
            DefaultEventDuration = int.Parse(root.SelectSingleNode("DefaultEventDuration")?.InnerText ?? "60"),
            ShowAllDayEventsAtTop = bool.Parse(root.SelectSingleNode("ShowAllDayEventsAtTop")?.InnerText ?? "true"),
            ShowWeekends = bool.Parse(root.SelectSingleNode("ShowWeekends")?.InnerText ?? "true"),
            DayViewStartHour = int.Parse(root.SelectSingleNode("DayViewStartHour")?.InnerText ?? "6"),
            DayViewEndHour = int.Parse(root.SelectSingleNode("DayViewEndHour")?.InnerText ?? "22"),
            TimeSlotInterval = int.Parse(root.SelectSingleNode("TimeSlotInterval")?.InnerText ?? "30"),
            DefaultEventColor = root.SelectSingleNode("DefaultEventColor")?.InnerText ?? "#007bff",
            ConfirmEventDeletion = bool.Parse(root.SelectSingleNode("ConfirmEventDeletion")?.InnerText ?? "true"),
            ShowEventTooltips = bool.Parse(root.SelectSingleNode("ShowEventTooltips")?.InnerText ?? "true"),
            DefaultReminderMinutes = int.Parse(root.SelectSingleNode("DefaultReminderMinutes")?.InnerText ?? "15"),
            EnableReminders = bool.Parse(root.SelectSingleNode("EnableReminders")?.InnerText ?? "true"),
            DateFormat = root.SelectSingleNode("DateFormat")?.InnerText ?? "yyyy-MM-dd",
            TimeFormat = root.SelectSingleNode("TimeFormat")?.InnerText ?? "HH:mm",
            Use24HourFormat = bool.Parse(root.SelectSingleNode("Use24HourFormat")?.InnerText ?? "true"),
            MonthViewNavigationRange = int.Parse(root.SelectSingleNode("MonthViewNavigationRange")?.InnerText ?? "12"),
            HighlightToday = bool.Parse(root.SelectSingleNode("HighlightToday")?.InnerText ?? "true"),
            TodayHighlightColor = root.SelectSingleNode("TodayHighlightColor")?.InnerText ?? "#ffc107",
            ShowDeclinedEvents = bool.Parse(root.SelectSingleNode("ShowDeclinedEvents")?.InnerText ?? "false"),
            MaxEventsPerDayCell = int.Parse(root.SelectSingleNode("MaxEventsPerDayCell")?.InnerText ?? "3"),
            AutoSyncSubscriptions = bool.Parse(root.SelectSingleNode("AutoSyncSubscriptions")?.InnerText ?? "true"),
            AutoSyncIntervalMinutes = int.Parse(root.SelectSingleNode("AutoSyncIntervalMinutes")?.InnerText ?? "60"),
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }
}
