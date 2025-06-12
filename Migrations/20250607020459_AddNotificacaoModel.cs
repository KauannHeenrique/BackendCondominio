using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Condominio_API.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificacaoModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notificacoes_Apartamentos_ApartamentoDestinoId",
                table: "Notificacoes");

            migrationBuilder.RenameColumn(
                name: "DataHora",
                table: "Notificacoes",
                newName: "DataCriacao");

            migrationBuilder.AlterColumn<string>(
                name: "Mensagem",
                table: "Notificacoes",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "ApartamentoDestinoId",
                table: "Notificacoes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ComentarioSindico",
                table: "Notificacoes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Notificacoes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Notificacoes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "Notificacoes",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaAtualizacao",
                table: "Notificacoes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notificacoes_Apartamentos_ApartamentoDestinoId",
                table: "Notificacoes",
                column: "ApartamentoDestinoId",
                principalTable: "Apartamentos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notificacoes_Apartamentos_ApartamentoDestinoId",
                table: "Notificacoes");

            migrationBuilder.DropColumn(
                name: "ComentarioSindico",
                table: "Notificacoes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Notificacoes");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Notificacoes");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "Notificacoes");

            migrationBuilder.DropColumn(
                name: "UltimaAtualizacao",
                table: "Notificacoes");

            migrationBuilder.RenameColumn(
                name: "DataCriacao",
                table: "Notificacoes",
                newName: "DataHora");

            migrationBuilder.AlterColumn<string>(
                name: "Mensagem",
                table: "Notificacoes",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "ApartamentoDestinoId",
                table: "Notificacoes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notificacoes_Apartamentos_ApartamentoDestinoId",
                table: "Notificacoes",
                column: "ApartamentoDestinoId",
                principalTable: "Apartamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
