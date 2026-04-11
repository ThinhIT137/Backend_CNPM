using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTouristPlaceToMarker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Tourist_PlaceId",
                table: "markers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tourist_place_id",
                table: "markers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_markers_Tourist_PlaceId",
                table: "markers",
                column: "Tourist_PlaceId");

            migrationBuilder.CreateIndex(
                name: "ix_markers_touristPlace",
                table: "markers",
                column: "tourist_place_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Markers_TouristPlace",
                table: "markers",
                column: "tourist_place_id",
                principalTable: "tourist_places",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_markers_tourist_places_Tourist_PlaceId",
                table: "markers",
                column: "Tourist_PlaceId",
                principalTable: "tourist_places",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Markers_TouristPlace",
                table: "markers");

            migrationBuilder.DropForeignKey(
                name: "FK_markers_tourist_places_Tourist_PlaceId",
                table: "markers");

            migrationBuilder.DropIndex(
                name: "IX_markers_Tourist_PlaceId",
                table: "markers");

            migrationBuilder.DropIndex(
                name: "ix_markers_touristPlace",
                table: "markers");

            migrationBuilder.DropColumn(
                name: "Tourist_PlaceId",
                table: "markers");

            migrationBuilder.DropColumn(
                name: "tourist_place_id",
                table: "markers");
        }
    }
}
