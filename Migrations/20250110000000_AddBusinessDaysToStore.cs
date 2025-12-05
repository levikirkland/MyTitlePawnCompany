using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTitlePawnCompany.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessDaysToStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AccrueLateFeesSunday",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AccrueLateFeesSaturday",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "LateFeeAccrualHour",
                table: "Stores",
                type: "int",
                nullable: false,
                defaultValue: 18); // 6:00 PM
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccrueLateFeesSunday",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "AccrueLateFeesSaturday",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "LateFeeAccrualHour",
                table: "Stores");
        }
    }
}
