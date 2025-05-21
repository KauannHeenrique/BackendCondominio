using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Condominio_API.Migrations
{
    /// <inheritdoc />
    public partial class AddQRCodeTemp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QRCodeTemps_Usuarios_MoradorId",
                table: "QRCodeTemps");

            migrationBuilder.DropForeignKey(
                name: "FK_QRCodeTemps_Visitantes_VisitanteId",
                table: "QRCodeTemps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QRCodeTemps",
                table: "QRCodeTemps");

            migrationBuilder.RenameTable(
                name: "QRCodeTemps",
                newName: "QRCodesTemp");

            migrationBuilder.RenameIndex(
                name: "IX_QRCodeTemps_VisitanteId",
                table: "QRCodesTemp",
                newName: "IX_QRCodesTemp_VisitanteId");

            migrationBuilder.RenameIndex(
                name: "IX_QRCodeTemps_MoradorId",
                table: "QRCodesTemp",
                newName: "IX_QRCodesTemp_MoradorId");

            migrationBuilder.AddColumn<byte[]>(
                name: "QrCodeImagem",
                table: "QRCodesTemp",
                type: "longblob",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_QRCodesTemp",
                table: "QRCodesTemp",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodesTemp_Usuarios_MoradorId",
                table: "QRCodesTemp",
                column: "MoradorId",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodesTemp_Visitantes_VisitanteId",
                table: "QRCodesTemp",
                column: "VisitanteId",
                principalTable: "Visitantes",
                principalColumn: "VisitanteId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QRCodesTemp_Usuarios_MoradorId",
                table: "QRCodesTemp");

            migrationBuilder.DropForeignKey(
                name: "FK_QRCodesTemp_Visitantes_VisitanteId",
                table: "QRCodesTemp");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QRCodesTemp",
                table: "QRCodesTemp");

            migrationBuilder.DropColumn(
                name: "QrCodeImagem",
                table: "QRCodesTemp");

            migrationBuilder.RenameTable(
                name: "QRCodesTemp",
                newName: "QRCodeTemps");

            migrationBuilder.RenameIndex(
                name: "IX_QRCodesTemp_VisitanteId",
                table: "QRCodeTemps",
                newName: "IX_QRCodeTemps_VisitanteId");

            migrationBuilder.RenameIndex(
                name: "IX_QRCodesTemp_MoradorId",
                table: "QRCodeTemps",
                newName: "IX_QRCodeTemps_MoradorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QRCodeTemps",
                table: "QRCodeTemps",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodeTemps_Usuarios_MoradorId",
                table: "QRCodeTemps",
                column: "MoradorId",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QRCodeTemps_Visitantes_VisitanteId",
                table: "QRCodeTemps",
                column: "VisitanteId",
                principalTable: "Visitantes",
                principalColumn: "VisitanteId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
