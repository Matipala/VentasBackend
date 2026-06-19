using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VentasBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialUuidSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ventas");

            migrationBuilder.CreateTable(
                name: "clientes",
                schema: "ventas",
                columns: table => new
                {
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    id_empresa = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id_cliente);
                    table.ForeignKey(
                        name: "FK_clientes_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalSchema: "shared",
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionVentas",
                schema: "ventas",
                columns: table => new
                {
                    id_configuracion = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    id_empresa = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_impuesto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    porcentaje_impuesto = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionVentas", x => x.id_configuracion);
                });

            migrationBuilder.CreateTable(
                name: "cuentas_tickets",
                schema: "ventas",
                columns: table => new
                {
                    id_cuenta_ticket = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    id_empresa = table.Column<Guid>(type: "uuid", nullable: false),
                    id_almacen = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: true),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    mesero = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    impuesto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_pago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuentas_tickets", x => x.id_cuenta_ticket);
                    table.ForeignKey(
                        name: "FK_cuentas_tickets_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalSchema: "shared",
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cuentas_tickets_clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "ventas",
                        principalTable: "clientes",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cuentas_ticket_items",
                schema: "ventas",
                columns: table => new
                {
                    id_cuenta_ticket_item = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    id_cuenta_ticket = table.Column<Guid>(type: "uuid", nullable: false),
                    id_producto = table.Column<Guid>(type: "uuid", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    nota = table.Column<string>(type: "text", nullable: true),
                    estado_comanda = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuentas_ticket_items", x => x.id_cuenta_ticket_item);
                    table.ForeignKey(
                        name: "FK_cuentas_ticket_items_cuentas_tickets_id_cuenta_ticket",
                        column: x => x.id_cuenta_ticket,
                        principalSchema: "ventas",
                        principalTable: "cuentas_tickets",
                        principalColumn: "id_cuenta_ticket",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pagos",
                schema: "ventas",
                columns: table => new
                {
                    id_pago = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    id_empresa = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cuenta_ticket = table.Column<Guid>(type: "uuid", nullable: false),
                    metodo_pago = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.id_pago);
                    table.ForeignKey(
                        name: "FK_pagos_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalSchema: "shared",
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pagos_cuentas_tickets_id_cuenta_ticket",
                        column: x => x.id_cuenta_ticket,
                        principalSchema: "ventas",
                        principalTable: "cuentas_tickets",
                        principalColumn: "id_cuenta_ticket",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clientes_id_empresa",
                schema: "ventas",
                table: "clientes",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_id_empresa_telefono",
                schema: "ventas",
                table: "clientes",
                columns: new[] { "id_empresa", "telefono" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionVentas_id_empresa",
                schema: "ventas",
                table: "ConfiguracionVentas",
                column: "id_empresa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_ticket_items_estado_comanda",
                schema: "ventas",
                table: "cuentas_ticket_items",
                column: "estado_comanda");

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_ticket_items_id_cuenta_ticket",
                schema: "ventas",
                table: "cuentas_ticket_items",
                column: "id_cuenta_ticket");

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_ticket_items_id_producto",
                schema: "ventas",
                table: "cuentas_ticket_items",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_tickets_fecha_creacion",
                schema: "ventas",
                table: "cuentas_tickets",
                column: "fecha_creacion");

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_tickets_id_cliente",
                schema: "ventas",
                table: "cuentas_tickets",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_tickets_id_empresa_estado",
                schema: "ventas",
                table: "cuentas_tickets",
                columns: new[] { "id_empresa", "estado" });

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_tickets_id_empresa_numero",
                schema: "ventas",
                table: "cuentas_tickets",
                columns: new[] { "id_empresa", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pagos_fecha_pago",
                schema: "ventas",
                table: "pagos",
                column: "fecha_pago");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_id_cuenta_ticket",
                schema: "ventas",
                table: "pagos",
                column: "id_cuenta_ticket");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_id_empresa",
                schema: "ventas",
                table: "pagos",
                column: "id_empresa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionVentas",
                schema: "ventas");

            migrationBuilder.DropTable(
                name: "cuentas_ticket_items",
                schema: "ventas");

            migrationBuilder.DropTable(
                name: "pagos",
                schema: "ventas");

            migrationBuilder.DropTable(
                name: "cuentas_tickets",
                schema: "ventas");

            migrationBuilder.DropTable(
                name: "clientes",
                schema: "ventas");
        }
    }
}
