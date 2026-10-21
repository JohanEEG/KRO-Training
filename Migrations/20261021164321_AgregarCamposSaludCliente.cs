using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KROTraining.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposSaludCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cedula",
                table: "USUARIO",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Alergias",
                table: "CLIENTE",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactoEmergencia",
                table: "CLIENTE",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Medicamentos",
                table: "CLIENTE",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Padecimientos",
                table: "CLIENTE",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VISITANTE",
                columns: table => new
                {
                    visitante_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    convertido_cliente = table.Column<bool>(type: "bit", nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VISITANTE", x => x.visitante_id);
                    table.ForeignKey(
                        name: "FK_VISITANTE_USUARIO",
                        column: x => x.usuario_id,
                        principalTable: "USUARIO",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_VISITANTE_usuario_id",
                table: "VISITANTE",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "UQ_VISITANTE_CORREO",
                table: "VISITANTE",
                column: "correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VISITANTE");

            migrationBuilder.DropColumn(
                name: "Cedula",
                table: "USUARIO");

            migrationBuilder.DropColumn(
                name: "Alergias",
                table: "CLIENTE");

            migrationBuilder.DropColumn(
                name: "ContactoEmergencia",
                table: "CLIENTE");

            migrationBuilder.DropColumn(
                name: "Medicamentos",
                table: "CLIENTE");

            migrationBuilder.DropColumn(
                name: "Padecimientos",
                table: "CLIENTE");
        }
    }
}
