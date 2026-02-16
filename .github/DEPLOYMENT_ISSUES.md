# FleetFuel Deployment Issues

This file contains deployment tasks. Import these as GitHub issues.

---

## Issue 1: Configure Environment Variables

**Labels:** deployment, backend, frontend

**Description:**
Configure all required environment variables for production deployment.

**Tasks:**
- [ ] Frontend: `NEXT_PUBLIC_SUPABASE_URL`
- [ ] Frontend: `NEXT_PUBLIC_SUPABASE_ANON_KEY`
- [ ] Frontend: `NEXT_PUBLIC_API_URL`
- [ ] Backend: `SUPABASE_URL`
- [ ] Backend: `SUPABASE_KEY`
- [ ] Backend: `FRONTEND_URL`

**Acceptance Criteria:**
- All environment variables are set in Vercel and Render dashboards
- Application loads without errors

---

## Issue 2: Deploy Frontend to Vercel

**Labels:** deployment, frontend, critical

**Description:**
Deploy Next.js frontend to Vercel with production environment variables.

**Tasks:**
- [ ] Connect GitHub repo to Vercel
- [ ] Set root directory to `frontend`
- [ ] Add environment variables
- [ ] Deploy to production
- [ ] Verify deployment URL works

**Acceptance Criteria:**
- Frontend accessible at `https://fleetfuel.vercel.app`
- Login/register pages load
- No console errors

---

## Issue 3: Deploy Backend to Render

**Labels:** deployment, backend, critical

**Description:**
Deploy ASP.NET Core backend to Render free tier.

**Tasks:**
- [ ] Connect GitHub repo to Render
- [ ] Configure build command: `dotnet publish -c Release -o out`
- [ ] Configure start command: `dotnet out/FleetFuel.Api.dll`
- [ ] Add environment variables
- [ ] Deploy to production
- [ ] Verify health endpoint: `https://fleetfuel-api.onrender.com/health`

**Acceptance Criteria:**
- Backend accessible at production URL
- Health check returns `{"status":"healthy"}`
- API endpoints respond correctly

---

## Issue 4: Configure Supabase for Production

**Labels:** deployment, database, auth

**Description:**
Configure Supabase project for production use.

**Tasks:**
- [ ] Verify JWT validation works with Supabase keys
- [ ] Test user registration flow
- [ ] Test login flow
- [ ] Ensure RLS policies are correct

**Acceptance Criteria:**
- Users can register and login
- JWT tokens are validated correctly
- No authentication errors

---

## Issue 5: Configure CORS for Production

**Labels:** deployment, backend

**Description:**
Ensure CORS is configured for frontend URL.

**Tasks:**
- [ ] Update `appsettings.Production.json` with frontend URL
- [ ] Test API calls from frontend
- [ ] Verify no CORS errors in browser console

**Acceptance Criteria:**
- Frontend can call backend API without CORS errors
- Network tab shows successful responses

---

## Issue 6: Test Full User Flow

**Labels:** testing, deployment

**Description:**
End-to-end testing of all MVP features in production.

**Tasks:**
- [ ] Register new user
- [ ] Create vehicle
- [ ] Log trip
- [ ] Upload receipt (mock)
- [ ] View dashboard
- [ ] Generate yearly summary
- [ ] Export CSV

**Acceptance Criteria:**
- All user flows work without errors
- Data persists correctly
- CSV export downloads successfully

---

## Issue 7: Configure Custom Domain (Optional)

**Labels:** deployment, optional

**Description:**
Configure custom domain for production deployment.

**Tasks:**
- [ ] Point DNS to Vercel (frontend)
- [ ] Point DNS to Render (backend)
- [ ] Update environment variables
- [ ] Verify HTTPS works

**Acceptance Criteria:**
- App accessible at `fleetfuel.yourdomain.com`
- SSL certificate active

---

## Issue 8: Production Monitoring Setup

**Labels:** monitoring, deployment

**Description:**
Set up monitoring and alerts for production deployment.

**Tasks:**
- [ ] Configure error logging (Serilog → external service)
- [ ] Set up uptime monitoring
- [ ] Configure error alerts

**Acceptance Criteria:**
- Errors are logged and visible
- Uptime monitored
- Alerts configured for critical failures

---

*Last Updated: 2026-02-16*
