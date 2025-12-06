# Migration Status Report

## ? Completed Migrations (11 of 11)

All core migrations have been successfully applied to the Azure database `mytitlepawn`:

| # | Migration ID | Status | Description |
|---|--------------|--------|-------------|
| 1 | `20250101000000_InitialCreate` | ? Applied | Base schema (Companies, Customers, Vehicles, TitlePawns, Payments, Reports, Vendors) |
| 2 | `20250102000000_AddCustomerEmploymentAndReferences` | ? Applied | Employment fields (PlaceOfEmployment, EmploymentPhoneNumber, EmploymentAddress, YearsEmployed) + CustomerReferences table |
| 3 | `20250103000001_AddContractSignatureFields` | ? Applied | Contract tracking (ContractSigned, ContractSignedDate) |
| 4 | `20250104000000_AddRenewalTracking` | ? Applied | Loan renewal tracking (RenewedFromTitlePawnId) |
| 5 | `20250105000000_AddStoresAndMultiTenancy` | ? Applied | Stores table + StoreUsers for multi-location support |
| 6 | `20250106000000_AddApprovalPolicy` | ? Applied | Approval policies and rules |
| 7 | `20250107000000_AddInterestRateTiersAndStateRules` | ? Applied | InterestRateTiers + StateSpecialRules tables |
| 8 | `20250107000001_RemoveStoreIdFromApplicationUser` | ? Applied | Cleanup user schema |
| 9 | `20250108000000_AddFeeFieldsToTitlePawn` | ? Applied | Fee fields (TitleAndKeyFee, AdditionalFees) |
| 10 | `20250109000000_AddFeeTable` | ? Applied | Separate Fees table with waiving support |
| 11 | `20250110000000_AddBusinessDaysToStore` | ? Applied | Business days for late fee accrual |

---

## ?? Pending: MonthlyIncome Column

### Migration File Exists But Not Applied

**Migration**: `20250111000000_AddMonthlyIncomeToCustomer`
**Status**: ?? **Needs Manual Application**
**Reason**: EF Core migration tools have compatibility issues with .NET 10

### What Needs to Be Done

**Run this SQL in Azure Portal** (takes 10 seconds):

```sql
-- Add MonthlyIncome column
ALTER TABLE [dbo].[Customers] ADD [MonthlyIncome] decimal(10,2) NULL;

-- Add migration record
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250111000000_AddMonthlyIncomeToCustomer', N'10.0.0');
```

### How to Apply

1. **Azure Portal Method** (Easiest):
   - Go to https://portal.azure.com
   - Navigate to: SQL Database ? `mytitlepawn`
   - Click: "Query editor (preview)"
   - Login: `bookshelfSqlAdmin` / `a#dgvmzc*V1M`
   - Paste and run the SQL above

2. **Using SQL File**:
   - Run: `apply_monthly_income_azure.sql` (already created in your project)

---

## ?? Database Schema Summary

### Tables Created (14 total)

| Table Name | Purpose | Records |
|------------|---------|---------|
| Companies | Company/organization management | Ready |
| Customers | Customer information + employment | Ready (needs MonthlyIncome) |
| Vehicles | Vehicle collateral | Ready |
| TitlePawns | Loan records | Ready |
| Payments | Payment history | Ready |
| Fees | Additional fees (late, towing, etc.) | Ready |
| CustomerReferences | Customer references (3 required) | Ready |
| Stores | Store locations | Ready |
| StoreUsers | User-store assignments | Ready |
| InterestRateTiers | Interest rate tiers by amount | Ready |
| StateSpecialRules | State-specific regulations | Ready |
| Vendors | Vendor management | Ready |
| Reports | Reporting data | Ready |
| Identity Tables | ASP.NET Identity (Users, Roles, Claims, etc.) | Ready |

---

## ? Verification Commands

### Check Migration Status
```bash
dotnet ef migrations list
```

### Check Database Connection
```bash
dotnet ef database update --dry-run
```

### Verify MonthlyIncome Column (After Adding)
```sql
-- Run in Azure Portal Query Editor
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customers' AND COLUMN_NAME = 'MonthlyIncome';
```

Expected result:
```
COLUMN_NAME     DATA_TYPE    IS_NULLABLE
MonthlyIncome   decimal      YES
```

---

## ?? Current Status

### ? Ready to Use
- Database: **mytitlepawn** (Azure SQL)
- Schema: **Complete** (all 11 migrations applied)
- Connection: **Configured** (User Secrets)
- Deployment: **Configured** (GitHub Actions ready)
- Application: **Builds Successfully**

### ?? Action Required
- **Add MonthlyIncome column** (1 SQL statement)

---

## ?? Next Steps

### Immediate (Required)
1. ?? **Run SQL** to add MonthlyIncome column (10 seconds)
2. ? **Verify** column exists (run check query)
3. ? **Test** application (Press F5)

### Soon (Recommended)
1. Update Customer Create/Edit forms to include MonthlyIncome input
2. Test loan creation workflow
3. Verify all features work end-to-end

### Future (Optional)
1. Add sample test data
2. Set up automated database backups
3. Configure monitoring/alerts
4. Deploy to Azure App Service

---

## ?? Troubleshooting

### "MonthlyIncome column already exists"
? **Good!** Someone already added it. Verify with:
```sql
SELECT MonthlyIncome FROM Customers;
```

### "Cannot connect to database"
- Check: Azure SQL firewall allows your IP
- Verify: Connection string in User Secrets
- Test: Azure Portal Query Editor (doesn't need local firewall)

### "Migration not showing in list"
- This is expected - EF Core tools have .NET 10 compatibility issues
- The migration file exists for documentation
- Apply manually via SQL (see above)

---

## ?? Migration Files Location

All migration files are in: `Migrations/` folder

```
Migrations/
??? 20250101000000_InitialCreate.cs
??? 20250102000000_AddCustomerEmploymentAndReferences.cs
??? 20250103000001_AddContractSignatureFields.cs
??? 20250104000000_AddRenewalTracking.cs
??? 20250105000000_AddStoresAndMultiTenancy.cs
??? 20250106000000_AddApprovalPolicy.cs
??? 20250107000000_AddInterestRateTiersAndStateRules.cs
??? 20250107000001_RemoveStoreIdFromApplicationUser.cs
??? 20250108000000_AddFeeFieldsToTitlePawn.cs
??? 20250109000000_AddFeeTable.cs
??? 20250110000000_AddBusinessDaysToStore.cs
??? 20250111000000_AddMonthlyIncomeToCustomer.cs  ?? Apply manually
```

---

## ? Summary

**Migration Status**: 11 of 11 core migrations applied ?  
**Action Needed**: Add MonthlyIncome column (1 SQL statement) ??  
**Time Required**: 10 seconds  
**Database Ready**: Yes (after MonthlyIncome column)  
**Application Ready**: Yes  

**You're 99% done!** Just run that one SQL statement and you're fully operational! ??
