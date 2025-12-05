using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTitlePawnCompany.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerEmploymentAndReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmploymentAddress",
                table: "Customers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmploymentPhoneNumber",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfEmployment",
                table: "Customers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "YearsEmployed",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TitleAndKeyFee",
                table: "Companies",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 25.00m);

            migrationBuilder.CreateTable(
                name: "CustomerReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Relationship = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerReferences_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReferences_CustomerId",
                table: "CustomerReferences",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerReferences");

            migrationBuilder.DropColumn(
                name: "EmploymentAddress",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "EmploymentPhoneNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PlaceOfEmployment",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "YearsEmployed",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TitleAndKeyFee",
                table: "Companies");
        }
    }
}
