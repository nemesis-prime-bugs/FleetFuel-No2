# FleetFuel – MVP Project Specification

## 1. Objective

Build a mobile-first web application that allows users to:

- Manage vehicles
- Log trips (Fahrtenbuch-style)
- Upload fuel receipt photos
- Associate receipts with vehicles
- Generate yearly tax-ready CSV summaries

The MVP must:

- Run at 0€ infrastructure cost (free tiers only)
- Be accessible from anywhere (hosted WebApp)
- Support mobile browser usage
- Use simple, maintainable architecture
- Be scalable later without major refactoring

---

## 2. Tech Stack

### Backend
- ASP.NET Core Web API (.NET 8)
- Repository + Unit of Work Pattern
- Service Layer
- DTO Mapping
- SQLite (MVP)
- JWT Authentication

### Frontend
- Next.js (App Router)
- TypeScript (strict mode)
- Tailwind CSS
- shadcn/ui
- ESLint

### Hosting (0€)
- Frontend: Vercel (free tier)
- Backend: Render / Fly.io (free tier)
- Database: SQLite (file-based)

---

## 3. Core MVP Features

### 3.1 Authentication

- Register (email + password)
- Login (JWT-based)
- Protected API routes
- Logout

Acceptance Criteria:
- User can register and log in
- JWT required for all protected endpoints
- Authentication persists after refresh

---

### 3.2 Vehicle Management

User can:

- Create vehicle
- Edit vehicle
- Delete vehicle
- View list of vehicles

Vehicle Fields:
- Id (Guid)
- UserId (Guid)
- Name
- LicensePlate
- InitialMileage
- CreatedAt
- ModifiedAt
- IsDeleted (soft delete)

Acceptance Criteria:
- User can manage multiple vehicles
- Data persists after refresh
- Vehicles are user-scoped

---

### 3.3 Trip Logging (Fahrtenbuch)

User can:

- Create trip
- Edit trip
- Delete trip
- View trips per vehicle

Trip Fields:
- Id (Guid)
- UserId (Guid)
- VehicleId (Guid)
- Date
- StartKm
- EndKm
- CalculatedKm (EndKm - StartKm)
- Purpose
- IsBusiness (bool)
- CreatedAt
- ModifiedAt
- IsDeleted

Validation Rules:
- EndKm must be greater than StartKm
- Vehicle must belong to user

Acceptance Criteria:
- Trips correctly calculate kilometers
- Trips are filtered per vehicle
- Business/private flag is stored

---

### 3.4 Receipt Upload

User can:

- Upload receipt image
- Associate receipt with vehicle
- Enter manual metadata

Receipt Fields:
- Id (Guid)
- UserId (Guid)
- VehicleId (Guid)
- Date
- Amount
- FuelLiters (optional)
- FuelType (optional)
- StationName (optional)
- ImagePath (string)
- CreatedAt
- ModifiedAt
- IsDeleted

Image Storage (MVP):
- Stored in backend `/uploads` folder
- Max file size: 5MB
- Allowed types: jpg, jpeg, png
- Unique file name generation

Acceptance Criteria:
- Receipt image uploads successfully
- Image loads in detail view
- Receipt linked to correct vehicle
- Metadata persists

---

### 3.5 Yearly Summary (Steuerberater Export)

User can:

- Select year
- View summary per vehicle
- Export CSV

Summary Data:
- Total km
- Total business km
- Total fuel cost
- Total receipts count

CSV Export:
- One row per vehicle
- Year included
- Totals included

Acceptance Criteria:
- CSV matches UI totals
- Export works on mobile and desktop
- Only user-owned data included

---

## 4. Data Model Overview

### User
- Id
- Email
- PasswordHash
- CreatedAt

### Vehicle
- Id
- UserId
- Name
- LicensePlate
- InitialMileage
- CreatedAt
- ModifiedAt
- IsDeleted

### Trip
- Id
- UserId
- VehicleId
- Date
- StartKm
- EndKm
- Purpose
- IsBusiness
- CreatedAt
- ModifiedAt
- IsDeleted

### Receipt
- Id
- UserId
- VehicleId
- Date
- Amount
- FuelLiters
- FuelType
- StationName
- ImagePath
- CreatedAt
- ModifiedAt
- IsDeleted

Relationships:
- User → Vehicles (1:N)
- Vehicle → Trips (1:N)
- Vehicle → Receipts (1:N)

---

## 5. API Endpoints (MVP)

### Auth
- POST /api/auth/register
- POST /api/auth/login

### Vehicles
- GET /api/vehicles
- POST /api/vehicles
- PUT /api/vehicles/{id}
- DELETE /api/vehicles/{id}

### Trips
- GET /api/trips?vehicleId=
- POST /api/trips
- PUT /api/trips/{id}
- DELETE /api/trips/{id}

### Receipts
- GET /api/receipts?vehicleId=
- POST /api/receipts
- DELETE /api/receipts/{id}

### Summary
- GET /api/summary?year=
- GET /api/summary/export?year=

---

## 6. Security Requirements

- JWT authentication
- Password hashing (bcrypt or equivalent)
- Max file upload size enforced
- File type validation
- User-scoped queries (no cross-user access)
- Soft delete (no hard delete in MVP)

---

## 7. Non-Functional Requirements

- Mobile-first UI
- Responsive layout
- Clean, minimal UI
- Cold-start tolerant (free tier backend)
- Simple deployment process
- Environment-based configuration

---

## 8. Out of Scope (Post-MVP)

- OCR receipt parsing
- Google Drive integration
- Multi-user company accounts
- PDF export
- App Store / Play Store release
- Real-time sync
- Accountant portal

---

## 9. MVP Definition of Done

The MVP is complete when:

- User can register and log in
- User can create at least one vehicle
- User can log trips for that vehicle
- User can upload receipts linked to vehicle
- User can generate yearly CSV summary
- App is deployed and accessible publicly
- Entire system runs on 0€ infrastructure

---

End of MVP Specification.