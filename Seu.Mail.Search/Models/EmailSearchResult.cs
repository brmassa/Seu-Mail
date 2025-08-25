using System;
using System.Collections.Generic;
using Seu.Mail.Core.Models;

namespace Seu.Mail.Search.Models;

/// <summary>
/// Represents the result of an email search operation
/// </summary>
public class EmailSearchResult
{
    /// <summary>
    /// The emails found by the search
    /// </summary>
    public List<EmailMessage> Emails { get; set; } = new();

    /// <summary>
    /// Total count of matching emails (before pagination)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The search criteria that was used
    /// </summary>
    public EmailSearchCriteria? SearchCriteria { get; set; }

    /// <summary>
    /// Time taken to execute the search in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Error message if the search failed
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Whether the search completed successfully
    /// </summary>
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    /// <summary>
    /// Whether there are more results available
    /// </summary>
    public bool HasMore => SearchCriteria != null &&
                          (SearchCriteria.Offset + Emails.Count) < TotalCount;

    /// <summary>
    /// The current page number (1-based)
    /// </summary>
    public int CurrentPage => SearchCriteria != null ?
                             (SearchCriteria.Offset / Math.Max(SearchCriteria.MaxResults, 1)) + 1 : 1;

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => SearchCriteria != null ?
                            (int)Math.Ceiling((double)TotalCount / Math.Max(SearchCriteria.MaxResults, 1)) : 1;

    /// <summary>
    /// Search performance metrics
    /// </summary>
    public SearchMetrics? Metrics { get; set; }
}

/// <summary>
/// Search performance and debugging metrics
/// </summary>
public class SearchMetrics
{
    /// <summary>
    /// Time spent parsing the query
    /// </summary>
    public long QueryParsingMs { get; set; }

    /// <summary>
    /// Time spent filtering emails
    /// </summary>
    public long FilteringMs { get; set; }

    /// <summary>
    /// Time spent ranking/sorting results
    /// </summary>
    public long RankingMs { get; set; }

    /// <summary>
    /// Number of emails examined before filtering
    /// </summary>
    public int EmailsExamined { get; set; }

    /// <summary>
    /// Number of emails that passed all filters
    /// </summary>
    public int EmailsFiltered { get; set; }

    /// <summary>
    /// Average relevance score of results (if using relevance sorting)
    /// </summary>
    public double? AverageRelevanceScore { get; set; }
}
