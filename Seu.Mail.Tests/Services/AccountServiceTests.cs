using Seu.Mail.Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Seu.Mail.Data.Context;
using Seu.Mail.Core.Models;
using Seu.Mail.Core.Models.Shared;
using Seu.Mail.Services;

namespace Seu.Mail.Tests.Services;

public class AccountServiceTests : IAsyncDisposable
{
    private readonly EmailDbContext _context;
    private readonly IEmailService _mockEmailService;
    private readonly ILogger<AccountService> _mockLogger;
    private readonly IEncryptionService _mockEncryptionService;
    private readonly IValidationService _mockValidationService;
    private readonly AccountService _accountService;

    public AccountServiceTests()
    {
        var options = new DbContextOptionsBuilder<EmailDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new EmailDbContext(options);
        _mockEmailService = Substitute.For<IEmailService>();
        _mockLogger = Substitute.For<ILogger<AccountService>>();
        _mockEncryptionService = Substitute.For<IEncryptionService>();
        _mockValidationService = Substitute.For<IValidationService>();

        _accountService = new AccountService(
            _context,
            _mockEmailService,
            _mockLogger,
            _mockEncryptionService,
            _mockValidationService);

        // Setup encryption service defaults
        _mockEncryptionService.EncryptString(Arg.Any<string>())
            .Returns(x => "encrypted_" + x.Arg<string>());
        _mockEncryptionService.DecryptString(Arg.Any<string>())
            .Returns(x => x.Arg<string>().Replace("encrypted_", ""));

        // Setup validation service defaults
        _mockValidationService.SanitizeInput(Arg.Any<string>())
            .Returns(x => x.Arg<string>());
        _mockValidationService.ValidateAccountData(Arg.Any<EmailAccount>())
            .Returns(new InputValidationResult(true));
    }

    #region GetAllAccountsAsync Tests

    [Test]
    public async Task GetAllAccountsAsync_WithNoAccounts_ShouldReturnEmptyList()
    {
        // Act
        var result = await _accountService.GetAllAccountsAsync();

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GetAllAccountsAsync_WithAccounts_ShouldReturnAllAccounts()
    {
        // Arrange
        var accounts = new List<EmailAccount>
        {
            new() { Id = 1, Email = "test1@example.com", DisplayName = "Test 1" },
            new() { Id = 2, Email = "test2@example.com", DisplayName = "Test 2" }
        };

        await _context.EmailAccounts.AddRangeAsync(accounts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _accountService.GetAllAccountsAsync();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result.Any(a => a.Email == "test1@example.com")).IsTrue();
        await Assert.That(result.Any(a => a.Email == "test2@example.com")).IsTrue();
    }

    #endregion

    #region GetAccountByIdAsync Tests

    [Test]
    public async Task GetAccountByIdAsync_WithValidId_ShouldReturnAccount()
    {
        // Arrange
        var account = new EmailAccount
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        await _context.EmailAccounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _accountService.GetAccountByIdAsync(1);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Email).IsEqualTo("test@example.com");
        await Assert.That(result.DisplayName).IsEqualTo("Test User");
    }

    [Test]
    public async Task GetAccountByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _accountService.GetAccountByIdAsync(999);

        // Assert
        await Assert.That(result).IsNull();
    }

    #endregion

    #region GetDefaultAccountAsync Tests

    [Test]
    public async Task GetDefaultAccountAsync_WithDefaultAccount_ShouldReturnDefaultAccount()
    {
        // Arrange
        var accounts = new List<EmailAccount>
        {
            new() { Id = 1, Email = "test1@example.com", DisplayName = "Test 1", IsDefault = false },
            new() { Id = 2, Email = "test2@example.com", DisplayName = "Test 2", IsDefault = true }
        };

        await _context.EmailAccounts.AddRangeAsync(accounts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _accountService.GetDefaultAccountAsync();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Id).IsEqualTo(2);
        await Assert.That(result.Email).IsEqualTo("test2@example.com");
    }

    [Test]
    public async Task GetDefaultAccountAsync_WithNoDefaultAccount_ShouldReturnFirstAccount()
    {
        // Arrange
        var accounts = new List<EmailAccount>
        {
            new() { Id = 1, Email = "test1@example.com", DisplayName = "Test 1", IsDefault = false },
            new() { Id = 2, Email = "test2@example.com", DisplayName = "Test 2", IsDefault = false }
        };

        await _context.EmailAccounts.AddRangeAsync(accounts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _accountService.GetDefaultAccountAsync();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Id).IsEqualTo(1);
    }

    [Test]
    public async Task GetDefaultAccountAsync_WithNoAccounts_ShouldReturnNull()
    {
        // Act
        var result = await _accountService.GetDefaultAccountAsync();

        // Assert
        await Assert.That(result).IsNull();
    }

    #endregion

    #region AddAccountAsync Tests

    [Test]
    public async Task AddAccountAsync_WithValidAccount_ShouldReturnTrueAndEncryptPassword()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            DisplayName = "Test Account",
            Password = "plainPassword",
            ImapServer = "imap.example.com",
            ImapPort = 993,
            SmtpServer = "smtp.example.com",
            SmtpPort = 587
        };

        _mockValidationService.ValidateAccountData(account).Returns(new InputValidationResult(true));

        // Act
        var result = await _accountService.AddAccountAsync(account);

        // Assert
        await Assert.That(result).IsTrue();

        var savedAccount = await _context.EmailAccounts.FirstOrDefaultAsync();
        await Assert.That(savedAccount).IsNotNull();
        await Assert.That(savedAccount!.Password).IsEqualTo("encrypted_plainPassword");

        _mockEncryptionService.Received(1).EncryptString("plainPassword");
    }

    [Test]
    public async Task AddAccountAsync_WithInvalidAccount_ShouldReturnFalse()
    {
        // Arrange
        var account = new EmailAccount { Email = "invalid-email" };
        _mockValidationService.ValidateAccountData(account)
            .Returns(new InputValidationResult(false, "Invalid email"));

        // Act
        var result = await _accountService.AddAccountAsync(account);

        // Assert
        await Assert.That(result).IsFalse();

        var accountCount = await _context.EmailAccounts.CountAsync();
        await Assert.That(accountCount).IsEqualTo(0);
    }

    [Test]
    public async Task AddAccountAsync_WhenFirstAccount_ShouldSetAsDefault()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            DisplayName = "Test Account",
            Password = "password"
        };

        _mockValidationService.ValidateAccountData(account).Returns(new InputValidationResult(true));

        // Act
        var result = await _accountService.AddAccountAsync(account);

        // Assert
        await Assert.That(result).IsTrue();

        var savedAccount = await _context.EmailAccounts.FirstOrDefaultAsync();
        await Assert.That(savedAccount).IsNotNull();
        await Assert.That(savedAccount!.IsDefault).IsTrue();
    }

    [Test]
    public async Task AddAccountAsync_WhenNotFirstAccount_ShouldNotSetAsDefault()
    {
        // Arrange
        var existingAccount = new EmailAccount
        {
            Email = "existing@example.com",
            DisplayName = "Existing",
            IsDefault = true
        };
        await _context.EmailAccounts.AddAsync(existingAccount);
        await _context.SaveChangesAsync();

        var newAccount = new EmailAccount
        {
            Email = "new@example.com",
            DisplayName = "New Account",
            Password = "password"
        };

        _mockValidationService.ValidateAccountData(newAccount).Returns(new InputValidationResult(true));

        // Act
        var result = await _accountService.AddAccountAsync(newAccount);

        // Assert
        await Assert.That(result).IsTrue();

        var savedNewAccount = await _context.EmailAccounts
            .FirstOrDefaultAsync(x => x.Email == "new@example.com");
        await Assert.That(savedNewAccount).IsNotNull();
        await Assert.That(savedNewAccount!.IsDefault).IsFalse();
    }

    #endregion

    #region UpdateAccountAsync Tests

    [Test]
    public async Task UpdateAccountAsync_WithValidAccount_ShouldReturnTrueAndUpdateAccount()
    {
        // Arrange
        var account = new EmailAccount
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Original Name",
            Password = "encrypted_originalPassword",
            SmtpServer = "smtp.example.com",
            SmtpPort = 587,
            ImapServer = "imap.example.com",
            ImapPort = 993
        };
        await _context.EmailAccounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Create a new account object with updated values
        var updatedAccount = new EmailAccount
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Updated Name",
            Password = "newPassword",
            SmtpServer = "smtp.example.com",
            SmtpPort = 587,
            ImapServer = "imap.example.com",
            ImapPort = 993
        };

        // Act
        var result = await _accountService.UpdateAccountAsync(updatedAccount);

        // Assert
        await Assert.That(result).IsTrue();

        var savedAccount = await _context.EmailAccounts.FindAsync(1);
        await Assert.That(savedAccount).IsNotNull();
        await Assert.That(savedAccount!.DisplayName).IsEqualTo("Updated Name");
        await Assert.That(savedAccount.Password).IsEqualTo("encrypted_newPassword");

        _mockEncryptionService.Received(1).EncryptString("newPassword");
    }

    [Test]
    public async Task UpdateAccountAsync_WithInvalidAccount_ShouldReturnFalse()
    {
        // Arrange
        var account = new EmailAccount { Id = 1, Email = "invalid-email" };
        _mockValidationService.ValidateAccountData(account)
            .Returns(new InputValidationResult(false, "Invalid email"));

        // Act
        var result = await _accountService.UpdateAccountAsync(account);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task UpdateAccountAsync_WithNonExistentAccount_ShouldReturnFalse()
    {
        // Arrange
        var account = new EmailAccount { Id = 999, Email = "test@example.com" };
        _mockValidationService.ValidateAccountData(account).Returns(new InputValidationResult(true));

        // Act
        var result = await _accountService.UpdateAccountAsync(account);

        // Assert
        await Assert.That(result).IsFalse();
    }

    #endregion

    #region DeleteAccountAsync Tests

    [Test]
    public async Task DeleteAccountAsync_WithValidId_ShouldReturnTrueAndDeleteAccount()
    {
        // Arrange
        var account = new EmailAccount { Id = 1, Email = "test@example.com" };
        await _context.EmailAccounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _accountService.DeleteAccountAsync(1);

        // Assert
        await Assert.That(result).IsTrue();

        var deletedAccount = await _context.EmailAccounts.FindAsync(1);
        await Assert.That(deletedAccount).IsNull();
    }

    [Test]
    public async Task DeleteAccountAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _accountService.DeleteAccountAsync(999);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task DeleteAccountAsync_WhenDeletingDefaultAccount_ShouldSetNewDefault()
    {
        // Arrange
        var accounts = new List<EmailAccount>
        {
            new() { Id = 1, Email = "test1@example.com", IsDefault = true },
            new() { Id = 2, Email = "test2@example.com", IsDefault = false }
        };

        await _context.EmailAccounts.AddRangeAsync(accounts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _accountService.DeleteAccountAsync(1);

        // Assert
        await Assert.That(result).IsTrue();

        var remainingAccount = await _context.EmailAccounts.FindAsync(2);
        await Assert.That(remainingAccount).IsNotNull();
        await Assert.That(remainingAccount!.IsDefault).IsTrue();
    }

    #endregion

    #region SetDefaultAccountAsync Tests

    [Test]
    public async Task SetDefaultAccountAsync_WithValidId_ShouldReturnTrueAndSetDefault()
    {
        // Arrange
        var accounts = new List<EmailAccount>
        {
            new() { Id = 1, Email = "test1@example.com", IsDefault = true },
            new() { Id = 2, Email = "test2@example.com", IsDefault = false }
        };

        await _context.EmailAccounts.AddRangeAsync(accounts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _accountService.SetDefaultAccountAsync(2);

        // Assert
        await Assert.That(result).IsTrue();

        var account1 = await _context.EmailAccounts.FindAsync(1);
        var account2 = await _context.EmailAccounts.FindAsync(2);

        await Assert.That(account1!.IsDefault).IsFalse();
        await Assert.That(account2!.IsDefault).IsTrue();
    }

    [Test]
    public async Task SetDefaultAccountAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _accountService.SetDefaultAccountAsync(999);

        // Assert
        await Assert.That(result).IsFalse();
    }

    #endregion

    #region ValidateAccountAsync Tests

    [Test]
    public async Task ValidateAccountAsync_WithValidAccount_ShouldReturnSuccessResult()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "encrypted_password",
            ImapServer = "imap.example.com",
            ImapPort = 993
        };

        var expectedResult = new ConnectionTestResult
        {
            IsSuccessful = true,
            SmtpSuccess = true,
            ImapSuccess = true
        };

        _mockEmailService.TestConnectionAsync(Arg.Any<EmailAccount>()).Returns(expectedResult);

        // Act
        var result = await _accountService.ValidateAccountAsync(account);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.IsSuccessful).IsTrue();

        _ = _mockEmailService.Received(1).TestConnectionAsync(Arg.Any<EmailAccount>());
    }

    [Test]
    public async Task ValidateAccountAsync_WithInvalidAccount_ShouldReturnFailureResult()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "encrypted_password"
        };

        var expectedResult = new ConnectionTestResult
        {
            IsSuccessful = false,
            SmtpSuccess = false,
            ImapSuccess = false
        };
        expectedResult.GeneralError = "Connection failed";

        _mockEmailService.TestConnectionAsync(Arg.Any<EmailAccount>()).Returns(expectedResult);

        // Act
        var result = await _accountService.ValidateAccountAsync(account);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.IsSuccessful).IsFalse();
        await Assert.That(result.GetErrorMessage()).Contains("failed");
    }

    [Test]
    public async Task ValidateAccountAsync_WhenExceptionThrown_ShouldReturnFailureResult()
    {
        // Arrange
        var account = new EmailAccount { Email = "test@example.com" };
        _mockEmailService.TestConnectionAsync(Arg.Any<EmailAccount>())
            .Returns(Task.FromException<ConnectionTestResult>(new Exception("Connection error")));

        // Act
        var result = await _accountService.ValidateAccountAsync(account);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.IsSuccessful).IsFalse();
    }

    #endregion

    #region SyncAccountFoldersAndTagsAsync Tests

    [Test]
    public async Task SyncAccountFoldersAndTagsAsync_WithValidAccount_ShouldReturnTrue()
    {
        // Arrange
        var account = new EmailAccount { Id = 1, Email = "test@example.com" };
        await _context.EmailAccounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _accountService.SyncAccountFoldersAndTagsAsync(1);

        // Assert
        await Assert.That(result).IsTrue();
        _ = _mockEmailService.Received(1).SyncFoldersAndTagsAsync(account.Id);
    }

    [Test]
    public async Task SyncAccountFoldersAndTagsAsync_WithInvalidAccountId_ShouldReturnFalse()
    {
        // Act
        var result = await _accountService.SyncAccountFoldersAndTagsAsync(999);

        // Assert
        await Assert.That(result).IsFalse();
        await _mockEmailService.DidNotReceive().SyncFoldersAndTagsAsync(Arg.Any<int>());
    }

    [Test]
    public async Task SyncAccountFoldersAndTagsAsync_WhenExceptionThrown_ShouldReturnFalseAndLogError()
    {
        // Arrange
        var account = new EmailAccount { Id = 1, Email = "test@example.com" };
        await _context.EmailAccounts.AddAsync(account);
        await _context.SaveChangesAsync();

        _mockEmailService.SyncFoldersAndTagsAsync(account.Id).Returns(Task.FromException(new Exception("Sync error")));

        // Act
        var result = await _accountService.SyncAccountFoldersAndTagsAsync(1);

        // Assert
        await Assert.That(result).IsFalse();

        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Error syncing folders and tags")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion

    #region Edge Cases

    [Test]
    public async Task AddAccountAsync_WhenDatabaseSaveThrows_ShouldReturnFalse()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            DisplayName = "Test",
            Password = "password"
        };

        _mockValidationService.ValidateAccountData(account).Returns(new InputValidationResult(true));

        // Simulate database save failure by disposing context
        await _context.DisposeAsync();

        // Act
        var result = await _accountService.AddAccountAsync(account);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task UpdateAccountAsync_WithNullPassword_ShouldNotEncrypt()
    {
        // Arrange
        var account = new EmailAccount
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Original Name",
            Password = "original_password"
        };
        await _context.EmailAccounts.AddAsync(account);
        await _context.SaveChangesAsync();

        account.DisplayName = "Updated Name";
        account.Password = "original_password"; // Same password, should not trigger encryption
        _mockValidationService.ValidateAccountData(account).Returns(new InputValidationResult(true));

        // Act
        var result = await _accountService.UpdateAccountAsync(account);

        // Assert
        await Assert.That(result).IsTrue();

        var updatedAccount = await _context.EmailAccounts.FindAsync(1);
        await Assert.That(updatedAccount).IsNotNull();
        await Assert.That(updatedAccount!.DisplayName).IsEqualTo("Updated Name");

        _mockEncryptionService.DidNotReceive().EncryptString(Arg.Any<string>());
    }

    [Test]
    public async Task DeleteAccountAsync_WithRelatedEmails_ShouldDeleteCascade()
    {
        // Arrange
        var account = new EmailAccount { Id = 1, Email = "test@example.com" };
        var email1 = new EmailMessage { Id = 1, AccountId = 1, Subject = "Test 1", MessageId = "msg1@example.com" };
        var email2 = new EmailMessage { Id = 2, AccountId = 1, Subject = "Test 2", MessageId = "msg2@example.com" };

        await _context.EmailAccounts.AddAsync(account);
        await _context.EmailMessages.AddRangeAsync(email1, email2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _accountService.DeleteAccountAsync(1);

        // Assert
        await Assert.That(result).IsTrue();

        var deletedAccount = await _context.EmailAccounts.FindAsync(1);
        await Assert.That(deletedAccount).IsNull();

        var remainingEmails = await _context.EmailMessages.Where(e => e.AccountId == 1).ToListAsync();
        await Assert.That(remainingEmails).IsEmpty();
    }

    #endregion

    #region Performance Tests

    [Test]
    public async Task GetAllAccountsAsync_WithLargeNumberOfAccounts_ShouldPerformWell()
    {
        // Arrange
        var accounts = Enumerable.Range(1, 100)
            .Select(i => new EmailAccount { Id = i, Email = $"test{i}@example.com", DisplayName = $"Test {i}" })
            .ToList();

        await _context.EmailAccounts.AddRangeAsync(accounts);
        await _context.SaveChangesAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _accountService.GetAllAccountsAsync();
        stopwatch.Stop();

        // Assert
        await Assert.That(result.Count).IsEqualTo(100);
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(5000); // Should complete within 5 seconds
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}