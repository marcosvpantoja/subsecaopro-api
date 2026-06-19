using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubseOpro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var defaultSubsectionId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

            migrationBuilder.CreateTable(
                name: "platform_admins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "subsections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrimaryColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "#1B3A6B"),
                    AccentColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Hours = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subsections", x => x.Id);
                });

            migrationBuilder.Sql($"""
                INSERT INTO subsections ("Id", "Name", "Slug", "IsActive", "PrimaryColor", "Description", "Address", "Phone", "Email", "Hours", "CreatedAt")
                SELECT
                    '{defaultSubsectionId}'::uuid,
                    'OAB Cidade Exemplo',
                    'cidade-exemplo',
                    true,
                    '#1B3A6B',
                    COALESCE("Description", ''),
                    COALESCE("Address", ''),
                    COALESCE("Phone", ''),
                    COALESCE("Email", ''),
                    COALESCE("Hours", ''),
                    NOW()
                FROM subsection_info
                LIMIT 1;
                """);

            migrationBuilder.Sql($"""
                INSERT INTO subsections ("Id", "Name", "Slug", "IsActive", "PrimaryColor", "Description", "Address", "Phone", "Email", "Hours", "CreatedAt")
                SELECT '{defaultSubsectionId}'::uuid, 'OAB Cidade Exemplo', 'cidade-exemplo', true, '#1B3A6B', '', '', '', '', '', NOW()
                WHERE NOT EXISTS (SELECT 1 FROM subsections WHERE "Id" = '{defaultSubsectionId}'::uuid);
                """);

            migrationBuilder.DropIndex(
                name: "IX_specialties_Name",
                table: "specialties");

            migrationBuilder.DropIndex(
                name: "IX_lawyers_OabNumber",
                table: "lawyers");

            migrationBuilder.DropIndex(
                name: "IX_articles_Slug",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "IX_admin_users_Email",
                table: "admin_users");

            migrationBuilder.AddColumn<Guid>(
                name: "SubsectionId",
                table: "specialties",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubsectionId",
                table: "social_accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubsectionId",
                table: "rooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubsectionId",
                table: "reservations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubsectionId",
                table: "posts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubsectionId",
                table: "partners",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubsectionId",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubsectionId",
                table: "lawyers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubsectionId",
                table: "elections",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubsectionId",
                table: "banners",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubsectionId",
                table: "articles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubsectionId",
                table: "admin_users",
                type: "uuid",
                nullable: true);

            var tables = new[]
            {
                "specialties", "social_accounts", "rooms", "reservations", "posts",
                "partners", "notifications", "lawyers", "elections", "banners", "articles", "admin_users"
            };

            foreach (var table in tables)
            {
                migrationBuilder.Sql($"""
                    UPDATE "{table}" SET "SubsectionId" = '{defaultSubsectionId}'::uuid WHERE "SubsectionId" IS NULL;
                    """);
                migrationBuilder.AlterColumn<Guid>(
                    name: "SubsectionId",
                    table: table,
                    type: "uuid",
                    nullable: false,
                    oldClrType: typeof(Guid),
                    oldType: "uuid",
                    oldNullable: true);
            }

            migrationBuilder.DropTable(
                name: "subsection_info");

            migrationBuilder.CreateIndex(
                name: "IX_specialties_SubsectionId_Name",
                table: "specialties",
                columns: new[] { "SubsectionId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_social_accounts_SubsectionId",
                table: "social_accounts",
                column: "SubsectionId");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_SubsectionId",
                table: "rooms",
                column: "SubsectionId");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_SubsectionId",
                table: "reservations",
                column: "SubsectionId");

            migrationBuilder.CreateIndex(
                name: "IX_posts_SubsectionId",
                table: "posts",
                column: "SubsectionId");

            migrationBuilder.CreateIndex(
                name: "IX_partners_SubsectionId",
                table: "partners",
                column: "SubsectionId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_SubsectionId",
                table: "notifications",
                column: "SubsectionId");

            migrationBuilder.CreateIndex(
                name: "IX_lawyers_SubsectionId_OabNumber",
                table: "lawyers",
                columns: new[] { "SubsectionId", "OabNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_elections_SubsectionId",
                table: "elections",
                column: "SubsectionId");

            migrationBuilder.CreateIndex(
                name: "IX_banners_SubsectionId",
                table: "banners",
                column: "SubsectionId");

            migrationBuilder.CreateIndex(
                name: "IX_articles_SubsectionId_Slug",
                table: "articles",
                columns: new[] { "SubsectionId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_SubsectionId_Email",
                table: "admin_users",
                columns: new[] { "SubsectionId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_admins_Email",
                table: "platform_admins",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subsections_Slug",
                table: "subsections",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_admin_users_subsections_SubsectionId",
                table: "admin_users",
                column: "SubsectionId",
                principalTable: "subsections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_articles_subsections_SubsectionId",
                table: "articles",
                column: "SubsectionId",
                principalTable: "subsections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_banners_subsections_SubsectionId",
                table: "banners",
                column: "SubsectionId",
                principalTable: "subsections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_elections_subsections_SubsectionId",
                table: "elections",
                column: "SubsectionId",
                principalTable: "subsections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_lawyers_subsections_SubsectionId",
                table: "lawyers",
                column: "SubsectionId",
                principalTable: "subsections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_subsections_SubsectionId",
                table: "notifications",
                column: "SubsectionId",
                principalTable: "subsections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_partners_subsections_SubsectionId",
                table: "partners",
                column: "SubsectionId",
                principalTable: "subsections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_posts_subsections_SubsectionId",
                table: "posts",
                column: "SubsectionId",
                principalTable: "subsections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reservations_subsections_SubsectionId",
                table: "reservations",
                column: "SubsectionId",
                principalTable: "subsections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_rooms_subsections_SubsectionId",
                table: "rooms",
                column: "SubsectionId",
                principalTable: "subsections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_social_accounts_subsections_SubsectionId",
                table: "social_accounts",
                column: "SubsectionId",
                principalTable: "subsections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_specialties_subsections_SubsectionId",
                table: "specialties",
                column: "SubsectionId",
                principalTable: "subsections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_admin_users_subsections_SubsectionId",
                table: "admin_users");

            migrationBuilder.DropForeignKey(
                name: "FK_articles_subsections_SubsectionId",
                table: "articles");

            migrationBuilder.DropForeignKey(
                name: "FK_banners_subsections_SubsectionId",
                table: "banners");

            migrationBuilder.DropForeignKey(
                name: "FK_elections_subsections_SubsectionId",
                table: "elections");

            migrationBuilder.DropForeignKey(
                name: "FK_lawyers_subsections_SubsectionId",
                table: "lawyers");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_subsections_SubsectionId",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_partners_subsections_SubsectionId",
                table: "partners");

            migrationBuilder.DropForeignKey(
                name: "FK_posts_subsections_SubsectionId",
                table: "posts");

            migrationBuilder.DropForeignKey(
                name: "FK_reservations_subsections_SubsectionId",
                table: "reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_rooms_subsections_SubsectionId",
                table: "rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_social_accounts_subsections_SubsectionId",
                table: "social_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_specialties_subsections_SubsectionId",
                table: "specialties");

            migrationBuilder.DropTable(
                name: "platform_admins");

            migrationBuilder.DropTable(
                name: "subsections");

            migrationBuilder.DropIndex(
                name: "IX_specialties_SubsectionId_Name",
                table: "specialties");

            migrationBuilder.DropIndex(
                name: "IX_social_accounts_SubsectionId",
                table: "social_accounts");

            migrationBuilder.DropIndex(
                name: "IX_rooms_SubsectionId",
                table: "rooms");

            migrationBuilder.DropIndex(
                name: "IX_reservations_SubsectionId",
                table: "reservations");

            migrationBuilder.DropIndex(
                name: "IX_posts_SubsectionId",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "IX_partners_SubsectionId",
                table: "partners");

            migrationBuilder.DropIndex(
                name: "IX_notifications_SubsectionId",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_lawyers_SubsectionId_OabNumber",
                table: "lawyers");

            migrationBuilder.DropIndex(
                name: "IX_elections_SubsectionId",
                table: "elections");

            migrationBuilder.DropIndex(
                name: "IX_banners_SubsectionId",
                table: "banners");

            migrationBuilder.DropIndex(
                name: "IX_articles_SubsectionId_Slug",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "IX_admin_users_SubsectionId_Email",
                table: "admin_users");

            migrationBuilder.DropColumn(
                name: "SubsectionId",
                table: "specialties");

            migrationBuilder.DropColumn(
                name: "SubsectionId",
                table: "social_accounts");

            migrationBuilder.DropColumn(
                name: "SubsectionId",
                table: "rooms");

            migrationBuilder.DropColumn(
                name: "SubsectionId",
                table: "reservations");

            migrationBuilder.DropColumn(
                name: "SubsectionId",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "SubsectionId",
                table: "partners");

            migrationBuilder.DropColumn(
                name: "SubsectionId",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "SubsectionId",
                table: "lawyers");

            migrationBuilder.DropColumn(
                name: "SubsectionId",
                table: "elections");

            migrationBuilder.DropColumn(
                name: "SubsectionId",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "SubsectionId",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "SubsectionId",
                table: "admin_users");

            migrationBuilder.CreateTable(
                name: "subsection_info",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Hours = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subsection_info", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_specialties_Name",
                table: "specialties",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lawyers_OabNumber",
                table: "lawyers",
                column: "OabNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_articles_Slug",
                table: "articles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_Email",
                table: "admin_users",
                column: "Email",
                unique: true);
        }
    }
}
