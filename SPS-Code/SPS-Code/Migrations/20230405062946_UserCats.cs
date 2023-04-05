using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPSCode.Migrations
{
    /// <inheritdoc />
    public partial class UserCats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserCategoryID",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserCategoryes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCategoryes", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserCategoryID",
                table: "Users",
                column: "UserCategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserCategoryes_UserCategoryID",
                table: "Users",
                column: "UserCategoryID",
                principalTable: "UserCategoryes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserCategoryes_UserCategoryID",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserCategoryes");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserCategoryID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserCategoryID",
                table: "Users");
        }
    }
}
