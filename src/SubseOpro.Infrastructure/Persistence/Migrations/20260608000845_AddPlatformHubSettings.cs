using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubseOpro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformHubSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platform_hub_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    HubTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HubSubtitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    LoginBadge = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LoginHeadline = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LoginHeadlineAccent = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LoginDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrimaryColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "#1B3A6B"),
                    AccentColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FooterText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_hub_settings", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO platform_hub_settings (
                    "Id", "ProductName", "HubTitle", "HubSubtitle", "LoginBadge",
                    "LoginHeadline", "LoginHeadlineAccent", "LoginDescription",
                    "PrimaryColor", "AccentColor", "FooterText"
                )
                SELECT
                    'dddddddd-dddd-dddd-dddd-dddddddddddd'::uuid,
                    'SubseçãoPRO',
                    'Acesse o Hub',
                    'Painel administrativo das subseções OAB',
                    'Plataforma SubseçãoPRO',
                    'Gestão inteligente',
                    'da subseção',
                    'Administre advogados, salas, reservas, parceiros e eleições em um único painel seguro e eficiente.',
                    '#1B3A6B',
                    '#C9A84C',
                    'SubseçãoPRO · Plataforma multi-tenant'
                WHERE NOT EXISTS (
                    SELECT 1 FROM platform_hub_settings
                    WHERE "Id" = 'dddddddd-dddd-dddd-dddd-dddddddddddd'::uuid
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_hub_settings");
        }
    }
}
