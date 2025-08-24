using Seu.Mail.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Seu.Mail.Tests.Models;

/// <summary>
/// Tests for EmailAccount model including validation, property assignments,
/// and business logic validation
/// </summary>
public class EmailAccountTests
{
    // Basic Property Tests

    [Test]
    public async Task EmailAccount_DefaultConstruction_ShouldHaveExpectedDefaults()
    {
        // Act
        var account = new EmailAccount();

        // Assert
        await Assert.That(account.Id).IsEqualTo(0);
        await Assert.That(account.Email).IsEqualTo(string.Empty);
        await Assert.That(account.DisplayName).IsEqualTo(string.Empty);
        await Assert.That(account.Password).IsEqualTo(string.Empty);
        await Assert.That(account.SmtpServer).IsEqualTo(string.Empty);
        await Assert.That(account.SmtpPort).IsEqualTo(587);
        await Assert.That(account.ImapServer).IsEqualTo(string.Empty);
        await Assert.That(account.ImapPort).IsEqualTo(993);
        await Assert.That(account.UseSsl).IsTrue();
    }

    [Test]
    public async Task EmailAccount_PropertyAssignment_ShouldWorkCorrectly()
    {
        // Arrange
        var account = new EmailAccount();
        var testEmail = "test@example.com";
        var testPassword = "securePassword123";

        // Act
        account.Id = 1;
        account.Email = testEmail;
        account.Password = testPassword;
        account.SmtpServer = "smtp.example.com";
        account.ImapServer = "imap.example.com";

        // Assert
        await Assert.That(account.Id).IsEqualTo(1);
        await Assert.That(account.Email).IsEqualTo(testEmail);
        await Assert.That(account.Password).IsEqualTo(testPassword);
        await Assert.That(account.SmtpServer).IsEqualTo("smtp.example.com");
        await Assert.That(account.ImapServer).IsEqualTo("imap.example.com");
    }

    // Email Validation Tests

    [Test]
    [Arguments("test@example.com")]
    [Arguments("user.name@domain.co.uk")]
    [Arguments("firstname+lastname@company.org")]
    public async Task EmailAccount_WithValidEmail_ShouldPassValidation(string validEmail)
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = validEmail,
            Password = "password123",
            SmtpServer = "smtp.example.com",
            ImapServer = "imap.example.com"
        };

        // Act
        var validationResults = ValidateModel(account);

        // Assert
        await Assert.That(validationResults.Where(r => r.MemberNames.Contains("Email"))).IsEmpty();
    }

    [Test]
    [Arguments("invalid-email")]
    [Arguments("@domain.com")]
    [Arguments("test@")]
    public async Task EmailAccount_WithInvalidEmail_ShouldFailValidation(string invalidEmail)
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = invalidEmail,
            Password = "password123",
            SmtpServer = "smtp.example.com",
            ImapServer = "imap.example.com"
        };

        // Act
        var validationResults = ValidateModel(account);

        // Assert
        await Assert.That(validationResults.Any(r => r.MemberNames.Contains("Email"))).IsTrue();
    }

    // Required Field Validation Tests

    [Test]
    public async Task EmailAccount_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "valid@example.com",
            DisplayName = "Valid User",
            Password = "securePassword123",
            SmtpServer = "smtp.example.com",
            ImapServer = "imap.example.com"
        };

        // Act
        var validationResults = ValidateModel(account);

        // Assert
        await Assert.That(validationResults).IsEmpty();
    }

    [Test]
    public async Task EmailAccount_WithEmptyPassword_ShouldFailValidation()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "",
            SmtpServer = "smtp.example.com",
            ImapServer = "imap.example.com"
        };

        // Act
        var validationResults = ValidateModel(account);

        // Assert
        await Assert.That(validationResults.Any(r => r.MemberNames.Contains("EncryptedPassword"))).IsTrue();
    }

    [Test]
    public async Task EmailAccount_WithEmptyServers_ShouldFailValidation()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "password123",
            SmtpServer = "",
            ImapServer = ""
        };

        // Act
        var validationResults = ValidateModel(account);

        // Assert
        await Assert.That(validationResults.Any(r => r.MemberNames.Contains("SmtpServer"))).IsTrue();
        await Assert.That(validationResults.Any(r => r.MemberNames.Contains("ImapServer"))).IsTrue();
    }

    // Port Configuration Tests

    [Test]
    [Arguments(25, 143)] // SMTP/IMAP standard
    [Arguments(465, 993)] // SSL ports
    [Arguments(587, 995)] // TLS/POP3 ports
    public async Task EmailAccount_WithValidPorts_ShouldBeValid(int smtpPort, int imapPort)
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "password123",
            SmtpServer = "smtp.example.com",
            SmtpPort = smtpPort,
            ImapServer = "imap.example.com",
            ImapPort = imapPort
        };

        // Act
        var validationResults = ValidateModel(account);

        // Assert
        await Assert.That(account.SmtpPort).IsEqualTo(smtpPort);
        await Assert.That(account.ImapPort).IsEqualTo(imapPort);
        await Assert
            .That(validationResults.Where(r =>
                r.MemberNames.Contains("SmtpPort") || r.MemberNames.Contains("ImapPort"))).IsEmpty();
    }

    // SSL Configuration Tests

    [Test]
    public async Task EmailAccount_WithSslConfiguration_ShouldWorkCorrectly()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "test@example.com",
            Password = "password123",
            SmtpServer = "smtp.example.com",
            ImapServer = "imap.example.com",
            UseSsl = false
        };

        // Act
        var validationResults = ValidateModel(account);

        // Assert
        await Assert.That(validationResults).IsEmpty();
        await Assert.That(account.UseSsl).IsFalse();
    }

    // Provider Configuration Tests

    [Test]
    public async Task EmailAccount_WithGmailConfiguration_ShouldBeValid()
    {
        // Arrange
        var account = new EmailAccount
        {
            Email = "user@gmail.com",
            Password = "appspecificpassword",
            SmtpServer = "smtp.gmail.com",
            SmtpPort = 587,
            ImapServer = "imap.gmail.com",
            ImapPort = 993,
            UseSsl = true
        };

        // Act
        var validationResults = ValidateModel(account);

        // Assert
        await Assert.That(validationResults).IsEmpty();
        await Assert.That(account.SmtpServer).IsEqualTo("smtp.gmail.com");
        await Assert.That(account.ImapServer).IsEqualTo("imap.gmail.com");
    }

    // Performance Tests

    [Test]
    public async Task EmailAccount_Creation_ShouldBeEfficient()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (var i = 0; i < 1000; i++)
        {
            var account = new EmailAccount
            {
                Email = $"user{i}@example.com",
                Password = $"password{i}",
                SmtpServer = "smtp.example.com",
                ImapServer = "imap.example.com"
            };
        }

        stopwatch.Stop();

        // Assert
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(100);
    }

    // Helper Methods

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}