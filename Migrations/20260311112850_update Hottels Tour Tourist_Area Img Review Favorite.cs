using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class updateHottelsTourTourist_AreaImgReviewFavorite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hottels_users_UserId",
                table: "hottels");

            migrationBuilder.DropForeignKey(
                name: "FK_tourist_areas_users_UserId",
                table: "tourist_areas");

            migrationBuilder.DropForeignKey(
                name: "FK_Tours_users_UserId",
                table: "Tours");

            migrationBuilder.DropIndex(
                name: "IX_Tours_UserId",
                table: "Tours");

            migrationBuilder.DropIndex(
                name: "IX_tourist_areas_UserId",
                table: "tourist_areas");

            migrationBuilder.DropIndex(
                name: "IX_hottels_UserId",
                table: "hottels");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "tourist_areas");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "hottels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Tours",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "tourist_areas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "hottels",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tours_UserId",
                table: "Tours",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tourist_areas_UserId",
                table: "tourist_areas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_hottels_UserId",
                table: "hottels",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_hottels_users_UserId",
                table: "hottels",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tourist_areas_users_UserId",
                table: "tourist_areas",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tours_users_UserId",
                table: "Tours",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id");
        }
    }
}
