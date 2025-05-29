using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Condominio_API.Migrations
{
    /// <inheritdoc />
    public partial class AllowUsuarioWithoutApartamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Apartamentos_ApartamentoId",
                table: "Usuarios");

            migrationBuilder.AlterColumn<int>(
                name: "ApartamentoId",
                table: "Usuarios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Apartamentos_ApartamentoId",
                table: "Usuarios",
                column: "ApartamentoId",
                principalTable: "Apartamentos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Apartamentos_ApartamentoId",
                table: "Usuarios");

            migrationBuilder.AlterColumn<int>(
                name: "ApartamentoId",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Apartamentos_ApartamentoId",
                table: "Usuarios",
                column: "ApartamentoId",
                principalTable: "Apartamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
