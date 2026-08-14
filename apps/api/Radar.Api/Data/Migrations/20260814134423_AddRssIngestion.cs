using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Radar.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRssIngestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "Sources",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql("UPDATE \"Sources\" SET \"Enabled\" = TRUE WHERE \"Enabled\" = FALSE");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "SourceItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PublishedAt",
                table: "SourceItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "SourceItems",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "SourceItems",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FetchAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    EntryCount = table.Column<int>(type: "integer", nullable: false),
                    InsertedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    FailureCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FetchAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FetchAttempts_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FetchAttempts_SourceId_AttemptedAt",
                table: "FetchAttempts",
                columns: new[] { "SourceId", "AttemptedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FetchAttempts");

            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "SourceItems");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "SourceItems");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "SourceItems");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "SourceItems");
        }
    }
}
