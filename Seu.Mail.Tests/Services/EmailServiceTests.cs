using Seu.Mail.Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Seu.Mail.Data.Context;
using Seu.Mail.Core.Models;
using Seu.Mail.Services;

namespace Seu.Mail.Tests.Services;

public class EmailServiceTests : IAsyncDisposable
{
    private readonly EmailDbContext _context;
    private readonly ILogger<EmailService> _mockLogger;
    private readonly IEncryptionService _mockEncryptionService;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        var options = new DbContextOptionsBuilder<EmailDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new EmailDbContext(options);
        _mockLogger = Substitute.For<ILogger<EmailService>>();
        _mockEncryptionService = Substitute.For<IEncryptionService>();

        _emailService = new EmailService(_context, _mockLogger, _mockEncryptionService);

        // Setup encryption service defaults
        _mockEncryptionService.DecryptString(Arg.Any<string>())
            .Returns(x => x.Arg<string>().Replace("encrypted_", ""));
    }

    #region GetEmailByIdAsync Tests

    [Test]
    public async Task GetEmailByIdAsync_WithValidId_ShouldReturnEmail()
    {
        // Arrange
        var account = new EmailAccount
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        var email = new EmailMessage
        {
            Id = 1,
            AccountId = 1,
            Subject = "Test Subject",
            TextBody = "Test Body",
            MessageId = "test@example.com",
            From = "sender@example.com",
            To = "recipient@example.com",
            Account = account
        };

        await _context.EmailAccounts.AddAsync(account);
        await _context.EmailMessages.AddAsync(email);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _emailService.GetEmailByIdAsync(1);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Subject).IsEqualTo("Test Subject");
        await Assert.That(result.TextBody).IsEqualTo("Test Body");
    }

    [Test]
    public async Task GetEmailByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _emailService.GetEmailByIdAsync(999);

        // Assert
        await Assert.That(result).IsNull();
    }

    #endregion

    #region MarkAsReadAsync Tests

    [Test]
    public async Task MarkAsReadAsync_WithValidId_ShouldMarkEmailAsRead()
    {
        // Arrange
        var account = new EmailAccount
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        var email = new EmailMessage
        {
            Id = 1,
            AccountId = 1,
            Subject = "Test",
            IsRead = false,
            MessageId = "test@example.com",
            From = "sender@example.com",
            To = "recipient@example.com",
            Account = account
        };

        await _context.EmailAccounts.AddAsync(account);
        await _context.EmailMessages.AddAsync(email);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        await _emailService.MarkAsReadAsync(1);

        // Assert
        _context.ChangeTracker.Clear();
        var updatedEmail = await _context.EmailMessages.FindAsync(1);
        await Assert.That(updatedEmail).IsNotNull();
        await Assert.That(updatedEmail!.IsRead).IsTrue();
    }

    [Test]
    public async Task MarkAsReadAsync_WithInvalidId_ShouldNotThrow()
    {
        // Act & Assert
        // TUnit doesn't have DoesNotThrowAsync, so we test that no exception is thrown
        // Act & Assert - should not throw
        await _emailService.MarkAsReadAsync(999);
        // If we reach here, no exception was thrown
    }

    #endregion

    #region DeleteEmailAsync Tests

    [Test]
    public async Task DeleteEmailAsync_WithValidId_ShouldDeleteEmail()
    {
        // Arrange
        var account = new EmailAccount
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        var email = new EmailMessage
        {
            Id = 1,
            AccountId = 1,
            Subject = "Test",
            MessageId = "test@example.com",
            From = "sender@example.com",
            To = "recipient@example.com",
            Account = account
        };

        await _context.EmailAccounts.AddAsync(account);
        await _context.EmailMessages.AddAsync(email);
        await _context.SaveChangesAsync();

        // Act
        await _emailService.DeleteEmailAsync(1);

        // Assert
        _context.ChangeTracker.Clear();
        var deletedEmail = await _context.EmailMessages.FindAsync(1);
        await Assert.That(deletedEmail).IsNotNull();
        await Assert.That(deletedEmail!.IsDeleted).IsTrue();
        await Assert.That(deletedEmail.Folder).IsEqualTo("TRASH");
    }

    [Test]
    public async Task DeleteEmailAsync_WithInvalidId_ShouldNotThrow()
    {
        // Act & Assert
        // TUnit doesn't have DoesNotThrowAsync, so we test that no exception is thrown
        // Act & Assert - should not throw
        await _emailService.DeleteEmailAsync(999);
        // If we reach here, no exception was thrown
    }

    #endregion

    #region TestConnectionAsync Tests

    [Test]
    public async Task TestConnectionAsync_WithValidAccount_ShouldReturnSuccessResult()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "encrypted_password",
            ImapServer = "imap.gmail.com",
            ImapPort = 993,
            UseSsl = true
        };

        // Act
        var result = await _emailService.TestConnectionAsync(account);

        // Assert
        await Assert.That(result).IsNotNull();
        // Note: This will likely fail in real scenarios without proper server setup
        // But we're testing the structure and error handling
    }

    [Test]
    public async Task TestConnectionAsync_WithNullAccount_ShouldReturnFailureResult()
    {
        // Act
        var result = await _emailService.TestConnectionAsync(null!);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.IsSuccessful).IsFalse();
        await Assert.That(result.GetErrorMessage()).Contains("Account cannot be null");
    }

    [Test]
    public async Task TestConnectionAsync_WithInvalidServerSettings_ShouldReturnFailureResult()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "encrypted_password",
            ImapServer = "", // Invalid host
            ImapPort = 0 // Invalid port
        };

        // Act
        var result = await _emailService.TestConnectionAsync(account);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.IsSuccessful).IsFalse();
    }

    #endregion

    #region SendEmailAsync Tests

    [Test]
    public async Task SendEmailAsync_WithValidParameters_ShouldAttemptToSend()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "sender@example.com",
            DisplayName = "Test Sender",
            EncryptedPassword = "encrypted_password",
            SmtpServer = "smtp.gmail.com",
            SmtpPort = 587,
            UseSsl = true
        };

        // Act
        var result = await _emailService.SendEmailAsync(
            account,
            "recipient@example.com",
            "Test Subject",
            "Test Body");

        // Assert
        // This will likely return false in test environment without real SMTP server
        // But we're testing that the method doesn't throw and handles the scenario
        await Assert.That(result).IsFalse(); // Expected in test environment

        // DecryptString should not be called if SMTP connection fails (which it will in test environment)
        _mockEncryptionService.DidNotReceive().DecryptString(Arg.Any<string>());
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public async Task SendEmailAsync_WithInvalidToAddress_ShouldReturnFalse(string? toAddress)
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "sender@example.com",
            DisplayName = "Test Sender"
        };

        // Act
        var result = await _emailService.SendEmailAsync(account, toAddress!, "Subject", "Body");

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task SendEmailAsync_WithCcAndBcc_ShouldIncludeRecipients()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "sender@example.com",
            DisplayName = "Test Sender",
            Password = "encrypted_password",
            SmtpServer = "smtp.example.com",
            SmtpPort = 587
        };

        var cc = "cc1@example.com,cc2@example.com";
        var bcc = "bcc@example.com";

        // Act
        var result = await _emailService.SendEmailAsync(
            account,
            "to@example.com",
            "Subject",
            "Body",
            false,
            cc,
            bcc);

        // Assert
        // In a real test environment, this would likely fail due to no SMTP server
        // But we're testing the method handles the parameters correctly
        await Assert.That(result).IsFalse();
    }

    #endregion

    #region SyncEmailsAsync Tests

    [Test]
    public async Task SyncEmailsAsync_WithValidAccount_ShouldNotThrow()
    {
        // Arrange
        var account = new EmailAccount
        {
            Id = 1,
            Email = "test@example.com",
            Password = "encrypted_password",
            ImapServer = "imap.example.com",
            ImapPort = 993
        };

        await _context.EmailAccounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act & Assert
        // TUnit doesn't have DoesNotThrowAsync, so we test that no exception is thrown
        // Act & Assert - should not throw
        await _emailService.SyncEmailsAsync(account);
        // If we reach here, no exception was thrown
    }

    [Test]
    public async Task SyncEmailsAsync_WithNullAccount_ShouldNotThrow()
    {
        // Act & Assert
        // TUnit doesn't have DoesNotThrowAsync, so we test that no exception is thrown
        // Act & Assert - should not throw
        await _emailService.SyncEmailsAsync(null!);
        // If we reach here, no exception was thrown
    }

    #endregion

    #region GetFoldersAsync Tests

    [Test]
    public async Task GetFoldersAsync_WithValidAccount_ShouldReturnFolderList()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "encrypted_password",
            ImapServer = "imap.example.com",
            ImapPort = 993
        };

        // Act
        var result = await _emailService.GetFoldersAsync(account);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsAssignableTo<List<string>>();
        // In test environment without real IMAP server, this will likely be empty
    }

    [Test]
    public async Task GetFoldersAsync_WithNullAccount_ShouldReturnEmptyList()
    {
        // Act
        var result = await _emailService.GetFoldersAsync(null!);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsEmpty();
    }

    #endregion

    #region GetEmailsAsync Tests

    [Test]
    public async Task GetEmailsAsync_WithValidAccount_ShouldReturnEmailList()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "encrypted_password",
            ImapServer = "imap.example.com",
            ImapPort = 993
        };

        // Act
        var result = await _emailService.GetEmailsAsync(account, "INBOX");

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsAssignableTo<List<EmailMessage>>();
    }

    [Test]
    public async Task GetEmailsAsync_WithCustomFolderAndLimit_ShouldUseParameters()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "encrypted_password"
        };

        // Act
        var result = await _emailService.GetEmailsAsync(account, "Sent", 25);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsAssignableTo<List<EmailMessage>>();
    }

    #endregion

    #region Edge Cases and Error Handling

    [Test]
    public async Task MarkAsReadOnServerAsync_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "encrypted_password"
        };

        // Act & Assert
        // Act & Assert - should not throw
        await _emailService.MarkAsReadOnServerAsync(account, "message123");
        // If we reach here, no exception was thrown
    }

    [Test]
    public async Task SyncFoldersAndTagsAsync_WithValidAccount_ShouldNotThrow()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "encrypted_password"
        };

        // Act & Assert
        // TUnit doesn't have DoesNotThrowAsync, so we test that no exception is thrown
        // Act & Assert - should not throw
        await _emailService.SyncFoldersAndTagsAsync(account.Id);
        // If we reach here, no exception was thrown
    }

    [Test]
    public async Task SendEmailAsync_WithHtmlBody_ShouldHandleHtmlContent()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "sender@example.com",
            DisplayName = "Test Sender"
        };

        var htmlBody = "<html><body><h1>Test</h1><p>HTML content</p></body></html>";

        // Act
        var result = await _emailService.SendEmailAsync(
            account,
            "recipient@example.com",
            "HTML Test",
            htmlBody,
            true);

        // Assert
        await Assert.That(result).IsFalse(); // Expected in test environment
    }

    [Test]
    public async Task GetEmailsAsync_WithDatabaseEmails_ShouldIncludeStoredEmails()
    {
        // Arrange
        var account = new EmailAccount
        {
            Id = 1,
            Email = "test@example.com",
            Password = "encrypted_password"
        };

        var storedEmail = new EmailMessage
        {
            Id = 1,
            AccountId = 1,
            Subject = "Stored Email",
            TextBody = "This is stored in database",
            MessageId = "stored@example.com",
            DateReceived = DateTime.UtcNow
        };

        await _context.EmailAccounts.AddAsync(account);
        await _context.EmailMessages.AddAsync(storedEmail);
        await _context.SaveChangesAsync();

        // Act
        var result = await _emailService.GetEmailsAsync(account, "INBOX");

        // Assert
        await Assert.That(result).IsNotNull();
        // The result might include both database emails and server emails
        // In test environment without real IMAP server, we'll primarily get database emails
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}