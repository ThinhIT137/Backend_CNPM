using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class updatesettingsTourTourist_AreaHottelandupdateTourist_PlaceTour_Itinerary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_hotel_tourist_area",
                table: "hottels");

            migrationBuilder.DropForeignKey(
                name: "FK_Tours_tourist_areas_Tourist_AreaId",
                table: "Tours");

            migrationBuilder.DropIndex(
                name: "IX_Tours_Tourist_AreaId",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "Tourist_AreaId",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "address",
                table: "Tours");

            migrationBuilder.RenameTable(
                name: "Tours",
                newName: "tours");

            migrationBuilder.RenameIndex(
                name: "ix_tours_created_on",
                table: "tours",
                newName: "ix_tours_created_at");

            migrationBuilder.RenameColumn(
                name: "created_on",
                table: "tourist_areas",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "ix_tourist_area_created_on",
                table: "tourist_areas",
                newName: "ix_tourist_area_created_at");

            migrationBuilder.RenameColumn(
                name: "tourist_area_id",
                table: "hottels",
                newName: "tourist_place_id");

            migrationBuilder.RenameColumn(
                name: "created_on",
                table: "hottels",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "ix_hottels_tourist_area_id",
                table: "hottels",
                newName: "ix_hottels_tourist_place_id");

            migrationBuilder.RenameIndex(
                name: "ix_hottels_created_on",
                table: "hottels",
                newName: "ix_hottels_created_at");

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "tours",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "latitude",
                table: "tourist_areas",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "longitude",
                table: "tourist_areas",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "latitude",
                table: "hottels",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "longitude",
                table: "hottels",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tourist_places",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "text", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tourist_area_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    rating_total = table.Column<int>(type: "integer", nullable: false),
                    rating_count = table.Column<int>(type: "integer", nullable: false),
                    rating_average = table.Column<decimal>(type: "numeric", nullable: false),
                    favorite_count = table.Column<int>(type: "integer", nullable: false),
                    click_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tourist_places_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_tourist_place_area",
                        column: x => x.tourist_area_id,
                        principalTable: "tourist_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tourist_place_user",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tour_itineraries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    tour_id = table.Column<int>(type: "integer", nullable: false),
                    tourist_place_id = table.Column<int>(type: "integer", nullable: false),
                    day_number = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tour_itineraries_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_itinerary_place",
                        column: x => x.tourist_place_id,
                        principalTable: "tourist_places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_itinerary_tour",
                        column: x => x.tour_id,
                        principalTable: "tours",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_itinerary_place",
                table: "tour_itineraries",
                column: "tourist_place_id");

            migrationBuilder.CreateIndex(
                name: "ix_itinerary_tour",
                table: "tour_itineraries",
                column: "tour_id");

            migrationBuilder.CreateIndex(
                name: "ix_tourist_places_area",
                table: "tourist_places",
                column: "tourist_area_id");

            migrationBuilder.CreateIndex(
                name: "ix_tourist_places_name",
                table: "tourist_places",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_tourist_places_popular",
                table: "tourist_places",
                columns: new[] { "rating_average", "click_count" });

            migrationBuilder.CreateIndex(
                name: "ix_tourist_places_rating",
                table: "tourist_places",
                column: "rating_average");

            migrationBuilder.CreateIndex(
                name: "ix_tourist_places_user",
                table: "tourist_places",
                column: "created_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_hotel_tourist_place",
                table: "hottels",
                column: "tourist_place_id",
                principalTable: "tourist_places",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_hotel_tourist_place",
                table: "hottels");

            migrationBuilder.DropTable(
                name: "tour_itineraries");

            migrationBuilder.DropTable(
                name: "tourist_places");

            migrationBuilder.DropColumn(
                name: "price",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "tourist_areas");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "tourist_areas");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "hottels");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "hottels");

            migrationBuilder.RenameTable(
                name: "tours",
                newName: "Tours");

            migrationBuilder.RenameIndex(
                name: "ix_tours_created_at",
                table: "Tours",
                newName: "ix_tours_created_on");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "tourist_areas",
                newName: "created_on");

            migrationBuilder.RenameIndex(
                name: "ix_tourist_area_created_at",
                table: "tourist_areas",
                newName: "ix_tourist_area_created_on");

            migrationBuilder.RenameColumn(
                name: "tourist_place_id",
                table: "hottels",
                newName: "tourist_area_id");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "hottels",
                newName: "created_on");

            migrationBuilder.RenameIndex(
                name: "ix_hottels_tourist_place_id",
                table: "hottels",
                newName: "ix_hottels_tourist_area_id");

            migrationBuilder.RenameIndex(
                name: "ix_hottels_created_at",
                table: "hottels",
                newName: "ix_hottels_created_on");

            migrationBuilder.AddColumn<int>(
                name: "Tourist_AreaId",
                table: "Tours",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "Tours",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tours_Tourist_AreaId",
                table: "Tours",
                column: "Tourist_AreaId");

            migrationBuilder.AddForeignKey(
                name: "fk_hotel_tourist_area",
                table: "hottels",
                column: "tourist_area_id",
                principalTable: "tourist_areas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tours_tourist_areas_Tourist_AreaId",
                table: "Tours",
                column: "Tourist_AreaId",
                principalTable: "tourist_areas",
                principalColumn: "id");
        }
    }
}
