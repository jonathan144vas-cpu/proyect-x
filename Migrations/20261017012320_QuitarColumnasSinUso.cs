using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlViveresApp.Migrations
{
    /// <inheritdoc />
    public partial class QuitarColumnasSinUso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Departamento",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Destino",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Latitud",
                table: "Entregas");

            migrationBuilder.DropColumn(
                name: "Longitud",
                table: "Entregas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<double>(
                name: "Latitud",
                table: "Entregas",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitud",
                table: "Entregas",
                type: "double precision",
                nullable: true);
        }
    }
}
