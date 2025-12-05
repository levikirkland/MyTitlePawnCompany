-- Add MonthlyIncome column to Customers table
-- Run this script manually if EF migrations aren't applying

USE [YourDatabaseName]  -- Replace with your actual database name
GO

-- Check if column doesn't exist before adding
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') 
               AND name = 'MonthlyIncome')
BEGIN
    ALTER TABLE [dbo].[Customers]
    ADD [MonthlyIncome] decimal(10,2) NULL;
    
    PRINT 'MonthlyIncome column added successfully to Customers table';
END
ELSE
BEGIN
    PRINT 'MonthlyIncome column already exists in Customers table';
END
GO

-- Add migration record to history table (optional - for EF tracking)
IF NOT EXISTS (SELECT * FROM [dbo].[__EFMigrationsHistory] 
               WHERE [MigrationId] = N'20250111000000_AddMonthlyIncomeToCustomer')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250111000000_AddMonthlyIncomeToCustomer', N'10.0.0');
    
    PRINT 'Migration record added to __EFMigrationsHistory';
END
GO
