using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionAnticiposApp.Migrations
{
    /// <inheritdoc />
    public partial class AddComentariosToProcesosVinculados : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "Comentarios",
                table: "ProcesosVinculados",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comentarios",
                table: "ProcesosVinculados");

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
