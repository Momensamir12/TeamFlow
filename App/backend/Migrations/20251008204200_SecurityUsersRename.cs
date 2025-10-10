using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace app.Migrations
{
    /// <inheritdoc />
    public partial class SecurityUsersRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "SecurityUsers");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Username",
                table: "SecurityUsers",
                newName: "IX_SecurityUsers_Username");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SecurityUsers",
                table: "SecurityUsers",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SecurityUsers",
                table: "SecurityUsers");

            migrationBuilder.RenameTable(
                name: "SecurityUsers",
                newName: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_SecurityUsers_Username",
                table: "Users",
                newName: "IX_Users_Username");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");
        }
    }
}
