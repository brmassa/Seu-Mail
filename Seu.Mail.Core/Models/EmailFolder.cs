using System.ComponentModel.DataAnnotations;

namespace Seu.Mail.Core.Models;

/// <summary>
/// Represents an email folder for organizing messages.
/// </summary>
public class EmailFolder
{
    /// <summary>
    /// Unique identifier for the folder.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the account this folder belongs to.
    /// </summary>
    [Required]
    public int AccountId { get; set; }

    /// <summary>
    /// Name of the folder (e.g., INBOX, SENT).
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the folder (for UI).
    /// </summary>
    [StringLength(255)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the parent folder, if any.
    /// </summary>
    public string? ParentFolder { get; set; }

    /// <summary>
    /// Number of messages in the folder.
    /// </summary>
    public int MessageCount { get; set; } = 0;

    /// <summary>
    /// Number of unread messages in the folder.
    /// </summary>
    public int UnreadCount { get; set; } = 0;

    /// <summary>
    /// Indicates if this is a system folder (INBOX, SENT, etc.).
    /// </summary>
    public bool IsSystemFolder { get; set; } = false;

    /// <summary>
    /// Type of folder (INBOX, SENT, DRAFTS, TRASH, JUNK, etc.).
    /// </summary>
    public string? FolderType { get; set; } // INBOX, SENT, DRAFTS, TRASH, JUNK, etc.

    /// <summary>
    /// Last time the folder was synchronized.
    /// </summary>
    public DateTime LastSyncAt { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Date when the folder was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the folder was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the account.
    /// </summary>
    public virtual EmailAccount Account { get; set; } = null!;

    /// <summary>
    /// Navigation property to the messages in this folder.
    /// </summary>
    public virtual ICollection<EmailMessage> EmailMessages { get; set; } = new List<EmailMessage>();

    /// <summary>
    /// Gets the display name or falls back to the folder name.
    /// </summary>
    public string GetDisplayName() => string.IsNullOrWhiteSpace(DisplayName) ? Name : DisplayName;

    /// <summary>
    /// Gets the icon CSS class for the folder type.
    /// </summary>
    public string GetFolderIcon()
    {
        return FolderType?.ToUpper() switch
        {
            "INBOX" => "fas fa-inbox",
            "SENT" => "fas fa-paper-plane",
            "DRAFTS" => "fas fa-edit",
            "TRASH" => "fas fa-trash",
            "JUNK" => "fas fa-exclamation-triangle",
            "ARCHIVE" => "fas fa-archive",
            "SPAM" => "fas fa-ban",
            _ => "fas fa-folder"
        };
    }

    /// <summary>
    /// Gets the CSS class for the folder type (for UI coloring).
    /// </summary>
    public string GetFolderClass()
    {
        return FolderType?.ToUpper() switch
        {
            "INBOX" => "text-primary",
            "SENT" => "text-success",
            "DRAFTS" => "text-warning",
            "TRASH" => "text-danger",
            "JUNK" => "text-danger",
            "ARCHIVE" => "text-secondary",
            "SPAM" => "text-danger",
            _ => "text-muted"
        };
    }
}

/// <summary>
/// Represents a tag for categorizing emails.
/// </summary>
public class EmailTag
{
    /// <summary>
    /// Unique identifier for the tag.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the account this tag belongs to.
    /// </summary>
    [Required]
    public int AccountId { get; set; }

    /// <summary>
    /// Name of the tag.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Color of the tag (hex format).
    /// </summary>
    [StringLength(7)]
    public string Color { get; set; } = "#007bff"; // Default blue color

    /// <summary>
    /// Description of the tag.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Date when the tag was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the tag was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the account.
    /// </summary>
    public virtual EmailAccount Account { get; set; } = null!;

    /// <summary>
    /// Navigation property to the email message tags.
    /// </summary>
    public virtual ICollection<EmailMessageTag> EmailMessageTags { get; set; } = new List<EmailMessageTag>();
}

/// <summary>
/// Represents a tag assigned to an email message.
/// </summary>
public class EmailMessageTag
{
    /// <summary>
    /// Unique identifier for the email message tag.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the email message this tag is assigned to.
    /// </summary>
    [Required]
    public int EmailMessageId { get; set; }

    /// <summary>
    /// ID of the tag.
    /// </summary>
    [Required]
    public int TagId { get; set; }

    /// <summary>
    /// Date when the tag was assigned.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the email message.
    /// </summary>
    public virtual EmailMessage EmailMessage { get; set; } = null!;

    /// <summary>
    /// Navigation property to the tag.
    /// </summary>
    public virtual EmailTag Tag { get; set; } = null!;
}
