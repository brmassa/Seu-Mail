using System.ComponentModel.DataAnnotations;

namespace Seu.Mail.Core.Models.Calendar;

/// <summary>
/// Represents a reminder for a calendar event
/// </summary>
public class EventReminder
{
    /// <summary>
    /// Unique identifier for the event reminder.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The calendar event this reminder belongs to
    /// </summary>
    public int CalendarEventId { get; set; }

    /// <summary>
    /// Navigation property to the calendar event
    /// </summary>
    public CalendarEvent CalendarEvent { get; set; } = null!;

    /// <summary>
    /// How many minutes before the event to trigger the reminder
    /// </summary>
    public int MinutesBefore { get; set; }

    /// <summary>
    /// Type of reminder notification
    /// </summary>
    public ReminderType Type { get; set; } = ReminderType.Notification;

    /// <summary>
    /// Whether this reminder has been triggered
    /// </summary>
    public bool IsTriggered { get; set; }

    /// <summary>
    /// When this reminder was triggered (if applicable)
    /// </summary>
    public DateTime? TriggeredAt { get; set; }

    /// <summary>
    /// Optional custom message for the reminder
    /// </summary>
    [StringLength(500)]
    public string? CustomMessage { get; set; }

    /// <summary>
    /// Email address to send reminder to (for email reminders)
    /// </summary>
    [StringLength(256)]
    public string? EmailAddress { get; set; }

    /// <summary>
    /// Whether this reminder is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Date when the reminder was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a user-friendly description of when the reminder will trigger
    /// </summary>
    public string GetReminderDescription()
    {
        return MinutesBefore switch
        {
            0 => "At event time",
            < 60 => $"{MinutesBefore} minutes before",
            < 1440 => $"{MinutesBefore / 60} hours before",
            _ => $"{MinutesBefore / 1440} days before"
        };
    }

    /// <summary>
    /// Calculates when this reminder should trigger
    /// </summary>
    /// <param name="eventStartTime">The event start time</param>
    /// <returns>When the reminder should trigger</returns>
    public DateTime CalculateTriggerTime(DateTime eventStartTime)
    {
        return eventStartTime.AddMinutes(-MinutesBefore);
    }
}

/// <summary>
/// Types of reminder notifications
/// </summary>
public enum ReminderType
{
    /// <summary>
    /// Browser/application notification
    /// </summary>
    Notification = 1,

    /// <summary>
    /// Email notification
    /// </summary>
    Email = 2,

    /// <summary>
    /// Both notification and email
    /// </summary>
    Both = 3
}