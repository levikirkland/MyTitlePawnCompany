# Adding MonthlyIncome Column - Manual Database Update

## Issue
The EF Core migration tools are having compatibility issues with .NET 10, so the `MonthlyIncome` column wasn't automatically added to the database.

## Solution Options

### Option 1: Run SQL Script Directly (RECOMMENDED)

1. **Open SQL Server Management Studio (SSMS)** or **Azure Data Studio**
2. **Connect** to your database
3. **Open the file**: `add_monthly_income.sql`
4. **Edit line 4**: Replace `[YourDatabaseName]` with your actual database name
5. **Execute the script** (press F5 or click Execute)

The script will:
- Add the `MonthlyIncome` column if it doesn't exist
- Add a migration history record for EF tracking

### Option 2: Run via Command Line

If you have `sqlcmd` installed:

```bash
sqlcmd -S your_server_name -d your_database_name -i add_monthly_income.sql
```

### Option 3: Manual SQL Query

Simply run this in your database:

```sql
ALTER TABLE [dbo].[Customers]
ADD [MonthlyIncome] decimal(10,2) NULL;

INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250111000000_AddMonthlyIncomeToCustomer', N'10.0.0');
```

## Verification

After running the script, verify the column exists:

```sql
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customers' AND COLUMN_NAME = 'MonthlyIncome';
```

Expected result:
```
COLUMN_NAME     DATA_TYPE    IS_NULLABLE
MonthlyIncome   decimal      YES
```

## Files Added/Modified

### Code Changes:
? `Data/Models/Customer.cs` - Added `MonthlyIncome` property
? `Pages/TitlePawns/Details.cshtml.cs` - Added `OnPostMarkContractSignedAsync()` handler
? `Pages/TitlePawns/Details.cshtml` - Added "Mark Signed" button

### Database Changes Needed:
?? `add_monthly_income.sql` - SQL script to add the column
?? `Migrations/20250111000000_AddMonthlyIncomeToCustomer.cs` - Migration file (for reference)

## Next Steps

After adding the column to the database:

1. **Test the application** - Start debugging and verify no errors
2. **Update Customer forms** to include MonthlyIncome field:
   - `Pages/Customers/Create.cshtml`
   - `Pages/Customers/Edit.cshtml`
   - `Pages/Customers/Details.cshtml`

## Example HTML for Customer Forms

Add this to Create/Edit forms:

```html
<div class="form-group">
    <label asp-for="Customer.MonthlyIncome" class="form-label">Monthly Income</label>
    <div class="input-group">
        <span class="input-group-text">$</span>
        <input asp-for="Customer.MonthlyIncome" type="number" step="0.01" 
               class="form-control" placeholder="0.00" />
    </div>
    <span asp-validation-for="Customer.MonthlyIncome" class="text-danger"></span>
</div>
```

Add this to Details view:

```html
<p><strong>Monthly Income:</strong> 
   @(Model.Customer?.MonthlyIncome.HasValue ? 
     "$" + Model.Customer.MonthlyIncome.Value.ToString("N2") : 
     "Not provided")
</p>
```

## Why This Happened

The EF Core tools have a version mismatch with .NET 10:
```
Microsoft.CodeAnalysis.CSharp.Features 4.8.0 
requires Microsoft.CodeAnalysis.Common (= 4.8.0) 
but version Microsoft.CodeAnalysis.Common 4.14.0 was resolved
```

This causes migration generation to fail. The manual SQL approach bypasses this issue.

## Need Help?

If you encounter any issues:
1. Check database connection string in `appsettings.json`
2. Verify you have permissions to modify the database schema
3. Make sure the database is not in use by other processes
4. Check for any existing `MonthlyIncome` column first
