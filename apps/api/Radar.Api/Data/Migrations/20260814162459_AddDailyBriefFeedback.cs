using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Radar.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyBriefFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Sources",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ItemFeedback",
                columns: table => new
                {
                    SourceItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Read = table.Column<bool>(type: "boolean", nullable: false),
                    Important = table.Column<bool>(type: "boolean", nullable: false),
                    Saved = table.Column<bool>(type: "boolean", nullable: false),
                    NotRelevant = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemFeedback", x => x.SourceItemId);
                    table.ForeignKey(
                        name: "FK_ItemFeedback_SourceItems_SourceItemId",
                        column: x => x.SourceItemId,
                        principalTable: "SourceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemFeedback");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Sources");
        }
    }
}
