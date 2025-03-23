using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wg_backend_api.Migrations.GameDb
{
    /// <inheritdoc />
    public partial class InitialGlobalMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "default_schema");

            migrationBuilder.CreateTable(
                name: "cultures",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cultures", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    picture = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "map",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    mapLocation = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_map", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nations",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    fk_religions = table.Column<int>(type: "integer", nullable: false),
                    fk_cultures = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "players",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_User = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "productionShares",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_SocialGroups = table.Column<int>(type: "integer", nullable: false),
                    fk_Resources = table.Column<int>(type: "integer", nullable: false),
                    coefficient = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productionShares", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "religions",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_religions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resources",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ismain = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "socialgroups",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    basehappiness = table.Column<float>(type: "real", nullable: false),
                    volunteers = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_socialgroups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "unitTypes",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    melee = table.Column<int>(type: "integer", nullable: false),
                    range = table.Column<int>(type: "integer", nullable: false),
                    defense = table.Column<int>(type: "integer", nullable: false),
                    speed = table.Column<int>(type: "integer", nullable: false),
                    morale = table.Column<int>(type: "integer", nullable: false),
                    volunteersNeeded = table.Column<int>(type: "integer", nullable: false),
                    isNaval = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unitTypes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usedResources",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_SocialGroups = table.Column<int>(type: "integer", nullable: false),
                    fk_Resources = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usedResources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "actions",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_Nations = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    result = table.Column<string>(type: "text", nullable: true),
                    isSettled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_actions", x => x.id);
                    table.ForeignKey(
                        name: "FK_actions_nations_fk_Nations",
                        column: x => x.fk_Nations,
                        principalSchema: "default_schema",
                        principalTable: "nations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "factions",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    fk_Nations = table.Column<int>(type: "integer", nullable: false),
                    power = table.Column<int>(type: "integer", nullable: false),
                    agenda = table.Column<string>(type: "text", nullable: false),
                    contentment = table.Column<int>(type: "integer", nullable: false),
                    color = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_factions", x => x.id);
                    table.ForeignKey(
                        name: "FK_factions_nations_fk_Nations",
                        column: x => x.fk_Nations,
                        principalSchema: "default_schema",
                        principalTable: "nations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    size = table.Column<int>(type: "integer", nullable: false),
                    fortifications = table.Column<int>(type: "integer", nullable: false),
                    fk_nations = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.id);
                    table.ForeignKey(
                        name: "FK_locations_nations_fk_nations",
                        column: x => x.fk_nations,
                        principalSchema: "default_schema",
                        principalTable: "nations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "relatedEvents",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_Events = table.Column<int>(type: "integer", nullable: false),
                    fk_Nations = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relatedEvents", x => x.id);
                    table.ForeignKey(
                        name: "FK_relatedEvents_events_fk_Events",
                        column: x => x.fk_Events,
                        principalSchema: "default_schema",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_relatedEvents_nations_fk_Nations",
                        column: x => x.fk_Nations,
                        principalSchema: "default_schema",
                        principalTable: "nations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tradeagreements",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_nationoffering = table.Column<int>(type: "integer", nullable: false),
                    fk_nationreceiving = table.Column<int>(type: "integer", nullable: false),
                    isaccepted = table.Column<bool>(type: "boolean", nullable: false),
                    duration = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tradeagreements", x => x.id);
                    table.ForeignKey(
                        name: "FK_tradeagreements_nations_fk_nationoffering",
                        column: x => x.fk_nationoffering,
                        principalSchema: "default_schema",
                        principalTable: "nations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tradeagreements_nations_fk_nationreceiving",
                        column: x => x.fk_nationreceiving,
                        principalSchema: "default_schema",
                        principalTable: "nations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accessestonations",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_nations = table.Column<int>(type: "integer", nullable: false),
                    fk_users = table.Column<int>(type: "integer", nullable: false),
                    dateacquired = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accessestonations", x => x.id);
                    table.ForeignKey(
                        name: "FK_accessestonations_nations_fk_nations",
                        column: x => x.fk_nations,
                        principalSchema: "default_schema",
                        principalTable: "nations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_accessestonations_players_fk_users",
                        column: x => x.fk_users,
                        principalSchema: "default_schema",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mapAccess",
                schema: "default_schema",
                columns: table => new
                {
                    fk_Users = table.Column<int>(type: "integer", nullable: false),
                    fk_Maps = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mapAccess", x => new { x.fk_Users, x.fk_Maps });
                    table.ForeignKey(
                        name: "FK_mapAccess_map_fk_Maps",
                        column: x => x.fk_Maps,
                        principalSchema: "default_schema",
                        principalTable: "map",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mapAccess_players_fk_Users",
                        column: x => x.fk_Users,
                        principalSchema: "default_schema",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "modifiers",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_Events = table.Column<int>(type: "integer", nullable: false),
                    modifireType = table.Column<int>(type: "integer", nullable: false),
                    fk_Resources = table.Column<int>(type: "integer", nullable: true),
                    fk_SocialGroups = table.Column<int>(type: "integer", nullable: true),
                    fk_Cultures = table.Column<int>(type: "integer", nullable: true),
                    fk_Religion = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modifiers", x => x.id);
                    table.ForeignKey(
                        name: "FK_modifiers_cultures_fk_Cultures",
                        column: x => x.fk_Cultures,
                        principalSchema: "default_schema",
                        principalTable: "cultures",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_modifiers_events_fk_Events",
                        column: x => x.fk_Events,
                        principalSchema: "default_schema",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_modifiers_religions_fk_Religion",
                        column: x => x.fk_Religion,
                        principalSchema: "default_schema",
                        principalTable: "religions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_modifiers_resources_fk_Resources",
                        column: x => x.fk_Resources,
                        principalSchema: "default_schema",
                        principalTable: "resources",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_modifiers_socialgroups_fk_SocialGroups",
                        column: x => x.fk_SocialGroups,
                        principalSchema: "default_schema",
                        principalTable: "socialgroups",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "accessToUnits",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_Users = table.Column<int>(type: "integer", nullable: false),
                    fk_UnitTypes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accessToUnits", x => x.id);
                    table.ForeignKey(
                        name: "FK_accessToUnits_players_fk_Users",
                        column: x => x.fk_Users,
                        principalSchema: "default_schema",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_accessToUnits_unitTypes_fk_UnitTypes",
                        column: x => x.fk_UnitTypes,
                        principalSchema: "default_schema",
                        principalTable: "unitTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenanceCosts",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_UnitTypes = table.Column<int>(type: "integer", nullable: false),
                    fk_Resources = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenanceCosts", x => x.id);
                    table.ForeignKey(
                        name: "FK_maintenanceCosts_resources_fk_Resources",
                        column: x => x.fk_Resources,
                        principalSchema: "default_schema",
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintenanceCosts_unitTypes_fk_UnitTypes",
                        column: x => x.fk_UnitTypes,
                        principalSchema: "default_schema",
                        principalTable: "unitTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "productionCost",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_UnitTypes = table.Column<int>(type: "integer", nullable: false),
                    fk_Resources = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productionCost", x => x.id);
                    table.ForeignKey(
                        name: "FK_productionCost_resources_fk_Resources",
                        column: x => x.fk_Resources,
                        principalSchema: "default_schema",
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_productionCost_unitTypes_fk_UnitTypes",
                        column: x => x.fk_UnitTypes,
                        principalSchema: "default_schema",
                        principalTable: "unitTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "unitOrders",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_UnitTypes = table.Column<int>(type: "integer", nullable: false),
                    fk_Nations = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unitOrders", x => x.id);
                    table.ForeignKey(
                        name: "FK_unitOrders_nations_fk_Nations",
                        column: x => x.fk_Nations,
                        principalSchema: "default_schema",
                        principalTable: "nations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_unitOrders_unitTypes_fk_UnitTypes",
                        column: x => x.fk_UnitTypes,
                        principalSchema: "default_schema",
                        principalTable: "unitTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "armies",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    fk_Nations = table.Column<int>(type: "integer", nullable: false),
                    fk_Locations = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_armies", x => x.id);
                    table.ForeignKey(
                        name: "FK_armies_locations_fk_Locations",
                        column: x => x.fk_Locations,
                        principalSchema: "default_schema",
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_armies_nations_fk_Nations",
                        column: x => x.fk_Nations,
                        principalSchema: "default_schema",
                        principalTable: "nations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "populations",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_religions = table.Column<int>(type: "integer", nullable: false),
                    fk_cultures = table.Column<int>(type: "integer", nullable: false),
                    fk_socialgroups = table.Column<int>(type: "integer", nullable: false),
                    fk_locations = table.Column<int>(type: "integer", nullable: false),
                    happiness = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_populations", x => x.id);
                    table.ForeignKey(
                        name: "FK_populations_cultures_fk_cultures",
                        column: x => x.fk_cultures,
                        principalSchema: "default_schema",
                        principalTable: "cultures",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_populations_locations_fk_locations",
                        column: x => x.fk_locations,
                        principalSchema: "default_schema",
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_populations_religions_fk_religions",
                        column: x => x.fk_religions,
                        principalSchema: "default_schema",
                        principalTable: "religions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_populations_socialgroups_fk_socialgroups",
                        column: x => x.fk_socialgroups,
                        principalSchema: "default_schema",
                        principalTable: "socialgroups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "offeredresources",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_resource = table.Column<int>(type: "integer", nullable: false),
                    fk_tradeagreement = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offeredresources", x => x.id);
                    table.ForeignKey(
                        name: "FK_offeredresources_resources_fk_resource",
                        column: x => x.fk_resource,
                        principalSchema: "default_schema",
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_offeredresources_tradeagreements_fk_tradeagreement",
                        column: x => x.fk_tradeagreement,
                        principalSchema: "default_schema",
                        principalTable: "tradeagreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wantedresources",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_resource = table.Column<int>(type: "integer", nullable: false),
                    fk_tradeagreement = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wantedresources", x => x.id);
                    table.ForeignKey(
                        name: "FK_wantedresources_resources_fk_resource",
                        column: x => x.fk_resource,
                        principalSchema: "default_schema",
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_wantedresources_tradeagreements_fk_tradeagreement",
                        column: x => x.fk_tradeagreement,
                        principalSchema: "default_schema",
                        principalTable: "tradeagreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "troops",
                schema: "default_schema",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_UnitTypes = table.Column<int>(type: "integer", nullable: false),
                    fk_Armies = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_troops", x => x.id);
                    table.ForeignKey(
                        name: "FK_troops_armies_fk_Armies",
                        column: x => x.fk_Armies,
                        principalSchema: "default_schema",
                        principalTable: "armies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_troops_unitTypes_fk_UnitTypes",
                        column: x => x.fk_UnitTypes,
                        principalSchema: "default_schema",
                        principalTable: "unitTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accessestonations_fk_nations",
                schema: "default_schema",
                table: "accessestonations",
                column: "fk_nations");

            migrationBuilder.CreateIndex(
                name: "IX_accessestonations_fk_users",
                schema: "default_schema",
                table: "accessestonations",
                column: "fk_users");

            migrationBuilder.CreateIndex(
                name: "IX_accessToUnits_fk_UnitTypes",
                schema: "default_schema",
                table: "accessToUnits",
                column: "fk_UnitTypes");

            migrationBuilder.CreateIndex(
                name: "IX_accessToUnits_fk_Users",
                schema: "default_schema",
                table: "accessToUnits",
                column: "fk_Users");

            migrationBuilder.CreateIndex(
                name: "IX_actions_fk_Nations",
                schema: "default_schema",
                table: "actions",
                column: "fk_Nations");

            migrationBuilder.CreateIndex(
                name: "IX_armies_fk_Locations",
                schema: "default_schema",
                table: "armies",
                column: "fk_Locations");

            migrationBuilder.CreateIndex(
                name: "IX_armies_fk_Nations",
                schema: "default_schema",
                table: "armies",
                column: "fk_Nations");

            migrationBuilder.CreateIndex(
                name: "IX_factions_fk_Nations",
                schema: "default_schema",
                table: "factions",
                column: "fk_Nations");

            migrationBuilder.CreateIndex(
                name: "IX_locations_fk_nations",
                schema: "default_schema",
                table: "locations",
                column: "fk_nations");

            migrationBuilder.CreateIndex(
                name: "IX_maintenanceCosts_fk_Resources",
                schema: "default_schema",
                table: "maintenanceCosts",
                column: "fk_Resources");

            migrationBuilder.CreateIndex(
                name: "IX_maintenanceCosts_fk_UnitTypes",
                schema: "default_schema",
                table: "maintenanceCosts",
                column: "fk_UnitTypes");

            migrationBuilder.CreateIndex(
                name: "IX_mapAccess_fk_Maps",
                schema: "default_schema",
                table: "mapAccess",
                column: "fk_Maps");

            migrationBuilder.CreateIndex(
                name: "IX_modifiers_fk_Cultures",
                schema: "default_schema",
                table: "modifiers",
                column: "fk_Cultures");

            migrationBuilder.CreateIndex(
                name: "IX_modifiers_fk_Events",
                schema: "default_schema",
                table: "modifiers",
                column: "fk_Events");

            migrationBuilder.CreateIndex(
                name: "IX_modifiers_fk_Religion",
                schema: "default_schema",
                table: "modifiers",
                column: "fk_Religion");

            migrationBuilder.CreateIndex(
                name: "IX_modifiers_fk_Resources",
                schema: "default_schema",
                table: "modifiers",
                column: "fk_Resources");

            migrationBuilder.CreateIndex(
                name: "IX_modifiers_fk_SocialGroups",
                schema: "default_schema",
                table: "modifiers",
                column: "fk_SocialGroups");

            migrationBuilder.CreateIndex(
                name: "IX_offeredresources_fk_resource",
                schema: "default_schema",
                table: "offeredresources",
                column: "fk_resource");

            migrationBuilder.CreateIndex(
                name: "IX_offeredresources_fk_tradeagreement",
                schema: "default_schema",
                table: "offeredresources",
                column: "fk_tradeagreement");

            migrationBuilder.CreateIndex(
                name: "IX_populations_fk_cultures",
                schema: "default_schema",
                table: "populations",
                column: "fk_cultures");

            migrationBuilder.CreateIndex(
                name: "IX_populations_fk_locations",
                schema: "default_schema",
                table: "populations",
                column: "fk_locations");

            migrationBuilder.CreateIndex(
                name: "IX_populations_fk_religions",
                schema: "default_schema",
                table: "populations",
                column: "fk_religions");

            migrationBuilder.CreateIndex(
                name: "IX_populations_fk_socialgroups",
                schema: "default_schema",
                table: "populations",
                column: "fk_socialgroups");

            migrationBuilder.CreateIndex(
                name: "IX_productionCost_fk_Resources",
                schema: "default_schema",
                table: "productionCost",
                column: "fk_Resources");

            migrationBuilder.CreateIndex(
                name: "IX_productionCost_fk_UnitTypes",
                schema: "default_schema",
                table: "productionCost",
                column: "fk_UnitTypes");

            migrationBuilder.CreateIndex(
                name: "IX_relatedEvents_fk_Events",
                schema: "default_schema",
                table: "relatedEvents",
                column: "fk_Events");

            migrationBuilder.CreateIndex(
                name: "IX_relatedEvents_fk_Nations",
                schema: "default_schema",
                table: "relatedEvents",
                column: "fk_Nations");

            migrationBuilder.CreateIndex(
                name: "IX_tradeagreements_fk_nationoffering",
                schema: "default_schema",
                table: "tradeagreements",
                column: "fk_nationoffering");

            migrationBuilder.CreateIndex(
                name: "IX_tradeagreements_fk_nationreceiving",
                schema: "default_schema",
                table: "tradeagreements",
                column: "fk_nationreceiving");

            migrationBuilder.CreateIndex(
                name: "IX_troops_fk_Armies",
                schema: "default_schema",
                table: "troops",
                column: "fk_Armies");

            migrationBuilder.CreateIndex(
                name: "IX_troops_fk_UnitTypes",
                schema: "default_schema",
                table: "troops",
                column: "fk_UnitTypes");

            migrationBuilder.CreateIndex(
                name: "IX_unitOrders_fk_Nations",
                schema: "default_schema",
                table: "unitOrders",
                column: "fk_Nations");

            migrationBuilder.CreateIndex(
                name: "IX_unitOrders_fk_UnitTypes",
                schema: "default_schema",
                table: "unitOrders",
                column: "fk_UnitTypes");

            migrationBuilder.CreateIndex(
                name: "IX_wantedresources_fk_resource",
                schema: "default_schema",
                table: "wantedresources",
                column: "fk_resource");

            migrationBuilder.CreateIndex(
                name: "IX_wantedresources_fk_tradeagreement",
                schema: "default_schema",
                table: "wantedresources",
                column: "fk_tradeagreement");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accessestonations",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "accessToUnits",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "actions",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "factions",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "maintenanceCosts",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "mapAccess",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "modifiers",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "offeredresources",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "populations",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "productionCost",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "productionShares",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "relatedEvents",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "troops",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "unitOrders",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "usedResources",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "wantedresources",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "map",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "players",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "cultures",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "religions",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "socialgroups",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "events",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "armies",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "unitTypes",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "resources",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "tradeagreements",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "locations",
                schema: "default_schema");

            migrationBuilder.DropTable(
                name: "nations",
                schema: "default_schema");
        }
    }
}
