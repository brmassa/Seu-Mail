using System.ComponentModel.DataAnnotations;

namespace Seu.Mail.Core.Models;

/// <summary>
/// Represents an email account configuration with connection settings
/// </summary>
/// <summary>
/// Represents an email account configuration with connection settings.
/// </summary>
public class EmailAccount
{
    /// <summary>
    /// Unique identifier for the email account.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Email address for this account.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(320)] // RFC 5321 maximum email address length
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the account owner.
    /// </summary>
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Encrypted password for authentication.
    /// </summary>
    [Required]
    [StringLength(500)] // Allow space for encryption
    public string EncryptedPassword { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server hostname or IP address.
    /// </summary>
    [Required]
    [StringLength(255)]
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port number.
    /// </summary>
    [Range(1, 65535)]
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// IMAP server hostname or IP address.
    /// </summary>
    [Required]
    [StringLength(255)]
    public string ImapServer { get; set; } = string.Empty;

    /// <summary>
    /// IMAP server port number.
    /// </summary>
    [Range(1, 65535)]
    public int ImapPort { get; set; } = 993;

    /// <summary>
    /// Whether to use SSL/TLS encryption for SMTP.
    /// </summary>
    public bool SmtpUseSsl { get; set; } = true;

    /// <summary>
    /// Whether to use SSL/TLS encryption for IMAP.
    /// </summary>
    public bool ImapUseSsl { get; set; } = true;

    /// <summary>
    /// SMTP authentication method.
    /// </summary>
    [StringLength(50)]
    public string SmtpAuthMethod { get; set; } = "LOGIN";

    /// <summary>
    /// IMAP authentication method.
    /// </summary>
    [StringLength(50)]
    public string ImapAuthMethod { get; set; } = "LOGIN";

    /// <summary>
    /// Whether this is the default account for sending emails.
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Whether the account is currently enabled for sync.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Email provider type (Gmail, Outlook, Yahoo, Custom, etc.).
    /// </summary>
    [StringLength(50)]
    public string? ProviderType { get; set; }

    /// <summary>
    /// Email signature for outgoing messages.
    /// </summary>
    public string? Signature { get; set; }

    /// <summary>
    /// Automatic sync interval in minutes.
    /// </summary>
    [Range(1, 1440)] // 1 minute to 24 hours
    public int SyncIntervalMinutes { get; set; } = 15;

    /// <summary>
    /// Maximum number of emails to sync per session.
    /// </summary>
    [Range(1, 10000)]
    public int MaxEmailsPerSync { get; set; } = 100;

    /// <summary>
    /// Number of days of email history to sync (0 = all).
    /// </summary>
    [Range(0, 3650)] // 0 to 10 years
    public int SyncDaysHistory { get; set; } = 0;

    /// <summary>
    /// Whether to automatically mark emails as read when opened.
    /// </summary>
    public bool AutoMarkAsRead { get; set; } = true;

    /// <summary>
    /// Whether to enable desktop notifications for new emails.
    /// </summary>
    public bool EnableNotifications { get; set; } = true;

    /// <summary>
    /// Connection timeout in seconds.
    /// </summary>
    [Range(5, 300)] // 5 seconds to 5 minutes
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Date when the account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the account was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time of last successful sync.
    /// </summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>
    /// Date and time of last successful connection test.
    /// </summary>
    public DateTime? LastConnectionTest { get; set; }

    /// <summary>
    /// Last error message encountered during sync.
    /// </summary>
    [StringLength(1000)]
    public string? LastError { get; set; }

    /// <summary>
    /// Number of consecutive sync failures.
    /// </summary>
    public int ConsecutiveFailures { get; set; } = 0;

    /// <summary>
    /// Total number of emails in the account.
    /// </summary>
    public long TotalEmailCount { get; set; } = 0;

    /// <summary>
    /// Total size of all emails in bytes.
    /// </summary>
    public long TotalEmailSize { get; set; } = 0;

    /// <summary>
    /// Whether the account supports IDLE command for real-time sync.
    /// </summary>
    public bool SupportsIdle { get; set; } = false;

    /// <summary>
    /// Account-specific settings as JSON.
    /// </summary>
    public string? Settings { get; set; }

    /// <summary>
    /// Email messages associated with this account.
    /// </summary>
    public virtual ICollection<EmailMessage> EmailMessages { get; set; } = new List<EmailMessage>();

    /// <summary>
    /// Folders associated with this account.
    /// </summary>
    public virtual ICollection<EmailFolder> Folders { get; set; } = new List<EmailFolder>();

    /// <summary>
    /// Alias for Email property to match service interface expectations.
    /// </summary>
    public string EmailAddress
    {
        get => Email;
        set => Email = value;
    }

    /// <summary>
    /// Gets or sets the password (encrypted).
    /// </summary>
    public string Password
    {
        get => EncryptedPassword;
        set => EncryptedPassword = value;
    }

    /// <summary>
    /// Compatibility property for SSL usage (combines SMTP and IMAP SSL settings).
    /// </summary>
    public bool UseSsl
    {
        get => SmtpUseSsl && ImapUseSsl;
        set
        {
            SmtpUseSsl = value;
            ImapUseSsl = value;
        }
    }

    /// <summary>
    /// Gets the display name or falls back to email address.
    /// </summary>
    /// <returns>The display name or email address.</returns>
    public string GetDisplayName()
    {
        return string.IsNullOrWhiteSpace(DisplayName) ? Email : DisplayName;
    }

    /// <summary>
    /// Gets the username from the email address.
    /// </summary>
    /// <returns>The username part of the email address.</returns>
    public string GetUsername()
    {
        return Email.Split('@')[0];
    }

    /// <summary>
    /// Gets the domain from the email address.
    /// </summary>
    /// <returns>The domain part of the email address.</returns>
    public string GetDomain()
    {
        return Email.Contains('@') ? Email.Split('@')[1] : string.Empty;
    }

    /// <summary>
    /// Determines if the account needs attention (has errors or hasn't synced recently).
    /// </summary>
    public bool NeedsAttention
    {
        get
        {
            if (!IsEnabled) return false;
            if (ConsecutiveFailures > 3) return true;
            if (!LastSyncAt.HasValue) return true;

            var syncThreshold = TimeSpan.FromMinutes(SyncIntervalMinutes * 3);
            return DateTime.UtcNow - LastSyncAt.Value > syncThreshold;
        }
    }

    /// <summary>
    /// Gets the sync status as a human-readable string.
    /// </summary>
    public string SyncStatus
    {
        get
        {
            if (!IsEnabled) return "Disabled";
            if (ConsecutiveFailures > 0) return $"Error ({ConsecutiveFailures} failures)";
            if (!LastSyncAt.HasValue) return "Never synced";

            var timeSinceSync = DateTime.UtcNow - LastSyncAt.Value;
            if (timeSinceSync.TotalMinutes < 1) return "Just now";
            if (timeSinceSync.TotalMinutes < 60) return $"{(int)timeSinceSync.TotalMinutes}m ago";
            if (timeSinceSync.TotalHours < 24) return $"{(int)timeSinceSync.TotalHours}h ago";
            return $"{(int)timeSinceSync.TotalDays}d ago";
        }
    }

    /// <summary>
    /// Validates the account configuration.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(EncryptedPassword) &&
               !string.IsNullOrWhiteSpace(SmtpServer) &&
               !string.IsNullOrWhiteSpace(ImapServer) &&
               SmtpPort > 0 && SmtpPort <= 65535 &&
               ImapPort > 0 && ImapPort <= 65535;
    }
}