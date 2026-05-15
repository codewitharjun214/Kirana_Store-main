# Quick Start: Oracle Cloud Deployment

## 📋 Prerequisites

Before you start, you need:
1. Oracle Cloud account (free tier)
2. GitHub account
3. SSH key pair
4. Local machine with `dotnet`, `git`, and `docker` installed

---

## 🚀 Quick Deployment Steps (5 mins)

### 1. Create Oracle Cloud Account
- Visit: https://www.oracle.com/cloud/free/
- Sign up with email
- Complete identity verification
- You get $300 credits + Always Free services

### 2. Create VM in Oracle Cloud

1. Login to Oracle Cloud Console
2. **Compute** → **Instances** → **Create Instance**
3. Fill:
   - **Name:** `kirana-store`
   - **Image:** Ubuntu 22.04 (free)
   - **Shape:** Ampere (FREE - ARM-based)
   - Download SSH key and save locally
4. Click **Create** (wait ~2 mins)
5. Note down the **Public IP** (e.g., 132.145.123.45)

### 3. Prepare Your Code

#### A. Update DAL for PostgreSQL

Edit `DAL/DAL.csproj`:
```xml
<!-- Remove: -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />

<!-- Add: -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
```

Edit `DAL/Data/AppDbContext.cs`:
```csharp
// Find: optionsBuilder.UseSqlServer(...)
// Replace with:
optionsBuilder.UseNpgsql(connectionString);
```

#### B. Update Connection String

Edit `KiranaStore/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=kirana_store;User Id=kirana;Password=KiranaStorePassword123;"
  }
}
```

#### C. Push to GitHub

```bash
cd your-project-folder
git add .
git commit -m "Add Oracle Cloud deployment"
git push origin main
```

### 4. Connect to VM and Deploy

```bash
# SSH into your VM
ssh -i your-key.pem ubuntu@YOUR_PUBLIC_IP

# Clone deployment script (you can paste these commands)
git clone https://github.com/YOUR-USERNAME/kirana-store.git
cd kirana-store

# Run deployment
bash deploy-oracle.sh
```

**OR manually:**

```bash
# Install Docker
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker ubuntu
newgrp docker

# Clone repo
git clone https://github.com/YOUR-USERNAME/kirana-store.git
cd kirana-store

# Build and run
docker-compose up -d
```

### 5. Access Your App

- **API:** `http://YOUR_PUBLIC_IP:6000/swagger/index.html`
- **UI:** `http://YOUR_PUBLIC_IP:7000`

---

## 🛠️ Troubleshooting

### Container won't start
```bash
docker logs kirana_api
docker logs kirana_ui
```

### Database connection error
Check PostgreSQL is running:
```bash
docker ps | grep postgres
```

### Port already in use
```bash
docker port kirana_api
netstat -tulpn | grep 6000
```

---

## 📊 Cost Breakdown

| Service | Cost |
|---------|------|
| 2x Ampere VMs | FREE |
| PostgreSQL DB | FREE (20GB limit) |
| 1TB monthly traffic | FREE |
| **Total** | **$0/month** |

---

## 🔧 Managing Your Deployment

### View logs
```bash
docker logs -f kirana_api
```

### Restart services
```bash
docker restart kirana_api kirana_ui
```

### Update code
```bash
cd kirana-store
git pull
docker-compose build
docker-compose up -d
```

### Stop everything
```bash
docker-compose down
```

---

## 📱 Next Steps

1. ✅ Set up Oracle Cloud account
2. ✅ Create VM
3. ✅ Update code for PostgreSQL
4. ✅ Push to GitHub
5. ✅ SSH into VM and run deployment
6. ✅ Test endpoints
7. ⭐ (Optional) Set up custom domain

---

## 🆘 Need Help?

Check logs with:
```bash
docker logs kirana_api 2>&1 | tail -50
```

For database issues:
```bash
docker exec kirana_postgres psql -U kirana -d kirana_store -c "\dt"
```

