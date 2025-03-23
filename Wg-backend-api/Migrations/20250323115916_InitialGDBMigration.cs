using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wg_backend_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialGDBMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Global");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "Global",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    isarchived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "games",
                schema: "Global",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    image = table.Column<string>(type: "text", nullable: true),
                    ownerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.id);
                    table.ForeignKey(
                        name: "FK_games_users_ownerId",
                        column: x => x.ownerId,
                        principalSchema: "Global",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gameaccess",
                schema: "Global",
                columns: table => new
                {
                    fk_Users = table.Column<int>(type: "integer", nullable: false),
                    fk_Games = table.Column<int>(type: "integer", nullable: false),
                    accessType = table.Column<int>(type: "integer", nullable: false),
                    isArchived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gameaccess", x => new { x.fk_Users, x.fk_Games });
                    table.ForeignKey(
                        name: "FK_gameaccess_games_fk_Games",
                        column: x => x.fk_Games,
                        principalSchema: "Global",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gameaccess_users_fk_Users",
                        column: x => x.fk_Users,
                        principalSchema: "Global",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gameaccess_fk_Games",
                schema: "Global",
                table: "gameaccess",
                column: "fk_Games");

            migrationBuilder.CreateIndex(
                name: "IX_games_name",
                schema: "Global",
                table: "games",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_games_ownerId",
                schema: "Global",
                table: "games",
                column: "ownerId");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "Global",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_name",
                schema: "Global",
                table: "users",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gameaccess",
                schema: "Global");

            migrationBuilder.DropTable(
                name: "games",
                schema: "Global");

            migrationBuilder.DropTable(
                name: "users",
                schema: "Global");
        }
    }
}
