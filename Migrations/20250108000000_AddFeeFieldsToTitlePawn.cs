using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTitlePawnCompany.Migrations
{
    /// <inheritdoc />
    public partial class AddFeeFieldsToTitlePawn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add TitleAndKeyFee column to TitlePawns table
            migrationBuilder.AddColumn<decimal>(
                name: "TitleAndKeyFee",
                table: "TitlePawns",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            // Add AdditionalFees column to TitlePawns table
            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalFees",
                table: "TitlePawns",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            // Add TitleAndKeyFee column to Stores table
            migrationBuilder.AddColumn<decimal>(
                name: "TitleAndKeyFee",
                table: "Stores",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 25.00m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove columns in reverse order
            migrationBuilder.DropColumn(
                name: "TitleAndKeyFee",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "AdditionalFees",
                table: "TitlePawns");

            migrationBuilder.DropColumn(
                name: "TitleAndKeyFee",
                table: "TitlePawns");
        }
    }
}
