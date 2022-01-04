using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourGameServer.Migrations
{
    public partial class RemoveSecretColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Secret",
                table: "PlayerAccounts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Secret",
                table: "PlayerAccounts",
                type: "BLOB",
                maxLength: 64,
                nullable: true);
        }
    }
}
