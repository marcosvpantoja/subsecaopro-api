using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubseOpro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultPlatformAdmin : Migration
    {
        private const string PlatformAdminId = "cccccccc-cccc-cccc-cccc-ccccccccccc1";
        private const string BackofficeAdminId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1";
        private const string DefaultSubsectionId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
        // Hashes de senhas aleatórias e descartáveis. Troque pelo hash bcrypt da sua senha ou redefina a senha após o deploy.
        private const string PlatformPasswordHash = "$2a$12$p.jusEsQAOEkUYilRql91ewSymUz4BkFq2KYTIskxPXvbebgAnuWe";
        private const string BackofficePasswordHash = "$2a$12$jSgEU7DY6lsOfkJI/aBGvOntgV4L6ePk3D0ttM7rkoSQ/qRg6eXyu";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                INSERT INTO platform_admins ("Id", "Name", "Email", "PasswordHash", "IsActive")
                SELECT '{PlatformAdminId}'::uuid, 'Platform Admin', 'platform@subsecao.pro', '{PlatformPasswordHash}', true
                WHERE NOT EXISTS (
                    SELECT 1 FROM platform_admins WHERE "Email" = 'platform@subsecao.pro'
                );
                """);

            migrationBuilder.Sql($"""
                INSERT INTO admin_users ("Id", "SubsectionId", "Name", "Email", "PasswordHash", "Role", "IsActive")
                SELECT
                    '{BackofficeAdminId}'::uuid,
                    '{DefaultSubsectionId}'::uuid,
                    'Administrador Demo',
                    'admin@subsecao.pro',
                    '{BackofficePasswordHash}',
                    'admin',
                    true
                WHERE EXISTS (SELECT 1 FROM subsections WHERE "Id" = '{DefaultSubsectionId}'::uuid)
                  AND NOT EXISTS (
                    SELECT 1 FROM admin_users
                    WHERE "SubsectionId" = '{DefaultSubsectionId}'::uuid
                      AND "Email" = 'admin@subsecao.pro'
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                DELETE FROM admin_users
                WHERE "Id" = '{BackofficeAdminId}'::uuid;
                """);

            migrationBuilder.Sql($"""
                DELETE FROM platform_admins
                WHERE "Id" = '{PlatformAdminId}'::uuid;
                """);
        }
    }
}
