using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Seu.Mail.Core.Models;
using Seu.Mail.Search.Models;
using Seu.Mail.Search.Services;
using Xunit;

namespace Seu.Mail.Search.Tests;

/// <summary>
/// Tests for the EmailSearchService class
/// </summary>
public class EmailSearchServiceTests
{
    private readonly Mock<IEmailRepository> _mockRepository;
    private readonly EmailSearchQueryParser _parser;
    private readonly Mock<ILogger<EmailSearchService>> _mockLogger;
    private readonly EmailSearchOptions _options;
    private readonly EmailSearchService _searchService;

    public EmailSearchServiceTests()
    {
        _mockRepository = new Mock<IEmailRepository>();
        _parser = new EmailSearchQueryParser();
        _mockLogger = new Mock<ILogger<EmailSearchService>>();
        _options = new EmailSearchOptions();
        _searchService = new EmailSearchService(_mockRepository.Object, _parser, _mockLogger.Object);
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmptyResult()
    {
        // Arrange
        var query = "";

        // Act
        var result = await _searchService.SearchAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Emails.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_NullQuery_ReturnsEmptyResult()
    {
        // Arrange
        string? query = null;

        // Act
        var result = await _searchService.SearchAsync(query!);

        // Assert
        result.Should().NotBeNull();
        result.Emails.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_ValidQuery_ReturnsMatchingEmails()
    {
        // Arrange
        var query = "from:alice@example.com";
        var criteria = new EmailSearchCriteria { From = "alice@example.com" };
        var emails = CreateSampleEmails();

        // Parser will be called internally - we can't mock it since it's a concrete class
        _mockRepository.Setup(r => r.GetEmailsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emails);

        // Act
        var result = await _searchService.SearchAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.SearchCriteria.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetEmailsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithCriteria_FiltersEmailsCorrectly()
    {
        // Arrange
        var criteria = new EmailSearchCriteria
        {
            From = "alice@example.com",
            IsRead = false,
            HasAttachment = true
        };

        var emails = new List<EmailMessage>
        {
            new EmailMessage
            {
                Id = 1,
                From = "alice@example.com",
                Subject = "Test",
                IsRead = false,
                HasAttachments = true,
                DateReceived = DateTime.UtcNow.AddDays(-1),
                AccountId = 1
            },
            new EmailMessage
            {
                Id = 2,
                From = "bob@example.com",
                Subject = "Test 2",
                IsRead = true,
                HasAttachments = false,
                DateReceived = DateTime.UtcNow.AddDays(-2),
                AccountId = 1
            },
            new EmailMessage
            {
                Id = 3,
                From = "alice@example.com",
                Subject = "Test 3",
                IsRead = true,
                HasAttachments = true,
                DateReceived = DateTime.UtcNow.AddDays(-3),
                AccountId = 1
            }
        };

        _mockRepository.Setup(r => r.GetEmailsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emails);

        // Act
        var result = await _searchService.SearchAsync(criteria);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Emails.Should().HaveCount(1);
        result.Emails[0].Id.Should().Be(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_KeywordSearch_MatchesSubjectAndBody()
    {
        // Arrange
        var criteria = new EmailSearchCriteria { Keyword = "meeting" };

        var emails = new List<EmailMessage>
        {
            new EmailMessage
            {
                Id = 1,
                Subject = "Team meeting tomorrow",
                TextBody = "Don't forget about our meeting",
                DateReceived = DateTime.UtcNow.AddDays(-1),
                AccountId = 1
            },
            new EmailMessage
            {
                Id = 2,
                Subject = "Project update",
                TextBody = "The project is going well",
                DateReceived = DateTime.UtcNow.AddDays(-2),
                AccountId = 1
            },
            new EmailMessage
            {
                Id = 3,
                Subject = "Lunch plans",
                TextBody = "Let's schedule a meeting next week",
                DateReceived = DateTime.UtcNow.AddDays(-3),
                AccountId = 1
            }
        };

        _mockRepository.Setup(r => r.GetEmailsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emails);

        // Act
        var result = await _searchService.SearchAsync(criteria);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Emails.Should().HaveCount(2);
        result.Emails.Should().Contain(e => e.Id == 1);
        result.Emails.Should().Contain(e => e.Id == 3);
    }

    [Fact]
    public async Task SearchAsync_DateRangeFilter_FiltersCorrectly()
    {
        // Arrange
        var baseDate = new DateTime(2024, 1, 1);
        var criteria = new EmailSearchCriteria
        {
            After = baseDate.AddDays(2),
            Before = baseDate.AddDays(8)
        };

        var emails = new List<EmailMessage>
        {
            new EmailMessage { Id = 1, DateReceived = baseDate.AddDays(1), AccountId = 1 }, // Too early
            new EmailMessage { Id = 2, DateReceived = baseDate.AddDays(5), AccountId = 1 }, // In range
            new EmailMessage { Id = 3, DateReceived = baseDate.AddDays(10), AccountId = 1 }, // Too late
        };

        _mockRepository.Setup(r => r.GetEmailsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emails);

        // Act
        var result = await _searchService.SearchAsync(criteria);

        // Assert
        result.Should().NotBeNull();
        result.Emails.Should().HaveCount(1);
        result.Emails[0].Id.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_SizeFilter_FiltersCorrectly()
    {
        // Arrange
        var criteria = new EmailSearchCriteria
        {
            LargerThanBytes = 1000,
            SmallerThanBytes = 5000
        };

        var emails = new List<EmailMessage>
        {
            new EmailMessage { Id = 1, Size = 500, AccountId = 1 },   // Too small
            new EmailMessage { Id = 2, Size = 3000, AccountId = 1 },  // In range
            new EmailMessage { Id = 3, Size = 10000, AccountId = 1 }, // Too large
        };

        _mockRepository.Setup(r => r.GetEmailsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emails);

        // Act
        var result = await _searchService.SearchAsync(criteria);

        // Assert
        result.Should().NotBeNull();
        result.Emails.Should().HaveCount(1);
        result.Emails[0].Id.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_FolderFilter_FiltersCorrectly()
    {
        // Arrange
        var criteria = new EmailSearchCriteria { FolderName = "INBOX" };

        var emails = new List<EmailMessage>
        {
            new EmailMessage { Id = 1, Folder = "INBOX", AccountId = 1 },
            new EmailMessage { Id = 2, Folder = "SENT", AccountId = 1 },
            new EmailMessage { Id = 3, Folder = "inbox", AccountId = 1 }, // Case insensitive
        };

        _mockRepository.Setup(r => r.GetEmailsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emails);

        // Act
        var result = await _searchService.SearchAsync(criteria);

        // Assert
        result.Should().NotBeNull();
        result.Emails.Should().HaveCount(2);
        result.Emails.Should().Contain(e => e.Id == 1);
        result.Emails.Should().Contain(e => e.Id == 3);
    }

    [Fact]
    public async Task SearchAsync_AttachmentFilter_FiltersCorrectly()
    {
        // Arrange
        var criteria = new EmailSearchCriteria
        {
            AttachmentName = "report",
            AttachmentType = "pdf"
        };

        var emails = new List<EmailMessage>
        {
            new EmailMessage
            {
                Id = 1,
                AccountId = 1,
                Attachments = new List<EmailAttachment>
                {
                    new EmailAttachment { FileName = "monthly_report.pdf" }
                }
            },
            new EmailMessage
            {
                Id = 2,
                AccountId = 1,
                Attachments = new List<EmailAttachment>
                {
                    new EmailAttachment { FileName = "image.jpg" }
                }
            },
            new EmailMessage
            {
                Id = 3,
                AccountId = 1,
                Attachments = new List<EmailAttachment>()
            }
        };

        _mockRepository.Setup(r => r.GetEmailsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emails);

        // Act
        var result = await _searchService.SearchAsync(criteria);

        // Assert
        result.Should().NotBeNull();
        result.Emails.Should().HaveCount(1);
        result.Emails[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_ExcludesDeletedEmails_ByDefault()
    {
        // Arrange
        var criteria = new EmailSearchCriteria { Keyword = "test" };

        var emails = new List<EmailMessage>
        {
            new EmailMessage { Id = 1, Subject = "test", IsDeleted = false, AccountId = 1 },
            new EmailMessage { Id = 2, Subject = "test", IsDeleted = true, AccountId = 1 },
        };

        _mockRepository.Setup(r => r.GetEmailsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emails);

        // Act
        var result = await _searchService.SearchAsync(criteria);

        // Assert
        result.Should().NotBeNull();
        result.Emails.Should().HaveCount(1);
        result.Emails[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_IncludesDeletedEmails_WhenRequested()
    {
        // Arrange
        var criteria = new EmailSearchCriteria
        {
            Keyword = "test",
            IncludeDeleted = true
        };

        var emails = new List<EmailMessage>
        {
            new EmailMessage { Id = 1, Subject = "test", IsDeleted = false, AccountId = 1 },
            new EmailMessage { Id = 2, Subject = "test", IsDeleted = true, AccountId = 1 },
        };

        _mockRepository.Setup(r => r.GetEmailsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emails);

        // Act
        var result = await _searchService.SearchAsync(criteria);

        // Assert
        result.Should().NotBeNull();
        result.Emails.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_SortsByRelevance_WhenRequested()
    {
        // Arrange
        var criteria = new EmailSearchCriteria
        {
            Keyword = "meeting",
            SortOrder = SearchSortOrder.Relevance
        };

        var emails = new List<EmailMessage>
        {
            new EmailMessage
            {
                Id = 1,
                Subject = "Project update",
                TextBody = "meeting mentioned here",
                DateReceived = DateTime.UtcNow,
                AccountId = 1
            },
            new EmailMessage
            {
                Id = 2,
                Subject = "Meeting agenda", // Better match in subject
                TextBody = "Details about the meeting",
                DateReceived = DateTime.UtcNow.AddDays(-1),
                AccountId = 1
            }
        };

        _mockRepository.Setup(r => r.GetEmailsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emails);

        // Act
        var result = await _searchService.SearchAsync(criteria);

        // Assert
        result.Should().NotBeNull();
        result.Emails.Should().HaveCount(2);
        result.Emails[0].Id.Should().Be(2); // Should be first due to higher relevance
    }

    [Fact]
    public async Task SearchAsync_AppliesPagination_Correctly()
    {
        // Arrange
        var criteria = new EmailSearchCriteria
        {
            MaxResults = 2,
            Offset = 1
        };

        var emails = CreateSampleEmails(5);
        _mockRepository.Setup(r => r.GetEmailsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emails);

        // Act
        var result = await _searchService.SearchAsync(criteria);

        // Assert
        result.Should().NotBeNull();
        result.Emails.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.HasMore.Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_HandlesRepositoryException_Gracefully()
    {
        // Arrange
        var criteria = new EmailSearchCriteria { Keyword = "test" };
        _mockRepository.Setup(r => r.GetEmailsAsync(It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _searchService.SearchAsync(criteria);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        result.Emails.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSearchSuggestionsAsync_ReturnsOperatorSuggestions()
    {
        // Arrange
        var partialQuery = "fr";
        _mockRepository.Setup(r => r.GetRecentEmailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<EmailMessage>());

        // Act
        var suggestions = await _searchService.GetSearchSuggestionsAsync(partialQuery);

        // Assert
        suggestions.Should().NotBeNull();
        suggestions.Should().Contain("from:");
    }

    [Fact]
    public async Task GetSearchSuggestionsAsync_ReturnsSenderSuggestions()
    {
        // Arrange
        var partialQuery = "alice";
        var emails = new List<EmailMessage>
        {
            new EmailMessage { From = "alice@example.com", AccountId = 1 },
            new EmailMessage { From = "bob@example.com", AccountId = 1 }
        };

        _mockRepository.Setup(r => r.GetRecentEmailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(emails);

        // Act
        var suggestions = await _searchService.GetSearchSuggestionsAsync(partialQuery);

        // Assert
        suggestions.Should().NotBeNull();
        suggestions.Should().Contain("from:alice@example.com");
    }

    private List<EmailMessage> CreateSampleEmails(int count = 3)
    {
        var emails = new List<EmailMessage>();
        for (int i = 1; i <= count; i++)
        {
            emails.Add(new EmailMessage
            {
                Id = i,
                Subject = $"Email {i}",
                From = $"sender{i}@example.com",
                To = "recipient@example.com",
                DateReceived = DateTime.UtcNow.AddDays(-i),
                IsRead = i % 2 == 0,
                AccountId = 1,
                Folder = "INBOX"
            });
        }
        return emails;
    }
}
