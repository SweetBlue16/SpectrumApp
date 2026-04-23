using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spectrum.API.Migrations
{
    /// <inheritdoc />
    public partial class FixUserIdColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_admin_details_users_UserId",
                table: "admin_details");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "admin_details",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_admin_details_UserId",
                table: "admin_details",
                newName: "IX_admin_details_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_admin_details_users_user_id",
                table: "admin_details",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_admin_details_users_user_id",
                table: "admin_details");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "admin_details",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_admin_details_user_id",
                table: "admin_details",
                newName: "IX_admin_details_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_admin_details_users_UserId",
                table: "admin_details",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
