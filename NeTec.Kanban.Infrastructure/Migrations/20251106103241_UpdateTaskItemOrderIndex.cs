using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeTec.Kanban.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaskItemOrderIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "TaskItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "TaskItems");
        }
    }
}
