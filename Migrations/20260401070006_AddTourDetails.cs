using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTourDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DepartureLatitude",
                table: "tours",
                type: "numeric(18,10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartureLocationName",
                table: "tours",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DepartureLongitude",
                table: "tours",
                type: "numeric(18,10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "tours",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TourType",
                table: "tours",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vehicle",
                table: "tours",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartureLatitude",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "DepartureLocationName",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "DepartureLongitude",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "TourType",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "Vehicle",
                table: "tours");
        }
    }
}
