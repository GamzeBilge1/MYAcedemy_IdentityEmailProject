using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MYAcedemy_IdentityEmailProject.Migrations
{
    /// <inheritdoc />
    public partial class AddSenderFavoriteToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSenderFavorite",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSenderFavorite",
                table: "Messages");
        }
    }
}
