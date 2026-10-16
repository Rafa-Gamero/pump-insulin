using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pumpinsulin.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHidratosYRatioLectura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Alerta",
                table: "Lecturas",
                newName: "EstadoAlerta");

            migrationBuilder.AddColumn<double>(
                name: "HidratosCarbonoG",
                table: "Lecturas",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RatioInsulina",
                table: "Lecturas",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HidratosCarbonoG",
                table: "Lecturas");

            migrationBuilder.DropColumn(
                name: "RatioInsulina",
                table: "Lecturas");

            migrationBuilder.RenameColumn(
                name: "EstadoAlerta",
                table: "Lecturas",
                newName: "Alerta");
        }
    }
}
