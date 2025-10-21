using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TenantDomainChangedNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Domain",
                schema: "identity",
                table: "Tenant");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Domain",
                schema: "identity",
                table: "Tenant",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
