using Seu.Mail.Core.Models;

namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Service interface for managing user settings and preferences.
/// </summary>
public interface IUserSettingsService
{
    /// <summary>
    /// Gets the current user's settings.
    /// </summary>
    /// <returns>User settings.</returns>
    Task<UserSettings> GetUserSettingsAsync();

    /// <summary>
    /// Updates the user's settings.
    /// </summary>
    /// <param name="settings">The settings to update.</param>
    /// <returns>True if update was successful; otherwise, false.</returns>
    Task<bool> UpdateUserSettingsAsync(UserSettings settings);

    /// <summary>
    /// Updates the user's email display mode.
    /// </summary>
    /// <param name="displayMode">The display mode to set.</param>
    /// <returns>True if update was successful; otherwise, false.</returns>
    Task<bool> UpdateEmailDisplayModeAsync(EmailDisplayMode displayMode);

    /// <summary>
    /// Updates the user's email layout mode.
    /// </summary>
    /// <param name="layoutMode">The layout mode to set.</param>
    /// <returns>True if update was successful; otherwise, false.</returns>
    Task<bool> UpdateEmailLayoutModeAsync(EmailLayoutMode layoutMode);

    /// <summary>
    /// Updates the user's default email signature.
    /// </summary>
    /// <param name="signature">The signature to set.</param>
    /// <returns>True if update was successful; otherwise, false.</returns>
    Task<bool> UpdateDefaultSignatureAsync(string signature);

    /// <summary>
    /// Resets the user's settings to default values.
    /// </summary>
    /// <returns>True if reset was successful; otherwise, false.</returns>
    Task<bool> ResetToDefaultsAsync();
}
