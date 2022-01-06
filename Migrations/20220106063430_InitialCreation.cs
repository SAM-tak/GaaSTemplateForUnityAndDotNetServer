using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourGameServer.Migrations
{
    public partial class InitialCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Luid = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Since = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    InactivateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BanDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpireDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProfileId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerDevices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerId = table.Column<long>(type: "INTEGER", nullable: false),
                    DeviceType = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: true),
                    Since = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerDevices_PlayerAccounts_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "PlayerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerProfiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Motto = table.Column<string>(type: "TEXT", nullable: true),
                    IconBlobId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerProfiles_PlayerAccounts_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "PlayerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAccounts_Luid",
                table: "PlayerAccounts",
                column: "Luid");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerDevices_OwnerId",
                table: "PlayerDevices",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerProfiles_OwnerId",
                table: "PlayerProfiles",
                column: "OwnerId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerDevices");

            migrationBuilder.DropTable(
                name: "PlayerProfiles");

            migrationBuilder.DropTable(
                name: "PlayerAccounts");
        }
    }
}
