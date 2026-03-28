using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VentasBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKdsStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cuentas_ticket_items_comanda_enviada",
                schema: "ventas",
                table: "cuentas_ticket_items");

            migrationBuilder.DropColumn(
                name: "comanda_enviada",
                schema: "ventas",
                table: "cuentas_ticket_items");

            migrationBuilder.AddColumn<string>(
                name: "estado_comanda",
                schema: "ventas",
                table: "cuentas_ticket_items",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");


            migrationBuilder.CreateIndex(
                name: "IX_cuentas_ticket_items_estado_comanda",
                schema: "ventas",
                table: "cuentas_ticket_items",
                column: "estado_comanda");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropIndex(
                name: "IX_cuentas_ticket_items_estado_comanda",
                schema: "ventas",
                table: "cuentas_ticket_items");

            migrationBuilder.DropColumn(
                name: "estado_comanda",
                schema: "ventas",
                table: "cuentas_ticket_items");

            migrationBuilder.AddColumn<bool>(
                name: "comanda_enviada",
                schema: "ventas",
                table: "cuentas_ticket_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_ticket_items_comanda_enviada",
                schema: "ventas",
                table: "cuentas_ticket_items",
                column: "comanda_enviada");
        }
    }
}
