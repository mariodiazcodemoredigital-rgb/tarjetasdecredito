using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarjetasCredito.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPasskeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPasskeys",
                columns: table => new
                {
                    CredentialId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Data_PublicKey = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Data_Name = table.Column<string>(type: "TEXT", nullable: true),
                    Data_CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Data_SignCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    Data_Transports = table.Column<string>(type: "TEXT", nullable: true),
                    Data_IsUserVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    Data_IsBackupEligible = table.Column<bool>(type: "INTEGER", nullable: false),
                    Data_IsBackedUp = table.Column<bool>(type: "INTEGER", nullable: false),
                    Data_AttestationObject = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Data_ClientDataJson = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPasskeys", x => x.CredentialId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPasskeys_UserId",
                table: "UserPasskeys",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPasskeys");
        }
    }
}
