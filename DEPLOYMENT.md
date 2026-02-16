# FleetFuel Deployment Guide

## Overview
Deploy FleetFuel MVP to production using free tier services:
- **Frontend:** Vercel (Next.js)
- **Backend:** Render (ASP.NET Core)
- **Database:** SQLite (file-based, bundled with backend)
- **Auth:** Supabase (already configured)

---

## Pre-Deployment Checklist

### 1. Environment Variables

**Frontend (`.env.local`):**
```
NEXT_PUBLIC_SUPABASE_URL=your-supabase-url
NEXT_PUBLIC_SUPABASE_ANON_KEY=your-supabase-anon-key
NEXT_PUBLIC_API_URL=https://your-backend.onrender.com
```

**Backend (`appsettings.Production.json`):**
```json
{
  "ConnectionStrings": {
    "DatabaseConnectionString": "Data Source=fleetfuel.db"
  },
  "Supabase": {
    "Url": "your-supabase-url",
    "Key": "your-supabase-service-key"
  },
  "Frontend": {
    "Url": "https://your-vercel-app.vercel.app"
  }
}
```

---

## Frontend Deployment (Vercel)

### Option A: Vercel CLI (Recommended)
```bash
cd frontend
npm i -g vercel
vercel login
vercel --prod
```

### Option B: Vercel Dashboard
1. Go to https://vercel.com/dashboard
2. Click "Add New Project"
3. Select your GitHub repo: `nemesis-prime-bugs/FleetFuel-No2`
4. Set root directory: `frontend`
5. Add environment variables:
   - `NEXT_PUBLIC_SUPABASE_URL`
   - `NEXT_PUBLIC_SUPABASE_ANON_KEY`
   - `NEXT_PUBLIC_API_URL`
6. Click "Deploy"

### Vercel Settings
- Framework Preset: Next.js
- Build Command: `npm run build`
- Output Directory: `.next`

---

## Backend Deployment (Render)

### Option A: Render Dashboard
1. Go to https://dashboard.render.com
2. Click "New +" → "Web Service"
3. Connect your GitHub repo
4. Configure:
   - Root Directory: `backend`
   - Build Command: `dotnet publish -c Release -o out`
   - Start Command: `dotnet out/FleetFuel.Api.dll`
   - Environment: `.NET`
   - Instance Type: **Free**

### Option B: render.yaml (Infrastructure as Code)
Create `render.yaml` in repo root:

```yaml
services:
  - type: web
    name: fleetfuel-api
    env: dotnet
    region: frankfurt
    plan: free
    buildCommand: dotnet publish -c Release -o out
    startCommand: dotnet out/FleetFuel.Api.dll
    envVars:
      - key: DATABASE_URL
        fromDatabase:
          name: fleetfuel-db
          property: connectionString
```

### Environment Variables (Render)
```
SUPABASE_URL=your-supabase-url
SUPABASE_KEY=your-supabase-service-key
FRONTEND_URL=https://your-vercel-app.vercel.app
```

**Note:** SQLite file will be created automatically on first run.

---

## Supabase Setup

1. Create project at https://supabase.com
2. Go to Settings → API
3. Copy:
   - URL → `NEXT_PUBLIC_SUPABASE_URL` (frontend)
   - anon key → `NEXT_PUBLIC_SUPABASE_ANON_KEY` (frontend)
   - service_role key → `SUPABASE_KEY` (backend, for JWT validation)

### Database Schema
Run SQL in Supabase SQL Editor:
```sql
-- Tables are handled by EntityFramework migrations
-- For MVP, ensure RLS policies are configured
```

---

## Post-Deployment Checklist

- [ ] Frontend loads without errors
- [ ] Login/Register works
- [ ] API calls succeed (check browser console)
- [ ] Health endpoint: `https://your-api.onrender.com/health`
- [ ] CORS configured for frontend URL

---

## Troubleshooting

### CORS Errors
Ensure `Frontend:Url` in backend appsettings matches your Vercel URL exactly.

### 502 Bad Gateway
- Check backend logs in Render dashboard
- Ensure startup command is correct

### SQLite Issues
- Ensure write permissions on `/mnt`
- Consider using Render's persistent disk for database

---

## Cost Breakdown

| Service | Free Tier | Cost |
|---------|-----------|------|
| Vercel | 100GB bandwidth/month | 0€ |
| Render | 750 hours/month | 0€ |
| Supabase | Free tier | 0€ |
| **Total** | | **0€** |
