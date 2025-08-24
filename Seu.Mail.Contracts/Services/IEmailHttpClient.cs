using System;
using System.Threading.Tasks;

namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Abstraction for HTTP client operations used in email autodiscovery.
/// This interface allows for easy mocking in unit tests and provides a clean separation
/// of concerns for HTTP operations.
/// </summary>
public interface IEmailHttpClient
{
    /// <summary>
    /// Gets or sets the timeout for HTTP requests.
    /// </summary>
    TimeSpan Timeout { get; set; }

    /// <summary>
    /// Sends a POST request with string content to the specified URL.
    /// </summary>
    /// <param name="url">The URL to send the POST request to.</param>
    /// <param name="content">The string content to send in the request body.</param>
    /// <param name="contentType">The content type of the request body.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response body as a string if successful, or null if the request failed.</returns>
    Task<string?> PostStringAsync(string url, string content, string contentType);

    /// <summary>
    /// Sends a GET request to the specified URL and returns the response body as a string.
    /// </summary>
    /// <param name="url">The URL to send the GET request to.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response body as a string if successful, or null if the request failed.</returns>
    Task<string?> GetStringAsync(string url);

    /// <summary>
    /// Configures the User-Agent header for all requests.
    /// </summary>
    /// <param name="userAgent">The User-Agent string to use for requests.</param>
    void SetUserAgent(string userAgent);
}