# Kirana Store Deployment Configuration Guide

## Overview
This guide provides all necessary environment variable configurations for deploying the Kirana Store application on Render with Neo Postgres database.

## Backend (KiranaStore) - Environment Variables

### For Render Deployment

```
# Database Configuration (Neo Postgres)
ConnectionStrings__DefaultConnection=Host=<neon-host>.c-<region>.aws.neon.tech;Database=<database-name>;Username=<username>;Password=<password>;Port=5432;SSL Mode=Require;

# JWT Configuration
JWT_KEY=<your-secure-jwt-key-minimum-32-characters>

# Aspnet Core
ASPNETCORE_ENVIRONMENT=Production
```

### Example Values Structure:
- `<neon-host>`: Your Neo Postgres hostname (e.g., ep-crimson-shadow-apiv7l9c)
- `<region>`: AWS region code (e.g., c-7)
- `<database-name>`: Your database name (e.g., neondb)
- `<username>`: Your database username
- `<password>`: Your database password

## Frontend (KiranaStoreUI) - Environment Variables

### For Render Deployment

```
# API Base URL Configuration
API_BASE_URL=https://<backend-render-url>/api/

# Aspnet Core
ASPNETCORE_ENVIRONMENT=Production
```

### Example Values Structure:
- `<backend-render-url>`: Your Render backend deployment URL (e.g., https://kirana-store-api.onrender.com)

## Database Migration Notes

The application now uses Entity Framework Core migrations for database initialization:

1. **Automatic Migration**: Migrations are automatically applied on application startup
2. **No Manual Steps Required**: The database schema will be created/updated automatically
3. **Error Handling**: If migration fails, the error will be logged but won't prevent app startup

## Deployment Steps

### 1. Backend Deployment on Render

1. Create new Web Service on Render
2. Connect your GitHub repository
3. Set Runtime: `.NET 8`
4. Set Build Command: `dotnet publish -c Release -o out`
5. Set Start Command: `cd out && dotnet KiranaStore.dll`
6. Add Environment Variables (see Backend section above)
7. Deploy

### 2. Frontend Deployment on Render

1. Create new Web Service on Render
2. Connect your GitHub repository
3. Set Runtime: `.NET 8`
4. Set Build Command: `dotnet publish -c Release -o out`
5. Set Start Command: `cd out && dotnet KiranaStoreUI.dll`
6. Add Environment Variables:
   - `API_BASE_URL`: Set to your backend URL (e.g., https://your-backend.onrender.com/api/)
   - `ASPNETCORE_ENVIRONMENT`: Set to `Production`
7. Deploy

### 3. Neo Postgres Setup

1. Create a database on Neo Postgres (Neon.tech)
2. Get connection string
3. Add to backend environment variables
4. Application will automatically create/migrate schema on first run

## Verification Steps

1. **Backend Health Check**
   ```bash
   curl https://<your-backend-url>/
   # Should return: "Kirana Store Backend Running Successfully"
   ```

2. **Frontend Access**
   ```
   Navigate to https://<your-frontend-url>/
   Should redirect to login page
   ```

3. **Database Connection Test**
   - Login page should load without errors
   - Check Render logs for any database migration errors

## Security Best Practices

✅ **Implemented:**
- JWT tokens for API authentication
- Environment variables for sensitive data (no hardcoded credentials)
- CORS enabled for cross-origin requests
- Session management with HTTP-only cookies
- SSL/TLS enforced (handled by Render)

⚠️ **Recommended for Production:**
1. Use strong, unique JWT_KEY (minimum 32 characters)
2. Regularly rotate JWT_KEY
3. Monitor application logs on Render dashboard
4. Set up error alerting
5. Back up Neo Postgres regularly

## Troubleshooting

### Application Won't Start
- Check environment variables are set correctly
- Verify database connection string in logs
- Ensure JWT_KEY is set and valid

### Database Migration Fails
- Check Neo Postgres connection string
- Verify user has proper permissions
- Check Render logs for detailed error message

### Frontend Can't Connect to Backend
- Verify `API_BASE_URL` environment variable is set correctly
- Check backend service is running and accessible
- Verify CORS is enabled in backend

## Docker Compose (Local Development)

For local testing with Docker:

```bash
docker-compose up
```

This will start:
- PostgreSQL container
- Kirana Store API
- Kirana Store UI

Access via:
- Frontend: http://localhost:3000
- Backend: http://localhost:6000
- API Docs: http://localhost:6000/swagger/index.html

## Files Modified for Deployment

1. **KiranaStore/Program.cs**
   - Added JWT_KEY environment variable support
   - Changed from `EnsureCreated()` to `Migrate()` for migrations
   - Added error handling for migrations

2. **KiranaStoreUI/Program.cs**
   - Added API_BASE_URL environment variable support

3. **appsettings.Production.json**
   - Removed hardcoded credentials
   - Uses environment variables instead

## Support

For deployment issues, check:
- Render Dashboard logs
- Neo Postgres connection status
- GitHub Actions deployment logs (if using CI/CD)
