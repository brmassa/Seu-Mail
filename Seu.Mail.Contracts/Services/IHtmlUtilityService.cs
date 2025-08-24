namespace Seu.Mail.Contracts.Services;

/// <summary>
/// Provides utility methods for working with HTML content.
/// </summary>
public interface IHtmlUtilityService
{
    /// <summary>
    /// Removes all HTML tags from the specified string.
    /// </summary>
    /// <param name="html">The HTML string to strip tags from.</param>
    /// <returns>A plain text string without HTML tags.</returns>
    string StripHtml(string html);

    /// <summary>
    /// Converts HTML content to plain text.
    /// </summary>
    /// <param name="html">The HTML string to convert.</param>
    /// <param name="preserveImages">Whether to preserve image alt text in the conversion.</param>
    /// <returns>A plain text representation of the HTML content.</returns>
    string ConvertHtmlToText(string html, bool preserveImages = false);

    /// <summary>
    /// Sanitizes HTML content to remove potentially dangerous elements.
    /// </summary>
    /// <param name="html">The HTML string to sanitize.</param>
    /// <returns>Sanitized HTML string.</returns>
    string SanitizeHtml(string html);

    /// <summary>
    /// Determines whether the specified content contains HTML tags.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns>True if the content contains HTML; otherwise, false.</returns>
    bool ContainsHtml(string content);
}