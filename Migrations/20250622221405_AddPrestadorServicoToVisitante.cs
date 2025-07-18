﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Condominio_API.Migrations
{
    /// <inheritdoc />
    public partial class AddPrestadorServicoToVisitante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PrestadorServico",
                table: "Visitantes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrestadorServico",
                table: "Visitantes");
        }
    }
}
