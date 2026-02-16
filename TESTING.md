# FleetFuel Production Testing Checklist

**Last Updated:** 2026-02-16 22:30

## Deployment Status
| Component | Status | URL |
|-----------|--------|-----|
| Frontend | ✅ Live | https://frontend-iota-lac-88.vercel.app |
| Backend | ✅ Live | https://fleetfuel.onrender.com |
| Database | ✅ Connected | Supabase (PostgreSQL) |
| Auth | ✅ Configured | Supabase Auth |

---

## Automated Tests Passed ✅
- [x] Backend health check: `{"status":"healthy"}`
- [x] API endpoints respond (401 = auth required)
- [x] CORS headers correct for frontend URL
- [x] Frontend accessible (HTTP 200)

---

## Manual User Flow Tests Required

### Step 1: Authentication
- [ ] **Register:** Go to https://frontend-iota-lac-88.vercel.app/register
- [ ] **Enter email** and password
- [ ] **Click Register** - should redirect to login
- [ ] **Login** with credentials
- [ ] **Expected:** Redirect to dashboard

### Step 2: Vehicle Management
- [ ] **Add Vehicle:** Click "Add Vehicle"
- [ ] **Fill form:**
  - Name: "Company Car"
  - License Plate: "AB-123-CD"
  - Initial Mileage: "50000"
- [ ] **Submit** - should redirect to vehicles list
- [ ] **Expected:** Vehicle appears in grid

### Step 3: Trip Logging
- [ ] **Log Trip:** Click "Log Trip"
- [ ] **Fill form:**
  - Select vehicle from dropdown
  - Date: Today
  - Start KM: (auto-filled)
  - End KM: Start KM + 100
  - Purpose: "Client meeting"
  - Check "Business trip"
- [ ] **Submit** - should redirect to trips list
- [ ] **Expected:** Trip appears in table with distance calculated

### Step 4: Receipt Upload
- [ ] **Add Receipt:** Click "Upload Receipt"
- [ ] **Fill form:**
  - Date: Today
  - Amount: "45.50"
  - Select vehicle
- [ ] **Submit** - should redirect to receipts list
- [ ] **Expected:** Receipt appears in table

### Step 5: Dashboard
- [ ] **View Dashboard:** Should show:
  - Total Vehicles count
  - Total Trips count
  - Total Distance (km)
  - Total Expenses (€)
  - Recent trips list
  - Quick action buttons

### Step 6: Summary & Export
- [ ] **View Summary:** Click "View Summary"
- [ ] **Select year:** Current year (2026)
- [ ] **Expected:** KPI cards with yearly totals
- [ ] **Export CSV:** Click export button
- [ ] **Expected:** CSV file downloads

---

## Issues to Report

If any step fails, document:
1. Error message displayed
2. Screenshot if possible
3. Browser console errors (F12 → Console)

---

## API Endpoints Verified
- GET /health → `{"status":"healthy"}`
- GET /api/v1/vehicles → 401 (auth required)
- GET /api/v1/trips → 401 (auth required)
- GET /api/v1/receipts → 401 (auth required)
- GET /api/v1/summary → 401 (auth required)

All endpoints return 401 Unauthorized when no JWT token is provided, which is correct behavior for authenticated routes.
