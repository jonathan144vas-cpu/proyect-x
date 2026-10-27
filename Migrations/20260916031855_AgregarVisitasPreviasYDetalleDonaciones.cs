using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControlViveresApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVisitasPreviasYDetalleDonaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntregaProgramadaId",
                table: "Entregas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DetallesEntrega",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntregaId = table.Column<int>(type: "integer", nullable: false),
                    Producto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SistemaMedida = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesEntrega", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesEntrega_Entregas_EntregaId",
                        column: x => x.EntregaId,
                        principalTable: "Entregas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitasPrevias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Lugar = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Departamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Municipio = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    FechaPropuesta = table.Column<DateOnly>(type: "date", nullable: false),
                    FamiliasBeneficiadas = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    RegistradoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaRevision = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EntregaProgramadaGeneradaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitasPrevias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetallesVisitaPrevia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitaPreviaId = table.Column<int>(type: "integer", nullable: false),
                    Producto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SistemaMedida = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesVisitaPrevia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesVisitaPrevia_VisitasPrevias_VisitaPreviaId",
                        column: x => x.VisitaPreviaId,
                        principalTable: "VisitasPrevias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_EntregaProgramadaId",
                table: "Entregas",
                column: "EntregaProgramadaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesEntrega_EntregaId",
                table: "DetallesEntrega",
                column: "EntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVisitaPrevia_VisitaPreviaId",
                table: "DetallesVisitaPrevia",
                column: "VisitaPreviaId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitasPrevias_Departamento",
                table: "VisitasPrevias",
                column: "Departamento");

            migrationBuilder.CreateIndex(
                name: "IX_VisitasPrevias_Estado",
                table: "VisitasPrevias",
                column: "Estado");

            migrationBuilder.AddForeignKey(
                name: "FK_Entregas_EntregasProgramadas_EntregaProgramadaId",
                table: "Entregas",
                column: "EntregaProgramadaId",
                principalTable: "EntregasProgramadas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entregas_EntregasProgramadas_EntregaProgramadaId",
                table: "Entregas");

            migrationBuilder.DropTable(
                name: "DetallesEntrega");

            migrationBuilder.DropTable(
                name: "DetallesVisitaPrevia");

            migrationBuilder.DropTable(
                name: "VisitasPrevias");

            migrationBuilder.DropIndex(
                name: "IX_Entregas_EntregaProgramadaId",
                table: "Entregas");

            migrationBuilder.DropColumn(
                name: "EntregaProgramadaId",
                table: "Entregas");
        }
    }
}
