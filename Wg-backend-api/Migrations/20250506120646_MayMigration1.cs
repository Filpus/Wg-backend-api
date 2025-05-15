using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wg_backend_api.Migrations
{
    /// <inheritdoc />
    public partial class MayMigration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gameaccess_games_GameId1",
                schema: "Global",
                table: "gameaccess");

            migrationBuilder.DropForeignKey(
                name: "FK_gameaccess_players_fk_Users",
                schema: "Global",
                table: "gameaccess");

            migrationBuilder.DropTable(
                name: "players",
                schema: "Global");

            migrationBuilder.DropIndex(
                name: "IX_gameaccess_GameId1",
                schema: "Global",
                table: "gameaccess");

            migrationBuilder.DropColumn(
                name: "GameId1",
                schema: "Global",
                table: "gameaccess");

            migrationBuilder.AddForeignKey(
                name: "FK_gameaccess_users_fk_Users",
                schema: "Global",
                table: "gameaccess",
                column: "fk_Users",
                principalSchema: "Global",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gameaccess_users_fk_Users",
                schema: "Global",
                table: "gameaccess");

            migrationBuilder.AddColumn<int>(
                name: "GameId1",
                schema: "Global",
                table: "gameaccess",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "players",
                schema: "Global",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_User = table.Column<int>(type: "integer", nullable: false),
                    playerType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.id);
                    table.ForeignKey(
                        name: "FK_players_users_fk_User",
                        column: x => x.fk_User,
                        principalSchema: "Global",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gameaccess_GameId1",
                schema: "Global",
                table: "gameaccess",
                column: "GameId1");

            migrationBuilder.CreateIndex(
                name: "IX_players_fk_User",
                schema: "Global",
                table: "players",
                column: "fk_User");

            migrationBuilder.AddForeignKey(
                name: "FK_gameaccess_games_GameId1",
                schema: "Global",
                table: "gameaccess",
                column: "GameId1",
                principalSchema: "Global",
                principalTable: "games",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_gameaccess_players_fk_Users",
                schema: "Global",
                table: "gameaccess",
                column: "fk_Users",
                principalSchema: "Global",
                principalTable: "players",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
