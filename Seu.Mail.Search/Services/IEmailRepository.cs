using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Seu.Mail.Core.Models;

namespace Seu.Mail.Search.Services;

/// <summary>
/// Repository interface for email search operations
/// </summary>
public interface IEmailRepository
{
    /// <summary>
    /// Gets a queryable collection of all emails for advanced filtering
    /// </summary>
    /// <returns>Queryable emails collection</returns>
    IQueryable<EmailMessage> GetEmailsQuery();

    /// <summary>
    /// Gets all emails from the repository
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All emails</returns>
    Task<IEnumerable<EmailMessage>> GetEmailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets emails for a specific account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Emails for the specified account</returns>
    Task<IEnumerable<EmailMessage>> GetEmailsByAccountAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets emails from a specific folder
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="folderName">Folder name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Emails from the specified folder</returns>
    Task<IEnumerable<EmailMessage>> GetEmailsByFolderAsync(int accountId, string folderName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent emails for search suggestions
    /// </summary>
    /// <param name="count">Number of recent emails to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recent emails</returns>
    Task<IEnumerable<EmailMessage>> GetRecentEmailsAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets emails by date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Emails in the specified date range</returns>
    Task<IEnumerable<EmailMessage>> GetEmailsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets distinct senders for autocomplete
    /// </summary>
    /// <param name="query">Partial sender query</param>
    /// <param name="limit">Maximum results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching senders</returns>
    Task<IEnumerable<string>> GetSendersAsync(string query, int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets distinct recipients for autocomplete
    /// </summary>
    /// <param name="query">Partial recipient query</param>
    /// <param name="limit">Maximum results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching recipients</returns>
    Task<IEnumerable<string>> GetRecipientsAsync(string query, int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets email count by folder for statistics
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of folder names and email counts</returns>
    Task<Dictionary<string, int>> GetEmailCountByFolderAsync(int accountId, CancellationToken cancellationToken = default);
}
