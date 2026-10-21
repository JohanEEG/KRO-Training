using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KROTraining.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTipoActividadAsistencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoActividad",
                table: "ASISTENCIA",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoActividad",
                table: "ASISTENCIA");
        }
    }
}
