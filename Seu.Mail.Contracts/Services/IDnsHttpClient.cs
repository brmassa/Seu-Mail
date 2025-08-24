using System;
using System.Threading.Tasks;

namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Abstraction for HTTP client operations used in DNS-based email discovery.
/// This interface allows for easy mocking in unit tests and provides a clean separation
/// of concerns for DNS-over-HTTPS operations.
/// </summary>
public interface IDnsHttpClient
{
    /// <summary>
    /// Gets or sets the timeout for HTTP requests.
    /// </summary>
    TimeSpan Timeout { get; set; }

    /// <summary>
    /// Sends a GET request to the specified URL with a cancellation token and returns the response body as a string.
    /// </summary>
    /// <param name="url">The URL to send the GET request to.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response body as a string if successful, or null if the request failed.</returns>
    Task<string?> GetStringAsync(string url, CancellationToken cancellationToken);

    /// <summary>
    /// Configures the User-Agent header for all requests.
    /// </summary>
    /// <param name="userAgent">The User-Agent string to use for requests.</param>
    void SetUserAgent(string userAgent);
}