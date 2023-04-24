using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPSCode.Migrations
{
    /// <inheritdoc />
    public partial class TaskUserCats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskModelUserCategory",
                columns: table => new
                {
                    TaskModelId = table.Column<int>(type: "int", nullable: false),
                    ViewUserCategoriesID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskModelUserCategory", x => new { x.TaskModelId, x.ViewUserCategoriesID });
                    table.ForeignKey(
                        name: "FK_TaskModelUserCategory_Tasks_TaskModelId",
                        column: x => x.TaskModelId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskModelUserCategory_UserCategories_ViewUserCategoriesID",
                        column: x => x.ViewUserCategoriesID,
                        principalTable: "UserCategories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskModelUserCategory_ViewUserCategoriesID",
                table: "TaskModelUserCategory",
                column: "ViewUserCategoriesID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskModelUserCategory");
        }
    }
}
