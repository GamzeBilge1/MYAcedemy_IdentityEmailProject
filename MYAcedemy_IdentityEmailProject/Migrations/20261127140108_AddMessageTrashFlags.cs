using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MYAcedemy_IdentityEmailProject.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageTrashFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedByReceiver",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedBySender",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrashedByReceiver",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrashedBySender",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeletedByReceiver",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsDeletedBySender",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsTrashedByReceiver",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsTrashedBySender",
                table: "Messages");
        }
    }
}
