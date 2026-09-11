using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarjetasCredito.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CrearCardPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CreditCardId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Monto = table.Column<decimal>(type: "TEXT", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Nota = table.Column<string>(type: "TEXT", nullable: true),
                    PaymentReminderId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardPayments_CreditCards_CreditCardId",
                        column: x => x.CreditCardId,
                        principalTable: "CreditCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CardPayments_PaymentReminders_PaymentReminderId",
                        column: x => x.PaymentReminderId,
                        principalTable: "PaymentReminders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardPayments_CreditCardId",
                table: "CardPayments",
                column: "CreditCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardPayments_PaymentReminderId",
                table: "CardPayments",
                column: "PaymentReminderId");

            migrationBuilder.CreateIndex(
                name: "IX_CardPayments_UserId",
                table: "CardPayments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardPayments");
        }
    }
}
