namespace Seu.Mail.Core.Enums;

/// <summary>
/// Represents the different types of email folders
/// </summary>
public enum EmailFolderType
{
    /// <summary>
    /// Inbox folder for incoming messages
    /// </summary>
    Inbox = 0,

    /// <summary>
    /// Sent folder for outgoing messages
    /// </summary>
    Sent = 1,

    /// <summary>
    /// Drafts folder for unsent messages
    /// </summary>
    Drafts = 2,

    /// <summary>
    /// Trash folder for deleted messages
    /// </summary>
    Trash = 3,

    /// <summary>
    /// Archive folder for stored messages
    /// </summary>
    Archive = 4,

    /// <summary>
    /// Spam/Junk folder
    /// </summary>
    Spam = 5,

    /// <summary>
    /// Outbox folder for messages waiting to be sent
    /// </summary>
    Outbox = 6,

    /// <summary>
    /// Custom user-created folder
    /// </summary>
    Custom = 7,

    /// <summary>
    /// All Mail folder (Gmail-specific)
    /// </summary>
    AllMail = 8,

    /// <summary>
    /// Important folder (Gmail-specific)
    /// </summary>
    Important = 9
}

/// <summary>
/// Email message priorities
/// </summary>
public enum EmailPriority
{
    /// <summary>
    /// Low priority
    /// </summary>
    Low = 1,

    /// <summary>
    /// Normal priority (default)
    /// </summary>
    Normal = 3,

    /// <summary>
    /// High priority
    /// </summary>
    High = 5
}

/// <summary>
/// Email synchronization status
/// </summary>
public enum EmailSyncStatus
{
    /// <summary>
    /// Not synchronized
    /// </summary>
    None = 0,

    /// <summary>
    /// Synchronization in progress
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Successfully synchronized
    /// </summary>
    Success = 2,

    /// <summary>
    /// Synchronization failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Synchronization cancelled
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Partial synchronization completed
    /// </summary>
    Partial = 5
}

/// <summary>
/// Email account synchronization modes
/// </summary>
public enum SyncMode
{
    /// <summary>
    /// Manual synchronization only
    /// </summary>
    Manual = 0,

    /// <summary>
    /// Automatic synchronization at intervals
    /// </summary>
    Automatic = 1,

    /// <summary>
    /// Real-time synchronization using IDLE
    /// </summary>
    RealTime = 2,

    /// <summary>
    /// Push notifications when supported
    /// </summary>
    Push = 3
}

/// <summary>
/// Email provider types for quick configuration
/// </summary>
public enum EmailProvider
{
    /// <summary>
    /// Custom configuration
    /// </summary>
    Custom = 0,

    /// <summary>
    /// Google Gmail
    /// </summary>
    Gmail = 1,

    /// <summary>
    /// Microsoft Outlook/Hotmail
    /// </summary>
    Outlook = 2,

    /// <summary>
    /// Yahoo Mail
    /// </summary>
    Yahoo = 3,

    /// <summary>
    /// Apple iCloud Mail
    /// </summary>
    iCloud = 4,

    /// <summary>
    /// ProtonMail
    /// </summary>
    ProtonMail = 5,

    /// <summary>
    /// Fastmail
    /// </summary>
    Fastmail = 6,

    /// <summary>
    /// Zoho Mail
    /// </summary>
    Zoho = 7,

    /// <summary>
    /// AOL Mail
    /// </summary>
    AOL = 8,

    /// <summary>
    /// Generic IMAP/SMTP
    /// </summary>
    IMAP = 9
}

/// <summary>
/// Email authentication methods
/// </summary>
public enum EmailAuthMethod
{
    /// <summary>
    /// Plain text authentication
    /// </summary>
    Plain = 0,

    /// <summary>
    /// LOGIN authentication
    /// </summary>
    Login = 1,

    /// <summary>
    /// CRAM-MD5 authentication
    /// </summary>
    CramMd5 = 2,

    /// <summary>
    /// NTLM authentication
    /// </summary>
    Ntlm = 3,

    /// <summary>
    /// OAuth 2.0 authentication
    /// </summary>
    OAuth2 = 4,

    /// <summary>
    /// XOAUTH2 authentication
    /// </summary>
    XOAuth2 = 5
}

/// <summary>
/// SSL/TLS connection security types
/// </summary>
public enum ConnectionSecurity
{
    /// <summary>
    /// No encryption
    /// </summary>
    None = 0,

    /// <summary>
    /// SSL/TLS encryption
    /// </summary>
    SSL = 1,

    /// <summary>
    /// STARTTLS encryption
    /// </summary>
    StartTls = 2,

    /// <summary>
    /// Auto-detect best security
    /// </summary>
    Auto = 3
}

/// <summary>
/// Email content encoding types
/// </summary>
public enum EmailEncoding
{
    /// <summary>
    /// 7-bit ASCII encoding
    /// </summary>
    SevenBit = 0,

    /// <summary>
    /// 8-bit encoding
    /// </summary>
    EightBit = 1,

    /// <summary>
    /// Base64 encoding
    /// </summary>
    Base64 = 2,

    /// <summary>
    /// Quoted-printable encoding
    /// </summary>
    QuotedPrintable = 3,

    /// <summary>
    /// Binary encoding
    /// </summary>
    Binary = 4
}

/// <summary>
/// Email message formats
/// </summary>
public enum EmailFormat
{
    /// <summary>
    /// Plain text only
    /// </summary>
    PlainText = 0,

    /// <summary>
    /// HTML format
    /// </summary>
    Html = 1,

    /// <summary>
    /// Both plain text and HTML (multipart)
    /// </summary>
    Both = 2
}

/// <summary>
/// Email notification types
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// No notification
    /// </summary>
    None = 0,

    /// <summary>
    /// Desktop notification
    /// </summary>
    Desktop = 1,

    /// <summary>
    /// Sound notification
    /// </summary>
    Sound = 2,

    /// <summary>
    /// Both desktop and sound
    /// </summary>
    Both = 3,

    /// <summary>
    /// Email notification
    /// </summary>
    Email = 4,

    /// <summary>
    /// Mobile push notification
    /// </summary>
    Push = 5
}

/// <summary>
/// Email thread grouping strategies
/// </summary>
public enum ThreadingStrategy
{
    /// <summary>
    /// No threading
    /// </summary>
    None = 0,

    /// <summary>
    /// Thread by subject
    /// </summary>
    Subject = 1,

    /// <summary>
    /// Thread by references header
    /// </summary>
    References = 2,

    /// <summary>
    /// Thread by in-reply-to header
    /// </summary>
    InReplyTo = 3,

    /// <summary>
    /// Smart threading using multiple methods
    /// </summary>
    Smart = 4
}

/// <summary>
/// Email search scopes
/// </summary>
public enum SearchScope
{
    /// <summary>
    /// Search current folder only
    /// </summary>
    CurrentFolder = 0,

    /// <summary>
    /// Search all folders
    /// </summary>
    AllFolders = 1,

    /// <summary>
    /// Search specific folders
    /// </summary>
    SpecificFolders = 2,

    /// <summary>
    /// Search current account only
    /// </summary>
    CurrentAccount = 3,

    /// <summary>
    /// Search all accounts
    /// </summary>
    AllAccounts = 4
}

/// <summary>
/// Email attachment security status
/// </summary>
public enum AttachmentSecurity
{
    /// <summary>
    /// Not scanned
    /// </summary>
    NotScanned = 0,

    /// <summary>
    /// Safe to open
    /// </summary>
    Safe = 1,

    /// <summary>
    /// Potentially dangerous
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Blocked due to security risk
    /// </summary>
    Blocked = 3,

    /// <summary>
    /// Encrypted attachment
    /// </summary>
    Encrypted = 4
}