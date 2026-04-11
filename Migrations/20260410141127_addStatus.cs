using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class addStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "tours",
                type: "text",
                nullable: true,
                defaultValue: "Available",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "tourist_places",
                type: "text",
                nullable: true,
                defaultValue: "Available");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "tourist_areas",
                type: "text",
                nullable: true,
                defaultValue: "Available");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "tourist_places");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "tourist_areas");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "tours",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "Available");
        }
    }
}
