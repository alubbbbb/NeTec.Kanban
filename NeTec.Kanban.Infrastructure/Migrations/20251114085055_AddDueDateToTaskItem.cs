using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeTec.Kanban.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDueDateToTaskItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "TaskItems",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "TaskItems");
        }

    }
}
