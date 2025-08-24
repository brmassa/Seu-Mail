using Microsoft.EntityFrameworkCore;
using Seu.Mail.Core.Models;
using Seu.Mail.Core.Models.Calendar;

namespace Seu.Mail.Data.Context;

/// <summary>
/// Entity Framework database context for the Seu.Mail application.
/// </summary>
public class EmailDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public EmailDbContext(DbContextOptions<EmailDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the email accounts.
    /// </summary>
    public DbSet<EmailAccount> EmailAccounts { get; set; } = null!;
    /// <summary>
    /// Gets or sets the email messages.
    /// </summary>
    public DbSet<EmailMessage> EmailMessages { get; set; } = null!;
    /// <summary>
    /// Gets or sets the email attachments.
    /// </summary>
    public DbSet<EmailAttachment> EmailAttachments { get; set; } = null!;
    /// <summary>
    /// Gets or sets the email folders.
    /// </summary>
    public DbSet<EmailFolder> EmailFolders { get; set; } = null!;
    /// <summary>
    /// Gets or sets the email tags.
    /// </summary>
    public DbSet<EmailTag> EmailTags { get; set; } = null!;
    /// <summary>
    /// Gets or sets the email message tags.
    /// </summary>
    public DbSet<EmailMessageTag> EmailMessageTags { get; set; } = null!;
    /// <summary>
    /// Gets or sets the user settings.
    /// </summary>
    public DbSet<UserSettings> UserSettings { get; set; } = null!;

    // Calendar entities
    /// <summary>
    /// Gets or sets the calendar events.
    /// </summary>
    public DbSet<CalendarEvent> CalendarEvents { get; set; } = null!;
    /// <summary>
    /// Gets or sets the recurrence rules.
    /// </summary>
    public DbSet<RecurrenceRule> RecurrenceRules { get; set; } = null!;
    /// <summary>
    /// Gets or sets the event reminders.
    /// </summary>
    public DbSet<EventReminder> EventReminders { get; set; } = null!;
    /// <summary>
    /// Gets or sets the event attendees.
    /// </summary>
    public DbSet<EventAttendee> EventAttendees { get; set; } = null!;
    /// <summary>
    /// Gets or sets the calendar subscriptions.
    /// </summary>
    public DbSet<CalendarSubscription> CalendarSubscriptions { get; set; } = null!;
    /// <summary>
    /// Gets or sets the calendar settings.
    /// </summary>
    public DbSet<CalendarSettings> CalendarSettings { get; set; } = null!;

    /// <summary>
    /// Configures the schema needed for the context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure EmailAccount
        modelBuilder.Entity<EmailAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.DisplayName).HasMaxLength(256); // Made optional
            entity.Property(e => e.EncryptedPassword).IsRequired().HasMaxLength(512);
            entity.Property(e => e.SmtpServer).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ImapServer).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure EmailMessage
        modelBuilder.Entity<EmailMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MessageId).IsRequired().HasMaxLength(512);
            entity.Property(e => e.From).IsRequired().HasMaxLength(512);
            entity.Property(e => e.To).IsRequired().HasMaxLength(2048);
            entity.Property(e => e.Cc).HasMaxLength(2048);
            entity.Property(e => e.Bcc).HasMaxLength(2048);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(512);
            entity.Property(e => e.Folder).HasMaxLength(128);

            entity.HasOne(e => e.Account)
                .WithMany(a => a.EmailMessages)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.AccountId, e.MessageId }).IsUnique();
            entity.HasIndex(e => e.DateReceived);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.Folder);
        });

        // Configure EmailAttachment
        modelBuilder.Entity<EmailAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(512);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(256);

            entity.HasOne(e => e.EmailMessage)
                .WithMany(m => m.Attachments)
                .HasForeignKey(e => e.EmailMessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure CalendarEvent
        modelBuilder.Entity<CalendarEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.Color).HasMaxLength(7);
            entity.Property(e => e.ExternalId).HasMaxLength(255);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MasterEvent)
                .WithMany(p => p.RecurrenceInstances)
                .HasForeignKey(e => e.MasterEventId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.StartDateTime);
            entity.HasIndex(e => e.EndDateTime);
            entity.HasIndex(e => new { e.AccountId, e.ExternalId });
        });

        // Configure RecurrenceRule
        modelBuilder.Entity<RecurrenceRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ByDayOfWeek).HasMaxLength(20);
            entity.Property(e => e.ByDayOfMonth).HasMaxLength(100);
            entity.Property(e => e.ByMonth).HasMaxLength(50);
            entity.Property(e => e.ExceptionDates).HasMaxLength(2000);

            entity.HasOne(e => e.CalendarEvent)
                .WithOne(c => c.Recurrence)
                .HasForeignKey<RecurrenceRule>(e => e.CalendarEventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure EventReminder
        modelBuilder.Entity<EventReminder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomMessage).HasMaxLength(500);
            entity.Property(e => e.EmailAddress).HasMaxLength(256);

            entity.HasOne(e => e.CalendarEvent)
                .WithMany(c => c.Reminders)
                .HasForeignKey(e => e.CalendarEventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.IsTriggered);
        });

        // Configure EventAttendee
        modelBuilder.Entity<EventAttendee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.ResponseComment).HasMaxLength(500);

            entity.HasOne(e => e.CalendarEvent)
                .WithMany(c => c.Attendees)
                .HasForeignKey(e => e.CalendarEventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CalendarEventId, e.Email }).IsUnique();
        });

        // Configure CalendarSubscription
        modelBuilder.Entity<CalendarSubscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Color).HasMaxLength(7);
            entity.Property(e => e.LastSyncError).HasMaxLength(1000);
            entity.Property(e => e.Username).HasMaxLength(256);
            entity.Property(e => e.Password).HasMaxLength(512);
            entity.Property(e => e.ApiKey).HasMaxLength(512);
            entity.Property(e => e.ETag).HasMaxLength(256);
            entity.Property(e => e.TimeZone).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.LastSyncAt);
        });

        // Configure CalendarSettings
        modelBuilder.Entity<CalendarSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TimeZone).HasMaxLength(100);
            entity.Property(e => e.DefaultEventColor).HasMaxLength(7);
            entity.Property(e => e.DateFormat).HasMaxLength(50);
            entity.Property(e => e.TimeFormat).HasMaxLength(50);
            entity.Property(e => e.TodayHighlightColor).HasMaxLength(7);

            entity.HasOne(e => e.Account)
                .WithOne()
                .HasForeignKey<CalendarSettings>(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.AccountId).IsUnique();
        });

        // Configure UserSettings
        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DefaultSignature).HasMaxLength(2000);
        });
    }
}
