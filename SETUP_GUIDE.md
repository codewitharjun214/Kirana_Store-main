# Kirana Store - PostgreSQL Setup Guide

## ✅ Completed Steps

### 1. Database Configuration
- ✅ Installed `Npgsql.EntityFrameworkCore.PostgreSQL` (v8.0.0)
- ✅ Configured connection to Neon PostgreSQL
- ✅ Applied migrations - all tables created
- ✅ Removed SQL Server dependencies

### 2. API Configuration (KiranaStore)
- ✅ Backend running on `http://localhost:5013`
- ✅ Swagger UI available at `http://localhost:5013/swagger`
- ✅ JWT authentication configured
- ✅ Production config template created

### 3. UI Configuration (KiranaStoreUI)
- ✅ API base URL set to `http://localhost:5013/api/`
- ✅ Production config template created

## 🚀 Running the Application

### Terminal 1: Start API Backend
```powershell
cd "c:\Users\HP\Downloads\Kirana_Store-main\Kirana_Store-main"
dotnet run --project KiranaStore
```
API runs on: http://localhost:5013

### Terminal 2: Start UI Frontend
```powershell
cd "c:\Users\HP\Downloads\Kirana_Store-main\Kirana_Store-main"
dotnet run --project KiranaStoreUI
```
UI runs on: http://localhost:5000 (or next available port)

## 📋 Next Steps Before Production Deployment

### 1. Initial Data Seeding
Create admin user and demo data:
```powershell
cd "c:\Users\HP\Downloads\Kirana_Store-main\Kirana_Store-main"
dotnet run --project KiranaStore -- --seed
```

### 2. Security Configuration
- Update `appsettings.Production.json` with:
  - Secure JWT key (min 32 characters)
  - Production database credentials
  - API domain name

### 3. Update UI Production Config
- Edit `KiranaStoreUI/appsettings.Production.json`
- Change `ApiBaseUrl` to your production domain

### 4. Environment Variables (Recommended)
Instead of hardcoding secrets in appsettings:
```powershell
# Set environment variables
$env:ConnectionStrings__DefaultConnection = "your-prod-connection-string"
$env:Jwt__Key = "your-secure-jwt-key"
```

### 5. Build for Production
```powershell
dotnet publish -c Release --output ./publish
```

## 📁 Files to Delete (Optional Cleanup)

Remove these SQL Server-specific/temporary files:
```
- .gitignore (if not needed)
- WeatherForecast.cs
- bin/ directories
- obj/ directories
- *.lscache files
- .vs/ directory (if using Visual Studio)
```

## 🔗 Neon PostgreSQL Connection Details

**Connection String Format (ADO.NET):**
```
Host=ep-crimson-shadow-apiv7l9c.c-7.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_ekE0tum2INfL;Port=5432;SSL Mode=Require;
```

**Tables Created:**
- Users, Categories, Products, Suppliers
- Purchases, PurchaseItems
- Customers, Sales, SaleItems
- Orders, OrderItems
- Payments, Stocks, StockLedgers, AuditLogs

## 🐳 Docker Deployment (Optional)

Use the provided `Dockerfile` and `docker-compose.yml`:
```powershell
docker-compose up -d
```

## ✨ API Endpoints (Swagger)
- http://localhost:5013/swagger - Full API documentation
- Authentication endpoints: /api/auth/*
- CRUD operations for all entities

---
**Status:** Ready for production deployment
