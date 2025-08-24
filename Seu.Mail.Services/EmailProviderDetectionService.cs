using Seu.Mail.Core.Models;
using Seu.Mail.Contracts.Services;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using MailKit.Net.Smtp;
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Seu.Mail.Services;

/// <summary>
/// Provides services for detecting and configuring email provider settings based on domain analysis and heuristics.
/// </summary>
public class EmailProviderDetectionService : IEmailProviderDetectionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailProviderDetectionService> _logger;
    private readonly Dictionary<string, EmailProviderSettings> _providers;
    private readonly IDnsEmailDiscoveryService _dnsDiscoveryService;
    private readonly IEmailAutodiscoveryService _autodiscoveryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailProviderDetectionService"/> class.
    /// </summary>
    /// <param name="configuration">Configuration settings for email provider detection.</param>
    /// <param name="logger">Logger for email provider detection events and errors.</param>
    /// <param name="dnsDiscoveryService">DNS discovery service for resolving email server information.</param>
    /// <param name="autodiscoveryService">Autodiscovery service for automatic provider configuration.</param>
    public EmailProviderDetectionService(
        IConfiguration configuration,
        ILogger<EmailProviderDetectionService> logger,
        IDnsEmailDiscoveryService dnsDiscoveryService,
        IEmailAutodiscoveryService autodiscoveryService)
    {
        _configuration = configuration;
        _logger = logger;
        _dnsDiscoveryService = dnsDiscoveryService;
        _autodiscoveryService = autodiscoveryService;
        _providers = LoadProvidersFromConfiguration();
    }

    /// <summary>
    /// Detects email provider settings for the specified email address using various discovery methods.
    /// </summary>
    /// <param name="emailAddress">The email address to detect provider settings for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the detected email provider settings or null if detection fails.</returns>
    public async Task<EmailProviderSettings?> DetectProviderAsync(string emailAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(emailAddress) || !IsValidEmail(emailAddress))
            {
                _logger.LogWarning("Invalid email address provided: {Email}", emailAddress);
                return null;
            }

            var domain = ExtractDomain(emailAddress);
            if (string.IsNullOrEmpty(domain))
            {
                _logger.LogWarning("Could not extract domain from email: {Email}", emailAddress);
                return null;
            }

            // First, try exact domain match
            var provider = FindProviderByDomain(domain);
            if (provider != null)
            {
                _logger.LogInformation("Provider detected for {Email}: {Provider}", emailAddress, provider.DisplayName);
                return provider;
            }

            // If no exact match, try common domain variations
            provider = await TryCommonDomainVariationsAsync(domain);
            if (provider != null)
            {
                _logger.LogInformation("Provider detected via domain variation for {Email}: {Provider}", emailAddress,
                    provider.DisplayName);
                return provider;
            }

            // If no configuration found, try comprehensive autodiscovery
            _logger.LogInformation(
                "No provider configuration found for domain: {Domain}, attempting comprehensive autodiscovery", domain);

            var autodiscoveredProvider = await _autodiscoveryService.AutodiscoverAsync(emailAddress);
            if (autodiscoveredProvider != null)
            {
                _logger.LogInformation("Comprehensive autodiscovery successful for {Email}", emailAddress);
                return autodiscoveredProvider;
            }

            _logger.LogInformation("Comprehensive autodiscovery failed, attempting fallback probe for domain: {Domain}",
                domain);
            return await ProbeEmailServersAsync(domain);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting provider for email: {Email}", emailAddress);
            return null;
        }
    }

    /// <summary>
    /// Gets all known email provider configurations.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of all known email provider settings.</returns>
    public async Task<List<EmailProviderSettings>> GetAllProvidersAsync()
    {
        return await Task.FromResult(_providers.Values.ToList());
    }

    /// <summary>
    /// Gets email provider settings by provider name.
    /// </summary>
    /// <param name="providerName">The name of the email provider.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the email provider settings or null if not found.</returns>
    public async Task<EmailProviderSettings?> GetProviderByNameAsync(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            return null;

        // Try case-sensitive first
        if (_providers.TryGetValue(providerName, out var provider))
            return await Task.FromResult(provider);

        // Try case-insensitive
        var kvp = _providers.FirstOrDefault(p =>
            string.Equals(p.Key, providerName, StringComparison.OrdinalIgnoreCase));

        return await Task.FromResult(kvp.Key != null ? kvp.Value : null);
    }

    private Dictionary<string, EmailProviderSettings> LoadProvidersFromConfiguration()
    {
        var providers = new Dictionary<string, EmailProviderSettings>();

        try
        {
            var providersSection = _configuration.GetSection("EmailProviders");
            if (!providersSection.Exists())
            {
                _logger.LogWarning("EmailProviders configuration section not found");
                return GetDefaultProviders();
            }

            foreach (var providerSection in providersSection.GetChildren())
            {
                var providerName = providerSection.Key;
                var settings = new EmailProviderSettings();
                providerSection.Bind(settings);

                if (IsValidProviderSettings(settings))
                {
                    providers[providerName] = settings;
                    _logger.LogDebug("Loaded provider configuration: {Provider}", providerName);
                }
                else
                {
                    _logger.LogWarning("Invalid provider configuration for: {Provider}", providerName);
                }
            }

            if (!providers.Any())
            {
                _logger.LogWarning("No valid provider configurations found, using defaults");
                return GetDefaultProviders();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading provider configurations, using defaults");
            return GetDefaultProviders();
        }

        return providers;
    }

    private static Dictionary<string, EmailProviderSettings> GetDefaultProviders()
    {
        return new Dictionary<string, EmailProviderSettings>
        {
            ["Gmail"] = new()
            {
                DisplayName = "Gmail",
                SmtpServer = "smtp.gmail.com",
                SmtpPort = 587,
                ImapServer = "imap.gmail.com",
                ImapPort = 993,
                UseSsl = true,
                DomainPatterns = ["gmail.com", "googlemail.com"]
            },
            ["Outlook"] = new()
            {
                DisplayName = "Outlook/Hotmail",
                SmtpServer = "smtp-mail.outlook.com",
                SmtpPort = 587,
                ImapServer = "outlook.office365.com",
                ImapPort = 993,
                UseSsl = true,
                DomainPatterns = ["outlook.com", "hotmail.com", "live.com", "msn.com"]
            },
            ["Yahoo"] = new()
            {
                DisplayName = "Yahoo Mail",
                SmtpServer = "smtp.mail.yahoo.com",
                SmtpPort = 587,
                ImapServer = "imap.mail.yahoo.com",
                ImapPort = 993,
                UseSsl = true,
                DomainPatterns = ["yahoo.com", "yahoo.co.uk", "yahoo.ca", "ymail.com"]
            },
            ["Zoho"] = new()
            {
                DisplayName = "Zoho Mail",
                SmtpServer = "smtp.zoho.com",
                SmtpPort = 587,
                ImapServer = "imap.zoho.com",
                ImapPort = 993,
                UseSsl = true,
                DomainPatterns = ["zoho.com", "zohomail.com"]
            },
            ["Fastmail"] = new()
            {
                DisplayName = "Fastmail",
                SmtpServer = "smtp.fastmail.com",
                SmtpPort = 587,
                ImapServer = "imap.fastmail.com",
                ImapPort = 993,
                UseSsl = true,
                DomainPatterns = ["fastmail.com", "fastmail.fm"]
            },
            ["AOL"] = new()
            {
                DisplayName = "AOL Mail",
                SmtpServer = "smtp.aol.com",
                SmtpPort = 587,
                ImapServer = "imap.aol.com",
                ImapPort = 993,
                UseSsl = true,
                DomainPatterns = ["aol.com"]
            },
            ["iCloud"] = new()
            {
                DisplayName = "iCloud Mail",
                SmtpServer = "smtp.mail.me.com",
                SmtpPort = 587,
                ImapServer = "imap.mail.me.com",
                ImapPort = 993,
                UseSsl = true,
                DomainPatterns = ["icloud.com", "me.com", "mac.com"]
            }
        };
    }

    /// <summary>
    /// Probes email servers for the specified domain and attempts to detect the provider configuration.
    /// </summary>
    /// <param name="domain">The domain to probe and detect provider settings for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the detected email provider settings or null if detection fails.</returns>
    public async Task<EmailProviderSettings?> ProbeAndDetectProviderAsync(string domain)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                _logger.LogWarning("Invalid domain provided for probing: {Domain}", domain);
                return null;
            }

            // Create a test email address for detection
            var testEmailAddress = $"test@{domain}";

            // First try configuration-based detection
            var configProvider = await DetectProviderAsync(testEmailAddress);
            if (configProvider != null) return configProvider;

            // If no config found, use comprehensive autodiscovery
            return await _autodiscoveryService.AutodiscoverAsync(testEmailAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in probing detection for domain: {Domain}", domain);
            return null;
        }
    }

    private async Task<EmailProviderSettings?> ProbeEmailServersAsync(string domain)
    {
        try
        {
            _logger.LogInformation("Probing email servers for domain: {Domain}", domain);

            var commonSmtpServers = new[]
            {
                $"smtp.{domain}",
                $"mail.{domain}",
                $"outgoing.{domain}",
                $"send.{domain}",
                $"smtp-mail.{domain}",
                $"secure.{domain}"
            };

            var commonImapServers = new[]
            {
                $"imap.{domain}",
                $"mail.{domain}",
                $"incoming.{domain}",
                $"receive.{domain}",
                $"secure.{domain}",
                $"imap-mail.{domain}"
            };

            var smtpPorts = new[] { 587, 465, 25 };
            var imapPorts = new[] { 993, 143 };

            string? workingSmtpServer = null;
            var workingSmtpPort = 587;
            string? workingImapServer = null;
            var workingImapPort = 993;

            // Probe SMTP servers
            foreach (var server in commonSmtpServers)
            {
                foreach (var port in smtpPorts)
                    if (await ProbeSmtpServerAsync(server, port))
                    {
                        workingSmtpServer = server;
                        workingSmtpPort = port;
                        _logger.LogInformation("Found working SMTP server: {Server}:{Port}", server, port);
                        break;
                    }

                if (workingSmtpServer != null) break;
            }

            // Probe IMAP servers
            foreach (var server in commonImapServers)
            {
                foreach (var port in imapPorts)
                    if (await ProbeImapServerAsync(server, port))
                    {
                        workingImapServer = server;
                        workingImapPort = port;
                        _logger.LogInformation("Found working IMAP server: {Server}:{Port}", server, port);
                        break;
                    }

                if (workingImapServer != null) break;
            }

            // If we found working servers, create a provider configuration
            if (workingSmtpServer != null && workingImapServer != null)
                return new EmailProviderSettings
                {
                    DisplayName = $"Auto-detected ({domain})",
                    SmtpServer = workingSmtpServer,
                    SmtpPort = workingSmtpPort,
                    ImapServer = workingImapServer,
                    ImapPort = workingImapPort,
                    UseSsl = workingSmtpPort != 25 && workingImapPort != 143, // Use SSL for secure ports
                    DomainPatterns = [domain]
                };

            _logger.LogWarning("Could not probe working email servers for domain: {Domain}", domain);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error probing email servers for domain: {Domain}", domain);
            return null;
        }
    }

    private async Task<bool> ProbeSmtpServerAsync(string server, int port)
    {
        try
        {
            // First check if port is open
            if (!await IsPortOpenAsync(server, port))
                return false;

            // Try to connect with SMTP client
            using var client = new SmtpClient();
            client.Timeout = 8000; // 8 second timeout

            var sslOptions = port == 465 ? SecureSocketOptions.SslOnConnect :
                port == 25 ? SecureSocketOptions.None :
                SecureSocketOptions.StartTls;

            try
            {
                await client.ConnectAsync(server, port, sslOptions);
                await client.DisconnectAsync(true);
                return true;
            }
            catch when (port != 25 && sslOptions != SecureSocketOptions.None)
            {
                // Try without SSL if initial connection fails
                await client.ConnectAsync(server, port, SecureSocketOptions.None);
                await client.DisconnectAsync(true);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("SMTP probe failed for {Server}:{Port} - {Error}", server, port, ex.Message);
            return false;
        }
    }

    private async Task<bool> ProbeImapServerAsync(string server, int port)
    {
        try
        {
            // First check if port is open
            if (!await IsPortOpenAsync(server, port))
                return false;

            // Try to connect with IMAP client
            using var client = new ImapClient();
            client.Timeout = 8000; // 8 second timeout

            var sslOptions = port == 143 ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect;

            try
            {
                await client.ConnectAsync(server, port, sslOptions);
                await client.DisconnectAsync(true);
                return true;
            }
            catch when (port == 143)
            {
                // Try without SSL if StartTls fails on port 143
                await client.ConnectAsync(server, port, SecureSocketOptions.None);
                await client.DisconnectAsync(true);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("IMAP probe failed for {Server}:{Port} - {Error}", server, port, ex.Message);
            return false;
        }
    }

    private async Task<bool> IsPortOpenAsync(string server, int port)
    {
        try
        {
            using var tcpClient = new TcpClient();
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

    private EmailProviderSettings? FindProviderByDomain(string domain)
    {
        foreach (var provider in _providers.Values)
            if (provider.DomainPatterns.Any(pattern =>
                    string.Equals(pattern, domain, StringComparison.OrdinalIgnoreCase)))
                return provider;

        return null;
    }

    private async Task<EmailProviderSettings?> TryCommonDomainVariationsAsync(string domain)
    {
        // Try some common variations
        var variations = new List<string>
        {
            domain,
            $"mail.{domain}",
            domain.StartsWith("mail.") ? domain[5..] : $"mail.{domain}",
            domain.StartsWith("www.") ? domain[4..] : domain
        };

        foreach (var variation in variations.Distinct())
        {
            var provider = FindProviderByDomain(variation);
            if (provider != null) return provider;
        }

        return await Task.FromResult<EmailProviderSettings?>(null);
    }

    private static string ExtractDomain(string emailAddress)
    {
        var atIndex = emailAddress.LastIndexOf('@');
        return atIndex > 0 && atIndex < emailAddress.Length - 1
            ? emailAddress.Substring(atIndex + 1).ToLowerInvariant()
            : string.Empty;
    }

    /// <summary>
    /// Validates if the provided string is a valid email address format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email format is valid; otherwise, false.</returns>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return emailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidProviderSettings(EmailProviderSettings settings)
    {
        return !string.IsNullOrWhiteSpace(settings.DisplayName) &&
               !string.IsNullOrWhiteSpace(settings.SmtpServer) &&
               !string.IsNullOrWhiteSpace(settings.ImapServer) &&
               settings is { SmtpPort: > 0, ImapPort: > 0 } &&
               settings.DomainPatterns.Any();
    }
}