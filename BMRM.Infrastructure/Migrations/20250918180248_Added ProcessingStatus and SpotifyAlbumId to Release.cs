using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedProcessingStatusandSpotifyAlbumIdtoRelease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcessingStatus",
                table: "Releases",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SpotifyAlbumId",
                table: "Releases",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessingStatus",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "SpotifyAlbumId",
                table: "Releases");
        }
    }
}
