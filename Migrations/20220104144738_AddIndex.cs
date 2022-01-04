using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourGameServer.Migrations
{
    public partial class AddIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PlayerAccounts_PlayerId",
                table: "PlayerAccounts",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerAccounts_PlayerId",
                table: "PlayerAccounts");
        }
    }
}
