using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Seu.Mail.Core.Models;

/// <summary>
/// Represents an application user with identity and profile information.
/// </summary>
public class User : IdentityUser
{
    /// <summary>
    /// The user's first name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// The user's last name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// The tenant ID this user belongs to.
    /// </summary>
    [Required]
    public int TenantId { get; set; }

    /// <summary>
    /// The date and time when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The date and time of the user's last login.
    /// </summary>
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether the user is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The user's profile picture URL or path.
    /// </summary>
    public string? ProfilePicture { get; set; }

    /// <summary>
    /// The user's preferred time zone.
    /// </summary>
    public string? TimeZone { get; set; } = "UTC";

    /// <summary>
    /// The user's preferred language.
    /// </summary>
    public string? Language { get; set; } = "en-US";

    // Navigation properties
    // public virtual Tenant Tenant { get; set; } = null!;
    // public virtual ICollection<EmailAccount> EmailAccounts { get; set; } = new List<EmailAccount>();
    // public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    // public virtual ICollection<UserSettings> UserSettings { get; set; } = new List<UserSettings>();

    /// <summary>
    /// Gets the user's full name by combining first and last name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}
