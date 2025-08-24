using System.ComponentModel.DataAnnotations;

namespace Seu.Mail.Core.Models.Calendar;

/// <summary>
/// Represents an attendee for a calendar event
/// </summary>
public class EventAttendee
{
    /// <summary>
    /// Unique identifier for the event attendee.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The calendar event this attendee belongs to
    /// </summary>
    public int CalendarEventId { get; set; }

    /// <summary>
    /// Navigation property to the calendar event
    /// </summary>
    public CalendarEvent CalendarEvent { get; set; } = null!;

    /// <summary>
    /// Email address of the attendee
    /// </summary>
    [Required]
    [StringLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the attendee
    /// </summary>
    [StringLength(200)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Role of the attendee in the event
    /// </summary>
    public AttendeeRole Role { get; set; } = AttendeeRole.Required;

    /// <summary>
    /// Response status of the attendee
    /// </summary>
    public AttendeeResponseStatus ResponseStatus { get; set; } = AttendeeResponseStatus.Pending;

    /// <summary>
    /// When the attendee responded to the invitation
    /// </summary>
    public DateTime? ResponseDate { get; set; }

    /// <summary>
    /// Optional comment from the attendee
    /// </summary>
    [StringLength(500)]
    public string? ResponseComment { get; set; }

    /// <summary>
    /// Whether this attendee is the event organizer
    /// </summary>
    public bool IsOrganizer { get; set; }

    /// <summary>
    /// Whether the attendee should receive notifications
    /// </summary>
    public bool ReceiveNotifications { get; set; } = true;

    /// <summary>
    /// Date when the attendee was added
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the attendee information was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the display name or email if display name is not available
    /// </summary>
    public string GetDisplayText() => !string.IsNullOrWhiteSpace(DisplayName) ? DisplayName : Email;

    /// <summary>
    /// Gets a formatted string combining display name and email
    /// </summary>
    public string GetFormattedName()
    {
        return !string.IsNullOrWhiteSpace(DisplayName)
            ? $"{DisplayName} <{Email}>"
            : Email;
    }
}

/// <summary>
/// Role of an attendee in an event
/// </summary>
public enum AttendeeRole
{
    /// <summary>
    /// Required attendee
    /// </summary>
    Required = 1,

    /// <summary>
    /// Optional attendee
    /// </summary>
    Optional = 2,

    /// <summary>
    /// Resource (e.g., meeting room, equipment)
    /// </summary>
    Resource = 3
}

/// <summary>
/// Response status of an attendee
/// </summary>
public enum AttendeeResponseStatus
{
    /// <summary>
    /// No response yet
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Accepted the invitation
    /// </summary>
    Accepted = 2,

    /// <summary>
    /// Declined the invitation
    /// </summary>
    Declined = 3,

    /// <summary>
    /// Tentatively accepted
    /// </summary>
    Tentative = 4
}
