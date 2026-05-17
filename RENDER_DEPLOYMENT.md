# Docker Deployment Guide for Render

This guide explains how to deploy Kirana Store to Render using Docker containers.

## Prerequisites

1. **Render Account** - https://render.com
2. **Neo Postgres Database** - Connection details ready
3. **GitHub Repository** - Code pushed to GitHub
4. **Generated JWT_KEY** - Minimum 32 characters

## Render Deployment Steps

### Step 1: Prepare Your Repository

Ensure your repository contains:
- `Dockerfile` (backend)
- `Dockerfile.UI` (frontend)
- `docker-compose.yml` (local development reference)
- All source code in proper structure

### Step 2: Deploy Backend API

#### Create New Web Service on Render

1. Go to https://dashboard.render.com
2. Click "New +" → "Web Service"
3. Connect your GitHub repository
4. Fill in the following:

```
Name: kirana-store-api
Environment: Docker
Build Command: (leave as default)
Start Command: (leave as default)
```

#### Add Environment Variables

In the "Environment" section, add:

```
ConnectionStrings__DefaultConnection=Host=<your-neon-host>;Database=<db-name>;Username=<user>;Password=<pass>;Port=5432;SSL Mode=Require;
JWT_KEY=<your-generated-jwt-key>
ASPNETCORE_ENVIRONMENT=Production
```

**Where to get these values:**
- `<your-neon-host>`: From Neo Postgres dashboard (e.g., `ep-crimson-shadow-apiv7l9c.c-7.us-east-1.aws.neon.tech`)
- `<db-name>`: Your database name (e.g., `neondb`)
- `<user>`: Your database user
- `<pass>`: Your database password
- `<your-generated-jwt-key>`: Generate with: `openssl rand -base64 32`

5. Click "Create Web Service"
6. Wait for deployment to complete (usually 5-10 minutes)
7. Note the deployment URL (e.g., `https://kirana-store-api.onrender.com`)

#### Verify Backend Deployment

```bash
curl https://kirana-store-api.onrender.com/
# Should return: "Kirana Store Backend Running Successfully"
```

### Step 3: Deploy Frontend UI

#### Create New Web Service on Render

1. Go to https://dashboard.render.com
2. Click "New +" → "Web Service"
3. Connect your GitHub repository (same repo)
4. Fill in:

```
Name: kirana-store-ui
Environment: Docker
Dockerfile: Dockerfile.UI
Build Command: (leave as default)
Start Command: (leave as default)
```

#### Add Environment Variables

```
API_BASE_URL=https://kirana-store-api.onrender.com/api/
ASPNETCORE_ENVIRONMENT=Production
```

Replace `kirana-store-api` with your actual backend service name.

5. Click "Create Web Service"
6. Wait for deployment to complete

#### Verify Frontend Deployment

```bash
curl https://kirana-store-ui.onrender.com/
# Should redirect to /User/Login
```

## Environment Variables Reference

### Backend Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | Neo Postgres connection string | `Host=ep-xxx.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=xxx;Port=5432;SSL Mode=Require;` |
| `JWT_KEY` | JWT signing key (min 32 chars) | `your-secure-key-here-min-32-chars` |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |

### Frontend Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `API_BASE_URL` | Backend API URL | `https://kirana-store-api.onrender.com/api/` |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |

## Troubleshooting

### Backend Won't Start

**Check the logs on Render:**
1. Go to service dashboard
2. Click "Logs"
3. Look for error messages

**Common issues:**
- Missing or invalid `ConnectionStrings__DefaultConnection` - verify Neo Postgres details
- Missing `JWT_KEY` - ensure it's set and at least 32 characters
- Database unreachable - check Neo Postgres is running and IP whitelist allows Render

### Frontend Can't Connect to Backend

**Check the logs:**
1. Go to frontend service logs
2. Look for connection errors

**Fix:**
1. Verify `API_BASE_URL` is set correctly
2. Ensure it has trailing `/api/`
3. Test backend health: `curl <backend-url>/`
4. Check CORS is enabled (it is by default)

### Database Migration Fails

**Check backend logs for:**
```
"Database migration error:"
```

**Solutions:**
1. Verify connection string is correct
2. Check Neo Postgres user has proper permissions
3. Ensure database exists and is accessible
4. Check SSL Mode=Require matches Neo Postgres settings

## Render Service Configuration Files

The following files are used by Render for deployment:

### Dockerfile (Backend)
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Builds the backend for production
```

### Dockerfile.UI (Frontend)
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Builds the frontend for production
```

Both files are configured to:
1. Build in Release mode
2. Publish output to `/app/publish`
3. Expose port 8080
4. Run the .NET application

## Monitoring & Maintenance

### View Logs
- Backend: Render Dashboard → kirana-store-api → Logs
- Frontend: Render Dashboard → kirana-store-ui → Logs

### Update Application

To deploy new changes:
1. Push changes to GitHub
2. Render automatically redeploys when push is detected (if auto-deploy enabled)
3. Monitor the "Events" tab for deployment status

### Environment Variables Updates

To update environment variables:
1. Go to service settings
2. Click "Environment"
3. Edit variables
4. Click "Save"
5. Service automatically restarts with new variables

## Performance Optimization

### Database Connections
The application includes connection pooling configured for production. Monitor:
- Response times in logs
- Database connection count in Neo Postgres dashboard

### Frontend Caching
Static assets are cached by CDN. Clear cache if needed in Render settings.

## Security Checklist

- [x] Credentials stored in environment variables (not in code)
- [x] JWT_KEY is unique and strong
- [x] SSL/TLS enabled (handled by Render)
- [x] CORS configured appropriately
- [x] SQL Connection uses SSL Mode=Require
- [ ] Consider: Enable health checks on Render
- [ ] Consider: Set up error notifications
- [ ] Consider: Enable HTTPS enforcement

## Cost Considerations

On Render's free tier:
- Services spin down after 15 min inactivity
- Database connections limited
- Consider: Upgrade to paid plan for production

## Useful Render Commands

### View Deployment Logs
```bash
# Through Render UI: Service → Logs
```

### Restart Service
```bash
# Through Render UI: Service → Settings → Restart Latest Deploy
```

### View Environment Variables
```bash
# Through Render UI: Service → Settings → Environment
```

## Support

For issues specific to Render, see: https://docs.render.com/

For application issues, check:
- Application logs on Render dashboard
- GitHub repository for latest code
- Database connection on Neo Postgres dashboard

---

**Ready to Deploy!** Follow the steps above to get your application running on Render.
