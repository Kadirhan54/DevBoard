using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedTenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "identity",
                table: "Tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                schema: "identity",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "identity",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "identity",
                table: "Projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                schema: "identity",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "identity",
                table: "Boards",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                schema: "identity",
                table: "Boards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "identity",
                table: "Boards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "identity",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "identity",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "identity",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "identity",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "identity",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "identity",
                table: "Boards");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "identity",
                table: "Boards");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "identity",
                table: "Boards");
        }
    }
}
