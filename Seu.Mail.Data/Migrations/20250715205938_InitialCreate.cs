using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seu.Mail.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    SmtpServer = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    SmtpPort = table.Column<int>(type: "INTEGER", nullable: false),
                    ImapServer = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ImapPort = table.Column<int>(type: "INTEGER", nullable: false),
                    UseSsl = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalendarSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstDayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    ShowWeekNumbers = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultView = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeZone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DefaultEventStartTime = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultEventDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    ShowAllDayEventsAtTop = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowWeekends = table.Column<bool>(type: "INTEGER", nullable: false),
                    DayViewStartHour = table.Column<int>(type: "INTEGER", nullable: false),
                    DayViewEndHour = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeSlotInterval = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultEventColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    ConfirmEventDeletion = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowEventTooltips = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultReminderMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    EnableReminders = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateFormat = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TimeFormat = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Use24HourFormat = table.Column<bool>(type: "INTEGER", nullable: false),
                    MonthViewNavigationRange = table.Column<int>(type: "INTEGER", nullable: false),
                    HighlightToday = table.Column<bool>(type: "INTEGER", nullable: false),
                    TodayHighlightColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    ShowDeclinedEvents = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxEventsPerDayCell = table.Column<int>(type: "INTEGER", nullable: false),
                    AutoSyncSubscriptions = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoSyncIntervalMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarSettings_EmailAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "EmailAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "INTEGER", nullable: false),
                    SyncIntervalMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastSyncStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSyncError = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    EventCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Password = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    ApiKey = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    ETag = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AutoSync = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeZone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarSubscriptions_EmailAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "EmailAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ParentFolder = table.Column<string>(type: "TEXT", nullable: true),
                    MessageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    UnreadCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSystemFolder = table.Column<bool>(type: "INTEGER", nullable: false),
                    FolderType = table.Column<string>(type: "TEXT", nullable: true),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailFolders_EmailAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "EmailAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailTags_EmailAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "EmailAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    StartDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsAllDay = table.Column<bool>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentEventId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubscriptionId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_CalendarEvents_ParentEventId",
                        column: x => x.ParentEventId,
                        principalTable: "CalendarEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_CalendarSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "CalendarSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_EmailAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "EmailAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageId = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    From = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    To = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    Cc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    Bcc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    TextBody = table.Column<string>(type: "TEXT", nullable: true),
                    HtmlBody = table.Column<string>(type: "TEXT", nullable: true),
                    DateSent = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateReceived = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsImportant = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Folder = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    HasAttachments = table.Column<bool>(type: "INTEGER", nullable: false),
                    Size = table.Column<int>(type: "INTEGER", nullable: false),
                    FolderNavigationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailMessages_EmailAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "EmailAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailMessages_EmailFolders_FolderNavigationId",
                        column: x => x.FolderNavigationId,
                        principalTable: "EmailFolders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventAttendees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CalendarEventId = table.Column<int>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponseStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResponseComment = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsOrganizer = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReceiveNotifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventAttendees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventAttendees_CalendarEvents_CalendarEventId",
                        column: x => x.CalendarEventId,
                        principalTable: "CalendarEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CalendarEventId = table.Column<int>(type: "INTEGER", nullable: false),
                    MinutesBefore = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsTriggered = table.Column<bool>(type: "INTEGER", nullable: false),
                    TriggeredAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CustomMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EmailAddress = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventReminders_CalendarEvents_CalendarEventId",
                        column: x => x.CalendarEventId,
                        principalTable: "CalendarEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecurrenceRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Frequency = table.Column<int>(type: "INTEGER", nullable: false),
                    Interval = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: true),
                    Until = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ByDayOfWeek = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ByDayOfMonth = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ByMonth = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ByWeekOfMonth = table.Column<int>(type: "INTEGER", nullable: true),
                    CalendarEventId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExceptionDates = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurrenceRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurrenceRules_CalendarEvents_CalendarEventId",
                        column: x => x.CalendarEventId,
                        principalTable: "CalendarEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmailMessageId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailAttachments_EmailMessages_EmailMessageId",
                        column: x => x.EmailMessageId,
                        principalTable: "EmailMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailMessageTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmailMessageId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailMessageTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailMessageTags_EmailMessages_EmailMessageId",
                        column: x => x.EmailMessageId,
                        principalTable: "EmailMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailMessageTags_EmailTags_TagId",
                        column: x => x.TagId,
                        principalTable: "EmailTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_AccountId_ExternalId",
                table: "CalendarEvents",
                columns: new[] { "AccountId", "ExternalId" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_EndDateTime",
                table: "CalendarEvents",
                column: "EndDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_ParentEventId",
                table: "CalendarEvents",
                column: "ParentEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_StartDateTime",
                table: "CalendarEvents",
                column: "StartDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_SubscriptionId",
                table: "CalendarEvents",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarSettings_AccountId",
                table: "CalendarSettings",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarSubscriptions_AccountId",
                table: "CalendarSubscriptions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarSubscriptions_IsActive",
                table: "CalendarSubscriptions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarSubscriptions_LastSyncAt",
                table: "CalendarSubscriptions",
                column: "LastSyncAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailAccounts_Email",
                table: "EmailAccounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailAttachments_EmailMessageId",
                table: "EmailAttachments",
                column: "EmailMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailFolders_AccountId",
                table: "EmailFolders",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_AccountId_MessageId",
                table: "EmailMessages",
                columns: new[] { "AccountId", "MessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_DateReceived",
                table: "EmailMessages",
                column: "DateReceived");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_Folder",
                table: "EmailMessages",
                column: "Folder");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_FolderNavigationId",
                table: "EmailMessages",
                column: "FolderNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_IsRead",
                table: "EmailMessages",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessageTags_EmailMessageId",
                table: "EmailMessageTags",
                column: "EmailMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessageTags_TagId",
                table: "EmailMessageTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTags_AccountId",
                table: "EmailTags",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendees_CalendarEventId_Email",
                table: "EventAttendees",
                columns: new[] { "CalendarEventId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventReminders_CalendarEventId",
                table: "EventReminders",
                column: "CalendarEventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventReminders_IsTriggered",
                table: "EventReminders",
                column: "IsTriggered");

            migrationBuilder.CreateIndex(
                name: "IX_RecurrenceRules_CalendarEventId",
                table: "RecurrenceRules",
                column: "CalendarEventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarSettings");

            migrationBuilder.DropTable(
                name: "EmailAttachments");

            migrationBuilder.DropTable(
                name: "EmailMessageTags");

            migrationBuilder.DropTable(
                name: "EventAttendees");

            migrationBuilder.DropTable(
                name: "EventReminders");

            migrationBuilder.DropTable(
                name: "RecurrenceRules");

            migrationBuilder.DropTable(
                name: "EmailMessages");

            migrationBuilder.DropTable(
                name: "EmailTags");

            migrationBuilder.DropTable(
                name: "CalendarEvents");

            migrationBuilder.DropTable(
                name: "EmailFolders");

            migrationBuilder.DropTable(
                name: "CalendarSubscriptions");

            migrationBuilder.DropTable(
                name: "EmailAccounts");
        }
    }
}
