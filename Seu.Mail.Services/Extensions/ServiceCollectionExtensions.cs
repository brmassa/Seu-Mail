using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Seu.Mail.Contracts.Services;

namespace Seu.Mail.Services.Extensions;

/// <summary>
/// Extension methods for registering email services in the dependency injection container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all core email services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Core email services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IEncryptionService, EncryptionService>();

        // Calendar services
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<ICalendarSettingsService, CalendarSettingsService>();

        // Discovery and detection services
        services.AddScoped<IDnsEmailDiscoveryService, DnsEmailDiscoveryService>();
        services.AddScoped<IEmailHttpClient, DefaultEmailHttpClient>();
        services.AddScoped<IEmailAutodiscoveryService, EmailAutodiscoveryService>();
        services.AddScoped<IEmailProviderDetectionService, EmailProviderDetectionService>();

        // Utility and management services
        services.AddScoped<IFolderTagService, FolderTagService>();
        services.AddScoped<IHtmlUtilityService, HtmlUtilityService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();

        // HTTP clients for external services
        services.AddHttpClient();

        return services;
    }

    /// <summary>
    /// Registers calendar services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCalendarModule(this IServiceCollection services)
    {
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<ICalendarSettingsService, CalendarSettingsService>();

        return services;
    }

    /// <summary>
    /// Registers email services with custom configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure email options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        Action<EmailServiceOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services.AddEmailServices();
    }
}

/// <summary>
/// Configuration options for email services
/// </summary>
public class EmailServiceOptions
{
    /// <summary>
    /// Maximum attachment size in bytes
    /// </summary>
    public long MaxAttachmentSize { get; set; } = 26214400; // 25MB

    /// <summary>
    /// Default sync interval in minutes
    /// </summary>
    public int DefaultSyncInterval { get; set; } = 15;

    /// <summary>
    /// Maximum number of emails to sync per operation
    /// </summary>
    public int MaxEmailsPerSync { get; set; } = 100;

    /// <summary>
    /// Enable automatic email synchronization
    /// </summary>
    public bool EnableAutoSync { get; set; } = true;

    /// <summary>
    /// Default page size for email listings
    /// </summary>
    public int DefaultPageSize { get; set; } = 50;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Enable email caching
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache expiration time in minutes
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 10;

    /// <summary>
    /// Maximum number of concurrent IMAP connections
    /// </summary>
    public int MaxConcurrentConnections { get; set; } = 5;

    /// <summary>
    /// Temporary directory for attachment processing
    /// </summary>
    public string TempDirectory { get; set; } = Path.GetTempPath();

    /// <summary>
    /// Enable detailed logging for email operations
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
}
