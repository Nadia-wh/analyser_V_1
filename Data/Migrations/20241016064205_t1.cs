using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace demo_analyser.Data.Migrations
{
    /// <inheritdoc />
    public partial class t1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AnswerSheets",
                newName: "Upload_Status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AnswerSheets",
                newName: "AP_Id");

            migrationBuilder.AddColumn<string>(
                name: "AP_Link",
                table: "AnswerSheets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AP_Link",
                table: "AnswerSheets");

            migrationBuilder.RenameColumn(
                name: "Upload_Status",
                table: "AnswerSheets",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "AP_Id",
                table: "AnswerSheets",
                newName: "Id");
        }
    }
}
