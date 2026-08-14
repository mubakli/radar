using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Radar.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeterministicStoryGrouping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StorySourceItems_SourceItemId",
                table: "StorySourceItems");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "StorySourceItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "MembershipMethodVersion",
                table: "StorySourceItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "legacy-v1");

            migrationBuilder.Sql("UPDATE \"StorySourceItems\" m SET \"CreatedAt\" = s.\"CreatedAt\" FROM \"Stories\" s WHERE s.\"Id\" = m.\"StoryId\"");

            migrationBuilder.CreateTable(
                name: "StoryCorrections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResultStoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousStoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryCorrections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoryCorrections_Stories_ResultStoryId",
                        column: x => x.ResultStoryId,
                        principalTable: "Stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StorySourceItems_SourceItemId",
                table: "StorySourceItems",
                column: "SourceItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoryCorrections_Action_ResultStoryId_PreviousStoryId_Sourc~",
                table: "StoryCorrections",
                columns: new[] { "Action", "ResultStoryId", "PreviousStoryId", "SourceItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoryCorrections_ResultStoryId",
                table: "StoryCorrections",
                column: "ResultStoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoryCorrections");

            migrationBuilder.DropIndex(
                name: "IX_StorySourceItems_SourceItemId",
                table: "StorySourceItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "StorySourceItems");

            migrationBuilder.DropColumn(
                name: "MembershipMethodVersion",
                table: "StorySourceItems");

            migrationBuilder.CreateIndex(
                name: "IX_StorySourceItems_SourceItemId",
                table: "StorySourceItems",
                column: "SourceItemId");
        }
    }
}
