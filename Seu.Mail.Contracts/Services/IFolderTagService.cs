using Seu.Mail.Core.Models;

namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Service interface for managing folders and tags for email accounts.
/// </summary>
public interface IFolderTagService
{
    /// <summary>
    /// Gets all folders for the specified account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>List of folders.</returns>
    Task<List<EmailFolder>> GetFoldersAsync(int accountId);

    /// <summary>
    /// Gets all tags for the specified account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>List of tags.</returns>
    Task<List<EmailTag>> GetTagsAsync(int accountId);

    /// <summary>
    /// Gets a folder by its unique identifier.
    /// </summary>
    /// <param name="folderId">The folder ID.</param>
    /// <returns>The folder if found; otherwise, null.</returns>
    Task<EmailFolder?> GetFolderByIdAsync(int folderId);

    /// <summary>
    /// Gets a tag by its unique identifier.
    /// </summary>
    /// <param name="tagId">The tag ID.</param>
    /// <returns>The tag if found; otherwise, null.</returns>
    Task<EmailTag?> GetTagByIdAsync(int tagId);

    /// <summary>
    /// Creates a new folder.
    /// </summary>
    /// <param name="folder">The folder to create.</param>
    /// <returns>True if created successfully; otherwise, false.</returns>
    Task<bool> CreateFolderAsync(EmailFolder folder);

    /// <summary>
    /// Creates a new tag.
    /// </summary>
    /// <param name="tag">The tag to create.</param>
    /// <returns>True if created successfully; otherwise, false.</returns>
    Task<bool> CreateTagAsync(EmailTag tag);

    /// <summary>
    /// Updates an existing folder.
    /// </summary>
    /// <param name="folder">The folder to update.</param>
    /// <returns>True if updated successfully; otherwise, false.</returns>
    Task<bool> UpdateFolderAsync(EmailFolder folder);

    /// <summary>
    /// Updates an existing tag.
    /// </summary>
    /// <param name="tag">The tag to update.</param>
    /// <returns>True if updated successfully; otherwise, false.</returns>
    Task<bool> UpdateTagAsync(EmailTag tag);

    /// <summary>
    /// Deletes a folder by its unique identifier.
    /// </summary>
    /// <param name="folderId">The folder ID.</param>
    /// <returns>True if deleted successfully; otherwise, false.</returns>
    Task<bool> DeleteFolderAsync(int folderId);

    /// <summary>
    /// Deletes a tag by its unique identifier.
    /// </summary>
    /// <param name="tagId">The tag ID.</param>
    /// <returns>True if deleted successfully; otherwise, false.</returns>
    Task<bool> DeleteTagAsync(int tagId);

    /// <summary>
    /// Synchronizes folders from the server for the specified account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>True if synchronization was successful; otherwise, false.</returns>
    Task<bool> SyncFoldersFromServerAsync(int accountId);

    /// <summary>
    /// Adds a tag to an email.
    /// </summary>
    /// <param name="emailId">The email ID.</param>
    /// <param name="tagId">The tag ID.</param>
    /// <returns>True if added successfully; otherwise, false.</returns>
    Task<bool> AddTagToEmailAsync(int emailId, int tagId);

    /// <summary>
    /// Removes a tag from an email.
    /// </summary>
    /// <param name="emailId">The email ID.</param>
    /// <param name="tagId">The tag ID.</param>
    /// <returns>True if removed successfully; otherwise, false.</returns>
    Task<bool> RemoveTagFromEmailAsync(int emailId, int tagId);

    /// <summary>
    /// Gets all tags associated with an email.
    /// </summary>
    /// <param name="emailId">The email ID.</param>
    /// <returns>List of tags.</returns>
    Task<List<EmailTag>> GetEmailTagsAsync(int emailId);

    /// <summary>
    /// Gets the message count for a folder.
    /// </summary>
    /// <param name="folderId">The folder ID.</param>
    /// <returns>Message count.</returns>
    Task<int> GetFolderMessageCountAsync(int folderId);

    /// <summary>
    /// Gets the unread message count for a folder.
    /// </summary>
    /// <param name="folderId">The folder ID.</param>
    /// <returns>Unread message count.</returns>
    Task<int> GetFolderUnreadCountAsync(int folderId);
}