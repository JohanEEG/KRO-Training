using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KROTraining.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTipoYNivelEjercicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nivel",
                table: "EJERCICIO",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "EJERCICIO",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nivel",
                table: "EJERCICIO");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "EJERCICIO");
        }
    }
}
