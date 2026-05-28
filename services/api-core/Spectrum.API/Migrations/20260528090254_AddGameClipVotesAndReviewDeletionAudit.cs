using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spectrum.API.Migrations
{
    /// <inheritdoc />
    public partial class AddGameClipVotesAndReviewDeletionAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "reviews",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by_admin_id",
                table: "reviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deletion_reason",
                table: "reviews",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "game_clip_votes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_positive = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_clip_votes", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_clip_votes_game_clips_clip_id",
                        column: x => x.clip_id,
                        principalTable: "game_clips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_game_clip_votes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_clip_votes_clip_id_is_positive",
                table: "game_clip_votes",
                columns: new[] { "clip_id", "is_positive" });

            migrationBuilder.CreateIndex(
                name: "IX_game_clip_votes_clip_id_user_id",
                table: "game_clip_votes",
                columns: new[] { "clip_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_clip_votes_user_id",
                table: "game_clip_votes",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_clip_votes");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "deleted_by_admin_id",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "deletion_reason",
                table: "reviews");
        }
    }
}
