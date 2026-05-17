# Quick Reference - Environment Variables

## For Render Deployment

### Backend Service (kirana-store-api)

Copy and paste into Render Environment Variables:

```
ConnectionStrings__DefaultConnection=Host=<NEON_HOST>;Database=<DB_NAME>;Username=<DB_USER>;Password=<DB_PASSWORD>;Port=5432;SSL Mode=Require;
JWT_KEY=<YOUR_GENERATED_JWT_KEY>
ASPNETCORE_ENVIRONMENT=Production
```

**How to fill in:**
- `<NEON_HOST>`: Get from Neo Postgres dashboard (looks like: `ep-crimson-shadow-apiv7l9c.c-7.us-east-1.aws.neon.tech`)
- `<DB_NAME>`: Your database name (e.g., `neondb`)
- `<DB_USER>`: Your database user (e.g., `neondb_owner`)
- `<DB_PASSWORD>`: Your database password
- `<YOUR_GENERATED_JWT_KEY>`: Run `openssl rand -base64 32` to generate

### Frontend Service (kirana-store-ui)

Copy and paste into Render Environment Variables:

```
API_BASE_URL=https://kirana-store-api.onrender.com/api/
ASPNETCORE_ENVIRONMENT=Production
```

**How to fill in:**
- Replace `kirana-store-api` with your actual backend service name on Render
- Must include `/api/` at the end

---

## Generating JWT Key

Open terminal and run:
```bash
# Linux/Mac
openssl rand -base64 32

# Windows PowerShell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

Copy the output and use as `JWT_KEY` value.

---

## Neo Postgres Connection String Format

Standard format (PostgreSQL):
```
Host=<host>;Database=<db>;Username=<user>;Password=<password>;Port=5432;SSL Mode=Require;
```

Example with actual values:
```
Host=ep-xyz.c-7.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=my_password_123;Port=5432;SSL Mode=Require;
```

---

## Testing

After deployment, verify by running:

```bash
# Test backend health
curl https://<your-backend-url>/
# Expected response: "Kirana Store Backend Running Successfully"

# Test frontend (should redirect to login)
curl -L https://<your-frontend-url>/
```

---

**Template Ready!** Copy the environment variable sections above directly into Render.
