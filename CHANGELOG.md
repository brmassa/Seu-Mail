# Changelog

All notable changes to Seu Mail will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned
- Email encryption (PGP/GPG support)
- Advanced email rules and filters
- Contact management system
- Email scheduling
- Enhanced mobile experience
- Themes and customization
- Plugin system
- Import from other email clients
- Email analytics and insights
- Multi-language support

## [1.2.0] - 2024-12-XX

### Added
- Calendar integration with event management
- Multiple calendar support
- Event reminders and notifications
- Meeting invitation handling
- Recurring events with flexible rules
- Calendar settings and preferences
- Enhanced email threading and conversation view
- Advanced search functionality with filters
- Email provider auto-detection service
- DNS-based email server discovery
- Improved attachment handling (up to 25MB)
- Email templates support
- Custom email signatures
- Multiple layout modes (split-screen, separate page, bottom pane)
- Customizable email display preferences
- Compact mode for email lists
- Email preview functionality
- Mark as read on open option
- Unified inbox for multiple accounts

### Changed
- Upgraded to .NET 9.0
- Improved user interface with better responsiveness
- Enhanced email synchronization performance
- Better error handling and user feedback
- Optimized database queries for faster loading
- Updated dependencies to latest versions

### Fixed
- Email attachment download issues
- Database migration problems
- Memory leaks in email synchronization
- UI freezing during large email imports
- Calendar event timezone handling
- Email HTML rendering improvements
- Various security vulnerabilities

### Security
- Implemented password encryption for stored credentials
- Enhanced input validation across all forms
- Added CSRF protection to all state-changing operations
- Improved SSL/TLS certificate validation
- Enhanced HTML sanitization for email content

## [1.1.0] - 2024-10-15

### Added
- Multiple email account support
- Gmail-like interface with modern design
- Email composition with rich text editor
- Attachment support for sending and receiving
- Folder management (Inbox, Sent, Drafts, Trash)
- Email search functionality
- Import/export capabilities
- Offline email reading
- Auto-save for email drafts
- Email threading support
- Mobile-responsive design
- Bootstrap 5 integration
- Font Awesome icons

### Changed
- Improved email synchronization speed
- Better error messages and user feedback
- Enhanced database schema for better performance
- Updated UI components for better accessibility

### Fixed
- IMAP connection timeout issues
- Email parsing errors with special characters
- Database locking problems during sync
- UI rendering issues on mobile devices
- Memory usage optimization

### Security
- Added rate limiting for authentication attempts
- Implemented secure session management
- Enhanced email content sanitization
- SSL/TLS encryption for all server communications

## [1.0.0] - 2024-08-20

### Added
- Initial release of Seu Mail
- Basic SMTP email sending functionality
- IMAP email receiving and synchronization
- Single email account support
- Basic email management (read, delete, move)
- SQLite database for local storage
- Simple web-based interface
- Email folder organization
- Basic email search
- Attachment viewing
- Email composition with plain text
- Account configuration interface
- Automatic database schema creation
- Basic error handling and logging

### Security
- SSL/TLS support for email connections
- Basic input validation
- Secure password storage
- Local data storage (no cloud dependencies)

## [0.9.0-beta] - 2024-07-10

### Added
- Beta release for testing
- Core email functionality
- Basic Blazor Server interface
- SQLite integration
- MailKit/MimeKit integration
- Initial project structure

### Known Issues
- Limited error handling
- No mobile optimization
- Basic UI design
- Single account limitation
- No calendar integration

---

## Version History Summary

- **v1.2.0** - Major update with calendar integration and advanced features
- **v1.1.0** - Multiple accounts, modern UI, and enhanced functionality
- **v1.0.0** - Initial stable release with core email features
- **v0.9.0-beta** - Beta release for early testing

## Contributing

When contributing to this project, please update this changelog following the [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) format:

- Use `Added` for new features
- Use `Changed` for changes in existing functionality
- Use `Deprecated` for soon-to-be removed features
- Use `Removed` for now removed features
- Use `Fixed` for any bug fixes
- Use `Security` for security-related changes

## Links

- [Repository](https://github.com/brmassa/Seu-Mail)
- [Issues](https://github.com/brmassa/Seu-Mail/issues)
- [Releases](https://github.com/brmassa/Seu-Mail/releases)