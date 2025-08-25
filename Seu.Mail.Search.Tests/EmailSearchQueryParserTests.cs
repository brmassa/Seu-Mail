using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Seu.Mail.Search.Models;
using Seu.Mail.Search.Services;
using Xunit;

namespace Seu.Mail.Search.Tests;

/// <summary>
/// Tests for the EmailSearchQueryParser class
/// </summary>
public class EmailSearchQueryParserTests
{
    private readonly EmailSearchQueryParser _parser;
    private readonly Mock<ILogger<EmailSearchQueryParser>> _mockLogger;

    public EmailSearchQueryParserTests()
    {
        _mockLogger = new Mock<ILogger<EmailSearchQueryParser>>();
        _parser = new EmailSearchQueryParser(_mockLogger.Object);
    }

    [Fact]
    public void Parse_EmptyQuery_ReturnsEmptyCriteria()
    {
        // Arrange
        var query = "";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.RawQuery.Should().BeNull();
        result.Keyword.Should().BeNull();
    }

    [Fact]
    public void Parse_SimpleKeyword_SetsKeywordProperty()
    {
        // Arrange
        var query = "hello world";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.Keyword.Should().Be("hello world");
        result.RawQuery.Should().Be("hello world");
    }

    [Fact]
    public void Parse_QuotedPhrase_SetsExactPhraseProperty()
    {
        // Arrange
        var query = "\"project update meeting\"";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.ExactPhrase.Should().Be("project update meeting");
        result.RawQuery.Should().Be("\"project update meeting\"");
    }

    [Fact]
    public void Parse_FromOperator_SetsFromProperty()
    {
        // Arrange
        var query = "from:alice@example.com";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.From.Should().Be("alice@example.com");
    }

    [Fact]
    public void Parse_ToOperator_SetsToProperty()
    {
        // Arrange
        var query = "to:bob@example.com";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.To.Should().Be("bob@example.com");
    }

    [Fact]
    public void Parse_SubjectOperator_SetsSubjectProperty()
    {
        // Arrange
        var query = "subject:\"Important Meeting\"";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.Subject.Should().Be("Important Meeting");
    }

    [Fact]
    public void Parse_HasAttachmentOperator_SetsHasAttachmentProperty()
    {
        // Arrange
        var query = "has:attachment";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.HasAttachment.Should().BeTrue();
    }

    [Theory]
    [InlineData("is:read", true, null)]
    [InlineData("is:unread", null, true)]
    [InlineData("is:starred", null, null, true)]
    [InlineData("is:important", null, null, null, true)]
    public void Parse_StatusOperators_SetsStatusProperties(string query, bool? isRead, bool? isUnread, bool? isStarred = null, bool? isImportant = null)
    {
        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.IsRead.Should().Be(isRead);
        result.IsUnread.Should().Be(isUnread);
        if (isStarred.HasValue)
            result.IsStarred.Should().Be(isStarred.Value);
        if (isImportant.HasValue)
            result.IsImportant.Should().Be(isImportant.Value);
    }

    [Theory]
    [InlineData("before:2024-01-15", "2024-01-15")]
    [InlineData("after:2024/01/15", "2024-01-15")]
    [InlineData("on:2024-01-15", null, "2024-01-15")]
    public void Parse_DateOperators_SetsDateProperties(string query, string? beforeDate = null, string? onDate = null)
    {
        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        if (beforeDate != null)
        {
            result.Before.Should().NotBeNull();
            result.Before!.Value.Date.Should().Be(DateTime.Parse(beforeDate).Date);
        }
        if (onDate != null)
        {
            result.On.Should().NotBeNull();
            result.On!.Value.Date.Should().Be(DateTime.Parse(onDate).Date);
        }
    }

    [Theory]
    [InlineData("larger:1MB", 1024 * 1024)]
    [InlineData("larger:500KB", 500 * 1024)]
    [InlineData("smaller:2GB", null, 2L * 1024 * 1024 * 1024)]
    [InlineData("larger:1024", 1024)]
    public void Parse_SizeOperators_SetsSizeProperties(string query, long? largerThan = null, long? smallerThan = null)
    {
        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        if (largerThan.HasValue)
            result.LargerThanBytes.Should().Be(largerThan.Value);
        if (smallerThan.HasValue)
            result.SmallerThanBytes.Should().Be(smallerThan.Value);
    }

    [Theory]
    [InlineData("older:7d", 7)]
    [InlineData("newer:1w", null, 7)]
    [InlineData("older:2weeks", 14)]
    [InlineData("newer:1month", null, 30)]
    public void Parse_RelativeDateOperators_SetsRelativeDateProperties(string query, int? olderThanDays = null, int? newerThanDays = null)
    {
        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        if (olderThanDays.HasValue)
            result.OlderThanDays.Should().Be(olderThanDays.Value);
        if (newerThanDays.HasValue)
            result.NewerThanDays.Should().Be(newerThanDays.Value);
    }

    [Theory]
    [InlineData("in:inbox", SearchScope.Inbox, "INBOX")]
    [InlineData("in:sent", SearchScope.Sent, "SENT")]
    [InlineData("in:drafts", SearchScope.Drafts, "DRAFTS")]
    [InlineData("in:trash", SearchScope.Trash, "TRASH")]
    [InlineData("in:spam", SearchScope.Spam, "SPAM")]
    [InlineData("in:starred", SearchScope.Starred, null)]
    [InlineData("in:anywhere", SearchScope.AllFolders, null)]
    public void Parse_InOperator_SetsScopeAndFolder(string query, SearchScope expectedScope, string? expectedFolder)
    {
        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.Scope.Should().Be(expectedScope);
        if (expectedFolder != null)
            result.FolderName.Should().Be(expectedFolder);
    }

    [Fact]
    public void Parse_FilenameOperator_SetsAttachmentNameProperty()
    {
        // Arrange
        var query = "filename:report.pdf";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.AttachmentName.Should().Be("report.pdf");
    }

    [Fact]
    public void Parse_FiletypeOperator_SetsAttachmentTypeProperty()
    {
        // Arrange
        var query = "filetype:pdf";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.AttachmentType.Should().Be("pdf");
    }

    [Fact]
    public void Parse_ComplexQuery_SetsMultipleProperties()
    {
        // Arrange
        var query = "from:alice@example.com has:attachment larger:1MB before:2024-01-15 \"project update\"";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.From.Should().Be("alice@example.com");
        result.HasAttachment.Should().BeTrue();
        result.LargerThanBytes.Should().Be(1024 * 1024);
        result.Before.Should().NotBeNull();
        result.Before!.Value.Date.Should().Be(new DateTime(2024, 1, 15).Date);
        result.ExactPhrase.Should().Be("project update");
    }

    [Fact]
    public void Parse_MixedKeywordsAndOperators_HandlesCorrectly()
    {
        // Arrange
        var query = "hello from:alice@example.com world is:unread";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.From.Should().Be("alice@example.com");
        result.IsUnread.Should().BeTrue();
        result.Keyword.Should().Be("hello world");
    }

    [Fact]
    public void Parse_SizeWithComparison_HandlesCorrectly()
    {
        // Arrange
        var query = "size:>10MB";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.LargerThanBytes.Should().Be(10 * 1024 * 1024);
    }

    [Fact]
    public void Parse_SizeWithSmallerComparison_HandlesCorrectly()
    {
        // Arrange
        var query = "size:<500KB";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.SmallerThanBytes.Should().Be(500 * 1024);
    }

    [Fact]
    public void Parse_CcAndBccOperators_SetsProperties()
    {
        // Arrange
        var query = "cc:manager@example.com bcc:hr@example.com";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.Cc.Should().Be("manager@example.com");
        result.Bcc.Should().Be("hr@example.com");
    }

    [Fact]
    public void Parse_BodyOperator_SetsBodyProperty()
    {
        // Arrange
        var query = "body:\"contract details\"";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.Body.Should().Be("contract details");
    }

    [Fact]
    public void Parse_ThreadOperator_SetsThreadIdProperty()
    {
        // Arrange
        var query = "thread:abc123";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.ThreadId.Should().Be("abc123");
    }

    [Fact]
    public void Parse_LabelOperator_AddsToLabels()
    {
        // Arrange
        var query = "label:important label:work";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.Labels.Should().NotBeNull();
        result.Labels.Should().Contain("important");
        result.Labels.Should().Contain("work");
        result.Labels.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_AccountOperator_SetsAccountIdProperty()
    {
        // Arrange
        var query = "account:5";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(5);
    }

    [Fact]
    public void Parse_PriorityOperator_SetsPriorityProperty()
    {
        // Arrange
        var query = "priority:1";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.Priority.Should().Be(1);
    }

    [Fact]
    public void Parse_QuotedValueInOperator_HandlesCorrectly()
    {
        // Arrange
        var query = "from:\"John Doe <john@example.com>\"";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.From.Should().Be("John Doe <john@example.com>");
    }

    [Fact]
    public void Parse_InvalidDateFormat_DoesNotSetDate()
    {
        // Arrange
        var query = "before:invalid-date";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.Before.Should().BeNull();
    }

    [Fact]
    public void Parse_InvalidSizeFormat_DoesNotSetSize()
    {
        // Arrange
        var query = "larger:invalid-size";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.LargerThanBytes.Should().BeNull();
    }

    [Fact]
    public void Normalize_LimitsMaxResults_SetsReasonableLimit()
    {
        // Arrange
        var criteria = new EmailSearchCriteria { MaxResults = 5000 };

        // Act
        var result = _parser.Normalize(criteria);

        // Assert
        result.MaxResults.Should().BeLessOrEqualTo(1000);
    }

    [Fact]
    public void Normalize_ConflictingReadFlags_ClearsBoth()
    {
        // Arrange
        var criteria = new EmailSearchCriteria { IsRead = true, IsUnread = true };

        // Act
        var result = _parser.Normalize(criteria);

        // Assert
        result.IsRead.Should().BeNull();
        result.IsUnread.Should().BeNull();
    }

    [Fact]
    public void Normalize_SwapsDateRange_WhenAfterIsLater()
    {
        // Arrange
        var laterDate = DateTime.UtcNow;
        var earlierDate = DateTime.UtcNow.AddDays(-10);
        var criteria = new EmailSearchCriteria { After = laterDate, Before = earlierDate };

        // Act
        var result = _parser.Normalize(criteria);

        // Assert
        result.After.Should().Be(earlierDate);
        result.Before.Should().Be(laterDate);
    }

    [Fact]
    public void Normalize_SwapsSizeRange_WhenLargerIsSmallerThanSmaller()
    {
        // Arrange
        var criteria = new EmailSearchCriteria { LargerThanBytes = 1000, SmallerThanBytes = 500 };

        // Act
        var result = _parser.Normalize(criteria);

        // Assert
        result.LargerThanBytes.Should().Be(500);
        result.SmallerThanBytes.Should().Be(1000);
    }

    [Fact]
    public void Parse_MultipleQuotedPhrases_HandlesFirst()
    {
        // Arrange
        var query = "\"first phrase\" \"second phrase\"";

        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        result.ExactPhrase.Should().Be("first phrase");
        result.Keyword.Should().Be("\"second phrase\"");
    }

    [Theory]
    [InlineData("is:draft", true)]
    [InlineData("is:sent", null, true)]
    public void Parse_DraftAndSentOperators_SetsProperties(string query, bool? isDraft = null, bool? isSent = null)
    {
        // Act
        var result = _parser.Parse(query);

        // Assert
        result.Should().NotBeNull();
        if (isDraft.HasValue)
            result.IsDraft.Should().Be(isDraft.Value);
        if (isSent.HasValue)
            result.IsSent.Should().Be(isSent.Value);
    }
}
