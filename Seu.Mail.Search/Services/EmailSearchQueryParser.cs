using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Seu.Mail.Search.Models;

namespace Seu.Mail.Search.Services;

/// <summary>
/// Parses Gmail-like search queries into structured search criteria
/// </summary>
public class EmailSearchQueryParser
{
    private readonly ILogger<EmailSearchQueryParser>? _logger;

    // Regex patterns for parsing search operators
    private static readonly Regex KeyValuePattern = new(@"(\w+):([^\s]+|""[^""]*"")", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex QuotedPattern = new(@"""([^""]*)""", RegexOptions.Compiled);
    private static readonly Regex SizePattern = new(@"(\d+(?:\.\d+)?)(b|kb|mb|gb)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DatePattern = new(@"(\d{4}-\d{2}-\d{2}|\d{4}/\d{2}/\d{2}|\d{2}/\d{2}/\d{4}|\d{4}-\d{2}|\d{4}/\d{2}|\d{4})", RegexOptions.Compiled);
    private static readonly Regex RelativeDatePattern = new(@"(\d+)(d|day|days|w|week|weeks|m|month|months|y|year|years)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public EmailSearchQueryParser(ILogger<EmailSearchQueryParser>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parses a Gmail-like search query string into structured search criteria
    /// </summary>
    /// <param name="query">The search query string</param>
    /// <returns>Parsed search criteria</returns>
    public EmailSearchCriteria Parse(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new EmailSearchCriteria();

        var criteria = new EmailSearchCriteria
        {
            RawQuery = query.Trim()
        };

        try
        {
            var tokens = TokenizeQuery(query);
            var keywordTokens = new List<string>();

            foreach (var token in tokens)
            {
                if (!ProcessToken(token, criteria))
                {
                    // If not a special operator, treat as keyword
                    keywordTokens.Add(token);
                }
            }

            // Combine remaining tokens as general keyword search
            if (keywordTokens.Any())
            {
                criteria.Keyword = string.Join(" ", keywordTokens);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error parsing search query: {Query}", query);
            // Fallback: treat entire query as keyword search
            criteria.Keyword = query;
        }

        return criteria;
    }

    /// <summary>
    /// Tokenizes the query into individual search terms and operators
    /// </summary>
    private List<string> TokenizeQuery(string query)
    {
        var tokens = new List<string>();
        var currentToken = string.Empty;
        var inQuotes = false;
        var escapeNext = false;

        for (var i = 0; i < query.Length; i++)
        {
            var ch = query[i];

            if (escapeNext)
            {
                currentToken += ch;
                escapeNext = false;
                continue;
            }

            if (ch == '\\')
            {
                escapeNext = true;
                continue;
            }

            if (ch == '"')
            {
                inQuotes = !inQuotes;
                currentToken += ch;
                continue;
            }

            if (char.IsWhiteSpace(ch) && !inQuotes)
            {
                if (!string.IsNullOrEmpty(currentToken))
                {
                    tokens.Add(currentToken.Trim());
                    currentToken = string.Empty;
                }
            }
            else
            {
                currentToken += ch;
            }
        }

        if (!string.IsNullOrEmpty(currentToken))
        {
            tokens.Add(currentToken.Trim());
        }

        return tokens;
    }

    /// <summary>
    /// Processes a single token and updates search criteria
    /// </summary>
    /// <returns>True if token was processed as an operator, false if it should be treated as keyword</returns>
    private bool ProcessToken(string token, EmailSearchCriteria criteria)
    {
        if (string.IsNullOrEmpty(token))
            return true;

        // Handle quoted phrases
        if (token.StartsWith("\"") && token.EndsWith("\"") && token.Length > 2)
        {
            criteria.ExactPhrase = token.Substring(1, token.Length - 2);
            return true;
        }

        // Handle key:value patterns
        var keyValueMatch = KeyValuePattern.Match(token);
        if (keyValueMatch.Success)
        {
            var key = keyValueMatch.Groups[1].Value.ToLowerInvariant();
            var value = keyValueMatch.Groups[2].Value;

            // Remove quotes from value if present
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 2);

            return ProcessKeyValuePair(key, value, criteria);
        }

        // Handle special boolean operators
        switch (token.ToLowerInvariant())
        {
            case "is:read":
                criteria.IsRead = true;
                return true;
            case "is:unread":
                criteria.IsUnread = true;
                return true;
            case "is:starred":
                criteria.IsStarred = true;
                return true;
            case "is:important":
                criteria.IsImportant = true;
                return true;
            case "is:draft":
                criteria.IsDraft = true;
                return true;
            case "is:sent":
                criteria.IsSent = true;
                return true;
            case "has:attachment":
                criteria.HasAttachment = true;
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Processes key:value pairs from the search query
    /// </summary>
    private bool ProcessKeyValuePair(string key, string value, EmailSearchCriteria criteria)
    {
        switch (key)
        {
            case "from":
                criteria.From = value;
                return true;

            case "to":
                criteria.To = value;
                return true;

            case "cc":
                criteria.Cc = value;
                return true;

            case "bcc":
                criteria.Bcc = value;
                return true;

            case "subject":
                criteria.Subject = value;
                return true;

            case "body":
                criteria.Body = value;
                return true;

            case "filename":
                criteria.AttachmentName = value;
                return true;

            case "filetype":
                criteria.AttachmentType = value;
                return true;

            case "folder":
                criteria.FolderName = value;
                return true;

            case "in":
                ProcessInOperator(value, criteria);
                return true;

            case "before":
                criteria.Before = ParseDate(value);
                return true;

            case "after":
                criteria.After = ParseDate(value);
                return true;

            case "on":
                criteria.On = ParseDate(value);
                return true;

            case "older":
                criteria.OlderThanDays = ParseRelativeDate(value);
                return true;

            case "newer":
                criteria.NewerThanDays = ParseRelativeDate(value);
                return true;

            case "larger":
                criteria.LargerThanBytes = ParseSize(value);
                return true;

            case "smaller":
                criteria.SmallerThanBytes = ParseSize(value);
                return true;

            case "size":
                ProcessSizeOperator(value, criteria);
                return true;

            case "priority":
                if (int.TryParse(value, out var priority))
                    criteria.Priority = priority;
                return true;

            case "account":
                if (int.TryParse(value, out var accountId))
                    criteria.AccountId = accountId;
                return true;

            case "label":
            case "tag":
                criteria.Labels ??= new List<string>();
                criteria.Labels.Add(value);
                return true;

            case "thread":
                criteria.ThreadId = value;
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Processes the "in:" operator for folder/scope specification
    /// </summary>
    private void ProcessInOperator(string value, EmailSearchCriteria criteria)
    {
        switch (value.ToLowerInvariant())
        {
            case "inbox":
                criteria.Scope = SearchScope.Inbox;
                criteria.FolderName = "INBOX";
                break;
            case "sent":
                criteria.Scope = SearchScope.Sent;
                criteria.FolderName = "SENT";
                break;
            case "drafts":
                criteria.Scope = SearchScope.Drafts;
                criteria.FolderName = "DRAFTS";
                break;
            case "trash":
                criteria.Scope = SearchScope.Trash;
                criteria.FolderName = "TRASH";
                break;
            case "spam":
                criteria.Scope = SearchScope.Spam;
                criteria.FolderName = "SPAM";
                break;
            case "starred":
                criteria.Scope = SearchScope.Starred;
                criteria.IsStarred = true;
                break;
            case "important":
                criteria.Scope = SearchScope.Important;
                criteria.IsImportant = true;
                break;
            case "anywhere":
                criteria.Scope = SearchScope.AllFolders;
                break;
            default:
                criteria.FolderName = value;
                break;
        }
    }

    /// <summary>
    /// Processes size operators (e.g., "size:&gt;10MB", "size:&lt;500KB")
    /// </summary>
    private void ProcessSizeOperator(string value, EmailSearchCriteria criteria)
    {
        if (value.StartsWith(">"))
        {
            criteria.LargerThanBytes = ParseSize(value.Substring(1));
        }
        else if (value.StartsWith("<"))
        {
            criteria.SmallerThanBytes = ParseSize(value.Substring(1));
        }
        else
        {
            // Exact size match (treat as larger than)
            criteria.LargerThanBytes = ParseSize(value);
        }
    }

    /// <summary>
    /// Parses date strings in various formats
    /// </summary>
    private DateTime? ParseDate(string dateString)
    {
        if (string.IsNullOrEmpty(dateString))
            return null;

        // Try relative date first
        var relativeDays = ParseRelativeDate(dateString);
        if (relativeDays.HasValue)
        {
            return DateTime.UtcNow.AddDays(-relativeDays.Value);
        }

        // Try absolute date parsing
        var formats = new[]
        {
            "yyyy-MM-dd",
            "yyyy/MM/dd",
            "MM/dd/yyyy",
            "yyyy-MM",
            "yyyy/MM",
            "yyyy"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }
        }

        // Fallback to general parsing
        if (DateTime.TryParse(dateString, out var fallbackDate))
        {
            return DateTime.SpecifyKind(fallbackDate, DateTimeKind.Utc);
        }

        return null;
    }

    /// <summary>
    /// Parses relative date expressions (e.g., "7d", "2w", "1m")
    /// </summary>
    private int? ParseRelativeDate(string relativeDateString)
    {
        var match = RelativeDatePattern.Match(relativeDateString);
        if (!match.Success)
            return null;

        if (!int.TryParse(match.Groups[1].Value, out var number))
            return null;

        var unit = match.Groups[2].Value.ToLowerInvariant();

        return unit switch
        {
            "d" or "day" or "days" => number,
            "w" or "week" or "weeks" => number * 7,
            "m" or "month" or "months" => number * 30,
            "y" or "year" or "years" => number * 365,
            _ => null
        };
    }

    /// <summary>
    /// Parses size expressions (e.g., "10MB", "500KB", "1.5GB")
    /// </summary>
    private long? ParseSize(string sizeString)
    {
        if (string.IsNullOrEmpty(sizeString))
            return null;

        var match = SizePattern.Match(sizeString);
        if (!match.Success)
            return null;

        if (!decimal.TryParse(match.Groups[1].Value, out var size))
            return null;

        var unit = match.Groups[2].Value.ToUpperInvariant();

        var multiplier = unit switch
        {
            "B" or "" => 1m,
            "KB" => 1024m,
            "MB" => 1024m * 1024m,
            "GB" => 1024m * 1024m * 1024m,
            _ => 1m
        };

        return (long)(size * multiplier);
    }

    /// <summary>
    /// Validates and normalizes the parsed search criteria
    /// </summary>
    public EmailSearchCriteria Normalize(EmailSearchCriteria criteria)
    {
        // Ensure reasonable limits
        criteria.MaxResults = Math.Max(1, Math.Min(criteria.MaxResults, 1000));
        criteria.Offset = Math.Max(0, criteria.Offset);

        // Handle conflicting read/unread flags
        if (criteria.IsRead == true && criteria.IsUnread == true)
        {
            criteria.IsRead = null;
            criteria.IsUnread = null;
        }

        // Handle date range validation
        if (criteria.After.HasValue && criteria.Before.HasValue && criteria.After > criteria.Before)
        {
            (criteria.After, criteria.Before) = (criteria.Before, criteria.After);
        }

        // Handle size range validation
        if (criteria.LargerThanBytes.HasValue && criteria.SmallerThanBytes.HasValue &&
            criteria.LargerThanBytes > criteria.SmallerThanBytes)
        {
            (criteria.LargerThanBytes, criteria.SmallerThanBytes) = (criteria.SmallerThanBytes, criteria.LargerThanBytes);
        }

        return criteria;
    }
}
