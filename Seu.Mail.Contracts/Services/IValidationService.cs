using Seu.Mail.Core.Models;
using Seu.Mail.Core.Models.Shared;

namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Service interface for validating input data and sanitizing content.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates an email address.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>Validation result.</returns>
    InputValidationResult ValidateEmail(string email);

    /// <summary>
    /// Validates a password.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>Validation result.</returns>
    InputValidationResult ValidatePassword(string password);

    /// <summary>
    /// Validates a URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>Validation result.</returns>
    InputValidationResult ValidateUrl(string url);

    /// <summary>
    /// Validates a file name.
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns>Validation result.</returns>
    InputValidationResult ValidateFileName(string fileName);

    /// <summary>
    /// Validates HTML content.
    /// </summary>
    /// <param name="htmlContent">The HTML content to validate.</param>
    /// <returns>Validation result.</returns>
    InputValidationResult ValidateHtmlContent(string htmlContent);

    /// <summary>
    /// Validates server settings.
    /// </summary>
    /// <param name="server">The server address.</param>
    /// <param name="port">The port number.</param>
    /// <returns>Validation result.</returns>
    InputValidationResult ValidateServerSettings(string server, int port);

    /// <summary>
    /// Validates account data.
    /// </summary>
    /// <param name="account">The account to validate.</param>
    /// <returns>Validation result.</returns>
    InputValidationResult ValidateAccountData(EmailAccount account);

    /// <summary>
    /// Sanitizes input string.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <returns>Sanitized string.</returns>
    string SanitizeInput(string input);

    /// <summary>
    /// Sanitizes a file name.
    /// </summary>
    /// <param name="fileName">The file name to sanitize.</param>
    /// <returns>Sanitized file name.</returns>
    string SanitizeFileName(string fileName);

    /// <summary>
    /// Checks if a file name has a valid extension.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <param name="allowedExtensions">Allowed extensions.</param>
    /// <returns>True if valid, otherwise false.</returns>
    bool IsValidFileExtension(string fileName, string[] allowedExtensions);

    /// <summary>
    /// Checks if input contains malicious patterns.
    /// </summary>
    /// <param name="input">The input string to check.</param>
    /// <returns>True if malicious patterns are found, otherwise false.</returns>
    bool ContainsMaliciousPatterns(string input);
}