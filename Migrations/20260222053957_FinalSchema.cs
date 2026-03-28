using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FinalSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hottels_users_UserId1",
                table: "hottels");

            migrationBuilder.DropForeignKey(
                name: "fk_hottels_users",
                table: "hottels");

            migrationBuilder.DropForeignKey(
                name: "FK_tourist_areas_users_UserId1",
                table: "tourist_areas");

            migrationBuilder.DropForeignKey(
                name: "fk_tourist_areas_users",
                table: "tourist_areas");

            migrationBuilder.DropIndex(
                name: "IX_tourist_areas_UserId",
                table: "tourist_areas");

            migrationBuilder.DropIndex(
                name: "IX_tourist_areas_UserId1",
                table: "tourist_areas");

            migrationBuilder.DropIndex(
                name: "IX_hottels_UserId",
                table: "hottels");

            migrationBuilder.DropIndex(
                name: "IX_hottels_UserId1",
                table: "hottels");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "tourist_areas");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "tourist_areas");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "hottels");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "hottels");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "tourist_areas",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Img",
                table: "hottels",
                newName: "img");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "hottels",
                newName: "created_at");

            migrationBuilder.AlterColumn<string>(
                name: "Img",
                table: "tourist_areas",
                type: "text",
                nullable: false,
                comment: "img",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "tourist_areas",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "hottels",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_tourist_areas_CreatedBy",
                table: "tourist_areas",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_hottels_CreatedBy",
                table: "hottels",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_hottels_users_CreatedBy",
                table: "hottels",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tourist_areas_users_CreatedBy",
                table: "tourist_areas",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hottels_users_CreatedBy",
                table: "hottels");

            migrationBuilder.DropForeignKey(
                name: "FK_tourist_areas_users_CreatedBy",
                table: "tourist_areas");

            migrationBuilder.DropIndex(
                name: "IX_tourist_areas_CreatedBy",
                table: "tourist_areas");

            migrationBuilder.DropIndex(
                name: "IX_hottels_CreatedBy",
                table: "hottels");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "tourist_areas",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "img",
                table: "hottels",
                newName: "Img");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "hottels",
                newName: "CreatedOn");

            migrationBuilder.AlterColumn<string>(
                name: "Img",
                table: "tourist_areas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "img");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "tourist_areas",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "tourist_areas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "tourist_areas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "hottels",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "hottels",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "hottels",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tourist_areas_UserId",
                table: "tourist_areas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tourist_areas_UserId1",
                table: "tourist_areas",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_hottels_UserId",
                table: "hottels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_hottels_UserId1",
                table: "hottels",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_hottels_users_UserId1",
                table: "hottels",
                column: "UserId1",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_hottels_users",
                table: "hottels",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tourist_areas_users_UserId1",
                table: "tourist_areas",
                column: "UserId1",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_tourist_areas_users",
                table: "tourist_areas",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
