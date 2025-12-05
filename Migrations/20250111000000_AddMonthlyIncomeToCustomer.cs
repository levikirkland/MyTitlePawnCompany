using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTitlePawnCompany.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyIncomeToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyIncome",
                table: "Customers",
                type: "decimal(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyIncome",
                table: "Customers");
        }
    }
}
