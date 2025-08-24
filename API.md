# Seu Mail API Documentation 🔌

This document describes the internal API structure and architecture of Seu Mail, including services, models, and integration points.

## 📋 Table of Contents

- [Architecture Overview](#architecture-overview)
- [Core Services](#core-services)
- [Data Models](#data-models)
- [API Endpoints](#api-endpoints)
- [Service Interfaces](#service-interfaces)
- [Database Schema](#database-schema)
- [Integration Guide](#integration-guide)
- [Extension Points](#extension-points)

## 🏗️ Architecture Overview

Seu Mail follows a layered architecture pattern with clear separation of concerns:

```
┌─────────────────────────────────────────────┐
│                 Presentation Layer          │
│  (Blazor Components, Pages, Razor Views)   │
├─────────────────────────────────────────────┤
│                Business Logic Layer         │
│     (Services, Domain Logic, Validation)   │
├─────────────────────────────────────────────┤
│                Data Access Layer            │
│   (Entity Framework, Repositories, DTOs)   │
├─────────────────────────────────────────────┤
│              Infrastructure Layer           │
│    (Email Protocols, File System, Cache)   │
└─────────────────────────────────────────────┘
```

### Technology Stack

- **Framework**: ASP.NET Core 9.0, Blazor Server
- **Database**: SQLite with Entity Framework Core
- **Email**: MailKit/MimeKit libraries
- **Authentication**: Built-in ASP.NET Core Identity
- **UI**: Bootstrap 5, Font Awesome
- **Testing**: TUnit framework

## 🛠️ Core Services

### IEmailService

Primary service for email operations.

```csharp
public interface IEmailService
{
    Task<List<EmailMessage>> GetEmailsAsync(
        EmailAccount account, 
        string folderName, 
        int count = 50);
    
    Task<EmailMessage> GetEmailByIdAsync(int emailId);
    
    Task SendEmailAsync(
        EmailAccount account, 
        string to, 
        string subject, 
        string body, 
        List<EmailAttachment>? attachments = null);
    
    Task<bool> DeleteEmailAsync(int emailId);
    
    Task<bool> MarkAsReadAsync(int emailId);
    
    Task<bool> MarkAsUnreadAsync(int emailId);
    
    Task SyncEmailsAsync(EmailAccount account);
    
    Task<List<EmailMessage>> SearchEmailsAsync(
        string query, 
        EmailAccount? account = null);
}
```

#### Key Methods

**GetEmailsAsync**
- Retrieves emails from specified folder
- Supports pagination
- Returns sorted by date (newest first)

**SendEmailAsync**
- Sends email via SMTP
- Supports HTML and plain text
- Handles attachments up to 25MB

**SyncEmailsAsync**
- Synchronizes with IMAP server
- Two-way sync (download new, upload changes)
- Handles folder structure

### IAccountService

Manages email account configuration and authentication.

```csharp
public interface IAccountService
{
    Task<List<EmailAccount>> GetAccountsAsync();
    
    Task<EmailAccount?> GetAccountByIdAsync(int accountId);
    
    Task<EmailAccount?> GetDefaultAccountAsync();
    
    Task<bool> AddAccountAsync(EmailAccount account);
    
    Task<bool> UpdateAccountAsync(EmailAccount account);
    
    Task<bool> DeleteAccountAsync(int accountId);
    
    Task<bool> TestConnectionAsync(EmailAccount account);
    
    Task<bool> SetDefaultAccountAsync(int accountId);
}
```

### ICalendarService

Handles calendar events and scheduling.

```csharp
public interface ICalendarService
{
    Task<List<CalendarEvent>> GetEventsAsync(
        DateTime startDate, 
        DateTime endDate);
    
    Task<CalendarEvent?> GetEventByIdAsync(int eventId);
    
    Task<bool> CreateEventAsync(CalendarEvent calendarEvent);
    
    Task<bool> UpdateEventAsync(CalendarEvent calendarEvent);
    
    Task<bool> DeleteEventAsync(int eventId);
    
    Task<List<CalendarEvent>> GetRecurringEventsAsync(
        int eventId, 
        DateTime startDate, 
        DateTime endDate);
}
```

### IUserSettingsService

Manages user preferences and configuration.

```csharp
public interface IUserSettingsService
{
    Task<UserSettings> GetUserSettingsAsync();
    
    Task<bool> UpdateUserSettingsAsync(UserSettings settings);
    
    Task<T> GetSettingAsync<T>(string key, T defaultValue);
    
    Task<bool> SetSettingAsync<T>(string key, T value);
    
    Task<bool> ResetSettingsAsync();
}
```

## 📊 Data Models

### EmailMessage

Core email entity representing a single email message.

```csharp
public class EmailMessage
{
    public int Id { get; set; }
    
    public string MessageId { get; set; } = string.Empty;
    
    public string From { get; set; } = string.Empty;
    
    public string To { get; set; } = string.Empty;
    
    public string? Cc { get; set; }
    
    public string? Bcc { get; set; }
    
    public string Subject { get; set; } = string.Empty;
    
    public string? TextBody { get; set; }
    
    public string? HtmlBody { get; set; }
    
    public DateTime DateReceived { get; set; }
    
    public DateTime DateSent { get; set; }
    
    public bool IsRead { get; set; }
    
    public bool IsImportant { get; set; }
    
    public bool HasAttachments { get; set; }
    
    public string FolderName { get; set; } = "INBOX";
    
    public int AccountId { get; set; }
    
    public EmailAccount Account { get; set; } = null!;
    
    public List<EmailAttachment> Attachments { get; set; } = new();
}
```

### EmailAccount

Represents an email account configuration.

```csharp
public class EmailAccount
{
    public int Id { get; set; }
    
    public string DisplayName { get; set; } = string.Empty;
    
    public string EmailAddress { get; set; } = string.Empty;
    
    public string EncryptedPassword { get; set; } = string.Empty;
    
    public string ImapServer { get; set; } = string.Empty;
    
    public int ImapPort { get; set; } = 993;
    
    public bool ImapUseSsl { get; set; } = true;
    
    public string SmtpServer { get; set; } = string.Empty;
    
    public int SmtpPort { get; set; } = 587;
    
    public bool SmtpUseTls { get; set; } = true;
    
    public bool IsDefault { get; set; }
    
    public bool IsEnabled { get; set; } = true;
    
    public DateTime LastSyncTime { get; set; }
    
    public string? Signature { get; set; }
    
    public EmailProviderSettings? ProviderSettings { get; set; }
}
```

### CalendarEvent

Represents a calendar event or appointment.

```csharp
public class CalendarEvent
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string? Location { get; set; }
    
    public DateTime StartTime { get; set; }
    
    public DateTime EndTime { get; set; }
    
    public bool IsAllDay { get; set; }
    
    public string? TimeZone { get; set; }
    
    public EventRecurrenceType RecurrenceType { get; set; }
    
    public RecurrenceRule? RecurrenceRule { get; set; }
    
    public List<EventReminder> Reminders { get; set; } = new();
    
    public List<EventAttendee> Attendees { get; set; } = new();
    
    public string? CalendarId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}
```

### UserSettings

User preferences and configuration options.

```csharp
public class UserSettings
{
    public int Id { get; set; }
    
    public EmailDisplayMode EmailDisplayMode { get; set; } = EmailDisplayMode.TitleSenderPreview;
    
    public EmailLayoutMode EmailLayoutMode { get; set; } = EmailLayoutMode.SeparatePage;
    
    public bool ShowEmailPreview { get; set; } = true;
    
    public bool UseCompactMode { get; set; } = false;
    
    public bool MarkAsReadOnOpen { get; set; } = true;
    
    public int EmailsPerPage { get; set; } = 50;
    
    public int SyncIntervalMinutes { get; set; } = 15;
    
    public bool AutoSyncEnabled { get; set; } = true;
    
    public string Theme { get; set; } = "light";
    
    public string DefaultFontSize { get; set; } = "14px";
    
    public bool EnableNotifications { get; set; } = true;
    
    public bool EnableSounds { get; set; } = false;
    
    public CalendarSettings CalendarSettings { get; set; } = new();
}
```

## 🌐 API Endpoints

### Internal Blazor API

Seu Mail uses Blazor Server, so most interactions happen server-side. However, there are some internal endpoints:

#### Health Check
```
GET /health
Response: 200 OK
{
  "status": "healthy",
  "timestamp": "2024-12-XX T10:00:00Z"
}
```

#### Attachment Download
```
GET /api/attachments/{attachmentId}
Headers: 
  - Authorization: Required
Response: File stream with appropriate content-type
```

#### Email Export
```
GET /api/emails/{emailId}/export
Headers:
  - Authorization: Required
Response: .eml file download
```

### SignalR Hubs

Real-time notifications via SignalR:

#### EmailHub
```csharp
public class EmailHub : Hub
{
    public async Task JoinEmailGroup(string accountId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"emails_{accountId}");
    }
    
    public async Task NotifyNewEmail(string accountId, EmailMessage email)
    {
        await Clients.Group($"emails_{accountId}")
            .SendAsync("NewEmailReceived", email);
    }
}
```

## 🔧 Service Interfaces

### Validation Services

#### IValidationService
```csharp
public interface IValidationService
{
    Task<ValidationResult> ValidateEmailAddressAsync(string email);
    
    Task<ValidationResult> ValidateEmailAccountAsync(EmailAccount account);
    
    Task<ValidationResult> ValidateEmailMessageAsync(EmailMessage message);
    
    ValidationResult ValidateAttachment(IFormFile file);
}
```

### Security Services

#### IEncryptionService
```csharp
public interface IEncryptionService
{
    string EncryptPassword(string password);
    
    string DecryptPassword(string encryptedPassword);
    
    string GenerateSecureToken(int length = 32);
    
    bool VerifyPassword(string password, string hash);
}
```

### Discovery Services

#### IEmailProviderDetectionService
```csharp
public interface IEmailProviderDetectionService
{
    Task<EmailProviderSettings?> DetectProviderAsync(string emailAddress);
    
    Task<List<EmailProviderSettings>> GetKnownProvidersAsync();
    
    Task<bool> TestProviderSettingsAsync(EmailProviderSettings settings);
}
```

#### IDnsEmailDiscoveryService
```csharp
public interface IDnsEmailDiscoveryService
{
    Task<EmailServerSettings?> DiscoverImapSettingsAsync(string domain);
    
    Task<EmailServerSettings?> DiscoverSmtpSettingsAsync(string domain);
    
    Task<EmailServerSettings?> DiscoverAutoconfigAsync(string domain);
}
```

## 🗄️ Database Schema

### Core Tables

#### EmailMessages Table
```sql
CREATE TABLE EmailMessages (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MessageId TEXT NOT NULL,
    From TEXT NOT NULL,
    To TEXT NOT NULL,
    Cc TEXT NULL,
    Bcc TEXT NULL,
    Subject TEXT NOT NULL,
    TextBody TEXT NULL,
    HtmlBody TEXT NULL,
    DateReceived DATETIME NOT NULL,
    DateSent DATETIME NOT NULL,
    IsRead BOOLEAN NOT NULL DEFAULT 0,
    IsImportant BOOLEAN NOT NULL DEFAULT 0,
    HasAttachments BOOLEAN NOT NULL DEFAULT 0,
    FolderName TEXT NOT NULL DEFAULT 'INBOX',
    AccountId INTEGER NOT NULL,
    FOREIGN KEY (AccountId) REFERENCES EmailAccounts (Id)
);
```

#### EmailAccounts Table
```sql
CREATE TABLE EmailAccounts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    DisplayName TEXT NOT NULL,
    EmailAddress TEXT NOT NULL UNIQUE,
    EncryptedPassword TEXT NOT NULL,
    ImapServer TEXT NOT NULL,
    ImapPort INTEGER NOT NULL DEFAULT 993,
    ImapUseSsl BOOLEAN NOT NULL DEFAULT 1,
    SmtpServer TEXT NOT NULL,
    SmtpPort INTEGER NOT NULL DEFAULT 587,
    SmtpUseTls BOOLEAN NOT NULL DEFAULT 1,
    IsDefault BOOLEAN NOT NULL DEFAULT 0,
    IsEnabled BOOLEAN NOT NULL DEFAULT 1,
    LastSyncTime DATETIME NULL,
    Signature TEXT NULL
);
```

#### CalendarEvents Table
```sql
CREATE TABLE CalendarEvents (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT NULL,
    Location TEXT NULL,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    IsAllDay BOOLEAN NOT NULL DEFAULT 0,
    TimeZone TEXT NULL,
    RecurrenceType INTEGER NOT NULL DEFAULT 0,
    CalendarId TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

### Indexes

```sql
-- Email performance indexes
CREATE INDEX IX_EmailMessages_AccountId ON EmailMessages (AccountId);
CREATE INDEX IX_EmailMessages_DateReceived ON EmailMessages (DateReceived DESC);
CREATE INDEX IX_EmailMessages_IsRead ON EmailMessages (IsRead);
CREATE INDEX IX_EmailMessages_FolderName ON EmailMessages (FolderName);

-- Calendar indexes
CREATE INDEX IX_CalendarEvents_StartTime ON CalendarEvents (StartTime);
CREATE INDEX IX_CalendarEvents_EndTime ON CalendarEvents (EndTime);
```

## 🔗 Integration Guide

### Adding Custom Email Providers

```csharp
public class CustomEmailProviderService : IEmailProviderDetectionService
{
    public async Task<EmailProviderSettings?> DetectProviderAsync(string emailAddress)
    {
        var domain = emailAddress.Split('@')[1].ToLower();
        
        return domain switch
        {
            "custom.com" => new EmailProviderSettings
            {
                ProviderName = "Custom Provider",
                ImapServer = "imap.custom.com",
                ImapPort = 993,
                ImapUseSsl = true,
                SmtpServer = "smtp.custom.com",
                SmtpPort = 587,
                SmtpUseTls = true
            },
            _ => null
        };
    }
}
```

### Extending Calendar Functionality

```csharp
public class CustomCalendarService : ICalendarService
{
    private readonly ICalendarService _baseService;
    
    public CustomCalendarService(ICalendarService baseService)
    {
        _baseService = baseService;
    }
    
    public async Task<bool> CreateEventAsync(CalendarEvent calendarEvent)
    {
        // Add custom validation or processing
        if (await ValidateCustomRules(calendarEvent))
        {
            return await _baseService.CreateEventAsync(calendarEvent);
        }
        
        return false;
    }
    
    private async Task<bool> ValidateCustomRules(CalendarEvent calendarEvent)
    {
        // Custom validation logic
        return true;
    }
}
```

### Custom Middleware

```csharp
public class EmailSecurityMiddleware
{
    private readonly RequestDelegate _next;
    
    public EmailSecurityMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Add custom security headers
        context.Response.Headers.Add("X-Email-Client", "SeuMail/1.2.0");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        
        await _next(context);
    }
}
```

## 🔌 Extension Points

### Plugin Architecture (Future)

```csharp
public interface IEmailPlugin
{
    string Name { get; }
    string Version { get; }
    string Author { get; }
    
    Task<bool> InitializeAsync(IServiceProvider serviceProvider);
    
    Task<bool> ProcessEmailAsync(EmailMessage email, PluginContext context);
    
    Task ShutdownAsync();
}
```

### Custom Email Rules

```csharp
public interface IEmailRule
{
    string Name { get; }
    
    Task<bool> ShouldApplyAsync(EmailMessage email);
    
    Task<EmailRuleResult> ApplyAsync(EmailMessage email);
}

public class EmailRuleResult
{
    public bool Success { get; set; }
    public string? TargetFolder { get; set; }
    public bool MarkAsRead { get; set; }
    public bool MarkAsImportant { get; set; }
    public bool Delete { get; set; }
    public string? ForwardTo { get; set; }
}
```

### Event System

```csharp
public interface IEmailEventHandler
{
    Task HandleEmailReceivedAsync(EmailReceivedEvent eventArgs);
    Task HandleEmailSentAsync(EmailSentEvent eventArgs);
    Task HandleEmailDeletedAsync(EmailDeletedEvent eventArgs);
}

public class EmailReceivedEvent
{
    public EmailMessage Email { get; set; } = null!;
    public EmailAccount Account { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

## 📈 Performance Considerations

### Database Optimization

```csharp
// Use compiled queries for frequently executed operations
private static readonly Func<EmailDbContext, int, Task<EmailMessage?>> GetEmailById =
    EF.CompileAsyncQuery((EmailDbContext context, int id) =>
        context.EmailMessages
            .Include(e => e.Account)
            .Include(e => e.Attachments)
            .FirstOrDefault(e => e.Id == id));
```

### Caching Strategy

```csharp
public class CachedEmailService : IEmailService
{
    private readonly IEmailService _baseService;
    private readonly IMemoryCache _cache;
    
    public async Task<EmailMessage> GetEmailByIdAsync(int emailId)
    {
        var cacheKey = $"email_{emailId}";
        
        if (_cache.TryGetValue(cacheKey, out EmailMessage cachedEmail))
        {
            return cachedEmail;
        }
        
        var email = await _baseService.GetEmailByIdAsync(emailId);
        
        _cache.Set(cacheKey, email, TimeSpan.FromMinutes(10));
        
        return email;
    }
}
```

### Async Best Practices

```csharp
// Good: Use ConfigureAwait(false) for library code
public async Task<bool> SendEmailAsync(EmailMessage email)
{
    await ProcessEmailAsync(email).ConfigureAwait(false);
    return await SaveToDatabase(email).ConfigureAwait(false);
}

// Good: Use Task.WhenAll for parallel operations
public async Task SyncMultipleAccountsAsync(List<EmailAccount> accounts)
{
    var tasks = accounts.Select(account => SyncAccountAsync(account));
    await Task.WhenAll(tasks);
}
```

## 📚 Additional Resources

### Code Examples

Complete examples available in the `/examples` directory:
- Custom email provider implementation
- Plugin development template
- Advanced search implementation
- Calendar integration examples

### Testing

```csharp
[Test]
public async Task EmailService_SendEmail_Success()
{
    // Arrange
    var mockContext = new Mock<EmailDbContext>();
    var service = new EmailService(mockContext.Object);
    var account = CreateTestAccount();
    
    // Act
    var result = await service.SendEmailAsync(
        account, 
        "test@example.com", 
        "Test Subject", 
        "Test Body");
    
    // Assert
    Assert.True(result);
}
```

### Documentation

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [MailKit Documentation](https://github.com/jstedfast/MailKit)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)

---

## 📝 Changelog

### Version 1.2.0
- Added calendar API endpoints
- Implemented SignalR for real-time updates
- Enhanced security services
- Added email provider detection

### Version 1.1.0
- Initial API documentation
- Core service interfaces defined
- Database schema established
- Basic extension points added

---

This API documentation is maintained alongside the codebase. For the most up-to-date information, refer to the source code and inline documentation.

**Last Updated**: December 2024