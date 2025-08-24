using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Seu.Mail.Data.Context;
using Seu.Mail.Core.Models;
using Seu.Mail.Services;

namespace Seu.Mail.Tests.Services;

public class UserSettingsServiceTests : IAsyncDisposable
{
    private readonly EmailDbContext _context;
    private readonly ILogger<UserSettingsService> _mockLogger;
    private readonly UserSettingsService _userSettingsService;

    public UserSettingsServiceTests()
    {
        var options = new DbContextOptionsBuilder<EmailDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new EmailDbContext(options);
        _mockLogger = Substitute.For<ILogger<UserSettingsService>>();
        _userSettingsService = new UserSettingsService(_context, _mockLogger);
    }

    #region GetUserSettingsAsync Tests

    [Test]
    public async Task GetUserSettingsAsync_WithExistingSettings_ShouldReturnSettings()
    {
        // Arrange
        var existingSettings = new UserSettings
        {
            Id = 1,
            EmailDisplayMode = EmailDisplayMode.TitleSender,
            EmailLayoutMode = EmailLayoutMode.SplitRight,
            DefaultSignature = "Test Signature",
            UseCompactMode = true,
            EmailsPerPage = 25
        };

        await _context.UserSettings.AddAsync(existingSettings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userSettingsService.GetUserSettingsAsync();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.EmailDisplayMode).IsEqualTo(EmailDisplayMode.TitleSender);
        await Assert.That(result.EmailLayoutMode).IsEqualTo(EmailLayoutMode.SplitRight);
        await Assert.That(result.DefaultSignature).IsEqualTo("Test Signature");
        await Assert.That(result.UseCompactMode).IsTrue();
        await Assert.That(result.EmailsPerPage).IsEqualTo(25);
    }

    [Test]
    public async Task GetUserSettingsAsync_WithNoExistingSettings_ShouldCreateDefaultSettings()
    {
        // Act
        var result = await _userSettingsService.GetUserSettingsAsync();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.EmailDisplayMode).IsEqualTo(EmailDisplayMode.TitleSenderPreview);
        await Assert.That(result.EmailLayoutMode).IsEqualTo(EmailLayoutMode.SeparatePage);
        await Assert.That(result.DefaultSignature).IsEmpty();
        await Assert.That(result.UseCompactMode).IsFalse();
        await Assert.That(result.EmailsPerPage).IsEqualTo(50);
        await Assert.That(result.MarkAsReadOnOpen).IsTrue();
        await Assert.That(result.ShowEmailPreview).IsTrue();
        await Assert.That(result.EnableKeyboardNavigation).IsTrue();

        // Should be saved to database
        var savedSettings = await _context.UserSettings.FirstOrDefaultAsync();
        await Assert.That(savedSettings).IsNotNull();
    }

    #endregion

    #region UpdateUserSettingsAsync Tests

    [Test]
    public async Task UpdateUserSettingsAsync_WithValidSettings_ShouldReturnTrueAndUpdateSettings()
    {
        // Arrange
        var existingSettings = new UserSettings
        {
            Id = 1,
            EmailDisplayMode = EmailDisplayMode.TitleOnly,
            UseCompactMode = false
        };

        await _context.UserSettings.AddAsync(existingSettings);
        await _context.SaveChangesAsync();

        var updatedSettings = new UserSettings
        {
            Id = 1,
            EmailDisplayMode = EmailDisplayMode.TitleSenderPreview,
            UseCompactMode = true,
            DefaultSignature = "Updated Signature",
            EmailsPerPage = 100
        };

        // Act
        var result = await _userSettingsService.UpdateUserSettingsAsync(updatedSettings);

        // Assert
        await Assert.That(result).IsTrue();

        var savedSettings = await _context.UserSettings.FindAsync(1);
        await Assert.That(savedSettings).IsNotNull();
        await Assert.That(savedSettings!.EmailDisplayMode).IsEqualTo(EmailDisplayMode.TitleSenderPreview);
        await Assert.That(savedSettings.UseCompactMode).IsTrue();
        await Assert.That(savedSettings.DefaultSignature).IsEqualTo("Updated Signature");
        await Assert.That(savedSettings.EmailsPerPage).IsEqualTo(100);
    }

    [Test]
    public async Task UpdateUserSettingsAsync_WithNonExistentSettings_ShouldCreateNewSettings()
    {
        // Arrange
        var settings = new UserSettings
        {
            Id = 999, // ID is ignored - service works with single settings record
            EmailDisplayMode = EmailDisplayMode.TitleOnly
        };

        // Act
        var result = await _userSettingsService.UpdateUserSettingsAsync(settings);

        // Assert
        await Assert.That(result).IsTrue();

        var savedSettings = await _context.UserSettings.FirstOrDefaultAsync();
        await Assert.That(savedSettings).IsNotNull();
        await Assert.That(savedSettings!.EmailDisplayMode).IsEqualTo(EmailDisplayMode.TitleOnly);
    }

    #endregion

    #region UpdateEmailDisplayModeAsync Tests

    [Test]
    [Arguments(EmailDisplayMode.TitleOnly)]
    [Arguments(EmailDisplayMode.TitleSender)]
    [Arguments(EmailDisplayMode.TitleSenderPreview)]
    public async Task UpdateEmailDisplayModeAsync_WithValidMode_ShouldReturnTrueAndUpdateMode(EmailDisplayMode mode)
    {
        // Arrange
        var settings = new UserSettings
        {
            Id = 1,
            EmailDisplayMode = EmailDisplayMode.TitleOnly
        };

        await _context.UserSettings.AddAsync(settings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userSettingsService.UpdateEmailDisplayModeAsync(mode);

        // Assert
        await Assert.That(result).IsTrue();

        var updatedSettings = await _context.UserSettings.FindAsync(1);
        await Assert.That(updatedSettings).IsNotNull();
        await Assert.That(updatedSettings!.EmailDisplayMode).IsEqualTo(mode);
    }

    [Test]
    public async Task UpdateEmailDisplayModeAsync_WithNoExistingSettings_ShouldCreateSettingsAndReturnTrue()
    {
        // Act
        var result = await _userSettingsService.UpdateEmailDisplayModeAsync(EmailDisplayMode.TitleSender);

        // Assert
        await Assert.That(result).IsTrue();

        var settings = await _context.UserSettings.FirstOrDefaultAsync();
        await Assert.That(settings).IsNotNull();
        await Assert.That(settings!.EmailDisplayMode).IsEqualTo(EmailDisplayMode.TitleSender);
    }

    #endregion

    #region UpdateEmailLayoutModeAsync Tests

    [Test]
    [Arguments(EmailLayoutMode.SeparatePage)]
    [Arguments(EmailLayoutMode.SplitRight)]
    [Arguments(EmailLayoutMode.SplitBottom)]
    public async Task UpdateEmailLayoutModeAsync_WithValidMode_ShouldReturnTrueAndUpdateMode(EmailLayoutMode mode)
    {
        // Arrange
        var settings = new UserSettings
        {
            Id = 1,
            EmailLayoutMode = EmailLayoutMode.SeparatePage
        };

        await _context.UserSettings.AddAsync(settings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userSettingsService.UpdateEmailLayoutModeAsync(mode);

        // Assert
        await Assert.That(result).IsTrue();

        var updatedSettings = await _context.UserSettings.FindAsync(1);
        await Assert.That(updatedSettings).IsNotNull();
        await Assert.That(updatedSettings!.EmailLayoutMode).IsEqualTo(mode);
    }

    #endregion

    #region UpdateDefaultSignatureAsync Tests

    [Test]
    public async Task UpdateDefaultSignatureAsync_WithValidSignature_ShouldReturnTrueAndUpdateSignature()
    {
        // Arrange
        var settings = new UserSettings
        {
            Id = 1,
            DefaultSignature = "Old Signature"
        };

        await _context.UserSettings.AddAsync(settings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userSettingsService.UpdateDefaultSignatureAsync("New Signature");

        // Assert
        await Assert.That(result).IsTrue();

        var updatedSettings = await _context.UserSettings.FindAsync(1);
        await Assert.That(updatedSettings).IsNotNull();
        await Assert.That(updatedSettings!.DefaultSignature).IsEqualTo("New Signature");
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public async Task UpdateDefaultSignatureAsync_WithEmptyOrNullSignature_ShouldReturnTrueAndClearSignature(
        string? signature)
    {
        // Arrange
        var settings = new UserSettings
        {
            Id = 1,
            DefaultSignature = "Existing Signature"
        };

        await _context.UserSettings.AddAsync(settings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userSettingsService.UpdateDefaultSignatureAsync(signature!);

        // Assert
        await Assert.That(result).IsTrue();

        var updatedSettings = await _context.UserSettings.FindAsync(1);
        await Assert.That(updatedSettings).IsNotNull();
        await Assert.That(updatedSettings!.DefaultSignature ?? "").IsEqualTo(signature ?? "");
    }

    #endregion

    #region ResetToDefaultsAsync Tests

    [Test]
    public async Task ResetToDefaultsAsync_WithExistingSettings_ShouldReturnTrueAndResetToDefaults()
    {
        // Arrange
        var settings = new UserSettings
        {
            Id = 1,
            EmailDisplayMode = EmailDisplayMode.TitleOnly,
            EmailLayoutMode = EmailLayoutMode.SplitRight,
            DefaultSignature = "Custom Signature",
            UseCompactMode = true,
            EmailsPerPage = 25,
            MarkAsReadOnOpen = false,
            ShowEmailPreview = false,
            EnableKeyboardNavigation = false
        };

        await _context.UserSettings.AddAsync(settings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userSettingsService.ResetToDefaultsAsync();

        // Assert
        await Assert.That(result).IsTrue();

        var resetSettings = await _context.UserSettings.FindAsync(1);
        await Assert.That(resetSettings).IsNotNull();
        await Assert.That(resetSettings!.EmailDisplayMode).IsEqualTo(EmailDisplayMode.TitleSenderPreview);
        await Assert.That(resetSettings.EmailLayoutMode).IsEqualTo(EmailLayoutMode.SeparatePage);
        await Assert.That(resetSettings.DefaultSignature).IsEmpty();
        await Assert.That(resetSettings.UseCompactMode).IsFalse();
        await Assert.That(resetSettings.EmailsPerPage).IsEqualTo(50);
        await Assert.That(resetSettings.MarkAsReadOnOpen).IsTrue();
        await Assert.That(resetSettings.ShowEmailPreview).IsTrue();
        await Assert.That(resetSettings.EnableKeyboardNavigation).IsTrue();
    }

    [Test]
    public async Task ResetToDefaultsAsync_WithNoExistingSettings_ShouldCreateDefaultSettingsAndReturnTrue()
    {
        // Act
        var result = await _userSettingsService.ResetToDefaultsAsync();

        // Assert
        await Assert.That(result).IsTrue();

        var settings = await _context.UserSettings.FirstOrDefaultAsync();
        await Assert.That(settings).IsNotNull();

        // Verify the settings were reset properly
        await Assert.That(settings!.EmailDisplayMode).IsEqualTo(EmailDisplayMode.TitleSenderPreview);
        await Assert.That(settings.EmailLayoutMode).IsEqualTo(EmailLayoutMode.SeparatePage);
        await Assert.That(settings.UseCompactMode).IsFalse();
    }

    #endregion

    #region Edge Cases and Error Handling

    [Test]
    public async Task UpdateUserSettingsAsync_WhenDatabaseThrows_ShouldReturnFalse()
    {
        // Arrange
        await _context.DisposeAsync();
        var settings = new UserSettings { Id = 1 };

        // Act
        var result = await _userSettingsService.UpdateUserSettingsAsync(settings);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task UpdateEmailDisplayModeAsync_WithInvalidEnum_ShouldStillUpdateButLogWarning()
    {
        // Arrange
        var settings = new UserSettings { Id = 1 };
        await _context.UserSettings.AddAsync(settings);
        await _context.SaveChangesAsync();

        // Act - Using invalid enum value (cast from int)
        var result = await _userSettingsService.UpdateEmailDisplayModeAsync((EmailDisplayMode)999);

        // Assert
        await Assert.That(result).IsTrue(); // EF Core will still save it

        var updatedSettings = await _context.UserSettings.FindAsync(1);
        await Assert.That(updatedSettings!.EmailDisplayMode).IsEqualTo((EmailDisplayMode)999);
    }

    [Test]
    public async Task UpdateDefaultSignatureAsync_WithVeryLongSignature_ShouldHandleGracefully()
    {
        // Arrange
        var longSignature = new string('A', 10000); // Very long signature
        var settings = new UserSettings { Id = 1 };
        await _context.UserSettings.AddAsync(settings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userSettingsService.UpdateDefaultSignatureAsync(longSignature);

        // Assert
        await Assert.That(result).IsTrue();

        var updatedSettings = await _context.UserSettings.FindAsync(1);
        await Assert.That(updatedSettings!.DefaultSignature).IsEqualTo(longSignature);
    }

    #endregion

    #region Performance Tests

    [Test]
    public async Task GetUserSettingsAsync_WithMultipleCalls_ShouldPerformWell()
    {
        // Arrange
        var settings = new UserSettings { Id = 1 };
        await _context.UserSettings.AddAsync(settings);
        await _context.SaveChangesAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (var i = 0; i < 20; i++) await _userSettingsService.GetUserSettingsAsync();

        stopwatch.Stop();

        // Assert
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(1000); // Should complete within 1 second
    }

    [Test]
    public async Task UpdateUserSettingsAsync_WithFrequentUpdates_ShouldMaintainConsistency()
    {
        // Arrange
        var settings = new UserSettings
        {
            Id = 1,
            EmailsPerPage = 50
        };
        await _context.UserSettings.AddAsync(settings);
        await _context.SaveChangesAsync();

        // Act - Multiple rapid updates
        var tasks = new List<Task<bool>>();
        for (var i = 1; i <= 5; i++)
        {
            var updateSettings = new UserSettings
            {
                Id = 1,
                EmailsPerPage = 50 + i
            };
            tasks.Add(_userSettingsService.UpdateUserSettingsAsync(updateSettings));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        foreach (var result in results) await Assert.That(result).IsTrue();

        var finalSettings = await _context.UserSettings.FindAsync(1);
        await Assert.That(finalSettings).IsNotNull();
        await Assert.That(finalSettings!.EmailsPerPage).IsGreaterThan(50);
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}