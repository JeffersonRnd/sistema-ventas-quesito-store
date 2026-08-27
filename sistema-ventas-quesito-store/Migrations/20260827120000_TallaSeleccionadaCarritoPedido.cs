using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using sistema_ventas_quesito_store.Data;

#nullable disable

namespace sistema_ventas_quesito_store.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260827120000_TallaSeleccionadaCarritoPedido")]
    public partial class TallaSeleccionadaCarritoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TallaSeleccionada",
                table: "CarritoDetalles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TallaSeleccionada",
                table: "DetallesPedido",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TallaSeleccionada",
                table: "CarritoDetalles");

            migrationBuilder.DropColumn(
                name: "TallaSeleccionada",
                table: "DetallesPedido");
        }
    }
}
