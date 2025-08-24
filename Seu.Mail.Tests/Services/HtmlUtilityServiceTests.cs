using Seu.Mail.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Seu.Mail.Tests.Services;

public class HtmlUtilityServiceTests
{
    private readonly ILogger<HtmlUtilityService> _mockLogger;
    private readonly HtmlUtilityService _htmlUtilityService;

    public HtmlUtilityServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<HtmlUtilityService>>();
        _htmlUtilityService = new HtmlUtilityService(_mockLogger);
    }

    #region StripHtml Tests

    [Test]
    [Arguments("<p>Hello World</p>", "Hello World")]
    [Arguments("<div><b>Bold</b> text</div>", "Bold text")]
    [Arguments("<script>alert('xss')</script>Hello", "Hello")]
    [Arguments("<style>body{color:red;}</style>Content", "Content")]
    [Arguments("Plain text", "Plain text")]
    [Arguments("", "")]
    public async Task StripHtml_WithVariousInputs_ShouldRemoveTagsCorrectly(string input, string expected)
    {
        // Act
        var result = _htmlUtilityService.StripHtml(input);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task StripHtml_WithNullInput_ShouldReturnEmpty()
    {
        // Act
        var result = _htmlUtilityService.StripHtml(null!);

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task StripHtml_WithHtmlEntities_ShouldDecodeCorrectly()
    {
        // Arrange
        var input = "<p>&lt;Hello&gt; &amp; &quot;World&quot;</p>";

        // Act
        var result = _htmlUtilityService.StripHtml(input);

        // Assert
        await Assert.That(result).IsEqualTo("<Hello> & \"World\"");
    }

    [Test]
    public async Task StripHtml_WithExcessiveWhitespace_ShouldNormalize()
    {
        // Arrange
        var input = "<p>  Hello    \n\n   World  </p>";

        // Act
        var result = _htmlUtilityService.StripHtml(input);

        // Assert
        await Assert.That(result).IsEqualTo("Hello World");
    }

    #endregion

    #region ConvertHtmlToText Tests

    [Test]
    [Arguments("<p>Hello World</p>", "Hello World")]
    [Arguments("<h1>Title</h1><p>Content</p>", "Title\n\nContent")]
    [Arguments("<ul><li>Item 1</li><li>Item 2</li></ul>", "• Item 1\n• Item 2")]
    [Arguments("<a href=\"https://example.com\">Link</a>", "Link [https://example.com]")]
    [Arguments("<br>Line<br>Break", "Line\nBreak")]
    public async Task ConvertHtmlToText_WithVariousElements_ShouldConvertCorrectly(string input, string expected)
    {
        // Act
        var result = _htmlUtilityService.ConvertHtmlToText(input, false);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task ConvertHtmlToText_WithImages_WhenPreserveImagesTrue_ShouldShowImagePlaceholders()
    {
        // Arrange
        var input = "<img src=\"image.jpg\" alt=\"Test Image\"/>";

        // Act
        var result = _htmlUtilityService.ConvertHtmlToText(input, true);

        // Assert
        await Assert.That(result).Contains("[IMAGE: Test Image]");
    }

    [Test]
    public async Task ConvertHtmlToText_WithImages_WhenPreserveImagesFalse_ShouldIgnoreImages()
    {
        // Arrange
        var input = "<p>Text</p><img src=\"image.jpg\" alt=\"Test Image\"/><p>More text</p>";

        // Act
        var result = _htmlUtilityService.ConvertHtmlToText(input, false);

        // Assert
        await Assert.That(result).DoesNotContain("IMAGE");
        await Assert.That(result).Contains("Text");
        await Assert.That(result).Contains("More text");
    }

    [Test]
    public async Task ConvertHtmlToText_WithComplexStructure_ShouldMaintainReadableFormat()
    {
        // Arrange
        var input = @"
            <html>
                <body>
                    <h1>Main Title</h1>
                    <p>Introduction paragraph.</p>
                    <h2>Section Title</h2>
                    <ul>
                        <li>First item</li>
                        <li>Second item</li>
                    </ul>
                    <blockquote>This is a quote.</blockquote>
                    <p>Final paragraph with <strong>bold</strong> text.</p>
                </body>
            </html>";

        // Act
        var result = _htmlUtilityService.ConvertHtmlToText(input, false);

        // Assert
        await Assert.That(result).Contains("Main Title");
        await Assert.That(result).Contains("Introduction paragraph.");
        await Assert.That(result).Contains("• First item");
        await Assert.That(result).Contains("• Second item");
        await Assert.That(result).Contains("> This is a quote.");
        await Assert.That(result).Contains("bold");
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public async Task ConvertHtmlToText_WithNullOrEmpty_ShouldReturnEmpty(string? input)
    {
        // Act
        var result = _htmlUtilityService.ConvertHtmlToText(input!);

        // Assert
        await Assert.That(result).IsEmpty();
    }

    #endregion

    #region SanitizeHtml Tests

    [Test]
    public async Task SanitizeHtml_WithScriptTag_ShouldRemoveScript()
    {
        // Arrange
        var input = "<p>Safe content</p><script>alert('xss')</script>";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(input);

        // Assert
        await Assert.That(result).DoesNotContain("script");
        await Assert.That(result).DoesNotContain("alert");
        await Assert.That(result).Contains("Safe content");
    }

    [Test]
    public async Task SanitizeHtml_WithEventHandlers_ShouldRemoveEventHandlers()
    {
        // Arrange
        var input = "<div onclick=\"alert('xss')\" onmouseover=\"alert('xss')\">Content</div>";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(input);

        // Assert
        await Assert.That(result).DoesNotContain("onclick");
        await Assert.That(result).DoesNotContain("onmouseover");
        await Assert.That(result).DoesNotContain("alert");
        await Assert.That(result).Contains("Content");
    }

    [Test]
    public async Task SanitizeHtml_WithDangerousLinks_ShouldSanitizeOrRemove()
    {
        // Arrange
        var input = "<a href=\"javascript:alert('xss')\">Dangerous Link</a>";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(input);

        // Assert
        await Assert.That(result).DoesNotContain("javascript:");
        await Assert.That(result).DoesNotContain("alert");
    }

    [Test]
    public async Task SanitizeHtml_WithValidLinks_ShouldMakeSecure()
    {
        // Arrange
        var input = "<a href=\"https://example.com\">Safe Link</a>";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(input);

        // Assert
        await Assert.That(result).Contains("target=\"_blank\"");
        await Assert.That(result).Contains("rel=\"noopener noreferrer nofollow\"");
        await Assert.That(result).Contains("https://example.com");
    }

    [Test]
    public async Task SanitizeHtml_WithImages_ShouldMakeResponsive()
    {
        // Arrange
        var input = "<img src=\"image.jpg\" alt=\"Test\">";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(input);

        // Assert
        await Assert.That(result).Contains("max-width: 100%");
        await Assert.That(result).Contains("height: auto");
    }

    [Test]
    [Arguments("<object data=\"evil.swf\"></object>")]
    [Arguments("<embed src=\"evil.swf\">")]
    [Arguments("<form><input type=\"text\"></form>")]
    [Arguments("<iframe src=\"evil.html\"></iframe>")]
    [Arguments("<style>body { background: url(javascript:alert('xss')); }</style>")]
    [Arguments("<link rel=\"stylesheet\" href=\"evil.css\">")]
    public async Task SanitizeHtml_WithDangerousTags_ShouldRemoveTags(string input)
    {
        // Act
        var result = _htmlUtilityService.SanitizeHtml(input);

        // Assert
        await Assert.That(result).DoesNotContain("object");
        await Assert.That(result).DoesNotContain("embed");
        await Assert.That(result).DoesNotContain("form");
        await Assert.That(result).DoesNotContain("input");
        await Assert.That(result).DoesNotContain("iframe");
        await Assert.That(result).DoesNotContain("style");
        await Assert.That(result).DoesNotContain("link");
    }

    [Test]
    public async Task SanitizeHtml_WithMaliciousStyleAttribute_ShouldRemoveStyle()
    {
        // Arrange
        var input = "<div style=\"background: url(javascript:alert('xss'));\">Content</div>";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(input);

        // Assert
        await Assert.That(result).DoesNotContain("javascript:");
        await Assert.That(result).DoesNotContain("alert");
        await Assert.That(result).Contains("Content");
    }

    [Test]
    [Arguments("vbscript:msgbox('xss')")]
    [Arguments("data:text/html,<script>alert('xss')</script>")]
    [Arguments("file:///etc/passwd")]
    public async Task SanitizeHtml_WithDangerousProtocols_ShouldRemoveOrSanitize(string href)
    {
        // Arrange
        var input = $"<a href=\"{href}\">Link</a>";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(input);

        // Assert
        await Assert.That(result).DoesNotContain("vbscript:");
        await Assert.That(result).DoesNotContain("data:");
        await Assert.That(result).DoesNotContain("file:");
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public async Task SanitizeHtml_WithNullOrEmpty_ShouldReturnEmpty(string? input)
    {
        // Act
        var result = _htmlUtilityService.SanitizeHtml(input!);

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task SanitizeHtml_WithValidHtml_ShouldPreserveStructure()
    {
        // Arrange
        var input = "<div><p><strong>Bold</strong> and <em>italic</em> text.</p></div>";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(input);

        // Assert
        await Assert.That(result).Contains("<div>");
        await Assert.That(result).Contains("<p>");
        await Assert.That(result).Contains("<strong>Bold</strong>");
        await Assert.That(result).Contains("<em>italic</em>");
        await Assert.That(result).Contains("</div>");
    }

    #endregion

    #region ContainsHtml Tests

    [Test]
    [Arguments("<p>HTML content</p>", true)]
    [Arguments("<div>Test</div>", true)]
    [Arguments("<br>", true)]
    [Arguments("Plain text", false)]
    [Arguments("Text with < and > symbols", false)]
    [Arguments("", false)]
    [Arguments(null, false)]
    public async Task ContainsHtml_WithVariousInputs_ShouldDetectHtmlCorrectly(string? input, bool expected)
    {
        // Act
        var result = _htmlUtilityService.ContainsHtml(input!);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Test]
    public async Task SanitizeHtml_WithMalformedHtml_ShouldHandleGracefully()
    {
        // Arrange
        var input = "<div><p>Unclosed tags<span>More content";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(input);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("Unclosed tags");
        await Assert.That(result).Contains("More content");
    }

    [Test]
    public async Task ConvertHtmlToText_WithMalformedHtml_ShouldFallbackGracefully()
    {
        // Arrange
        var input = "<div><p>Broken < HTML >< structure";

        // Act
        var result = _htmlUtilityService.ConvertHtmlToText(input);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("Broken");
        await Assert.That(result).Contains("HTML");
        await Assert.That(result).Contains("structure");
    }

    [Test]
    public async Task SanitizeHtml_WithVeryLongInput_ShouldHandleEfficiently()
    {
        // Arrange
        var longContent = new string('A', 10000);
        var input = $"<div>{longContent}</div>";

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = _htmlUtilityService.SanitizeHtml(input);
        stopwatch.Stop();

        // Assert
        await Assert.That(result).Contains(longContent);
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(5000); // Should complete within 5 seconds
    }

    [Test]
    public async Task SanitizeHtml_WithNestedMaliciousContent_ShouldSanitizeAll()
    {
        // Arrange
        var input = @"
            <div>
                <p onclick=""alert('outer')"">
                    Safe text
                    <span onmouseover=""alert('inner')"">
                        <script>alert('nested')</script>
                        More text
                    </span>
                </p>
            </div>";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(input);

        // Assert
        await Assert.That(result).DoesNotContain("onclick");
        await Assert.That(result).DoesNotContain("onmouseover");
        await Assert.That(result).DoesNotContain("script");
        await Assert.That(result).DoesNotContain("alert");
        await Assert.That(result).Contains("Safe text");
        await Assert.That(result).Contains("More text");
    }

    [Test]
    public async Task SanitizeHtml_WithEncodedMaliciousContent_ShouldPreventBypass()
    {
        // Arrange
        var input =
            "<div onclick=\"&#97;&#108;&#101;&#114;&#116;&#40;&#39;&#120;&#115;&#115;&#39;&#41;\">Content</div>";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(input);

        // Assert
        await Assert.That(result).DoesNotContain("onclick");
        await Assert.That(result).Contains("Content");
    }

    [Test]
    [Arguments("<svg onload=\"alert('xss')\"><g/onmouseover=\"alert('xss')\"></svg>")]
    [Arguments("<math><mi//xlink:href=\"data:x,<script>alert('xss')</script>\">")]
    [Arguments("<IMG SRC=javascript:alert('XSS')>")]
    [Arguments("<IMG SRC=JaVaScRiPt:alert('XSS')>")]
    public async Task SanitizeHtml_WithAdvancedXSSAttacks_ShouldPreventExecution(string input)
    {
        // Act
        var result = _htmlUtilityService.SanitizeHtml(input);

        // Assert
        await Assert.That(result).DoesNotContain("javascript:");
        await Assert.That(result).DoesNotContain("alert");
        await Assert.That(result).DoesNotContain("onload");
        await Assert.That(result).DoesNotContain("onmouseover");
    }

    #endregion

    #region Performance Tests

    [Test]
    public async Task StripHtml_WithLargeDocument_ShouldPerformEfficiently()
    {
        // Arrange
        var largeHtml = "<html><body>" +
                        string.Concat(Enumerable.Repeat("<p>Content paragraph with some text.</p>", 1000)) +
                        "</body></html>";

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = _htmlUtilityService.StripHtml(largeHtml);
        stopwatch.Stop();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("Content paragraph");
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(1000); // Should complete within 1 second
    }

    [Test]
    public async Task ConvertHtmlToText_WithComplexStructure_ShouldPerformEfficiently()
    {
        // Arrange
        var complexHtml = string.Concat(Enumerable.Repeat(
            "<div><h1>Title</h1><p>Paragraph</p><ul><li>Item</li></ul><table><tr><td>Cell</td></tr></table></div>",
            100));

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = _htmlUtilityService.ConvertHtmlToText(complexHtml);
        stopwatch.Stop();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("Title");
        await Assert.That(result).Contains("Paragraph");
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(2000); // Should complete within 2 seconds
    }

    #endregion

    #region Additional Security and Coverage Tests

    [Test]
    public async Task SanitizeHtml_WithDeepNestedMaliciousContent_ShouldSanitizeAll()
    {
        // Arrange
        var maliciousHtml = @"
            <div>
                <p>Safe content</p>
                <div onclick=""alert('level1')"">
                    <span onmouseover=""alert('level2')"">
                        <a href=""javascript:alert('level3')"">
                            <script>alert('level4')</script>
                            <img src=""x"" onerror=""alert('level5')"">
                            Nested content
                        </a>
                    </span>
                </div>
            </div>";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(maliciousHtml);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).DoesNotContain("onclick");
        await Assert.That(result).DoesNotContain("onmouseover");
        await Assert.That(result).DoesNotContain("javascript:");
        await Assert.That(result).DoesNotContain("script");
        await Assert.That(result).DoesNotContain("onerror");
        await Assert.That(result).DoesNotContain("alert");
        await Assert.That(result).Contains("Safe content");
        await Assert.That(result).Contains("Nested content");
    }

    [Test]
    [Arguments("<svg><script>alert('xss')</script></svg>")]
    [Arguments("<math><script>alert('xss')</script></math>")]
    [Arguments("<foreignobject><script>alert('xss')</script></foreignobject>")]
    [Arguments("<iframe srcdoc=\"<script>alert('xss')</script>\"></iframe>")]
    public async Task SanitizeHtml_WithSvgAndMathMlVectors_ShouldRemoveScripts(string maliciousInput)
    {
        // Act
        var result = _htmlUtilityService.SanitizeHtml(maliciousInput);

        // Assert
        await Assert.That(result).DoesNotContain("script");
        await Assert.That(result).DoesNotContain("alert");
    }

    [Test]
    public async Task ConvertHtmlToText_WithTableStructure_ShouldFormatCorrectly()
    {
        // Arrange
        var tableHtml = @"
            <table>
                <thead>
                    <tr><th>Name</th><th>Email</th></tr>
                </thead>
                <tbody>
                    <tr><td>John Doe</td><td>john@example.com</td></tr>
                    <tr><td>Jane Smith</td><td>jane@example.com</td></tr>
                </tbody>
            </table>";

        // Act
        var result = _htmlUtilityService.ConvertHtmlToText(tableHtml);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("Name");
        await Assert.That(result).Contains("Email");
        await Assert.That(result).Contains("John Doe");
        await Assert.That(result).Contains("jane@example.com");
    }

    [Test]
    public async Task StripHtml_WithCommentedMaliciousCode_ShouldRemoveComments()
    {
        // Arrange
        var htmlWithComments = @"
            <p>Visible content</p>
            <!-- <script>alert('hidden')</script> -->
            <!--[if IE]><script>alert('ie')</script><![endif]-->
            <p>More content</p>";

        // Act
        var result = _htmlUtilityService.StripHtml(htmlWithComments);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("Visible content");
        await Assert.That(result).Contains("More content");
        await Assert.That(result).DoesNotContain("script");
        await Assert.That(result).DoesNotContain("alert");
        await Assert.That(result).DoesNotContain("<!--");
    }

    [Test]
    public async Task SanitizeHtml_WithDataAttributes_ShouldHandleCorrectly()
    {
        // Arrange
        var htmlWithDataAttrs = @"
            <div data-user-id=""123"" data-action=""delete"">
                <button data-confirm=""Are you sure?"" onclick=""deleteUser()"">Delete</button>
            </div>";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(htmlWithDataAttrs);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).DoesNotContain("onclick");
        await Assert.That(result).DoesNotContain("deleteUser");
        // Data attributes should be preserved as they're generally safe
        await Assert.That(result).Contains("data-");
    }

    [Test]
    public async Task ConvertHtmlToText_WithFormElements_ShouldExtractRelevantText()
    {
        // Arrange
        var formHtml = @"
            <form>
                <label for=""email"">Email Address:</label>
                <input type=""email"" id=""email"" placeholder=""Enter your email"">
                <textarea placeholder=""Your message here""></textarea>
                <select>
                    <option value=""1"">Option 1</option>
                    <option value=""2"">Option 2</option>
                </select>
                <button type=""submit"">Submit</button>
            </form>";

        // Act
        var result = _htmlUtilityService.ConvertHtmlToText(formHtml);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("Email Address:");
        await Assert.That(result).Contains("Submit");
        // Form elements shouldn't include their values/placeholders in basic text conversion
    }

    [Test]
    public async Task SanitizeHtml_WithCssPoisoning_ShouldRemoveMaliciousStyles()
    {
        // Arrange
        var maliciousCss = @"
            <div style=""background: url(javascript:alert('xss'));"">
                <p style=""color: expression(alert('ie-xss'));"">Content</p>
                <span style=""behavior: url(#default#userData);"">Data</span>
            </div>";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(maliciousCss);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).DoesNotContain("javascript:");
        await Assert.That(result).DoesNotContain("expression(");
        await Assert.That(result).DoesNotContain("behavior:");
        await Assert.That(result).DoesNotContain("alert");
        await Assert.That(result).Contains("Content");
        await Assert.That(result).Contains("Data");
    }

    [Test]
    public async Task ContainsHtml_WithBorderlineCases_ShouldDetectCorrectly()
    {
        // Test various borderline cases for HTML detection
        var testCases = new[]
        {
            ("This is < 5 and > 3", false), // Math comparison, not HTML
            ("Email: user@domain.com", false), // Email address
            ("C++ template<int>", false), // Programming syntax
            ("XML: <?xml version='1.0'?>", true), // XML is HTML-like
            ("HTML: <br/>", true), // Self-closing tag
            ("Angle brackets: < >", false), // Just brackets
            ("Tag-like: <notarealtag>", true) // Unknown tag is still HTML-like
        };

        foreach (var (input, expected) in testCases)
        {
            // Act
            var result = _htmlUtilityService.ContainsHtml(input);

            // Assert
            await Assert.That(result).IsEqualTo(expected);
        }
    }

    [Test]
    public async Task SanitizeHtml_WithInternationalContent_ShouldPreserveCorrectly()
    {
        // Arrange
        var internationalHtml = @"
            <div>
                <p>English: Hello World</p>
                <p>中文: 你好世界</p>
                <p>العربية: مرحبا بالعالم</p>
                <p>Русский: Привет мир</p>
                <p>Emoji: 🌍🌎🌏</p>
            </div>";

        // Act
        var result = _htmlUtilityService.SanitizeHtml(internationalHtml);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).Contains("Hello World");
        await Assert.That(result).Contains("你好世界");
        await Assert.That(result).Contains("مرحبا بالعالم");
        await Assert.That(result).Contains("Привет мир");
        await Assert.That(result).Contains("🌍🌎🌏");
    }

    [Test]
    public async Task ConvertHtmlToText_WithLargeDocument_ShouldHandleEfficiently()
    {
        // Arrange - Create a large HTML document
        var largeHtml = "<html><body>";
        for (var i = 0; i < 1000; i++)
            largeHtml +=
                $"<div><h2>Section {i}</h2><p>Content for section {i} with some detailed text that includes more comprehensive information about this particular section. This ensures we have enough text to exceed the 10,000 character threshold for testing purposes.</p></div>";
        largeHtml += "</body></html>";

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = _htmlUtilityService.ConvertHtmlToText(largeHtml);
        stopwatch.Stop();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Length).IsGreaterThan(10000);
        await Assert.That(result).Contains("Section 0");
        await Assert.That(result).Contains("Section 999");
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(5000); // Should complete within 5 seconds
    }

    [Test]
    public void Debug_HtmlConversion()
    {
        // Debug test - let's examine the simple <br> case step by step
        var testInput = "<br>Line<br>Break";
        var expectedOutput = "Line\nBreak";

        Console.WriteLine($"Debug: Testing '{testInput}'");
        Console.WriteLine($"Expected: '{expectedOutput.Replace("\n", "\\n")}'");

        // Create HtmlUtilityService to debug internal state
        var logger = Substitute.For<ILogger<HtmlUtilityService>>();
        var service = new HtmlUtilityService(logger);

        // Let's manually trace what should happen
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(testInput);

        Console.WriteLine($"Original HTML: {doc.DocumentNode.OuterHtml}");
        Console.WriteLine($"Original InnerText: '{doc.DocumentNode.InnerText.Replace("\n", "\\n")}'");

        // Now let's see what happens after BR processing
        var brNodes = doc.DocumentNode.SelectNodes("//br");
        if (brNodes != null)
        {
            Console.WriteLine($"Found {brNodes.Count} BR nodes");
            foreach (var br in brNodes)
            {
                var newlineNode = HtmlAgilityPack.HtmlNode.CreateNode("\n");
                br.ParentNode.ReplaceChild(newlineNode, br);
            }
        }

        Console.WriteLine($"After BR replacement HTML: {doc.DocumentNode.OuterHtml}");
        Console.WriteLine($"After BR replacement InnerText: '{doc.DocumentNode.InnerText.Replace("\n", "\\n")}'");

        // Test the actual service
        var actualResult = service.ConvertHtmlToText(testInput, false);
        Console.WriteLine($"Service result: '{actualResult.Replace("\n", "\\n")}'");
        Console.WriteLine($"Match: {actualResult == expectedOutput}");

        // Always pass - this is just for debugging
    }

    #endregion
}