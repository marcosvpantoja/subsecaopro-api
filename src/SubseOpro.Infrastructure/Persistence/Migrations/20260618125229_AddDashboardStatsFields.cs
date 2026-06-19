using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubseOpro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardStatsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "partners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "partners",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() AT TIME ZONE 'UTC'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "lawyers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() AT TIME ZONE 'UTC'");

            migrationBuilder.Sql("""
                UPDATE partners
                SET "CouponCode" = UPPER(LEFT(REGEXP_REPLACE("Name", '[^a-zA-Z0-9]', '', 'g'), 10))
                WHERE "CouponCode" = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "partners");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "partners");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "lawyers");
        }
    }
}
