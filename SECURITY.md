# Security Policy

## Supported Versions

We take security seriously and provide security updates for the following versions of Seu Mail:

| Version | Supported          |
|---------|--------------------|
| 1.2.x   | :white_check_mark: |
| 1.1.x   | :white_check_mark: |
| 1.0.x   | :x:                |
| < 1.0   | :x:                |

## Security Features

Seu Mail implements several security measures to protect your data:

### Data Protection

- **Local Storage**: All email data is stored locally using SQLite
- **Password Encryption**: Account passwords are encrypted using industry-standard methods
- **No Data Sharing**: No user data is transmitted to third parties
- **Self-Hosted**: Complete control over your email data

### Communication Security

- **SSL/TLS Encryption**: All email server connections use encrypted protocols
- **Secure Authentication**: Support for modern authentication methods
- **Certificate Validation**: Proper SSL certificate verification
- **Secure Protocols**: SMTP over SSL/TLS, IMAP over SSL

### Application Security

- **Input Validation**: All user inputs are validated and sanitized
- **SQL Injection Prevention**: Parameterized queries and ORM protection
- **XSS Protection**: Proper HTML encoding and content sanitization
- **CSRF Protection**: Built-in ASP.NET Core CSRF protection

## Reporting a Vulnerability

If you discover a security vulnerability in Seu Mail, please report it responsibly:

### How to Report

1. **Do NOT** create a public GitHub issue for security vulnerabilities
2. Email security reports to: **security@seumail.project** (replace with actual email when available)
3. Include detailed information about the vulnerability:
    - Description of the issue
    - Steps to reproduce
    - Potential impact
    - Any suggested fixes

### What to Include

Please provide as much information as possible:

```
- Affected version(s)
- Component or feature affected
- Attack vector and severity
- Proof of concept (if available)
- Suggested remediation
```

### Response Timeline

We are committed to addressing security issues promptly:

- **Initial Response**: Within 48 hours of report
- **Assessment**: Within 1 week of report
- **Fix Development**: Based on severity (critical issues prioritized)
- **Release**: Security patches released as soon as possible

### Severity Guidelines

We classify vulnerabilities using the following criteria:

#### Critical (24-48 hour response)

- Remote code execution
- Authentication bypass
- Data exposure of all users
- Complete system compromise

#### High (1 week response)

- Privilege escalation
- Significant data exposure
- Authentication issues
- Major denial of service

#### Medium (2 weeks response)

- Limited data exposure
- Moderate denial of service
- Cross-site scripting (XSS)
- Information disclosure

#### Low (1 month response)

- Minor information disclosure
- Limited denial of service
- Configuration issues

## Security Best Practices

### For Users

**Account Security**:

- Use strong, unique passwords for email accounts
- Enable two-factor authentication when available
- Use app passwords for 2FA-enabled accounts
- Regularly review connected accounts

**Installation Security**:

- Download only from official sources
- Keep Seu Mail updated to the latest version
- Use HTTPS when accessing the web interface
- Secure your local database file

**Email Security**:

- Be cautious with email attachments
- Verify sender authenticity for sensitive emails
- Use encrypted email when handling sensitive information
- Regularly backup your email data

### For Developers

**Code Security**:

- Follow secure coding practices
- Validate and sanitize all inputs
- Use parameterized queries
- Implement proper error handling

**Dependency Management**:

- Keep dependencies updated
- Monitor for security advisories
- Use tools like `dotnet list package --vulnerable`
- Review third-party package security

**Testing**:

- Include security testing in CI/CD
- Test authentication and authorization
- Validate input sanitization
- Test for common vulnerabilities

## Security Audit History

### Version 1.2.0

- Implemented password encryption for stored credentials
- Enhanced input validation across all forms
- Added CSRF protection to all state-changing operations
- Updated dependencies to address known vulnerabilities

### Version 1.1.0

- Improved SSL/TLS certificate validation
- Enhanced HTML sanitization for email content
- Added rate limiting for authentication attempts
- Implemented secure session management

## Vulnerability Disclosure Policy

### Coordinated Disclosure

We follow responsible disclosure practices:

1. **Private Reporting**: Report vulnerabilities privately first
2. **Assessment Period**: Allow time for assessment and fix development
3. **Coordinated Release**: Public disclosure after fix is available
4. **Credit**: Security researchers receive appropriate credit

### Hall of Fame

We maintain a security hall of fame to recognize researchers who help improve Seu Mail's security:

<!-- Security researchers who have reported vulnerabilities will be listed here -->

## Security Tools and Resources

### Recommended Tools

For security testing and analysis:

- **OWASP ZAP**: Web application security scanner
- **SonarQube**: Code quality and security analysis
- **Snyk**: Dependency vulnerability scanning
- **Microsoft Security Code Analysis**: Static analysis tools

### Security Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Microsoft Security Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Email Security Best Practices](https://www.cisa.gov/email-security-best-practices)

## Incident Response

In case of a security incident:

1. **Immediate Response**: Contain the issue and assess impact
2. **Communication**: Notify affected users promptly
3. **Remediation**: Deploy fixes and security updates
4. **Post-Incident**: Conduct review and improve processes

## Contact Information

For security-related inquiries:

- **Security Email**: security@seumail.project (to be established)
- **Project Repository**: [https://github.com/brmassa/Seu-Mail](https://github.com/brmassa/Seu-Mail)
- **Maintainer**: Bruno Massa (@brmassa)

## Legal

### Responsible Disclosure

By reporting security vulnerabilities, you agree to:

- Provide reasonable time for fixes before public disclosure
- Not access, modify, or delete user data
- Not perform testing that could harm users or systems
- Follow applicable laws and regulations

### Safe Harbor

We commit to:

- Not pursue legal action against security researchers
- Work with researchers to understand and fix issues
- Provide credit for responsible disclosure
- Maintain confidentiality of researcher information

---

**Last Updated**: December 2024

Thank you for helping keep Seu Mail secure! 🔒