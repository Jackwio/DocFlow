using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DocFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddAiSuggestionSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ConfidenceScore",
                table: "DocumentTags",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AiConfidence",
                table: "Documents",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AiGeneratedAt",
                table: "Documents",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AiSuggestedQueueId",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiSummary",
                table: "Documents",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentAiSuggestedTags",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TagName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    Reasoning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentAiSuggestedTags", x => new { x.DocumentId, x.Id });
                    table.ForeignKey(
                        name: "FK_DocumentAiSuggestedTags_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentAiSuggestedTags");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "DocumentTags");

            migrationBuilder.DropColumn(
                name: "AiConfidence",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "AiGeneratedAt",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "AiSuggestedQueueId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "AiSummary",
                table: "Documents");
        }
    }
}
