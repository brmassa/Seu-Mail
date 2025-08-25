# Seu.Mail.Search

A comprehensive Gmail-like email search engine for the Seu Mail application. This independent C# library provides advanced search capabilities with powerful query parsing, filtering, and ranking features.

## Features

### 🔍 Gmail-Like Search Syntax
- **Keyword Search**: Simple text matching across subject, body, and participants
- **Quoted Phrases**: Exact phrase matching with `"project update"`
- **Sender/Recipient Filters**: `from:alice@example.com`, `to:bob@example.com`
- **Date Ranges**: `before:2024-01-15`, `after:2023-12-01`, `on:2024-01-01`
- **Size Filters**: `larger:10MB`, `smaller:500KB`
- **Status Filters**: `is:read`, `is:unread`, `is:starred`, `is:important`
- **Attachment Filters**: `has:attachment`, `filename:report.pdf`, `filetype:pdf`
- **Folder Scoping**: `in:inbox`, `in:sent`, `in:drafts`
- **Relative Dates**: `older:7d`, `newer:1w`, `older:2months`

### 🎯 Advanced Filtering
- Content search across subject, body, HTML, and plain text
- Participant matching (from, to, cc, bcc)
- Date range filtering with flexible date formats
- File size filtering with human-readable units (KB, MB, GB)
- Folder and scope-based filtering
- Attachment name and type filtering
- Priority and importance filtering
- Custom label and tag filtering

### 📊 Smart Ranking & Sorting
- **Relevance Scoring**: Intelligent scoring based on keyword matches
- **Multiple Sort Options**: Date, sender, subject, size, relevance
- **Match Boosting**: Higher scores for subject matches, recent emails, important messages
- **Pagination Support**: Efficient result pagination with customizable page sizes

### ⚡ Performance Features
- **Efficient Filtering**: Optimized LINQ queries with proper indexing support
- **Configurable Limits**: Customizable result limits and pagination
- **Error Handling**: Graceful error handling with detailed logging
- **Async Operations**: Full async/await support for non-blocking operations

## Quick Start

### Installation

Add the search project to your solution:

```xml
<ProjectReference Include="Path\To\Seu.Mail.Search\Seu.Mail.Search.csproj" />
```

### Basic Usage

```csharp
// Register services
services.AddEmailSearch(options =>
{
    options.DefaultMaxResults = 50;
    options.EnableRelevanceScoring = true;
    options.EnableSearchSuggestions = true;
});

// Use the search service
var searchService = serviceProvider.GetRequiredService<EmailSearchService>();

// Simple keyword search
var results = await searchService.SearchAsync("project meeting");

// Advanced Gmail-like queries
var complexResults = await searchService.SearchAsync(
    "from:alice@example.com has:attachment larger:1MB before:2024-01-15"
);
```

## Search Query Examples

### Basic Searches
```
hello world                    # Keywords in subject or body
"project update meeting"       # Exact phrase matching
```

### Sender & Recipients
```
from:alice@example.com         # Emails from Alice
to:bob@company.com            # Emails to Bob  
cc:manager@company.com        # Emails with manager in CC
```

### Date Filtering
```
before:2024-01-15             # Before specific date
after:2023-12-01              # After specific date
on:2024-01-01                 # On specific date
older:7d                      # Older than 7 days
newer:1w                      # Newer than 1 week
older:2months                 # Older than 2 months
```

### Size & Attachments
```
larger:10MB                   # Larger than 10 megabytes
smaller:500KB                 # Smaller than 500 kilobytes
has:attachment                # Has any attachments
filename:report.pdf           # Attachment named "report.pdf"
filetype:pdf                  # PDF attachments
```

### Status & Folders
```
is:read                       # Read messages
is:unread                     # Unread messages
is:starred                    # Starred/important messages
is:draft                      # Draft messages
in:inbox                      # In inbox folder
in:sent                       # In sent folder
folder:CUSTOM                 # In custom folder
```

### Complex Queries
```
# Unread emails from Alice with attachments in the last week
from:alice@example.com is:unread has:attachment newer:1w

# Large emails about projects, excluding drafts
"project" larger:5MB -is:draft

# Important emails with PDF attachments from specific domain
is:important filetype:pdf from:@company.com
```

## API Reference

### EmailSearchService

Main search service class providing comprehensive email search functionality.

#### Methods

```csharp
// Search with query string
Task<EmailSearchResult> SearchAsync(string query, CancellationToken cancellationToken = default)

// Search with structured criteria
Task<EmailSearchResult> SearchAsync(EmailSearchCriteria criteria, CancellationToken cancellationToken = default)

// Get search suggestions
Task<List<string>> GetSearchSuggestionsAsync(string partialQuery, CancellationToken cancellationToken = default)
```

### EmailSearchCriteria

Structured search criteria for programmatic search construction.

```csharp
public class EmailSearchCriteria
{
    public string? Keyword { get; set; }              // General keyword search
    public string? ExactPhrase { get; set; }          // Exact phrase matching
    public string? From { get; set; }                 // Sender filter
    public string? To { get; set; }                   // Recipient filter
    public DateTime? Before { get; set; }             // Date before
    public DateTime? After { get; set; }              // Date after
    public long? LargerThanBytes { get; set; }        // Size filter
    public bool? HasAttachment { get; set; }          // Attachment filter
    public SearchScope Scope { get; set; }            // Search scope
    public int MaxResults { get; set; } = 50;         // Result limit
    // ... and many more properties
}
```

### EmailSearchResult

Search result container with metadata and pagination info.

```csharp
public class EmailSearchResult
{
    public List<EmailMessage> Emails { get; set; }    // Found emails
    public int TotalCount { get; set; }               // Total matches
    public EmailSearchCriteria SearchCriteria { get; set; } // Used criteria
    public bool IsSuccess { get; set; }               // Success status
    public bool HasMore { get; set; }                 // More results available
    public int CurrentPage { get; set; }              // Current page
    public int TotalPages { get; set; }               // Total pages
}
```

## Configuration Options

Configure the search service with `EmailSearchOptions`:

```csharp
services.AddEmailSearch(options =>
{
    options.DefaultMaxResults = 50;           // Default result limit
    options.MaxAllowedResults = 1000;         // Maximum allowed results
    options.EnableRelevanceScoring = true;    // Enable smart ranking
    options.EnableSearchSuggestions = true;   // Enable autocomplete
    options.MinimumQueryLength = 1;           // Minimum query length
    options.SearchTimeoutSeconds = 30;        // Search timeout
    options.CaseSensitiveSearch = false;      // Case sensitivity
    options.IncludeDeletedEmails = false;     // Include deleted emails
    options.IncludeSpamEmails = false;        // Include spam emails
});
```

## Search Performance

### Optimization Tips

1. **Use Specific Filters**: Narrow searches with date ranges, folders, or senders
2. **Leverage Indexing**: Ensure database indexes on frequently searched fields
3. **Pagination**: Use reasonable page sizes (25-100 results)
4. **Caching**: Enable result caching for frequently repeated searches

### Database Indexes

Recommended indexes for optimal performance:

```sql
-- Compound index for common search patterns
CREATE INDEX IX_EmailMessages_AccountId_DateReceived ON EmailMessages(AccountId, DateReceived DESC);
CREATE INDEX IX_EmailMessages_From ON EmailMessages(From);
CREATE INDEX IX_EmailMessages_Folder_IsDeleted ON EmailMessages(Folder, IsDeleted);

-- Full-text search indexes (if supported)
CREATE FULLTEXT INDEX IX_EmailMessages_Content ON EmailMessages(Subject, TextBody, HtmlBody);
```

## Testing

The library includes comprehensive test coverage:

```bash
# Run all search tests
dotnet test Seu.Mail.Search.Tests

# Run with coverage
dotnet test Seu.Mail.Search.Tests --collect:"XPlat Code Coverage"
```

### Test Categories

- **Query Parser Tests**: Verify Gmail-like syntax parsing
- **Search Service Tests**: Test filtering and ranking logic  
- **Integration Tests**: End-to-end search scenarios
- **Performance Tests**: Validate search performance with large datasets

## Architecture

### Components

```
EmailSearchService
├── EmailSearchQueryParser    # Parses Gmail-like queries
├── IEmailRepository         # Data access abstraction
└── EmailSearchOptions       # Configuration options
```

### Data Flow

1. **Query Input** → EmailSearchQueryParser
2. **Parsed Criteria** → EmailSearchService  
3. **Filtered Results** → Ranking & Sorting
4. **Paginated Output** → EmailSearchResult

## Examples

### Blazor Integration

```csharp
@inject EmailSearchService SearchService

<input @bind="searchQuery" @onkeypress="OnSearchKeyPress" />

@code {
    private string searchQuery = "";
    private List<EmailMessage> results = new();
    
    private async Task OnSearchKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(searchQuery))
        {
            var searchResult = await SearchService.SearchAsync(searchQuery);
            results = searchResult.Emails;
        }
    }
}
```

### API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly EmailSearchService _searchService;
    
    [HttpGet]
    public async Task<ActionResult<EmailSearchResult>> Search(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var criteria = new EmailSearchCriteria 
        { 
            Keyword = q,
            MaxResults = pageSize,
            Offset = (page - 1) * pageSize
        };
        
        var result = await _searchService.SearchAsync(criteria);
        return Ok(result);
    }
}
```

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/search-enhancement`
3. Add tests for new functionality
4. Ensure all tests pass: `dotnet test`
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.