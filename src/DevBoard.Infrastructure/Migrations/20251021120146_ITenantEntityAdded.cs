using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ITenantEntityAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                schema: "identity",
                table: "Tasks",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                schema: "identity",
                table: "Projects",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                schema: "identity",
                table: "Boards",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "identity",
                table: "AspNetUsers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Tenant",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TenantId",
                schema: "identity",
                table: "Tasks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TenantId",
                schema: "identity",
                table: "Projects",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Boards_TenantId",
                schema: "identity",
                table: "Boards",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TenantId",
                schema: "identity",
                table: "AspNetUsers",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tenant_TenantId",
                schema: "identity",
                table: "AspNetUsers",
                column: "TenantId",
                principalSchema: "identity",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Boards_Tenant_TenantId",
                schema: "identity",
                table: "Boards",
                column: "TenantId",
                principalSchema: "identity",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Tenant_TenantId",
                schema: "identity",
                table: "Projects",
                column: "TenantId",
                principalSchema: "identity",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tenant_TenantId",
                schema: "identity",
                table: "Tasks",
                column: "TenantId",
                principalSchema: "identity",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tenant_TenantId",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Boards_Tenant_TenantId",
                schema: "identity",
                table: "Boards");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Tenant_TenantId",
                schema: "identity",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tenant_TenantId",
                schema: "identity",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "Tenant",
                schema: "identity");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_TenantId",
                schema: "identity",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TenantId",
                schema: "identity",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Boards_TenantId",
                schema: "identity",
                table: "Boards");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TenantId",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "identity",
                table: "Tasks",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "identity",
                table: "Projects",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "identity",
                table: "Boards",
                newName: "CreatedOn");
        }
    }
}
