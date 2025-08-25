using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Seu.Mail.Core.Models;
using Seu.Mail.Search.Models;

namespace Seu.Mail.Search.Services;

/// <summary>
/// Advanced email search service with comprehensive filtering and ranking capabilities
/// </summary>
public class EmailSearchService
{
    private readonly IEmailRepository _repository;
    private readonly EmailSearchQueryParser _queryParser;
    private readonly ILogger<EmailSearchService> _logger;

    public EmailSearchService(
        IEmailRepository repository,
        EmailSearchQueryParser queryParser,
        ILogger<EmailSearchService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _queryParser = queryParser ?? throw new ArgumentNullException(nameof(queryParser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Searches emails using a Gmail-like query string
    /// </summary>
    /// <param name="query">The search query string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching emails with relevance scoring</returns>
    public async Task<EmailSearchResult> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new EmailSearchResult { Emails = new List<EmailMessage>(), TotalCount = 0 };

        var criteria = _queryParser.Parse(query);
        criteria = _queryParser.Normalize(criteria);

        return await SearchAsync(criteria, cancellationToken);
    }

    /// <summary>
    /// Searches emails using structured search criteria
    /// </summary>
    /// <param name="criteria">The search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching emails with relevance scoring</returns>
    public async Task<EmailSearchResult> SearchAsync(EmailSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing email search with criteria: {Criteria}", criteria.RawQuery ?? "Advanced search");

            var emails = await _repository.GetEmailsAsync(cancellationToken);
            var filteredEmails = ApplyFilters(emails, criteria);
            var rankedEmails = ApplyRanking(filteredEmails, criteria);
            var paginatedEmails = ApplyPagination(rankedEmails, criteria);

            var result = new EmailSearchResult
            {
                Emails = paginatedEmails.ToList(),
                TotalCount = filteredEmails.Count(),
                SearchCriteria = criteria,
                ExecutionTimeMs = 0 // TODO: Add timing
            };

            _logger.LogInformation("Search completed. Found {Count} total matches, returning {PageSize} results",
                result.TotalCount, result.Emails.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing email search");
            return new EmailSearchResult
            {
                Emails = new List<EmailMessage>(),
                TotalCount = 0,
                SearchCriteria = criteria,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Applies all search filters to the email collection
    /// </summary>
    private IEnumerable<EmailMessage> ApplyFilters(IEnumerable<EmailMessage> emails, EmailSearchCriteria criteria)
    {
        var query = emails.AsQueryable();

        // Account filtering
        if (criteria.AccountId.HasValue)
            query = query.Where(e => e.AccountId == criteria.AccountId.Value);

        if (criteria.AccountIds?.Any() == true)
            query = query.Where(e => criteria.AccountIds.Contains(e.AccountId));

        // Folder/Scope filtering
        query = ApplyFolderFilters(query, criteria);

        // Sender/Recipient filtering
        query = ApplyParticipantFilters(query, criteria);

        // Content filtering
        query = ApplyContentFilters(query, criteria);

        // Date filtering
        query = ApplyDateFilters(query, criteria);

        // Size filtering
        query = ApplySizeFilters(query, criteria);

        // Status filtering
        query = ApplyStatusFilters(query, criteria);

        // Attachment filtering
        query = ApplyAttachmentFilters(query, criteria);

        // Priority filtering
        if (criteria.Priority.HasValue)
            query = query.Where(e => e.Priority == criteria.Priority.Value);

        // Thread filtering
        if (!string.IsNullOrEmpty(criteria.ThreadId))
            query = query.Where(e => e.ThreadId == criteria.ThreadId);

        // Label/Tag filtering
        if (criteria.Labels?.Any() == true)
        {
            foreach (var label in criteria.Labels)
            {
                var labelToSearch = label;
                query = query.Where(e => e.Labels != null && e.Labels.Contains(labelToSearch));
            }
        }

        return query;
    }

    /// <summary>
    /// Applies folder and scope filtering
    /// </summary>
    private IQueryable<EmailMessage> ApplyFolderFilters(IQueryable<EmailMessage> query, EmailSearchCriteria criteria)
    {
        // Specific folder name
        if (!string.IsNullOrEmpty(criteria.FolderName))
            query = query.Where(e => e.Folder.ToUpper() == criteria.FolderName.ToUpper());

        // Scope-based filtering
        switch (criteria.Scope)
        {
            case SearchScope.Inbox:
                query = query.Where(e => e.Folder.ToUpper() == "INBOX");
                break;
            case SearchScope.Sent:
                query = query.Where(e => e.Folder.ToUpper() == "SENT");
                break;
            case SearchScope.Drafts:
                query = query.Where(e => e.Folder.ToUpper() == "DRAFTS");
                break;
            case SearchScope.Trash:
                query = query.Where(e => e.Folder.ToUpper() == "TRASH");
                break;
            case SearchScope.Spam:
                query = query.Where(e => e.Folder.ToUpper() == "SPAM" || e.IsSpam);
                break;
            case SearchScope.Starred:
                query = query.Where(e => e.IsImportant);
                break;
            case SearchScope.Important:
                query = query.Where(e => e.IsImportant);
                break;
            case SearchScope.AllFolders:
            default:
                // No folder restriction
                break;
        }

        // Include/exclude deleted and spam
        if (!criteria.IncludeDeleted)
            query = query.Where(e => !e.IsDeleted);

        if (!criteria.IncludeSpam)
            query = query.Where(e => !e.IsSpam);

        return query;
    }

    /// <summary>
    /// Applies sender and recipient filtering
    /// </summary>
    private IQueryable<EmailMessage> ApplyParticipantFilters(IQueryable<EmailMessage> query, EmailSearchCriteria criteria)
    {
        if (!string.IsNullOrEmpty(criteria.From))
            query = query.Where(e => e.From.ToLower().Contains(criteria.From.ToLower()));

        if (!string.IsNullOrEmpty(criteria.To))
            query = query.Where(e => e.To.ToLower().Contains(criteria.To.ToLower()));

        if (!string.IsNullOrEmpty(criteria.Cc))
            query = query.Where(e => e.Cc != null && e.Cc.ToLower().Contains(criteria.Cc.ToLower()));

        if (!string.IsNullOrEmpty(criteria.Bcc))
            query = query.Where(e => e.Bcc != null && e.Bcc.ToLower().Contains(criteria.Bcc.ToLower()));

        return query;
    }

    /// <summary>
    /// Applies content filtering (subject, body, keywords)
    /// </summary>
    private IQueryable<EmailMessage> ApplyContentFilters(IQueryable<EmailMessage> query, EmailSearchCriteria criteria)
    {
        // Exact phrase search (highest priority)
        if (!string.IsNullOrEmpty(criteria.ExactPhrase))
        {
            var phrase = criteria.ExactPhrase.ToLower();
            query = query.Where(e =>
                (e.Subject != null && e.Subject.ToLower().Contains(phrase)) ||
                (e.TextBody != null && e.TextBody.ToLower().Contains(phrase)) ||
                (e.HtmlBody != null && e.HtmlBody.ToLower().Contains(phrase)));
        }

        // Specific subject filtering
        if (!string.IsNullOrEmpty(criteria.Subject))
            query = query.Where(e => e.Subject != null && e.Subject.ToLower().Contains(criteria.Subject.ToLower()));

        // Specific body filtering
        if (!string.IsNullOrEmpty(criteria.Body))
            query = query.Where(e =>
                (e.TextBody != null && e.TextBody.ToLower().Contains(criteria.Body.ToLower())) ||
                (e.HtmlBody != null && e.HtmlBody.ToLower().Contains(criteria.Body.ToLower())));

        // General keyword search (across subject, body, participants)
        if (!string.IsNullOrEmpty(criteria.Keyword))
        {
            var keywords = criteria.Keyword.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var keyword in keywords)
            {
                var kw = keyword;
                query = query.Where(e =>
                    (e.Subject != null && e.Subject.ToLower().Contains(kw)) ||
                    (e.TextBody != null && e.TextBody.ToLower().Contains(kw)) ||
                    (e.HtmlBody != null && e.HtmlBody.ToLower().Contains(kw)) ||
                    e.From.ToLower().Contains(kw) ||
                    e.To.ToLower().Contains(kw) ||
                    (e.Cc != null && e.Cc.ToLower().Contains(kw)) ||
                    (e.Bcc != null && e.Bcc.ToLower().Contains(kw)));
            }
        }

        return query;
    }

    /// <summary>
    /// Applies date range filtering
    /// </summary>
    private IQueryable<EmailMessage> ApplyDateFilters(IQueryable<EmailMessage> query, EmailSearchCriteria criteria)
    {
        if (criteria.After.HasValue)
            query = query.Where(e => e.DateReceived >= criteria.After.Value);

        if (criteria.Before.HasValue)
            query = query.Where(e => e.DateReceived <= criteria.Before.Value);

        if (criteria.On.HasValue)
        {
            var startOfDay = criteria.On.Value.Date;
            var endOfDay = startOfDay.AddDays(1);
            query = query.Where(e => e.DateReceived >= startOfDay && e.DateReceived < endOfDay);
        }

        if (criteria.OlderThanDays.HasValue)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-criteria.OlderThanDays.Value);
            query = query.Where(e => e.DateReceived < cutoffDate);
        }

        if (criteria.NewerThanDays.HasValue)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-criteria.NewerThanDays.Value);
            query = query.Where(e => e.DateReceived > cutoffDate);
        }

        return query;
    }

    /// <summary>
    /// Applies size filtering
    /// </summary>
    private IQueryable<EmailMessage> ApplySizeFilters(IQueryable<EmailMessage> query, EmailSearchCriteria criteria)
    {
        if (criteria.LargerThanBytes.HasValue)
            query = query.Where(e => e.Size > criteria.LargerThanBytes.Value);

        if (criteria.SmallerThanBytes.HasValue)
            query = query.Where(e => e.Size < criteria.SmallerThanBytes.Value);

        return query;
    }

    /// <summary>
    /// Applies status filtering (read, important, etc.)
    /// </summary>
    private IQueryable<EmailMessage> ApplyStatusFilters(IQueryable<EmailMessage> query, EmailSearchCriteria criteria)
    {
        if (criteria.IsRead.HasValue)
            query = query.Where(e => e.IsRead == criteria.IsRead.Value);

        if (criteria.IsUnread.HasValue)
            query = query.Where(e => e.IsRead == !criteria.IsUnread.Value);

        if (criteria.IsStarred.HasValue)
            query = query.Where(e => e.IsImportant == criteria.IsStarred.Value);

        if (criteria.IsImportant.HasValue)
            query = query.Where(e => e.IsImportant == criteria.IsImportant.Value);

        if (criteria.IsDraft.HasValue)
            query = query.Where(e => e.IsDraft == criteria.IsDraft.Value);

        if (criteria.IsSent.HasValue)
        {
            if (criteria.IsSent.Value)
                query = query.Where(e => e.Folder.ToUpper() == "SENT");
            else
                query = query.Where(e => e.Folder.ToUpper() != "SENT");
        }

        if (criteria.IsSpam.HasValue)
            query = query.Where(e => e.IsSpam == criteria.IsSpam.Value);

        if (criteria.IsDeleted.HasValue)
            query = query.Where(e => e.IsDeleted == criteria.IsDeleted.Value);

        return query;
    }

    /// <summary>
    /// Applies attachment filtering
    /// </summary>
    private IQueryable<EmailMessage> ApplyAttachmentFilters(IQueryable<EmailMessage> query, EmailSearchCriteria criteria)
    {
        if (criteria.HasAttachment.HasValue)
            query = query.Where(e => e.HasAttachments == criteria.HasAttachment.Value);

        if (!string.IsNullOrEmpty(criteria.AttachmentName))
        {
            var attachmentName = criteria.AttachmentName.ToLower();
            query = query.Where(e => e.Attachments.Any(a => a.FileName.ToLower().Contains(attachmentName)));
        }

        if (!string.IsNullOrEmpty(criteria.AttachmentType))
        {
            var extension = criteria.AttachmentType.ToLower();
            if (!extension.StartsWith("."))
                extension = "." + extension;
            query = query.Where(e => e.Attachments.Any(a => a.FileName.ToLower().EndsWith(extension)));
        }

        return query;
    }

    /// <summary>
    /// Applies ranking/scoring to search results for relevance
    /// </summary>
    private IEnumerable<EmailMessage> ApplyRanking(IEnumerable<EmailMessage> emails, EmailSearchCriteria criteria)
    {
        if (criteria.SortOrder == SearchSortOrder.Relevance && !string.IsNullOrEmpty(criteria.Keyword))
        {
            return emails.Select(email => new { Email = email, Score = CalculateRelevanceScore(email, criteria) })
                        .OrderByDescending(x => x.Score)
                        .ThenByDescending(x => x.Email.DateReceived)
                        .Select(x => x.Email);
        }

        return criteria.SortOrder switch
        {
            SearchSortOrder.DateAscending => emails.OrderBy(e => e.DateReceived),
            SearchSortOrder.DateDescending => emails.OrderByDescending(e => e.DateReceived),
            SearchSortOrder.SenderAscending => emails.OrderBy(e => e.From),
            SearchSortOrder.SenderDescending => emails.OrderByDescending(e => e.From),
            SearchSortOrder.SubjectAscending => emails.OrderBy(e => e.Subject),
            SearchSortOrder.SubjectDescending => emails.OrderByDescending(e => e.Subject),
            SearchSortOrder.SizeAscending => emails.OrderBy(e => e.Size),
            SearchSortOrder.SizeDescending => emails.OrderByDescending(e => e.Size),
            _ => emails.OrderByDescending(e => e.DateReceived),
        };
    }

    /// <summary>
    /// Calculates relevance score for search ranking
    /// </summary>
    private double CalculateRelevanceScore(EmailMessage email, EmailSearchCriteria criteria)
    {
        double score = 0;

        if (string.IsNullOrEmpty(criteria.Keyword))
            return score;

        var keywords = criteria.Keyword.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var keyword in keywords)
        {
            // Subject matches are worth more
            if (email.Subject?.ToLower().Contains(keyword) == true)
                score += 10;

            // From/To matches
            if (email.From.ToLower().Contains(keyword))
                score += 5;
            if (email.To.ToLower().Contains(keyword))
                score += 3;

            // Body matches
            if (email.TextBody?.ToLower().Contains(keyword) == true)
                score += 2;
            if (email.HtmlBody?.ToLower().Contains(keyword) == true)
                score += 2;

            // Exact matches get bonus points
            if (email.Subject?.ToLower() == keyword)
                score += 20;
        }

        // Boost for recent emails
        var daysSinceReceived = (DateTime.UtcNow - email.DateReceived).TotalDays;
        if (daysSinceReceived < 7)
            score += 5;
        else if (daysSinceReceived < 30)
            score += 2;

        // Boost for important emails
        if (email.IsImportant)
            score += 3;

        // Boost for unread emails
        if (!email.IsRead)
            score += 1;

        return score;
    }

    /// <summary>
    /// Applies pagination to search results
    /// </summary>
    private IEnumerable<EmailMessage> ApplyPagination(IEnumerable<EmailMessage> emails, EmailSearchCriteria criteria)
    {
        return emails.Skip(criteria.Offset).Take(criteria.MaxResults);
    }

    /// <summary>
    /// Gets search suggestions based on partial query
    /// </summary>
    public async Task<List<string>> GetSearchSuggestionsAsync(string partialQuery, CancellationToken cancellationToken = default)
    {
        var suggestions = new List<string>();

        if (string.IsNullOrWhiteSpace(partialQuery))
            return suggestions;

        try
        {
            // Add common search operators
            var operators = new[]
            {
                "from:", "to:", "cc:", "bcc:", "subject:", "body:", "has:attachment",
                "is:read", "is:unread", "is:important", "is:starred", "is:draft",
                "before:", "after:", "larger:", "smaller:", "in:inbox", "in:sent"
            };

            suggestions.AddRange(operators.Where(op => op.StartsWith(partialQuery, StringComparison.OrdinalIgnoreCase)));

            // Add recent sender suggestions
            var emails = await _repository.GetRecentEmailsAsync(100, cancellationToken);
            var senders = emails.Select(e => e.From)
                               .Where(from => from.ToLower().Contains(partialQuery.ToLower()))
                               .Distinct()
                               .Take(5);

            suggestions.AddRange(senders.Select(sender => $"from:{sender}"));

            return suggestions.Take(10).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating search suggestions");
            return suggestions;
        }
    }
}
