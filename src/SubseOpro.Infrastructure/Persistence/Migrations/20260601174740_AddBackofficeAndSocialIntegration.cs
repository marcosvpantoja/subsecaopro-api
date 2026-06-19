using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubseOpro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBackofficeAndSocialIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "posts",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkUrl",
                table: "posts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "posts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Manual");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "posts",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "admin_users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "moderator"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "social_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Platform = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Handle = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ProfileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Connected = table.Column<bool>(type: "boolean", nullable: false),
                    FollowersCount = table.Column<int>(type: "integer", nullable: true),
                    LastSync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_accounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_Email",
                table: "admin_users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_users");

            migrationBuilder.DropTable(
                name: "social_accounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "LinkUrl",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "posts");
        }
    }
}
