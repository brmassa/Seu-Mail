namespace Seu.Mail.Core.Models;

/// <summary>
/// Represents configuration settings for an email provider.
/// </summary>
public class EmailProviderSettings
{
    /// <summary>
    /// Display name of the email provider.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server address.
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port.
    /// </summary>
    public int SmtpPort { get; set; }

    /// <summary>
    /// IMAP server address.
    /// </summary>
    public string ImapServer { get; set; } = string.Empty;

    /// <summary>
    /// IMAP server port.
    /// </summary>
    public int ImapPort { get; set; }

    /// <summary>
    /// Indicates whether SSL should be used for connections.
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// List of domain patterns supported by this provider.
    /// </summary>
    public List<string> DomainPatterns { get; set; } = new();
}

/// <summary>
/// Represents a configuration containing multiple email providers.
/// </summary>
public class EmailProvidersConfiguration
{
    /// <summary>
    /// Dictionary of provider names and their settings.
    /// </summary>
    public Dictionary<string, EmailProviderSettings> Providers { get; set; } = new();
}

/// <summary>
/// Represents the result of testing an email account's connection settings.
/// </summary>
public class ConnectionTestResult
{
    /// <summary>
    /// Indicates whether the connection test was successful.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Indicates whether the SMTP connection was successful.
    /// </summary>
    public bool SmtpSuccess { get; set; }

    /// <summary>
    /// Indicates whether the IMAP connection was successful.
    /// </summary>
    public bool ImapSuccess { get; set; }

    /// <summary>
    /// Error message from the SMTP connection test, if any.
    /// </summary>
    public string? SmtpError { get; set; }

    /// <summary>
    /// Error message from the IMAP connection test, if any.
    /// </summary>
    public string? ImapError { get; set; }

    /// <summary>
    /// General error message, if any.
    /// </summary>
    public string? GeneralError { get; set; }

    /// <summary>
    /// Gets a combined error message summarizing all connection errors.
    /// </summary>
    /// <returns>A string describing the connection errors, or success message.</returns>
    public string GetErrorMessage()
    {
        if (IsSuccessful)
            return "Connection test successful!";

        var errors = new List<string>();

        if (!SmtpSuccess && !string.IsNullOrEmpty(SmtpError))
            errors.Add($"SMTP: {SmtpError}");

        if (!ImapSuccess && !string.IsNullOrEmpty(ImapError))
            errors.Add($"IMAP: {ImapError}");

        if (!string.IsNullOrEmpty(GeneralError))
            errors.Add(GeneralError);

        return errors.Any() ? string.Join("; ", errors) : "Connection test failed";
    }
}
