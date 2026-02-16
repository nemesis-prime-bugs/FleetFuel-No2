# FleetFuel Deployment Issues

This file contains deployment tasks. Import these as GitHub issues.

---

## Issue 1: Configure Environment Variables ✅ DONE

**Labels:** deployment, backend, frontend

**Completed:**
- [x] Frontend: `NEXT_PUBLIC_SUPABASE_URL`
- [x] Frontend: `NEXT_PUBLIC_SUPABASE_ANON_KEY`
- [x] Frontend: `NEXT_PUBLIC_API_URL`
- [x] Backend: `SUPABASE_URL`
- [x] Backend: `SUPABASE_KEY`
- [x] Backend: `FRONTEND_URL`

**Status:** All env vars configured in Vercel and Render ✅

---

## Issue 2: Deploy Frontend to Vercel ✅ DONE

**Labels:** deployment, frontend, critical

**Completed:**
- [x] Connect GitHub repo to Vercel
- [x] Set root directory to `frontend`
- [x] Add environment variables
- [x] Deploy to production
- [x] Verify deployment URL works

**URL:** https://frontend-iota-lac-88.vercel.app ✅

---

## Issue 3: Deploy Backend to Render ✅ DONE

**Labels:** deployment, backend, critical

**Completed:**
- [x] Connect GitHub repo to Render
- [x] Configure Dockerfile for .NET 8
- [x] Add environment variables
- [x] Deploy to production
- [x] Verify health endpoint

**URL:** https://fleetfuel.onrender.com ✅

---

## Issue 4: Configure Supabase for Production 🔄 IN PROGRESS

**Labels:** deployment, database, auth

**Tasks:**
- [x] Verify JWT validation works with Supabase keys
- [ ] Test user registration flow
- [ ] Test login flow
- [ ] Ensure RLS policies are correct

**Acceptance Criteria:**
- Users can register and login
- JWT tokens are validated correctly
- No authentication errors

---

## Issue 5: Configure CORS for Production ✅ DONE

**Labels:** deployment, backend

**Completed:**
- [x] CORS configured in Program.cs to use FRONTEND_URL env var
- [x] Verify frontend URL in CORS policy
- [x] Backend rebuilt and deployed
- [x] Health check verified: `{"status":"healthy"}`

**CORS Configuration:**
```csharp
var frontendUrl = builder.Configuration["FRONTEND_URL"] 
    ?? builder.Configuration["Frontend:Url"] 
    ?? "http://localhost:3000";
policy.WithOrigins(frontendUrl)
```

**Status:** CORS configured for frontend-iota-lac-88.vercel.app ✅

---

## Issue 6: Test Full User Flow ✅ TECHNICAL VERIFICATION DONE

**Labels:** testing, deployment

**Automated Tests Passed:**
- [x] Backend health check: `{"status":"healthy"}`
- [x] API endpoints respond (401 = auth required = ✅)
- [x] CORS configured correctly
- [x] Frontend accessible (HTTP 200)
- [x] Frontend can call backend (CORS headers verified)

**Manual Testing Required (User Action):**
- [ ] Register user via https://frontend-iota-lac-88.vercel.app/register
- [ ] Login with registered user
- [ ] Create vehicle
- [ ] Log trip
- [ ] Upload receipt
- [ ] View dashboard
- [ ] Generate yearly summary
- [ ] Export CSV

**Documentation Created:**
- `TESTING.md` - Full testing checklist with steps and expected results

**Status:** Technical verification complete. Manual user flow testing in progress.

---

## Issue 7: Configure Custom Domain (Optional)

**Labels:** deployment, optional

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

**Tasks:**
- [ ] Configure error logging (Serilog → console/file)
- [ ] Set up uptime monitoring (Render health checks)
- [ ] Configure error alerts

**Acceptance Criteria:**
- Errors are logged and visible
- Uptime monitored
- Alerts configured for critical failures

---

*Last Updated: 2026-02-16 22:15*
