using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubseOpro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubsectionIaConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IaAreasCobertasJson",
                table: "subsections",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "IaDisclaimer",
                table: "subsections",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IaEnabled",
                table: "subsections",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "IaMaxTurns",
                table: "subsections",
                type: "integer",
                nullable: false,
                defaultValue: 8);

            migrationBuilder.AddColumn<string>(
                name: "IaSystemPrompt",
                table: "subsections",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IaAreasCobertasJson",
                table: "subsections");

            migrationBuilder.DropColumn(
                name: "IaDisclaimer",
                table: "subsections");

            migrationBuilder.DropColumn(
                name: "IaEnabled",
                table: "subsections");

            migrationBuilder.DropColumn(
                name: "IaMaxTurns",
                table: "subsections");

            migrationBuilder.DropColumn(
                name: "IaSystemPrompt",
                table: "subsections");
        }
    }
}
