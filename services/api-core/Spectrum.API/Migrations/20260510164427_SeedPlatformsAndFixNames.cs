using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Spectrum.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedPlatformsAndFixNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO platforms (id, name) VALUES (1, 'PC') ON CONFLICT (id) DO NOTHING;
                INSERT INTO platforms (id, name) VALUES (2, 'PlayStation') ON CONFLICT (id) DO NOTHING;
                INSERT INTO platforms (id, name) VALUES (3, 'Xbox') ON CONFLICT (id) DO NOTHING;
                INSERT INTO platforms (id, name) VALUES (4, 'Nintendo') ON CONFLICT (id) DO NOTHING;
                INSERT INTO platforms (id, name) VALUES (5, 'Phone') ON CONFLICT (id) DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "platforms",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "platforms",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "platforms",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "platforms",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "platforms",
                keyColumn: "id",
                keyValue: 5);
        }
    }
}
