using Seu.Mail.Core.Models;
using Seu.Mail.Services;
using Seu.Mail.Tests.TestHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Seu.Mail.Tests.Services;

/// <summary>
/// Basic tests to verify the test framework and architecture migration
/// </summary>
public class BasicTests
{
    [Test]
    public async Task Test_Framework_IsWorking()
    {
        // Arrange
        var testValue = 42;

        // Act
        var result = testValue * 2;

        // Assert
        await Assert.That(result).IsEqualTo(84);
    }

    [Test]
    public async Task EncryptionService_CanBeInstantiated()
    {
        // Arrange
        var configuration = Substitute.For<IConfiguration>();
        var logger = Substitute.For<ILogger<EncryptionService>>();

        configuration["Security:EncryptionKey"].Returns("VGVzdEVuY3J5cHRpb25LZXkxMjM0NTY3ODkwMTIzNDU2Nzg5MDEyMw==");
        configuration["Security:EncryptionIV"].Returns("VGVzdEVuY3J5cHRpb25JVjEyMw==");

        // Act
        var encryptionService = new EncryptionService(configuration, logger);

        // Assert
        await Assert.That(encryptionService).IsNotNull();
    }

    [Test]
    public async Task EmailAccount_CanBeCreated()
    {
        // Arrange & Act
        var account = new EmailAccount
        {
            Email = "test@example.com",
            DisplayName = "Test Account",
            EncryptedPassword = "encrypted_password",
            SmtpServer = "smtp.example.com",
            SmtpPort = 587,
            ImapServer = "imap.example.com",
            ImapPort = 993
        };

        // Assert
        await Assert.That(account.Email).IsEqualTo("test@example.com");
        await Assert.That(account.DisplayName).IsEqualTo("Test Account");
        await Assert.That(account.SmtpServer).IsEqualTo("smtp.example.com");
        await Assert.That(account.ImapServer).IsEqualTo("imap.example.com");
    }

    [Test]
    public async Task EncryptionService_CanEncryptAndDecrypt()
    {
        // Arrange
        var configuration = TestConfigurationHelper.CreateTestConfiguration();
        var logger = Substitute.For<ILogger<EncryptionService>>();

        var encryptionService = new EncryptionService(configuration, logger);
        var originalText = "Hello World!";

        // Act
        var encrypted = encryptionService.EncryptString(originalText);
        var decrypted = encryptionService.DecryptString(encrypted);

        // Assert
        await Assert.That(encrypted).IsNotEqualTo(originalText);
        await Assert.That(decrypted).IsEqualTo(originalText);
    }
}
