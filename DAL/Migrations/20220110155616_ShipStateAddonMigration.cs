using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    public partial class ShipStateAddonMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipInBsBrainConfigurations_BsBrainConfigurations_BsBrainConfigurationId",
                table: "ShipInBsBrainConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipInBsBrainConfigurations_ShipConfigurations_ShipConfigurationId",
                table: "ShipInBsBrainConfigurations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShipInBsBrainConfigurations",
                table: "ShipInBsBrainConfigurations");

            migrationBuilder.RenameTable(
                name: "ShipInBsBrainConfigurations",
                newName: "ShipInBSBrainConfigurations");

            migrationBuilder.RenameIndex(
                name: "IX_ShipInBsBrainConfigurations_ShipConfigurationId",
                table: "ShipInBSBrainConfigurations",
                newName: "IX_ShipInBSBrainConfigurations_ShipConfigurationId");

            migrationBuilder.RenameIndex(
                name: "IX_ShipInBsBrainConfigurations_BsBrainConfigurationId",
                table: "ShipInBSBrainConfigurations",
                newName: "IX_ShipInBSBrainConfigurations_BsBrainConfigurationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShipInBSBrainConfigurations",
                table: "ShipInBSBrainConfigurations",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ShipPlacementStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SavedBsBrainId = table.Column<int>(type: "int", nullable: false),
                    Rotation = table.Column<int>(type: "int", nullable: false),
                    ShipConfigIndex = table.Column<int>(type: "int", nullable: false),
                    CurrentShipQuantity = table.Column<int>(type: "int", nullable: false),
                    ShipPlacementPreviewCoordinate = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PreviouslyPlacedShips = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: false)
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
                name: "IX_ShipPlacementStates_SavedBsBrainId",
                table: "ShipPlacementStates",
                column: "SavedBsBrainId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipInBSBrainConfigurations_BsBrainConfigurations_BsBrainConfigurationId",
                table: "ShipInBSBrainConfigurations",
                column: "BsBrainConfigurationId",
                principalTable: "BsBrainConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipInBSBrainConfigurations_ShipConfigurations_ShipConfigurationId",
                table: "ShipInBSBrainConfigurations",
                column: "ShipConfigurationId",
                principalTable: "ShipConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipInBSBrainConfigurations_BsBrainConfigurations_BsBrainConfigurationId",
                table: "ShipInBSBrainConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipInBSBrainConfigurations_ShipConfigurations_ShipConfigurationId",
                table: "ShipInBSBrainConfigurations");

            migrationBuilder.DropTable(
                name: "ShipPlacementStates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShipInBSBrainConfigurations",
                table: "ShipInBSBrainConfigurations");

            migrationBuilder.RenameTable(
                name: "ShipInBSBrainConfigurations",
                newName: "ShipInBsBrainConfigurations");

            migrationBuilder.RenameIndex(
                name: "IX_ShipInBSBrainConfigurations_ShipConfigurationId",
                table: "ShipInBsBrainConfigurations",
                newName: "IX_ShipInBsBrainConfigurations_ShipConfigurationId");

            migrationBuilder.RenameIndex(
                name: "IX_ShipInBSBrainConfigurations_BsBrainConfigurationId",
                table: "ShipInBsBrainConfigurations",
                newName: "IX_ShipInBsBrainConfigurations_BsBrainConfigurationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShipInBsBrainConfigurations",
                table: "ShipInBsBrainConfigurations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipInBsBrainConfigurations_BsBrainConfigurations_BsBrainConfigurationId",
                table: "ShipInBsBrainConfigurations",
                column: "BsBrainConfigurationId",
                principalTable: "BsBrainConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipInBsBrainConfigurations_ShipConfigurations_ShipConfigurationId",
                table: "ShipInBsBrainConfigurations",
                column: "ShipConfigurationId",
                principalTable: "ShipConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
