using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Condominio_API.Migrations
{
    /// <inheritdoc />
    public partial class AdicaoFluxoAprovacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ParaTodos",
                table: "Notificacoes",
                newName: "CriadoPorSindico");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CriadoPorSindico",
                table: "Notificacoes",
                newName: "ParaTodos");
        }
    }
}
