# SQL Server → PostgreSQL Migration Guide

## Step-by-Step Migration

This guide walks through converting your project from SQL Server to PostgreSQL for free Oracle Cloud deployment.

---

## 1. Update NuGet Packages

### DAL/DAL.csproj

Open the file and replace:

```xml
<!-- OLD - Remove -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />

<!-- NEW - Add -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
```

Then run:
```bash
cd DAL
dotnet restore
cd ..
```

---

## 2. Update DbContext Configuration

### DAL/Data/AppDbContext.cs

Find the `OnConfiguring` method and update it:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    // OLD CODE - Remove this
    // if (!optionsBuilder.IsConfigured)
    // {
    //     optionsBuilder.UseSqlServer("Server=localhost;Database=KiranaStore;Trusted_Connection=True;");
    // }

    // NEW CODE - Add this
    if (!optionsBuilder.IsConfigured)
    {
        optionsBuilder.UseNpgsql(GetConnectionString());
    }
}

private string GetConnectionString()
{
    // Read from environment or config
    return System.Environment.GetEnvironmentVariable("ASPNETCORE_CONNECTIONSTRING") 
        ?? "Server=localhost;Port=5432;Database=kirana_store;User Id=kirana;Password=KiranaStorePassword123;";
}
```

---

## 3. Update Connection Strings

### KiranaStore/appsettings.json

Replace the entire `ConnectionStrings` section:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=kirana_store;User Id=kirana;Password=KiranaStorePassword123;"
  },
  "Jwt": {
    "Key": "ThisIsMySuperSecretKeyForJwtAuthentication1234567890",
    "Issuer": "WebApi",
    "Audience": "WebApiUsers",
    "ExpireMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### KiranaStore/appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=kirana_store;User Id=kirana;Password=KiranaStorePassword123;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.AspNetCore.Routing": "Debug"
    }
  }
}
```

---

## 4. PostgreSQL-Specific Changes in Models

### Common Issues & Fixes

#### A. Decimal Precision

**SQL Server:** Uses `DECIMAL(18,2)` by default
**PostgreSQL:** Use explicit configuration

In your model properties:
```csharp
[Column(TypeName = "decimal(18,2)")]
public decimal Price { get; set; }

[Column(TypeName = "decimal(10,2)")]
public decimal Quantity { get; set; }
```

#### B. DateTime vs DateTime2

**SQL Server:** Has `datetime` and `datetime2`
**PostgreSQL:** Uses `timestamp`

In your DbContext's `OnModelCreating`:
```csharp
modelBuilder.Entity<Product>()
    .Property(p => p.CreatedAt)
    .HasColumnType("timestamp without time zone");
```

#### C. GUID/UUID

PostgreSQL uses `uuid` type:
```csharp
modelBuilder.Entity<Product>()
    .Property(p => p.Id)
    .HasColumnType("uuid")
    .HasDefaultValueSql("gen_random_uuid()");
```

#### D. String Length

Add `[MaxLength]` attributes:
```csharp
[MaxLength(100)]
public string ProductName { get; set; }

[MaxLength(255)]
public string Description { get; set; }
```

---

## 5. Update Entity Framework Configuration

### DAL/Data/AppDbContext.cs - OnModelCreating

Add this for better PostgreSQL compatibility:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Configure decimal precision
    foreach (var property in modelBuilder.Model.GetEntityTypes()
        .SelectMany(t => t.GetProperties())
        .Where(p => p.ClrType == typeof(decimal)))
    {
        property.SetPrecision(18);
        property.SetScale(2);
    }

    // Add your existing configurations here
    // modelBuilder.Entity<Product>()
    //     .HasIndex(p => p.ProductName);
}
```

---

## 6. Create/Update Migrations

```bash
# Remove old migrations (if any)
cd DAL
rm -r Migrations
cd ..

# Create new migrations for PostgreSQL
cd DAL
dotnet ef migrations add InitialCreate
cd ..

# Check migration file is created
ls DAL/Migrations/
```

---

## 7. Local Testing with PostgreSQL

### Option A: Using Docker Compose

```bash
# Start PostgreSQL locally
docker-compose up -d postgres

# Wait for it to start
sleep 3

# Run API with local PostgreSQL
dotnet run --project KiranaStore/KiranaStore.csproj
```

### Option B: Manual PostgreSQL Setup

**Windows:**
1. Download: https://www.postgresql.org/download/windows/
2. Install with username `postgres`, password `postgres`
3. Create database:
   ```sql
   CREATE DATABASE kirana_store;
   CREATE USER kirana WITH PASSWORD 'KiranaStorePassword123';
   GRANT ALL PRIVILEGES ON DATABASE kirana_store TO kirana;
   ```

**Linux/Mac:**
```bash
brew install postgresql  # macOS
sudo apt install postgresql  # Linux

# Start service
sudo systemctl start postgresql

# Create user and database
sudo -u postgres psql
CREATE DATABASE kirana_store;
CREATE USER kirana WITH PASSWORD 'KiranaStorePassword123';
GRANT ALL PRIVILEGES ON DATABASE kirana_store TO kirana;
\q
```

---

## 8. Test Locally

```bash
# Build solution
dotnet build

# Run migrations
dotnet ef database update --project DAL

# Run API
dotnet run --project KiranaStore/KiranaStore.csproj

# Test Swagger
# Open browser to http://localhost:6000/swagger
```

---

## 9. PostgreSQL vs SQL Server - Syntax Differences

| Feature | SQL Server | PostgreSQL |
|---------|-----------|-----------|
| Auto-increment | `IDENTITY` | `SERIAL` |
| Boolean | `BIT` | `BOOLEAN` |
| String | `VARCHAR(max)` | `TEXT` |
| DateTime | `DATETIME2` | `TIMESTAMP` |
| GUID | `UNIQUEIDENTIFIER` | `UUID` |
| NULL handling | `ISNULL()` | `COALESCE()` |
| Case sensitivity | Case-insensitive | Case-sensitive |

---

## 10. Known Issues & Solutions

### Issue: Encoding errors
```csharp
// In appsettings.json, ensure UTF-8:
// Connection string includes: "Default Encoding='UTF8';"
```

### Issue: Locale/Collation
PostgreSQL might use different collation. In migrations:
```csharp
modelBuilder.Entity<Product>()
    .Property(p => p.ProductName)
    .HasCollation("en_US.UTF-8");
```

### Issue: Case-sensitive queries
PostgreSQL is case-sensitive by default:
```csharp
// Use .EF.Functions for case-insensitive search
.Where(p => EF.Functions.ILike(p.ProductName, "%search%"))
```

---

## 11. Validation Checklist

Before deploying:

- [ ] Updated `DAL.csproj` with PostgreSQL provider
- [ ] Updated `AppDbContext.cs` with `UseNpgsql`
- [ ] Updated connection strings in `appsettings.json`
- [ ] All models have `[MaxLength]` attributes
- [ ] Decimal properties have `[Column(TypeName = "decimal(18,2)")]`
- [ ] Ran `dotnet ef migrations add InitialCreate`
- [ ] Tested locally with PostgreSQL
- [ ] API runs without errors
- [ ] Database migration applies successfully
- [ ] All unit tests pass

---

## 12. Rollback (if needed)

If you want to go back to SQL Server:

```bash
# Revert migrations
dotnet ef database update 0 --project DAL

# Restore old packages
git checkout DAL/DAL.csproj
git checkout KiranaStore/appsettings.json

# Restore context
git checkout DAL/Data/AppDbContext.cs

# Restore and rebuild
dotnet restore
dotnet build
```

---

## 🎉 Done!

Your project is now PostgreSQL-compatible and ready for Oracle Cloud deployment!

Next step: Follow `ORACLE_QUICK_START.md` to deploy.
