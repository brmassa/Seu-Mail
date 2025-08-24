using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Seu.Mail.Core.Models;
using Seu.Mail.Contracts.Services;
using Seu.Mail.Core.Models.Shared;

namespace Seu.Mail.Services;

/// <summary>
/// Provides validation and sanitization services for email, password, URLs, file names, HTML content, and account data.
/// </summary>
public class ValidationService : IValidationService
{
    private readonly ILogger<ValidationService> _logger;
    private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);
    private static readonly Regex UrlRegex = new(@"^https?://[a-zA-Z0-9.-]+(?:\.[a-zA-Z]{2,})+(?:/[^\s]*)?$", RegexOptions.Compiled);
    private static readonly Regex ServerRegex = new(@"^[a-zA-Z0-9.-]+(?:\.[a-zA-Z]{2,})+$", RegexOptions.Compiled);
    private static readonly Regex IpAddressRegex = new(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", RegexOptions.Compiled);
    private static readonly Regex FileNameRegex = new(@"^[a-zA-Z0-9._-]+$", RegexOptions.Compiled);
    private static readonly Regex MaliciousPatterns = new(@"(<script[^>]*>.*?</script>|javascript:|data:|vbscript:|onload=|onerror=|onclick=|onmouseover=)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationService"/> class.
    /// </summary>
    /// <param name="logger">Logger for validation events and errors.</param>
    public ValidationService(ILogger<ValidationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates an email address.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>Validation result.</returns>
    public InputValidationResult ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new InputValidationResult(false, "Email address is required");
        }

        if (email.Length > 254)
        {
            return new InputValidationResult(false, "Email address is too long (maximum 254 characters)");
        }

        // Use a more permissive regex that allows unicode and double dots in domain
        if (!EmailRegex.IsMatch(email))
        {
            return new InputValidationResult(false, "Invalid email address format");
        }

        try
        {
            var mailAddress = new MailAddress(email);
            // Accept if MailAddress can parse it successfully
            return new InputValidationResult(true);
        }
        catch (FormatException)
        {
            return new InputValidationResult(false, "Invalid email address format");
        }
    }

    /// <summary>
    /// Validates a password for length and complexity.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>Validation result.</returns>
    public InputValidationResult ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return new InputValidationResult(false, "Password is required");
        }

        if (password.Length < 8)
        {
            return new InputValidationResult(false, "Password must be at least 8 characters long");
        }

        if (password.Length > 128)
        {
            return new InputValidationResult(false, "Password is too long");
        }

        // Check for basic password complexity
        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

        if (!hasUpper || !hasLower || !hasDigit || !hasSpecial)
        {
            return new InputValidationResult(false, "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");
        }

        return new InputValidationResult(true);
    }

    /// <summary>
    /// Validates a URL for format and protocol.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>Validation result.</returns>
    public InputValidationResult ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return new InputValidationResult(false, "URL is required");
        }

        if (url.Length > 2048)
        {
            return new InputValidationResult(false, "URL is too long (maximum 2048 characters)");
        }

        if (!UrlRegex.IsMatch(url))
        {
            return new InputValidationResult(false, "Invalid URL format");
        }

        try
        {
            var uri = new Uri(url);
            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                return new InputValidationResult(false, "URL must use HTTP or HTTPS protocol");
            }

            // Check for suspicious domains
            var suspiciousDomains = new[] { "localhost", "127.0.0.1", "0.0.0.0", "::1" };
            if (suspiciousDomains.Contains(uri.Host.ToLower()))
            {
                return new InputValidationResult(false, "Local URLs are not allowed");
            }
        }
        catch (UriFormatException)
        {
            return new InputValidationResult(false, "Invalid URL format");
        }

        return new InputValidationResult(true);
    }

    /// <summary>
    /// Validates a file name for length, reserved names, and invalid characters.
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns>Validation result.</returns>
    public InputValidationResult ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return new InputValidationResult(false, "File name is required");
        }

        if (fileName.Length > 255)
        {
            return new InputValidationResult(false, "File name is too long (maximum 255 characters)");
        }

        // Check for directory traversal attempts
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            return new InputValidationResult(false, "File name contains invalid characters");
        }

        // Check for reserved Windows file names
        var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName).ToUpper();
        if (reservedNames.Contains(nameWithoutExtension))
        {
            return new InputValidationResult(false, "File name uses a reserved system name");
        }

        // Check for invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.Any(invalidChars.Contains))
        {
            return new InputValidationResult(false, "File name contains invalid characters");
        }

        return new InputValidationResult(true);
    }

    /// <summary>
    /// Validates HTML content for size and malicious patterns.
    /// </summary>
    /// <param name="htmlContent">The HTML content to validate.</param>
    /// <returns>Validation result.</returns>
    public InputValidationResult ValidateHtmlContent(string htmlContent)
    {
        if (string.IsNullOrEmpty(htmlContent))
        {
            return new InputValidationResult(true); // Empty content is valid
        }

        if (htmlContent.Length > 1048576) // 1MB limit
        {
            return new InputValidationResult(false, "HTML content is too large (maximum 1MB)");
        }

        // Check for malicious patterns
        if (ContainsMaliciousPatterns(htmlContent))
        {
            return new InputValidationResult(false, "HTML content contains potentially malicious code");
        }

        return new InputValidationResult(true);
    }

    /// <summary>
    /// Validates server settings for address format and port range.
    /// </summary>
    /// <param name="server">The server address.</param>
    /// <param name="port">The port number.</param>
    /// <returns>Validation result.</returns>
    public InputValidationResult ValidateServerSettings(string server, int port)
    {
        if (string.IsNullOrWhiteSpace(server))
        {
            return new InputValidationResult(false, "Server address is required");
        }

        if (server.Length > 253)
        {
            return new InputValidationResult(false, "Server address is too long (maximum 253 characters)");
        }

        // Allow both domain names and IP addresses
        if (!ServerRegex.IsMatch(server) && !IpAddressRegex.IsMatch(server))
        {
            return new InputValidationResult(false, "Invalid server address format");
        }

        if (port < 1 || port > 65535)
        {
            return new InputValidationResult(false, "Port must be between 1 and 65535");
        }

        // Check for suspicious ports
        var commonMaliciousPorts = new[] { 135, 139, 445, 593, 1433, 1521, 3306, 3389, 5432, 6379, 27017 };
        if (commonMaliciousPorts.Contains(port))
        {
            _logger.LogWarning("Potentially suspicious port {Port} used for server {Server}", port, server);
        }

        return new InputValidationResult(true);
    }

    /// <summary>
    /// Validates an email account's data including email, password, display name, and server settings.
    /// </summary>
    /// <param name="account">The account to validate.</param>
    /// <returns>Validation result.</returns>
    public InputValidationResult ValidateAccountData(EmailAccount account)
    {
        if (account == null)
        {
            return new InputValidationResult(false, "Account cannot be null");
        }

        var errors = new List<string>();

        // Validate email
        var emailResult = ValidateEmail(account.Email);
        if (!emailResult.IsValid)
        {
            errors.Add($"Email: {emailResult.ErrorMessage}");
        }

        // Validate password
        if (!string.IsNullOrWhiteSpace(account.Password))
        {
            var passwordResult = ValidatePassword(account.Password);
            if (!passwordResult.IsValid)
            {
                errors.Add($"Password: {passwordResult.ErrorMessage}");
            }
        }

        // Validate display name
        if (!string.IsNullOrWhiteSpace(account.DisplayName))
        {
            if (account.DisplayName.Length > 256)
            {
                errors.Add("Display name is too long (maximum 256 characters)");
            }

            if (ContainsMaliciousPatterns(account.DisplayName))
            {
                errors.Add("Display name contains invalid characters");
            }
        }

        // Validate SMTP server
        var smtpResult = ValidateServerSettings(account.SmtpServer, account.SmtpPort);
        if (!smtpResult.IsValid)
        {
            errors.Add($"SMTP Server: {smtpResult.ErrorMessage}");
        }

        // Validate IMAP server
        var imapResult = ValidateServerSettings(account.ImapServer, account.ImapPort);
        if (!imapResult.IsValid)
        {
            errors.Add($"IMAP Server: {imapResult.ErrorMessage}");
        }

        if (errors.Any())
        {
            return new InputValidationResult(false, string.Join("; ", errors));
        }

        return new InputValidationResult(true);
    }

    /// <summary>
    /// Sanitizes an input string by removing dangerous characters and normalizing whitespace.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <returns>Sanitized string.</returns>
    public string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Remove null bytes
        input = input.Replace("\0", string.Empty);

        // Remove or escape potentially dangerous characters
        input = input.Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&#x27;")
                    .Replace("&", "&amp;");

        // Trim and normalize whitespace
        input = Regex.Replace(input.Trim(), @"\s+", " ");

        return input;
    }

    /// <summary>
    /// Sanitizes a file name by removing dangerous and invalid characters.
    /// </summary>
    /// <param name="fileName">The file name to sanitize.</param>
    /// <returns>Sanitized file name.</returns>
    public string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "untitled";

        // Remove path separators and other dangerous characters
        var sanitized = Regex.Replace(fileName, @"[<>:""/\\|?*]", "_");

        // Remove any remaining invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        sanitized = new string(sanitized.Where(ch => !invalidChars.Contains(ch)).ToArray());

        // Ensure it's not empty after sanitization
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "untitled";
        }

        // Truncate if too long
        if (sanitized.Length > 255)
        {
            var extension = Path.GetExtension(sanitized);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitized);
            sanitized = nameWithoutExtension.Substring(0, 255 - extension.Length) + extension;
        }

        return sanitized;
    }

    /// <summary>
    /// Checks if a file name has a valid extension.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <param name="allowedExtensions">Allowed extensions.</param>
    /// <returns>True if valid, otherwise false.</returns>
    public bool IsValidFileExtension(string fileName, string[] allowedExtensions)
    {
        if (string.IsNullOrWhiteSpace(fileName) || allowedExtensions == null || allowedExtensions.Length == 0)
            return false;

        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
            return false;

        return allowedExtensions.Any(ext => ext.ToLowerInvariant() == extension);
    }

    /// <summary>
    /// Checks if input contains malicious patterns such as scripts or dangerous attributes.
    /// </summary>
    /// <param name="input">The input string to check.</param>
    /// <returns>True if malicious patterns are found, otherwise false.</returns>
    public bool ContainsMaliciousPatterns(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return MaliciousPatterns.IsMatch(input);
    }
}
