using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Seu.Mail.Contracts.Services;

namespace Seu.Mail.Tests.TestHelpers;

/// <summary>
/// Mock implementation of IEmailHttpClient for unit testing.
/// Allows tests to configure responses and verify HTTP operations without making actual network calls.
/// </summary>
public class MockEmailHttpClient : IEmailHttpClient
{
    private readonly Dictionary<string, string> _getResponses = new();
    private readonly Dictionary<string, string> _postResponses = new();
    private readonly List<HttpRequest> _requestHistory = new();

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(15);

    public string? UserAgent { get; private set; }

    /// <summary>
    /// Gets the history of all HTTP requests made through this mock client.
    /// </summary>
    public IReadOnlyList<HttpRequest> RequestHistory => _requestHistory.AsReadOnly();

    /// <summary>
    /// Configures a mock response for GET requests to the specified URL.
    /// </summary>
    /// <param name="url">The URL to mock.</param>
    /// <param name="response">The response to return. Use null to simulate a failed request.</param>
    public void SetupGetResponse(string url, string? response)
    {
        if (response == null)
        {
            _getResponses.Remove(url);
        }
        else
        {
            _getResponses[url] = response;
        }
    }

    /// <summary>
    /// Configures a mock response for POST requests to the specified URL.
    /// </summary>
    /// <param name="url">The URL to mock.</param>
    /// <param name="response">The response to return. Use null to simulate a failed request.</param>
    public void SetupPostResponse(string url, string? response)
    {
        if (response == null)
        {
            _postResponses.Remove(url);
        }
        else
        {
            _postResponses[url] = response;
        }
    }

    /// <summary>
    /// Clears all configured responses and request history.
    /// </summary>
    public void Reset()
    {
        _getResponses.Clear();
        _postResponses.Clear();
        _requestHistory.Clear();
        UserAgent = null;
    }

    /// <inheritdoc />
    public Task<string?> GetStringAsync(string url)
    {
        _requestHistory.Add(new HttpRequest("GET", url, null, null));

        if (_getResponses.TryGetValue(url, out var response))
        {
            return Task.FromResult<string?>(response);
        }

        return Task.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public Task<string?> PostStringAsync(string url, string content, string contentType)
    {
        _requestHistory.Add(new HttpRequest("POST", url, content, contentType));

        if (_postResponses.TryGetValue(url, out var response))
        {
            return Task.FromResult<string?>(response);
        }

        return Task.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public void SetUserAgent(string userAgent)
    {
        UserAgent = userAgent;
    }

    /// <summary>
    /// Represents an HTTP request made through the mock client.
    /// </summary>
    public class HttpRequest
    {
        public HttpRequest(string method, string url, string? content, string? contentType)
        {
            Method = method;
            Url = url;
            Content = content;
            ContentType = contentType;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the HTTP method (GET, POST, etc.).
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Gets the request URL.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets the request content (for POST requests).
        /// </summary>
        public string? Content { get; }

        /// <summary>
        /// Gets the content type (for POST requests).
        /// </summary>
        public string? ContentType { get; }

        /// <summary>
        /// Gets the timestamp when the request was made.
        /// </summary>
        public DateTime Timestamp { get; }
    }
}
