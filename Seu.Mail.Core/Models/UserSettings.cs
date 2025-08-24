namespace Seu.Mail.Core.Models;

/// <summary>
/// Represents user-specific settings and preferences for email display, layout, and general behavior.
/// </summary>
public class UserSettings
{
    /// <summary>
    /// Unique identifier for the user settings.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display mode for the email list (e.g., title only, title and sender, etc.).
    /// </summary>
    public EmailDisplayMode EmailDisplayMode { get; set; } = EmailDisplayMode.TitleSenderPreview;

    /// <summary>
    /// Layout mode for email viewing (e.g., separate page, split right, split bottom).
    /// </summary>
    public EmailLayoutMode EmailLayoutMode { get; set; } = EmailLayoutMode.SeparatePage;

    /// <summary>
    /// Default signature to use when composing emails.
    /// </summary>
    public string? DefaultSignature { get; set; }

    /// <summary>
    /// Whether to use compact mode for UI elements.
    /// </summary>
    public bool UseCompactMode { get; set; } = false;

    /// <summary>
    /// Number of emails to display per page in the email list.
    /// </summary>
    public int EmailsPerPage { get; set; } = 50;

    /// <summary>
    /// Whether to mark emails as read when opened.
    /// </summary>
    public bool MarkAsReadOnOpen { get; set; } = true;

    /// <summary>
    /// Whether to show a preview of the email content in the list.
    /// </summary>
    public bool ShowEmailPreview { get; set; } = true;

    /// <summary>
    /// Whether to enable HTML parsing for email content.
    /// </summary>
    public bool EnableHtmlParsing { get; set; } = true;

    /// <summary>
    /// Whether to enable keyboard navigation for email actions.
    /// </summary>
    public bool EnableKeyboardNavigation { get; set; } = true;

    /// <summary>
    /// Date and time when the settings were created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the settings were last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Specifies the display mode for the email list.
/// </summary>
public enum EmailDisplayMode
{
    /// <summary>
    /// Show only the email title.
    /// </summary>
    TitleOnly = 1,

    /// <summary>
    /// Show the email title and sender.
    /// </summary>
    TitleSender = 2,

    /// <summary>
    /// Show the email title, sender, and a preview of the content.
    /// </summary>
    TitleSenderPreview = 3
}

/// <summary>
/// Specifies the layout mode for viewing emails.
/// </summary>
public enum EmailLayoutMode
{
    /// <summary>
    /// Each email is viewed on a separate page.
    /// </summary>
    SeparatePage = 1,

    /// <summary>
    /// Email view is split to the right of the list.
    /// </summary>
    SplitRight = 2,

    /// <summary>
    /// Email view is split to the bottom of the list.
    /// </summary>
    SplitBottom = 3
}
