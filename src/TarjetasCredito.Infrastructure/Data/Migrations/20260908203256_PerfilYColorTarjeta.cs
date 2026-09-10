using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarjetasCredito.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PerfilYColorTarjeta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "AspNetUsers",
                newName: "Nombres");

            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                table: "CreditCards",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Apellidos",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorHex",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "Apellidos",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Nombres",
                table: "AspNetUsers",
                newName: "DisplayName");
        }
    }
}
