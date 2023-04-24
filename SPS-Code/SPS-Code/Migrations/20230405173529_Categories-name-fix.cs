using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPSCode.Migrations
{
    /// <inheritdoc />
    public partial class Categoriesnamefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "UserCategories");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserCategories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserCategories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "UserCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
