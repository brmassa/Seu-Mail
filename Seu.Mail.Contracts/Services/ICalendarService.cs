using Seu.Mail.Core.Models.Calendar;

namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Service interface for calendar operations including CRUD operations for events, recurrence handling, and calendar management
/// </summary>
/// <summary>
/// Service interface for calendar operations including CRUD operations for events, recurrence handling, and calendar management.
/// </summary>
public interface ICalendarService
{
    /// <summary>
    /// Creates a new calendar event.
    /// </summary>
    /// <param name="calendarEvent">The event to create.</param>
    /// <returns>The created event with assigned ID.</returns>
    Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent);

    /// <summary>
    /// Updates an existing calendar event.
    /// </summary>
    /// <param name="calendarEvent">The event to update.</param>
    /// <returns>The updated event.</returns>
    Task<CalendarEvent> UpdateEventAsync(CalendarEvent calendarEvent);

    /// <summary>
    /// Deletes a calendar event.
    /// </summary>
    /// <param name="eventId">ID of the event to delete.</param>
    /// <param name="deleteRecurring">Whether to delete all recurring instances.</param>
    Task DeleteEventAsync(int eventId, bool deleteRecurring = false);

    /// <summary>
    /// Gets a calendar event by ID.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <returns>The calendar event or null if not found.</returns>
    Task<CalendarEvent?> GetEventAsync(int eventId);

    /// <summary>
    /// Gets events for a specific account within a date range.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="startDate">Start date of the range.</param>
    /// <param name="endDate">End date of the range.</param>
    /// <returns>List of events in the specified range.</returns>
    Task<List<CalendarEvent>> GetEventsAsync(int accountId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets events for a specific day.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="date">The date to get events for.</param>
    /// <returns>List of events for the specified day.</returns>
    Task<List<CalendarEvent>> GetEventsByDateAsync(int accountId, DateTime date);

    /// <summary>
    /// Searches events by title or description.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <returns>List of matching events.</returns>
    Task<List<CalendarEvent>> SearchEventsAsync(int accountId, string searchTerm, int maxResults = 50);

    /// <summary>
    /// Creates recurring event instances based on recurrence rule.
    /// </summary>
    /// <param name="parentEvent">The parent recurring event.</param>
    /// <param name="endDate">End date for generating instances.</param>
    /// <returns>List of generated event instances.</returns>
    Task<List<CalendarEvent>> GenerateRecurringInstancesAsync(CalendarEvent parentEvent, DateTime endDate);

    /// <summary>
    /// Updates a single instance of a recurring event.
    /// </summary>
    /// <param name="eventId">ID of the event instance to update.</param>
    /// <param name="updatedEvent">The updated event data.</param>
    /// <returns>The updated event instance.</returns>
    Task<CalendarEvent> UpdateRecurringInstanceAsync(int eventId, CalendarEvent updatedEvent);

    /// <summary>
    /// Deletes a single instance of a recurring event.
    /// </summary>
    /// <param name="eventId">ID of the event instance to delete.</param>
    Task DeleteRecurringInstanceAsync(int eventId);

    /// <summary>
    /// Adds a reminder to an event.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="reminder">The reminder to add.</param>
    /// <returns>The created reminder.</returns>
    Task<EventReminder> AddReminderAsync(int eventId, EventReminder reminder);

    /// <summary>
    /// Removes a reminder from an event.
    /// </summary>
    /// <param name="reminderId">The reminder ID to remove.</param>
    Task RemoveReminderAsync(int reminderId);

    /// <summary>
    /// Gets pending reminders that should be triggered.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>List of reminders to trigger.</returns>
    Task<List<EventReminder>> GetPendingRemindersAsync(int accountId);

    /// <summary>
    /// Marks a reminder as triggered.
    /// </summary>
    /// <param name="reminderId">The reminder ID.</param>
    Task MarkReminderTriggeredAsync(int reminderId);

    /// <summary>
    /// Adds an attendee to an event.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="attendee">The attendee to add.</param>
    /// <returns>The created attendee.</returns>
    Task<EventAttendee> AddAttendeeAsync(int eventId, EventAttendee attendee);

    /// <summary>
    /// Updates an attendee's response status.
    /// </summary>
    /// <param name="attendeeId">The attendee ID.</param>
    /// <param name="responseStatus">The new response status.</param>
    /// <param name="comment">Optional response comment.</param>
    Task UpdateAttendeeResponseAsync(int attendeeId, AttendeeResponseStatus responseStatus, string? comment = null);

    /// <summary>
    /// Removes an attendee from an event.
    /// </summary>
    /// <param name="attendeeId">The attendee ID to remove.</param>
    Task RemoveAttendeeAsync(int attendeeId);

    /// <summary>
    /// Gets event count for a specific month.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="year">The year.</param>
    /// <param name="month">The month.</param>
    /// <returns>Number of events in the specified month.</returns>
    Task<int> GetEventCountForMonthAsync(int accountId, int year, int month);

    /// <summary>
    /// Gets upcoming events for an account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="daysAhead">Number of days to look ahead.</param>
    /// <param name="maxResults">Maximum number of results.</param>
    /// <returns>List of upcoming events.</returns>
    Task<List<CalendarEvent>> GetUpcomingEventsAsync(int accountId, int daysAhead = 7, int maxResults = 10);

    /// <summary>
    /// Imports multiple events from an external source.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="events">List of events to import.</param>
    /// <param name="subscriptionId">Optional subscription ID for tracking.</param>
    /// <returns>Number of events successfully imported.</returns>
    Task<int> ImportEventsAsync(int accountId, List<CalendarEvent> events, int? subscriptionId = null);

    /// <summary>
    /// Exports events to a specific format.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="startDate">Start date for export.</param>
    /// <param name="endDate">End date for export.</param>
    /// <param name="format">Export format (e.g., "ics", "csv").</param>
    /// <returns>Exported data as string.</returns>
    Task<string> ExportEventsAsync(int accountId, DateTime startDate, DateTime endDate, string format = "ics");

    /// <summary>
    /// Synchronizes events from all active subscriptions for an account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>Number of events synchronized.</returns>
    Task<int> SyncSubscriptionsAsync(int accountId);
}