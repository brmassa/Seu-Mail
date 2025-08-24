using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using Seu.Mail.Data.Context;
using Seu.Mail.Core.Models;
using Seu.Mail.Core.Enums;
using Seu.Mail.Contracts.Services;

namespace Seu.Mail.Services;

/// <summary>
/// Provides email management services including sending, receiving, syncing, and searching emails.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailDbContext _context;
    private readonly ILogger<EmailService> _logger;
    private readonly IEncryptionService _encryptionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="context">The database context for email data.</param>
    /// <param name="logger">Logger for email events and errors.</param>
    /// <param name="encryptionService">Encryption service for secure data handling.</param>
    public EmailService(EmailDbContext context, ILogger<EmailService> logger, IEncryptionService encryptionService)
    {
        _context = context;
        _logger = logger;
        _encryptionService = encryptionService;
    }

    // Events
    /// <summary>
    /// Event raised when a new email is received and synchronized.
    /// </summary>
#pragma warning disable CS0067 // Event is never used but is part of the public API
    public event EventHandler<EmailReceivedEventArgs>? EmailReceived;
#pragma warning restore CS0067

    /// <summary>
    /// Event raised when an email is successfully sent.
    /// </summary>
    public event EventHandler<EmailSentEventArgs>? EmailSent;

    /// <summary>
    /// Event raised when the email synchronization status changes.
    /// </summary>
    public event EventHandler<SyncStatusChangedEventArgs>? SyncStatusChanged;

    /// <summary>
    /// Event raised when an error occurs during email operations.
    /// </summary>
    public event EventHandler<EmailErrorEventArgs>? ErrorOccurred;

    // Email retrieval methods
    /// <summary>
    /// Gets a list of emails for the specified account and folder.
    /// </summary>
    /// <param name="account">The email account.</param>
    /// <param name="folderName">The folder name.</param>
    /// <param name="count">Maximum number of emails to retrieve.</param>
    /// <param name="offset">Number of emails to skip for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of email messages.</returns>
    public async Task<List<EmailMessage>> GetEmailsAsync(EmailAccount account, string folderName, int count = 50, int offset = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {Count} emails from folder {Folder} (offset: {Offset}) for account {Email}",
                count, folderName, offset, account.EmailAddress);

            var emails = await _context.EmailMessages
                .Where(e => e.AccountId == account.Id && e.Folder == folderName && !e.IsDeleted)
                .OrderByDescending(e => e.DateReceived)
                .Skip(offset)
                .Take(count)
                .Include(e => e.Attachments)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {EmailCount} emails from database for account {Email}",
                emails.Count, account.EmailAddress);

            return emails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emails for account {Email}", account.EmailAddress);
            return new List<EmailMessage>();
        }
    }

    /// <summary>
    /// Gets an email by its unique identifier.
    /// </summary>
    /// <param name="emailId">The email ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The email message if found; otherwise, null.</returns>
    public async Task<EmailMessage?> GetEmailByIdAsync(int emailId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.EmailMessages
                .Include(e => e.Attachments)
                .FirstOrDefaultAsync(e => e.Id == emailId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email by ID {EmailId}", emailId);
            return null;
        }
    }

    /// <summary>
    /// Gets an email by its server UID.
    /// </summary>
    /// <param name="account">The email account.</param>
    /// <param name="folder">The folder name.</param>
    /// <param name="uid">The server UID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The email message if found; otherwise, null.</returns>
    public async Task<EmailMessage?> GetEmailByUidAsync(EmailAccount account, string folder, uint uid, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.EmailMessages
                .Include(e => e.Attachments)
                .FirstOrDefaultAsync(e => e.AccountId == account.Id && e.Folder == folder && e.Uid == uid, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email by UID {Uid}", uid);
            return null;
        }
    }

    /// <summary>
    /// Gets the count of emails in a folder for the specified account.
    /// </summary>
    /// <param name="account">The email account.</param>
    /// <param name="folder">The folder name (default: INBOX).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of emails in the folder.</returns>
    public async Task<int> GetEmailCountAsync(EmailAccount account, string folder = "INBOX", CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.EmailMessages
                .CountAsync(e => e.AccountId == account.Id && e.Folder == folder && !e.IsDeleted, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email count for folder {Folder}", folder);
            return 0;
        }
    }

    // Email sending methods
    /// <summary>
    /// Sends an email using the specified account and parameters.
    /// </summary>
    /// <param name="account">The email account to send from.</param>
    /// <param name="to">Comma-separated list of recipient email addresses.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="body">The email body content.</param>
    /// <param name="isHtml">Whether the body content is HTML formatted.</param>
    /// <param name="cc">Comma-separated list of CC recipients (optional).</param>
    /// <param name="bcc">Comma-separated list of BCC recipients (optional).</param>
    /// <param name="attachments">List of email attachments (optional).</param>
    /// <param name="priority">The email priority level.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if sent successfully.</returns>
    public async Task<bool> SendEmailAsync(EmailAccount account, string to, string subject, string body, bool isHtml = false, string? cc = null, string? bcc = null, List<EmailAttachment>? attachments = null, EmailPriority priority = EmailPriority.Normal, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(account.DisplayName, account.EmailAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            if (!string.IsNullOrEmpty(cc))
            {
                foreach (var ccEmail in cc.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    message.Cc.Add(MailboxAddress.Parse(ccEmail.Trim()));
                }
            }

            if (!string.IsNullOrEmpty(bcc))
            {
                foreach (var bccEmail in bcc.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    message.Bcc.Add(MailboxAddress.Parse(bccEmail.Trim()));
                }
            }

            var bodyBuilder = new BodyBuilder();
            if (isHtml)
            {
                bodyBuilder.HtmlBody = body;
            }
            else
            {
                bodyBuilder.TextBody = body;
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(account.SmtpServer, account.SmtpPort, SecureSocketOptions.SslOnConnect, cancellationToken);

            _logger.LogWarning("SMTP: Encrypted password length: {Length}", account.Password?.Length ?? 0);
            var decryptedPassword = _encryptionService.DecryptString(account.EncryptedPassword);
            _logger.LogWarning("SMTP: Decrypted password length: {Length}", decryptedPassword?.Length ?? 0);

            await client.AuthenticateAsync(account.EmailAddress, decryptedPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            EmailSent?.Invoke(this, new EmailSentEventArgs(new EmailMessage { Subject = subject, ToAddress = to }, account));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email from {Email}", account.EmailAddress);
            ErrorOccurred?.Invoke(this, new EmailErrorEventArgs(ex.Message, ex, account.Id));
            return false;
        }
    }

    /// <summary>
    /// Sends an email using the specified account and email message object.
    /// </summary>
    /// <param name="account">The email account to send from.</param>
    /// <param name="email">The email message object containing all email details.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if sent successfully.</returns>
    public async Task<bool> SendEmailAsync(EmailAccount account, EmailMessage email, CancellationToken cancellationToken = default)
    {
        return await SendEmailAsync(account, email.ToAddress, email.Subject, email.Body ?? "", email.IsHtml,
        email.CcAddress, email.BccAddress, email.Attachments?.ToList(), EmailPriority.Normal, cancellationToken);
    }

    /// <summary>
    /// Saves an email as a draft in the specified account.
    /// </summary>
    /// <param name="account">The email account to save the draft to.</param>
    /// <param name="email">The email message to save as draft.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the saved draft email.</returns>
    public async Task<EmailMessage> SaveDraftAsync(EmailAccount account, EmailMessage email, CancellationToken cancellationToken = default)
    {
        try
        {
            email.AccountId = account.Id;
            email.Folder = "DRAFTS";
            email.DateReceived = DateTime.UtcNow;

            _context.EmailMessages.Add(email);
            await _context.SaveChangesAsync(cancellationToken);
            return email;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving draft");
            throw;
        }
    }

    // Email management methods
    /// <summary>
    /// Marks an email as read in the database and on the server.
    /// </summary>
    /// <param name="emailId">The database ID of the email to mark as read.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if successful.</returns>
    public async Task<bool> MarkAsReadAsync(int emailId, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = await _context.EmailMessages.FindAsync(emailId);
            if (email != null)
            {
                email.IsRead = true;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking email as read {EmailId}", emailId);
            return false;
        }
    }

    /// <summary>
    /// Marks an email as unread in the database and on the server.
    /// </summary>
    /// <param name="emailId">The database ID of the email to mark as unread.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if successful.</returns>
    public async Task<bool> MarkAsUnreadAsync(int emailId, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = await _context.EmailMessages.FindAsync(emailId);
            if (email != null)
            {
                email.IsRead = false;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking email as unread {EmailId}", emailId);
            return false;
        }
    }

    /// <summary>
    /// Sets the importance flag of an email.
    /// </summary>
    /// <param name="emailId">The database ID of the email.</param>
    /// <param name="isImportant">Whether to mark the email as important.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if successful.</returns>
    public async Task<bool> SetImportantAsync(int emailId, bool isImportant, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = await _context.EmailMessages.FindAsync(emailId);
            if (email != null)
            {
                email.IsImportant = isImportant;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting importance for email {EmailId}", emailId);
            return false;
        }
    }

    /// <summary>
    /// Moves an email to the trash/deleted items folder.
    /// </summary>
    /// <param name="emailId">The database ID of the email to delete.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if successful.</returns>
    public async Task<bool> DeleteEmailAsync(int emailId, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = await _context.EmailMessages.FindAsync(emailId);
            if (email != null)
            {
                email.IsDeleted = true;
                email.Folder = "TRASH";
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email {EmailId}", emailId);
            return false;
        }
    }

    /// <summary>
    /// Permanently deletes an email from the server and database.
    /// </summary>
    /// <param name="emailId">The database ID of the email to permanently delete.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if successful.</returns>
    public async Task<bool> PermanentlyDeleteEmailAsync(int emailId, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = await _context.EmailMessages.FindAsync(emailId);
            if (email != null)
            {
                _context.EmailMessages.Remove(email);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently deleting email {EmailId}", emailId);
            return false;
        }
    }

    /// <summary>
    /// Restores a deleted email from trash to a target folder.
    /// </summary>
    /// <param name="emailId">The database ID of the email to restore.</param>
    /// <param name="targetFolder">The target folder to restore the email to (default: INBOX).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if successful.</returns>
    public async Task<bool> RestoreEmailAsync(int emailId, string targetFolder = "INBOX", CancellationToken cancellationToken = default)
    {
        try
        {
            var email = await _context.EmailMessages.FindAsync(emailId);
            if (email != null)
            {
                email.IsDeleted = false;
                email.Folder = targetFolder;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring email {EmailId}", emailId);
            return false;
        }
    }

    /// <summary>
    /// Moves an email to a different folder.
    /// </summary>
    /// <param name="emailId">The database ID of the email to move.</param>
    /// <param name="targetFolder">The target folder to move the email to.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if successful.</returns>
    public async Task<bool> MoveEmailAsync(int emailId, string targetFolder, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = await _context.EmailMessages.FindAsync(emailId);
            if (email != null)
            {
                email.Folder = targetFolder;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving email {EmailId}", emailId);
            return false;
        }
    }

    /// <summary>
    /// Archives an email by moving it to the Archive folder.
    /// </summary>
    /// <param name="emailId">The database ID of the email to archive.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if successful.</returns>
    public async Task<bool> ArchiveEmailAsync(int emailId, CancellationToken cancellationToken = default)
    {
        return await MoveEmailAsync(emailId, "ARCHIVE", cancellationToken);
    }

    /// <summary>
    /// Marks an email as spam by moving it to the Spam/Junk folder.
    /// </summary>
    /// <param name="emailId">The database ID of the email to mark as spam.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if successful.</returns>
    public async Task<bool> MarkAsSpamAsync(int emailId, CancellationToken cancellationToken = default)
    {
        return await MoveEmailAsync(emailId, "JUNK", cancellationToken);
    }

    // Bulk operations
    /// <summary>
    /// Performs a bulk operation on multiple emails.
    /// </summary>
    /// <param name="emailIds">List of email database IDs to perform the operation on.</param>
    /// <param name="operation">The bulk operation to perform.</param>
    /// <param name="targetFolder">Target folder for move operations (optional).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of successfully processed emails.</returns>
    public async Task<int> BulkOperationAsync(List<int> emailIds, BulkEmailOperation operation, string? targetFolder = null, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var emailId in emailIds)
            {
                switch (operation)
                {
                    case BulkEmailOperation.MarkAsRead:
                        await MarkAsReadAsync(emailId, cancellationToken);
                        break;
                    case BulkEmailOperation.MarkAsUnread:
                        await MarkAsUnreadAsync(emailId, cancellationToken);
                        break;
                    case BulkEmailOperation.Delete:
                        await DeleteEmailAsync(emailId, cancellationToken);
                        break;
                    case BulkEmailOperation.Archive:
                        await ArchiveEmailAsync(emailId, cancellationToken);
                        break;
                    case BulkEmailOperation.MarkAsSpam:
                        await MarkAsSpamAsync(emailId, cancellationToken);
                        break;
                    case BulkEmailOperation.Move:
                        if (!string.IsNullOrEmpty(targetFolder))
                            await MoveEmailAsync(emailId, targetFolder, cancellationToken);
                        break;
                    case BulkEmailOperation.SetImportant:
                        await SetImportantAsync(emailId, true, cancellationToken);
                        break;
                    case BulkEmailOperation.RemoveImportant:
                        await SetImportantAsync(emailId, false, cancellationToken);
                        break;
                }
            }
            return emailIds.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation {Operation}", operation);
            return 0;
        }
    }

    // Sync methods
    /// <summary>
    /// Synchronizes emails from the server to the local database for the specified account.
    /// </summary>
    /// <param name="account">The email account to synchronize.</param>
    /// <param name="folder">Specific folder to synchronize (null for all folders).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the sync operation results.</returns>
    public async Task<EmailSyncResult> SyncEmailsAsync(EmailAccount account, string? folder = null, CancellationToken cancellationToken = default)
    {
        if (account == null)
        {
            _logger.LogWarning("Account is null in SyncEmailsAsync");
            return new EmailSyncResult
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                Status = EmailSyncStatus.Failed,
                ErrorMessage = "Account cannot be null"
            };
        }

        var result = new EmailSyncResult
        {
            StartTime = DateTime.UtcNow,
            Status = EmailSyncStatus.InProgress
        };

        ImapClient? client = null;
        try
        {
            // Create cancellation token with reasonable timeout (45 seconds for retries)
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(45));

            client = new ImapClient();

            // Configure client timeouts (shorter individual timeout, but with retries)
            client.Timeout = (int)TimeSpan.FromSeconds(20).TotalMilliseconds;

            _logger.LogInformation("Connecting to IMAP server {Server}:{Port} for account {Email}",
                account.ImapServer, account.ImapPort, account.EmailAddress);

            // Retry logic for connection
            var maxRetries = 2;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogDebug("Connection attempt {Attempt}/{MaxRetries} for {Email}",
                        attempt, maxRetries, account.EmailAddress);

                    // Try to connect with timeout
                    var sslOptions = account.ImapUseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None;
                    await client.ConnectAsync(account.ImapServer, account.ImapPort, sslOptions, timeoutCts.Token);

                    _logger.LogDebug("Connection successful on attempt {Attempt} for {Email}",
                        attempt, account.EmailAddress);
                    break;
                }
                catch (Exception ex) when (attempt < maxRetries && !timeoutCts.Token.IsCancellationRequested)
                {
                    lastException = ex;
                    _logger.LogWarning("Connection attempt {Attempt} failed for {Email}: {Message}. Retrying...",
                        attempt, account.EmailAddress, ex.Message);

                    // Wait before retry (exponential backoff)
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2), timeoutCts.Token);

                    // Dispose and create new client for retry
                    client.Dispose();
                    client = new ImapClient();
                    client.Timeout = (int)TimeSpan.FromSeconds(20).TotalMilliseconds;
                }
            }

            if (!client.IsConnected)
            {
                throw lastException ?? new InvalidOperationException("Failed to connect after all retry attempts");
            }

            _logger.LogWarning("Authenticating account {Email}", account.EmailAddress);
            _logger.LogWarning("Encrypted password length: {Length}, starts with: {Start}",
                account.EncryptedPassword?.Length ?? 0,
                account.EncryptedPassword?.Substring(0, Math.Min(10, account.EncryptedPassword?.Length ?? 0)) ?? "null");

            var decryptedPassword = _encryptionService.DecryptString(account.EncryptedPassword ?? string.Empty);
            _logger.LogWarning("Decrypted password length: {Length}, starts with: {Start}",
                decryptedPassword?.Length ?? 0,
                decryptedPassword?.Substring(0, Math.Min(3, decryptedPassword?.Length ?? 0)) ?? "null");

            await client.AuthenticateAsync(account.EmailAddress, decryptedPassword, timeoutCts.Token);

            var folderName = folder ?? "INBOX";
            _logger.LogDebug("Opening folder {Folder} for account {Email}", folderName, account.EmailAddress);

            var mailFolder = await client.GetFolderAsync(folderName, timeoutCts.Token);
            await mailFolder.OpenAsync(FolderAccess.ReadOnly, timeoutCts.Token);

            // Basic sync implementation - get recent messages (limit to last 50)
            var count = Math.Min(mailFolder.Count, 50);
            var newEmails = 0;

            _logger.LogInformation("Folder {Folder} has {TotalCount} messages, fetching last {FetchCount} for account {Email}",
                folderName, mailFolder.Count, count, account.EmailAddress);

            if (count > 0)
            {
                var messages = await mailFolder.FetchAsync(Math.Max(0, mailFolder.Count - count), -1,
                    MessageSummaryItems.Envelope | MessageSummaryItems.Flags | MessageSummaryItems.UniqueId,
                    timeoutCts.Token);

                _logger.LogInformation("Fetched {MessageCount} messages from server for account {Email}",
                    messages.Count(), account.EmailAddress);

                foreach (var message in messages)
                {
                    try
                    {
                        var messageId = message.Envelope?.MessageId ?? $"uid-{message.UniqueId}";

                        // Check if this message already exists
                        var existingEmail = await _context.EmailMessages
                            .FirstOrDefaultAsync(e => e.MessageId == messageId && e.AccountId == account.Id, cancellationToken);

                        if (existingEmail == null)
                        {
                            _logger.LogDebug("Processing new message: {Subject} from {From} for account {Email}",
                                message.Envelope?.Subject ?? "(No Subject)",
                                message.Envelope?.From?.FirstOrDefault()?.ToString() ?? "(Unknown)",
                                account.EmailAddress);

                            var emailMessage = new EmailMessage
                            {
                                AccountId = account.Id,
                                MessageId = messageId,
                                Subject = message.Envelope?.Subject ?? "(No Subject)",
                                From = message.Envelope?.From?.FirstOrDefault()?.ToString() ?? "",
                                To = message.Envelope?.To?.FirstOrDefault()?.ToString() ?? "",
                                DateReceived = message.Envelope?.Date?.DateTime ?? DateTime.UtcNow,
                                DateSent = message.Envelope?.Date?.DateTime ?? DateTime.UtcNow,
                                IsRead = message.Flags?.HasFlag(MessageFlags.Seen) ?? false,
                                IsImportant = message.Flags?.HasFlag(MessageFlags.Flagged) ?? false,
                                Folder = folderName,
                                ServerUid = message.UniqueId.Id,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            _context.EmailMessages.Add(emailMessage);
                            newEmails++;
                        }
                        else
                        {
                            _logger.LogDebug("Message {MessageId} already exists for account {Email}, skipping",
                                messageId, account.EmailAddress);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing message {MessageId} for account {Email}",
                            message.Envelope?.MessageId ?? "unknown", account.EmailAddress);
                        continue;
                    }
                }

                if (newEmails > 0)
                {
                    _logger.LogInformation("Saving {NewEmails} new emails to database for account {Email}",
                        newEmails, account.EmailAddress);

                    try
                    {
                        await _context.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("Successfully saved {NewEmails} new emails for account {Email}",
                            newEmails, account.EmailAddress);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save {NewEmails} emails to database for account {Email}",
                            newEmails, account.EmailAddress);
                        throw;
                    }
                }
                else
                {
                    _logger.LogInformation("No new emails found for account {Email} - all {MessageCount} messages already exist",
                        account.EmailAddress, messages.Count());
                }

                _logger.LogInformation("Synced {NewEmails} new emails from {Folder} for account {Email}",
                    newEmails, folderName, account.EmailAddress);
            }
            else
            {
                _logger.LogDebug("No messages found in folder {Folder} for account {Email}",
                    folderName, account.EmailAddress);
            }

            await client.DisconnectAsync(true, timeoutCts.Token);

            result.Status = EmailSyncStatus.Success;
            result.NewEmailsCount = newEmails;
            result.EndTime = DateTime.UtcNow;

            SyncStatusChanged?.Invoke(this, new SyncStatusChangedEventArgs(account.Id, EmailSyncStatus.Success, "Sync completed successfully"));
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Email sync cancelled by user for account {Email}", account.EmailAddress);
            result.Status = EmailSyncStatus.Cancelled;
            result.ErrorMessage = "Sync cancelled by user";
            result.EndTime = DateTime.UtcNow;
            return result;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Email sync timed out for account {Email}: {Message}", account.EmailAddress, ex.Message);
            result.Status = EmailSyncStatus.Failed;
            result.ErrorMessage = "Connection timed out - server may be slow or unreachable";
            result.EndTime = DateTime.UtcNow;

            SyncStatusChanged?.Invoke(this, new SyncStatusChangedEventArgs(account.Id, EmailSyncStatus.Failed, "Connection timed out"));
            return result;
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex, "Email sync timed out for account {Email}", account.EmailAddress);
            result.Status = EmailSyncStatus.Failed;
            result.ErrorMessage = "Connection timed out";
            result.EndTime = DateTime.UtcNow;

            SyncStatusChanged?.Invoke(this, new SyncStatusChangedEventArgs(account.Id, EmailSyncStatus.Failed, "Connection timed out"));
            return result;
        }
        catch (MailKit.Security.AuthenticationException ex)
        {
            _logger.LogWarning("Authentication failed for account {Email}: {Message}", account.EmailAddress, ex.Message);
            result.Status = EmailSyncStatus.Failed;
            result.ErrorMessage = $"Authentication failed: {ex.Message}";
            result.EndTime = DateTime.UtcNow;

            SyncStatusChanged?.Invoke(this, new SyncStatusChangedEventArgs(account.Id, EmailSyncStatus.Failed, "Authentication failed - please check credentials"));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing emails for account {Email}", account.EmailAddress);
            result.Status = EmailSyncStatus.Failed;
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.UtcNow;

            SyncStatusChanged?.Invoke(this, new SyncStatusChangedEventArgs(account.Id, EmailSyncStatus.Failed, ex.Message));
            ErrorOccurred?.Invoke(this, new EmailErrorEventArgs(ex.Message, ex, account.Id));
            return result;
        }
        finally
        {
            // Ensure client is properly disposed
            if (client?.IsConnected == true)
            {
                try
                {
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error disconnecting IMAP client for account {Email}", account.EmailAddress);
                }
            }
            client?.Dispose();
        }
    }

    /// <summary>
    /// Gets the last synchronization status for the specified account.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the last sync status or null if not found.</returns>
    public async Task<EmailSyncResult?> GetLastSyncStatusAsync(int accountId, CancellationToken cancellationToken = default)
    {
        // Stub implementation - would need to store sync results
        await Task.CompletedTask;
        return new EmailSyncResult
        {
            Status = EmailSyncStatus.None,
            StartTime = DateTime.MinValue,
            EndTime = DateTime.MinValue
        };
    }

    /// <summary>
    /// Starts idle synchronization (real-time email monitoring) for the specified account and folder.
    /// </summary>
    /// <param name="account">The email account to monitor.</param>
    /// <param name="folder">The folder to monitor for new emails (default: INBOX).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StartIdleSyncAsync(EmailAccount account, string folder = "INBOX", CancellationToken cancellationToken = default)
    {
        // Stub implementation - would need to implement IDLE monitoring
        await Task.CompletedTask;
        _logger.LogInformation("IDLE sync started for account {Email}", account.EmailAddress);
    }

    /// <summary>
    /// Stops idle synchronization for the specified account.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StopIdleSyncAsync(int accountId, CancellationToken cancellationToken = default)
    {
        // Stub implementation - would need to stop IDLE monitoring
        await Task.CompletedTask;
        _logger.LogInformation("IDLE sync stopped for account {AccountId}", accountId);
    }

    // Search methods
    /// <summary>
    /// Searches emails using advanced search criteria.
    /// </summary>
    /// <param name="criteria">The search criteria containing filters and parameters.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains matching emails.</returns>
    public async Task<List<EmailMessage>> SearchEmailsAsync(EmailSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.EmailMessages.AsQueryable();

            if (criteria.AccountId.HasValue)
                query = query.Where(e => e.AccountId == criteria.AccountId.Value);

            if (!string.IsNullOrEmpty(criteria.Query))
                query = query.Where(e =>
                    (e.Subject != null && criteria.Query != null && e.Subject.Contains(criteria.Query)) ||
                    (e.Body != null && criteria.Query != null && e.Body.Contains(criteria.Query)));

            if (!string.IsNullOrEmpty(criteria.From))
                query = query.Where(e => e.FromAddress.Contains(criteria.From));

            if (!string.IsNullOrEmpty(criteria.To))
                query = query.Where(e => e.ToAddress.Contains(criteria.To));

            if (!string.IsNullOrEmpty(criteria.Subject))
                query = query.Where(e => e.Subject.Contains(criteria.Subject));

            if (!string.IsNullOrEmpty(criteria.FolderName))
                query = query.Where(e => e.Folder == criteria.FolderName);

            if (criteria.DateFrom.HasValue)
                query = query.Where(e => e.DateReceived >= criteria.DateFrom.Value);

            if (criteria.DateTo.HasValue)
                query = query.Where(e => e.DateReceived <= criteria.DateTo.Value);

            if (criteria.IsRead.HasValue)
                query = query.Where(e => e.IsRead == criteria.IsRead.Value);

            if (criteria.IsImportant.HasValue)
                query = query.Where(e => e.IsImportant == criteria.IsImportant.Value);

            return await query
                .Skip(criteria.Offset)
                .Take(criteria.MaxResults)
                .Include(e => e.Attachments)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching emails");
            return new List<EmailMessage>();
        }
    }

    /// <summary>
    /// Searches emails using a simple text query.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="accountId">Specific account ID to search in (null for all accounts).</param>
    /// <param name="folder">Specific folder to search in (null for all folders).</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains matching emails.</returns>
    public async Task<List<EmailMessage>> SearchEmailsAsync(string query, int? accountId = null, string? folder = null, int maxResults = 50, CancellationToken cancellationToken = default)
    {
        var criteria = new EmailSearchCriteria
        {
            Query = query,
            AccountId = accountId,
            FolderName = folder,
            MaxResults = maxResults
        };
        return await SearchEmailsAsync(criteria, cancellationToken);
    }

    // Attachment methods
    /// <summary>
    /// Downloads an email attachment by its database ID.
    /// </summary>
    /// <param name="attachmentId">The database ID of the attachment to download.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the attachment data or null if not found.</returns>
    public async Task<EmailAttachment?> DownloadAttachmentAsync(int attachmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var attachment = await _context.EmailAttachments.FindAsync(attachmentId);
            return attachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", attachmentId);
            return null;
        }
    }

    /// <summary>
    /// Downloads and saves an email attachment to a specified file path.
    /// </summary>
    /// <param name="attachmentId">The database ID of the attachment to save.</param>
    /// <param name="filePath">The file path where the attachment should be saved.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if saved successfully.</returns>
    public async Task<bool> SaveAttachmentAsync(int attachmentId, string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var attachment = await DownloadAttachmentAsync(attachmentId, cancellationToken);
            if (attachment?.Data != null)
            {
                await File.WriteAllBytesAsync(filePath, attachment.Data, cancellationToken);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving attachment {AttachmentId}", attachmentId);
            return false;
        }
    }

    /// <summary>
    /// Gets all attachments for a specific email.
    /// </summary>
    /// <param name="emailId">The database ID of the email.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of attachments.</returns>
    public async Task<List<EmailAttachment>> GetAttachmentsAsync(int emailId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.EmailAttachments
                .Where(a => a.EmailMessageId == emailId)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attachments for email {EmailId}", emailId);
            return new List<EmailAttachment>();
        }
    }

    /// <summary>
    /// Tests the connection to the email server using the provided account settings.
    /// </summary>
    /// <param name="account">The email account to test connection for.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains connection test results.</returns>
    public async Task<ConnectionTestResult> TestConnectionAsync(EmailAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
        {
            _logger.LogWarning("Account is null in TestConnectionAsync");
            return new ConnectionTestResult
            {
                IsSuccessful = false,
                GeneralError = "Account cannot be null"
            };
        }

        var result = new ConnectionTestResult();

        // Create timeout token (15 seconds for connection tests)
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));

        try
        {
            var password = _encryptionService.DecryptString(account.EncryptedPassword);

            // Test IMAP connection with retry logic
            _logger.LogDebug("Testing IMAP connection for {Email}", account.EmailAddress);
            try
            {
                var maxRetries = 2;
                Exception? lastImapException = null;

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        using var imapClient = new ImapClient();
                        imapClient.Timeout = 10000; // 10 seconds per attempt

                        _logger.LogDebug("IMAP test attempt {Attempt}/{MaxRetries} for {Email}",
                            attempt, maxRetries, account.EmailAddress);

                        await imapClient.ConnectAsync(account.ImapServer, account.ImapPort, account.ImapUseSsl, timeoutCts.Token);
                        await imapClient.AuthenticateAsync(account.EmailAddress, password, timeoutCts.Token);
                        await imapClient.DisconnectAsync(true, timeoutCts.Token);
                        result.ImapSuccess = true;
                        _logger.LogDebug("IMAP connection successful for {Email}", account.EmailAddress);
                        break;
                    }
                    catch (Exception ex) when (attempt < maxRetries && !timeoutCts.Token.IsCancellationRequested)
                    {
                        lastImapException = ex;
                        _logger.LogDebug("IMAP test attempt {Attempt} failed for {Email}: {Message}",
                            attempt, account.EmailAddress, ex.Message);
                        await Task.Delay(1000, timeoutCts.Token); // 1 second delay
                    }
                }

                if (!result.ImapSuccess && lastImapException != null)
                {
                    throw lastImapException;
                }
            }
            catch (MailKit.Security.AuthenticationException ex)
            {
                _logger.LogWarning("IMAP authentication failed for {Email}: {Message}", account.EmailAddress, ex.Message);
                result.ImapError = $"IMAP Authentication failed: {ex.Message}";
                result.ImapSuccess = false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("IMAP connection failed for {Email}: {Message}", account.EmailAddress, ex.Message);
                result.ImapError = $"IMAP connection failed: {ex.Message}";
                result.ImapSuccess = false;
            }

            // Test SMTP connection with retry logic
            _logger.LogDebug("Testing SMTP connection for {Email}", account.EmailAddress);
            try
            {
                var maxRetries = 2;
                Exception? lastSmtpException = null;

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        using var smtpClient = new SmtpClient();
                        smtpClient.Timeout = 10000; // 10 seconds per attempt

                        _logger.LogDebug("SMTP test attempt {Attempt}/{MaxRetries} for {Email}",
                            attempt, maxRetries, account.EmailAddress);

                        var smtpSslOptions = account.SmtpPort == 465 ? SecureSocketOptions.SslOnConnect :
                                           account.SmtpPort == 25 ? SecureSocketOptions.None :
                                           SecureSocketOptions.StartTls;

                        await smtpClient.ConnectAsync(account.SmtpServer, account.SmtpPort, smtpSslOptions, timeoutCts.Token);
                        await smtpClient.AuthenticateAsync(account.EmailAddress, password, timeoutCts.Token);
                        await smtpClient.DisconnectAsync(true, timeoutCts.Token);
                        result.SmtpSuccess = true;
                        _logger.LogDebug("SMTP connection successful for {Email}", account.EmailAddress);
                        break;
                    }
                    catch (Exception ex) when (attempt < maxRetries && !timeoutCts.Token.IsCancellationRequested)
                    {
                        lastSmtpException = ex;
                        _logger.LogDebug("SMTP test attempt {Attempt} failed for {Email}: {Message}",
                            attempt, account.EmailAddress, ex.Message);
                        await Task.Delay(1000, timeoutCts.Token); // 1 second delay
                    }
                }

                if (!result.SmtpSuccess && lastSmtpException != null)
                {
                    throw lastSmtpException;
                }
            }
            catch (MailKit.Security.AuthenticationException ex)
            {
                _logger.LogWarning("SMTP authentication failed for {Email}: {Message}", account.EmailAddress, ex.Message);
                result.SmtpError = $"SMTP Authentication failed: {ex.Message}";
                result.SmtpSuccess = false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("SMTP connection failed for {Email}: {Message}", account.EmailAddress, ex.Message);
                result.SmtpError = $"SMTP connection failed: {ex.Message}";
                result.SmtpSuccess = false;
            }

            result.IsSuccessful = result.ImapSuccess && result.SmtpSuccess;

            if (!result.IsSuccessful)
            {
                result.GeneralError = "One or more connection tests failed. Check individual error messages.";
            }
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            _logger.LogWarning("Connection test timed out for account {Email}", account.EmailAddress);
            result.GeneralError = "Connection test timed out after 15 seconds";
            result.IsSuccessful = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error testing connection for account {Email}", account.EmailAddress);
            result.GeneralError = $"Unexpected error: {ex.Message}";
            result.IsSuccessful = false;
        }

        return result;
    }

    /// <summary>
    /// Synchronizes folders and tags from the email server to the local database.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SyncFoldersAndTagsAsync(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _context.EmailAccounts.FindAsync(accountId);
            if (account == null)
            {
                _logger.LogWarning("Account {AccountId} not found for folder sync", accountId);
                return;
            }

            using var client = new ImapClient();
            await client.ConnectAsync(account.ImapServer, account.ImapPort, account.ImapUseSsl, cancellationToken);
            await client.AuthenticateAsync(account.EmailAddress, _encryptionService.DecryptString(account.EncryptedPassword), cancellationToken);

            // Get all folders from server
            var folders = await client.GetFoldersAsync(client.PersonalNamespaces[0], cancellationToken: cancellationToken);

            foreach (var folder in folders)
            {
                var existingFolder = await _context.EmailFolders
                    .FirstOrDefaultAsync(f => f.AccountId == accountId && f.Name == folder.Name, cancellationToken);

                if (existingFolder == null)
                {
                    // Create new folder
                    var newFolder = new EmailFolder
                    {
                        AccountId = accountId,
                        Name = folder.Name,
                        DisplayName = folder.Name,
                        MessageCount = (int)folder.Count,
                        UnreadCount = (int)folder.Unread,
                        IsSystemFolder = IsSystemFolder(folder.Name),
                        FolderType = GetFolderType(folder.Name),
                        LastSyncAt = DateTime.UtcNow
                    };

                    _context.EmailFolders.Add(newFolder);
                }
                else
                {
                    // Update existing folder
                    existingFolder.MessageCount = (int)folder.Count;
                    existingFolder.UnreadCount = (int)folder.Unread;
                    existingFolder.LastSyncAt = DateTime.UtcNow;
                    existingFolder.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Successfully synced folders for account {AccountId}", accountId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing folders and tags for account {AccountId}", accountId);
            throw;
        }
    }

    private bool IsSystemFolder(string folderName)
    {
        var systemFolders = new[] { "INBOX", "SENT", "DRAFTS", "TRASH", "SPAM", "JUNK" };
        return systemFolders.Contains(folderName.ToUpper());
    }

    private string GetFolderType(string folderName)
    {
        return folderName.ToUpper() switch
        {
            "INBOX" => "Inbox",
            "SENT" => "Sent",
            "DRAFTS" => "Drafts",
            "TRASH" => "Trash",
            "SPAM" or "JUNK" => "Spam",
            _ => "Custom"
        };
    }

    /// <summary>
    /// Gets all folders for the specified email account from the server.
    /// </summary>
    /// <param name="account">The email account to get folders for.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of folder names.</returns>
    public async Task<List<string>> GetFoldersAsync(EmailAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
        {
            _logger.LogWarning("Account is null for GetFoldersAsync");
            return new List<string>();
        }

        try
        {
            var password = _encryptionService.DecryptPassword(account.EncryptedPassword ?? account.Password ?? "");

            using var client = new ImapClient();
            await client.ConnectAsync(account.ImapServer, account.ImapPort, account.ImapUseSsl, cancellationToken);
            await client.AuthenticateAsync(account.Email, password, cancellationToken);

            var folders = new List<string>();
            var namespaces = client.PersonalNamespaces;

            foreach (var ns in namespaces)
            {
                var topLevel = await client.GetFoldersAsync(ns, cancellationToken: cancellationToken);
                foreach (var folder in topLevel)
                {
                    folders.Add(folder.FullName);

                    // Get subfolders
                    var subfolders = await folder.GetSubfoldersAsync(cancellationToken: cancellationToken);
                    foreach (var subfolder in subfolders)
                    {
                        folders.Add(subfolder.FullName);
                    }
                }
            }

            await client.DisconnectAsync(true, cancellationToken);
            return folders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folders for account {Email}", account.Email);
            return new List<string>();
        }
    }

    /// <summary>
    /// Marks a message as read directly on the email server using its UID.
    /// </summary>
    /// <param name="account">The email account.</param>
    /// <param name="messageUid">The unique identifier of the message on the server.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task MarkAsReadOnServerAsync(EmailAccount account, string messageUid, CancellationToken cancellationToken = default)
    {
        if (account == null || string.IsNullOrEmpty(messageUid))
        {
            _logger.LogWarning("Invalid parameters for MarkAsReadOnServerAsync");
            return;
        }

        try
        {
            var password = _encryptionService.DecryptPassword(account.EncryptedPassword ?? account.Password ?? "");

            using var client = new ImapClient();
            await client.ConnectAsync(account.ImapServer, account.ImapPort, account.ImapUseSsl, cancellationToken);
            await client.AuthenticateAsync(account.Email, password, cancellationToken);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken);

            // Find message by UID
            var uid = new UniqueId(uint.Parse(messageUid));
            var uids = new[] { uid };

            if (uids.Length > 0)
            {
                await inbox.AddFlagsAsync(uids, MessageFlags.Seen, true, cancellationToken);
            }

            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message as read on server for account {Email}, UID {Uid}", account.Email, messageUid);
        }
    }
}
