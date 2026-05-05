using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spectrum.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewCountersAndUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE reviews DROP CONSTRAINT IF EXISTS ""FK_reviews_games_GameId1"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_reviews_GameId1"";");
            migrationBuilder.Sql(@"ALTER TABLE reviews DROP COLUMN IF EXISTS ""GameId1"";");
            migrationBuilder.Sql(@"ALTER TABLE reviews ADD COLUMN IF NOT EXISTS likes_count integer NOT NULL DEFAULT 0;");
            migrationBuilder.Sql(@"ALTER TABLE reviews ADD COLUMN IF NOT EXISTS dislikes_count integer NOT NULL DEFAULT 0;");
            migrationBuilder.Sql(@"ALTER TABLE reviews ADD COLUMN IF NOT EXISTS updated_at timestamp with time zone NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE reviews DROP COLUMN IF EXISTS updated_at;");
            migrationBuilder.Sql(@"ALTER TABLE reviews DROP COLUMN IF EXISTS dislikes_count;");
            migrationBuilder.Sql(@"ALTER TABLE reviews DROP COLUMN IF EXISTS likes_count;");
        }
    }
}
