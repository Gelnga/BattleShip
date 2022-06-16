using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAL.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BsBrainConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BoardWidth = table.Column<int>(type: "integer", nullable: false),
                    BoardLength = table.Column<int>(type: "integer", nullable: false),
                    EShipTouchRule = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BsBrainConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShipConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShipName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ShipQuantity = table.Column<int>(type: "integer", nullable: false),
                    ShipSizeX = table.Column<int>(type: "integer", nullable: false),
                    ShipSizeY = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedBsBrains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BsBrainConfigurationId = table.Column<int>(type: "integer", nullable: false),
                    GameBoards = table.Column<string>(type: "character varying(100000)", maxLength: 100000, nullable: false),
                    AgainstAi = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentPlayerId = table.Column<int>(type: "integer", nullable: false),
                    SavedBrainName = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedBsBrains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedBsBrains_BsBrainConfigurations_BsBrainConfigurationId",
                        column: x => x.BsBrainConfigurationId,
                        principalTable: "BsBrainConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShipInBSBrainConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BsBrainConfigurationId = table.Column<int>(type: "integer", nullable: false),
                    ShipConfigurationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipInBSBrainConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipInBSBrainConfigurations_BsBrainConfigurations_BsBrainCo~",
                        column: x => x.BsBrainConfigurationId,
                        principalTable: "BsBrainConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShipInBSBrainConfigurations_ShipConfigurations_ShipConfigur~",
                        column: x => x.ShipConfigurationId,
                        principalTable: "ShipConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShipPlacementStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SavedBsBrainId = table.Column<int>(type: "integer", nullable: false),
                    Rotation = table.Column<int>(type: "integer", nullable: false),
                    ShipConfigIndex = table.Column<int>(type: "integer", nullable: false),
                    CurrentShipQuantity = table.Column<int>(type: "integer", nullable: false),
                    ShipPlacementPreviewCoordinate = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PreviouslyPlacedShips = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipPlacementStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipPlacementStates_SavedBsBrains_SavedBsBrainId",
                        column: x => x.SavedBsBrainId,
                        principalTable: "SavedBsBrains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedBsBrains_BsBrainConfigurationId",
                table: "SavedBsBrains",
                column: "BsBrainConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipInBSBrainConfigurations_BsBrainConfigurationId",
                table: "ShipInBSBrainConfigurations",
                column: "BsBrainConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipInBSBrainConfigurations_ShipConfigurationId",
                table: "ShipInBSBrainConfigurations",
                column: "ShipConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipPlacementStates_SavedBsBrainId",
                table: "ShipPlacementStates",
                column: "SavedBsBrainId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShipInBSBrainConfigurations");

            migrationBuilder.DropTable(
                name: "ShipPlacementStates");

            migrationBuilder.DropTable(
                name: "ShipConfigurations");

            migrationBuilder.DropTable(
                name: "SavedBsBrains");

            migrationBuilder.DropTable(
                name: "BsBrainConfigurations");
        }
    }
}
