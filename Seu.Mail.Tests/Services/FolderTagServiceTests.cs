using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Seu.Mail.Data.Context;
using Seu.Mail.Core.Models;
using Seu.Mail.Services;

namespace Seu.Mail.Tests.Services;

public class FolderTagServiceTests : IAsyncDisposable
{
    private readonly EmailDbContext _context;
    private readonly ILogger<FolderTagService> _mockLogger;
    private readonly FolderTagService _folderTagService;

    public FolderTagServiceTests()
    {
        var options = new DbContextOptionsBuilder<EmailDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new EmailDbContext(options);
        _context.Database.EnsureCreated();
        _mockLogger = Substitute.For<ILogger<FolderTagService>>();
        _folderTagService = new FolderTagService(_context, _mockLogger);
    }

    #region GetFoldersAsync Tests

    [Test]
    public async Task GetFoldersAsync_WithValidAccountId_ShouldReturnFoldersOrderedCorrectly()
    {
        // Arrange
        var account = new EmailAccount { Id = 1, Email = "test@example.com" };
        var folders = new List<EmailFolder>
        {
            new() { Id = 1, AccountId = 1, Name = "INBOX", FolderType = "INBOX", IsSystemFolder = true },
            new() { Id = 2, AccountId = 1, Name = "Custom", FolderType = null, IsSystemFolder = false },
            new() { Id = 3, AccountId = 1, Name = "SENT", FolderType = "SENT", IsSystemFolder = true }
        };

        await _context.EmailAccounts.AddAsync(account);
        await _context.EmailFolders.AddRangeAsync(folders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _folderTagService.GetFoldersAsync(1);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Count).IsEqualTo(3);
        // System folders should come first, then ordered by name
        await Assert.That(result[0].IsSystemFolder).IsTrue();
        await Assert.That(result[1].IsSystemFolder).IsTrue();
        await Assert.That(result[2].IsSystemFolder).IsFalse();
    }

    [Test]
    public async Task GetFoldersAsync_WithInvalidAccountId_ShouldReturnEmptyList()
    {
        // Act
        var result = await _folderTagService.GetFoldersAsync(999);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsEmpty();
    }

    #endregion

    #region GetTagsAsync Tests

    [Test]
    public async Task GetTagsAsync_WithValidAccountId_ShouldReturnTags()
    {
        // Arrange
        var account = new EmailAccount { Id = 1, Email = "test@example.com" };
        var tags = new List<EmailTag>
        {
            new() { Id = 1, AccountId = 1, Name = "Important", Color = "#ff0000" },
            new() { Id = 2, AccountId = 1, Name = "Work", Color = "#00ff00" }
        };

        await _context.EmailAccounts.AddAsync(account);
        await _context.EmailTags.AddRangeAsync(tags);
        await _context.SaveChangesAsync();

        // Act
        var result = await _folderTagService.GetTagsAsync(1);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result.Any(t => t.Name == "Important")).IsTrue();
        await Assert.That(result.Any(t => t.Name == "Work")).IsTrue();
    }

    #endregion

    #region GetFolderByIdAsync Tests

    [Test]
    public async Task GetFolderByIdAsync_WithValidId_ShouldReturnFolder()
    {
        // Arrange
        var account = new EmailAccount
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        var folder = new EmailFolder
        {
            Id = 1,
            AccountId = 1,
            Name = "INBOX",
            DisplayName = "Inbox",
            FolderType = "INBOX",
            Account = account
        };

        await _context.EmailAccounts.AddAsync(account);
        await _context.EmailFolders.AddAsync(folder);
        await _context.SaveChangesAsync();

        // Act
        var result = await _folderTagService.GetFolderByIdAsync(1);

        // Assert
        _context.ChangeTracker.Clear();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Name).IsEqualTo("INBOX");
        await Assert.That(result.DisplayName).IsEqualTo("Inbox");
        await Assert.That(result.FolderType).IsEqualTo("INBOX");
    }

    [Test]
    public async Task GetFolderByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _folderTagService.GetFolderByIdAsync(999);

        // Assert
        await Assert.That(result).IsNull();
    }

    #endregion

    #region CreateFolderAsync Tests

    [Test]
    public async Task CreateFolderAsync_WithValidFolder_ShouldReturnTrueAndCreateFolder()
    {
        // Arrange
        var folder = new EmailFolder
        {
            AccountId = 1,
            Name = "Custom Folder",
            DisplayName = "My Custom Folder",
            FolderType = "CUSTOM"
        };

        // Act
        var result = await _folderTagService.CreateFolderAsync(folder);

        // Assert
        await Assert.That(result).IsTrue();

        var savedFolder = await _context.EmailFolders.FirstOrDefaultAsync();
        await Assert.That(savedFolder).IsNotNull();
        await Assert.That(savedFolder!.Name).IsEqualTo("Custom Folder");
        await Assert.That(savedFolder.DisplayName).IsEqualTo("My Custom Folder");
    }

    #endregion

    #region CreateTagAsync Tests

    [Test]
    public async Task CreateTagAsync_WithValidTag_ShouldReturnTrueAndCreateTag()
    {
        // Arrange
        var tag = new EmailTag
        {
            AccountId = 1,
            Name = "Important",
            Color = "#ff0000",
            Description = "Important emails"
        };

        // Act
        var result = await _folderTagService.CreateTagAsync(tag);

        // Assert
        await Assert.That(result).IsTrue();

        var savedTag = await _context.EmailTags.FirstOrDefaultAsync();
        await Assert.That(savedTag).IsNotNull();
        await Assert.That(savedTag!.Name).IsEqualTo("Important");
        await Assert.That(savedTag.Color).IsEqualTo("#ff0000");
        await Assert.That(savedTag.Description).IsEqualTo("Important emails");
    }

    #endregion

    #region UpdateFolderAsync Tests

    [Test]
    public async Task UpdateFolderAsync_WithValidFolder_ShouldReturnTrueAndUpdateFolder()
    {
        // Arrange
        var folder = new EmailFolder
        {
            Id = 1,
            AccountId = 1,
            Name = "Old Name",
            DisplayName = "Old Display"
        };

        await _context.EmailFolders.AddAsync(folder);
        await _context.SaveChangesAsync();

        folder.Name = "New Name";
        folder.DisplayName = "New Display";

        // Act
        var result = await _folderTagService.UpdateFolderAsync(folder);

        // Assert
        await Assert.That(result).IsTrue();

        var updatedFolder = await _context.EmailFolders.FindAsync(1);
        await Assert.That(updatedFolder).IsNotNull();
        await Assert.That(updatedFolder!.Name).IsEqualTo("New Name");
        await Assert.That(updatedFolder.DisplayName).IsEqualTo("New Display");
    }

    [Test]
    public async Task UpdateFolderAsync_WithNonExistentFolder_ShouldReturnFalse()
    {
        // Arrange
        var folder = new EmailFolder { Id = 999, AccountId = 1, Name = "Test" };

        // Act
        var result = await _folderTagService.UpdateFolderAsync(folder);

        // Assert
        await Assert.That(result).IsFalse();
    }

    #endregion

    #region DeleteFolderAsync Tests

    [Test]
    public async Task DeleteFolderAsync_WithValidId_ShouldReturnTrueAndDeleteFolder()
    {
        // Arrange
        var folder = new EmailFolder { Id = 1, AccountId = 1, Name = "Test" };
        await _context.EmailFolders.AddAsync(folder);
        await _context.SaveChangesAsync();

        // Act
        var result = await _folderTagService.DeleteFolderAsync(1);

        // Assert
        await Assert.That(result).IsTrue();

        var deletedFolder = await _context.EmailFolders.FindAsync(1);
        await Assert.That(deletedFolder).IsNull();
    }

    [Test]
    public async Task DeleteFolderAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _folderTagService.DeleteFolderAsync(999);

        // Assert
        await Assert.That(result).IsFalse();
    }

    #endregion

    #region AddTagToEmailAsync Tests

    [Test]
    public async Task AddTagToEmailAsync_WithValidIds_ShouldReturnTrueAndCreateAssociation()
    {
        // Arrange
        var email = new EmailMessage { Id = 1, AccountId = 1, Subject = "Test", MessageId = "test@example.com" };
        var tag = new EmailTag { Id = 1, AccountId = 1, Name = "Important" };

        await _context.EmailMessages.AddAsync(email);
        await _context.EmailTags.AddAsync(tag);
        await _context.SaveChangesAsync();

        // Act
        var result = await _folderTagService.AddTagToEmailAsync(1, 1);

        // Assert
        await Assert.That(result).IsTrue();

        var association = await _context.EmailMessageTags
            .FirstOrDefaultAsync(emt => emt.EmailMessageId == 1 && emt.TagId == 1);
        await Assert.That(association).IsNotNull();
    }

    [Test]
    public async Task AddTagToEmailAsync_WhenAlreadyExists_ShouldReturnTrueWithoutDuplicate()
    {
        // Arrange
        var email = new EmailMessage { Id = 1, AccountId = 1, Subject = "Test", MessageId = "test@example.com" };
        var tag = new EmailTag { Id = 1, AccountId = 1, Name = "Important" };
        var existingAssociation = new EmailMessageTag { EmailMessageId = 1, TagId = 1 };

        await _context.EmailMessages.AddAsync(email);
        await _context.EmailTags.AddAsync(tag);
        await _context.EmailMessageTags.AddAsync(existingAssociation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _folderTagService.AddTagToEmailAsync(1, 1);

        // Assert
        await Assert.That(result).IsTrue();

        var associationCount = await _context.EmailMessageTags
            .CountAsync(emt => emt.EmailMessageId == 1 && emt.TagId == 1);
        await Assert.That(associationCount).IsEqualTo(1); // Should still be only one
    }

    #endregion

    #region RemoveTagFromEmailAsync Tests

    [Test]
    public async Task RemoveTagFromEmailAsync_WithValidIds_ShouldReturnTrueAndRemoveAssociation()
    {
        // Arrange
        var association = new EmailMessageTag { Id = 1, EmailMessageId = 1, TagId = 1 };
        await _context.EmailMessageTags.AddAsync(association);
        await _context.SaveChangesAsync();

        // Act
        var result = await _folderTagService.RemoveTagFromEmailAsync(1, 1);

        // Assert
        await Assert.That(result).IsTrue();

        var deletedAssociation = await _context.EmailMessageTags.FindAsync(1);
        await Assert.That(deletedAssociation).IsNull();
    }

    [Test]
    public async Task RemoveTagFromEmailAsync_WithNonExistentAssociation_ShouldReturnTrue()
    {
        // Act
        var result = await _folderTagService.RemoveTagFromEmailAsync(999, 999);

        // Assert
        await Assert.That(result).IsTrue(); // Should not fail even if association doesn't exist
    }

    #endregion

    #region GetEmailTagsAsync Tests

    [Test]
    public async Task GetEmailTagsAsync_WithValidEmailId_ShouldReturnAssociatedTags()
    {
        // Arrange
        var email = new EmailMessage { Id = 1, AccountId = 1, Subject = "Test", MessageId = "test@example.com" };
        var tag1 = new EmailTag { Id = 1, AccountId = 1, Name = "Important" };
        var tag2 = new EmailTag { Id = 2, AccountId = 1, Name = "Work" };
        var association1 = new EmailMessageTag { EmailMessageId = 1, TagId = 1 };
        var association2 = new EmailMessageTag { EmailMessageId = 1, TagId = 2 };

        await _context.EmailMessages.AddAsync(email);
        await _context.EmailTags.AddRangeAsync(tag1, tag2);
        await _context.EmailMessageTags.AddRangeAsync(association1, association2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _folderTagService.GetEmailTagsAsync(1);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result.Any(t => t.Name == "Important")).IsTrue();
        await Assert.That(result.Any(t => t.Name == "Work")).IsTrue();
    }

    #endregion

    #region GetFolderMessageCountAsync Tests

    [Test]
    public async Task GetFolderMessageCountAsync_WithValidFolderId_ShouldReturnCorrectCount()
    {
        // Arrange
        var folder = new EmailFolder { Id = 1, AccountId = 1, Name = "INBOX" };
        var messages = new List<EmailMessage>
        {
            new() { Id = 1, AccountId = 1, Folder = "INBOX", Subject = "Test 1", MessageId = "test1@example.com" },
            new() { Id = 2, AccountId = 1, Folder = "INBOX", Subject = "Test 2", MessageId = "test2@example.com" },
            new() { Id = 3, AccountId = 1, Folder = "SENT", Subject = "Test 3", MessageId = "test3@example.com" }
        };

        await _context.EmailFolders.AddAsync(folder);
        await _context.EmailMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();

        // Act
        var result = await _folderTagService.GetFolderMessageCountAsync(1);

        // Assert
        await Assert.That(result).IsEqualTo(2); // Only INBOX messages should be counted
    }

    #endregion

    #region GetFolderUnreadCountAsync Tests

    [Test]
    public async Task GetFolderUnreadCountAsync_WithValidFolderId_ShouldReturnCorrectUnreadCount()
    {
        // Arrange
        var folder = new EmailFolder { Id = 1, AccountId = 1, Name = "INBOX" };
        var messages = new List<EmailMessage>
        {
            new() { Id = 1, AccountId = 1, Folder = "INBOX", Subject = "Test 1", IsRead = false, MessageId = "test1@example.com" },
            new() { Id = 2, AccountId = 1, Folder = "INBOX", Subject = "Test 2", IsRead = true, MessageId = "test2@example.com" },
            new() { Id = 3, AccountId = 1, Folder = "INBOX", Subject = "Test 3", IsRead = false, MessageId = "test3@example.com" }
        };

        await _context.EmailFolders.AddAsync(folder);
        await _context.EmailMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();

        // Act
        var result = await _folderTagService.GetFolderUnreadCountAsync(1);

        // Assert
        await Assert.That(result).IsEqualTo(2); // Only unread INBOX messages should be counted
    }

    #endregion

    #region Edge Cases and Error Handling

    [Test]
    public async Task CreateFolderAsync_WhenDatabaseThrows_ShouldReturnFalse()
    {
        // Arrange
        await _context.DisposeAsync();
        var folder = new EmailFolder { AccountId = 1, Name = "Test" };

        // Act
        var result = await _folderTagService.CreateFolderAsync(folder);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GetTagsAsync_WithMultipleAccounts_ShouldReturnOnlyRequestedAccountTags()
    {
        // Arrange
        var tags = new List<EmailTag>
        {
            new() { Id = 1, AccountId = 1, Name = "Account1 Tag" },
            new() { Id = 2, AccountId = 2, Name = "Account2 Tag" },
            new() { Id = 3, AccountId = 1, Name = "Another Account1 Tag" }
        };

        await _context.EmailTags.AddRangeAsync(tags);
        await _context.SaveChangesAsync();

        // Act
        var result = await _folderTagService.GetTagsAsync(1);

        // Assert
        await Assert.That(result.Count).IsEqualTo(2);
        foreach (var tag in result)
        {
            await Assert.That(tag.AccountId).IsEqualTo(1);
        }
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
