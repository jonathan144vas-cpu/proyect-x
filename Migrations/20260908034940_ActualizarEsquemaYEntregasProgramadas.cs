using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControlViveresApp.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarEsquemaYEntregasProgramadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaVencimiento",
                table: "Pedidos",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EntregasProgramadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Lugar = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Departamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Municipio = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    FechaProgramada = table.Column<DateOnly>(type: "date", nullable: false),
                    FamiliasBeneficiadas = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    RegistradoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCompletada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntregasProgramadas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetallesEntregaProgramada",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntregaProgramadaId = table.Column<int>(type: "integer", nullable: false),
                    Producto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SistemaMedida = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesEntregaProgramada", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesEntregaProgramada_EntregasProgramadas_EntregaProgra~",
                        column: x => x.EntregaProgramadaId,
                        principalTable: "EntregasProgramadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesEntregaProgramada_EntregaProgramadaId",
                table: "DetallesEntregaProgramada",
                column: "EntregaProgramadaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasProgramadas_Departamento",
                table: "EntregasProgramadas",
                column: "Departamento");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasProgramadas_Estado",
                table: "EntregasProgramadas",
                column: "Estado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesEntregaProgramada");

            migrationBuilder.DropTable(
                name: "EntregasProgramadas");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "Pedidos");
        }
    }
}
