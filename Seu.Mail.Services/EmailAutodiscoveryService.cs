using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Seu.Mail.Core.Models;
using Seu.Mail.Contracts.Services;

namespace Seu.Mail.Services;



/// <summary>
/// Provides autodiscovery services for email provider settings using various protocols and heuristics.
/// </summary>
public class EmailAutodiscoveryService : IEmailAutodiscoveryService
{
    private readonly IEmailHttpClient _httpClient;
    private readonly ILogger<EmailAutodiscoveryService> _logger;
    private readonly IDnsEmailDiscoveryService _dnsDiscoveryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAutodiscoveryService"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client for making web requests during autodiscovery.</param>
    /// <param name="logger">Logger for autodiscovery events and errors.</param>
    /// <param name="dnsDiscoveryService">DNS discovery service for resolving email server information.</param>
    public EmailAutodiscoveryService(
        IEmailHttpClient httpClient,
        ILogger<EmailAutodiscoveryService> logger,
        IDnsEmailDiscoveryService dnsDiscoveryService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _dnsDiscoveryService = dnsDiscoveryService;

        // Configure HttpClient
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
        _httpClient.SetUserAgent("Seu.Mail/1.0 (Email Client)");
    }

    /// <summary>
    /// Attempts to automatically discover email server configuration for the specified email address.
    /// </summary>
    /// <param name="emailAddress">The email address to discover configuration for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the discovered email provider configuration or null if discovery fails.</returns>
    public async Task<EmailProviderSettings?> AutodiscoverAsync(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress) || !IsValidEmail(emailAddress))
        {
            _logger.LogWarning("Invalid email address provided: {Email}", emailAddress);
            return null;
        }

        var domain = ExtractDomain(emailAddress);
        if (string.IsNullOrEmpty(domain))
        {
            return null;
        }

        _logger.LogInformation("Starting autodiscovery for email: {Email}, domain: {Domain}", emailAddress, domain);

        // Try different autodiscovery methods in order of preference
        var discoveryMethods = new[]
        {
            () => TryOutlookAutodiscoverAsync(emailAddress),
            () => TryMozillaAutoconfigAsync(domain),
            () => TryAppleAutoconfigAsync(domain),
            () => TryWellKnownAutoconfigAsync(domain),
            () => _dnsDiscoveryService.DiscoverEmailServersAsync(domain)
        };

        foreach (var method in discoveryMethods)
        {
            try
            {
                var result = await method();
                if (result != null)
                {
                    _logger.LogInformation("Autodiscovery successful for {Email} using method: {Method}",
                        emailAddress, method.Method.Name);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Autodiscovery method {Method} failed for {Email}",
                    method.Method.Name, emailAddress);
            }
        }

        _logger.LogWarning("All autodiscovery methods failed for email: {Email}", emailAddress);
        return null;
    }

    /// <summary>
    /// Attempts to discover email configuration using Microsoft Outlook/Exchange autodiscovery protocol.
    /// </summary>
    /// <param name="emailAddress">The email address to discover configuration for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the discovered configuration or null if discovery fails.</returns>
    public async Task<EmailProviderSettings?> TryOutlookAutodiscoverAsync(string emailAddress)
    {
        try
        {
            var domain = ExtractDomain(emailAddress);
            var autodiscoverUrls = new[]
            {
                $"https://autodiscover.{domain}/autodiscover/autodiscover.xml",
                $"https://{domain}/autodiscover/autodiscover.xml",
                $"https://autodiscover.{domain}/Autodiscover/Autodiscover.xml",
                "https://autodiscover-s.outlook.com/autodiscover/autodiscover.xml"
            };

            var requestXml = CreateOutlookAutodiscoverRequest(emailAddress);

            foreach (var url in autodiscoverUrls)
            {
                try
                {
                    var responseXml = await _httpClient.PostStringAsync(url, requestXml, "text/xml");
                    if (responseXml != null)
                    {
                        var settings = ParseOutlookAutodiscoverResponse(responseXml, domain);
                        if (settings != null)
                        {
                            _logger.LogInformation("Outlook autodiscover successful for {Email} at {Url}", emailAddress, url);
                            return settings;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Outlook autodiscover failed at {Url} for {Email}", url, emailAddress);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Outlook autodiscover for {Email}", emailAddress);
            return null;
        }
    }

    /// <summary>
    /// Attempts to discover email configuration using Mozilla Autoconfig protocol.
    /// </summary>
    /// <param name="domain">The domain to discover configuration for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the discovered configuration or null if discovery fails.</returns>
    public async Task<EmailProviderSettings?> TryMozillaAutoconfigAsync(string domain)
    {
        try
        {
            var autoconfigUrls = new[]
            {
                $"https://autoconfig.{domain}/mail/config-v1.1.xml",
                $"https://{domain}/.well-known/autoconfig/mail/config-v1.1.xml",
                $"https://autoconfig.thunderbird.net/v1.1/{domain}",
                $"http://autoconfig.{domain}/mail/config-v1.1.xml"
            };

            foreach (var url in autoconfigUrls)
            {
                try
                {
                    var response = await _httpClient.GetStringAsync(url);
                    if (response != null)
                    {
                        var settings = ParseMozillaAutoconfigResponse(response, domain);
                        if (settings != null)
                        {
                            _logger.LogInformation("Mozilla autoconfig successful for domain {Domain} at {Url}", domain, url);
                            return settings;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Mozilla autoconfig failed at {Url} for domain {Domain}", url, domain);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Mozilla autoconfig for domain {Domain}", domain);
            return null;
        }
    }

    /// <summary>
    /// Attempts to discover email configuration using Apple's autoconfig protocol.
    /// </summary>
    /// <param name="domain">The domain to discover configuration for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the discovered configuration or null if discovery fails.</returns>
    public async Task<EmailProviderSettings?> TryAppleAutoconfigAsync(string domain)
    {
        try
        {
            var autoconfigUrls = new[]
            {
                $"https://{domain}/.well-known/autoconfig/mail/config-v1.1.xml",
                $"https://autoconfig.{domain}/mail/config-v1.1.xml"
            };

            foreach (var url in autoconfigUrls)
            {
                try
                {
                    var response = await _httpClient.GetStringAsync(url);
                    if (response != null)
                    {
                        var settings = ParseAppleAutoconfigResponse(response, domain);
                        if (settings != null)
                        {
                            _logger.LogInformation("Apple autoconfig successful for domain {Domain} at {Url}", domain, url);
                            return settings;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Apple autoconfig failed at {Url} for domain {Domain}", url, domain);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Apple autoconfig for domain {Domain}", domain);
            return null;
        }
    }



    /// <summary>
    /// Attempts to discover email configuration using well-known autoconfig endpoints.
    /// </summary>
    /// <param name="domain">The domain to discover configuration for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the discovered configuration or null if discovery fails.</returns>
    public async Task<EmailProviderSettings?> TryWellKnownAutoconfigAsync(string domain)
    {
        try
        {
            var wellKnownUrls = new[]
            {
                $"https://{domain}/.well-known/mail-configuration",
                $"https://{domain}/.well-known/email-configuration",
                $"https://{domain}/.well-known/autoconfig",
                $"https://mail.{domain}/.well-known/mail-configuration"
            };

            foreach (var url in wellKnownUrls)
            {
                try
                {
                    var response = await _httpClient.GetStringAsync(url);
                    if (response != null)
                    {
                        var settings = ParseWellKnownResponse(response, domain);
                        if (settings != null)
                        {
                            _logger.LogInformation("Well-known autoconfig successful for domain {Domain} at {Url}", domain, url);
                            return settings;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Well-known autoconfig failed at {Url} for domain {Domain}", url, domain);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in well-known autoconfig for domain {Domain}", domain);
            return null;
        }
    }

    private static string CreateOutlookAutodiscoverRequest(string emailAddress)
    {
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Autodiscover xmlns=""http://schemas.microsoft.com/exchange/autodiscover/outlook/requestschema/2006"">
    <Request>
        <EMailAddress>{emailAddress}</EMailAddress>
        <AcceptableResponseSchema>http://schemas.microsoft.com/exchange/autodiscover/outlook/responseschema/2006a</AcceptableResponseSchema>
    </Request>
</Autodiscover>";
    }

    private EmailProviderSettings? ParseOutlookAutodiscoverResponse(string responseXml, string domain)
    {
        try
        {
            var doc = XDocument.Parse(responseXml);
            var ns = XNamespace.Get("http://schemas.microsoft.com/exchange/autodiscover/outlook/responseschema/2006a");

            var account = doc.Descendants(ns + "Account").FirstOrDefault();
            if (account == null) return null;

            var protocols = account.Descendants(ns + "Protocol").ToList();

            var imapProtocol = protocols.FirstOrDefault(p =>
                p.Element(ns + "Type")?.Value.Equals("IMAP", StringComparison.OrdinalIgnoreCase) == true);
            var smtpProtocol = protocols.FirstOrDefault(p =>
                p.Element(ns + "Type")?.Value.Equals("SMTP", StringComparison.OrdinalIgnoreCase) == true);

            if (imapProtocol == null || smtpProtocol == null) return null;

            var imapServer = imapProtocol.Element(ns + "Server")?.Value;
            var imapPort = int.TryParse(imapProtocol.Element(ns + "Port")?.Value, out var iPort) ? iPort : 993;
            var imapSsl = imapProtocol.Element(ns + "SSL")?.Value?.Equals("on", StringComparison.OrdinalIgnoreCase) == true;

            var smtpServer = smtpProtocol.Element(ns + "Server")?.Value;
            var smtpPort = int.TryParse(smtpProtocol.Element(ns + "Port")?.Value, out var sPort) ? sPort : 587;
            var smtpSsl = smtpProtocol.Element(ns + "SSL")?.Value?.Equals("on", StringComparison.OrdinalIgnoreCase) == true;

            if (string.IsNullOrEmpty(imapServer) || string.IsNullOrEmpty(smtpServer))
                return null;

            return new EmailProviderSettings
            {
                DisplayName = $"Outlook Autodiscovered ({domain})",
                ImapServer = imapServer,
                ImapPort = imapPort,
                SmtpServer = smtpServer,
                SmtpPort = smtpPort,
                UseSsl = imapSsl || smtpSsl,
                DomainPatterns = [domain]
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse Outlook autodiscover response");
            return null;
        }
    }

    private EmailProviderSettings? ParseMozillaAutoconfigResponse(string responseXml, string domain)
    {
        try
        {
            var doc = XDocument.Parse(responseXml);

            var imapServer = doc.Descendants("incomingServer")
                .Where(s => s.Attribute("type")?.Value == "imap")
                .FirstOrDefault();

            var smtpServer = doc.Descendants("outgoingServer")
                .Where(s => s.Attribute("type")?.Value == "smtp")
                .FirstOrDefault();

            if (imapServer == null || smtpServer == null) return null;

            var imapHostname = imapServer.Element("hostname")?.Value;
            var imapPort = int.TryParse(imapServer.Element("port")?.Value, out var iPort) ? iPort : 993;
            var imapSocketType = imapServer.Element("socketType")?.Value;

            var smtpHostname = smtpServer.Element("hostname")?.Value;
            var smtpPort = int.TryParse(smtpServer.Element("port")?.Value, out var sPort) ? sPort : 587;
            var smtpSocketType = smtpServer.Element("socketType")?.Value;

            if (string.IsNullOrEmpty(imapHostname) || string.IsNullOrEmpty(smtpHostname))
                return null;

            var useSsl = imapSocketType == "SSL" || smtpSocketType == "SSL" ||
                        imapSocketType == "STARTTLS" || smtpSocketType == "STARTTLS";

            return new EmailProviderSettings
            {
                DisplayName = $"Mozilla Autodiscovered ({domain})",
                ImapServer = imapHostname,
                ImapPort = imapPort,
                SmtpServer = smtpHostname,
                SmtpPort = smtpPort,
                UseSsl = useSsl,
                DomainPatterns = [domain]
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse Mozilla autoconfig response");
            return null;
        }
    }

    private EmailProviderSettings? ParseAppleAutoconfigResponse(string responseXml, string domain)
    {
        // Apple uses a similar format to Mozilla, so we can reuse the parser
        return ParseMozillaAutoconfigResponse(responseXml, domain);
    }

    private EmailProviderSettings? ParseWellKnownResponse(string response, string domain)
    {
        try
        {
            // Try to parse as JSON first
            if (response.TrimStart().StartsWith('{'))
            {
                return ParseWellKnownJsonResponse(response, domain);
            }

            // Try to parse as XML
            if (response.TrimStart().StartsWith('<'))
            {
                return ParseMozillaAutoconfigResponse(response, domain);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse well-known response");
            return null;
        }
    }

    private EmailProviderSettings? ParseWellKnownJsonResponse(string jsonResponse, string domain)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonResponse);
            var root = document.RootElement;

            if (!root.TryGetProperty("mailServers", out var mailServers))
                return null;

            var imap = mailServers.EnumerateObject()
                .FirstOrDefault(ms => ms.Name.Contains("imap", StringComparison.OrdinalIgnoreCase));
            var smtp = mailServers.EnumerateObject()
                .FirstOrDefault(ms => ms.Name.Contains("smtp", StringComparison.OrdinalIgnoreCase));

            if (imap.Value.ValueKind == JsonValueKind.Undefined ||
                smtp.Value.ValueKind == JsonValueKind.Undefined)
                return null;

            var imapServer = imap.Value.TryGetProperty("hostname", out var imapHost) ? imapHost.GetString() : null;
            var imapPort = imap.Value.TryGetProperty("port", out var imapP) ? imapP.GetInt32() : 993;
            var imapSsl = imap.Value.TryGetProperty("ssl", out var imapSslProp) ? imapSslProp.GetBoolean() : true;

            var smtpServer = smtp.Value.TryGetProperty("hostname", out var smtpHost) ? smtpHost.GetString() : null;
            var smtpPort = smtp.Value.TryGetProperty("port", out var smtpP) ? smtpP.GetInt32() : 587;
            var smtpSsl = smtp.Value.TryGetProperty("ssl", out var smtpSslProp) ? smtpSslProp.GetBoolean() : true;

            if (string.IsNullOrEmpty(imapServer) || string.IsNullOrEmpty(smtpServer))
                return null;

            return new EmailProviderSettings
            {
                DisplayName = $"Auto-discovered ({domain})",
                ImapServer = imapServer,
                ImapPort = imapPort,
                SmtpServer = smtpServer,
                SmtpPort = smtpPort,
                UseSsl = imapSsl || smtpSsl,
                DomainPatterns = [domain]
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse well-known JSON response");
            return null;
        }
    }

    private static string ExtractDomain(string emailAddress)
    {
        var atIndex = emailAddress.LastIndexOf('@');
        return atIndex > 0 && atIndex < emailAddress.Length - 1
            ? emailAddress.Substring(atIndex + 1).ToLowerInvariant()
            : string.Empty;
    }

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
}
