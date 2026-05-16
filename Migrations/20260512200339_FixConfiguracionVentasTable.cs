using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VentasBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixConfiguracionVentasTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionVentas",
                schema: "ventas",
                columns: table => new
                {
                    id_configuracion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    nombre_impuesto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    porcentaje_impuesto = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionVentas", x => x.id_configuracion);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionVentas_id_empresa",
                schema: "ventas",
                table: "ConfiguracionVentas",
                column: "id_empresa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionVentas",
                schema: "ventas");
        }
    }
}
