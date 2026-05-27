using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spectrum.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAnalyticsIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_reviews_user_id",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_game_clips_user_id",
                table: "game_clips");

            migrationBuilder.CreateIndex(
                name: "IX_users_created_at",
                table: "users",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_game_id_created_at",
                table: "reviews",
                columns: new[] { "game_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_reviews_media_type_created_at_likes_count",
                table: "reviews",
                columns: new[] { "media_type", "created_at", "likes_count" });

            migrationBuilder.CreateIndex(
                name: "IX_reviews_user_id_created_at",
                table: "reviews",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_game_clips_user_id_created_at",
                table: "game_clips",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_admin_details_rfc",
                table: "admin_details",
                column: "rfc",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_created_at",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_username",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_reviews_game_id_created_at",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_reviews_media_type_created_at_likes_count",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_reviews_user_id_created_at",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_game_clips_user_id_created_at",
                table: "game_clips");

            migrationBuilder.DropIndex(
                name: "IX_admin_details_rfc",
                table: "admin_details");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_user_id",
                table: "reviews",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_clips_user_id",
                table: "game_clips",
                column: "user_id");
        }
    }
}
