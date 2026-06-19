using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubseOpro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalAccessTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActiveAiTokenEncrypted",
                table: "subsections",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActiveAiTokenId",
                table: "subsections",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiServiceUrl",
                table: "subsections",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "personal_access_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SubsectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personal_access_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_personal_access_tokens_subsections_SubsectionId",
                        column: x => x.SubsectionId,
                        principalTable: "subsections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_personal_access_tokens_SubsectionId",
                table: "personal_access_tokens",
                column: "SubsectionId");

            migrationBuilder.CreateIndex(
                name: "IX_personal_access_tokens_TokenHash",
                table: "personal_access_tokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "personal_access_tokens");

            migrationBuilder.DropColumn(
                name: "ActiveAiTokenEncrypted",
                table: "subsections");

            migrationBuilder.DropColumn(
                name: "ActiveAiTokenId",
                table: "subsections");

            migrationBuilder.DropColumn(
                name: "AiServiceUrl",
                table: "subsections");
        }
    }
}
