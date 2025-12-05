# ?? Azure Deployment Guide - GitHub Actions

## Files Created

? `.deployment` - Azure deployment configuration
? `deploy.cmd` - Kudu deployment script
? `.github/workflows/azure-deploy.yml` - GitHub Actions CI/CD workflow

---

## ?? Deployment Options

### Option 1: GitHub Actions (RECOMMENDED) ?
**Automated CI/CD** - Deploys automatically on every push to `main` branch

### Option 2: Azure Portal Deployment Center
**Direct from GitHub** - Simple setup through Azure Portal

### Option 3: Manual Deployment
**Git push** - Push directly to Azure Git repository

---

## ?? Setup: GitHub Actions Deployment

### Step 1: Create Azure App Service

1. **Go to Azure Portal**: https://portal.azure.com
2. **Create App Service**:
   - Click "Create a resource"
   - Search for "Web App"
   - Click "Create"

3. **Configure Web App**:
   ```
   Resource Group: Create new or use existing
   Name: mytitlepawn-app (or your preferred name)
   Publish: Code
   Runtime stack: .NET 10
   Operating System: Windows or Linux
   Region: Choose closest to your users
   App Service Plan: Choose appropriate tier (F1 Free, B1 Basic, S1 Standard, etc.)
   ```

4. **Click "Review + Create"** ? **Create**

### Step 2: Configure Connection String

1. In Azure Portal, go to your App Service
2. Navigate to: **Configuration** ? **Connection strings**
3. Click **+ New connection string**
4. Add:
   ```
   Name: DefaultConnection
   Value: Server=tcp:mybookshelfserver.database.windows.net,1433;Initial Catalog=mytitlepawn;Persist Security Info=False;User ID=bookshelfSqlAdmin;Password=a#dgvmzc*V1M;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   Type: SQLAzure
   ```
5. Click **OK** ? **Save**

### Step 3: Get Publish Profile

1. In Azure Portal, go to your App Service
2. Click **Get publish profile** (top menu)
3. Save the downloaded `.PublishSettings` file
4. Open the file and **copy the entire contents**

### Step 4: Add GitHub Secret

1. Go to your GitHub repository: https://github.com/levikirkland/MyTitlePawnCompany
2. Click **Settings** ? **Secrets and variables** ? **Actions**
3. Click **New repository secret**
4. Add:
   ```
   Name: AZURE_WEBAPP_PUBLISH_PROFILE
   Value: [Paste the entire publish profile contents]
   ```
5. Click **Add secret**

### Step 5: Update Workflow File

Edit `.github/workflows/azure-deploy.yml`:

```yaml
env:
  AZURE_WEBAPP_NAME: mytitlepawn-app    # ? Change this to YOUR App Service name
  AZURE_WEBAPP_PACKAGE_PATH: '.'
  DOTNET_VERSION: '10.0.x'
```

### Step 6: Push to GitHub

```bash
git add .
git commit -m "Add Azure deployment configuration"
git push origin main
```

### Step 7: Watch Deployment

1. Go to GitHub repository
2. Click **Actions** tab
3. You'll see your workflow running
4. Click on the workflow to see progress
5. Once complete (green checkmark), your app is deployed!

---

## ?? Access Your Application

After successful deployment:

```
https://mytitlepawn-app.azurewebsites.net
```

*(Replace `mytitlepawn-app` with your actual App Service name)*

---

## ?? Setup: Azure Portal Deployment (Alternative)

### Quick Setup

1. **Azure Portal** ? Your App Service
2. **Deployment Center** ? **GitHub**
3. **Authorize** GitHub
4. **Select**:
   - Organization: `levikirkland`
   - Repository: `MyTitlePawnCompany`
   - Branch: `main`
5. **Save**

Azure will automatically set up the deployment pipeline!

---

## ?? Workflow Explanation

### What Happens on Push to `main`:

1. **Build Job**:
   - ? Checkout code from GitHub
   - ? Setup .NET 10
   - ? Restore NuGet packages
   - ? Build in Release mode
   - ? Run tests
   - ? Publish application
   - ? Upload build artifacts

2. **Deploy Job**:
   - ? Download build artifacts
   - ? Deploy to Azure App Service
   - ? Start application
   - ? Run database migrations (automatic)

### Total Time: ~3-5 minutes ??

---

## ?? Environment Configuration

### Production Settings (Azure)

Configure in Azure Portal ? Configuration ? Application settings:

```
ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_HTTPS_PORT = 443
WEBSITE_RUN_FROM_PACKAGE = 1
```

### Connection Strings

Already configured in Step 2 above. The app will automatically use:
- Azure SQL Database: `mytitlepawn`
- Server: `mybookshelfserver.database.windows.net`

---

## ?? Important: Database Migrations

### First Deployment

The database is already set up with all migrations applied! ?

If you need to run migrations manually:

```bash
# From Azure Portal ? Console (Kudu)
cd site\wwwroot
dotnet ef database update
```

### Adding New Migrations

1. Create migration locally:
   ```bash
   dotnet ef migrations add YourMigrationName
   ```

2. Push to GitHub:
   ```bash
   git add .
   git commit -m "Add new migration"
   git push origin main
   ```

3. Migrations run automatically on deployment! ?

---

## ?? Monitoring & Logs

### View Application Logs

**Azure Portal** ? Your App Service ? **Log stream**

### View Deployment Logs

**GitHub** ? Actions tab ? Click workflow run

### Application Insights (Optional)

1. Azure Portal ? Create **Application Insights**
2. Copy Instrumentation Key
3. Add to App Settings:
   ```
   APPLICATIONINSIGHTS_CONNECTION_STRING = [Your connection string]
   ```

---

## ?? Troubleshooting

### Deployment Fails

1. **Check GitHub Actions logs**:
   - Go to Actions tab
   - Click failed workflow
   - Expand failed steps

2. **Common Issues**:
   - ? Wrong App Service name ? Update workflow YAML
   - ? Invalid publish profile ? Re-download and update secret
   - ? Build errors ? Check code compiles locally first

### Application Won't Start

1. **Check Application Logs**:
   - Azure Portal ? Log stream

2. **Common Issues**:
   - ? Missing connection string ? Check Configuration
   - ? Firewall blocking Azure ? Add Azure IPs to SQL firewall
   - ? Missing environment variables

### Database Connection Fails

1. **Azure SQL Firewall**:
   - Go to Azure SQL Server
   - Firewalls and virtual networks
   - Enable: "Allow Azure services and resources to access this server"
   - Add your IP if testing locally

---

## ?? Cost Considerations

### App Service Tiers

| Tier | Cost/Month | Features |
|------|------------|----------|
| F1 (Free) | $0 | 1 GB RAM, 60 min/day CPU, No custom domain |
| B1 (Basic) | ~$13 | 1.75 GB RAM, Custom domain, SSL |
| S1 (Standard) | ~$70 | 1.75 GB RAM, Auto-scale, Slots |
| P1v2 (Premium) | ~$146 | 3.5 GB RAM, Better performance |

### SQL Database

Already created and configured: `mytitlepawn`
- Check current tier in Azure Portal
- Adjust as needed based on usage

---

## ?? Next Steps After Deployment

### Immediate

1. ? Visit your app URL
2. ? Register first admin user
3. ? Create company
4. ? Create store
5. ? Test loan creation

### Configuration

1. Enable HTTPS redirect
2. Configure custom domain (optional)
3. Set up SSL certificate
4. Configure CORS if needed
5. Set up backup schedule

### Security

1. ?? Change database password
2. Enable Azure AD authentication
3. Configure IP restrictions (optional)
4. Set up Key Vault for secrets
5. Enable managed identity

---

## ?? Deployment Checklist

- [ ] Azure App Service created
- [ ] Connection string configured
- [ ] Publish profile downloaded
- [ ] GitHub secret added
- [ ] Workflow YAML updated with app name
- [ ] Code pushed to GitHub
- [ ] Deployment successful (green checkmark)
- [ ] Application accessible at URL
- [ ] Database connection working
- [ ] First user registered
- [ ] SSL/HTTPS working

---

## ?? Need Help?

### GitHub Actions Not Running?

1. Check: Settings ? Actions ? Allow all actions
2. Verify: `.github/workflows/` folder exists
3. Check: Branch name is `main` (not `master`)

### Can't Access Application?

1. Check: Azure Portal ? App Service ? Browse
2. Verify: App Service is running (not stopped)
3. Check: Firewall rules allow your IP

### Database Issues?

1. Test connection: Azure Portal ? SQL Database ? Query editor
2. Check: Firewall allows Azure services
3. Verify: Connection string is correct

---

## ?? You're Ready!

Your application is now set up for **continuous deployment**:

1. ? Push code to GitHub `main` branch
2. ? GitHub Actions automatically builds
3. ? Deploys to Azure App Service
4. ? Application updates live in ~5 minutes

**Happy Deploying!** ??
