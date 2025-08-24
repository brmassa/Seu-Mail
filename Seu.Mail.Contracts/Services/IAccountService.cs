using Seu.Mail.Core.Models;

namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Service interface for managing email accounts.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Gets all email accounts.
    /// </summary>
    /// <returns>List of all email accounts.</returns>
    Task<List<EmailAccount>> GetAllAccountsAsync();

    /// <summary>
    /// Gets an email account by its unique identifier.
    /// </summary>
    /// <param name="id">The account ID.</param>
    /// <returns>The email account if found; otherwise, null.</returns>
    Task<EmailAccount?> GetAccountByIdAsync(int id);

    /// <summary>
    /// Gets the default email account.
    /// </summary>
    /// <returns>The default email account if set; otherwise, null.</returns>
    Task<EmailAccount?> GetDefaultAccountAsync();

    /// <summary>
    /// Adds a new email account.
    /// </summary>
    /// <param name="account">The account to add.</param>
    /// <returns>True if the account was added successfully; otherwise, false.</returns>
    Task<bool> AddAccountAsync(EmailAccount account);

    /// <summary>
    /// Updates an existing email account.
    /// </summary>
    /// <param name="account">The account to update.</param>
    /// <returns>True if the account was updated successfully; otherwise, false.</returns>
    Task<bool> UpdateAccountAsync(EmailAccount account);

    /// <summary>
    /// Deletes an email account by its unique identifier.
    /// </summary>
    /// <param name="id">The account ID.</param>
    /// <returns>True if the account was deleted successfully; otherwise, false.</returns>
    Task<bool> DeleteAccountAsync(int id);

    /// <summary>
    /// Sets the default email account.
    /// </summary>
    /// <param name="id">The account ID to set as default.</param>
    /// <returns>True if the default account was set successfully; otherwise, false.</returns>
    Task<bool> SetDefaultAccountAsync(int id);

    /// <summary>
    /// Validates the connection settings for an email account.
    /// </summary>
    /// <param name="account">The account to validate.</param>
    /// <returns>The result of the connection test.</returns>
    Task<ConnectionTestResult> ValidateAccountAsync(EmailAccount account);

    /// <summary>
    /// Synchronizes folders and tags for the specified account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <returns>True if synchronization was successful; otherwise, false.</returns>
    Task<bool> SyncAccountFoldersAndTagsAsync(int accountId);
}