using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GamesPlatform.Migrations
{
    /// <inheritdoc />
    public partial class LibGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_Games_GameId",
                table: "Libraries");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_GameId",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "AddedAt",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Libraries");

            migrationBuilder.CreateTable(
                name: "LibGames",
                columns: table => new
                {
                    LibGameId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LibraryId = table.Column<int>(type: "integer", nullable: false),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibGames", x => x.LibGameId);
                    table.ForeignKey(
                        name: "FK_LibGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibGames_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "LibraryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LibGames_GameId",
                table: "LibGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_LibGames_LibraryId",
                table: "LibGames",
                column: "LibraryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibGames");

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedAt",
                table: "Libraries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "Libraries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Libraries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_GameId",
                table: "Libraries",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Libraries_Games_GameId",
                table: "Libraries",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "GameId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
