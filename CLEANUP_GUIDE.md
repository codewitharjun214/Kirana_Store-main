# Cleanup Guide - Files to Delete

This document lists unnecessary files and directories that can be safely removed before deployment to reduce package size and improve cleanliness.

## 🗑️ Safe to Delete Immediately

### Build Artifacts (Can be regenerated)
```
BLL/bin/
BLL/obj/
DAL/bin/
DAL/obj/
KiranaStore/bin/
KiranaStore/obj/
KiranaStoreUI/bin/
KiranaStoreUI/obj/
```

### Cache Files
```
**/*.lscache
**/.vs/ (Visual Studio cache)
**/.vscode/ (if using VS Code workspace settings)
```

### NuGet Cache
```
# In your .nuget folder (optional, will be recreated on next restore)
~/.nuget/packages/microsoft.entityframeworkcore.sqlserver/ (Old SQL Server package)
~/.nuget/packages/microsoft.sqlserver.server/
```

## 📝 Optional Cleanup (Development-Related)

### Example/Template Files
```
WeatherForecast.cs (not used in Kirana Store)
```

### Test Files (If Not Using)
```
# If you have xUnit/NUnit test projects that aren't needed
**/*Tests/ directory
```

## ⚠️ DO NOT DELETE

```
✓ Source code files (.cs)
✓ Project files (.csproj, .sln)
✓ Configuration files (appsettings.json, appsettings.*.json)
✓ Migrations folder (DAL/Migrations/)
✓ All models, controllers, services
✓ Database connection strings
✓ Docker files (Dockerfile, docker-compose.yml)
✓ Documentation files (.md)
```

## 🔄 Cleanup Script

### PowerShell - Remove Build Artifacts Only
```powershell
# Navigate to solution root
cd "c:\Users\HP\Downloads\Kirana_Store-main\Kirana_Store-main"

# Remove all bin and obj folders
Get-ChildItem -Include bin, obj -Recurse -Force | Remove-Item -Recurse -Force

# Remove cache files
Get-ChildItem -Include *.lscache -Recurse -Force | Remove-Item -Force
```

### PowerShell - Full Cleanup (Keep only source)
```powershell
cd "c:\Users\HP\Downloads\Kirana_Store-main\Kirana_Store-main"

# Clean build artifacts
Get-ChildItem -Include bin, obj -Recurse -Force | Remove-Item -Recurse -Force
Get-ChildItem -Include *.lscache -Recurse -Force | Remove-Item -Force

# Clean unused files
Remove-Item -Path "KiranaStore/WeatherForecast.cs" -Force

# Optional: Clean VS Code workspace cache
Remove-Item -Path ".vscode/" -Recurse -Force
```

## 📊 Size Impact

| Item | Size | Cleanable |
|------|------|-----------|
| bin/ directories | ~200-300 MB | ✅ Yes |
| obj/ directories | ~100-150 MB | ✅ Yes |
| .lscache files | ~10-20 MB | ✅ Yes |
| Source code | ~2-3 MB | ❌ No |

**Total cleanup potential: 300-500 MB**

## ✅ Pre-Deployment Checklist

Before running cleanup:
- [ ] Commit all changes to Git
- [ ] Verify build succeeds locally
- [ ] Database migrations applied
- [ ] appsettings.Production.json configured
- [ ] JWT keys set correctly
- [ ] PostgreSQL connection string verified

After cleanup:
- [ ] Run `dotnet restore` (should redownload packages)
- [ ] Run `dotnet build` (should rebuild successfully)
- [ ] Verify API starts on `http://localhost:5013`
- [ ] Test at least one endpoint
