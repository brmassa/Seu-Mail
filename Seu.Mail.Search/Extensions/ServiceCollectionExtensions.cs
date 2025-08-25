using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Seu.Mail.Data.Context;
using Seu.Mail.Search.Services;

namespace Seu.Mail.Search.Extensions;

/// <summary>
/// Extension methods for configuring search services in dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds email search services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional configuration for search options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEmailSearch(this IServiceCollection services, Action<EmailSearchOptions>? configureOptions = null)
    {
        // Configure options
        var options = new EmailSearchOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        // Register search services
        services.AddScoped<EmailSearchQueryParser>();
        services.AddScoped<EmailSearchService>();
        services.AddScoped<IEmailRepository, EmailRepository>();

        return services;
    }

    /// <summary>
    /// Adds email search services with custom repository implementation
    /// </summary>
    /// <typeparam name="TRepository">Custom repository implementation</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional configuration for search options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEmailSearch<TRepository>(this IServiceCollection services, Action<EmailSearchOptions>? configureOptions = null)
        where TRepository : class, IEmailRepository
    {
        // Configure options
        var options = new EmailSearchOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        // Register search services with custom repository
        services.AddScoped<EmailSearchQueryParser>();
        services.AddScoped<EmailSearchService>();
        services.AddScoped<IEmailRepository, TRepository>();

        return services;
    }
}

/// <summary>
/// Configuration options for email search functionality
/// </summary>
public class EmailSearchOptions
{
    /// <summary>
    /// Default maximum number of search results to return
    /// </summary>
    public int DefaultMaxResults { get; set; } = 50;

    /// <summary>
    /// Maximum allowed results per search query
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
    /// Maximum number of search suggestions to return
    /// </summary>
    public int MaxSuggestions { get; set; } = 10;

    /// <summary>
    /// Cache search results for improved performance
    /// </summary>
    public bool EnableResultCaching { get; set; } = false;

    /// <summary>
    /// Cache duration for search results in minutes
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 5;

    /// <summary>
    /// Enable search analytics and logging
    /// </summary>
    public bool EnableSearchAnalytics { get; set; } = true;

    /// <summary>
    /// Minimum query length before triggering search
    /// </summary>
    public int MinimumQueryLength { get; set; } = 1;

    /// <summary>
    /// Include deleted emails in search results
    /// </summary>
    public bool IncludeDeletedEmails { get; set; } = false;

    /// <summary>
    /// Include spam emails in search results
    /// </summary>
    public bool IncludeSpamEmails { get; set; } = false;

    /// <summary>
    /// Default search timeout in seconds
    /// </summary>
    public int SearchTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable full-text search indexing (requires additional setup)
    /// </summary>
    public bool EnableFullTextSearch { get; set; } = false;

    /// <summary>
    /// Case sensitivity for search operations
    /// </summary>
    public bool CaseSensitiveSearch { get; set; } = false;
}
