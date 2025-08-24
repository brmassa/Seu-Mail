using Seu.Mail.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Seu.Mail.Tests.Models;

/// <summary>
/// Tests for EmailMessage model including validation, property assignments,
/// and business logic validation
/// </summary>
public class EmailMessageTests
{
    // Basic Property Tests

    [Test]
    public async Task EmailMessage_DefaultConstruction_ShouldHaveExpectedDefaults()
    {
        // Act
        var message = new EmailMessage();

        // Assert
        await Assert.That(message.Id).IsEqualTo(0);
        await Assert.That(message.MessageId).IsEqualTo(string.Empty);
        await Assert.That(message.AccountId).IsEqualTo(0);
        await Assert.That(message.From).IsEqualTo(string.Empty);
        await Assert.That(message.To).IsEqualTo(string.Empty);
        await Assert.That(message.Subject).IsEqualTo(string.Empty);
        await Assert.That(message.Cc).IsNull();
        await Assert.That(message.Bcc).IsNull();
        await Assert.That(message.TextBody).IsNull();
        await Assert.That(message.HtmlBody).IsNull();
    }

    [Test]
    public async Task EmailMessage_PropertyAssignment_ShouldWorkCorrectly()
    {
        // Arrange
        var message = new EmailMessage();
        var testMessageId = "test-message-id-12345";
        var testAccountId = 42;
        var testFrom = "sender@example.com";
        var testTo = "recipient@example.com";
        var testSubject = "Test Email Subject";

        // Act
        message.Id = 1;
        message.MessageId = testMessageId;
        message.AccountId = testAccountId;
        message.From = testFrom;
        message.To = testTo;
        message.Subject = testSubject;

        // Assert
        await Assert.That(message.Id).IsEqualTo(1);
        await Assert.That(message.MessageId).IsEqualTo(testMessageId);
        await Assert.That(message.AccountId).IsEqualTo(testAccountId);
        await Assert.That(message.From).IsEqualTo(testFrom);
        await Assert.That(message.To).IsEqualTo(testTo);
        await Assert.That(message.Subject).IsEqualTo(testSubject);
    }

    // Validation Tests

    [Test]
    public async Task EmailMessage_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var message = new EmailMessage
        {
            MessageId = "valid-message-id",
            AccountId = 1,
            From = "sender@example.com",
            To = "recipient@example.com",
            Subject = "Valid Subject"
        };

        // Act
        var validationResults = ValidateModel(message);

        // Assert
        await Assert.That(validationResults).IsEmpty();
    }

    [Test]
    public async Task EmailMessage_WithEmptyMessageId_ShouldFailValidation()
    {
        // Arrange
        var message = new EmailMessage
        {
            MessageId = "",
            AccountId = 1,
            From = "sender@example.com",
            To = "recipient@example.com",
            Subject = "Valid Subject"
        };

        // Act
        var validationResults = ValidateModel(message);

        // Assert
        await Assert.That(validationResults).IsNotEmpty();
        await Assert.That(validationResults.Any(r => r.MemberNames.Contains("MessageId"))).IsTrue();
    }

    [Test]
    public async Task EmailMessage_WithEmptyFrom_ShouldFailValidation()
    {
        // Arrange
        var message = new EmailMessage
        {
            MessageId = "valid-message-id",
            AccountId = 1,
            From = "",
            To = "recipient@example.com",
            Subject = "Valid Subject"
        };

        // Act
        var validationResults = ValidateModel(message);

        // Assert
        await Assert.That(validationResults).IsNotEmpty();
        await Assert.That(validationResults.Any(r => r.MemberNames.Contains("From"))).IsTrue();
    }

    [Test]
    public async Task EmailMessage_WithEmptyTo_ShouldFailValidation()
    {
        // Arrange
        var message = new EmailMessage
        {
            MessageId = "valid-message-id",
            AccountId = 1,
            From = "sender@example.com",
            To = "",
            Subject = "Valid Subject"
        };

        // Act
        var validationResults = ValidateModel(message);

        // Assert
        await Assert.That(validationResults).IsNotEmpty();
        await Assert.That(validationResults.Any(r => r.MemberNames.Contains("To"))).IsTrue();
    }

    [Test]
    public async Task EmailMessage_WithEmptySubject_ShouldFailValidation()
    {
        // Arrange
        var message = new EmailMessage
        {
            MessageId = "valid-message-id",
            AccountId = 1,
            From = "sender@example.com",
            To = "recipient@example.com",
            Subject = ""
        };

        // Act
        var validationResults = ValidateModel(message);

        // Assert
        await Assert.That(validationResults).IsNotEmpty();
        await Assert.That(validationResults.Any(r => r.MemberNames.Contains("Subject"))).IsTrue();
    }

    // Optional Fields Tests

    [Test]
    public async Task EmailMessage_WithOptionalFields_ShouldWorkCorrectly()
    {
        // Arrange
        var message = new EmailMessage
        {
            MessageId = "valid-message-id",
            AccountId = 1,
            From = "sender@example.com",
            To = "recipient@example.com",
            Subject = "Valid Subject",
            Cc = "cc@example.com",
            Bcc = "bcc@example.com",
            TextBody = "This is the text body",
            HtmlBody = "<p>This is the HTML body</p>"
        };

        // Act
        var validationResults = ValidateModel(message);

        // Assert
        await Assert.That(validationResults).IsEmpty();
        await Assert.That(message.Cc).IsEqualTo("cc@example.com");
        await Assert.That(message.Bcc).IsEqualTo("bcc@example.com");
        await Assert.That(message.TextBody).IsEqualTo("This is the text body");
        await Assert.That(message.HtmlBody).IsEqualTo("<p>This is the HTML body</p>");
    }

    [Test]
    public async Task EmailMessage_WithNullOptionalFields_ShouldPassValidation()
    {
        // Arrange
        var message = new EmailMessage
        {
            MessageId = "valid-message-id",
            AccountId = 1,
            From = "sender@example.com",
            To = "recipient@example.com",
            Subject = "Valid Subject",
            Cc = null,
            Bcc = null,
            TextBody = null,
            HtmlBody = null
        };

        // Act
        var validationResults = ValidateModel(message);

        // Assert
        await Assert.That(validationResults).IsEmpty();
    }

    // Edge Cases

    [Test]
    public async Task EmailMessage_WithLongSubject_ShouldHandleCorrectly()
    {
        // Arrange
        var longSubject = new string('A', 1000);
        var message = new EmailMessage
        {
            MessageId = "valid-message-id",
            AccountId = 1,
            From = "sender@example.com",
            To = "recipient@example.com",
            Subject = longSubject
        };

        // Act
        var validationResults = ValidateModel(message);

        // Assert
        await Assert.That(message.Subject).IsEqualTo(longSubject);
        // Note: Basic validation should pass, but specific business rules might apply
    }

    [Test]
    public async Task EmailMessage_WithSpecialCharactersInEmail_ShouldWorkCorrectly()
    {
        // Arrange
        var message = new EmailMessage
        {
            MessageId = "message-with-special-chars",
            AccountId = 1,
            From = "test+tag@example.com",
            To = "recipient.name+tag@sub.domain.com",
            Subject = "Subject with émojis 🚀 and unicode ñ"
        };

        // Act
        var validationResults = ValidateModel(message);

        // Assert
        await Assert.That(validationResults).IsEmpty();
        await Assert.That(message.From).Contains("+");
        await Assert.That(message.To).Contains(".");
        await Assert.That(message.Subject).Contains("🚀");
    }

    [Test]
    public async Task EmailMessage_WithMultipleRecipients_ShouldHandleCorrectly()
    {
        // Arrange
        var message = new EmailMessage
        {
            MessageId = "multi-recipient-message",
            AccountId = 1,
            From = "sender@example.com",
            To = "recipient1@example.com, recipient2@example.com",
            Cc = "cc1@example.com, cc2@example.com",
            Bcc = "bcc1@example.com, bcc2@example.com",
            Subject = "Multi-recipient message"
        };

        // Act
        var validationResults = ValidateModel(message);

        // Assert
        await Assert.That(validationResults).IsEmpty();
        await Assert.That(message.To).Contains(",");
        await Assert.That(message.Cc).Contains(",");
        await Assert.That(message.Bcc).Contains(",");
    }

    // Content Body Tests

    [Test]
    public async Task EmailMessage_WithBothTextAndHtmlBody_ShouldHandleCorrectly()
    {
        // Arrange
        var textContent = "This is plain text content";
        var htmlContent = "<html><body><p>This is HTML content</p></body></html>";
        var message = new EmailMessage
        {
            MessageId = "dual-content-message",
            AccountId = 1,
            From = "sender@example.com",
            To = "recipient@example.com",
            Subject = "Dual content message",
            TextBody = textContent,
            HtmlBody = htmlContent
        };

        // Act
        var validationResults = ValidateModel(message);

        // Assert
        await Assert.That(validationResults).IsEmpty();
        await Assert.That(message.TextBody).IsEqualTo(textContent);
        await Assert.That(message.HtmlBody).IsEqualTo(htmlContent);
    }

    [Test]
    public async Task EmailMessage_WithOnlyTextBody_ShouldBeValid()
    {
        // Arrange
        var message = new EmailMessage
        {
            MessageId = "text-only-message",
            AccountId = 1,
            From = "sender@example.com",
            To = "recipient@example.com",
            Subject = "Text only message",
            TextBody = "This is plain text only",
            HtmlBody = null
        };

        // Act
        var validationResults = ValidateModel(message);

        // Assert
        await Assert.That(validationResults).IsEmpty();
        await Assert.That(message.TextBody).IsNotNull();
        await Assert.That(message.HtmlBody).IsNull();
    }

    [Test]
    public async Task EmailMessage_WithOnlyHtmlBody_ShouldBeValid()
    {
        // Arrange
        var message = new EmailMessage
        {
            MessageId = "html-only-message",
            AccountId = 1,
            From = "sender@example.com",
            To = "recipient@example.com",
            Subject = "HTML only message",
            TextBody = null,
            HtmlBody = "<p>This is HTML only</p>"
        };

        // Act
        var validationResults = ValidateModel(message);

        // Assert
        await Assert.That(validationResults).IsEmpty();
        await Assert.That(message.TextBody).IsNull();
        await Assert.That(message.HtmlBody).IsNotNull();
    }

    // Performance Tests

    [Test]
    public async Task EmailMessage_Creation_ShouldBeEfficient()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (var i = 0; i < 1000; i++)
        {
            var message = new EmailMessage
            {
                MessageId = $"message-{i}",
                AccountId = i,
                From = $"sender{i}@example.com",
                To = $"recipient{i}@example.com",
                Subject = $"Subject {i}"
            };
        }

        stopwatch.Stop();

        // Assert
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(100); // Should create 1000 objects quickly
    }

    // Equality Tests (if implemented)

    [Test]
    public async Task EmailMessage_WithSameData_ShouldHaveSameValues()
    {
        // Arrange
        var message1 = new EmailMessage
        {
            MessageId = "same-message-id",
            AccountId = 1,
            From = "sender@example.com",
            To = "recipient@example.com",
            Subject = "Same Subject"
        };

        var message2 = new EmailMessage
        {
            MessageId = "same-message-id",
            AccountId = 1,
            From = "sender@example.com",
            To = "recipient@example.com",
            Subject = "Same Subject"
        };

        // Act & Assert
        await Assert.That(message1.MessageId).IsEqualTo(message2.MessageId);
        await Assert.That(message1.AccountId).IsEqualTo(message2.AccountId);
        await Assert.That(message1.From).IsEqualTo(message2.From);
        await Assert.That(message1.To).IsEqualTo(message2.To);
        await Assert.That(message1.Subject).IsEqualTo(message2.Subject);
    }

    // Helper Methods

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}