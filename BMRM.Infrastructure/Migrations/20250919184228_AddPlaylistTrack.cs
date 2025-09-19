using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaylistTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    SpotifyTrackId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_SpotifyTracks_SpotifyTrackId",
                        column: x => x.SpotifyTrackId,
                        principalTable: "SpotifyTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_SpotifyTrackId",
                table: "Playlists",
                column: "SpotifyTrackId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Playlists");
        }
    }
}
