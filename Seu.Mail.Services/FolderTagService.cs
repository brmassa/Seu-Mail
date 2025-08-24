using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Seu.Mail.Data.Context;
using Seu.Mail.Core.Models;
using Seu.Mail.Contracts.Services;
using MailKit.Net.Imap;
using MailKit.Security;

namespace Seu.Mail.Services;

/// <summary>
/// Provides services for managing email folders and tags including creation, synchronization, and organization.
/// </summary>
public class FolderTagService : IFolderTagService
{
    private readonly EmailDbContext _context;
    private readonly ILogger<FolderTagService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderTagService"/> class.
    /// </summary>
    /// <param name="context">The database context for folder and tag data operations.</param>
    /// <param name="logger">Logger for folder and tag service events and errors.</param>
    public FolderTagService(EmailDbContext context, ILogger<FolderTagService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all folders for the specified email account.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of email folders.</returns>
    public async Task<List<EmailFolder>> GetFoldersAsync(int accountId)
    {
        try
        {
            return await _context.EmailFolders
                .Where(f => f.AccountId == accountId)
                .OrderByDescending(f => f.IsSystemFolder)
                .ThenBy(f => f.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folders for account {AccountId}", accountId);
            return new List<EmailFolder>();
        }
    }

    /// <summary>
    /// Gets all tags for the specified email account.
    /// </summary>
    /// <param name="accountId">The database ID of the email account.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of email tags.</returns>
    public async Task<List<EmailTag>> GetTagsAsync(int accountId)
    {
        try
        {
            return await _context.EmailTags
                .Where(t => t.AccountId == accountId)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tags for account {AccountId}", accountId);
            return new List<EmailTag>();
        }
    }

    /// <summary>
    /// Gets a specific email folder by its database ID.
    /// </summary>
    /// <param name="folderId">The database ID of the folder to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the email folder or null if not found.</returns>
    public async Task<EmailFolder?> GetFolderByIdAsync(int folderId)
    {
        try
        {
            return await _context.EmailFolders.FindAsync(folderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folder by ID {FolderId}", folderId);
            return null;
        }
    }

    /// <summary>
    /// Gets a specific email tag by its database ID.
    /// </summary>
    /// <param name="tagId">The database ID of the tag to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the email tag or null if not found.</returns>
    public async Task<EmailTag?> GetTagByIdAsync(int tagId)
    {
        try
        {
            return await _context.EmailTags.FindAsync(tagId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tag by ID {TagId}", tagId);
            return null;
        }
    }

    /// <summary>
    /// Creates a new email folder.
    /// </summary>
    /// <param name="folder">The email folder to create.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the folder was created successfully.</returns>
    public async Task<bool> CreateFolderAsync(EmailFolder folder)
    {
        try
        {
            _context.EmailFolders.Add(folder);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating folder {FolderName}", folder.Name);
            return false;
        }
    }

    /// <summary>
    /// Creates a new email tag.
    /// </summary>
    /// <param name="tag">The email tag to create.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the tag was created successfully.</returns>
    public async Task<bool> CreateTagAsync(EmailTag tag)
    {
        try
        {
            _context.EmailTags.Add(tag);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tag {TagName}", tag.Name);
            return false;
        }
    }

    /// <summary>
    /// Updates an existing email folder.
    /// </summary>
    /// <param name="folder">The email folder with updated information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the folder was updated successfully.</returns>
    public async Task<bool> UpdateFolderAsync(EmailFolder folder)
    {
        try
        {
            _context.EmailFolders.Update(folder);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating folder {FolderName}", folder.Name);
            return false;
        }
    }

    /// <summary>
    /// Updates an existing email tag.
    /// </summary>
    /// <param name="tag">The email tag with updated information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the tag was updated successfully.</returns>
    public async Task<bool> UpdateTagAsync(EmailTag tag)
    {
        try
        {
            _context.EmailTags.Update(tag);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tag {TagName}", tag.Name);
            return false;
        }
    }

    /// <summary>
    /// Deletes an email folder by its database ID.
    /// </summary>
    /// <param name="folderId">The database ID of the folder to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the folder was deleted successfully.</returns>
    public async Task<bool> DeleteFolderAsync(int folderId)
    {
        try
        {
            var folder = await _context.EmailFolders.FindAsync(folderId);
            if (folder != null)
            {
                _context.EmailFolders.Remove(folder);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting folder {FolderId}", folderId);
            return false;
        }
    }

    /// <summary>
    /// Deletes an email tag by its database ID.
    /// </summary>
    /// <param name="tagId">The database ID of the tag to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the tag was deleted successfully.</returns>
    public async Task<bool> DeleteTagAsync(int tagId)
    {
        try
        {
            var tag = await _context.EmailTags.FindAsync(tagId);
            if (tag != null)
            {
                _context.EmailTags.Remove(tag);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tag {TagId}", tagId);
            return false;
        }
    }

    /// <summary>
    /// Synchronizes folders from the email server for the specified account.
    /// </summary>
    /// <param name="accountId">The database ID of the email account to synchronize folders for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if synchronization was successful.</returns>
    public async Task<bool> SyncFoldersFromServerAsync(int accountId)
    {
        try
        {
            var account = await _context.EmailAccounts.FindAsync(accountId);
            if (account == null)
            {
                _logger.LogWarning("Account {AccountId} not found for folder sync", accountId);
                return false;
            }

            using var client = new ImapClient();
            await client.ConnectAsync(account.ImapServer, account.ImapPort, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(account.EmailAddress, account.Password);

            var serverFolders = await client.GetFoldersAsync(client.PersonalNamespaces[0]);
            var existingFolders = await GetFoldersAsync(accountId);

            foreach (var serverFolder in serverFolders)
            {
                var existingFolder = existingFolders.FirstOrDefault(f => f.Name == serverFolder.FullName);
                if (existingFolder == null)
                {
                    var newFolder = new EmailFolder
                    {
                        Name = serverFolder.FullName,
                        AccountId = accountId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await CreateFolderAsync(newFolder);
                }
            }

            await client.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing folders for account {AccountId}", accountId);
            return false;
        }
    }

    /// <summary>
    /// Adds a tag to an email message.
    /// </summary>
    /// <param name="emailId">The database ID of the email message.</param>
    /// <param name="tagId">The database ID of the tag to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the tag was added successfully.</returns>
    public async Task<bool> AddTagToEmailAsync(int emailId, int tagId)
    {
        try
        {
            var email = await _context.EmailMessages.FindAsync(emailId);
            var tag = await _context.EmailTags.FindAsync(tagId);

            if (email != null && tag != null)
            {
                // Check if association already exists
                var existingAssociation = await _context.EmailMessageTags
                    .FirstOrDefaultAsync(emt => emt.EmailMessageId == emailId && emt.TagId == tagId);

                if (existingAssociation == null)
                {
                    // Create new association
                    var association = new EmailMessageTag
                    {
                        EmailMessageId = emailId,
                        TagId = tagId
                    };

                    await _context.EmailMessageTags.AddAsync(association);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tag {TagId} to email {EmailId}", tagId, emailId);
            return false;
        }
    }

    /// <summary>
    /// Removes a tag from an email message.
    /// </summary>
    /// <param name="emailId">The database ID of the email message.</param>
    /// <param name="tagId">The database ID of the tag to remove.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the tag was removed successfully.</returns>
    public async Task<bool> RemoveTagFromEmailAsync(int emailId, int tagId)
    {
        try
        {
            // Find and remove the association
            var association = await _context.EmailMessageTags
                .FirstOrDefaultAsync(emt => emt.EmailMessageId == emailId && emt.TagId == tagId);

            if (association != null)
            {
                _context.EmailMessageTags.Remove(association);
                await _context.SaveChangesAsync();
            }

            return true; // Return true whether association existed or not (idempotent)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tag {TagId} from email {EmailId}", tagId, emailId);
            return false;
        }
    }

    /// <summary>
    /// Gets all tags associated with a specific email message.
    /// </summary>
    /// <param name="emailId">The database ID of the email message.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of tags associated with the email.</returns>
    public async Task<List<EmailTag>> GetEmailTagsAsync(int emailId)
    {
        try
        {
            var tags = await _context.EmailMessageTags
                .Where(emt => emt.EmailMessageId == emailId)
                .Include(emt => emt.Tag)
                .Select(emt => emt.Tag)
                .ToListAsync();

            return tags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tags for email {EmailId}", emailId);
            return new List<EmailTag>();
        }
    }

    /// <summary>
    /// Gets the total number of messages in the specified folder.
    /// </summary>
    /// <param name="folderId">The database ID of the folder.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total message count.</returns>
    public async Task<int> GetFolderMessageCountAsync(int folderId)
    {
        try
        {
            // Map folder ID to folder name
            var folderName = folderId switch
            {
                1 => "INBOX",
                2 => "SENT",
                3 => "DRAFTS",
                4 => "TRASH",
                5 => "SPAM",
                _ => null
            };

            if (folderName == null)
                return 0;

            return await _context.EmailMessages
                .CountAsync(m => m.Folder == folderName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message count for folder {FolderId}", folderId);
            return 0;
        }
    }

    /// <summary>
    /// Gets the number of unread messages in the specified folder.
    /// </summary>
    /// <param name="folderId">The database ID of the folder.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the unread message count.</returns>
    public async Task<int> GetFolderUnreadCountAsync(int folderId)
    {
        try
        {
            // Map folder ID to folder name
            var folderName = folderId switch
            {
                1 => "INBOX",
                2 => "SENT",
                3 => "DRAFTS",
                4 => "TRASH",
                5 => "SPAM",
                _ => null
            };

            if (folderName == null)
                return 0;

            return await _context.EmailMessages
                .CountAsync(m => m.Folder == folderName && !m.IsRead);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for folder {FolderId}", folderId);
            return 0;
        }
    }
}
