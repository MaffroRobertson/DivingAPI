using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DivingAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class PreDockerMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExperienceLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperienceLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiveSites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExperienceLevelId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiveSites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiveSites_ExperienceLevels_ExperienceLevelId",
                        column: x => x.ExperienceLevelId,
                        principalTable: "ExperienceLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiveSiteId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    MaxDepth = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dives_DiveSites_DiveSiteId",
                        column: x => x.DiveSiteId,
                        principalTable: "DiveSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ExperienceLevels",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Open Water" },
                    { 2, "Advanced" },
                    { 3, "Technical" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dives_DiveSiteId",
                table: "Dives",
                column: "DiveSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_DiveSites_ExperienceLevelId",
                table: "DiveSites",
                column: "ExperienceLevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dives");

            migrationBuilder.DropTable(
                name: "DiveSites");

            migrationBuilder.DropTable(
                name: "ExperienceLevels");
        }
    }
}
