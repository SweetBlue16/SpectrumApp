using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spectrum.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewTitleAndMediaType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "media_type",
                table: "reviews",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "reviews",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "Resena existente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "media_type",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "title",
                table: "reviews");
        }
    }
}
