# .gitignore Configuration

## ? Comprehensive .gitignore Created

Your `.gitignore` file has been updated with a comprehensive configuration for .NET 10 projects.

---

## ?? What's Being Ignored (Security & Build Artifacts)

### Critical Security Files (Never Committed)
- ? **User Secrets** - `secrets.json`, `UserSecrets/`
- ? **Connection Strings** - `appsettings.*.json` (except Development)
- ? **Environment Files** - `.env`, `.env.local`
- ? **Publish Profiles** - `*.pubxml`, `*.publishproj`
- ? **Certificates** - `*.pfx`
- ? **API Keys** - `local.settings.json`

### Build Outputs (Auto-Generated)
- ? `bin/`, `obj/` folders
- ? `Debug/`, `Release/` folders
- ? `*.dll`, `*.exe`, `*.pdb` files
- ? NuGet packages (`*.nupkg`)
- ? Compiled artifacts

### Visual Studio Files
- ? `.vs/` folder (cache)
- ? `*.user`, `*.suo` files
- ? User-specific settings
- ? Local history
- ? Test results

### IDE-Specific Files
- ? **VS Code**: `.vscode/` (with exceptions)
- ? **JetBrains Rider**: `.idea/`, `*.iml`
- ? **ReSharper**: `_ReSharper*/`

### Database Files (Local Development)
- ? `*.db`, `*.sqlite`, `*.sqlite3`
- ? `*.mdf`, `*.ldf` (SQL Server)
- ? `healthchecksdb`

### OS-Specific Files
- ? **Windows**: `Thumbs.db`, `Desktop.ini`
- ? **macOS**: `.DS_Store`, `._*`
- ? **Linux**: `*~`, `.swp`

### Node/NPM (If Using)
- ? `node_modules/`
- ? `package-lock.json`
- ? `yarn.lock`

---

## ? What's Being Tracked (Intentionally Kept)

### Source Code
- ? All `.cs`, `.cshtml`, `.css`, `.js` files
- ? Project files (`.csproj`, `.sln`)
- ? Configuration files (`appsettings.json`, `appsettings.Development.json`)

### Documentation
- ? All Markdown files (`*.md`)
- ? `README.md`, `CHANGELOG.md`
- ? Your guide files (deployment, setup, etc.)

### Deployment Files
- ? `.deployment` - Azure deployment config
- ? `deploy.cmd` - Kudu deployment script
- ? `.github/workflows/*.yml` - GitHub Actions workflows

### Migrations
- ? `Migrations/` folder and all migration files
- ? `ApplicationDbContextModelSnapshot.cs`

### Static Assets
- ? `wwwroot/` contents (except minified/generated files)
- ? Images, fonts, static CSS/JS

### Tests
- ? Test project files
- ? Test code (`.cs` files)

---

## ?? Special Sections Explained

### User Secrets Section
```gitignore
# User Secrets - SECURITY CRITICAL
**/appsettings.*.json      # Ignore all environment configs
!**/appsettings.json       # EXCEPT base appsettings.json
!**/appsettings.Development.json  # EXCEPT Development
secrets.json               # Ignore user secrets file
UserSecrets/               # Ignore secrets directory
```

**Why?** Prevents accidental commit of:
- Database connection strings
- API keys
- Passwords
- Production configurations

### Deployment Files Section
```gitignore
# Deployment Files - KEEP THESE
!.deployment               # Keep deployment config
!deploy.cmd                # Keep deployment script
!.github/workflows/*.yml   # Keep GitHub Actions
```

**Why?** These files are needed for:
- Automated deployment to Azure
- CI/CD pipeline functionality
- GitHub Actions workflows

---

## ?? Security Note

### Files That Should NEVER Be Committed

Even if not in `.gitignore`, never manually add:

1. **Connection Strings** with real passwords
2. **API Keys** for external services
3. **Certificates** (`.pfx`, `.cer`)
4. **SSH Keys** or credentials
5. **Azure Publish Profiles** with passwords
6. **Production Configuration** files

### If You Accidentally Commit Secrets

1. **Rotate credentials immediately**
2. **Remove from Git history**:
   ```bash
   git filter-branch --force --index-filter \
   "git rm --cached --ignore-unmatch path/to/secret/file" \
   --prune-empty --tag-name-filter cat -- --all
   ```
3. **Force push** (if safe):
   ```bash
   git push origin --force --all
   ```

---

## ? Verification

### Check What's Ignored
```bash
git status --ignored
```

### Check What Would Be Committed
```bash
git status
```

### See Ignored Files
```bash
git check-ignore -v *
```

### Test Specific File
```bash
git check-ignore -v path/to/file
```

---

## ?? Common Scenarios

### Adding a File That's Ignored
If you need to track a file that's ignored:

1. **Update `.gitignore`** to exclude it
2. **Force add**:
   ```bash
   git add -f path/to/file
   ```

### Removing Tracked File That Should Be Ignored
If a file is tracked but should be ignored:

1. **Add to `.gitignore`**
2. **Remove from tracking**:
   ```bash
   git rm --cached path/to/file
   git commit -m "Remove tracked file that should be ignored"
   ```

### Clean Ignored Files
Remove all ignored files from working directory:
```bash
git clean -fdX
```

**Warning**: This deletes files! Make sure you want to do this.

---

## ?? What This Protects Against

### Security Risks
- ? Leaking database credentials
- ? Exposing API keys
- ? Sharing production configs
- ? Publishing certificates

### Repository Bloat
- ? Large build outputs
- ? Compiled binaries
- ? NuGet packages (restored via `dotnet restore`)
- ? Generated files

### Conflicts
- ? User-specific settings causing merge conflicts
- ? IDE-specific files
- ? OS-specific files

### Privacy
- ? Local development database files
- ? User preferences
- ? Temporary files

---

## ?? Before & After

### Before (Minimal .gitignore)
```gitignore
/.vs
```

Only excluded Visual Studio cache folder.

### After (Comprehensive)
- ? 200+ patterns
- ? Security files protected
- ? Build outputs ignored
- ? IDE files excluded
- ? Deployment files preserved
- ? Documentation tracked

---

## ? Summary

Your repository is now properly configured to:

1. **Protect Secrets** - No credentials will be committed
2. **Reduce Size** - No build artifacts or packages
3. **Avoid Conflicts** - No user-specific files
4. **Keep Important Files** - Source code, docs, and deployment files tracked

**Status**: ?? Production-Ready `.gitignore` configured!

---

## ?? Resources

- GitHub .gitignore Templates: https://github.com/github/gitignore
- .NET .gitignore: https://github.com/github/gitignore/blob/main/VisualStudio.gitignore
- Git Documentation: https://git-scm.com/docs/gitignore
