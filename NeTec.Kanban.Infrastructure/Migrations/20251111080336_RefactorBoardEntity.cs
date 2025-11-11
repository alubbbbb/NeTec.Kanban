using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeTec.Kanban.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorBoardEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Boards_AspNetUsers_UserId",
                table: "Boards");

            migrationBuilder.DropIndex(
                name: "IX_Boards_UserId",
                table: "Boards");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Boards",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boards_ApplicationUserId",
                table: "Boards",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Boards_AspNetUsers_ApplicationUserId",
                table: "Boards",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Boards_AspNetUsers_ApplicationUserId",
                table: "Boards");

            migrationBuilder.DropIndex(
                name: "IX_Boards_ApplicationUserId",
                table: "Boards");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Boards");

            migrationBuilder.CreateIndex(
                name: "IX_Boards_UserId",
                table: "Boards",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Boards_AspNetUsers_UserId",
                table: "Boards",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
