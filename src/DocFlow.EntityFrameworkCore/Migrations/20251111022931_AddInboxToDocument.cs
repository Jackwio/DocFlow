using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddInboxToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Inbox",
                table: "Documents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Inbox",
                table: "Documents");
        }
    }
}
