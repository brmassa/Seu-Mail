using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Seu.Mail.Data.Context;
using Seu.Mail.Core.Models;
using Seu.Mail.Contracts.Services;

namespace Seu.Mail.Services;

/// <summary>
/// Provides services for managing user settings and preferences.
/// </summary>
public class UserSettingsService : IUserSettingsService
{
    private readonly EmailDbContext _context;
    private readonly ILogger<UserSettingsService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSettingsService"/> class.
    /// </summary>
    /// <param name="context">The database context for user settings data operations.</param>
    /// <param name="logger">Logger for user settings service events and errors.</param>
    public UserSettingsService(EmailDbContext context, ILogger<UserSettingsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user settings, creating default settings if none exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user settings.</returns>
    public async Task<UserSettings> GetUserSettingsAsync()
    {
        try
        {
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                // Create default settings
                settings = new UserSettings
                {
                    EmailDisplayMode = EmailDisplayMode.TitleSenderPreview,
                    EmailLayoutMode = EmailLayoutMode.SeparatePage,
                    DefaultSignature = "",
                    UseCompactMode = false,
                    EmailsPerPage = 50,
                    MarkAsReadOnOpen = true,
                    ShowEmailPreview = true,
                    EnableKeyboardNavigation = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user settings");
            // Return default settings if error occurs
            return new UserSettings
            {
                EmailDisplayMode = EmailDisplayMode.TitleSenderPreview,
                EmailLayoutMode = EmailLayoutMode.SeparatePage
            };
        }
    }

    /// <summary>
    /// Updates the user settings with the provided settings object.
    /// </summary>
    /// <param name="settings">The user settings to update.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the update was successful.</returns>
    public async Task<bool> UpdateUserSettingsAsync(UserSettings settings)
    {
        try
        {
            var existingSettings = await _context.UserSettings
                .FirstOrDefaultAsync();

            if (existingSettings == null)
            {
                _context.UserSettings.Add(settings);
            }
            else
            {
                existingSettings.EmailDisplayMode = settings.EmailDisplayMode;
                existingSettings.EmailLayoutMode = settings.EmailLayoutMode;
                existingSettings.DefaultSignature = settings.DefaultSignature;
                existingSettings.UseCompactMode = settings.UseCompactMode;
                existingSettings.EmailsPerPage = settings.EmailsPerPage;
                existingSettings.MarkAsReadOnOpen = settings.MarkAsReadOnOpen;
                existingSettings.ShowEmailPreview = settings.ShowEmailPreview;
                existingSettings.EnableKeyboardNavigation = settings.EnableKeyboardNavigation;
                existingSettings.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("User settings updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user settings");
            return false;
        }
    }

    /// <summary>
    /// Updates the email display mode setting.
    /// </summary>
    /// <param name="displayMode">The new email display mode.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the update was successful.</returns>
    public async Task<bool> UpdateEmailDisplayModeAsync(EmailDisplayMode displayMode)
    {
        try
        {
            var settings = await GetUserSettingsAsync();
            settings.EmailDisplayMode = displayMode;
            settings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email display mode");
            return false;
        }
    }

    /// <summary>
    /// Updates the email layout mode setting.
    /// </summary>
    /// <param name="layoutMode">The new email layout mode.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the update was successful.</returns>
    public async Task<bool> UpdateEmailLayoutModeAsync(EmailLayoutMode layoutMode)
    {
        try
        {
            var settings = await GetUserSettingsAsync();
            settings.EmailLayoutMode = layoutMode;
            settings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email layout mode");
            return false;
        }
    }

    /// <summary>
    /// Updates the default email signature setting.
    /// </summary>
    /// <param name="signature">The new default signature text.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the update was successful.</returns>
    public async Task<bool> UpdateDefaultSignatureAsync(string signature)
    {
        try
        {
            var settings = await GetUserSettingsAsync();
            settings.DefaultSignature = signature;
            settings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating default signature");
            return false;
        }
    }

    /// <summary>
    /// Resets user settings to their default values.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the reset was successful.</returns>
    public async Task<bool> ResetToDefaultsAsync()
    {
        try
        {
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync();

            if (settings != null)
            {
                settings.EmailDisplayMode = EmailDisplayMode.TitleSenderPreview;
                settings.EmailLayoutMode = EmailLayoutMode.SeparatePage;
                settings.DefaultSignature = "";
                settings.UseCompactMode = false;
                settings.EmailsPerPage = 50;
                settings.MarkAsReadOnOpen = true;
                settings.ShowEmailPreview = true;
                settings.EnableKeyboardNavigation = true;
                settings.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create default settings if none exist
                settings = new UserSettings
                {
                    EmailDisplayMode = EmailDisplayMode.TitleSenderPreview,
                    EmailLayoutMode = EmailLayoutMode.SeparatePage,
                    DefaultSignature = "",
                    UseCompactMode = false,
                    EmailsPerPage = 50,
                    MarkAsReadOnOpen = true,
                    ShowEmailPreview = true,
                    EnableKeyboardNavigation = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserSettings.Add(settings);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("User settings reset to defaults");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting user settings to defaults");
            return false;
        }
    }
}