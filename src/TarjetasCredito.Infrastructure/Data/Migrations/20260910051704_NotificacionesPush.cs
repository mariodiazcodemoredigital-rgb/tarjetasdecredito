using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarjetasCredito.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class NotificacionesPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NotificacionOptimaEnviadaUtc",
                table: "PaymentReminders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NotificacionT1EnviadaUtc",
                table: "PaymentReminders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NotificacionT3EnviadaUtc",
                table: "PaymentReminders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscriptions_Endpoint",
                table: "PushSubscriptions",
                column: "Endpoint",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PushSubscriptions_Endpoint",
                table: "PushSubscriptions");

            migrationBuilder.DropColumn(
                name: "NotificacionOptimaEnviadaUtc",
                table: "PaymentReminders");

            migrationBuilder.DropColumn(
                name: "NotificacionT1EnviadaUtc",
                table: "PaymentReminders");

            migrationBuilder.DropColumn(
                name: "NotificacionT3EnviadaUtc",
                table: "PaymentReminders");
        }
    }
}
