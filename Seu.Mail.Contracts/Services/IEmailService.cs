using Seu.Mail.Core.Models;
using Seu.Mail.Core.Enums;

namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Service contract for email operations including IMAP, SMTP, and message management
/// </summary>
public interface IEmailService
{
    #region Email Retrieval

    /// <summary>
    /// Retrieves emails from a specific folder for the given account
    /// </summary>
    /// <param name="account">Email account to retrieve from</param>
    /// <param name="folderName">Name of the folder (INBOX, SENT, etc.)</param>
    /// <param name="count">Maximum number of emails to retrieve</param>
    /// <param name="offset">Number of emails to skip for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of email messages</returns>
    Task<List<EmailMessage>> GetEmailsAsync(
        EmailAccount account,
        string folderName,
        int count = 50,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific email by its ID
    /// </summary>
    /// <param name="emailId">Unique identifier of the email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email message or null if not found</returns>
    Task<EmailMessage?> GetEmailByIdAsync(int emailId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific email by its server UID and account
    /// </summary>
    /// <param name="account">Email account</param>
    /// <param name="folderName">Folder name</param>
    /// <param name="uid">Server UID of the message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email message or null if not found</returns>
    Task<EmailMessage?> GetEmailByUidAsync(
        EmailAccount account,
        string folderName,
        uint uid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of emails in a specific folder
    /// </summary>
    /// <param name="account">Email account</param>
    /// <param name="folderName">Folder name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total email count</returns>
    Task<int> GetEmailCountAsync(
        EmailAccount account,
        string folderName,
        CancellationToken cancellationToken = default);

    #endregion

    #region Email Sending

    /// <summary>
    /// Sends an email message
    /// </summary>
    /// <param name="account">Sender's email account</param>
    /// <param name="to">Recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body content</param>
    /// <param name="isHtml">Whether the body is HTML formatted</param>
    /// <param name="cc">Carbon copy recipients (optional)</param>
    /// <param name="bcc">Blind carbon copy recipients (optional)</param>
    /// <param name="attachments">File attachments (optional)</param>
    /// <param name="priority">Email priority level</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    Task<bool> SendEmailAsync(
        EmailAccount account,
        string to,
        string subject,
        string body,
        bool isHtml = true,
        string? cc = null,
        string? bcc = null,
        List<EmailAttachment>? attachments = null,
        EmailPriority priority = EmailPriority.Normal,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a complete email message object
    /// </summary>
    /// <param name="account">Sender's email account</param>
    /// <param name="message">Complete email message to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    Task<bool> SendEmailAsync(
        EmailAccount account,
        EmailMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves an email as draft
    /// </summary>
    /// <param name="account">Email account</param>
    /// <param name="message">Email message to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Saved email message with ID</returns>
    Task<EmailMessage> SaveDraftAsync(
        EmailAccount account,
        EmailMessage message,
        CancellationToken cancellationToken = default);

    #endregion

    #region Email Actions

    /// <summary>
    /// Marks an email as read
    /// </summary>
    /// <param name="emailId">Email ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> MarkAsReadAsync(int emailId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an email as unread
    /// </summary>
    /// <param name="emailId">Email ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> MarkAsUnreadAsync(int emailId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an email as important/starred
    /// </summary>
    /// <param name="emailId">Email ID</param>
    /// <param name="isImportant">Whether to mark as important</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> SetImportantAsync(int emailId, bool isImportant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an email (moves to trash)
    /// </summary>
    /// <param name="emailId">Email ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteEmailAsync(int emailId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes an email (expunge)
    /// </summary>
    /// <param name="emailId">Email ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> PermanentlyDeleteEmailAsync(int emailId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores an email from trash
    /// </summary>
    /// <param name="emailId">Email ID</param>
    /// <param name="targetFolder">Target folder to restore to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> RestoreEmailAsync(int emailId, string targetFolder = "INBOX", CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves an email to a different folder
    /// </summary>
    /// <param name="emailId">Email ID</param>
    /// <param name="targetFolder">Target folder name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> MoveEmailAsync(int emailId, string targetFolder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives an email
    /// </summary>
    /// <param name="emailId">Email ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> ArchiveEmailAsync(int emailId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an email as spam
    /// </summary>
    /// <param name="emailId">Email ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> MarkAsSpamAsync(int emailId, CancellationToken cancellationToken = default);

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Performs bulk operations on multiple emails
    /// </summary>
    /// <param name="emailIds">List of email IDs</param>
    /// <param name="operation">Operation to perform</param>
    /// <param name="targetFolder">Target folder for move operations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of emails successfully processed</returns>
    Task<int> BulkOperationAsync(
        List<int> emailIds,
        BulkEmailOperation operation,
        string? targetFolder = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Synchronization

    /// <summary>
    /// Synchronizes emails for a specific account
    /// </summary>
    /// <param name="account">Email account to sync</param>
    /// <param name="folderName">Specific folder to sync (null for all folders)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync status and statistics</returns>
    Task<EmailSyncResult> SyncEmailsAsync(
        EmailAccount account,
        string? folderName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the last synchronization status for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Last sync result</returns>
    Task<EmailSyncResult?> GetLastSyncStatusAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts real-time synchronization using IMAP IDLE if supported
    /// </summary>
    /// <param name="account">Email account</param>
    /// <param name="folderName">Folder to monitor</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the idle connection</returns>
    Task StartIdleSyncAsync(
        EmailAccount account,
        string folderName = "INBOX",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops real-time synchronization
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StopIdleSyncAsync(int accountId, CancellationToken cancellationToken = default);

    #endregion

    #region Search

    /// <summary>
    /// Searches emails based on criteria
    /// </summary>
    /// <param name="searchCriteria">Search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching emails</returns>
    Task<List<EmailMessage>> SearchEmailsAsync(
        EmailSearchCriteria searchCriteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a simple text search across all email content
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="accountId">Account ID to search in (null for all accounts)</param>
    /// <param name="folderName">Folder to search in (null for all folders)</param>
    /// <param name="maxResults">Maximum number of results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching emails</returns>
    Task<List<EmailMessage>> SearchEmailsAsync(
        string query,
        int? accountId = null,
        string? folderName = null,
        int maxResults = 100,
        CancellationToken cancellationToken = default);

    #endregion

    #region Attachments

    /// <summary>
    /// Downloads an attachment from the server
    /// </summary>
    /// <param name="attachmentId">Attachment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Attachment with content</returns>
    Task<EmailAttachment?> DownloadAttachmentAsync(int attachmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves an attachment to the local file system
    /// </summary>
    /// <param name="attachmentId">Attachment ID</param>
    /// <param name="filePath">Local file path to save to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> SaveAttachmentAsync(int attachmentId, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all attachments for a specific email
    /// </summary>
    /// <param name="emailId">Email ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of attachments</returns>
    Task<List<EmailAttachment>> GetAttachmentsAsync(int emailId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to the email server for the specified account
    /// </summary>
    /// <param name="account">The email account to test</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Connection test result</returns>
    Task<ConnectionTestResult> TestConnectionAsync(EmailAccount account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes folders and tags for the specified account
    /// </summary>
    /// <param name="accountId">The account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the sync operation</returns>
    Task SyncFoldersAndTagsAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all folders for the specified email account
    /// </summary>
    /// <param name="account">The email account</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of folder names</returns>
    Task<List<string>> GetFoldersAsync(EmailAccount account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an email as read on the server
    /// </summary>
    /// <param name="account">The email account</param>
    /// <param name="messageUid">The message UID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task MarkAsReadOnServerAsync(EmailAccount account, string messageUid, CancellationToken cancellationToken = default);

    #endregion

    #region Events

    /// <summary>
    /// Event raised when a new email is received
    /// </summary>
    event EventHandler<EmailReceivedEventArgs>? EmailReceived;

    /// <summary>
    /// Event raised when an email is sent
    /// </summary>
    event EventHandler<EmailSentEventArgs>? EmailSent;

    /// <summary>
    /// Event raised when sync status changes
    /// </summary>
    event EventHandler<SyncStatusChangedEventArgs>? SyncStatusChanged;

    /// <summary>
    /// Event raised when an error occurs
    /// </summary>
    event EventHandler<EmailErrorEventArgs>? ErrorOccurred;

    #endregion
}

/// <summary>
/// Bulk email operations
/// </summary>
/// <summary>
/// Represents bulk operations that can be performed on multiple emails.
/// </summary>
public enum BulkEmailOperation
{
    /// <summary>
    /// Mark emails as read.
    /// </summary>
    MarkAsRead,
    /// <summary>
    /// Mark emails as unread.
    /// </summary>
    MarkAsUnread,
    /// <summary>
    /// Delete emails.
    /// </summary>
    Delete,
    /// <summary>
    /// Archive emails.
    /// </summary>
    Archive,
    /// <summary>
    /// Mark emails as spam.
    /// </summary>
    MarkAsSpam,
    /// <summary>
    /// Move emails to another folder.
    /// </summary>
    Move,
    /// <summary>
    /// Set emails as important/starred.
    /// </summary>
    SetImportant,
    /// <summary>
    /// Remove important/starred status from emails.
    /// </summary>
    RemoveImportant
}

/// <summary>
/// Email synchronization result
/// </summary>
/// <summary>
/// Represents the result of an email synchronization operation.
/// </summary>
public class EmailSyncResult
{
    /// <summary>
    /// The status of the email synchronization.
    /// </summary>
    public EmailSyncStatus Status { get; set; }
    /// <summary>
    /// The start time of the synchronization.
    /// </summary>
    public DateTime StartTime { get; set; }
    /// <summary>
    /// The end time of the synchronization.
    /// </summary>
    public DateTime EndTime { get; set; }
    /// <summary>
    /// The number of new emails found during synchronization.
    /// </summary>
    public int NewEmailsCount { get; set; }
    /// <summary>
    /// The number of updated emails during synchronization.
    /// </summary>
    public int UpdatedEmailsCount { get; set; }
    /// <summary>
    /// The number of deleted emails during synchronization.
    /// </summary>
    public int DeletedEmailsCount { get; set; }
    /// <summary>
    /// The number of errors encountered during synchronization.
    /// </summary>
    public int ErrorsCount { get; set; }
    /// <summary>
    /// The error message, if any, encountered during synchronization.
    /// </summary>
    public string? ErrorMessage { get; set; }
    /// <summary>
    /// A dictionary of folder names and their respective email counts.
    /// </summary>
    public Dictionary<string, int> FolderCounts { get; set; } = new();
}

/// <summary>
/// Email search criteria
/// </summary>
/// <summary>
/// Represents criteria for searching emails.
/// </summary>
public class EmailSearchCriteria
{
    /// <summary>
    /// The search query string.
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// The sender's email address to filter by.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// The recipient's email address to filter by.
    /// </summary>
    public string? To { get; set; }

    /// <summary>
    /// The subject to filter by.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// The start date for filtering emails.
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// The end date for filtering emails.
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Whether to filter emails that have attachments.
    /// </summary>
    public bool? HasAttachments { get; set; }

    /// <summary>
    /// Whether to filter emails that are read.
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    /// Whether to filter emails that are marked as important.
    /// </summary>
    public bool? IsImportant { get; set; }

    /// <summary>
    /// The folder name to filter emails by.
    /// </summary>
    public string? FolderName { get; set; }

    /// <summary>
    /// The account ID to filter emails by.
    /// </summary>
    public int? AccountId { get; set; }

    /// <summary>
    /// The scope of the search (e.g., current folder, all folders).
    /// </summary>
    public SearchScope Scope { get; set; } = SearchScope.CurrentFolder;

    /// <summary>
    /// The maximum number of results to return.
    /// </summary>
    public int MaxResults { get; set; } = 100;

    /// <summary>
    /// The offset for pagination.
    /// </summary>
    public int Offset { get; set; } = 0;
}

/// <summary>
/// Event arguments for email received event
/// </summary>
/// <summary>
/// Provides data for the event raised when an email is received.
/// </summary>
public class EmailReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the received email message.
    /// </summary>
    public EmailMessage Email { get; }

    /// <summary>
    /// Gets the account associated with the received email.
    /// </summary>
    public EmailAccount Account { get; }

    /// <summary>
    /// Gets the timestamp when the email was received.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="email">The received email message.</param>
    /// <param name="account">The account associated with the received email.</param>
    public EmailReceivedEventArgs(EmailMessage email, EmailAccount account)
    {
        Email = email;
        Account = account;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Event arguments for email sent event
/// </summary>
/// <summary>
/// Provides data for the event raised when an email is sent.
/// </summary>
public class EmailSentEventArgs : EventArgs
{
    /// <summary>
    /// Gets the sent email message.
    /// </summary>
    public EmailMessage Email { get; }

    /// <summary>
    /// Gets the account associated with the sent email.
    /// </summary>
    public EmailAccount Account { get; }

    /// <summary>
    /// Gets the timestamp when the email was sent.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSentEventArgs"/> class.
    /// </summary>
    /// <param name="email">The sent email message.</param>
    /// <param name="account">The account associated with the sent email.</param>
    public EmailSentEventArgs(EmailMessage email, EmailAccount account)
    {
        Email = email;
        Account = account;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Event arguments for sync status changed event
/// </summary>
/// <summary>
/// Provides data for the event raised when the email sync status changes.
/// </summary>
public class SyncStatusChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the account ID associated with the sync status change.
    /// </summary>
    public int AccountId { get; }

    /// <summary>
    /// Gets the new sync status.
    /// </summary>
    public EmailSyncStatus Status { get; }

    /// <summary>
    /// Gets the message associated with the sync status change.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Gets the timestamp when the sync status changed.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncStatusChangedEventArgs"/> class.
    /// </summary>
    /// <param name="accountId">The account ID associated with the sync status change.</param>
    /// <param name="status">The new sync status.</param>
    /// <param name="message">The message associated with the sync status change.</param>
    public SyncStatusChangedEventArgs(int accountId, EmailSyncStatus status, string? message = null)
    {
        AccountId = accountId;
        Status = status;
        Message = message;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Event arguments for email error event
/// </summary>
/// <summary>
/// Provides data for the event raised when an email error occurs.
/// </summary>
public class EmailErrorEventArgs : EventArgs
{
    /// <summary>
    /// Gets the account ID associated with the error, if available.
    /// </summary>
    public int? AccountId { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Gets the exception associated with the error, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the timestamp when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailErrorEventArgs"/> class.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="exception">The exception associated with the error, if any.</param>
    /// <param name="accountId">The account ID associated with the error, if available.</param>
    public EmailErrorEventArgs(string errorMessage, Exception? exception = null, int? accountId = null)
    {
        ErrorMessage = errorMessage;
        Exception = exception;
        AccountId = accountId;
        Timestamp = DateTime.UtcNow;
    }
}
