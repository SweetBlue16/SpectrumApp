using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spectrum.API.Migrations
{
    public partial class ChangeReviewGameIdToInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE reviews DROP CONSTRAINT IF EXISTS ""FK_reviews_games_game_id"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_reviews_game_id"";");

            migrationBuilder.DropColumn(
                name: "game_id",
                table: "reviews");

            migrationBuilder.AddColumn<int>(
                name: "game_id",
                table: "reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                table: "reviews",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "game_id",
                table: "reviews");

            migrationBuilder.AddColumn<Guid>(
                name: "game_id",
                table: "reviews",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.CreateIndex(
                name: "IX_reviews_game_id",
                table: "reviews",
                column: "game_id");

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_games_game_id",
                table: "reviews",
                column: "game_id",
                principalTable: "games",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                table: "reviews",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}