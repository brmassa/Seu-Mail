using System;
using System.Collections.Generic;

namespace Seu.Mail.Search.Models;

/// <summary>
/// Enhanced search criteria for Gmail-like email search functionality
/// </summary>
public class EmailSearchCriteria
{
    /// <summary>
    /// Raw search query string (e.g., "from:alice has:attachment larger:1MB")
    /// </summary>
    public string? RawQuery { get; set; }

    /// <summary>
    /// General keyword search across subject, body, sender, recipient
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// Exact phrase search (words in quotes)
    /// </summary>
    public string? ExactPhrase { get; set; }

    /// <summary>
    /// Sender email address or name
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Recipient email address (To field)
    /// </summary>
    public string? To { get; set; }

    /// <summary>
    /// CC recipient email address
    /// </summary>
    public string? Cc { get; set; }

    /// <summary>
    /// BCC recipient email address
    /// </summary>
    public string? Bcc { get; set; }

    /// <summary>
    /// Subject line contains
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Search within email body content
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Filter emails with attachments
    /// </summary>
    public bool? HasAttachment { get; set; }

    /// <summary>
    /// Attachment filename contains
    /// </summary>
    public string? AttachmentName { get; set; }

    /// <summary>
    /// Attachment type/extension (e.g., "pdf", "jpg")
    /// </summary>
    public string? AttachmentType { get; set; }

    /// <summary>
    /// Date range - after this date
    /// </summary>
    public DateTime? After { get; set; }

    /// <summary>
    /// Date range - before this date
    /// </summary>
    public DateTime? Before { get; set; }

    /// <summary>
    /// Specific date (year, month, or full date)
    /// </summary>
    public DateTime? On { get; set; }

    /// <summary>
    /// Older than X days
    /// </summary>
    public int? OlderThanDays { get; set; }

    /// <summary>
    /// Newer than X days
    /// </summary>
    public int? NewerThanDays { get; set; }

    /// <summary>
    /// Email size larger than (in bytes)
    /// </summary>
    public long? LargerThanBytes { get; set; }

    /// <summary>
    /// Email size smaller than (in bytes)
    /// </summary>
    public long? SmallerThanBytes { get; set; }

    /// <summary>
    /// Read status filter
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    /// Unread emails only
    /// </summary>
    public bool? IsUnread { get; set; }

    /// <summary>
    /// Starred/Important emails
    /// </summary>
    public bool? IsStarred { get; set; }

    /// <summary>
    /// Important emails
    /// </summary>
    public bool? IsImportant { get; set; }

    /// <summary>
    /// Draft emails
    /// </summary>
    public bool? IsDraft { get; set; }

    /// <summary>
    /// Sent emails
    /// </summary>
    public bool? IsSent { get; set; }

    /// <summary>
    /// Spam/junk emails
    /// </summary>
    public bool? IsSpam { get; set; }

    /// <summary>
    /// Deleted/trash emails
    /// </summary>
    public bool? IsDeleted { get; set; }

    /// <summary>
    /// Specific folder name
    /// </summary>
    public string? FolderName { get; set; }

    /// <summary>
    /// Email priority level (1=High, 3=Normal, 5=Low)
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// Search within specific account ID
    /// </summary>
    public int? AccountId { get; set; }

    /// <summary>
    /// Search within specific account IDs
    /// </summary>
    public List<int>? AccountIds { get; set; }

    /// <summary>
    /// Custom labels/tags
    /// </summary>
    public List<string>? Labels { get; set; }

    /// <summary>
    /// Email thread ID
    /// </summary>
    public string? ThreadId { get; set; }

    /// <summary>
    /// Search scope (current folder, all folders, etc.)
    /// </summary>
    public SearchScope Scope { get; set; } = SearchScope.AllFolders;

    /// <summary>
    /// Maximum results to return
    /// </summary>
    public int MaxResults { get; set; } = 50;

    /// <summary>
    /// Pagination offset
    /// </summary>
    public int Offset { get; set; } = 0;

    /// <summary>
    /// Sort order
    /// </summary>
    public SearchSortOrder SortOrder { get; set; } = SearchSortOrder.DateDescending;

    /// <summary>
    /// Include deleted emails in search
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// Include spam emails in search
    /// </summary>
    public bool IncludeSpam { get; set; } = false;

    /// <summary>
    /// Search operators for combining criteria
    /// </summary>
    public List<SearchOperator>? Operators { get; set; }

    /// <summary>
    /// Negated criteria (NOT conditions)
    /// </summary>
    public EmailSearchCriteria? NotCriteria { get; set; }
}

/// <summary>
/// Search scope options
/// </summary>
public enum SearchScope
{
    CurrentFolder,
    AllFolders,
    Inbox,
    Sent,
    Drafts,
    Trash,
    Spam,
    Starred,
    Important
}

/// <summary>
/// Sort order options
/// </summary>
public enum SearchSortOrder
{
    DateAscending,
    DateDescending,
    SenderAscending,
    SenderDescending,
    SubjectAscending,
    SubjectDescending,
    SizeAscending,
    SizeDescending,
    Relevance
}

/// <summary>
/// Search operators for complex queries
/// </summary>
public enum SearchOperator
{
    And,
    Or,
    Not
}
