using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTitlePawnCompany.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStoreIdFromApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if StoreId column exists in AspNetUsers and remove it if it does
            migrationBuilder.Sql(
                @"IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                  WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'StoreId')
                BEGIN
                    ALTER TABLE [AspNetUsers] DROP COLUMN [StoreId]
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This is a data cleanup migration, no need to restore StoreId
        }
    }
}
