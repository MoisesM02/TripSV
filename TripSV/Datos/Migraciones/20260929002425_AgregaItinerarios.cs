using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripSV.Datos.Migraciones
{
    public partial class AgregaItinerarios : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "itinerarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "date", nullable: false),
                    cantidad_dias = table.Column<int>(type: "int", nullable: false),
                    notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itinerarios", x => x.id);
                    table.CheckConstraint("CK_itinerarios_cantidad_dias", "[cantidad_dias] BETWEEN 1 AND 15");
                    table.ForeignKey(
                        name: "FK_itinerarios_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visitas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    itinerario_id = table.Column<int>(type: "int", nullable: false),
                    sitio_id = table.Column<int>(type: "int", nullable: false),
                    dia = table.Column<int>(type: "int", nullable: false),
                    orden = table.Column<int>(type: "int", nullable: false),
                    notas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visitas", x => x.id);
                    table.CheckConstraint("CK_visitas_dia", "[dia] BETWEEN 1 AND 15");
                    table.ForeignKey(
                        name: "FK_visitas_itinerarios_itinerario_id",
                        column: x => x.itinerario_id,
                        principalTable: "itinerarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_visitas_sitios_sitio_id",
                        column: x => x.sitio_id,
                        principalTable: "sitios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_itinerarios_usuario_id",
                table: "itinerarios",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitas_itinerario_id_dia_sitio_id",
                table: "visitas",
                columns: new[] { "itinerario_id", "dia", "sitio_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_visitas_sitio_id",
                table: "visitas",
                column: "sitio_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visitas");

            migrationBuilder.DropTable(
                name: "itinerarios");
        }
    }
}
