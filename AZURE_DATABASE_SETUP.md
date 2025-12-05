# Azure Database Setup Complete! ?

## What Was Done

### ? Database Created
- **Server**: `mybookshelfserver.database.windows.net`
- **Database**: `mytitlepawn`
- **Status**: All migrations applied successfully!

### ? Migrations Applied (11 total)
1. ? `20250101000000_InitialCreate` - Base schema
2. ? `20250102000000_AddCustomerEmploymentAndReferences` - Employment fields
3. ? `20250103000001_AddContractSignatureFields` - Contract tracking
4. ? `20250104000000_AddRenewalTracking` - Loan renewals
5. ? `20250105000000_AddStoresAndMultiTenancy` - Multi-store support
6. ? `20250106000000_AddApprovalPolicy` - Approval rules
7. ? `20250107000000_AddInterestRateTiersAndStateRules` - Interest tiers
8. ? `20250107000001_RemoveStoreIdFromApplicationUser` - User cleanup
9. ? `20250108000000_AddFeeFieldsToTitlePawn` - Fee tracking
10. ? `20250109000000_AddFeeTable` - Separate fees table
11. ? `20250110000000_AddBusinessDaysToStore` - Late fee business days

### ?? One More Step Needed
**MonthlyIncome Column**: This needs to be added manually

---

## ?? Final Step: Add MonthlyIncome Column

### Option 1: Using Azure Portal (EASIEST)
1. Go to https://portal.azure.com
2. Navigate to SQL Database ? `mytitlepawn`
3. Click "Query editor"
4. Login with:
   - Username: `bookshelfSqlAdmin`
   - Password: `a#dgvmzc*V1M`
5. Run this SQL:
   ```sql
   ALTER TABLE [dbo].[Customers]
   ADD [MonthlyIncome] decimal(10,2) NULL;

   INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
   VALUES (N'20250111000000_AddMonthlyIncomeToCustomer', N'10.0.0');
   ```
6. Click "Run"

### Option 2: Using SQL Server Management Studio
1. Connect to: `mybookshelfserver.database.windows.net`
2. Login: `bookshelfSqlAdmin` / `a#dgvmzc*V1M`
3. Open file: `apply_monthly_income_azure.sql`
4. Execute (F5)

### Option 3: Using Azure Data Studio
1. Connect to Azure SQL Database
2. Use connection string from user secrets
3. Open `apply_monthly_income_azure.sql`
4. Run the script

---

## ?? Database Tables Created

| Table Name | Purpose |
|------------|---------|
| Companies | Company/organization data |
| Customers | Customer information with employment fields |
| Vehicles | Vehicle collateral |
| TitlePawns | Loan records |
| Payments | Payment history |
| Fees | Additional fees and charges |
| CustomerReferences | Customer references |
| Stores | Physical store locations |
| StoreUsers | User-store assignments |
| InterestRateTiers | Interest rate tiers |
| StateSpecialRules | State-specific regulations |
| Vendors | Vendor management |
| Reports | Reporting data |
| Identity Tables | ASP.NET Identity (Users, Roles, etc.) |

---

## ?? Connection String Configuration

### Current Setup (User Secrets)
Location: `%APPDATA%\Microsoft\UserSecrets\150352ef-f1a8-4fc8-91e8-50895d94939b\secrets.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:mybookshelfserver.database.windows.net,1433;Initial Catalog=mytitlepawn;..."
  }
}
```

? This is correct and will be used when running the application.

---

## ? Verification Steps

After adding MonthlyIncome column, verify setup:

### 1. Check Tables Exist
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

### 2. Verify MonthlyIncome Column
```sql
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customers'
ORDER BY ORDINAL_POSITION;
```

### 3. Check Migration History
```sql
SELECT * FROM __EFMigrationsHistory
ORDER BY MigrationId;
```
Should show 12 migrations (11 applied + MonthlyIncome)

---

## ?? Next Steps

1. ? Add MonthlyIncome column (see above)
2. ? Start the application (`F5` in Visual Studio)
3. ? Application will connect to Azure automatically
4. ? Create test data through the UI

---

## ?? Security Notes

- ? Connection string stored in User Secrets (not in source control)
- ? Azure SQL requires SSL/TLS (Encrypt=True)
- ? Firewall: Make sure your IP is allowed in Azure portal
- ?? Password is visible in this doc - consider changing after setup

---

## ?? Troubleshooting

### "Cannot connect to server"
- Check Azure firewall rules
- Verify your IP address is allowed
- Test connection in Azure Portal query editor

### "Login failed for user"
- Verify username: `bookshelfSqlAdmin`
- Verify password: `a#dgvmzc*V1M`
- Check database name: `mytitlepawn`

### "Table already exists" errors
- Database is already set up
- Just add MonthlyIncome column if missing
- Application should work immediately

---

## ?? Database Size & Costs

- **Current Size**: ~5-10 MB (empty schema)
- **Azure Tier**: (Check in Azure portal)
- **Estimated Cost**: Depends on tier selected

---

## ? Ready to Use!

Your Azure database is **ready to go**! Just add the MonthlyIncome column and you can start using the application.

**Connection**: Application ? User Secrets ? Azure SQL Database
**Status**: ?? All migrations applied
**Action Needed**: ?? Add MonthlyIncome column (one SQL statement)
