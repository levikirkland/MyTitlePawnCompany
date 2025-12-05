using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTitlePawnCompany.Migrations
{
    /// <inheritdoc />
    public partial class AddRenewalTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RenewedFromTitlePawnId",
                table: "TitlePawns",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TitlePawns_RenewedFromTitlePawnId",
                table: "TitlePawns",
                column: "RenewedFromTitlePawnId");

            migrationBuilder.AddForeignKey(
                name: "FK_TitlePawns_TitlePawns_RenewedFromTitlePawnId",
                table: "TitlePawns",
                column: "RenewedFromTitlePawnId",
                principalTable: "TitlePawns",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TitlePawns_TitlePawns_RenewedFromTitlePawnId",
                table: "TitlePawns");

            migrationBuilder.DropIndex(
                name: "IX_TitlePawns_RenewedFromTitlePawnId",
                table: "TitlePawns");

            migrationBuilder.DropColumn(
                name: "RenewedFromTitlePawnId",
                table: "TitlePawns");
        }
    }
}
