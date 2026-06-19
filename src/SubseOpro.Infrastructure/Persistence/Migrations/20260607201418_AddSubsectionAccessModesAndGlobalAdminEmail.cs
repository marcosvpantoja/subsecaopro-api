using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubseOpro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubsectionAccessModesAndGlobalAdminEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_admin_users_SubsectionId_Email",
                table: "admin_users");

            migrationBuilder.AddColumn<string>(
                name: "DedicatedHubHost",
                table: "subsections",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EasProfileName",
                table: "subsections",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HubAccessMode",
                table: "subsections",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Shared");

            migrationBuilder.AddColumn<string>(
                name: "MobileAccessMode",
                table: "subsections",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "SharedApp");

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_Email",
                table: "admin_users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_SubsectionId",
                table: "admin_users",
                column: "SubsectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_admin_users_Email",
                table: "admin_users");

            migrationBuilder.DropIndex(
                name: "IX_admin_users_SubsectionId",
                table: "admin_users");

            migrationBuilder.DropColumn(
                name: "DedicatedHubHost",
                table: "subsections");

            migrationBuilder.DropColumn(
                name: "EasProfileName",
                table: "subsections");

            migrationBuilder.DropColumn(
                name: "HubAccessMode",
                table: "subsections");

            migrationBuilder.DropColumn(
                name: "MobileAccessMode",
                table: "subsections");

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_SubsectionId_Email",
                table: "admin_users",
                columns: new[] { "SubsectionId", "Email" },
                unique: true);
        }
    }
}
