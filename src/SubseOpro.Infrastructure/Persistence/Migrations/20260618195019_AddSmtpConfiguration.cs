using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubseOpro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSmtpConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "smtp_configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Host = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Password = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FromEmail = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FromName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UseTls = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_smtp_configurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_smtp_configurations_UpdatedAt",
                table: "smtp_configurations",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "smtp_configurations");
        }
    }
}
