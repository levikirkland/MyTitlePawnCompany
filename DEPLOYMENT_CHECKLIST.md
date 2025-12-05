# Quick Deployment Checklist

## ? Pre-Deployment (Already Done)
- [x] Azure SQL Database created (`mytitlepawn`)
- [x] All migrations applied
- [x] Code builds successfully
- [x] Features tested locally

## ?? Deployment Setup (Do This Now)

### 1. Create Azure App Service (5 min)
- [ ] Go to https://portal.azure.com
- [ ] Create Web App
  - Name: `mytitlepawn-app` (or choose your own)
  - Runtime: .NET 10
  - Region: Choose closest
  - Plan: Choose tier (F1 Free to start)
- [ ] Click Create

### 2. Configure Connection String (2 min)
- [ ] App Service ? Configuration ? Connection strings
- [ ] Add new connection string:
  - Name: `DefaultConnection`
  - Value: (Use your Azure SQL connection string)
  - Type: `SQLAzure`
- [ ] Save

### 3. Download Publish Profile (1 min)
- [ ] App Service ? Get publish profile
- [ ] Save file
- [ ] Open file and copy ALL contents

### 4. Add GitHub Secret (2 min)
- [ ] Go to https://github.com/levikirkland/MyTitlePawnCompany
- [ ] Settings ? Secrets and variables ? Actions
- [ ] New repository secret
  - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
  - Value: [Paste publish profile contents]
- [ ] Add secret

### 5. Update Workflow (1 min)
- [ ] Open `.github/workflows/azure-deploy.yml`
- [ ] Change line 11:
  ```yaml
  AZURE_WEBAPP_NAME: mytitlepawn-app    # ? YOUR App Service name
  ```
- [ ] Save

### 6. Push to GitHub (1 min)
```bash
git add .
git commit -m "Add Azure deployment"
git push origin main
```

### 7. Watch Deployment (5 min)
- [ ] GitHub ? Actions tab
- [ ] Click running workflow
- [ ] Wait for green checkmark ?

### 8. Access Application
- [ ] Visit: `https://YOUR-APP-NAME.azurewebsites.net`
- [ ] Register first admin user
- [ ] Create company
- [ ] Create store
- [ ] Test loan creation

## ?? Don't Forget
- [ ] Add MonthlyIncome column to database (if not done)
- [ ] Enable "Allow Azure services" in SQL firewall
- [ ] Add your IP to SQL firewall for testing

## ?? Done!
Your app is now live and auto-deploys on every push to `main`!

**Total Time: ~15-20 minutes** ??

---

## Quick Links
- Azure Portal: https://portal.azure.com
- GitHub Repo: https://github.com/levikirkland/MyTitlePawnCompany
- Full Guide: `AZURE_DEPLOYMENT_GUIDE.md`
