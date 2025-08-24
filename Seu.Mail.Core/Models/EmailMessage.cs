using System.ComponentModel.DataAnnotations;

namespace Seu.Mail.Core.Models;

/// <summary>
/// Represents an email message with all its properties and metadata
/// </summary>
/// <summary>
/// Represents an email message with all its properties and metadata.
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// Unique identifier for the email message.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique message ID from the email server.
    /// </summary>
    [Required]
    [StringLength(500)]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the email account this message belongs to.
    /// </summary>
    [Required]
    public int AccountId { get; set; }

    /// <summary>
    /// Sender's email address and display name.
    /// </summary>
    [Required]
    [StringLength(500)]
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Recipient email addresses (comma-separated).
    /// </summary>
    [Required]
    [StringLength(2000)]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Carbon copy recipients (comma-separated).
    /// </summary>
    [StringLength(2000)]
    public string? Cc { get; set; }

    /// <summary>
    /// Blind carbon copy recipients (comma-separated).
    /// </summary>
    [StringLength(2000)]
    public string? Bcc { get; set; }

    /// <summary>
    /// Email subject line.
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Plain text version of the email body.
    /// </summary>
    public string? TextBody { get; set; }

    /// <summary>
    /// HTML version of the email body.
    /// </summary>
    public string? HtmlBody { get; set; }

    /// <summary>
    /// Date and time when the email was originally sent.
    /// </summary>
    public DateTime DateSent { get; set; }

    /// <summary>
    /// Date and time when the email was received by the server.
    /// </summary>
    public DateTime DateReceived { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the email was downloaded locally.
    /// </summary>
    public DateTime DateDownloaded { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether the email has been read.
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Indicates whether the email is marked as important.
    /// </summary>
    public bool IsImportant { get; set; } = false;

    /// <summary>
    /// Indicates whether the email has been deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Indicates whether the email is marked as spam.
    /// </summary>
    public bool IsSpam { get; set; } = false;

    /// <summary>
    /// Folder where the email is stored (INBOX, SENT, DRAFTS, etc.).
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Folder { get; set; } = "INBOX";

    /// <summary>
    /// Indicates whether the email has file attachments.
    /// </summary>
    public bool HasAttachments { get; set; } = false;

    /// <summary>
    /// Size of the email in bytes.
    /// </summary>
    public long Size { get; set; } = 0;

    /// <summary>
    /// Priority level of the email (1=High, 3=Normal, 5=Low).
    /// </summary>
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Email headers as JSON string for advanced processing.
    /// </summary>
    public string? Headers { get; set; }

    /// <summary>
    /// Thread ID for email conversation grouping.
    /// </summary>
    [StringLength(500)]
    public string? ThreadId { get; set; }

    /// <summary>
    /// References to other messages in the thread.
    /// </summary>
    [StringLength(2000)]
    public string? References { get; set; }

    /// <summary>
    /// In-Reply-To header for threading.
    /// </summary>
    [StringLength(500)]
    public string? InReplyTo { get; set; }

    /// <summary>
    /// Server-side UID for the message.
    /// </summary>
    public uint? ServerUid { get; set; }

    /// <summary>
    /// IMAP flags as comma-separated string.
    /// </summary>
    [StringLength(200)]
    public string? ImapFlags { get; set; }

    /// <summary>
    /// Custom labels/tags applied to the message.
    /// </summary>
    [StringLength(500)]
    public string? Labels { get; set; }

    /// <summary>
    /// Date when the message was created locally.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the message was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the email account this message belongs to.
    /// </summary>
    public virtual EmailAccount? Account { get; set; }

    /// <summary>
    /// Navigation property to the attachments of this email message.
    /// </summary>
    public virtual ICollection<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();

    /// <summary>
    /// Alias for ServerUid property to match service interface expectations.
    /// </summary>
    public uint? Uid
    {
        get => ServerUid;
        set => ServerUid = value;
    }

    /// <summary>
    /// Gets the email body (prefers HTML over text).
    /// </summary>
    public string? Body
    {
        get => !string.IsNullOrEmpty(HtmlBody) ? HtmlBody : TextBody;
        set
        {
            if (value?.Contains("<html") == true || value?.Contains("<body") == true)
            {
                HtmlBody = value;
                if (string.IsNullOrEmpty(TextBody))
                    TextBody = value; // Simple fallback
            }
            else
            {
                TextBody = value;
            }
        }
    }

    /// <summary>
    /// Alias for To property to match service interface expectations.
    /// </summary>
    public string ToAddress
    {
        get => To;
        set => To = value;
    }

    /// <summary>
    /// Alias for From property to match service interface expectations.
    /// </summary>
    public string FromAddress
    {
        get => From;
        set => From = value;
    }

    /// <summary>
    /// Alias for Cc property to match service interface expectations.
    /// </summary>
    public string? CcAddress
    {
        get => Cc;
        set => Cc = value;
    }

    /// <summary>
    /// Alias for Bcc property to match service interface expectations.
    /// </summary>
    public string? BccAddress
    {
        get => Bcc;
        set => Bcc = value;
    }

    /// <summary>
    /// Indicates if the email body contains HTML content.
    /// </summary>
    public bool IsHtml => !string.IsNullOrEmpty(HtmlBody);

    /// <summary>
    /// Indicates if this is a draft message.
    /// </summary>
    public bool IsDraft => Folder.Equals("DRAFTS", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Folder ID for compatibility (derived from folder name).
    /// </summary>
    public int? FolderId => Folder switch
    {
        "INBOX" => 1,
        "SENT" => 2,
        "DRAFTS" => 3,
        "TRASH" => 4,
        "SPAM" => 5,
        _ => null
    };
}