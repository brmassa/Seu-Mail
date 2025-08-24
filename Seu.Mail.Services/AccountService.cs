using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Seu.Mail.Data.Context;
using Seu.Mail.Core.Models;
using Seu.Mail.Contracts.Services;

namespace Seu.Mail.Services;

/// <summary>
/// Provides services for managing email accounts including creation, validation, and synchronization.
/// </summary>
public class AccountService : IAccountService
{
    private readonly EmailDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountService> _logger;
    private readonly IEncryptionService _encryptionService;
    private readonly IValidationService _validationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountService"/> class.
    /// </summary>
    /// <param name="context">The database context for email data.</param>
    /// <param name="emailService">The email service for account operations.</param>
    /// <param name="logger">Logger for account service events and errors.</param>
    /// <param name="encryptionService">Service for encrypting sensitive account data.</param>
    /// <param name="validationService">Service for validating account settings.</param>
    public AccountService(EmailDbContext context, IEmailService emailService, ILogger<AccountService> logger,
        IEncryptionService encryptionService, IValidationService validationService)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
        _encryptionService = encryptionService;
        _validationService = validationService;
    }

    /// <summary>
    /// Gets all email accounts from the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of all email accounts.</returns>
    public async Task<List<EmailAccount>> GetAllAccountsAsync()
    {
        try
        {
            return await _context.EmailAccounts
                .OrderBy(a => a.DisplayName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all accounts");
            return new List<EmailAccount>();
        }
    }

    /// <summary>
    /// Gets a specific email account by its database ID.
    /// </summary>
    /// <param name="id">The database ID of the email account.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the email account or null if not found.</returns>
    public async Task<EmailAccount?> GetAccountByIdAsync(int id)
    {
        try
        {
            return await _context.EmailAccounts.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account by id {AccountId}", id);
            return null;
        }
    }

    /// <summary>
    /// Gets the default email account from the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the default email account or null if not found.</returns>
    public async Task<EmailAccount?> GetDefaultAccountAsync()
    {
        try
        {
            var defaultAccount = await _context.EmailAccounts
                .FirstOrDefaultAsync(a => a.IsDefault);

            if (defaultAccount == null)
                // If no default is set, return the first account
                defaultAccount = await _context.EmailAccounts
                    .OrderBy(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

            return defaultAccount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default account");
            return null;
        }
    }

    /// <summary>
    /// Adds a new email account to the database.
    /// </summary>
    /// <param name="account">The email account to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the account was added successfully.</returns>
    public async Task<bool> AddAccountAsync(EmailAccount account)
    {
        try
        {
            // Validate account data
            var validationResult = _validationService.ValidateAccountData(account);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Account validation failed: {Error}", validationResult.ErrorMessage);
                return false;
            }

            // Check if email already exists
            var existingAccount = await _context.EmailAccounts
                .FirstOrDefaultAsync(a => a.Email == account.Email);

            if (existingAccount != null)
            {
                _logger.LogWarning("Account with email {Email} already exists", account.Email);
                return false;
            }

            // Encrypt password before storing (input comes as plaintext via Password property)
            var plaintextPassword = account.Password; // This gets the plaintext from UI
            _logger.LogWarning("AddAccount: Original password length: {Length}", plaintextPassword?.Length ?? 0);

            if (string.IsNullOrEmpty(plaintextPassword))
            {
                _logger.LogWarning("Cannot add account with null or empty password");
                return false;
            }

            var encryptedPassword = _encryptionService.EncryptString(plaintextPassword);
            _logger.LogWarning("AddAccount: Encrypted password length: {Length}", encryptedPassword?.Length ?? 0);
            account.EncryptedPassword = encryptedPassword ?? string.Empty;

            // Sanitize display name
            if (!string.IsNullOrWhiteSpace(account.DisplayName))
                account.DisplayName = _validationService.SanitizeInput(account.DisplayName);
            else
                account.DisplayName = account.Email;

            // If this is the first account, make it default
            var accountCount = await _context.EmailAccounts.CountAsync();
            if (accountCount == 0) account.IsDefault = true;

            account.CreatedAt = DateTime.UtcNow;
            _context.EmailAccounts.Add(account);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Account {Email} added successfully", account.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding account {Email}", account.Email);
            return false;
        }
    }

    /// <summary>
    /// Updates an existing email account in the database.
    /// </summary>
    /// <param name="account">The email account with updated information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the update was successful.</returns>
    public async Task<bool> UpdateAccountAsync(EmailAccount account)
    {
        try
        {
            // Validate account data
            var validationResult = _validationService.ValidateAccountData(account);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Account validation failed: {Error}", validationResult.ErrorMessage);
                return false;
            }

            var existingAccount = await _context.EmailAccounts.FindAsync(account.Id);
            if (existingAccount == null)
            {
                _logger.LogWarning("Account with ID {AccountId} not found", account.Id);
                return false;
            }

            // Update properties
            existingAccount.DisplayName = _validationService.SanitizeInput(account.DisplayName);
            existingAccount.Email = account.Email;

            // Handle password updates carefully
            var currentPassword = account.Password;

            // Check if password was actually changed (UI sends "***UNCHANGED***" placeholder when not changed)
            if (!string.IsNullOrEmpty(currentPassword) && currentPassword != "***UNCHANGED***")
                // Password was changed - decrypt existing to compare
                try
                {
                    if (string.IsNullOrEmpty(existingAccount.EncryptedPassword))
                    {
                        _logger.LogWarning("Existing account has null or empty encrypted password");
                        existingAccount.EncryptedPassword =
                            _encryptionService.EncryptString(currentPassword ?? string.Empty);
                    }
                    else
                    {
                        var existingDecrypted = _encryptionService.DecryptString(existingAccount.EncryptedPassword);

                        // Only update if the plaintext passwords are actually different
                        if (currentPassword != existingDecrypted)
                        {
                            _logger.LogInformation("Password changed for account {Email}, updating encrypted password",
                                account.Email);
                            existingAccount.EncryptedPassword =
                                _encryptionService.EncryptString(currentPassword ?? string.Empty) ?? string.Empty;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        "Failed to decrypt existing password for comparison: {Error}. Assuming password changed.",
                        ex.Message);
                    // If we can't decrypt existing, assume password changed and encrypt the new one
                    existingAccount.EncryptedPassword =
                        _encryptionService.EncryptString(currentPassword ?? string.Empty) ?? string.Empty;
                }
            // If password is "***UNCHANGED***" or empty, don't modify the existing encrypted password

            existingAccount.SmtpServer = account.SmtpServer;
            existingAccount.SmtpPort = account.SmtpPort;
            existingAccount.ImapServer = account.ImapServer;
            existingAccount.ImapPort = account.ImapPort;
            existingAccount.UseSsl = account.UseSsl;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Account {Email} updated successfully", account.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account {Email}", account.Email);
            return false;
        }
    }

    /// <summary>
    /// Deletes an email account from the database.
    /// </summary>
    /// <param name="id">The database ID of the account to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the deletion was successful.</returns>
    public async Task<bool> DeleteAccountAsync(int id)
    {
        try
        {
            var account = await _context.EmailAccounts.FindAsync(id);
            if (account == null)
            {
                _logger.LogWarning("Account with id {AccountId} not found", id);
                return false;
            }

            // If this was the default account, set another one as default
            if (account.IsDefault)
            {
                var nextAccount = await _context.EmailAccounts
                    .Where(a => a.Id != id)
                    .FirstOrDefaultAsync();

                if (nextAccount != null) nextAccount.IsDefault = true;
            }

            _context.EmailAccounts.Remove(account);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Account {Email} deleted successfully", account.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account with id {AccountId}", id);
            return false;
        }
    }

    /// <summary>
    /// Sets the specified account as the default email account.
    /// </summary>
    /// <param name="id">The database ID of the account to set as default.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the operation was successful.</returns>
    public async Task<bool> SetDefaultAccountAsync(int id)
    {
        try
        {
            // Remove default from all accounts
            var allAccounts = await _context.EmailAccounts.ToListAsync();
            foreach (var account in allAccounts) account.IsDefault = false;

            // Set the specified account as default
            var targetAccount = allAccounts.FirstOrDefault(a => a.Id == id);
            if (targetAccount == null)
            {
                _logger.LogWarning("Account with id {AccountId} not found", id);
                return false;
            }

            targetAccount.IsDefault = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Account {Email} set as default", targetAccount.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default account {AccountId}", id);
            return false;
        }
    }

    /// <summary>
    /// Validates an email account's configuration and tests the connection.
    /// </summary>
    /// <param name="account">The email account to validate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains connection test results.</returns>
    public async Task<ConnectionTestResult> ValidateAccountAsync(EmailAccount account)
    {
        try
        {
            string passwordToUse;
            var isNewPassword = false;

            // Check if this is a new password from the UI or unchanged placeholder
            if (!string.IsNullOrEmpty(account.Password) && account.Password != "***UNCHANGED***")
            {
                // New password from UI - use it directly (it's plaintext)
                passwordToUse = account.Password;
                isNewPassword = true;
                _logger.LogWarning("ValidateAccount: Using new password from UI, length: {Length}",
                    passwordToUse?.Length ?? 0);
            }
            else
            {
                // No new password provided - decrypt the existing encrypted password
                if (string.IsNullOrEmpty(account.EncryptedPassword))
                    throw new ArgumentException("Account has no password set");
                passwordToUse = _encryptionService.DecryptString(account.EncryptedPassword);
                _logger.LogWarning("ValidateAccount: Using existing decrypted password, length: {Length}",
                    passwordToUse?.Length ?? 0);
            }

            // Handle legacy password data and re-encrypt if needed (only for existing passwords)
            if (!isNewPassword)
            {
                var needsReEncryption = IsLegacyPassword(account.EncryptedPassword, passwordToUse);
                if (needsReEncryption)
                {
                    _logger.LogInformation("Updating legacy password for account {Email}", account.Email);
                    var newEncryptedPassword = _encryptionService.EncryptString(passwordToUse ?? string.Empty);
                    account.EncryptedPassword = newEncryptedPassword;
                    await UpdateAccountAsync(account);
                }
            }

            // Create a test account with the password properly encrypted for the EmailService
            var encryptedPasswordForTest = isNewPassword
                ? _encryptionService.EncryptString(passwordToUse ?? string.Empty)
                : account.EncryptedPassword;

            var testAccount = new EmailAccount
            {
                Id = account.Id,
                Email = account.Email,
                DisplayName = account.DisplayName,
                EncryptedPassword = encryptedPasswordForTest,
                SmtpServer = account.SmtpServer,
                SmtpPort = account.SmtpPort,
                ImapServer = account.ImapServer,
                ImapPort = account.ImapPort,
                SmtpUseSsl = account.SmtpUseSsl,
                ImapUseSsl = account.ImapUseSsl,
                SmtpAuthMethod = account.SmtpAuthMethod,
                ImapAuthMethod = account.ImapAuthMethod
            };

            return await _emailService.TestConnectionAsync(testAccount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating account {Email}", account.Email);
            return new ConnectionTestResult
            {
                IsSuccessful = false,
                SmtpSuccess = false,
                ImapSuccess = false,
                SmtpError = "Validation failed",
                ImapError = "Validation failed"
            };
        }
    }

    /// <summary>
    /// Synchronizes folders and tags for the specified email account.
    /// </summary>
    /// <param name="accountId">The database ID of the account to synchronize.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if synchronization was successful.</returns>
    public async Task<bool> SyncAccountFoldersAndTagsAsync(int accountId)
    {
        try
        {
            var account = await _context.EmailAccounts.FindAsync(accountId);
            if (account == null)
            {
                _logger.LogWarning("Account with id {AccountId} not found for folder sync", accountId);
                return false;
            }

            await _emailService.SyncFoldersAndTagsAsync(account.Id);
            _logger.LogInformation("Folder and tag sync completed for account {Email}", account.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing folders and tags for account {AccountId}", accountId);
            return false;
        }
    }

    /// <summary>
    /// Determines if a password needs re-encryption (legacy data)
    /// </summary>
    private bool IsLegacyPassword(string? storedPassword, string? decryptedPassword)
    {
        // If the stored password equals the decrypted password, it was stored as plaintext
        if (storedPassword == null || decryptedPassword == null)
            return false;
        return string.Equals(storedPassword, decryptedPassword, StringComparison.Ordinal);
    }
}