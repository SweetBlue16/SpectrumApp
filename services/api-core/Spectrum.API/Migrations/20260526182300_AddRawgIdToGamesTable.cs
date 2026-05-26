using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spectrum.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRawgIdToGamesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "rawg_id",
                table: "games",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                DO $$
                DECLARE 
                    r RECORD;
                    counter INT := 1;
                BEGIN
                    FOR r IN SELECT id FROM games LOOP
                        UPDATE games SET rawg_id = counter WHERE id = r.id;
                        counter := counter + 1;
                    END LOOP;
                END $$;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_games_rawg_id",
                table: "games",
                column: "rawg_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_games_rawg_id",
                table: "games");

            migrationBuilder.DropColumn(
                name: "rawg_id",
                table: "games");
        }
    }
}
