using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeTec.Kanban.Infrastructure.Migrations
{
    public partial class AddDomainEntitiesValidations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //
            // Drop FKs that reference the old column names - these will be re-added later after rename
            //
            migrationBuilder.DropForeignKey(
                name: "FK_Boards_AspNetUsers_UserId",
                table: "Boards");

            migrationBuilder.DropForeignKey(
                name: "FK_Columns_Boards_BoardID",
                table: "Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_UserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_TaskItems_TaskItemID",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_AspNetUsers_UserId",
                table: "TaskItems");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_Columns_ColumnID",
                table: "TaskItems");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeTrackings_AspNetUsers_UserId",
                table: "TimeTrackings");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeTrackings_TaskItems_TaskItemID",
                table: "TimeTrackings");

            //
            // Rename columns that used the old "<Entity>NameID" convention to "Id" and adjust indexes
            //
            migrationBuilder.RenameColumn(
                name: "TaskItemID",
                table: "TimeTrackings",
                newName: "TaskItemId");

            migrationBuilder.RenameColumn(
                name: "TimeTrackingID",
                table: "TimeTrackings",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_TimeTrackings_TaskItemID",
                table: "TimeTrackings",
                newName: "IX_TimeTrackings_TaskItemId");

            migrationBuilder.RenameColumn(
                name: "ColumnID",
                table: "TaskItems",
                newName: "ColumnId");

            migrationBuilder.RenameColumn(
                name: "TaskItemID",
                table: "TaskItems",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_TaskItems_ColumnID",
                table: "TaskItems",
                newName: "IX_TaskItems_ColumnId");

            migrationBuilder.RenameColumn(
                name: "TaskItemID",
                table: "Comments",
                newName: "TaskItemId");

            migrationBuilder.RenameColumn(
                name: "CommentID",
                table: "Comments",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_TaskItemID",
                table: "Comments",
                newName: "IX_Comments_TaskItemId");

            migrationBuilder.RenameColumn(
                name: "BoardID",
                table: "Columns",
                newName: "BoardId");

            migrationBuilder.RenameColumn(
                name: "ColumnID",
                table: "Columns",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Columns_BoardID",
                table: "Columns",
                newName: "IX_Columns_BoardId");

            migrationBuilder.RenameColumn(
                name: "BoardID",
                table: "Boards",
                newName: "Id");

            //
            // Alter columns: adjust lengths / types to match the model
            // (Keep changes minimal and safe where possible)
            //
            migrationBuilder.AlterColumn<decimal>(
                name: "HoursSpent",
                table: "TimeTrackings",
                type: "decimal(8,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TimeTrackings",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // Make TaskItems.UserId nullable in model, so allow nullable in DB
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TaskItems",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // UpdatedAt on TaskItems: allow null (nullable in model)
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TaskItems",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            // Title length constraint
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "TaskItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "RemainingHours",
                table: "TaskItems",
                type: "decimal(8,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            //
            // Replace a direct AlterColumn for Priority with a safe conversion:
            //  - add temporary nvarchar column Priority_tmp
            //  - migrate existing values
            //  - drop old Priority
            //  - rename Priority_tmp -> Priority
            //
            migrationBuilder.AddColumn<string>(
                name: "Priority_tmp",
                table: "TaskItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Medium");

            // Migrate values: try cast int -> map codes; if already string, preserve basic values
            migrationBuilder.Sql(@"
                UPDATE TaskItems
                SET Priority_tmp =
                    CASE
                        WHEN TRY_CAST(Priority AS int) = 0 THEN 'Low'
                        WHEN TRY_CAST(Priority AS int) = 1 THEN 'Medium'
                        WHEN TRY_CAST(Priority AS int) = 2 THEN 'High'
                        WHEN Priority IN ('Low','Medium','High') THEN Priority
                        ELSE 'Medium'
                    END
            ");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "TaskItems");

            migrationBuilder.RenameColumn(
                name: "Priority_tmp",
                table: "TaskItems",
                newName: "Priority");

            // EstimatedHours precision
            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedHours",
                table: "TaskItems",
                type: "decimal(8,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            // Description length
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TaskItems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // Comments.Content
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Columns.Name
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Columns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Boards UpdatedAt nullable
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Boards",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            // Boards.Name length
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Boards",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Boards.Description length
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Boards",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            //
            // Identity fields sizing adjustments
            //
            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "AspNetUsers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            //
            // Add new AspNetUsers fields: CreatedAt (default GETUTCDATE), FullName, UpdatedAt
            //
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            //
            // Roles length changes
            //
            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "AspNetRoles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetRoles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            //
            // Recreate foreign keys with the new column names
            //
            migrationBuilder.AddForeignKey(
                name: "FK_Boards_AspNetUsers_UserId",
                table: "Boards",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Columns_Boards_BoardId",
                table: "Columns",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_UserId",
                table: "Comments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_TaskItems_TaskItemId",
                table: "Comments",
                column: "TaskItemId",
                principalTable: "TaskItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_AspNetUsers_UserId",
                table: "TaskItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_Columns_ColumnId",
                table: "TaskItems",
                column: "ColumnId",
                principalTable: "Columns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTrackings_AspNetUsers_UserId",
                table: "TimeTrackings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTrackings_TaskItems_TaskItemId",
                table: "TimeTrackings",
                column: "TaskItemId",
                principalTable: "TaskItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //
            // Remove added foreign keys (so we can rename back)
            //
            migrationBuilder.DropForeignKey(
                name: "FK_Boards_AspNetUsers_UserId",
                table: "Boards");

            migrationBuilder.DropForeignKey(
                name: "FK_Columns_Boards_BoardId",
                table: "Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_UserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_TaskItems_TaskItemId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_AspNetUsers_UserId",
                table: "TaskItems");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_Columns_ColumnId",
                table: "TaskItems");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeTrackings_AspNetUsers_UserId",
                table: "TimeTrackings");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeTrackings_TaskItems_TaskItemId",
                table: "TimeTrackings");

            //
            // Drop added AspNetUsers columns
            //
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspNetUsers");

            //
            // Revert Identity role sizing
            //
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetRoles",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "AspNetRoles",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            //
            // Revert Columns.Name
            //
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Columns",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            //
            // Revert Comments.Content
            //
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            //
            // Revert TaskItems.Description
            //
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TaskItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            // Revert EstimatedHours precision
            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedHours",
                table: "TaskItems",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldNullable: true);

            // Recreate old Priority as int column if possible
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "TaskItems",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // If there were values migrated to string, attempt to map back; using CASE with TRY_PARSE to be safe
            migrationBuilder.Sql(@"
                UPDATE TaskItems
                SET Priority =
                    CASE
                        WHEN LOWER(Priority) = 'low' THEN 0
                        WHEN LOWER(Priority) = 'high' THEN 2
                        ELSE 1
                    END
                WHERE ISNUMERIC(Priority) = 0 OR TRY_CAST(Priority AS INT) IS NULL;
            ");

            // If a string->int conversion was possible for some rows, we try to cast them
            migrationBuilder.Sql(@"
                UPDATE TaskItems
                SET Priority = TRY_CAST(Priority AS INT)
                WHERE TRY_CAST(Priority AS INT) IS NOT NULL;
            ");

            // Finally drop the temporary (string) Priority column if exists (this code assumes we dropped it earlier when migrating up)
            // In down path we do not have a migrationBuilder.DropColumn for the string Priority because AddColumn re-added int Priority
            // If the string Priority column exists with name "Priority" because Up renamed Priority_tmp -> Priority, we need to rename back:
            // We'll rename the current string "Priority" to "Priority_tmp_str" and re-create the int "Priority" above. Then drop the string column.
            // Check and perform drop if necessary via SQL:
            migrationBuilder.Sql(@"
                IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'Priority' AND Object_ID(N'dbo.TaskItems') = OBJECT_ID(N'dbo.TaskItems') AND TYPE_NAME(user_type_id) = 'nvarchar')
                BEGIN
                    ALTER TABLE dbo.TaskItems DROP COLUMN Priority;
                END
            ");

            //
            // Revert RemainingHours precision
            //
            migrationBuilder.AlterColumn<decimal>(
                name: "RemainingHours",
                table: "TaskItems",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldNullable: true);

            //
            // Revert Title to nvarchar(max)
            //
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "TaskItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            //
            // Revert TaskItems.UpdatedAt not-nullable change
            //
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TaskItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            //
            // Revert TaskItems.UserId nullability
            //
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TaskItems",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            //
            // Revert TimeTrackings Description
            //
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TimeTrackings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            //
            // Revert HoursSpent precision
            //
            migrationBuilder.AlterColumn<decimal>(
                name: "HoursSpent",
                table: "TimeTrackings",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)");

            //
            // Rename columns back to original names
            //
            migrationBuilder.RenameColumn(
                name: "TaskItemId",
                table: "TimeTrackings",
                newName: "TaskItemID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TimeTrackings",
                newName: "TimeTrackingID");

            migrationBuilder.RenameIndex(
                name: "IX_TimeTrackings_TaskItemId",
                table: "TimeTrackings",
                newName: "IX_TimeTrackings_TaskItemID");

            migrationBuilder.RenameColumn(
                name: "ColumnId",
                table: "TaskItems",
                newName: "ColumnID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TaskItems",
                newName: "TaskItemID");

            migrationBuilder.RenameIndex(
                name: "IX_TaskItems_ColumnId",
                table: "TaskItems",
                newName: "IX_TaskItems_ColumnID");

            migrationBuilder.RenameColumn(
                name: "TaskItemId",
                table: "Comments",
                newName: "TaskItemID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Comments",
                newName: "CommentID");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_TaskItemId",
                table: "Comments",
                newName: "IX_Comments_TaskItemID");

            migrationBuilder.RenameColumn(
                name: "BoardId",
                table: "Columns",
                newName: "BoardID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Columns",
                newName: "ColumnID");

            migrationBuilder.RenameIndex(
                name: "IX_Columns_BoardId",
                table: "Columns",
                newName: "IX_Columns_BoardID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Boards",
                newName: "BoardID");

            //
            // Recreate original foreign keys (names from initial schema)
            //
            migrationBuilder.AddForeignKey(
                name: "FK_Boards_AspNetUsers_UserId",
                table: "Boards",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Columns_Boards_BoardID",
                table: "Columns",
                column: "BoardID",
                principalTable: "Boards",
                principalColumn: "BoardID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_UserId",
                table: "Comments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_TaskItems_TaskItemID",
                table: "Comments",
                column: "TaskItemID",
                principalTable: "TaskItems",
                principalColumn: "TaskItemID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_AspNetUsers_UserId",
                table: "TaskItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_Columns_ColumnID",
                table: "TaskItems",
                column: "ColumnID",
                principalTable: "Columns",
                principalColumn: "ColumnID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTrackings_AspNetUsers_UserId",
                table: "TimeTrackings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTrackings_TaskItems_TaskItemID",
                table: "TimeTrackings",
                column: "TaskItemID",
                principalTable: "TaskItems",
                principalColumn: "TaskItemID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}