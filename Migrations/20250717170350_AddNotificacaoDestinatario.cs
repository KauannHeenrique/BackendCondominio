using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Condominio_API.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificacaoDestinatario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notificacoes_Apartamentos_ApartamentoDestinoId",
                table: "Notificacoes");

            migrationBuilder.DropIndex(
                name: "IX_Notificacoes_ApartamentoDestinoId",
                table: "Notificacoes");

            migrationBuilder.DropColumn(
                name: "ApartamentoDestinoId",
                table: "Notificacoes");

            migrationBuilder.DropColumn(
                name: "NiveisAcessoDestino",
                table: "Notificacoes");

            migrationBuilder.CreateTable(
                name: "NotificacaoDestinatarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NotificacaoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioDestinoId = table.Column<int>(type: "int", nullable: false),
                    Lido = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacaoDestinatarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificacaoDestinatarios_Notificacoes_NotificacaoId",
                        column: x => x.NotificacaoId,
                        principalTable: "Notificacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificacaoDestinatarios_Usuarios_UsuarioDestinoId",
                        column: x => x.UsuarioDestinoId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacaoDestinatarios_NotificacaoId",
                table: "NotificacaoDestinatarios",
                column: "NotificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacaoDestinatarios_UsuarioDestinoId",
                table: "NotificacaoDestinatarios",
                column: "UsuarioDestinoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificacaoDestinatarios");

            migrationBuilder.AddColumn<int>(
                name: "ApartamentoDestinoId",
                table: "Notificacoes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NiveisAcessoDestino",
                table: "Notificacoes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_ApartamentoDestinoId",
                table: "Notificacoes",
                column: "ApartamentoDestinoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notificacoes_Apartamentos_ApartamentoDestinoId",
                table: "Notificacoes",
                column: "ApartamentoDestinoId",
                principalTable: "Apartamentos",
                principalColumn: "Id");
        }
    }
}
