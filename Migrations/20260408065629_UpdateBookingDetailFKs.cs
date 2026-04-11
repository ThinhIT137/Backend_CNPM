using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookingDetailFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "fk_booking_detail_hotel_room",
                table: "booking_details",
                column: "hotel_room_id",
                principalTable: "hotel_rooms",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_booking_detail_tour_departure",
                table: "booking_details",
                column: "tour_departure_id",
                principalTable: "tour_departures",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_booking_detail_hotel_room",
                table: "booking_details");

            migrationBuilder.DropForeignKey(
                name: "fk_booking_detail_tour_departure",
                table: "booking_details");
        }
    }
}
