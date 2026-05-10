using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Spectrum.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "biography",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameUser",
                columns: table => new
                {
                    InterestedGamesId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterestedUsersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameUser", x => new { x.InterestedGamesId, x.InterestedUsersId });
                    table.ForeignKey(
                        name: "FK_GameUser_games_InterestedGamesId",
                        column: x => x.InterestedGamesId,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameUser_users_InterestedUsersId",
                        column: x => x.InterestedUsersId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "platforms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platforms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformUser",
                columns: table => new
                {
                    InterestedUsersId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformUser", x => new { x.InterestedUsersId, x.PlatformsId });
                    table.ForeignKey(
                        name: "FK_PlatformUser_platforms_PlatformsId",
                        column: x => x.PlatformsId,
                        principalTable: "platforms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlatformUser_users_InterestedUsersId",
                        column: x => x.InterestedUsersId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameUser_InterestedUsersId",
                table: "GameUser",
                column: "InterestedUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformUser_PlatformsId",
                table: "PlatformUser",
                column: "PlatformsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameUser");

            migrationBuilder.DropTable(
                name: "PlatformUser");

            migrationBuilder.DropTable(
                name: "platforms");

            migrationBuilder.DropColumn(
                name: "biography",
                table: "users");
        }
    }
}
