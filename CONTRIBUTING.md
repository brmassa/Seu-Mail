# Contributing to Seu Mail 🤝

Thank you for your interest in contributing to Seu Mail! This document provides guidelines and information for
contributors.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Submission Process](#submission-process)
- [Issue Reporting](#issue-reporting)
- [Feature Requests](#feature-requests)
- [Documentation](#documentation)

## 📜 Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment. We welcome contributors
from all backgrounds and experience levels.

### Our Standards

- **Be respectful** and considerate in all interactions
- **Be constructive** when providing feedback
- **Be patient** with new contributors
- **Be collaborative** and help others learn
- **Focus on what's best** for the community and project

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/)
- A code editor (Visual Studio 2022, VS Code, or JetBrains Rider recommended)
- Basic knowledge of C#, Blazor, and web development

### First-Time Contributors

1. **Star the repository** ⭐ to show your support
2. **Fork the repository** to your GitHub account
3. **Clone your fork** locally
4. **Set up the development environment**
5. **Pick an issue** labeled `good first issue` or `help wanted`

## 🛠️ Development Setup

### 1. Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/Seu-Mail.git
cd Seu-Mail
```

### 2. Set Up Upstream Remote

```bash
git remote add upstream https://github.com/brmassa/Seu-Mail.git
```

### 3. Install Dependencies

```bash
dotnet restore
```

### 4. Run the Application

```bash
cd Mail
dotnet run
```

### 5. Run Tests

```bash
dotnet test
```

## 🤲 How to Contribute

### Types of Contributions

We welcome various types of contributions:

- 🐛 **Bug fixes**
- ✨ **New features**
- 📚 **Documentation improvements**
- 🎨 **UI/UX enhancements**
- ⚡ **Performance optimizations**
- 🧪 **Test coverage improvements**
- 🌍 **Translations**

### Contribution Workflow

1. **Check existing issues** - Look for existing issues or create a new one
2. **Fork and branch** - Create a feature branch from `main`
3. **Make changes** - Implement your changes following coding standards
4. **Test thoroughly** - Ensure all tests pass and add new tests if needed
5. **Commit changes** - Use clear, descriptive commit messages
6. **Submit PR** - Create a pull request with detailed description

### Branch Naming Convention

Use descriptive branch names that indicate the type of change:

```
feature/add-calendar-integration
bugfix/fix-email-attachment-issue
docs/improve-readme
refactor/optimize-email-service
hotfix/critical-security-patch
```

## 📝 Coding Standards

### C# Coding Conventions

Follow
Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions):

```csharp
// ✅ Good
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    
    public async Task<List<EmailMessage>> GetEmailsAsync(
        EmailAccount account, 
        string folderName, 
        int count = 50)
    {
        // Implementation
    }
}

// ❌ Avoid
public class emailservice : IEmailService 
{
    private ILogger<EmailService> logger;
    
    public async Task<List<EmailMessage>> getEmails(EmailAccount account, string folderName, int count = 50)
    {
        // Implementation
    }
}
```

### Code Style Guidelines

- **Use PascalCase** for public members, methods, and classes
- **Use camelCase** for private fields and local variables
- **Use underscore prefix** for private fields (`_fieldName`)
- **Use descriptive names** for variables and methods
- **Keep methods focused** - Single responsibility principle
- **Add XML documentation** for public APIs
- **Use async/await** for asynchronous operations
- **Handle exceptions** appropriately with try-catch blocks

### Blazor Component Guidelines

```razor
@* ✅ Good component structure *@
@page "/example"
@using Seu.Mail.Services
@inject IEmailService EmailService

<PageTitle>Example - Seu Mail</PageTitle>

<div class="container">
    <h2>Example Component</h2>
    @if (isLoading)
    {
        <div class="spinner-border" role="status">
            <span class="visually-hidden">Loading...</span>
        </div>
    }
    else
    {
        <div class="content">
            <!-- Component content -->
        </div>
    }
</div>

@code {
    private bool isLoading = true;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            // Load data
        }
        catch (Exception ex)
        {
            // Handle error
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

## 🧪 Testing Guidelines

### Writing Tests

- **Write unit tests** for all new functionality
- **Use TUnit** testing framework (project standard)
- **Follow AAA pattern** - Arrange, Act, Assert
- **Use descriptive test names** that explain what is being tested
- **Mock dependencies** using interfaces
- **Test both positive and negative scenarios**

### Test Example

```csharp
[Test]
public async Task GetEmailsAsync_WithValidAccount_ReturnsEmailList()
{
    // Arrange
    var mockContext = new Mock<EmailDbContext>();
    var emailService = new EmailService(mockContext.Object);
    var account = new EmailAccount { Id = 1, Email = "test@example.com" };

    // Act
    var result = await emailService.GetEmailsAsync(account, "INBOX", 10);

    // Assert
    Assert.NotNull(result);
    Assert.IsInstanceOf<List<EmailMessage>>(result);
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "GetEmailsAsync_WithValidAccount_ReturnsEmailList"
```

## 📤 Submission Process

### Before Submitting

- [ ] Code follows project conventions
- [ ] All tests pass
- [ ] New functionality has tests
- [ ] Documentation is updated
- [ ] No merge conflicts with main branch
- [ ] Commit messages are clear and descriptive

### Pull Request Guidelines

1. **Create a descriptive title** that summarizes the change
2. **Provide detailed description** explaining:
    - What problem does this solve?
    - How does it solve it?
    - Any breaking changes?
    - Screenshots (if UI changes)
3. **Link related issues** using keywords (fixes #123, closes #456)
4. **Request appropriate reviewers**
5. **Ensure CI checks pass**

### Pull Request Template

```markdown
## Description
Brief description of changes made.

## Type of Change
- [ ] Bug fix (non-breaking change fixing an issue)
- [ ] New feature (non-breaking change adding functionality)
- [ ] Breaking change (fix or feature causing existing functionality to not work)
- [ ] Documentation update

## Testing
- [ ] Unit tests pass
- [ ] Manual testing completed
- [ ] New tests added for new functionality

## Screenshots (if applicable)
Include screenshots of UI changes.

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review of code completed
- [ ] Documentation updated
- [ ] No merge conflicts
```

## 🐛 Issue Reporting

### Before Creating an Issue

1. **Search existing issues** to avoid duplicates
2. **Check documentation** and troubleshooting guides
3. **Test with latest version** if possible

### Bug Report Template

```markdown
**Describe the bug**
A clear description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior:
1. Go to '...'
2. Click on '...'
3. Scroll down to '...'
4. See error

**Expected behavior**
A clear description of what you expected to happen.

**Screenshots**
If applicable, add screenshots to help explain your problem.

**Environment:**
- OS: [e.g. Windows 10, macOS, Linux]
- Browser: [e.g. Chrome, Firefox, Safari]
- .NET Version: [e.g. .NET 9.0]
- Seu Mail Version: [e.g. 1.2.0]

**Additional context**
Add any other context about the problem here.
```

## 💡 Feature Requests

We welcome feature requests! Please provide:

- **Clear description** of the proposed feature
- **Use cases** explaining why it would be valuable
- **Possible implementation** ideas (if you have any)
- **Alternatives considered** and why this approach is preferred

### Feature Request Template

```markdown
**Is your feature request related to a problem?**
A clear description of what the problem is.

**Describe the solution you'd like**
A clear description of what you want to happen.

**Describe alternatives you've considered**
A clear description of any alternative solutions or features you've considered.

**Additional context**
Add any other context or screenshots about the feature request here.
```

## 📚 Documentation

### Documentation Standards

- **Write clear, concise documentation**
- **Include code examples** where appropriate
- **Update documentation** when making changes
- **Use proper markdown formatting**
- **Include screenshots** for UI-related documentation

### Areas Needing Documentation

- API documentation
- Configuration options
- Troubleshooting guides
- User guides
- Developer guides
- Architecture documentation

## 🏷️ Issue Labels

We use labels to categorize issues and pull requests:

- `bug` - Something isn't working
- `enhancement` - New feature or request
- `documentation` - Improvements or additions to documentation
- `good first issue` - Good for newcomers
- `help wanted` - Extra attention is needed
- `question` - Further information is requested
- `wontfix` - This will not be worked on
- `duplicate` - This issue or pull request already exists
- `priority: high` - High priority issue
- `priority: medium` - Medium priority issue
- `priority: low` - Low priority issue

## 🎯 Development Priorities

### Current Focus Areas

1. **Core Email Functionality** - SMTP/IMAP improvements
2. **Calendar Integration** - Enhanced calendar features
3. **User Experience** - UI/UX improvements
4. **Performance** - Optimization and responsiveness
5. **Security** - Enhanced security features
6. **Testing** - Improved test coverage

### Long-term Goals

- Email encryption (PGP/GPG)
- Advanced email rules and filters
- Mobile app companion
- Plugin system
- Multi-language support

## 📞 Getting Help

### Communication Channels

- **GitHub Issues** - Bug reports and feature requests
- **GitHub Discussions** - General questions and community discussion
- **Pull Request Reviews** - Code review and feedback

### Response Times

- We aim to respond to issues within 48 hours
- Pull requests typically reviewed within 1 week
- Complex features may take longer to review

## 🙏 Recognition

### Contributors

All contributors are recognized in our README and release notes. We appreciate every contribution, no matter how small!

### Ways to Contribute Without Code

- **Report bugs** and test new features
- **Improve documentation** and write tutorials
- **Answer questions** in issues and discussions
- **Spread the word** about Seu Mail
- **Provide feedback** on new features
- **Design assets** and UI improvements

---

## Thank You! 🎉

Thank you for contributing to Seu Mail! Your efforts help make email communication more private, secure, and
user-friendly for everyone.

**Questions?** Don't hesitate to ask! We're here to help new contributors get started.

---

*This document is a living guide and will be updated as the project evolves.*