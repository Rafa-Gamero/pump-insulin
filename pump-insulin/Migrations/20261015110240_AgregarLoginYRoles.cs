using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pumpinsulin.Migrations
{
    /// <inheritdoc />
    public partial class AgregarLoginYRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_Medicos_MedicoId",
                table: "Pacientes");

            migrationBuilder.AlterColumn<int>(
                name: "MedicoId",
                table: "Pacientes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Pacientes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Pacientes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Medicos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Medicos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_Medicos_MedicoId",
                table: "Pacientes",
                column: "MedicoId",
                principalTable: "Medicos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_Medicos_MedicoId",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Medicos");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Medicos");

            migrationBuilder.AlterColumn<int>(
                name: "MedicoId",
                table: "Pacientes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_Medicos_MedicoId",
                table: "Pacientes",
                column: "MedicoId",
                principalTable: "Medicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
