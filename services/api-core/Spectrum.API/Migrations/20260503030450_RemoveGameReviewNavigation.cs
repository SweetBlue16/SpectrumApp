using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spectrum.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGameReviewNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE reviews DROP CONSTRAINT IF EXISTS ""FK_reviews_games_GameId1"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_reviews_GameId1"";");
            migrationBuilder.Sql(@"ALTER TABLE reviews DROP COLUMN IF EXISTS ""GameId1"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GameId1",
                table: "reviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reviews_GameId1",
                table: "reviews",
                column: "GameId1");

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_games_GameId1",
                table: "reviews",
                column: "GameId1",
                principalTable: "games",
                principalColumn: "id");
        }
    }
}