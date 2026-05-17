# Fresh Deployment Guide - Step by Step

## Overview
Complete fresh deployment with new database and fresh Render services.

---

## STEP 1: Prepare Your GitHub Repository
**Time: 2 minutes**

Before doing anything on Render or database, ensure your code is pushed to GitHub.

### 1.1 Open Terminal in Project Root
```bash
cd c:\Users\HP\Downloads\Kirana_Store-main\Kirana_Store-main
```

### 1.2 Push Code to GitHub
```bash
git add .
git commit -m "Production deployment - secure configuration with environment variables"
git push origin main
```

**Wait for**: Code visible on GitHub in browser

---

## STEP 2: Delete Previous Neo Postgres Database
**Time: 5 minutes**

### 2.1 Go to Neo Postgres Dashboard
1. Open https://console.neon.tech/
2. Login to your account
3. Find your project
4. Look for the current database (e.g., `neondb`)

### 2.2 Delete Database
1. Click on your database name
2. Click "Delete" button
3. Confirm deletion
4. **Wait** for deletion to complete (usually 30 seconds)

### 2.3 Create Fresh Database
1. Click "Create New Database"
2. Name: `neondb` (or your preferred name)
3. Click "Create"
4. **Wait** for creation (usually 1-2 minutes)

### 2.4 Get New Connection String
1. Click on the new database
2. Copy the "Connection String" (PostgreSQL format)
3. **Save it** - you'll need this in STEP 4

**Format should look like:**
```
postgresql://neondb_owner:npg_xxxxxxxxxxxx@ep-xxx.c-x.us-east-1.aws.neon.tech/neondb?sslmode=require
```

---

## STEP 3: Generate Required Secrets
**Time: 5 minutes**

You need to generate a strong JWT Key.

### 3.1 Open PowerShell Terminal
```powershell
# Windows PowerShell command to generate JWT_KEY
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

### 3.2 Copy the Output
You should get something like:
```
AbCd1234EfGh5678IjKl9012MnOp3456QrSt7890UvW+XyZ=
```

**Save this value** - this is your `JWT_KEY`

---

## STEP 4: Delete Previous Render Services
**Time: 5 minutes**

### 4.1 Go to Render Dashboard
1. Open https://dashboard.render.com/
2. Login to your account

### 4.2 Delete Backend Service
1. Find service named `kirana-store-api` (or your backend name)
2. Click on it
3. Go to "Settings"
4. Scroll to bottom
5. Click "Delete Service"
6. Confirm deletion
7. **Wait** for deletion (1-2 minutes)

### 4.3 Delete Frontend Service
1. Find service named `kirana-store-ui` (or your frontend name)
2. Click on it
3. Go to "Settings"
4. Scroll to bottom
5. Click "Delete Service"
6. Confirm deletion
7. **Wait** for deletion

---

## STEP 5: Deploy Backend to Render (Fresh)
**Time: 10-15 minutes**

### 5.1 Create New Backend Service
1. Go to https://dashboard.render.com/
2. Click "New +" button
3. Select "Web Service"

### 5.2 Connect GitHub Repository
1. Select "GitHub" as the source
2. Select your repository: `Kirana_Store-main`
3. Click "Connect"

### 5.3 Configure Backend Service

Fill in the form with:

| Field | Value |
|-------|-------|
| Name | `kirana-store-api` |
| Environment | `Docker` |
| Region | `Ohio (US East)` |
| Branch | `main` |
| Dockerfile | Leave blank (uses default) |
| Build Command | Leave blank (uses default) |
| Start Command | Leave blank (uses default) |

### 5.4 Add Environment Variables

**IMPORTANT**: Before clicking "Create Web Service", scroll down to "Environment" section.

Add these variables:

```
Name: ConnectionStrings__DefaultConnection
Value: [PASTE YOUR NEO POSTGRES CONNECTION STRING FROM STEP 2.4]
```

To format the connection string correctly:
- Take the PostgreSQL connection string
- Format it as:
```
Host=ep-xxx.c-x.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=YOUR_PASSWORD;Port=5432;SSL Mode=Require;
```

Second variable:
```
Name: JWT_KEY
Value: [PASTE YOUR JWT KEY FROM STEP 3.2]
```

Third variable:
```
Name: ASPNETCORE_ENVIRONMENT
Value: Production
```

### 5.5 Deploy Backend
1. Click "Create Web Service"
2. **Wait** for deployment (5-10 minutes)
3. You'll see deployment logs
4. **Success indicator**: Logs show `Now listening on: http://0.0.0.0:8080`

### 5.6 Get Backend URL
1. Look at top of dashboard
2. You'll see URL like: `https://kirana-store-api.onrender.com`
3. **Copy this URL** - you'll need it for frontend

### 5.7 Test Backend Health
```bash
curl https://kirana-store-api.onrender.com/
```

Expected response:
```
Kirana Store Backend Running Successfully
```

If you get this → ✅ Backend is working

---

## STEP 6: Deploy Frontend to Render (Fresh)
**Time: 10-15 minutes**

### 6.1 Create New Frontend Service
1. Go to https://dashboard.render.com/
2. Click "New +" button
3. Select "Web Service"

### 6.2 Connect GitHub Repository
1. Select "GitHub" as the source
2. Select your repository: `Kirana_Store-main`
3. Click "Connect"

### 6.3 Configure Frontend Service

Fill in the form with:

| Field | Value |
|-------|-------|
| Name | `kirana-store-ui` |
| Environment | `Docker` |
| Region | `Ohio (US East)` |
| Branch | `main` |
| Dockerfile | `Dockerfile.UI` |
| Build Command | Leave blank (uses default) |
| Start Command | Leave blank (uses default) |

### 6.4 Add Environment Variables

**IMPORTANT**: Scroll down to "Environment" section.

Add these variables:

```
Name: API_BASE_URL
Value: https://kirana-store-api.onrender.com/api/
```

Replace `kirana-store-api` with your actual backend service name if different.

Second variable:
```
Name: ASPNETCORE_ENVIRONMENT
Value: Production
```

### 6.5 Deploy Frontend
1. Click "Create Web Service"
2. **Wait** for deployment (5-10 minutes)
3. You'll see deployment logs
4. **Success indicator**: Logs show `Now listening on: http://0.0.0.0:8080`

### 6.6 Get Frontend URL
1. Look at top of dashboard
2. You'll see URL like: `https://kirana-store-ui.onrender.com`

---

## STEP 7: Verify Everything Works
**Time: 5 minutes**

### 7.1 Test Backend Endpoint
```bash
curl https://kirana-store-api.onrender.com/
```

Expected: `Kirana Store Backend Running Successfully`

### 7.2 Test Frontend Loads
Open in browser:
```
https://kirana-store-ui.onrender.com/
```

Expected: Login page loads (may redirect from `/` to `/User/Login`)

### 7.3 Check Database Initialization
1. Go to backend service on Render
2. Click "Logs"
3. Look for success message about migrations:
```
Applying migration '20260517043912_InitialCreate'
```

If you see this → ✅ Database is properly initialized

### 7.4 Monitor for Errors
In backend logs, you should NOT see:
```
Database migration error: table ... already exists
```

If you see this error → ❌ Something went wrong, see troubleshooting below

---

## STEP 8: Test Login Functionality (Optional)
**Time: 10 minutes**

### 8.1 Access Frontend
Open: https://kirana-store-ui.onrender.com/

You should see login page.

### 8.2 Check Console (Developer Tools)
1. Right-click → "Inspect"
2. Go to "Console" tab
3. Look for any red error messages
4. If API errors show → Check that API_BASE_URL is correct

---

## Troubleshooting

### Backend Deployment Stuck
1. Go to "Logs"
2. Look for error messages
3. Common issues:
   - `ConnectionStrings__DefaultConnection` format wrong → Fix connection string
   - `JWT_KEY` not set → Add JWT_KEY variable
   - Database unreachable → Check Neo Postgres is running

### Frontend Can't Connect to Backend
1. Go to frontend "Logs"
2. Check `API_BASE_URL` is exactly: `https://kirana-store-api.onrender.com/api/`
3. Replace `kirana-store-api` with your actual backend service name

### Database Migration Error
Error message: `table "..." already exists`

This shouldn't happen with fresh database, but if it does:
1. Go back to Neo Postgres
2. Delete the database again
3. Create fresh database
4. Get new connection string
5. Update backend environment variable on Render
6. Redeploy backend

### Still Having Issues?
1. Check all environment variables are set correctly
2. Verify Neo Postgres connection string format
3. Check JWT_KEY is at least 32 characters
4. Ensure code is pushed to GitHub with all fixes

---

## Summary of URLs You'll Have

After successful deployment:
- **Backend API**: `https://kirana-store-api.onrender.com`
- **Frontend**: `https://kirana-store-ui.onrender.com`
- **API Endpoint**: `https://kirana-store-api.onrender.com/api/`
- **Neo Postgres**: Connection via `ep-xxx.c-x.aws.neon.tech`

---

## Checklist

- [ ] Step 1: Code pushed to GitHub
- [ ] Step 2: Fresh Neo Postgres database created
- [ ] Step 3: JWT_KEY generated
- [ ] Step 4: Old Render services deleted
- [ ] Step 5: Backend deployed and health endpoint working
- [ ] Step 6: Frontend deployed and loads login page
- [ ] Step 7: All verifications passed
- [ ] Step 8: (Optional) Login test passed

✅ **When all checked → Deployment is complete and working!**

---

**Estimated Total Time**: 45-60 minutes

Start from STEP 1 and follow each step carefully.
