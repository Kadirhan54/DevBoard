using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevBoard.Services.Tasks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentsForDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_Comments_CommentId",
                table: "Attachment");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_Tasks_TaskItemId",
                table: "Attachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attachment",
                table: "Attachment");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "Tasks",
                newName: "Tasks",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Comments",
                newName: "Comments",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Attachment",
                newName: "Attachments");

            migrationBuilder.RenameIndex(
                name: "IX_Attachment_TaskItemId",
                table: "Attachments",
                newName: "IX_Attachments_TaskItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachment_CommentId",
                table: "Attachments",
                newName: "IX_Attachments_CommentId");

            migrationBuilder.AlterColumn<string>(
                name: "StoragePath",
                table: "Attachments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Attachments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Attachments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "Attachments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attachments",
                table: "Attachments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_TenantId",
                table: "Attachments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_UploadedAt",
                table: "Attachments",
                column: "UploadedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Comments_CommentId",
                table: "Attachments",
                column: "CommentId",
                principalSchema: "public",
                principalTable: "Comments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Tasks_TaskItemId",
                table: "Attachments",
                column: "TaskItemId",
                principalSchema: "public",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Comments_CommentId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Tasks_TaskItemId",
                table: "Attachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attachments",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_TenantId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_UploadedAt",
                table: "Attachments");

            migrationBuilder.RenameTable(
                name: "Tasks",
                schema: "public",
                newName: "Tasks");

            migrationBuilder.RenameTable(
                name: "Comments",
                schema: "public",
                newName: "Comments");

            migrationBuilder.RenameTable(
                name: "Attachments",
                newName: "Attachment");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_TaskItemId",
                table: "Attachment",
                newName: "IX_Attachment_TaskItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_CommentId",
                table: "Attachment",
                newName: "IX_Attachment_CommentId");

            migrationBuilder.AlterColumn<string>(
                name: "StoragePath",
                table: "Attachment",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Attachment",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Attachment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "Attachment",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attachment",
                table: "Attachment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_Comments_CommentId",
                table: "Attachment",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_Tasks_TaskItemId",
                table: "Attachment",
                column: "TaskItemId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
