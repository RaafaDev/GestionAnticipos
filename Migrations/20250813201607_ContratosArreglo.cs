using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionAnticiposApp.Migrations
{
    /// <inheritdoc />
    public partial class ContratosArreglo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcesosVinculadosId",
                table: "Contratos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcesosVinculadosId",
                table: "Contratos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
