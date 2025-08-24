using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Seu.Mail.Contracts.Services;

namespace Seu.Mail.Services;

/// <summary>
/// Default implementation of IDnsHttpClient that wraps HttpClient.
/// This implementation provides HTTP operations for DNS-over-HTTPS queries with proper error handling and logging.
/// </summary>
public class DefaultDnsHttpClient : IDnsHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DefaultDnsHttpClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultDnsHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="logger">The logger for HTTP operations.</param>
    public DefaultDnsHttpClient(HttpClient httpClient, ILogger<DefaultDnsHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public TimeSpan Timeout
    {
        get => _httpClient.Timeout;
        set => _httpClient.Timeout = value;
    }

    /// <inheritdoc />
    public async Task<string?> GetStringAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            _logger.LogDebug("DNS GET request to {Url} succeeded", url);
            return response;
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("DNS GET request to {Url} was cancelled", url);
            return null;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogDebug("DNS GET request to {Url} timed out", url);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogDebug(ex, "HTTP error during DNS GET request to {Url}", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unexpected error during DNS GET request to {Url}", url);
            return null;
        }
    }

    /// <inheritdoc />
    public void SetUserAgent(string userAgent)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
            _logger.LogDebug("User-Agent set to: {UserAgent}", userAgent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set User-Agent header to: {UserAgent}", userAgent);
        }
    }
}
