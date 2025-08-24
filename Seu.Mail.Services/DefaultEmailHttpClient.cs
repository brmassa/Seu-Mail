using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Seu.Mail.Contracts.Services;

namespace Seu.Mail.Services;

/// <summary>
/// Default implementation of IEmailHttpClient that wraps HttpClient.
/// This implementation provides HTTP operations for email autodiscovery with proper error handling and logging.
/// </summary>
public class DefaultEmailHttpClient : IEmailHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DefaultEmailHttpClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultEmailHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="logger">The logger for HTTP operations.</param>
    public DefaultEmailHttpClient(HttpClient httpClient, ILogger<DefaultEmailHttpClient> logger)
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
    public async Task<string?> PostStringAsync(string url, string content, string contentType)
    {
        try
        {
            using var stringContent = new StringContent(content, Encoding.UTF8, contentType);
            using var response = await _httpClient.PostAsync(url, stringContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("POST request to {Url} succeeded with status {StatusCode}", url, response.StatusCode);
                return responseContent;
            }

            _logger.LogDebug("POST request to {Url} failed with status {StatusCode}", url, response.StatusCode);
            return null;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogDebug("POST request to {Url} timed out", url);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogDebug(ex, "HTTP error during POST request to {Url}", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unexpected error during POST request to {Url}", url);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetStringAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(url);
            _logger.LogDebug("GET request to {Url} succeeded", url);
            return response;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogDebug("GET request to {Url} timed out", url);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogDebug(ex, "HTTP error during GET request to {Url}", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unexpected error during GET request to {Url}", url);
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
