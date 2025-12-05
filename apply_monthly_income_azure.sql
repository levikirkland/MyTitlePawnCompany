-- Add MonthlyIncome column to Azure database
ALTER TABLE [dbo].[Customers]
ADD [MonthlyIncome] decimal(10,2) NULL;

-- Add migration record
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250111000000_AddMonthlyIncomeToCustomer', N'10.0.0');

SELECT 'MonthlyIncome column added successfully!' AS Result;
