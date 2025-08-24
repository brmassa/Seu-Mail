using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seu.Mail.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmailDisplayMode = table.Column<int>(type: "INTEGER", nullable: false),
                    EmailLayoutMode = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultSignature = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    UseCompactMode = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmailsPerPage = table.Column<int>(type: "INTEGER", nullable: false),
                    MarkAsReadOnOpen = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowEmailPreview = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableKeyboardNavigation = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettings");
        }
    }
}
