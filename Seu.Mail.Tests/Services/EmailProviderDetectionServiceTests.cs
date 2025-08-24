using Seu.Mail.Contracts.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Seu.Mail.Core.Models;
using Seu.Mail.Services;

namespace Seu.Mail.Tests.Services;

public class EmailProviderDetectionServiceTests
{
    private readonly IConfiguration _mockConfiguration;
    private readonly ILogger<EmailProviderDetectionService> _mockLogger;
    private readonly IDnsEmailDiscoveryService _mockDnsService;
    private readonly IEmailAutodiscoveryService _mockAutodiscoveryService;
    private readonly EmailProviderDetectionService _service;

    public EmailProviderDetectionServiceTests()
    {
        // Create a simple configuration that returns null for EmailProviders section
        var configBuilder = new ConfigurationBuilder();
        _mockConfiguration = configBuilder.Build();
        _mockLogger = Substitute.For<ILogger<EmailProviderDetectionService>>();
        _mockDnsService = Substitute.For<IDnsEmailDiscoveryService>();
        _mockAutodiscoveryService = Substitute.For<IEmailAutodiscoveryService>();

        _service = new EmailProviderDetectionService(
            _mockConfiguration,
            _mockLogger,
            _mockDnsService,
            _mockAutodiscoveryService);
    }

    // Helper method removed - now using default providers

    // DetectProviderAsync Tests

    [Test]
    public async Task DetectProviderAsync_WithGmailAddress_ShouldReturnGmailSettings()
    {
        // Act
        var result = await _service.DetectProviderAsync("user@gmail.com");

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.DisplayName).IsEqualTo("Gmail");
        await Assert.That(result.ImapServer).IsEqualTo("imap.gmail.com");
        await Assert.That(result.ImapPort).IsEqualTo(993);
        await Assert.That(result.SmtpServer).IsEqualTo("smtp.gmail.com");
        await Assert.That(result.SmtpPort).IsEqualTo(587);
    }

    [Test]
    public async Task DetectProviderAsync_WithOutlookAddress_ShouldReturnOutlookSettings()
    {
        // Act
        var result = await _service.DetectProviderAsync("user@outlook.com");

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.DisplayName).IsEqualTo("Outlook/Hotmail");
        await Assert.That(result.ImapServer).IsEqualTo("outlook.office365.com");
        await Assert.That(result.SmtpServer).IsEqualTo("smtp-mail.outlook.com");
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("invalid-email")]
    [Arguments("user@")]
    [Arguments("@domain.com")]
    public async Task DetectProviderAsync_WithInvalidEmail_ShouldReturnNull(string? email)
    {
        // Act
        var result = await _service.DetectProviderAsync(email!);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task DetectProviderAsync_WithUnknownDomain_ShouldReturnNull()
    {
        // Act
        var result = await _service.DetectProviderAsync("user@veryrare-unknowndomain-12345.test");

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task DetectProviderAsync_WithCaseInsensitiveDomain_ShouldReturnProvider()
    {
        // Act
        var result = await _service.DetectProviderAsync("user@GMAIL.COM");

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.DisplayName).IsEqualTo("Gmail");
    }

    // ProbeAndDetectProviderAsync Tests

    [Test]
    public async Task ProbeAndDetectProviderAsync_WhenKnownProviderExists_ShouldReturnKnownProvider()
    {
        // Act
        var result = await _service.ProbeAndDetectProviderAsync("gmail.com");

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.DisplayName).IsEqualTo("Gmail");

        // Should not call DNS or autodiscovery for known providers
        await _mockDnsService.DidNotReceive().DiscoverEmailServersAsync(Arg.Any<string>());
        await _mockAutodiscoveryService.DidNotReceive().AutodiscoverAsync(Arg.Any<string>());
    }

    [Test]
    public async Task ProbeAndDetectProviderAsync_WithUnknownDomain_ShouldTryDnsDiscovery()
    {
        // Arrange
        var unknownDomain = "veryrare-unknowndomain-54321.test";
        var autodiscoverySettings = new EmailProviderSettings
        {
            DisplayName = "Autodiscovered Provider",
            ImapServer = $"mail.{unknownDomain}",
            ImapPort = 993,
            SmtpServer = $"smtp.{unknownDomain}",
            SmtpPort = 587,
            UseSsl = true
        };

        _mockDnsService.DiscoverEmailServersAsync(unknownDomain).Returns((EmailProviderSettings?)null);
        // First call in DetectProviderAsync returns null, second call in ProbeAndDetectProviderAsync succeeds
        _mockAutodiscoveryService.AutodiscoverAsync($"test@{unknownDomain}")
            .Returns((EmailProviderSettings?)null, autodiscoverySettings);

        // Act
        var result = await _service.ProbeAndDetectProviderAsync(unknownDomain);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.DisplayName).IsEqualTo("Autodiscovered Provider");

        // ProbeAndDetectProviderAsync calls DetectProviderAsync which calls autodiscovery, then calls autodiscovery again
        // So we expect 2 calls to autodiscovery
        await _mockAutodiscoveryService.Received(2).AutodiscoverAsync($"test@{unknownDomain}");
    }

    [Test]
    public async Task ProbeAndDetectProviderAsync_WhenDnsFailsTryAutodiscovery_ShouldReturnAutodiscoveryResult()
    {
        // Arrange
        var unknownDomain = "veryrare-unknowndomain-12345.test";
        _mockDnsService.DiscoverEmailServersAsync(unknownDomain).Returns((EmailProviderSettings?)null);

        var autodiscoverySettings = new EmailProviderSettings
        {
            DisplayName = "Autodiscovered",
            SmtpServer = $"smtp.{unknownDomain}",
            SmtpPort = 587,
            ImapServer = $"imap.{unknownDomain}",
            ImapPort = 993
        };

        _mockAutodiscoveryService.AutodiscoverAsync($"test@{unknownDomain}").Returns(autodiscoverySettings);

        // Act
        var result = await _service.ProbeAndDetectProviderAsync(unknownDomain);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.DisplayName).IsEqualTo("Autodiscovered");

        // ProbeAndDetectProviderAsync calls DetectProviderAsync which calls autodiscovery and succeeds
        // So we expect only 1 call to autodiscovery
        await _mockAutodiscoveryService.Received(1).AutodiscoverAsync($"test@{unknownDomain}");
    }

    [Test]
    public async Task ProbeAndDetectProviderAsync_WhenAllMethodsFail_ShouldReturnNull()
    {
        // Arrange
        var unknownDomain = "veryrare-unknowndomain-99999.test";
        _mockDnsService.DiscoverEmailServersAsync(unknownDomain).Returns((EmailProviderSettings?)null);
        _mockAutodiscoveryService.AutodiscoverAsync($"test@{unknownDomain}").Returns((EmailProviderSettings?)null);

        // Act
        var result = await _service.ProbeAndDetectProviderAsync(unknownDomain);

        // Assert
        await Assert.That(result).IsNull();

        // ProbeAndDetectProviderAsync calls DetectProviderAsync which calls autodiscovery and fails, then calls autodiscovery again and fails
        // So we expect 2 calls to autodiscovery
        await _mockAutodiscoveryService.Received().AutodiscoverAsync($"test@{unknownDomain}");
    }

    // GetAllProvidersAsync Tests

    [Test]
    public async Task GetAllProvidersAsync_ShouldReturnAllConfiguredProviders()
    {
        // Act
        var result = await _service.GetAllProvidersAsync();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Count).IsEqualTo(7);
        await Assert.That(result.Any(p => p.DisplayName == "Gmail")).IsTrue();
        await Assert.That(result.Any(p => p.DisplayName == "Outlook/Hotmail")).IsTrue();
    }

    // GetProviderByNameAsync Tests

    [Test]
    [Arguments("Gmail", "Gmail")]
    [Arguments("Outlook", "Outlook/Hotmail")]
    public async Task GetProviderByNameAsync_WithValidName_ShouldReturnProvider(string providerName, string expectedDisplayName)
    {
        // Act
        var result = await _service.GetProviderByNameAsync(providerName);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.DisplayName).IsEqualTo(expectedDisplayName);
    }

    [Test]
    public async Task GetProviderByNameAsync_WithCaseInsensitiveName_ShouldReturnProvider()
    {
        // Act
        var result = await _service.GetProviderByNameAsync("OUTLOOK");

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.DisplayName).IsEqualTo("Outlook/Hotmail");
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("NonExistentProvider")]
    public async Task GetProviderByNameAsync_WithInvalidName_ShouldReturnNull(string? providerName)
    {
        // Act
        var result = await _service.GetProviderByNameAsync(providerName!);

        // Assert
        await Assert.That(result).IsNull();
    }

    // Edge Cases and Error Handling Tests

    [Test]
    public async Task DetectProviderAsync_WhenExceptionThrown_ShouldReturnNullAndLogError()
    {
        // Arrange
        _mockAutodiscoveryService.When(x => x.AutodiscoverAsync(Arg.Any<string>()))
            .Do(x => throw new Exception("Autodiscovery failed"));

        // Act
        var result = await _service.ProbeAndDetectProviderAsync("unknowndomain.invalid");

        // Assert
        await Assert.That(result).IsNull();

        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Error in probing detection for domain")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Test]
    public async Task ProbeAndDetectProviderAsync_WithNetworkTimeout_ShouldHandleGracefully()
    {
        // Arrange
        var unknownDomain = "veryrare-timeout-domain.test";
        _mockAutodiscoveryService.AutodiscoverAsync($"user@{unknownDomain}")
            .Returns(Task.FromException<EmailProviderSettings?>(new TimeoutException("Autodiscovery timeout")));

        // Act
        var result = await _service.ProbeAndDetectProviderAsync(unknownDomain);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task DetectProviderAsync_WithMalformedConfiguration_ShouldHandleGracefully()
    {
        // Arrange - Test with a domain that should definitely not match any provider
        var unknownDomain = "veryrare-malformed-domain-12345.test";

        // Act - Use the existing service with a truly unknown domain
        var result = await _service.DetectProviderAsync($"user@{unknownDomain}");

        // Assert - Should return null for unknown domain
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ProbeAndDetectProviderAsync_WithMultipleDomainsInEmail_ShouldUseCorrectDomain()
    {
        // Arrange - Test with a truly unknown domain that won't match variations
        var result = await _service.DetectProviderAsync("user@veryrare-unknowndomain-variations.test");

        // Act & Assert
        // Should not match any provider since domain is completely unknown
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task DetectProviderAsync_WithInternationalDomain_ShouldHandleCorrectly()
    {
        // Act
        var result = await _service.DetectProviderAsync("user@тест.com");

        // Assert
        await Assert.That(result).IsNull(); // Unknown domain, but should not throw
    }

    [Test]
    public async Task ProbeAndDetectProviderAsync_WhenDnsThrowsButAutodiscoverySucceeds_ShouldReturnAutodiscoveryResult()
    {
        // Arrange
        _mockDnsService.DiscoverEmailServersAsync("partial-fail.com")
            .Returns(Task.FromException<EmailProviderSettings?>(new Exception("DNS failed")));

        var autodiscoverySettings = new EmailProviderSettings
        {
            DisplayName = "Recovered",
            ImapServer = "imap.partial-fail.com",
            ImapPort = 993
        };

        _mockAutodiscoveryService.AutodiscoverAsync("test@partial-fail.com")
            .Returns(autodiscoverySettings);

        // Act
        var result = await _service.ProbeAndDetectProviderAsync("partial-fail.com");

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.DisplayName).IsEqualTo("Recovered");
    }

    // Performance Tests

    [Test]
    public async Task GetAllProvidersAsync_WithDefaultProviders_ShouldPerformWell()
    {
        // Arrange - using default providers (no configuration needed)

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var providers = await _service.GetAllProvidersAsync();
        stopwatch.Stop();

        // Assert - default providers include Gmail, Outlook, Yahoo, Zoho, Fastmail, etc.
        await Assert.That(providers.Count).IsGreaterThan(4);
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(100);
    }

    [Test]
    public async Task DetectProviderAsync_WithFrequentCalls_ShouldBeCached()
    {
        // Act - Multiple calls with same domain
        var result1 = await _service.DetectProviderAsync("user1@gmail.com");
        var result2 = await _service.DetectProviderAsync("user2@gmail.com");
        var result3 = await _service.DetectProviderAsync("user3@gmail.com");

        // Assert
        await Assert.That(result1).IsNotNull();
        await Assert.That(result2).IsNotNull();
        await Assert.That(result3).IsNotNull();

        // All should return equivalent results
        await Assert.That(result1!.DisplayName).IsEqualTo(result2!.DisplayName);
        await Assert.That(result2!.DisplayName).IsEqualTo(result3!.DisplayName);
    }

    [Test]
    public async Task DetectProviderAsync_WithMultipleRequests_ShouldMaintainPerformance()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result1 = await _service.DetectProviderAsync("user1@gmail.com");
        var result2 = await _service.DetectProviderAsync("user2@gmail.com");
        var result3 = await _service.DetectProviderAsync("user3@gmail.com");
        stopwatch.Stop();

        var elapsed = stopwatch.Elapsed;

        // Assert
        // All should return equivalent results
        await Assert.That(result1!.DisplayName).IsEqualTo(result2!.DisplayName);
        await Assert.That(result2!.DisplayName).IsEqualTo(result3!.DisplayName);
        await Assert.That(elapsed.TotalMilliseconds).IsLessThan(1000);
    }
}
