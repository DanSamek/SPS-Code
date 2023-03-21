using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPSCode.Migrations
{
    /// <inheritdoc />
    public partial class TaskInputsAndOutputs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Inputs",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Outputs",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Inputs",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Outputs",
                table: "Tasks");
        }
    }
}
