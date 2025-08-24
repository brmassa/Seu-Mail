# Seu Mail 📧

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-blue.svg)](https://blazor.net/)

A modern, feature-rich email client built with C# and Blazor Server. Part of the **Seu** (Yours) project - empowering
users with privacy-focused, self-hosted communication tools.

## ✨ Features

### 📬 Email Management

- **Multi-Protocol Support**: SMTP, IMAP, and POP3 connectivity
- **Multiple Account Management**: Add and manage unlimited email accounts
- **Unified Inbox**: View emails from all accounts in one place
- **Smart Email Organization**: Inbox, Sent, Drafts, Trash, and custom folders
- **Advanced Search**: Find emails quickly with powerful search functionality
- **Email Threading**: Conversation view for related messages

### 🎨 Modern Interface

- **Gmail-inspired UI**: Clean, intuitive interface familiar to users
- **Responsive Design**: Works seamlessly on desktop, tablet, and mobile
- **Multiple Layout Modes**: Split-screen, separate page, or bottom pane views
- **Customizable Views**: Compact mode, email previews, and display preferences
- **Dark/Light Themes**: Choose your preferred appearance

### 📝 Composition & Editing

- **Rich Text Editor**: HTML email composition with formatting options
- **File Attachments**: Support for multiple file attachments (up to 25MB)
- **Draft Management**: Auto-save drafts and resume editing
- **Email Templates**: Reusable templates for common messages
- **Signature Support**: Custom signatures for each account

### 📅 Calendar Integration

- **Event Management**: Create, edit, and manage calendar events
- **Multiple Calendars**: Support for multiple calendar accounts
- **Event Reminders**: Customizable notification system
- **Meeting Invitations**: Send and receive calendar invites
- **Recurring Events**: Support for repeating events with flexible rules

### 🔒 Security & Privacy

- **End-to-End Security**: SSL/TLS encryption for all connections
- **Local Data Storage**: All data stored locally using SQLite
- **Password Encryption**: Secure storage of account credentials
- **Privacy-First**: No data shared with third parties
- **Self-Hosted**: Complete control over your data

### 🎛️ Advanced Features

- **Email Provider Detection**: Automatic configuration for popular providers
- **DNS Autodiscovery**: Automatic server settings detection
- **Folder Synchronization**: Two-way sync with email servers
- **Offline Support**: Read and compose emails offline
- **Import/Export**: Backup and restore email data

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022, VS Code, or any .NET-compatible IDE (optional)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/brmassa/Seu-Mail.git
   cd Seu-Mail
   ```

2. **Run the web application**
   ```bash
   dotnet run --project Seu.Mail.Web
   ```

3. **Open in browser**
    - Navigate to `https://localhost:5000` or the URL shown in the console
    - The application will create the database automatically on first run

## 📱 Usage

### Adding Your First Email Account

1. Launch Seu Mail and click **"Add Account"**
2. Enter your email credentials:
    - **Display Name**: How you want to appear to recipients
    - **Email Address**: Your email address
    - **Password**: Your email password (use app passwords for 2FA-enabled accounts)
3. Use **Quick Setup** for popular providers or configure manually
4. Test the connection and save

### Supported Email Providers

| Provider | IMAP Server           | SMTP Server           | Port    | Security |
|----------|-----------------------|-----------------------|---------|----------|
| Gmail    | imap.gmail.com        | smtp.gmail.com        | 993/587 | SSL/TLS  |
| Outlook  | outlook.office365.com | smtp-mail.outlook.com | 993/587 | SSL/TLS  |
| Yahoo    | imap.mail.yahoo.com   | smtp.mail.yahoo.com   | 993/587 | SSL/TLS  |

## ⚙️ Configuration

### Application Settings

Customize behavior in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Seu.Mail.db"
  },
  "EmailSettings": {
    "MaxAttachmentSize": 26214400,
    "DefaultSyncInterval": 300,
    "MaxEmailsPerSync": 100,
    "EnableAutoSync": true,
    "DefaultPageSize": 50
  },
  "Security": {
    "EncryptPasswords": true,
    "RequireHttps": false
  },
  "Calendar": {
    "DefaultReminderMinutes": 15,
    "MaxEventsPerDay": 100
  }
}
```

### User Preferences

Customize your experience through the Settings page:

- **Layout Mode**: Choose between split-screen or separate page views
- **Email Display**: Configure sender display, previews, and compact mode
- **Sync Settings**: Automatic sync intervals and email limits
- **Notifications**: Email and calendar notification preferences


### Key Technologies

- **Architecture**: Clean Architecture with separated concerns
- **Frontend**: Blazor Server, Bootstrap 5, Font Awesome
- **Backend**: ASP.NET Core 9.0, Entity Framework Core
- **Database**: SQLite (local storage)
- **Email**: MailKit/MimeKit for SMTP/IMAP operations
- **Calendar**: Ical.Net for iCalendar support
- **Security**: Built-in encryption and secure communication
- **Testing**: TUnit testing framework
- **Modularity**: Pluggable calendar and future contact modules

## 🧪 Development

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Seu.Mail.Tests.Unit
dotnet test tests/Seu.Mail.Tests.Integration
```

### Database Migrations

When modifying data models in `Seu.Mail.Data`:

```bash
# Navigate to the data project
cd Seu.Mail.Data

# Add migration
dotnet ef migrations add YourMigrationName

# Update database
dotnet ef database update

# Or from solution root
dotnet ef migrations add YourMigrationName --project Seu.Mail.Data --startup-project Seu.Mail.Web
```

### Building for Production

```bash
# Build entire solution
dotnet build -c Release

# Publish web application
dotnet publish Seu.Mail.Web -c Release -o ./publish

# Build specific project
dotnet build Seu.Mail.Services -c Release
```

### Development Guidelines

- Follow C# coding standards and conventions
- Write unit tests for new features
- Use dependency injection for services
- Implement proper error handling and logging
- Ensure responsive design principles

## 🐛 Troubleshooting

### Common Issues

**Connection Failed**

- Verify server settings and credentials
- Check firewall and network connectivity
- Ensure SSL/TLS is properly configured

**Gmail Authentication Issues**

- Enable 2-Factor Authentication
- Use App Password instead of account password
- Check Google Account security settings

**Database Errors**

- Ensure write permissions in application directory
- Delete `Seu.Mail.db` to reset (will lose data)
- Check disk space availability

**Performance Issues**

- Reduce sync frequency in settings
- Limit emails per sync operation
- Clear browser cache and restart application

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details.

### Quick Contribution Guide

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit changes: `git commit -m 'Add amazing feature'`
4. Push to branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [MailKit](https://github.com/jstedfast/MailKit) - Excellent .NET email library
- [MimeKit](https://github.com/jstedfast/MimeKit) - MIME message parsing
- [Blazor](https://blazor.net/) - Modern web UI framework
- The open-source community for inspiration and tools

---

**Part of the Seu Project** - Building privacy-focused, user-owned communication tools.
