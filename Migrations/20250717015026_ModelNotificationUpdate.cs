using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Condominio_API.Migrations
{
    /// <inheritdoc />
    public partial class ModelNotificationUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NiveisAcessoDestino",
                table: "Notificacoes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NiveisAcessoDestino",
                table: "Notificacoes");
        }
    }
}
