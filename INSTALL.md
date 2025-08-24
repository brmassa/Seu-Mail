# Installation Guide for Seu Mail 🚀

This guide will walk you through the complete installation and setup process for Seu Mail on various platforms.

## 📋 Table of Contents

- [System Requirements](#system-requirements)
- [Quick Installation](#quick-installation)
- [Platform-Specific Setup](#platform-specific-setup)
- [Configuration](#configuration)
- [First-Time Setup](#first-time-setup)
- [Email Account Configuration](#email-account-configuration)
- [Troubleshooting](#troubleshooting)
- [Advanced Setup](#advanced-setup)

## 💻 System Requirements

### Minimum Requirements
- **OS**: Windows 10/11, macOS 10.15+, or Linux (Ubuntu 20.04+, CentOS 8+)
- **RAM**: 2 GB available memory
- **Storage**: 500 MB free disk space
- **Network**: Internet connection for email synchronization

### Recommended Requirements
- **OS**: Windows 11, macOS 12+, or Linux (latest LTS)
- **RAM**: 4 GB available memory
- **Storage**: 2 GB free disk space
- **Network**: Broadband internet connection

### Software Prerequisites
- [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) (automatically included in SDK)
- Modern web browser (Chrome 90+, Firefox 88+, Safari 14+, Edge 90+)

## ⚡ Quick Installation

### Option 1: Using .NET CLI (Recommended)

```bash
# Clone the repository
git clone https://github.com/brmassa/Seu-Mail.git
cd Seu-Mail

# Navigate to the main project
cd Mail

# Restore dependencies
dotnet restore

# Run the application
dotnet run
```

### Option 2: Download Release (Coming Soon)

1. Go to [Releases](https://github.com/brmassa/Seu-Mail/releases)
2. Download the latest release for your platform
3. Extract the archive
4. Run the executable

## 🖥️ Platform-Specific Setup

### Windows

#### Prerequisites Installation

1. **Install .NET 9.0 SDK**:
   - Download from [Microsoft .NET](https://dotnet.microsoft.com/download/dotnet/9.0)
   - Run the installer and follow the prompts
   - Verify installation: Open Command Prompt and run `dotnet --version`

2. **Install Git** (optional, for source installation):
   - Download from [Git for Windows](https://gitforwindows.org/)
   - Install with default settings

#### Installation Steps

```cmd
# Open Command Prompt or PowerShell
cd C:\
git clone https://github.com/brmassa/Seu-Mail.git
cd Seu-Mail\Mail
dotnet restore
dotnet run
```

#### Running as Windows Service (Optional)

```cmd
# Publish the application
dotnet publish -c Release -o C:\SeuMail

# Install as Windows Service (requires admin privileges)
sc create SeuMail binPath="C:\SeuMail\Seu.Mail.exe"
sc start SeuMail
```

### macOS

#### Prerequisites Installation

1. **Install .NET 9.0 SDK**:
   ```bash
   # Using Homebrew (recommended)
   brew install --cask dotnet
   
   # Or download from Microsoft
   # https://dotnet.microsoft.com/download/dotnet/9.0
   ```

2. **Install Git** (if not already installed):
   ```bash
   # Git is usually pre-installed on macOS
   git --version
   
   # If not installed, install Xcode Command Line Tools
   xcode-select --install
   ```

#### Installation Steps

```bash
# Open Terminal
cd ~/Downloads
git clone https://github.com/brmassa/Seu-Mail.git
cd Seu-Mail/Mail
dotnet restore
dotnet run
```

#### macOS App Bundle (Optional)

```bash
# Create app bundle
dotnet publish -c Release -r osx-x64 --self-contained true
# Follow additional steps to create .app bundle
```

### Linux (Ubuntu/Debian)

#### Prerequisites Installation

```bash
# Update package list
sudo apt update

# Install .NET 9.0 SDK
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt update
sudo apt install -y dotnet-sdk-9.0

# Install Git
sudo apt install -y git

# Verify installations
dotnet --version
git --version
```

#### Installation Steps

```bash
# Clone and run
cd ~/
git clone https://github.com/brmassa/Seu-Mail.git
cd Seu-Mail/Mail
dotnet restore
dotnet run
```

#### Running as systemd Service

1. Create service file:
```bash
sudo nano /etc/systemd/system/seumail.service
```

2. Add content:
```ini
[Unit]
Description=Seu Mail Application
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /home/yourusername/Seu-Mail/Mail/bin/Release/net9.0/Seu.Mail.dll
Restart=always
RestartSec=10
User=yourusername
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

3. Enable and start:
```bash
sudo systemctl enable seumail.service
sudo systemctl start seumail.service
```

### Linux (CentOS/RHEL/Fedora)

#### Prerequisites Installation

```bash
# For CentOS/RHEL 8+
sudo dnf install -y dotnet-sdk-9.0 git

# For Fedora
sudo dnf install -y dotnet-sdk-9.0 git

# Verify installations
dotnet --version
git --version
```

## ⚙️ Configuration

### Application Settings

Edit `appsettings.json` in the `Mail` directory:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Seu.Mail.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "EmailSettings": {
    "MaxAttachmentSize": 26214400,
    "DefaultSyncInterval": 300,
    "MaxEmailsPerSync": 100,
    "EnableAutoSync": true,
    "DefaultPageSize": 50,
    "ConnectionTimeout": 30
  },
  "Security": {
    "EncryptPasswords": true,
    "RequireHttps": false,
    "AllowedHosts": "*"
  },
  "Calendar": {
    "DefaultReminderMinutes": 15,
    "MaxEventsPerDay": 100,
    "EnableCalendarSync": true
  }
}
```

### Environment Variables

Set these for production deployment:

```bash
# Linux/macOS
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000

# Windows
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=http://0.0.0.0:5000
```

## 🎯 First-Time Setup

### 1. Launch the Application

After installation, the application will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:7000` (development)

### 2. Initial Database Setup

The application will automatically:
- Create the SQLite database file (`Seu.Mail.db`)
- Run initial migrations
- Set up required tables and indexes

### 3. Security Setup

On first launch, you'll be prompted to:
- Set up application security settings
- Configure HTTPS (recommended for production)
- Set up data encryption preferences

## 📧 Email Account Configuration

### Supported Email Providers

| Provider | Auto-Setup | Manual Setup Required |
|----------|------------|----------------------|
| Gmail | ✅ | App Password needed |
| Outlook/Hotmail | ✅ | Standard auth |
| Yahoo Mail | ✅ | App Password recommended |
| iCloud | ❌ | Manual configuration |
| ProtonMail | ❌ | Bridge required |
| Custom IMAP | ❌ | Full manual setup |

### Gmail Setup (Detailed)

1. **Enable 2-Factor Authentication**:
   - Go to [Google Account Security](https://myaccount.google.com/security)
   - Enable 2-Step Verification

2. **Generate App Password**:
   - Go to Security → 2-Step Verification → App passwords
   - Select "Mail" and generate password
   - Use this 16-character password in Seu Mail

3. **Configure in Seu Mail**:
   - Email: `your-email@gmail.com`
   - Password: `Generated App Password`
   - IMAP: `imap.gmail.com:993 (SSL)`
   - SMTP: `smtp.gmail.com:587 (TLS)`

### Outlook.com Setup

```
Email: your-email@outlook.com
Password: Your regular password
IMAP: outlook.office365.com:993 (SSL)
SMTP: smtp-mail.outlook.com:587 (TLS)
```

### Yahoo Mail Setup

```
Email: your-email@yahoo.com
Password: App Password (recommended)
IMAP: imap.mail.yahoo.com:993 (SSL)
SMTP: smtp.mail.yahoo.com:587 (TLS)
```

### Custom IMAP/SMTP Setup

For other providers, you'll need:
- IMAP server address and port
- SMTP server address and port
- Security protocol (SSL/TLS/STARTTLS)
- Authentication method

## 🔧 Troubleshooting

### Common Issues

#### Port Already in Use
```bash
# Check what's using the port
netstat -tulpn | grep :5000

# Run on different port
dotnet run --urls "http://localhost:5001"
```

#### Database Permission Issues
```bash
# Linux/macOS: Fix permissions
chmod 664 Seu.Mail.db
chown $USER:$USER Seu.Mail.db

# Windows: Run as Administrator or check folder permissions
```

#### Email Connection Failed
1. Verify server settings
2. Check firewall settings
3. Ensure correct authentication method
4. Test with email client (Thunderbird, etc.)

#### SSL/TLS Certificate Issues
```bash
# Skip certificate validation (development only)
export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
```

### Log File Locations

- **Windows**: `%APPDATA%\SeuMail\Logs\`
- **macOS**: `~/Library/Application Support/SeuMail/Logs/`
- **Linux**: `~/.local/share/SeuMail/Logs/`

### Debug Mode

Run in debug mode for detailed logging:

```bash
dotnet run --configuration Debug --verbosity diagnostic
```

## 🚀 Advanced Setup

### Reverse Proxy Setup (Nginx)

```nginx
server {
    listen 80;
    server_name yourdomain.com;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### Docker Deployment

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Mail/Seu.Mail.csproj", "Mail/"]
RUN dotnet restore "Mail/Seu.Mail.csproj"
COPY . .
WORKDIR "/src/Mail"
RUN dotnet build "Seu.Mail.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Seu.Mail.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Seu.Mail.dll"]
```

```bash
# Build and run with Docker
docker build -t seumail .
docker run -p 5000:80 -v $(pwd)/data:/app/data seumail
```

### Performance Optimization

#### Database Optimization
```bash
# Enable WAL mode for better concurrent access
sqlite3 Seu.Mail.db "PRAGMA journal_mode=WAL;"
```

#### Memory Settings
```json
{
  "EmailSettings": {
    "MaxEmailsPerSync": 50,
    "SyncBatchSize": 25,
    "ConnectionPoolSize": 5
  }
}
```

## 📚 Additional Resources

- [Official Documentation](https://github.com/brmassa/Seu-Mail/wiki)
- [Troubleshooting Guide](https://github.com/brmassa/Seu-Mail/wiki/Troubleshooting)
- [Community Support](https://github.com/brmassa/Seu-Mail/discussions)
- [Issue Tracker](https://github.com/brmassa/Seu-Mail/issues)

## 🆘 Getting Help

If you encounter issues during installation:

1. **Check the logs** for error messages
2. **Search existing issues** on GitHub
3. **Create a new issue** with:
   - Your operating system and version
   - .NET version (`dotnet --version`)
   - Complete error message
   - Steps to reproduce the issue

## ✅ Post-Installation Checklist

- [ ] Application starts without errors
- [ ] Database is created successfully
- [ ] Web interface is accessible
- [ ] At least one email account configured
- [ ] Email synchronization working
- [ ] Attachments can be viewed/downloaded
- [ ] Calendar integration functional (if used)
- [ ] Settings can be saved and loaded
- [ ] Backup strategy in place

---

**Congratulations!** 🎉 You've successfully installed Seu Mail. Enjoy your privacy-focused email experience!

For updates and announcements, watch the [GitHub repository](https://github.com/brmassa/Seu-Mail).