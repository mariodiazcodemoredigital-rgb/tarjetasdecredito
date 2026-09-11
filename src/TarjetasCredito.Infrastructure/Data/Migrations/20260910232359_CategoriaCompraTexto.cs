using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarjetasCredito.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CategoriaCompraTexto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migración escrita a mano (no la generada automáticamente por EF Core): la columna
            // Categoria pasa de enum (INTEGER) a texto libre (ver SPEC-002 "Categoría de compra: texto
            // libre con sugerencias"). Un simple AlterColumn de int a TEXT en SQLite deja el número
            // crudo como texto ("0", "1", ...) en vez del nombre real de la categoría — se perdería la
            // información legible de cualquier compra ya registrada (incluidas las de producción). Se
            // agrega una columna nueva, se llena traduciendo cada valor entero al nombre que tenía ese
            // enum, y se reemplaza la columna vieja — para no perder ninguna compra ya capturada.
            migrationBuilder.AddColumn<string>(
                name: "CategoriaTexto",
                table: "Purchases",
                type: "TEXT",
                nullable: false,
                defaultValue: "Otro");

            migrationBuilder.Sql("""
                UPDATE Purchases SET CategoriaTexto = CASE Categoria
                    WHEN 0 THEN 'Supermercado'
                    WHEN 1 THEN 'Restaurantes'
                    WHEN 2 THEN 'Transporte'
                    WHEN 3 THEN 'Servicios'
                    WHEN 4 THEN 'Entretenimiento'
                    WHEN 5 THEN 'Salud'
                    WHEN 6 THEN 'Otro'
                    ELSE 'Otro'
                END;
                """);

            migrationBuilder.DropColumn(name: "Categoria", table: "Purchases");
            migrationBuilder.RenameColumn(name: "CategoriaTexto", table: "Purchases", newName: "Categoria");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoriaInt",
                table: "Purchases",
                type: "INTEGER",
                nullable: false,
                defaultValue: 6);

            migrationBuilder.Sql("""
                UPDATE Purchases SET CategoriaInt = CASE Categoria
                    WHEN 'Supermercado' THEN 0
                    WHEN 'Restaurantes' THEN 1
                    WHEN 'Transporte' THEN 2
                    WHEN 'Servicios' THEN 3
                    WHEN 'Entretenimiento' THEN 4
                    WHEN 'Salud' THEN 5
                    ELSE 6
                END;
                """);

            migrationBuilder.DropColumn(name: "Categoria", table: "Purchases");
            migrationBuilder.RenameColumn(name: "CategoriaInt", table: "Purchases", newName: "Categoria");
        }
    }
}
