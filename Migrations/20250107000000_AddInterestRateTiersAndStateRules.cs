using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTitlePawnCompany.Migrations
{
    /// <inheritdoc />
    public partial class AddInterestRateTiersAndStateRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create InterestRateTiers table
            migrationBuilder.CreateTable(
                name: "InterestRateTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    TierName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MinimumPrincipal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MaximumPrincipal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterestRateTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterestRateTiers_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create StateSpecialRules table
            migrationBuilder.CreateTable(
                name: "StateSpecialRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    StateCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    StateName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstPeriodDays = table.Column<int>(type: "int", nullable: false, defaultValue: 90),
                    FirstPeriodMaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SubsequentPeriodMaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AdditionalRules = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateSpecialRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StateSpecialRules_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_InterestRateTiers_StoreId",
                table: "InterestRateTiers",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StateSpecialRules_StoreId",
                table: "StateSpecialRules",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StateSpecialRules_StoreId_StateCode",
                table: "StateSpecialRules",
                columns: new[] { "StoreId", "StateCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterestRateTiers");

            migrationBuilder.DropTable(
                name: "StateSpecialRules");
        }
    }
}
