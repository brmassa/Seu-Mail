using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Seu.Mail.Core.Models;
using Seu.Mail.Data.Context;

namespace Seu.Mail.Search.Services;

/// <summary>
/// Email repository implementation using Entity Framework
/// </summary>
public class EmailRepository : IEmailRepository
{
    private readonly EmailDbContext _context;
    private readonly ILogger<EmailRepository> _logger;

    public EmailRepository(EmailDbContext context, ILogger<EmailRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IQueryable<EmailMessage> GetEmailsQuery()
    {
        return _context.EmailMessages
            .Include(e => e.Attachments)
            .Include(e => e.Account)
            .Where(e => !e.IsDeleted);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmailMessage>> GetEmailsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.EmailMessages
                .Include(e => e.Attachments)
                .Include(e => e.Account)
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.DateReceived)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all emails");
            return Enumerable.Empty<EmailMessage>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmailMessage>> GetEmailsByAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.EmailMessages
                .Include(e => e.Attachments)
                .Include(e => e.Account)
                .Where(e => e.AccountId == accountId && !e.IsDeleted)
                .OrderByDescending(e => e.DateReceived)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emails for account {AccountId}", accountId);
            return Enumerable.Empty<EmailMessage>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmailMessage>> GetEmailsByFolderAsync(int accountId, string folderName, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.EmailMessages
                .Include(e => e.Attachments)
                .Include(e => e.Account)
                .Where(e => e.AccountId == accountId &&
                           e.Folder.ToUpper() == folderName.ToUpper() &&
                           !e.IsDeleted)
                .OrderByDescending(e => e.DateReceived)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emails for account {AccountId} in folder {FolderName}", accountId, folderName);
            return Enumerable.Empty<EmailMessage>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmailMessage>> GetRecentEmailsAsync(int count, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.EmailMessages
                .Include(e => e.Attachments)
                .Include(e => e.Account)
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.DateReceived)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent emails");
            return Enumerable.Empty<EmailMessage>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmailMessage>> GetEmailsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.EmailMessages
                .Include(e => e.Attachments)
                .Include(e => e.Account)
                .Where(e => e.DateReceived >= startDate &&
                           e.DateReceived <= endDate &&
                           !e.IsDeleted)
                .OrderByDescending(e => e.DateReceived)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emails by date range {StartDate} to {EndDate}", startDate, endDate);
            return Enumerable.Empty<EmailMessage>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetSendersAsync(string query, int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<string>();

            var lowerQuery = query.ToLower();

            return await _context.EmailMessages
                .Where(e => !e.IsDeleted && e.From.ToLower().Contains(lowerQuery))
                .Select(e => e.From)
                .Distinct()
                .OrderBy(f => f)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving senders matching query {Query}", query);
            return Enumerable.Empty<string>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetRecipientsAsync(string query, int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<string>();

            var lowerQuery = query.ToLower();

            var toRecipients = await _context.EmailMessages
                .Where(e => !e.IsDeleted && e.To.ToLower().Contains(lowerQuery))
                .Select(e => e.To)
                .ToListAsync(cancellationToken);

            var ccRecipients = await _context.EmailMessages
                .Where(e => !e.IsDeleted && e.Cc != null && e.Cc.ToLower().Contains(lowerQuery))
                .Select(e => e.Cc)
                .ToListAsync(cancellationToken);

            // Parse and flatten recipient lists (they might be comma-separated)
            var allRecipients = new List<string>();

            foreach (var to in toRecipients)
            {
                if (!string.IsNullOrEmpty(to))
                    allRecipients.AddRange(to.Split(',').Select(r => r.Trim()));
            }

            foreach (var cc in ccRecipients)
            {
                if (!string.IsNullOrEmpty(cc))
                    allRecipients.AddRange(cc.Split(',').Select(r => r.Trim()));
            }

            return allRecipients
                .Where(r => !string.IsNullOrEmpty(r) && r.ToLower().Contains(lowerQuery))
                .Distinct()
                .OrderBy(r => r)
                .Take(limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recipients matching query {Query}", query);
            return Enumerable.Empty<string>();
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, int>> GetEmailCountByFolderAsync(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.EmailMessages
                .Where(e => e.AccountId == accountId && !e.IsDeleted)
                .GroupBy(e => e.Folder)
                .Select(g => new { Folder = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Folder, x => x.Count, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email counts by folder for account {AccountId}", accountId);
            return new Dictionary<string, int>();
        }
    }
}
