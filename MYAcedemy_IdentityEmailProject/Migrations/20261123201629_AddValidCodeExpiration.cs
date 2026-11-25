using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MYAcedemy_IdentityEmailProject.Migrations
{
    /// <inheritdoc />
    public partial class AddValidCodeExpiration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ValidCodeExpiresAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidCodeExpiresAt",
                table: "AspNetUsers");
        }
    }
}
