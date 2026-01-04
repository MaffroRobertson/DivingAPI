using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DivingAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeededTestData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$IRw05ecQLaLiYGEvYt04yeS2KOs7VzKxB15FtapILkpekskP1LElC");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$g11E66FXSdgSav15B0MqR.uuCsEJsrCdgi9fi4dMAxBNRg2fM1V/O");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "testPassword");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "adminPassword");
        }
    }
}
