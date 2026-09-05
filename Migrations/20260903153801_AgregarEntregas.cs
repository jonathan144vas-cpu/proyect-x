using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControlViveresApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEntregas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Departamento",
                table: "Pedidos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Destino",
                table: "Pedidos",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Entregas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Lugar = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Departamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Municipio = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    FechaEntrega = table.Column<DateOnly>(type: "date", nullable: false),
                    FamiliasBeneficiadas = table.Column<int>(type: "integer", nullable: false),
                    TotalEntregado = table.Column<int>(type: "integer", nullable: false),
                    Latitud = table.Column<double>(type: "double precision", nullable: true),
                    Longitud = table.Column<double>(type: "double precision", nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    RegistradoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entregas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_Departamento",
                table: "Entregas",
                column: "Departamento");

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_FechaEntrega",
                table: "Entregas",
                column: "FechaEntrega");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entregas");

            migrationBuilder.DropColumn(
                name: "Departamento",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Destino",
                table: "Pedidos");
        }
    }
}
