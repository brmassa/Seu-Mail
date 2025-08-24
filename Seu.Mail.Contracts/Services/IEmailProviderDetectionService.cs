using Seu.Mail.Core.Models;

namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Service interface for detecting and retrieving email provider settings.
/// </summary>
public interface IEmailProviderDetectionService
{
    /// <summary>
    /// Detects the email provider settings based on the given email address.
    /// </summary>
    /// <param name="emailAddress">The email address to detect the provider for.</param>
    /// <returns>The detected provider settings, or null if not found.</returns>
    Task<EmailProviderSettings?> DetectProviderAsync(string emailAddress);

    /// <summary>
    /// Probes and detects the email provider settings for the given email address.
    /// </summary>
    /// <param name="emailAddress">The email address to probe and detect the provider for.</param>
    /// <returns>The detected provider settings, or null if not found.</returns>
    Task<EmailProviderSettings?> ProbeAndDetectProviderAsync(string emailAddress);

    /// <summary>
    /// Gets all known email provider settings.
    /// </summary>
    /// <returns>A list of all provider settings.</returns>
    Task<List<EmailProviderSettings>> GetAllProvidersAsync();

    /// <summary>
    /// Gets the provider settings by provider name.
    /// </summary>
    /// <param name="providerName">The name of the provider.</param>
    /// <returns>The provider settings, or null if not found.</returns>
    Task<EmailProviderSettings?> GetProviderByNameAsync(string providerName);
}
