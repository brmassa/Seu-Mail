using System;

namespace Seu.Mail.Search.Models;

/// <summary>
/// Configuration options for the email search service
/// </summary>
public class EmailSearchOptions
{
    /// <summary>
    /// Default maximum number of search results to return
    /// </summary>
    public int DefaultMaxResults { get; set; } = 50;

    /// <summary>
    /// Maximum allowed number of search results (prevents excessive memory usage)
    /// </summary>
    public int MaxAllowedResults { get; set; } = 1000;

    /// <summary>
    /// Enable relevance scoring for search results
    /// </summary>
    public bool EnableRelevanceScoring { get; set; } = true;

    /// <summary>
    /// Enable search suggestions and autocomplete
    /// </summary>
    public bool EnableSearchSuggestions { get; set; } = true;

    /// <summary>
    /// Minimum query length before search is executed
    /// </summary>
    public int MinimumQueryLength { get; set; } = 1;

    /// <summary>
    /// Search timeout in seconds
    /// </summary>
    public int SearchTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether search should be case sensitive
    /// </summary>
    public bool CaseSensitiveSearch { get; set; } = false;

    /// <summary>
    /// Include deleted emails in search results
    /// </summary>
    public bool IncludeDeletedEmails { get; set; } = false;

    /// <summary>
    /// Include spam emails in search results
    /// </summary>
    public bool IncludeSpamEmails { get; set; } = false;

    /// <summary>
    /// Enable fuzzy matching for keywords
    /// </summary>
    public bool EnableFuzzyMatching { get; set; } = false;

    /// <summary>
    /// Maximum edit distance for fuzzy matching
    /// </summary>
    public int FuzzyMatchingMaxDistance { get; set; } = 2;

    /// <summary>
    /// Enable caching of search results
    /// </summary>
    public bool EnableResultCaching { get; set; } = true;

    /// <summary>
    /// Cache duration in minutes
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Enable performance optimization with ZLinq
    /// </summary>
    public bool EnableZLinqOptimizations { get; set; } = true;

    /// <summary>
    /// Enable parallel processing for large result sets
    /// </summary>
    public bool EnableParallelProcessing { get; set; } = true;

    /// <summary>
    /// Minimum number of items before parallel processing is used
    /// </summary>
    public int ParallelProcessingThreshold { get; set; } = 100;

    /// <summary>
    /// Enable detailed search analytics and logging
    /// </summary>
    public bool EnableSearchAnalytics { get; set; } = false;

    /// <summary>
    /// Default page size for paginated results
    /// </summary>
    public int DefaultPageSize { get; set; } = 25;

    /// <summary>
    /// Maximum number of search suggestions to return
    /// </summary>
    public int MaxSearchSuggestions { get; set; } = 10;

    /// <summary>
    /// Enable highlighting of search terms in results
    /// </summary>
    public bool EnableSearchHighlighting { get; set; } = true;

    /// <summary>
    /// Maximum length of content preview in search results
    /// </summary>
    public int MaxContentPreviewLength { get; set; } = 200;

    /// <summary>
    /// Enable search history tracking
    /// </summary>
    public bool EnableSearchHistory { get; set; } = true;

    /// <summary>
    /// Maximum number of search history entries to keep
    /// </summary>
    public int MaxSearchHistoryEntries { get; set; } = 50;

    /// <summary>
    /// Boost factor for subject line matches (relevance scoring)
    /// </summary>
    public double SubjectMatchBoost { get; set; } = 2.0;

    /// <summary>
    /// Boost factor for sender matches (relevance scoring)
    /// </summary>
    public double SenderMatchBoost { get; set; } = 1.5;

    /// <summary>
    /// Boost factor for recent emails (relevance scoring)
    /// </summary>
    public double RecencyBoost { get; set; } = 1.2;

    /// <summary>
    /// Boost factor for important/starred emails (relevance scoring)
    /// </summary>
    public double ImportanceBoost { get; set; } = 1.8;

    /// <summary>
    /// Enable advanced query operators (AND, OR, NOT)
    /// </summary>
    public bool EnableAdvancedOperators { get; set; } = true;

    /// <summary>
    /// Enable regex pattern matching in search queries
    /// </summary>
    public bool EnableRegexPatterns { get; set; } = false;

    /// <summary>
    /// Validate configuration settings
    /// </summary>
    public void Validate()
    {
        if (DefaultMaxResults <= 0)
            throw new ArgumentException("DefaultMaxResults must be greater than 0");

        if (MaxAllowedResults <= 0)
            throw new ArgumentException("MaxAllowedResults must be greater than 0");

        if (DefaultMaxResults > MaxAllowedResults)
            throw new ArgumentException("DefaultMaxResults cannot be greater than MaxAllowedResults");

        if (SearchTimeoutSeconds <= 0)
            throw new ArgumentException("SearchTimeoutSeconds must be greater than 0");

        if (MinimumQueryLength < 0)
            throw new ArgumentException("MinimumQueryLength cannot be negative");

        if (FuzzyMatchingMaxDistance < 0)
            throw new ArgumentException("FuzzyMatchingMaxDistance cannot be negative");

        if (CacheDurationMinutes <= 0)
            throw new ArgumentException("CacheDurationMinutes must be greater than 0");

        if (ParallelProcessingThreshold <= 0)
            throw new ArgumentException("ParallelProcessingThreshold must be greater than 0");

        if (DefaultPageSize <= 0)
            throw new ArgumentException("DefaultPageSize must be greater than 0");

        if (MaxSearchSuggestions <= 0)
            throw new ArgumentException("MaxSearchSuggestions must be greater than 0");

        if (MaxContentPreviewLength <= 0)
            throw new ArgumentException("MaxContentPreviewLength must be greater than 0");

        if (MaxSearchHistoryEntries <= 0)
            throw new ArgumentException("MaxSearchHistoryEntries must be greater than 0");
    }
}
