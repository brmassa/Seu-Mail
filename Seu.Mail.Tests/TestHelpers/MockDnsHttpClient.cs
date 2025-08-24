using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Seu.Mail.Contracts.Services;

namespace Seu.Mail.Tests.TestHelpers;

/// <summary>
/// Mock implementation of IDnsHttpClient for unit testing.
/// Allows tests to configure DNS responses and verify DNS operations without making actual network calls.
/// </summary>
public class MockDnsHttpClient : IDnsHttpClient
{
    private readonly Dictionary<string, string> _dnsResponses = new();
    private readonly List<DnsRequest> _requestHistory = new();

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    public string? UserAgent { get; private set; }

    /// <summary>
    /// Gets the history of all DNS requests made through this mock client.
    /// </summary>
    public IReadOnlyList<DnsRequest> RequestHistory => _requestHistory.AsReadOnly();

    /// <summary>
    /// Configures a mock response for DNS requests to the specified URL.
    /// </summary>
    /// <param name="url">The DNS URL to mock.</param>
    /// <param name="response">The response to return. Use null to simulate a failed request.</param>
    public void SetupDnsResponse(string url, string? response)
    {
        if (response == null)
        {
            _dnsResponses.Remove(url);
        }
        else
        {
            _dnsResponses[url] = response;
        }
    }

    /// <summary>
    /// Configures a mock response for MX record queries for a specific domain.
    /// </summary>
    /// <param name="domain">The domain to mock MX records for.</param>
    /// <param name="mxRecords">Array of MX record hostnames to return.</param>
    public void SetupMxRecords(string domain, string[]? mxRecords)
    {
        var url = $"https://dns.google/resolve?name={domain}&type=MX";

        if (mxRecords == null || mxRecords.Length == 0)
        {
            SetupDnsResponse(url, @"{""Status"":0,""Answer"":[]}");
        }
        else
        {
            var answers = string.Join(",",
                Array.ConvertAll(mxRecords, mx => $@"{{""data"":""10 {mx}""}}"));

            var response = $@"{{""Status"":0,""Answer"":[{answers}]}}";
            SetupDnsResponse(url, response);
        }
    }

    /// <summary>
    /// Clears all configured responses and request history.
    /// </summary>
    public void Reset()
    {
        _dnsResponses.Clear();
        _requestHistory.Clear();
        UserAgent = null;
    }

    /// <inheritdoc />
    public Task<string?> GetStringAsync(string url, CancellationToken cancellationToken)
    {
        _requestHistory.Add(new DnsRequest(url, cancellationToken.IsCancellationRequested));

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult<string?>(null);
        }

        if (_dnsResponses.TryGetValue(url, out var response))
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
    /// Represents a DNS request made through the mock client.
    /// </summary>
    public class DnsRequest
    {
        public DnsRequest(string url, bool wasCancelled)
        {
            Url = url;
            WasCancelled = wasCancelled;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the DNS request URL.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets whether the request was cancelled.
        /// </summary>
        public bool WasCancelled { get; }

        /// <summary>
        /// Gets the timestamp when the request was made.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Extracts the domain being queried from the DNS URL.
        /// </summary>
        public string? Domain
        {
            get
            {
                var match = System.Text.RegularExpressions.Regex.Match(Url, @"name=([^&]+)");
                return match.Success ? match.Groups[1].Value : null;
            }
        }

        /// <summary>
        /// Extracts the record type being queried from the DNS URL.
        /// </summary>
        public string? RecordType
        {
            get
            {
                var match = System.Text.RegularExpressions.Regex.Match(Url, @"type=([^&]+)");
                return match.Success ? match.Groups[1].Value : null;
            }
        }
    }
}
