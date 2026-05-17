# 📋 Oracle Cloud Deployment - Complete Checklist

## 📚 Documentation Files Created

Your project now includes these deployment guides:

1. **`ORACLE_CLOUD_DEPLOYMENT.md`** - Full step-by-step guide (25 steps)
2. **`ORACLE_QUICK_START.md`** - Quick reference (5 mins to deploy)
3. **`POSTGRESQL_MIGRATION.md`** - Database migration instructions
4. **`Dockerfile`** - API containerization
5. **`Dockerfile.UI`** - Frontend containerization
6. **`docker-compose.yml`** - Local testing (docker-compose)
7. **`deploy-oracle.sh`** - Automated deployment script

---

## ✅ Pre-Deployment Checklist

### Local Preparation (Do These First)

- [ ] Fork/clone project to your GitHub account
- [ ] Read `POSTGRESQL_MIGRATION.md`
- [ ] Update `DAL/DAL.csproj` - replace SQL Server package with Npgsql
- [ ] Update `DAL/Data/AppDbContext.cs` - change to `UseNpgsql()`
- [ ] Update connection strings in `appsettings.json`
- [ ] Run locally with PostgreSQL:
  ```bash
  docker-compose up -d postgres
  dotnet run --project KiranaStore/KiranaStore.csproj
  ```
- [ ] Test API at `http://localhost:6000/swagger`
- [ ] Verify no SQL Server-specific code remains
- [ ] Commit and push to GitHub:
  ```bash
  git add .
  git commit -m "Add PostgreSQL support and Docker deployment"
  git push origin main
  ```

### Oracle Cloud Setup

- [ ] Create Oracle Cloud account at https://www.oracle.com/cloud/free/
- [ ] Complete identity verification
- [ ] Log into Oracle Cloud Console
- [ ] Create Compute Instance:
  - [ ] Name: `kirana-store`
  - [ ] Image: Ubuntu 22.04
  - [ ] Shape: Ampere (free)
  - [ ] **Download and save SSH key**
- [ ] Note the Public IP address
- [ ] Wait for instance to start (2-5 mins)

### Deployment

- [ ] SSH into VM:
  ```bash
  ssh -i your-key.pem ubuntu@YOUR_PUBLIC_IP
  ```
- [ ] Run deployment:
  ```bash
  git clone https://github.com/YOUR-USERNAME/kirana-store.git
  cd kirana-store
  bash deploy-oracle.sh
  ```
  Or use docker-compose:
  ```bash
  docker-compose up -d
  ```
- [ ] Configure firewall for ports 6000 and 7000
- [ ] Test endpoints:
  - [ ] API: `http://YOUR_PUBLIC_IP:6000/swagger/index.html`
  - [ ] UI: `http://YOUR_PUBLIC_IP:7000`

### Post-Deployment

- [ ] Verify all containers running:
  ```bash
  docker ps
  ```
- [ ] Check logs for errors:
  ```bash
  docker logs kirana_api
  docker logs kirana_ui
  ```
- [ ] Test login page works
- [ ] Test API endpoints
- [ ] Create admin user
- [ ] (Optional) Set up custom domain
- [ ] Document deployment details

---

## 🚀 Quick Command Reference

### Deployment (One Command)

```bash
# SSH into VM first
ssh -i your-key.pem ubuntu@YOUR_PUBLIC_IP

# Then run:
git clone https://github.com/YOUR-USERNAME/kirana-store.git
cd kirana-store
bash deploy-oracle.sh
```

### Local Testing

```bash
# Start all services
docker-compose up -d

# Check status
docker ps

# View logs
docker logs kirana_api
docker logs kirana_ui

# Stop all
docker-compose down
```

### VM Management

```bash
# View running containers
docker ps

# Restart a service
docker restart kirana_api

# View API logs (last 50 lines)
docker logs -f kirana_api --tail 50

# Update code and redeploy
cd kirana-store
git pull
docker-compose build
docker-compose up -d

# Stop everything
docker-compose down
```

---

## 💰 Cost Summary

| Item | Cost | Notes |
|------|------|-------|
| 2x Ampere VMs | FREE | ~1-2GB RAM each |
| PostgreSQL DB | FREE | 20GB storage included |
| Network | FREE | 1TB/month data transfer |
| **Total/month** | **$0** | Within Always Free tier |

---

## 🎯 Next Steps

### Immediate (Today)

1. Read `POSTGRESQL_MIGRATION.md` completely
2. Make code changes locally
3. Test with docker-compose
4. Push to GitHub
5. Create Oracle Cloud account

### Soon (This Week)

1. Set up VM in Oracle Cloud
2. SSH into VM
3. Run deployment script
4. Verify endpoints working
5. Test application features

### Optional (Later)

- [ ] Set up custom domain (point to your Public IP)
- [ ] Enable HTTPS with Let's Encrypt
- [ ] Set up monitoring/alerts
- [ ] Create backup strategy
- [ ] Document API endpoints
- [ ] Create deployment runbook

---

## 🆘 Troubleshooting Guide

### Problem: SSH connection denied
```bash
# Check key permissions
chmod 600 your-key.pem

# Try again
ssh -i your-key.pem ubuntu@YOUR_PUBLIC_IP
```

### Problem: Docker not found
```bash
# VM needs Docker installed
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker ubuntu
```

### Problem: Container won't start
```bash
# Check logs
docker logs kirana_api

# Check disk space
df -h

# Check resource usage
docker stats
```

### Problem: Database connection fails
```bash
# Verify PostgreSQL running
docker ps | grep postgres

# Check connection string
docker inspect kirana_api | grep -A 10 "Env"

# Test connection manually
docker exec kirana_postgres psql -U kirana -d kirana_store -c "\dt"
```

### Problem: Port already in use
```bash
# Find what's using port 6000
sudo netstat -tulpn | grep 6000

# Stop conflicting container
docker stop CONTAINER_ID

# Or use different port in docker-compose.yml
```

---

## 📞 Support & Resources

- **Oracle Cloud Free Tier:** https://www.oracle.com/cloud/free/
- **Docker Documentation:** https://docs.docker.com/
- **PostgreSQL Documentation:** https://www.postgresql.org/docs/
- **Entity Framework Core PostgreSQL:** https://www.npgsql.org/efcore/

---

## 📝 Deployment Summary Template

After successful deployment, save this info:

```
=== KIRANA STORE DEPLOYMENT ===
Date Deployed: [DATE]
Oracle Cloud Instance: [INSTANCE_ID]
Public IP: [YOUR_PUBLIC_IP]
Database: PostgreSQL on [DB_HOST]
API URL: http://[YOUR_PUBLIC_IP]:6000
UI URL: http://[YOUR_PUBLIC_IP]:7000
Admin Username: [ADMIN_USER]
Estimated Monthly Cost: $0 (Always Free tier)
Last Updated: [DATE]
Last Backup: [DATE]
```

---

## ✨ You're Ready!

All deployment files are prepared. Follow the guides above and you'll have your app running on Oracle Cloud for **$0/month**.

**Start with:** Read `POSTGRESQL_MIGRATION.md` → Make code changes → Test locally → Deploy to Oracle Cloud

Good luck! 🚀
