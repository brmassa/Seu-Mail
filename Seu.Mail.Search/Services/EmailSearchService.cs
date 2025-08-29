using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Seu.Mail.Core.Models;
using Seu.Mail.Search.Models;
using ZLinq;

namespace Seu.Mail.Search.Services;

/// <summary>
/// Advanced email search service with comprehensive filtering and ranking capabilities
/// </summary>
public class EmailSearchService : IEmailSearchService
{
    private readonly IEmailRepository _repository;
    private readonly EmailSearchQueryParser _queryParser;
    private readonly ILogger<EmailSearchService> _logger;

    public EmailSearchService(
        IEmailRepository repository,
        EmailSearchQueryParser queryParser,
        ILogger<EmailSearchService> logger)
    {
        _repository = repository;
        _queryParser = queryParser;
        _logger = logger;
    }

    /// <summary>
    /// Searches emails using advanced criteria
    /// </summary>
    public async Task<EmailSearchResult> SearchAsync(
        EmailSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new EmailSearchResult
        {
            SearchCriteria = criteria
        };

        try
        {
            _logger.LogDebug("Starting email search with criteria: {@Criteria}", criteria);

            var query = _repository.GetEmailsQuery();

            // Apply filters using ZLinq for performance
            query = ApplyFilters(query, criteria);

            // Get total count before pagination
            result.TotalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, criteria);

            // Apply pagination
            if (criteria.Offset > 0)
            {
                query = query.Skip(criteria.Offset);
            }

            query = query.Take(criteria.MaxResults);

            // Execute query
            result.Emails = await query.ToListAsync(cancellationToken);

            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation("Search completed: {Count} results in {Ms}ms",
                result.Emails.Count, result.ExecutionTimeMs);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            result.Error = ex.Message;

            _logger.LogError(ex, "Error during email search");
            return result;
        }
    }

    /// <summary>
    /// Searches emails using a raw query string (Gmail-like syntax)
    /// </summary>
    public async Task<EmailSearchResult> SearchAsync(
        string rawQuery,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new EmailSearchResult();

        try
        {
            // Parse the raw query into structured criteria
            var criteria = _queryParser.Parse(rawQuery);
            criteria.RawQuery = rawQuery;

            result = await SearchAsync(criteria, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            result.Error = $"Query parsing failed: {ex.Message}";

            _logger.LogError(ex, "Error parsing search query: {Query}", rawQuery);
            return result;
        }
    }

    /// <summary>
    /// Apply search filters to the query using ZLinq for performance
    /// </summary>
    private IQueryable<EmailMessage> ApplyFilters(IQueryable<EmailMessage> query, EmailSearchCriteria criteria)
    {
        // Text-based filters
        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var keyword = criteria.Keyword.ToLower();
            query = query.Where(e =>
                e.Subject.ToLower().Contains(keyword) ||
                e.TextBody.ToLower().Contains(keyword) ||
                e.From.ToLower().Contains(keyword) ||
                e.To.ToLower().Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(criteria.From))
        {
            var from = criteria.From.ToLower();
            query = query.Where(e => e.From.ToLower().Contains(from));
        }

        if (!string.IsNullOrWhiteSpace(criteria.To))
        {
            var to = criteria.To.ToLower();
            query = query.Where(e => e.To.ToLower().Contains(to));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Cc))
        {
            var cc = criteria.Cc.ToLower();
            query = query.Where(e => e.Cc != null && e.Cc.ToLower().Contains(cc));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Bcc))
        {
            var bcc = criteria.Bcc.ToLower();
            query = query.Where(e => e.Bcc != null && e.Bcc.ToLower().Contains(bcc));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Subject))
        {
            var subject = criteria.Subject.ToLower();
            query = query.Where(e => e.Subject.ToLower().Contains(subject));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Body))
        {
            var body = criteria.Body.ToLower();
            query = query.Where(e => e.TextBody.ToLower().Contains(body));
        }

        // Date filters
        if (criteria.After.HasValue)
        {
            query = query.Where(e => e.DateReceived >= criteria.After.Value);
        }

        if (criteria.Before.HasValue)
        {
            query = query.Where(e => e.DateReceived <= criteria.Before.Value);
        }

        if (criteria.On.HasValue)
        {
            var onDate = criteria.On.Value.Date;
            query = query.Where(e => e.DateReceived.Date == onDate);
        }

        if (criteria.OlderThanDays.HasValue)
        {
            var cutoffDate = DateTime.Now.AddDays(-criteria.OlderThanDays.Value);
            query = query.Where(e => e.DateReceived < cutoffDate);
        }

        if (criteria.NewerThanDays.HasValue)
        {
            var cutoffDate = DateTime.Now.AddDays(-criteria.NewerThanDays.Value);
            query = query.Where(e => e.DateReceived > cutoffDate);
        }

        // Size filters
        if (criteria.LargerThanBytes.HasValue)
        {
            query = query.Where(e => e.Size > criteria.LargerThanBytes.Value);
        }

        if (criteria.SmallerThanBytes.HasValue)
        {
            query = query.Where(e => e.Size < criteria.SmallerThanBytes.Value);
        }

        // Status filters
        if (criteria.IsRead.HasValue)
        {
            query = query.Where(e => e.IsRead == criteria.IsRead.Value);
        }

        if (criteria.IsUnread.HasValue)
        {
            query = query.Where(e => e.IsRead == !criteria.IsUnread.Value);
        }

        if (criteria.IsImportant.HasValue)
        {
            query = query.Where(e => e.IsImportant == criteria.IsImportant.Value);
        }

        if (criteria.IsDraft.HasValue)
        {
            query = query.Where(e => e.IsDraft == criteria.IsDraft.Value);
        }

        // Attachment filters
        if (criteria.HasAttachment.HasValue)
        {
            query = query.Where(e => e.HasAttachments == criteria.HasAttachment.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.AttachmentName))
        {
            var attachmentName = criteria.AttachmentName.ToLower();
            query = query.Where(e => e.Attachments.Any(a => a.FileName.ToLower().Contains(attachmentName)));
        }

        if (!string.IsNullOrWhiteSpace(criteria.AttachmentType))
        {
            var attachmentType = criteria.AttachmentType.ToLower();
            query = query.Where(e => e.Attachments.Any(a => a.ContentType.ToLower().Contains(attachmentType)));
        }

        // Folder filters
        if (!string.IsNullOrWhiteSpace(criteria.FolderName))
        {
            query = query.Where(e => e.Folder == criteria.FolderName);
        }

        // Account filters
        if (criteria.AccountId.HasValue)
        {
            query = query.Where(e => e.AccountId == criteria.AccountId.Value);
        }

        if (criteria.AccountIds?.Any() == true)
        {
            query = query.Where(e => criteria.AccountIds.Contains(e.AccountId));
        }

        // Exclude deleted emails unless specifically requested
        if (!criteria.IncludeDeleted)
        {
            query = query.Where(e => e.Folder != "Deleted Items" && e.Folder != "Trash");
        }

        // Exclude spam unless specifically requested
        if (!criteria.IncludeSpam)
        {
            query = query.Where(e => e.Folder != "Spam" && e.Folder != "Junk Email");
        }

        return query;
    }

    /// <summary>
    /// Apply sorting to the query
    /// </summary>
    private IQueryable<EmailMessage> ApplySorting(IQueryable<EmailMessage> query, EmailSearchCriteria criteria)
    {
        return criteria.SortOrder switch
        {
            SearchSortOrder.DateAscending => query.OrderBy(e => e.DateReceived),
            SearchSortOrder.DateDescending => query.OrderByDescending(e => e.DateReceived),
            SearchSortOrder.SenderAscending => query.OrderBy(e => e.From),
            SearchSortOrder.SenderDescending => query.OrderByDescending(e => e.From),
            SearchSortOrder.SubjectAscending => query.OrderBy(e => e.Subject),
            SearchSortOrder.SubjectDescending => query.OrderByDescending(e => e.Subject),
            SearchSortOrder.SizeAscending => query.OrderBy(e => e.Size),
            SearchSortOrder.SizeDescending => query.OrderByDescending(e => e.Size),
            SearchSortOrder.Relevance => query.OrderByDescending(e => e.DateReceived), // Default to date for relevance
            _ => query.OrderByDescending(e => e.DateReceived)
        };
    }

    /// <summary>
    /// Gets search suggestions based on partial input
    /// </summary>
    public async Task<List<string>> GetSearchSuggestionsAsync(
        string partialQuery,
        int maxSuggestions = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var suggestions = new List<string>();

            if (string.IsNullOrWhiteSpace(partialQuery))
            {
                return suggestions;
            }

            var query = _repository.GetEmailsQuery();
            var partial = partialQuery.ToLower();

            // Get sender suggestions
            var senderSuggestions = await query
                .Where(e => e.From.ToLower().Contains(partial))
                .Select(e => e.From)
                .Distinct()
                .Take(maxSuggestions / 2)
                .ToListAsync(cancellationToken);

            suggestions.AddRange(senderSuggestions.Select(s => $"from:{s}"));

            // Get subject suggestions
            var subjectSuggestions = await query
                .Where(e => e.Subject.ToLower().Contains(partial))
                .Select(e => e.Subject)
                .Distinct()
                .Take(maxSuggestions / 2)
                .ToListAsync(cancellationToken);

            suggestions.AddRange(subjectSuggestions.Select(s => $"subject:{s}"));

            return suggestions.Take(maxSuggestions).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search suggestions for: {Query}", partialQuery);
            return new List<string>();
        }
    }

    /// <summary>
    /// Gets popular search terms
    /// </summary>
    public async Task<List<string>> GetPopularSearchTermsAsync(
        int maxTerms = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _repository.GetEmailsQuery();

            // Get most common senders
            var popularSenders = await query
                .GroupBy(e => e.From)
                .OrderByDescending(g => g.Count())
                .Take(maxTerms / 2)
                .Select(g => $"from:{g.Key}")
                .ToListAsync(cancellationToken);

            // Get most common subjects (words)
            var popularSubjects = await query
                .Where(e => !string.IsNullOrEmpty(e.Subject))
                .GroupBy(e => e.Subject)
                .OrderByDescending(g => g.Count())
                .Take(maxTerms / 2)
                .Select(g => g.Key)
                .ToListAsync(cancellationToken);

            var results = new List<string>();
            results.AddRange(popularSenders);
            results.AddRange(popularSubjects);

            return results.Take(maxTerms).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular search terms");
            return new List<string>();
        }
    }
}
