using Seu.Mail.Core.Models;

namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Service interface for DNS-based email server discovery.
/// </summary>
public interface IDnsEmailDiscoveryService
{
    /// <summary>
    /// Discovers email server settings for a given domain using DNS records.
    /// </summary>
    /// <param name="domain">The domain to discover email servers for.</param>
    /// <returns>Email provider settings if discovered; otherwise, null.</returns>
    Task<EmailProviderSettings?> DiscoverEmailServersAsync(string domain);

    /// <summary>
    /// Gets the MX records for a given domain.
    /// </summary>
    /// <param name="domain">The domain to query for MX records.</param>
    /// <returns>List of MX record strings.</returns>
    Task<List<string>> GetMxRecordsAsync(string domain);

    /// <summary>
    /// Probes discovered servers using MX records to determine provider settings.
    /// </summary>
    /// <param name="domain">The domain to probe.</param>
    /// <param name="mxRecords">List of MX records to probe.</param>
    /// <returns>Email provider settings if successfully probed; otherwise, null.</returns>
    Task<EmailProviderSettings?> ProbeDiscoveredServersAsync(string domain, List<string> mxRecords);
}