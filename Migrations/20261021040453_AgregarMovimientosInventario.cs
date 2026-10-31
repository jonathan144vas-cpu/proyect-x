using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControlViveresApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMovimientosInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Producto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    UnidadMedida = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Origen = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    RegistradoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_Fecha",
                table: "MovimientosInventario",
                column: "Fecha");

            // Carga inicial para que las gráficas no arranquen vacías. Es una aproximación a
            // partir de lo que ya existe: cada lote cuenta como una entrada (con su cantidad
            // actual y su fecha de registro) y cada entrega ya realizada como una salida.
            // Desde aquí en adelante, los movimientos se registran exactos.
            migrationBuilder.Sql("""
                INSERT INTO "MovimientosInventario" ("Fecha", "Tipo", "Producto", "Categoria", "Cantidad", "UnidadMedida", "Origen")
                SELECT "FechaRegistro", 'Entrada', "Nombre", "Categoria", "Cantidad", "UnidadMedida", 'Registro anterior'
                FROM "Alimentos"
                WHERE "Cantidad" > 0;
                """);

            migrationBuilder.Sql("""
                INSERT INTO "MovimientosInventario" ("Fecha", "Tipo", "Producto", "Categoria", "Cantidad", "UnidadMedida", "Origen")
                SELECT e."FechaEntrega"::timestamp AT TIME ZONE 'UTC',
                       'Salida',
                       d."Producto",
                       COALESCE((SELECT a."Categoria" FROM "Alimentos" a WHERE lower(a."Nombre") = lower(d."Producto") LIMIT 1), 'Otros'),
                       d."Total"::int,
                       d."SistemaMedida",
                       'Entrega a ' || e."Lugar"
                FROM "DetallesEntrega" d
                JOIN "Entregas" e ON e."Id" = d."EntregaId"
                WHERE d."Total" >= 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosInventario");
        }
    }
}
