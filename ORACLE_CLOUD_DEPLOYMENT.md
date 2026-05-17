# Oracle Cloud Always Free Deployment Guide

## Step 1: Create Oracle Cloud Account

1. Go to: https://www.oracle.com/cloud/free/
2. Click **Sign Up**
3. Create account with your email
4. Complete identity verification
5. Get **$300 free credits + Always Free services**

## Step 2: Set Up Always Free Infrastructure

### A. Create a Compute Instance (VM)

1. Login to Oracle Cloud Console
2. Navigate to **Compute > Instances**
3. Click **Create Instance**
4. Configure:
   - **Name:** `kirana-store-app`
   - **Image:** Ubuntu 22.04 (free tier eligible)
   - **Shape:** Ampere (ARM-based) - **FREE**
   - **Network:** Create VCN or use default
   - **SSH Key:** Download and save your private key
5. Click **Create**
6. Wait for instance to start (note the **Public IP Address**)

### B. Create PostgreSQL Database (Always Free)

1. Navigate to **Databases > MySQL HeatWave**
   - OR use **OCI Database Service** 
   - OR simpler: Deploy PostgreSQL in Docker on the same VM
2. If using Oracle Database Free Tier:
   - Click **Create MySQL Database**
   - Choose Always Free eligible option
   - Database name: `kirana_store`
   - Username: `admin`
   - Set strong password

**Alternative (Easier):** Install PostgreSQL directly on the VM (see Step 4)

---

## Step 3: Connect to Your VM via SSH

```bash
# On your local machine
ssh -i your-private-key.key ubuntu@YOUR_PUBLIC_IP

# Example:
ssh -i oracle-key.key ubuntu@132.145.123.45
```

---

## Step 4: Install Docker on the VM

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
sudo apt install -y docker.io

# Add ubuntu user to docker group
sudo usermod -aG docker ubuntu
newgrp docker

# Test Docker
docker --version
```

---

## Step 5: Install PostgreSQL (on the VM)

```bash
# Option A: Using Docker (recommended)
docker run -d \
  --name postgres \
  -e POSTGRES_USER=kirana \
  -e POSTGRES_PASSWORD=YourStrongPassword123 \
  -e POSTGRES_DB=kirana_store \
  -p 5432:5432 \
  -v postgres_data:/var/lib/postgresql/data \
  postgres:15

# Option B: Direct installation
sudo apt install -y postgresql postgresql-contrib
sudo systemctl start postgresql
sudo -u postgres psql
```

---

## Step 6: Convert Project to PostgreSQL

### A. Update DAL/DAL.csproj

Replace SQL Server provider with PostgreSQL:

```xml
<!-- Remove this -->
<!-- <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" /> -->

<!-- Add this -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
```

### B. Update DAL/Data/AppDbContext.cs

```csharp
// Find this line:
// optionsBuilder.UseSqlServer(...)

// Replace with:
optionsBuilder.UseNpgsql(connectionString);
```

### C. Update KiranaStore/appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=kirana_store;User Id=kirana;Password=YourStrongPassword123;"
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

### D. Update KiranaStoreUI/Program.cs

Already updated to use config-based API URL:
```csharp
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:6000/api/";
```

---

## Step 7: Create Dockerfile

Create `Dockerfile` in project root:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and projects
COPY ["KiranaStore/KiranaStore.csproj", "KiranaStore/"]
COPY ["BLL/BLL.csproj", "BLL/"]
COPY ["DAL/DAL.csproj", "DAL/"]

# Restore dependencies
RUN dotnet restore "KiranaStore/KiranaStore.csproj"

# Copy all source code
COPY . .

# Build
RUN dotnet build "KiranaStore/KiranaStore.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "KiranaStore/KiranaStore.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "KiranaStore.dll"]
```

Create `Dockerfile.UI` for frontend:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["KiranaStoreUI/KiranaStoreUI.csproj", "KiranaStoreUI/"]
COPY ["BLL/BLL.csproj", "BLL/"]
COPY ["DAL/DAL.csproj", "DAL/"]

RUN dotnet restore "KiranaStoreUI/KiranaStoreUI.csproj"
COPY . .
RUN dotnet build "KiranaStoreUI/KiranaStoreUI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KiranaStoreUI/KiranaStoreUI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "KiranaStoreUI.dll"]
```

---

## Step 8: Push Project to GitHub

```bash
# Initialize git (if not already)
git init
git add .
git commit -m "Add Docker deployment files"
git remote add origin https://github.com/YOUR-USERNAME/kirana-store.git
git push -u origin main
```

---

## Step 9: Deploy on Oracle VM

### A. Clone Repository on VM

```bash
ssh -i oracle-key.key ubuntu@YOUR_PUBLIC_IP

# On VM:
cd ~
git clone https://github.com/YOUR-USERNAME/kirana-store.git
cd kirana-store
```

### B. Build Docker Images

```bash
# Build API image
docker build -f Dockerfile -t kirana-api:latest .

# Build UI image
docker build -f Dockerfile.UI -t kirana-ui:latest .
```

### C. Run Containers

```bash
# Start PostgreSQL (if using Docker)
docker run -d \
  --name postgres \
  --network kirana \
  -e POSTGRES_USER=kirana \
  -e POSTGRES_PASSWORD=YourStrongPassword123 \
  -e POSTGRES_DB=kirana_store \
  -v postgres_data:/var/lib/postgresql/data \
  postgres:15

# Wait for PostgreSQL to start
sleep 10

# Run API container
docker run -d \
  --name api \
  --network kirana \
  -e ConnectionStrings__DefaultConnection="Server=postgres;Port=5432;Database=kirana_store;User Id=kirana;Password=YourStrongPassword123;" \
  -e Jwt__Key="ThisIsMySuperSecretKeyForJwtAuthentication1234567890" \
  -e Jwt__Issuer="WebApi" \
  -e Jwt__Audience="WebApiUsers" \
  -p 6000:8080 \
  kirana-api:latest

# Run UI container
docker run -d \
  --name ui \
  --network kirana \
  -e ApiBaseUrl="http://api:8080/api/" \
  -p 7000:8080 \
  kirana-ui:latest
```

### D. Enable Firewall Access

```bash
# SSH into VM and configure firewall
sudo firewall-cmd --permanent --add-port=6000/tcp
sudo firewall-cmd --permanent --add-port=7000/tcp
sudo firewall-cmd --reload
```

---

## Step 10: Access Your App

1. Get your VM's Public IP from Oracle Cloud Console
2. Open browser:
   - **API/Swagger:** `http://YOUR_PUBLIC_IP:6000/swagger/index.html`
   - **Frontend UI:** `http://YOUR_PUBLIC_IP:7000`

---

## Step 11: Set Up Domain (Optional)

1. Buy domain from: GoDaddy, Namecheap, Google Domains
2. Create DNS A record pointing to your VM's Public IP
3. Access via: `http://yourdomain.com:6000` and `http://yourdomain.com:7000`

Or use free domain from:
- Freenom (free .tk domains)
- Cloudflare + redirect

---

## Step 12: Database Migrations (Important)

Run EF Core migrations to create tables:

```bash
# Option A: Before Docker (local)
dotnet ef database update --project DAL

# Option B: In Docker container
docker exec api dotnet ef database update --project DAL

# Option C: Manual connection
psql -h localhost -U kirana -d kirana_store -c "CREATE TABLE..."
```

---

## Monitoring & Maintenance

### Check running containers
```bash
docker ps
docker logs api
docker logs ui
docker logs postgres
```

### Restart services
```bash
docker restart api ui postgres
```

### Stop all
```bash
docker stop api ui postgres
```

### Update code
```bash
cd ~/kirana-store
git pull
docker build -f Dockerfile -t kirana-api:latest .
docker restart api
```

---

## Cost Summary (Always Free Tier)

- **Compute (2x Ampere VMs):** FREE ✅
- **PostgreSQL Database:** FREE (or ~$10/month if using managed DB) ✅
- **Network/Storage:** FREE (up to limits) ✅
- **Total:** $0/month (within free tier)

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Container won't start | `docker logs container-name` |
| Database connection fails | Check connection string in env vars |
| Port already in use | `docker port api` or change port mapping |
| DNS not resolving | Wait 24-48 hours or check Cloudflare |
| High memory usage | Increase VM specs or optimize code |

---

## Next Steps

1. ✅ Create Oracle Cloud account
2. ✅ Set up VM + PostgreSQL
3. ✅ Update project for PostgreSQL
4. ✅ Create Dockerfiles
5. ✅ Push to GitHub
6. ✅ Deploy containers
7. ✅ Test endpoints
8. ✅ Set up domain
9. ✅ Monitor logs

Questions? Each step has multiple variations based on your preference!
