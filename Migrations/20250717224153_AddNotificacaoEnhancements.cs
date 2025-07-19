using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Condominio_API.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificacaoEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificacaoHistoricos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NotificacaoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Acao = table.Column<int>(type: "int", nullable: false),
                    StatusAnterior = table.Column<int>(type: "int", nullable: true),
                    StatusNovo = table.Column<int>(type: "int", nullable: true),
                    Comentario = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataRegistro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacaoHistoricos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificacaoHistoricos_Notificacoes_NotificacaoId",
                        column: x => x.NotificacaoId,
                        principalTable: "Notificacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificacaoHistoricos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacaoHistoricos_NotificacaoId",
                table: "NotificacaoHistoricos",
                column: "NotificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacaoHistoricos_UsuarioId",
                table: "NotificacaoHistoricos",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificacaoHistoricos");
        }
    }
}
