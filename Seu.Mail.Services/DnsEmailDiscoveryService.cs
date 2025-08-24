using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Seu.Mail.Core.Models;
using Seu.Mail.Contracts.Services;

namespace Seu.Mail.Services;

/// <summary>
/// Provides DNS-based email server discovery services.
/// </summary>
public class DnsEmailDiscoveryService : IDnsEmailDiscoveryService
{
    private readonly ILogger<DnsEmailDiscoveryService> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="DnsEmailDiscoveryService"/> class.
    /// </summary>
    /// <param name="logger">Logger for DNS discovery events and errors.</param>
    /// <param name="httpClient">HTTP client for DNS queries.</param>
    public DnsEmailDiscoveryService(ILogger<DnsEmailDiscoveryService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Discovers email server settings for a given domain using DNS records.
    /// </summary>
    /// <param name="domain">The domain to discover email servers for.</param>
    /// <returns>Email provider settings if discovered; otherwise, null.</returns>
    public async Task<EmailProviderSettings?> DiscoverEmailServersAsync(string domain)
    {
        try
        {
            _logger.LogInformation("Starting DNS-based email server discovery for domain: {Domain}", domain);

            // Get MX records for the domain
            var mxRecords = await GetMxRecordsAsync(domain);
            if (!mxRecords.Any())
            {
                _logger.LogWarning("No MX records found for domain: {Domain}", domain);
                return null;
            }

            _logger.LogInformation("Found {Count} MX records for {Domain}: {Records}",
                mxRecords.Count, domain, string.Join(", ", mxRecords));

            // Try to probe the discovered servers
            var settings = await ProbeDiscoveredServersAsync(domain, mxRecords);
            if (settings != null)
            {
                settings.DisplayName = $"Auto-discovered ({domain})";
                settings.DomainPatterns = [domain];
                _logger.LogInformation("Successfully discovered email servers for {Domain}", domain);
            }

            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during DNS email server discovery for domain: {Domain}", domain);
            return null;
        }
    }

    /// <summary>
    /// Gets the MX records for a given domain.
    /// </summary>
    /// <param name="domain">The domain to query for MX records.</param>
    /// <returns>List of MX record strings.</returns>
    public async Task<List<string>> GetMxRecordsAsync(string domain)
    {
        var mxRecords = new List<string>();

        try
        {
            // Try multiple DNS resolution methods
            mxRecords.AddRange(await GetMxRecordsViaDnsLookupAsync(domain));

            if (!mxRecords.Any())
            {
                mxRecords.AddRange(await GetMxRecordsViaHttpAsync(domain));
            }

            // Remove duplicates and sort by preference (implied by common patterns)
            return mxRecords
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(GetMxPriority)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MX records for domain: {Domain}", domain);
            return [];
        }
    }

    /// <summary>
    /// Probes discovered servers using MX records to determine provider settings.
    /// </summary>
    /// <param name="domain">The domain to probe.</param>
    /// <param name="mxRecords">List of MX records to probe.</param>
    /// <returns>Email provider settings if successfully probed; otherwise, null.</returns>
    public async Task<EmailProviderSettings?> ProbeDiscoveredServersAsync(string domain, List<string> mxRecords)
    {
        try
        {
            string? workingImapServer = null;
            int workingImapPort = 993;
            string? workingSmtpServer = null;
            int workingSmtpPort = 587;
            bool usesSsl = true;

            // Generate potential server names from MX records and domain
            var potentialServers = GeneratePotentialServerNames(domain, mxRecords);

            // Probe for IMAP servers
            foreach (var server in potentialServers)
            {
                var imapPorts = new[] { 993, 143 };
                foreach (var port in imapPorts)
                {
                    if (await IsPortOpenAsync(server, port))
                    {
                        workingImapServer = server;
                        workingImapPort = port;
                        usesSsl = port == 993;
                        _logger.LogInformation("Found working IMAP server: {Server}:{Port}", server, port);
                        break;
                    }
                }
                if (workingImapServer != null) break;
            }

            // Probe for SMTP servers
            foreach (var server in potentialServers)
            {
                var smtpPorts = new[] { 587, 465, 25 };
                foreach (var port in smtpPorts)
                {
                    if (await IsPortOpenAsync(server, port))
                    {
                        workingSmtpServer = server;
                        workingSmtpPort = port;
                        if (port == 25) usesSsl = false; // Port 25 typically doesn't use SSL
                        _logger.LogInformation("Found working SMTP server: {Server}:{Port}", server, port);
                        break;
                    }
                }
                if (workingSmtpServer != null) break;
            }

            // If we found both servers, create configuration
            if (workingImapServer != null && workingSmtpServer != null)
            {
                return new EmailProviderSettings
                {
                    DisplayName = $"DNS Auto-discovered ({domain})",
                    ImapServer = workingImapServer,
                    ImapPort = workingImapPort,
                    SmtpServer = workingSmtpServer,
                    SmtpPort = workingSmtpPort,
                    UseSsl = usesSsl,
                    DomainPatterns = [domain]
                };
            }

            _logger.LogWarning("Could not find working email servers for domain: {Domain}", domain);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error probing discovered servers for domain: {Domain}", domain);
            return null;
        }
    }

    private async Task<List<string>> GetMxRecordsViaDnsLookupAsync(string domain)
    {
        var mxRecords = new List<string>();

        try
        {
            // Use System.Net.Dns to resolve MX records
            // Note: .NET doesn't have built-in MX record support, so we'll use a workaround
            var ipAddresses = await Dns.GetHostAddressesAsync(domain);

            // If we can resolve the domain, try common mail server patterns
            if (ipAddresses.Any())
            {
                var commonPatterns = new[]
                {
                    $"mail.{domain}",
                    $"smtp.{domain}",
                    $"imap.{domain}",
                    $"mx.{domain}",
                    $"mx1.{domain}",
                    $"mx2.{domain}",
                    domain
                };

                foreach (var pattern in commonPatterns)
                {
                    try
                    {
                        var addresses = await Dns.GetHostAddressesAsync(pattern);
                        if (addresses.Any())
                        {
                            mxRecords.Add(pattern);
                        }
                    }
                    catch
                    {
                        // Skip if DNS resolution fails
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "DNS lookup failed for domain: {Domain}", domain);
        }

        return mxRecords;
    }

    private async Task<List<string>> GetMxRecordsViaHttpAsync(string domain)
    {
        var mxRecords = new List<string>();

        try
        {
            // Use a public DNS API as fallback (Google DNS-over-HTTPS)
            var url = $"https://dns.google/resolve?name={domain}&type=MX";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await _httpClient.GetStringAsync(url, cts.Token);

            // Parse the JSON response (simple regex parsing)
            var mxPattern = new Regex(@"""data""\s*:\s*""[^""]*\s+([^""]+)""", RegexOptions.IgnoreCase);
            var matches = mxPattern.Matches(response);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    var mxRecord = match.Groups[1].Value.Trim().TrimEnd('.');
                    if (!string.IsNullOrEmpty(mxRecord))
                    {
                        mxRecords.Add(mxRecord);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "HTTP DNS lookup failed for domain: {Domain}", domain);
        }

        return mxRecords;
    }

    private static List<string> GeneratePotentialServerNames(string domain, List<string> mxRecords)
    {
        var servers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Add MX records as potential servers
        servers.UnionWith(mxRecords);

        // Generate common server name patterns
        var patterns = new[]
        {
            $"imap.{domain}",
            $"smtp.{domain}",
            $"mail.{domain}",
            $"pop.{domain}",
            $"pop3.{domain}",
            $"incoming.{domain}",
            $"outgoing.{domain}",
            $"secure.{domain}",
            $"ssl.{domain}",
            domain
        };

        servers.UnionWith(patterns);

        // Add variations of MX records
        foreach (var mx in mxRecords)
        {
            // Try to derive IMAP/SMTP servers from MX records
            if (mx.Contains("mx", StringComparison.OrdinalIgnoreCase))
            {
                var baseDomain = ExtractBaseDomain(mx, domain);
                if (!string.IsNullOrEmpty(baseDomain))
                {
                    servers.Add($"imap.{baseDomain}");
                    servers.Add($"smtp.{baseDomain}");
                    servers.Add($"mail.{baseDomain}");
                }
            }
        }

        return servers.ToList();
    }

    private static string ExtractBaseDomain(string mxRecord, string originalDomain)
    {
        // Try to extract the base domain from MX record
        if (mxRecord.EndsWith($".{originalDomain}", StringComparison.OrdinalIgnoreCase))
        {
            return originalDomain;
        }

        // Simple extraction - remove common prefixes
        var prefixes = new[] { "mx", "mx1", "mx2", "mx3", "mail", "smtp" };
        foreach (var prefix in prefixes)
        {
            if (mxRecord.StartsWith($"{prefix}.", StringComparison.OrdinalIgnoreCase))
            {
                return mxRecord.Substring(prefix.Length + 1);
            }
        }

        return originalDomain;
    }

    private async Task<bool> IsPortOpenAsync(string server, int port)
    {
        try
        {
            using var tcpClient = new System.Net.Sockets.TcpClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            await tcpClient.ConnectAsync(server, port, cts.Token);
            return tcpClient.Connected;
        }
        catch (OperationCanceledException)
        {
            return false; // Timeout
        }
        catch
        {
            return false;
        }
    }

    private static int GetMxPriority(string mxRecord)
    {
        // Simple priority based on common patterns
        if (mxRecord.Contains("mx1", StringComparison.OrdinalIgnoreCase)) return 1;
        if (mxRecord.Contains("mx2", StringComparison.OrdinalIgnoreCase)) return 2;
        if (mxRecord.Contains("mx3", StringComparison.OrdinalIgnoreCase)) return 3;
        if (mxRecord.Contains("mail", StringComparison.OrdinalIgnoreCase)) return 10;
        if (mxRecord.Contains("smtp", StringComparison.OrdinalIgnoreCase)) return 20;
        return 50; // Default priority
    }
}
