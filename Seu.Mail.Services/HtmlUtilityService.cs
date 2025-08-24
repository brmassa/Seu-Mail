using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Seu.Mail.Contracts.Services;

namespace Seu.Mail.Services;

/// <summary>
/// Provides HTML utility services for processing, sanitizing, and converting HTML content.
/// </summary>
public class HtmlUtilityService : IHtmlUtilityService
{
    private readonly ILogger<HtmlUtilityService>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlUtilityService"/> class.
    /// </summary>
    /// <param name="logger">Logger for HTML utility service events and errors (optional).</param>
    public HtmlUtilityService(ILogger<HtmlUtilityService>? logger = null)
    {
        _logger = logger;
    }

    // Dangerous tags that should always be removed
    private static readonly string[] DangerousTags = {
        "script", "object", "embed", "form", "input", "button", "iframe",
        "frame", "frameset", "noframes", "meta", "base", "link", "style",
        "applet", "param", "xml", "bgsound", "marquee", "layer", "ilayer"
    };

    // Dangerous attributes that should always be removed
    private static readonly string[] DangerousAttributes = {
        "javascript", "vbscript", "onabort", "onactivate", "onafterprint", "onafterupdate",
        "onbeforeactivate", "onbeforecopy", "onbeforecut", "onbeforedeactivate", "onbeforeeditfocus",
        "onbeforepaste", "onbeforeprint", "onbeforeunload", "onbeforeupdate", "onblur", "onbounce",
        "oncellchange", "onchange", "onclick", "oncontextmenu", "oncontrolselect", "oncopy", "oncut",
        "ondataavailable", "ondatasetchanged", "ondatasetcomplete", "ondblclick", "ondeactivate",
        "ondrag", "ondragend", "ondragenter", "ondragleave", "ondragover", "ondragstart", "ondrop",
        "onerror", "onerrorupdate", "onfilterchange", "onfinish", "onfocus", "onfocusin", "onfocusout",
        "onhelp", "onkeydown", "onkeypress", "onkeyup", "onlayoutcomplete", "onload", "onlosecapture",
        "onmousedown", "onmouseenter", "onmouseleave", "onmousemove", "onmouseout", "onmouseover",
        "onmouseup", "onmousewheel", "onmove", "onmoveend", "onmovestart", "onpaste", "onpropertychange",
        "onreadystatechange", "onreset", "onresize", "onresizeend", "onresizestart", "onrowenter",
        "onrowexit", "onrowsdelete", "onrowsinserted", "onscroll", "onselect", "onselectionchange",
        "onselectstart", "onstart", "onstop", "onsubmit", "onunload"
    };

    /// <summary>
    /// Simple HTML stripping for titles and simple content
    /// </summary>
    public string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove script and style elements completely
            var scriptsAndStyles = doc.DocumentNode.SelectNodes("//script | //style");
            if (scriptsAndStyles != null)
            {
                foreach (var node in scriptsAndStyles)
                {
                    node.Remove();
                }
            }

            var text = doc.DocumentNode.InnerText;

            // Decode HTML entities
            text = HtmlEntity.DeEntitize(text);

            // Clean up whitespace
            text = Regex.Replace(text, @"\s+", " ").Trim();

            return text;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error parsing HTML, falling back to simple strip");
            // Fallback to simple regex if HtmlAgilityPack fails
            return SimpleStripHtml(html);
        }
    }

    /// <summary>
    /// Convert HTML to readable text, preserving structure and optionally images
    /// </summary>
    public string ConvertHtmlToText(string html, bool preserveImages = false)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove script and style elements
            var scriptsAndStyles = doc.DocumentNode.SelectNodes("//script | //style");
            if (scriptsAndStyles != null)
            {
                foreach (var node in scriptsAndStyles)
                {
                    node.Remove();
                }
            }

            // Handle special elements before getting text
            HandleSpecialElements(doc, preserveImages);

            // Get the text content
            var text = doc.DocumentNode.InnerText;

            // Decode HTML entities
            text = HtmlEntity.DeEntitize(text);

            // Clean up whitespace but preserve line breaks
            text = Regex.Replace(text, @"[ \t]+", " "); // Multiple spaces/tabs to single space
            text = Regex.Replace(text, @"\n[ \t]+", "\n"); // Remove spaces after newlines
            text = Regex.Replace(text, @"[ \t]+\n", "\n"); // Remove spaces before newlines
            text = Regex.Replace(text, @"\n{3,}", "\n\n"); // Max 2 consecutive line breaks
            text = text.Trim();

            return text;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error converting HTML to text");
            return StripHtml(html);
        }
    }

    /// <summary>
    /// Sanitize HTML for safe display (remove dangerous elements)
    /// </summary>
    public string SanitizeHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove dangerous elements
            RemoveDangerousTags(doc);

            // Remove dangerous attributes
            RemoveDangerousAttributes(doc);

            // Make all links safe
            MakeLinksSafe(doc);

            // Make images responsive and safe
            MakeImagesSafe(doc);

            return doc.DocumentNode.OuterHtml;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error sanitizing HTML, falling back to simple cleaning");
            // Fallback to simple regex cleaning
            var sanitized = Regex.Replace(html, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            sanitized = Regex.Replace(sanitized, @"<link[^>]*>", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"on\w+\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
            return sanitized;
        }
    }

    /// <summary>
    /// Checks if content contains HTML tags. Only detects proper HTML tags, not just &lt; and &gt; symbols.
    /// </summary>
    public bool ContainsHtml(string content)
    {
        if (string.IsNullOrEmpty(content))
            return false;

        // First, quick check for angle brackets
        if (!content.Contains('<') || !content.Contains('>'))
            return false;

        // Check for specific patterns that are NOT HTML
        if (IsNonHtmlPattern(content))
            return false;

        // Look for actual HTML tag patterns
        // This regex matches proper HTML tags: <tag>, </tag>, <tag attr="value">, <tag/>
        var htmlTagPattern = @"<\s*/?[a-zA-Z][a-zA-Z0-9]*(?:\s+[^<>]*?)?\s*/?>";
        var xmlPattern = @"<\?\w+[^>]*\?>";  // XML declarations like <?xml ?>

        return Regex.IsMatch(content, htmlTagPattern, RegexOptions.IgnoreCase) ||
               Regex.IsMatch(content, xmlPattern, RegexOptions.IgnoreCase);
    }

    private bool IsNonHtmlPattern(string content)
    {
        // Check for mathematical expressions like "5 < 10 and 20 > 15"
        if (Regex.IsMatch(content, @"\b\d+\s*[<>]\s*\d+\b"))
            return true;

        // Check for generic "< and >" text patterns
        if (Regex.IsMatch(content, @"<\s+and\s+>|<\s*and\s*>"))
            return true;

        // Check for template expressions like "template<int>"
        if (Regex.IsMatch(content, @"\w+<\w+>"))
            return true;

        // Check for email-like patterns
        if (Regex.IsMatch(content, @"<[^>]*@[^>]*>"))
            return true;

        // Check if all content between < > are just symbols/spaces
        var matches = Regex.Matches(content, @"<([^>]*)>");
        foreach (Match match in matches)
        {
            var innerContent = match.Groups[1].Value.Trim();

            // If inner content is just spaces, symbols, or "and"
            if (string.IsNullOrWhiteSpace(innerContent) ||
                innerContent.Equals("and", StringComparison.OrdinalIgnoreCase) ||
                Regex.IsMatch(innerContent, @"^[\s\w]*\s+and\s+[\s\w]*$", RegexOptions.IgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private void RemoveDangerousTags(HtmlDocument doc)
    {
        foreach (var tag in DangerousTags)
        {
            var nodes = doc.DocumentNode.SelectNodes($"//{tag}");
            if (nodes != null)
            {
                foreach (var node in nodes.ToList())
                {
                    node.Remove();
                }
            }
        }
    }

    private void RemoveDangerousAttributes(HtmlDocument doc)
    {
        var allNodes = doc.DocumentNode.SelectNodes("//*[@*]");
        if (allNodes != null)
        {
            foreach (var node in allNodes)
            {
                var attributesToRemove = new List<HtmlAttribute>();

                foreach (var attr in node.Attributes)
                {
                    var attrName = attr.Name.ToLowerInvariant();
                    var attrValue = attr.Value?.ToLowerInvariant() ?? string.Empty;

                    // Remove event handlers
                    if (DangerousAttributes.Contains(attrName))
                    {
                        attributesToRemove.Add(attr);
                        continue;
                    }

                    // Remove attributes starting with "on"
                    if (attrName.StartsWith("on"))
                    {
                        attributesToRemove.Add(attr);
                        continue;
                    }

                    // Check for javascript: or other dangerous protocols
                    if (attrValue.Contains("javascript:") || attrValue.Contains("vbscript:") ||
                        attrValue.Contains("data:") || attrValue.Contains("file:"))
                    {
                        attributesToRemove.Add(attr);
                        continue;
                    }

                    // Remove style attributes with dangerous content
                    if (attrName == "style" && ContainsDangerousStyle(attrValue))
                    {
                        attributesToRemove.Add(attr);
                        continue;
                    }
                }

                foreach (var attr in attributesToRemove)
                {
                    node.Attributes.Remove(attr);
                }
            }
        }
    }

    private void MakeLinksSafe(HtmlDocument doc)
    {
        var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
        if (linkNodes != null)
        {
            foreach (var link in linkNodes)
            {
                link.SetAttributeValue("target", "_blank");
                link.SetAttributeValue("rel", "noopener noreferrer nofollow");

                // Validate href attribute
                var href = link.GetAttributeValue("href", "");
                if (IsUnsafeUrl(href))
                {
                    link.Remove();
                }
            }
        }
    }

    private void MakeImagesSafe(HtmlDocument doc)
    {
        var imgNodes = doc.DocumentNode.SelectNodes("//img");
        if (imgNodes != null)
        {
            foreach (var img in imgNodes)
            {
                img.SetAttributeValue("style", "max-width: 100%; height: auto;");

                // Validate src attribute
                var src = img.GetAttributeValue("src", "");
                if (IsUnsafeUrl(src))
                {
                    img.Remove();
                }
            }
        }
    }

    private bool IsUnsafeUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        var lowerUrl = url.ToLowerInvariant().Trim();

        // Block dangerous protocols
        var dangerousProtocols = new[] { "javascript:", "vbscript:", "data:", "file:", "ftp:" };
        return dangerousProtocols.Any(protocol => lowerUrl.StartsWith(protocol));
    }

    private bool ContainsDangerousStyle(string style)
    {
        if (string.IsNullOrEmpty(style))
            return false;

        var dangerousStylePatterns = new[]
        {
            "javascript:", "expression(", "eval(", "url(javascript:", "@import",
            "behavior:", "binding:", "-moz-binding:", "position:fixed", "position:absolute"
        };

        return dangerousStylePatterns.Any(pattern => style.Contains(pattern));
    }

    private void HandleSpecialElements(HtmlDocument doc, bool preserveImages)
    {
        // Handle line breaks
        var brNodes = doc.DocumentNode.SelectNodes("//br");
        if (brNodes != null)
        {
            foreach (var br in brNodes)
            {
                br.ParentNode.ReplaceChild(HtmlNode.CreateNode("\n"), br);
            }
        }

        // Handle paragraphs and divs - add line breaks
        var blockNodes = doc.DocumentNode.SelectNodes("//p | //div | //h1 | //h2 | //h3 | //h4 | //h5 | //h6");
        if (blockNodes != null)
        {
            foreach (var block in blockNodes)
            {
                // Add newline after block elements
                var tagName = block.Name.ToLowerInvariant();
                var newlineAfter = tagName.StartsWith("h") ? "\n\n" : "\n";

                var textNode = HtmlNode.CreateNode(newlineAfter);
                block.ParentNode.InsertAfter(textNode, block);
            }
        }

        // Handle list items
        var liNodes = doc.DocumentNode.SelectNodes("//li");
        if (liNodes != null)
        {
            foreach (var li in liNodes)
            {
                var bullet = HtmlNode.CreateNode("• ");
                li.PrependChild(bullet);

                var newline = HtmlNode.CreateNode("\n");
                li.ParentNode.InsertAfter(newline, li);
            }
        }

        // Handle links
        var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
        if (linkNodes != null)
        {
            foreach (var link in linkNodes)
            {
                var href = link.GetAttributeValue("href", "");
                if (!string.IsNullOrEmpty(href) && !href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                {
                    var linkText = HtmlNode.CreateNode($" [{href}]");
                    link.ParentNode.InsertAfter(linkText, link);
                }
            }
        }

        // Handle images
        if (preserveImages)
        {
            var imgNodes = doc.DocumentNode.SelectNodes("//img");
            if (imgNodes != null)
            {
                foreach (var img in imgNodes)
                {
                    var alt = img.GetAttributeValue("alt", "");
                    var src = img.GetAttributeValue("src", "");
                    var placeholder = HtmlNode.CreateNode($"[IMAGE: {(!string.IsNullOrEmpty(alt) ? alt : src)}]");
                    img.ParentNode.ReplaceChild(placeholder, img);
                }
            }
        }
        else
        {
            // Remove images if not preserving them
            var imgNodes = doc.DocumentNode.SelectNodes("//img");
            if (imgNodes != null)
            {
                foreach (var img in imgNodes)
                {
                    img.Remove();
                }
            }
        }

        // Handle blockquotes
        var quoteNodes = doc.DocumentNode.SelectNodes("//blockquote");
        if (quoteNodes != null)
        {
            foreach (var quote in quoteNodes)
            {
                var prefix = HtmlNode.CreateNode("> ");
                quote.PrependChild(prefix);

                var newline = HtmlNode.CreateNode("\n");
                quote.ParentNode.InsertAfter(newline, quote);
            }
        }

        // Handle horizontal rules
        var hrNodes = doc.DocumentNode.SelectNodes("//hr");
        if (hrNodes != null)
        {
            foreach (var hr in hrNodes)
            {
                var rule = HtmlNode.CreateNode("\n---\n");
                hr.ParentNode.ReplaceChild(rule, hr);
            }
        }

        // Handle table cells
        var cellNodes = doc.DocumentNode.SelectNodes("//td | //th");
        if (cellNodes != null)
        {
            foreach (var cell in cellNodes)
            {
                var separator = HtmlNode.CreateNode(" | ");
                cell.ParentNode.InsertAfter(separator, cell);
            }
        }

        // Handle table rows
        var rowNodes = doc.DocumentNode.SelectNodes("//tr");
        if (rowNodes != null)
        {
            foreach (var row in rowNodes)
            {
                var newline = HtmlNode.CreateNode("\n");
                row.ParentNode.InsertAfter(newline, row);
            }
        }
    }

    private string SimpleStripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        // Remove HTML tags using regex
        var stripped = Regex.Replace(html, @"<[^>]*>", "");

        // Decode HTML entities
        stripped = System.Net.WebUtility.HtmlDecode(stripped);

        // Clean up extra whitespace
        stripped = Regex.Replace(stripped, @"\s+", " ").Trim();

        return stripped;
    }
}
