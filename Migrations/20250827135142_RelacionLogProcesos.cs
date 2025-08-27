using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionAnticiposApp.Migrations
{
    /// <inheritdoc />
    public partial class RelacionLogProcesos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Documentos");

            migrationBuilder.AlterColumn<string>(
                name: "Funcionario",
                table: "ProcesosVinculados",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ProcesoVinculadoId",
                table: "Logs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Logs_ProcesoVinculadoId",
                table: "Logs",
                column: "ProcesoVinculadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Logs_ProcesosVinculados_ProcesoVinculadoId",
                table: "Logs",
                column: "ProcesoVinculadoId",
                principalTable: "ProcesosVinculados",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logs_ProcesosVinculados_ProcesoVinculadoId",
                table: "Logs");

            migrationBuilder.DropIndex(
                name: "IX_Logs_ProcesoVinculadoId",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "ProcesoVinculadoId",
                table: "Logs");

            migrationBuilder.AlterColumn<string>(
                name: "Funcionario",
                table: "ProcesosVinculados",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Documentos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "Documentos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
