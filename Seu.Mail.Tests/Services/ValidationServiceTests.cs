using Seu.Mail.Services;
using Seu.Mail.Core.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Seu.Mail.Tests.Services;

public class ValidationServiceTests
{
    private readonly ILogger<ValidationService> _mockLogger;
    private readonly ValidationService _validationService;

    public ValidationServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<ValidationService>>();
        _validationService = new ValidationService(_mockLogger);
    }

    #region Email Validation Tests

    [Test]
    [Arguments("test@example.com", true)]
    [Arguments("user.name@domain.co.uk", true)]
    [Arguments("user+tag@example.org", true)]
    [Arguments("user123@test-domain.com", true)]
    [Arguments("a@b.co", true)]
    public async Task ValidateEmail_WithValidEmails_ShouldReturnTrue(string email, bool expected)
    {
        // Act
        var result = _validationService.ValidateEmail(email);

        // Assert
        await Assert.That(result.IsValid).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", false)]
    [Arguments(null, false)]
    [Arguments("invalid-email", false)]
    [Arguments("@example.com", false)]
    [Arguments("user@", false)]
    [Arguments("user name@example.com", false)]
    [Arguments("user@example", false)]
    [Arguments("user@example", false)]
    [Arguments("user@.com", false)]
    public async Task ValidateEmail_WithInvalidEmails_ShouldReturnFalse(string? email, bool expected)
    {
        // Act
        var result = _validationService.ValidateEmail(email!);

        // Assert
        await Assert.That(result.IsValid).IsEqualTo(expected);
    }

    [Test]
    public async Task ValidateEmail_WithTooLongEmail_ShouldReturnFalse()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // Over 254 chars

        // Act
        var result = _validationService.ValidateEmail(longEmail);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.ErrorMessage).IsNotNull();
    }

    #endregion

    #region Password Validation Tests

    [Test]
    [Arguments("password", true)]
    [Arguments("Password123!", true)]
    [Arguments("simple", true)]
    [Arguments("MySecure1@", true)]
    public async Task ValidatePassword_WithValidPasswords_ShouldReturnTrue(string password, bool expected)
    {
        // Act
        var result = _validationService.ValidatePassword(password);

        // Assert
        await Assert.That(result.IsValid).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", false)]
    [Arguments(null, false)]
    public async Task ValidatePassword_WithInvalidPasswords_ShouldReturnFalse(string? password, bool expected)
    {
        // Act
        var result = _validationService.ValidatePassword(password!);

        // Assert
        await Assert.That(result.IsValid).IsEqualTo(expected);
        await Assert.That(result.ErrorMessage).IsNotNull();
    }

    [Test]
    public async Task ValidatePassword_WithTooLongPassword_ShouldReturnFalse()
    {
        // Arrange
        var longPassword = new string('A', 126) + "a1!"; // 129 chars - over 128 limit

        // Act
        var result = _validationService.ValidatePassword(longPassword);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.ErrorMessage).Contains("too long");
    }

    #endregion

    #region Account Validation Tests

    [Test]
    public async Task ValidateEmailAccount_WithValidAccount_ShouldReturnTrue()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "Password123!",
            ImapServer = "imap.gmail.com",
            ImapPort = 993,
            SmtpServer = "smtp.gmail.com",
            SmtpPort = 587,
            UseSsl = true
        };

        // Act
        var result = _validationService.ValidateAccountData(account);

        // Assert
        await Assert.That(result.IsValid).IsTrue();
    }

    [Test]
    public async Task ValidateEmailAccount_WithNullAccount_ShouldReturnFalse()
    {
        // Act
        var result = _validationService.ValidateAccountData(null!);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.ErrorMessage).Contains("Account cannot be null");
    }

    [Test]
    public async Task ValidateEmailAccount_WithInvalidEmail_ShouldReturnFalse()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "invalid-email",
            Password = "Password123!",
            ImapServer = "imap.gmail.com",
            ImapPort = 993
        };

        // Act
        var result = _validationService.ValidateAccountData(account);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.ErrorMessage).Contains("email");
    }

    [Test]
    public async Task ValidateEmailAccount_WithEmptyPassword_ShouldReturnTrue()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "", // Empty password is allowed since passwords are managed on server
            ImapServer = "imap.gmail.com",
            ImapPort = 993,
            SmtpServer = "smtp.gmail.com",
            SmtpPort = 587
        };

        // Act
        var result = _validationService.ValidateAccountData(account);

        // Assert
        await Assert.That(result.IsValid).IsTrue();
    }

    [Test]
    public async Task ValidateEmailAccount_WithInvalidServerSettings_ShouldReturnFalse()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "Password123!",
            ImapServer = "", // Invalid server
            ImapPort = 0 // Invalid port
        };

        // Act
        var result = _validationService.ValidateAccountData(account);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.ErrorMessage).Contains("Server", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Server Settings Validation Tests

    [Test]
    [Arguments("smtp.gmail.com", 587, true)]
    [Arguments("imap.outlook.com", 993, true)]
    [Arguments("mail.example.org", 465, true)]
    public async Task ValidateServerSettings_WithValidSettings_ShouldReturnTrue(string server, int port, bool expected)
    {
        // Act
        var result = _validationService.ValidateServerSettings(server, port);

        // Assert
        await Assert.That(result.IsValid).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", 587, false)]
    [Arguments(null, 587, false)]
    [Arguments("invalid_server", 587, false)]
    [Arguments("smtp.gmail.com", 0, false)]
    [Arguments("smtp.gmail.com", 70000, false)]
    public async Task ValidateServerSettings_WithInvalidServers_ShouldReturnFalse(string? server, int port,
        bool expected)
    {
        // Act
        var result = _validationService.ValidateServerSettings(server!, port);

        // Assert
        await Assert.That(result.IsValid).IsEqualTo(expected);
    }

    [Test]
    public async Task ValidateServerSettings_WithCommonPorts_ShouldReturnTrue()
    {
        // Test common email ports
        var commonPorts = new[] { 25, 465, 587, 993, 995, 143, 110 };

        foreach (var port in commonPorts)
        {
            // Act
            var result = _validationService.ValidateServerSettings("mail.example.com", port);

            // Assert
            await Assert.That(result.IsValid).IsTrue();
        }
    }

    #endregion

    #region Edge Cases and Error Handling

    [Test]
    public async Task ValidateEmail_WithUnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var unicodeEmail = "tëst@example.com";

        // Act
        var result = _validationService.ValidateEmail(unicodeEmail);

        // Assert
        await Assert.That(result.IsValid).IsTrue();
    }

    [Test]
    public async Task ValidateEmail_WithMultipleDots_ShouldHandleCorrectly()
    {
        // Arrange
        var emailWithMultipleDots = "user.name.lastname@sub.domain.example.com";

        // Act
        var result = _validationService.ValidateEmail(emailWithMultipleDots);

        // Assert
        await Assert.That(result.IsValid).IsTrue();
    }

    [Test]
    public async Task ValidatePassword_WithAllRequirements_ShouldReturnDetailedResult()
    {
        // Arrange
        var strongPassword = "MyVeryStr0ng!Password";

        // Act
        var result = _validationService.ValidatePassword(strongPassword);

        // Assert
        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.ErrorMessage).IsEmpty();
    }

    [Test]
    public async Task ValidateServerSettings_WithIPAddress_ShouldReturnTrue()
    {
        // Arrange
        var ipServer = "192.168.1.100";

        // Act
        var result = _validationService.ValidateServerSettings(ipServer, 587);

        // Assert
        await Assert.That(result.IsValid).IsTrue();
    }

    [Test]
    public async Task ValidateEmail_WithPlusAddressing_ShouldReturnTrue()
    {
        // Arrange
        var plusEmail = "user+filter@example.com";

        // Act
        var result = _validationService.ValidateEmail(plusEmail);

        // Assert
        await Assert.That(result.IsValid).IsTrue();
    }

    #endregion

    #region Performance Tests

    [Test]
    public async Task ValidateEmail_WithLargeNumberOfEmails_ShouldPerformEfficiently()
    {
        // Arrange
        var emails = new List<string>();
        for (var i = 0; i < 50; i++) emails.Add($"user{i}@example.com");

        // Act & Assert
        foreach (var email in emails)
        {
            var result = _validationService.ValidateEmail(email);
            await Assert.That(result.IsValid).IsTrue();
        }
    }

    [Test]
    public async Task ValidatePassword_WithLargeNumberOfPasswords_ShouldPerformEfficiently()
    {
        // Arrange
        var passwords = new List<string>();
        for (var i = 0; i < 50; i++) passwords.Add($"Password{i}!");

        // Act & Assert
        foreach (var password in passwords)
        {
            var result = _validationService.ValidatePassword(password);
            await Assert.That(result.IsValid).IsTrue();
        }
    }

    #endregion

    #region Security Tests

    [Test]
    public async Task ValidateEmail_WithSqlInjectionAttempts_ShouldRejectMaliciousInput()
    {
        // Arrange - SQL injection attempts
        var maliciousEmails = new[]
        {
            "test@example.com'; DROP TABLE users; --",
            "user@domain.com' OR '1'='1",
            "admin@test.com'; DELETE FROM emails; --",
            "test@evil.com' UNION SELECT * FROM passwords --"
        };

        foreach (var email in maliciousEmails)
        {
            // Act
            var result = _validationService.ValidateEmail(email);

            // Assert - Should be invalid
            await Assert.That(result.IsValid).IsFalse();
            await Assert.That(result.ErrorMessage).IsNotEmpty();
        }
    }

    [Test]
    public async Task ValidatePassword_WithScriptInjectionAttempts_ShouldAcceptAnyNonEmptyPassword()
    {
        // Arrange - Script injection attempts (now accepted since we don't validate strength)
        var passwords = new[]
        {
            "<script>alert('xss')</script>",
            "javascript:alert('xss')",
            "<img src=x onerror=alert('xss')>",
            "'; DROP TABLE users; --",
            "<?php system($_GET['cmd']); ?>"
        };

        foreach (var password in passwords)
        {
            // Act
            var result = _validationService.ValidatePassword(password);

            // Assert - Should be valid since we only check for non-empty and length limits
            await Assert.That(result.IsValid).IsTrue();
        }
    }

    [Test]
    public async Task ValidateServerSettings_WithMaliciousHostnames_ShouldRejectDangerousInputs()
    {
        // Arrange - Various malicious hostname attempts
        var maliciousHostnames = new[]
        {
            "localhost'; rm -rf /; --",
            "127.0.0.1' OR '1'='1",
            "evil.com<script>alert('xss')</script>",
            "test.com\"; system('rm -rf /'); \"",
            "hostname`rm -rf /`evil.com"
        };

        foreach (var hostname in maliciousHostnames)
        {
            // Act
            var result = _validationService.ValidateServerSettings(hostname, 587);

            // Assert - Should be invalid
            await Assert.That(result.IsValid).IsFalse();
            await Assert.That(result.ErrorMessage).IsNotEmpty();
        }
    }

    [Test]
    public async Task ValidateAccountData_WithMaliciousData_ShouldSafelyReject()
    {
        // Arrange
        var maliciousAccount = new EmailAccount
        {
            Email = "test@example.com<script>alert('xss')</script>",
            DisplayName = "'; DROP TABLE accounts; --",
            ImapServer = "localhost`rm -rf /`",
            SmtpServer = "smtp.evil.com\"; system('hack'); \"",
            ImapPort = -1,
            SmtpPort = 999999
        };

        // Act
        var result = _validationService.ValidateAccountData(maliciousAccount);

        // Assert - Should safely reject malformed data
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.ErrorMessage).IsNotEmpty();
    }

    #endregion
}