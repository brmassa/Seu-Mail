using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Seu.Mail.Search.Models;

namespace Seu.Mail.Search.Services;

/// <summary>
/// Interface for advanced email search functionality
/// </summary>
public interface IEmailSearchService
{
    /// <summary>
    /// Searches emails using structured search criteria
    /// </summary>
    /// <param name="criteria">Search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results</returns>
    Task<EmailSearchResult> SearchAsync(
        EmailSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches emails using a raw query string (Gmail-like syntax)
    /// </summary>
    /// <param name="rawQuery">Raw search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results</returns>
    Task<EmailSearchResult> SearchAsync(
        string rawQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets search suggestions based on partial input
    /// </summary>
    /// <param name="partialQuery">Partial search query</param>
    /// <param name="maxSuggestions">Maximum number of suggestions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of search suggestions</returns>
    Task<List<string>> GetSearchSuggestionsAsync(
        string partialQuery,
        int maxSuggestions = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets popular search terms
    /// </summary>
    /// <param name="maxTerms">Maximum number of terms</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of popular search terms</returns>
    Task<List<string>> GetPopularSearchTermsAsync(
        int maxTerms = 20,
        CancellationToken cancellationToken = default);
}
