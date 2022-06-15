using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BsBrainConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardWidth = table.Column<int>(type: "int", nullable: false),
                    BoardLength = table.Column<int>(type: "int", nullable: false),
                    EShipTouchRule = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BsBrainConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShipConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ShipQuantity = table.Column<int>(type: "int", nullable: false),
                    ShipSizeX = table.Column<int>(type: "int", nullable: false),
                    ShipSizeY = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedBsBrains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BsBrainConfigurationId = table.Column<int>(type: "int", nullable: false),
                    GameBoards = table.Column<string>(type: "nvarchar(max)", maxLength: 100000, nullable: false),
                    AgainstAi = table.Column<bool>(type: "bit", nullable: false),
                    CurrentPlayerId = table.Column<int>(type: "int", nullable: false),
                    SavedBrainName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
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
                name: "ShipInBsBrainConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BsBrainConfigurationId = table.Column<int>(type: "int", nullable: false),
                    ShipConfigurationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipInBsBrainConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipInBsBrainConfigurations_BsBrainConfigurations_BsBrainConfigurationId",
                        column: x => x.BsBrainConfigurationId,
                        principalTable: "BsBrainConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShipInBsBrainConfigurations_ShipConfigurations_ShipConfigurationId",
                        column: x => x.ShipConfigurationId,
                        principalTable: "ShipConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedBsBrains_BsBrainConfigurationId",
                table: "SavedBsBrains",
                column: "BsBrainConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipInBsBrainConfigurations_BsBrainConfigurationId",
                table: "ShipInBsBrainConfigurations",
                column: "BsBrainConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipInBsBrainConfigurations_ShipConfigurationId",
                table: "ShipInBsBrainConfigurations",
                column: "ShipConfigurationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedBsBrains");

            migrationBuilder.DropTable(
                name: "ShipInBsBrainConfigurations");

            migrationBuilder.DropTable(
                name: "BsBrainConfigurations");

            migrationBuilder.DropTable(
                name: "ShipConfigurations");
        }
    }
}
