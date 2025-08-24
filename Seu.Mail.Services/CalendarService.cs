using Microsoft.EntityFrameworkCore;
using Seu.Mail.Data.Context;
using Seu.Mail.Core.Models.Calendar;
using Seu.Mail.Contracts.Services;
using System.Text.Json;

namespace Seu.Mail.Services;

/// <summary>
/// Service implementation for calendar operations including CRUD operations for events, recurrence handling, and calendar management
/// </summary>
public class CalendarService : ICalendarService
{
    private readonly EmailDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarService"/> class.
    /// </summary>
    /// <param name="context">The database context for calendar data operations.</param>
    public CalendarService(EmailDbContext context)
    {
        _context = context;
    }

    // Event CRUD Operations
    /// <summary>
    /// Creates a new calendar event and generates recurring instances if applicable.
    /// </summary>
    /// <param name="calendarEvent">The calendar event to create.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created calendar event.</returns>
    public async Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent)
    {
        var entity = _context.CalendarEvents.Add(calendarEvent);
        await _context.SaveChangesAsync();

        // If this is a recurring event, generate instances
        if (calendarEvent.RecurrenceRule != null)
        {
            var endDate = calendarEvent.RecurrenceRule.Until ?? DateTime.UtcNow.AddYears(2);
            await GenerateRecurringInstancesAsync(entity.Entity, endDate);
        }

        return entity.Entity;
    }

    /// <summary>
    /// Updates an existing calendar event and regenerates recurring instances if the event has recurrence rules.
    /// </summary>
    /// <param name="calendarEvent">The calendar event with updated information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated calendar event.</returns>
    public async Task<CalendarEvent> UpdateEventAsync(CalendarEvent calendarEvent)
    {
        calendarEvent.ModifiedAt = DateTime.UtcNow;
        _context.CalendarEvents.Update(calendarEvent);
        await _context.SaveChangesAsync();

        // If recurrence rule changed, regenerate instances
        if (calendarEvent.RecurrenceRule != null && calendarEvent.ParentEventId == null)
        {
            // Remove existing instances
            var existingInstances = await _context.CalendarEvents
                .Where(e => e.ParentEventId == calendarEvent.Id)
                .ToListAsync();

            _context.CalendarEvents.RemoveRange(existingInstances);

            // Generate new instances
            var endDate = calendarEvent.RecurrenceRule.Until ?? DateTime.UtcNow.AddYears(2);
            await GenerateRecurringInstancesAsync(calendarEvent, endDate);
        }

        return calendarEvent;
    }

    /// <summary>
    /// Deletes a calendar event and optionally all its recurring instances.
    /// </summary>
    /// <param name="eventId">The database ID of the event to delete.</param>
    /// <param name="deleteRecurring">Whether to delete all recurring instances of the event.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteEventAsync(int eventId, bool deleteRecurring = false)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.ChildEvents)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (calendarEvent == null)
            return;

        if (deleteRecurring && calendarEvent.ChildEvents.Any())
        {
            _context.CalendarEvents.RemoveRange(calendarEvent.ChildEvents);
        }

        _context.CalendarEvents.Remove(calendarEvent);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets a calendar event by its database ID.
    /// </summary>
    /// <param name="eventId">The database ID of the event to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the calendar event or null if not found.</returns>
    public async Task<CalendarEvent?> GetEventAsync(int eventId)
    {
        return await _context.CalendarEvents
            .Include(e => e.RecurrenceRule)
            .Include(e => e.Reminders)
            .Include(e => e.Attendees)
            .Include(e => e.Account)
            .FirstOrDefaultAsync(e => e.Id == eventId);
    }

    /// <summary>
    /// Gets all calendar events for a specific account within the specified date range.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="startDate">The start date of the range to query.</param>
    /// <param name="endDate">The end date of the range to query.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of calendar events.</returns>
    public async Task<List<CalendarEvent>> GetEventsAsync(int accountId, DateTime startDate, DateTime endDate)
    {
        return await _context.CalendarEvents
            .Include(e => e.RecurrenceRule)
            .Include(e => e.Reminders)
            .Include(e => e.Attendees)
            .Where(e => e.AccountId == accountId &&
                       e.StartDateTime <= endDate &&
                       e.EndDateTime >= startDate)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all calendar events for a specific account on a specific date.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="date">The specific date to query events for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of calendar events for the specified date.</returns>
    public async Task<List<CalendarEvent>> GetEventsByDateAsync(int accountId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        return await GetEventsAsync(accountId, startOfDay, endOfDay);
    }

    /// <summary>
    /// Searches for calendar events by title or description for a specific account.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="searchTerm">The search term to match against event titles and descriptions.</param>
    /// <param name="maxResults">The maximum number of results to return (default: 50).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of matching calendar events.</returns>
    public async Task<List<CalendarEvent>> SearchEventsAsync(int accountId, string searchTerm, int maxResults = 50)
    {
        var lowerSearchTerm = searchTerm.ToLower();

        return await _context.CalendarEvents
            .Include(e => e.RecurrenceRule)
            .Include(e => e.Reminders)
            .Include(e => e.Attendees)
            .Where(e => e.AccountId == accountId &&
                       (e.Title.ToLower().Contains(lowerSearchTerm) ||
                        (e.Description != null && e.Description.ToLower().Contains(lowerSearchTerm)) ||
                        (e.Location != null && e.Location.ToLower().Contains(lowerSearchTerm))))
            .OrderBy(e => e.StartDateTime)
            .Take(maxResults)
            .ToListAsync();
    }

    // Recurrence Operations
    /// <summary>
    /// Generates recurring instances of a calendar event based on its recurrence rules.
    /// </summary>
    /// <param name="calendarEvent">The base calendar event with recurrence rules.</param>
    /// <param name="endDate">The end date until which to generate recurring instances.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of generated recurring event instances.</returns>
    public async Task<List<CalendarEvent>> GenerateRecurringInstancesAsync(CalendarEvent calendarEvent, DateTime endDate)
    {
        if (calendarEvent.RecurrenceRule == null)
            return new List<CalendarEvent>();

        var instances = new List<CalendarEvent>();
        var rule = calendarEvent.RecurrenceRule;
        var currentDate = calendarEvent.StartDateTime;
        var count = 0;
        var maxCount = rule.Count ?? 1000; // Safety limit

        while (currentDate <= endDate && count < maxCount)
        {
            // Skip the original event date
            if (currentDate != calendarEvent.StartDateTime)
            {
                var duration = calendarEvent.EndDateTime - calendarEvent.StartDateTime;
                var instance = new CalendarEvent
                {
                    Title = calendarEvent.Title,
                    Description = calendarEvent.Description,
                    StartDateTime = currentDate,
                    EndDateTime = currentDate + duration,
                    IsAllDay = calendarEvent.IsAllDay,
                    Location = calendarEvent.Location,
                    Color = calendarEvent.Color,
                    Priority = calendarEvent.Priority,
                    Status = calendarEvent.Status,
                    AccountId = calendarEvent.AccountId,
                    ParentEventId = calendarEvent.Id,
                    RecurrenceRule = null, // Instances don't have their own recurrence rules
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                instances.Add(instance);
            }

            currentDate = GetNextOccurrence(currentDate, rule);
            count++;
        }

        if (instances.Any())
        {
            _context.CalendarEvents.AddRange(instances);
            await _context.SaveChangesAsync();
        }

        return instances;
    }

    /// <summary>
    /// Updates a specific instance of a recurring calendar event.
    /// </summary>
    /// <param name="instanceId">The database ID of the specific recurring instance to update.</param>
    /// <param name="updatedEvent">The updated event information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated calendar event instance.</returns>
    public async Task<CalendarEvent> UpdateRecurringInstanceAsync(int instanceId, CalendarEvent updatedEvent)
    {
        var existingEvent = await GetEventAsync(instanceId);
        if (existingEvent?.ParentEventId == null)
            throw new InvalidOperationException("Event is not a recurring instance");

        updatedEvent.ModifiedAt = DateTime.UtcNow;
        _context.CalendarEvents.Update(updatedEvent);
        await _context.SaveChangesAsync();

        return updatedEvent;
    }

    /// <summary>
    /// Deletes a specific instance of a recurring calendar event.
    /// </summary>
    /// <param name="instanceId">The database ID of the specific recurring instance to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteRecurringInstanceAsync(int instanceId)
    {
        var calendarEvent = await GetEventAsync(instanceId);
        if (calendarEvent?.ParentEventId == null)
            throw new InvalidOperationException("Event is not a recurring instance");

        _context.CalendarEvents.Remove(calendarEvent);
        await _context.SaveChangesAsync();
    }

    // Reminder Operations
    /// <summary>
    /// Adds a reminder to a calendar event.
    /// </summary>
    /// <param name="eventId">The database ID of the event to add the reminder to.</param>
    /// <param name="reminder">The reminder to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created reminder.</returns>
    public async Task<EventReminder> AddReminderAsync(int eventId, EventReminder reminder)
    {
        var newReminder = new EventReminder
        {
            CalendarEventId = eventId,
            MinutesBefore = reminder.MinutesBefore,
            Type = reminder.Type,
            CustomMessage = reminder.CustomMessage,
            EmailAddress = reminder.EmailAddress,
            IsEnabled = reminder.IsEnabled
        };
        var entity = _context.EventReminders.Add(newReminder);
        await _context.SaveChangesAsync();
        return entity.Entity;
    }

    /// <summary>
    /// Removes a reminder from a calendar event.
    /// </summary>
    /// <param name="reminderId">The database ID of the reminder to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RemoveReminderAsync(int reminderId)
    {
        var reminder = await _context.EventReminders.FindAsync(reminderId);
        if (reminder != null)
        {
            _context.EventReminders.Remove(reminder);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Gets all pending reminders for a specific account that are due to be triggered.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of pending reminders.</returns>
    public async Task<List<EventReminder>> GetPendingRemindersAsync(int accountId)
    {
        var now = DateTime.UtcNow;

        return await _context.EventReminders
            .Include(r => r.CalendarEvent)
            .Where(r => r.CalendarEvent.AccountId == accountId &&
                       !r.IsTriggered &&
                       r.IsEnabled &&
                       r.CalendarEvent.StartDateTime.AddMinutes(-r.MinutesBefore) <= now)
            .ToListAsync();
    }

    /// <summary>
    /// Marks a reminder as triggered (completed).
    /// </summary>
    /// <param name="reminderId">The database ID of the reminder to mark as triggered.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task MarkReminderTriggeredAsync(int reminderId)
    {
        var reminder = await _context.EventReminders.FindAsync(reminderId);
        if (reminder != null)
        {
            var updatedReminder = new EventReminder
            {
                Id = reminder.Id,
                CalendarEventId = reminder.CalendarEventId,
                MinutesBefore = reminder.MinutesBefore,
                Type = reminder.Type,
                IsTriggered = true,
                TriggeredAt = DateTime.UtcNow,
                CustomMessage = reminder.CustomMessage,
                EmailAddress = reminder.EmailAddress,
                IsEnabled = reminder.IsEnabled,
                CreatedAt = reminder.CreatedAt
            };
            _context.EventReminders.Update(updatedReminder);
            await _context.SaveChangesAsync();
        }
    }

    // Attendee Operations
    /// <summary>
    /// Adds an attendee to a calendar event.
    /// </summary>
    /// <param name="eventId">The database ID of the event to add the attendee to.</param>
    /// <param name="attendee">The attendee to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created attendee.</returns>
    public async Task<EventAttendee> AddAttendeeAsync(int eventId, EventAttendee attendee)
    {
        var newAttendee = new EventAttendee
        {
            CalendarEventId = eventId,
            Email = attendee.Email,
            DisplayName = attendee.DisplayName,
            Role = attendee.Role,
            ResponseStatus = attendee.ResponseStatus,
            IsOrganizer = attendee.IsOrganizer,
            ReceiveNotifications = attendee.ReceiveNotifications
        };
        var entity = _context.EventAttendees.Add(newAttendee);
        await _context.SaveChangesAsync();
        return entity.Entity;
    }

    /// <summary>
    /// Updates an attendee's response status for a calendar event.
    /// </summary>
    /// <param name="attendeeId">The database ID of the attendee to update.</param>
    /// <param name="responseStatus">The new response status.</param>
    /// <param name="responseNote">Optional note accompanying the response.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task UpdateAttendeeResponseAsync(int attendeeId, AttendeeResponseStatus responseStatus, string? responseNote = null)
    {
        var attendee = await _context.EventAttendees.FindAsync(attendeeId);
        if (attendee != null)
        {
            var updatedAttendee = new EventAttendee
            {
                Id = attendee.Id,
                CalendarEventId = attendee.CalendarEventId,
                Email = attendee.Email,
                DisplayName = attendee.DisplayName,
                Role = attendee.Role,
                ResponseStatus = responseStatus,
                ResponseDate = DateTime.UtcNow,
                ResponseComment = responseNote,
                IsOrganizer = attendee.IsOrganizer,
                ReceiveNotifications = attendee.ReceiveNotifications,
                CreatedAt = attendee.CreatedAt,
                ModifiedAt = DateTime.UtcNow
            };
            _context.EventAttendees.Update(updatedAttendee);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Removes an attendee from a calendar event.
    /// </summary>
    /// <param name="attendeeId">The database ID of the attendee to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RemoveAttendeeAsync(int attendeeId)
    {
        var attendee = await _context.EventAttendees.FindAsync(attendeeId);
        if (attendee != null)
        {
            _context.EventAttendees.Remove(attendee);
            await _context.SaveChangesAsync();
        }
    }

    // Calendar Statistics
    /// <summary>
    /// Gets the count of events for a specific month and year for an account.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="year">The year to query.</param>
    /// <param name="month">The month to query.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the count of events.</returns>
    public async Task<int> GetEventCountForMonthAsync(int accountId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        return await _context.CalendarEvents
            .Where(e => e.AccountId == accountId &&
                       e.StartDateTime >= startDate &&
                       e.StartDateTime <= endDate)
            .CountAsync();
    }

    /// <summary>
    /// Gets upcoming calendar events for a specific account within the specified number of days.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="days">The number of days ahead to look for events (default: 7).</param>
    /// <param name="maxResults">The maximum number of results to return (default: 10).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of upcoming events.</returns>
    public async Task<List<CalendarEvent>> GetUpcomingEventsAsync(int accountId, int days = 7, int maxResults = 10)
    {
        var now = DateTime.UtcNow;
        var endDate = now.AddDays(days);

        return await _context.CalendarEvents
            .Include(e => e.Reminders)
            .Include(e => e.Attendees)
            .Where(e => e.AccountId == accountId &&
                       e.StartDateTime >= now &&
                       e.StartDateTime <= endDate)
            .OrderBy(e => e.StartDateTime)
            .Take(maxResults)
            .ToListAsync();
    }

    // Bulk Operations
    /// <summary>
    /// Imports a list of calendar events for a specific account.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="events">The list of events to import.</param>
    /// <param name="calendarId">Optional calendar ID to assign the events to.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of successfully imported events.</returns>
    public async Task<int> ImportEventsAsync(int accountId, List<CalendarEvent> events, int? calendarId = null)
    {
        foreach (var evt in events)
        {
            evt.AccountId = accountId;
            evt.SubscriptionId = calendarId;
            evt.CreatedAt = DateTime.UtcNow;
            evt.ModifiedAt = DateTime.UtcNow;
        }

        _context.CalendarEvents.AddRange(events);
        await _context.SaveChangesAsync();

        return events.Count;
    }

    /// <summary>
    /// Exports calendar events for a specific account within a date range to the specified format.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="startDate">The start date of the range to export.</param>
    /// <param name="endDate">The end date of the range to export.</param>
    /// <param name="format">The export format (default: "ics").</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the exported data as a string.</returns>
    public async Task<string> ExportEventsAsync(int accountId, DateTime startDate, DateTime endDate, string format = "ics")
    {
        var events = await GetEventsAsync(accountId, startDate, endDate);

        return format.ToLower() switch
        {
            "ics" => ExportToICalendar(events),
            "csv" => ExportToCsv(events),
            "json" => JsonSerializer.Serialize(events, new JsonSerializerOptions { WriteIndented = true }),
            _ => throw new ArgumentException($"Unsupported export format: {format}")
        };
    }

    /// <summary>
    /// Synchronizes calendar subscriptions for a specific account from external sources.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of synchronized subscriptions.</returns>
    public async Task<int> SyncSubscriptionsAsync(int accountId)
    {
        var subscriptions = await _context.CalendarSubscriptions
            .Where(s => s.AccountId == accountId && s.IsActive && s.IsDueForSync())
            .ToListAsync();

        var totalSynced = 0;

        foreach (var subscription in subscriptions)
        {
            try
            {
                // This would typically call an external calendar sync service
                // For now, we'll just update the sync timestamp
                var updatedSubscription = new CalendarSubscription
                {
                    Id = subscription.Id,
                    Name = subscription.Name,
                    Url = subscription.Url,
                    Type = subscription.Type,
                    Color = subscription.Color,
                    IsActive = subscription.IsActive,
                    IsReadOnly = subscription.IsReadOnly,
                    SyncIntervalMinutes = subscription.SyncIntervalMinutes,
                    LastSyncAt = DateTime.UtcNow,
                    LastSyncStatus = SyncStatus.Success,
                    LastSyncError = null,
                    EventCount = subscription.EventCount,
                    AccountId = subscription.AccountId,
                    Username = subscription.Username,
                    Password = subscription.Password,
                    ApiKey = subscription.ApiKey,
                    ETag = subscription.ETag,
                    LastModified = subscription.LastModified,
                    AutoSync = subscription.AutoSync,
                    TimeZone = subscription.TimeZone,
                    Description = subscription.Description,
                    CreatedAt = subscription.CreatedAt,
                    ModifiedAt = DateTime.UtcNow
                };

                _context.CalendarSubscriptions.Update(updatedSubscription);
                totalSynced++;
            }
            catch (Exception ex)
            {
                var failedSubscription = new CalendarSubscription
                {
                    Id = subscription.Id,
                    Name = subscription.Name,
                    Url = subscription.Url,
                    Type = subscription.Type,
                    Color = subscription.Color,
                    IsActive = subscription.IsActive,
                    IsReadOnly = subscription.IsReadOnly,
                    SyncIntervalMinutes = subscription.SyncIntervalMinutes,
                    LastSyncAt = DateTime.UtcNow,
                    LastSyncStatus = SyncStatus.Failed,
                    LastSyncError = ex.Message,
                    EventCount = subscription.EventCount,
                    AccountId = subscription.AccountId,
                    Username = subscription.Username,
                    Password = subscription.Password,
                    ApiKey = subscription.ApiKey,
                    ETag = subscription.ETag,
                    LastModified = subscription.LastModified,
                    AutoSync = subscription.AutoSync,
                    TimeZone = subscription.TimeZone,
                    Description = subscription.Description,
                    CreatedAt = subscription.CreatedAt,
                    ModifiedAt = DateTime.UtcNow
                };

                _context.CalendarSubscriptions.Update(failedSubscription);
            }
        }

        await _context.SaveChangesAsync();
        return totalSynced;
    }

    // Private helper methods
    private static DateTime GetNextOccurrence(DateTime currentDate, RecurrenceRule rule)
    {
        return rule.Frequency switch
        {
            RecurrenceFrequency.Daily => currentDate.AddDays(rule.Interval),
            RecurrenceFrequency.Weekly => GetNextWeeklyOccurrence(currentDate, rule),
            RecurrenceFrequency.Monthly => GetNextMonthlyOccurrence(currentDate, rule),
            RecurrenceFrequency.Yearly => currentDate.AddYears(rule.Interval),
            _ => currentDate.AddDays(1)
        };
    }

    private static DateTime GetNextWeeklyOccurrence(DateTime currentDate, RecurrenceRule rule)
    {
        var daysOfWeek = rule.GetDaysOfWeek();

        if (!daysOfWeek.Any())
        {
            return currentDate.AddDays(7 * rule.Interval);
        }

        var nextDate = currentDate.AddDays(1);

        // Find the next occurrence within the same week
        while (nextDate.DayOfWeek != currentDate.DayOfWeek ||
               (nextDate - currentDate).Days < 7 * rule.Interval)
        {
            if (daysOfWeek.Contains(nextDate.DayOfWeek) &&
                (nextDate - currentDate).Days >= 7 * rule.Interval)
            {
                break;
            }
            nextDate = nextDate.AddDays(1);
        }

        return nextDate;
    }

    private static DateTime GetNextMonthlyOccurrence(DateTime currentDate, RecurrenceRule rule)
    {
        var daysOfMonth = rule.GetDaysOfMonth();

        if (!daysOfMonth.Any())
        {
            return currentDate.AddMonths(rule.Interval);
        }

        var nextMonth = currentDate.AddMonths(rule.Interval);
        var targetDay = daysOfMonth.FirstOrDefault(d => d >= nextMonth.Day);

        if (targetDay == 0)
        {
            nextMonth = nextMonth.AddMonths(1);
            targetDay = daysOfMonth.First();
        }

        var daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
        targetDay = Math.Min(targetDay, daysInMonth);

        return new DateTime(nextMonth.Year, nextMonth.Month, targetDay,
                          currentDate.Hour, currentDate.Minute, currentDate.Second);
    }

    private static string ExportToICalendar(List<CalendarEvent> events)
    {
        var icalendar = new System.Text.StringBuilder();
        icalendar.AppendLine("BEGIN:VCALENDAR");
        icalendar.AppendLine("VERSION:2.0");
        icalendar.AppendLine("PRODID:-//Seu.Mail//Calendar//EN");

        foreach (var evt in events)
        {
            icalendar.AppendLine("BEGIN:VEVENT");
            icalendar.AppendLine($"UID:{evt.Id}@seu.mail");
            icalendar.AppendLine($"DTSTART:{evt.StartDateTime:yyyyMMddTHHmmssZ}");
            icalendar.AppendLine($"DTEND:{evt.EndDateTime:yyyyMMddTHHmmssZ}");
            icalendar.AppendLine($"SUMMARY:{evt.Title}");

            if (!string.IsNullOrEmpty(evt.Description))
                icalendar.AppendLine($"DESCRIPTION:{evt.Description}");

            if (!string.IsNullOrEmpty(evt.Location))
                icalendar.AppendLine($"LOCATION:{evt.Location}");

            icalendar.AppendLine($"STATUS:{evt.Status}");
            icalendar.AppendLine($"CREATED:{evt.CreatedAt:yyyyMMddTHHmmssZ}");
            icalendar.AppendLine($"LAST-MODIFIED:{evt.ModifiedAt:yyyyMMddTHHmmssZ}");
            icalendar.AppendLine("END:VEVENT");
        }

        icalendar.AppendLine("END:VCALENDAR");
        return icalendar.ToString();
    }

    private static string ExportToCsv(List<CalendarEvent> events)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Title,Description,Start,End,Location,Status,Priority");

        foreach (var evt in events)
        {
            csv.AppendLine($"\"{evt.Title}\",\"{evt.Description ?? ""}\",\"{evt.StartDateTime:yyyy-MM-dd HH:mm:ss}\",\"{evt.EndDateTime:yyyy-MM-dd HH:mm:ss}\",\"{evt.Location ?? ""}\",\"{evt.Status}\",\"{evt.Priority}\"");
        }

        return csv.ToString();
    }
}
