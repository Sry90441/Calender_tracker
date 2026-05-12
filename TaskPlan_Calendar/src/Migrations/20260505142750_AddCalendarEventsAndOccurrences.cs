using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskPlan_Calendar.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarEventsAndOccurrences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    StartDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsAllDay = table.Column<bool>(type: "INTEGER", nullable: false),
                    ColorHex = table.Column<string>(type: "TEXT", nullable: true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    RecurrencePattern = table.Column<int>(type: "INTEGER", nullable: false),
                    RecurrenceEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    WeeklyRecurrenceDays = table.Column<byte>(type: "INTEGER", nullable: true),
                    MonthlyRecurrenceDay = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEventOccurrences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CalendarEventId = table.Column<int>(type: "INTEGER", nullable: false),
                    OccurrenceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsModified = table.Column<bool>(type: "INTEGER", nullable: false),
                    ModifiedTitle = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedDescription = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedStartDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedEndDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsCanceled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEventOccurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarEventOccurrences_CalendarEvents_CalendarEventId",
                        column: x => x.CalendarEventId,
                        principalTable: "CalendarEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEventOccurrences_EventId_Date",
                table: "CalendarEventOccurrences",
                columns: new[] { "CalendarEventId", "OccurrenceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_CategoryId",
                table: "CalendarEvents",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_UserId_StartDateTime",
                table: "CalendarEvents",
                columns: new[] { "UserId", "StartDateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarEventOccurrences");

            migrationBuilder.DropTable(
                name: "CalendarEvents");
        }
    }
}
