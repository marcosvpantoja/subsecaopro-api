using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubseOpro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platform_app_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicAppEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PlatformName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DefaultSubsectionSlug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    AiMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "mock"),
                    AiGlobalPrompt = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AiDisclaimer = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PublicModulesJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_app_settings", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO platform_app_settings (
                    "Id", "PublicAppEnabled", "PlatformName", "AiMode", "PublicModulesJson"
                )
                SELECT
                    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee'::uuid,
                    false,
                    'SubseçãoPRO',
                    'mock',
                    '{"lawyers":{"enabled":true,"subsectionSlugs":[]},"banners":{"enabled":true,"subsectionSlugs":[]},"posts":{"enabled":true,"subsectionSlugs":[]},"articles":{"enabled":true,"subsectionSlugs":[]},"about":{"enabled":true,"subsectionSlugs":[]},"elections":{"enabled":true,"subsectionSlugs":[]},"notifications":{"enabled":false,"subsectionSlugs":[]}}'::jsonb
                WHERE NOT EXISTS (
                    SELECT 1 FROM platform_app_settings
                    WHERE "Id" = 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee'::uuid
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_app_settings");
        }
    }
}
