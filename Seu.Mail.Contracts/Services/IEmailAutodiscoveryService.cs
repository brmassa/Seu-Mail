using Seu.Mail.Core.Models;

namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Service interface for email provider autodiscovery operations.
/// </summary>
public interface IEmailAutodiscoveryService
{
    /// <summary>
    /// Attempts to autodiscover email provider settings for the specified email address.
    /// </summary>
    /// <param name="emailAddress">The email address to autodiscover settings for.</param>
    /// <returns>Email provider settings if discovered; otherwise, null.</returns>
    Task<EmailProviderSettings?> AutodiscoverAsync(string emailAddress);

    /// <summary>
    /// Attempts Outlook-style autodiscovery for the specified email address.
    /// </summary>
    /// <param name="emailAddress">The email address to autodiscover settings for.</param>
    /// <returns>Email provider settings if discovered; otherwise, null.</returns>
    Task<EmailProviderSettings?> TryOutlookAutodiscoverAsync(string emailAddress);

    /// <summary>
    /// Attempts Mozilla autoconfig for the specified domain.
    /// </summary>
    /// <param name="domain">The domain to autodiscover settings for.</param>
    /// <returns>Email provider settings if discovered; otherwise, null.</returns>
    Task<EmailProviderSettings?> TryMozillaAutoconfigAsync(string domain);

    /// <summary>
    /// Attempts Apple autoconfig for the specified domain.
    /// </summary>
    /// <param name="domain">The domain to autodiscover settings for.</param>
    /// <returns>Email provider settings if discovered; otherwise, null.</returns>
    Task<EmailProviderSettings?> TryAppleAutoconfigAsync(string domain);

    /// <summary>
    /// Attempts well-known autoconfig for the specified domain.
    /// </summary>
    /// <param name="domain">The domain to autodiscover settings for.</param>
    /// <returns>Email provider settings if discovered; otherwise, null.</returns>
    Task<EmailProviderSettings?> TryWellKnownAutoconfigAsync(string domain);
}
