using System.ComponentModel.DataAnnotations;

namespace Seu.Mail.Core.Models;

/// <summary>
/// Represents an email attachment with file metadata and content
/// </summary>
/// <summary>
/// Represents an email attachment with file metadata and content.
/// </summary>
public class EmailAttachment
{
    /// <summary>
    /// Unique identifier for the attachment.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the email message this attachment belongs to.
    /// </summary>
    [Required]
    public int EmailMessageId { get; set; }

    /// <summary>
    /// Original filename of the attachment.
    /// </summary>
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME content type of the attachment.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Size of the attachment in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Content-ID header for inline attachments.
    /// </summary>
    [StringLength(255)]
    public string? ContentId { get; set; }

    /// <summary>
    /// Content-Disposition header (attachment, inline, etc.).
    /// </summary>
    [StringLength(50)]
    public string ContentDisposition { get; set; } = "attachment";

    /// <summary>
    /// Indicates whether this is an inline attachment (embedded in HTML).
    /// </summary>
    public bool IsInline { get; set; } = false;

    /// <summary>
    /// MD5 hash of the file content for integrity checking.
    /// </summary>
    [StringLength(32)]
    public string? ContentHash { get; set; }

    /// <summary>
    /// File path where the attachment is stored locally (if saved to disk).
    /// </summary>
    [StringLength(500)]
    public string? LocalPath { get; set; }

    /// <summary>
    /// Binary content of the attachment (for small files).
    /// </summary>
    public byte[]? Content { get; set; }

    /// <summary>
    /// Indicates whether the attachment content has been downloaded.
    /// </summary>
    public bool IsDownloaded { get; set; } = false;

    /// <summary>
    /// Date when the attachment was downloaded.
    /// </summary>
    public DateTime? DownloadedAt { get; set; }

    /// <summary>
    /// Transfer encoding used for the attachment.
    /// </summary>
    [StringLength(50)]
    public string? TransferEncoding { get; set; }

    /// <summary>
    /// Character set for text attachments.
    /// </summary>
    [StringLength(50)]
    public string? Charset { get; set; }

    /// <summary>
    /// Additional MIME parameters as JSON string.
    /// </summary>
    public string? MimeParameters { get; set; }

    /// <summary>
    /// Indicates whether the attachment is safe to open (virus scan result).
    /// </summary>
    public bool? IsSafe { get; set; }

    /// <summary>
    /// Date when the attachment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the attachment was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the related email message.
    /// </summary>
    public virtual EmailMessage? EmailMessage { get; set; }

    /// <summary>
    /// Alias for Content property to match service interface expectations.
    /// </summary>
    public byte[]? Data
    {
        get => Content;
        set => Content = value;
    }

    /// <summary>
    /// Gets the file extension from the filename.
    /// </summary>
    public string FileExtension => Path.GetExtension(FileName).ToLowerInvariant();

    /// <summary>
    /// Gets a human-readable file size string.
    /// </summary>
    public string FormattedSize
    {
        get
        {
            if (Size == 0) return "0 B";

            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            int place = Convert.ToInt32(Math.Floor(Math.Log(Size, 1024)));
            double num = Math.Round(Size / Math.Pow(1024, place), 1);
            return $"{num} {suf[place]}";
        }
    }

    /// <summary>
    /// Determines if the attachment is an image based on content type.
    /// </summary>
    public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines if the attachment is a document based on content type.
    /// </summary>
    public bool IsDocument => ContentType.StartsWith("application/", StringComparison.OrdinalIgnoreCase) ||
                             ContentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines if the attachment is a video based on content type.
    /// </summary>
    public bool IsVideo => ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines if the attachment is an audio file based on content type.
    /// </summary>
    public bool IsAudio => ContentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the icon CSS class for the file type (for UI display).
    /// </summary>
    public string IconClass
    {
        get
        {
            return FileExtension switch
            {
                ".pdf" => "fas fa-file-pdf text-danger",
                ".doc" or ".docx" => "fas fa-file-word text-primary",
                ".xls" or ".xlsx" => "fas fa-file-excel text-success",
                ".ppt" or ".pptx" => "fas fa-file-powerpoint text-warning",
                ".zip" or ".rar" or ".7z" => "fas fa-file-archive text-secondary",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "fas fa-file-image text-info",
                ".mp4" or ".avi" or ".mkv" or ".mov" => "fas fa-file-video text-purple",
                ".mp3" or ".wav" or ".flac" or ".ogg" => "fas fa-file-audio text-warning",
                ".txt" or ".md" or ".rtf" => "fas fa-file-alt text-secondary",
                ".html" or ".htm" => "fas fa-file-code text-primary",
                _ => "fas fa-file text-muted"
            };
        }
    }
}
