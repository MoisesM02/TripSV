using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripSV.Datos.Migraciones
{
    public partial class AgregaFavoritos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "favoritos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    sitio_id = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favoritos", x => x.id);
                    table.ForeignKey(
                        name: "FK_favoritos_sitios_sitio_id",
                        column: x => x.sitio_id,
                        principalTable: "sitios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_favoritos_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_favoritos_sitio_id",
                table: "favoritos",
                column: "sitio_id");

            migrationBuilder.CreateIndex(
                name: "IX_favoritos_usuario_id_sitio_id",
                table: "favoritos",
                columns: new[] { "usuario_id", "sitio_id" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favoritos");
        }
    }
}
