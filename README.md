# FleetFuel 🚗⛽

A professional fleet management application for tracking vehicles, trips, and fuel expenses. Built with a mobile-first approach, suitable for individuals and small businesses.

![FleetFuel MVP](https://via.placeholder.com/800x400?text=FleetFuel+MVP)

## Features

### Core Functionality
- **Vehicle Management** - Add, edit, and track multiple vehicles
- **Trip Logging (Fahrtenbuch)** - Log trips with start/end kilometer readings
- **Receipt Tracking** - Upload and associate fuel receipts with vehicles
- **Yearly Summaries** - Generate tax-ready reports for accountants (Steuerberater)
- **CSV Export** - Download complete yearly summaries in CSV format

### Authentication & Security
- **Supabase JWT Authentication** - Secure email/password registration and login
- **User-scoped Data** - All data is private to each user
- **Soft Delete** - Data can be recovered (no hard deletion)

### User Experience
- **Guided Onboarding** - First-time users are guided through setup
- **Smart Defaults** - Date, vehicle, and preferences auto-filled
- **Keyboard Navigation** - Enter key navigation for fast data entry
- **Responsive Design** - Works on mobile, tablet, and desktop

## Tech Stack

### Frontend
- **Next.js 14** - React framework with App Router
- **TypeScript** - Type-safe development
- **Tailwind CSS** - Utility-first styling
- **React Hook Form** - Form handling with Zod validation
- **Supabase Client** - Authentication and API communication

### Backend
- **ASP.NET Core 8** - RESTful API framework
- **Entity Framework Core** - ORM with SQLite (MVP)
- **Repository Pattern** - Clean data access layer
- **Service Layer** - Business logic encapsulation

### Infrastructure
- **Frontend Hosting:** Vercel (free tier)
- **Backend Hosting:** Render (free tier)
- **Authentication:** Supabase (free tier)
- **Database:** SQLite (file-based, bundled with backend)

## Getting Started

### Prerequisites
- Node.js 18+ and npm
- .NET SDK 8.0+
- Supabase account (free tier)

### Frontend Setup

```bash
cd frontend
npm install
cp .env.example .env.local
# Edit .env.local with your Supabase credentials
npm run dev
```

Frontend runs at: http://localhost:3000

### Backend Setup

```bash
cd backend
export PATH="$HOME/.dotnet:$PATH"
dotnet restore
dotnet build
dotnet run --urls "http://localhost:5000"
```

Backend runs at: http://localhost:5000

### Environment Variables

**Frontend (`.env.local`):**
```env
NEXT_PUBLIC_SUPABASE_URL=https://your-project.supabase.co
NEXT_PUBLIC_SUPABASE_ANON_KEY=your-anon-key
NEXT_PUBLIC_API_URL=http://localhost:5000
```

**Backend (`appsettings.Development.json`):**
```json
{
  "ConnectionStrings": {
    "DatabaseConnectionString": "Data Source=fleetfuel.db"
  },
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "Key": "your-service-role-key"
  },
  "Frontend": {
    "Url": "http://localhost:3000"
  }
}
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login (returns JWT)

### Vehicles
- `GET /api/vehicles` - List user's vehicles
- `POST /api/vehicles` - Create vehicle
- `PUT /api/vehicles/{id}` - Update vehicle
- `DELETE /api/vehicles/{id}` - Soft delete vehicle

### Trips
- `GET /api/trips` - List all trips
- `GET /api/trips/{id}` - Get trip details
- `POST /api/trips` - Create trip
- `PUT /api/trips/{id}` - Update trip
- `DELETE /api/trips/{id}` - Soft delete trip

### Receipts
- `GET /api/receipts` - List all receipts
- `GET /api/receipts/{id}` - Get receipt details
- `POST /api/receipts` - Create receipt
- `PUT /api/receipts/{id}` - Update receipt
- `DELETE /api/receipts/{id}` - Soft delete receipt

### Summary
- `GET /api/summary?year=2024` - Get yearly summary
- `GET /api/summary/export?year=2024` - Download CSV

## User Flow

### First-Time Users
1. **Register** - Create account with email/password
2. **Add Vehicle** - Guided prompt to add first vehicle
3. **Log Trip** - Guided prompt after vehicle creation
4. **Upload Receipt** - Final onboarding step

### Regular Use
1. **Dashboard** - View KPIs and recent activity
2. **Quick Actions** - Fast access to log trips/receipts
3. **Reports** - Generate yearly summaries for taxes

## Deployment

See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed production deployment instructions.

### Quick Deploy

**Frontend (Vercel):**
```bash
cd frontend
npx vercel --prod
```

**Backend (Render):**
1. Connect GitHub repo to Render
2. Set root directory: `backend`
3. Build: `dotnet publish -c Release -o out`
4. Start: `dotnet out/FleetFuel.Api.dll`

## Project Structure

```
FleetFuel-No2/
├── frontend/                 # Next.js frontend
│   ├── src/
│   │   ├── app/             # App Router pages
│   │   ├── components/      # Reusable UI components
│   │   ├── contexts/        # React contexts
│   │   └── lib/api/         # API clients
│   └── package.json
│
├── backend/                  # ASP.NET Core backend
│   ├── Controllers/         # REST API controllers
│   ├── Services/            # Business logic
│   ├── Repositories/        # Data access
│   ├── Data/               # EF Core entities
│   └── Models/              # Domain models
│
├── scripts/                  # Utility scripts
├── DEPLOYMENT.md            # Deployment guide
└── README.md
```

## MVP Definition of Done

- [x] User can register and log in
- [x] User can create at least one vehicle
- [x] User can log trips for that vehicle
- [x] User can upload receipts linked to vehicle
- [x] User can generate yearly CSV summary
- [x] App is deployed and accessible publicly (pending)
- [x] Entire system runs on 0€ infrastructure

## Post-MVP Features (Out of Scope)

- OCR receipt parsing
- Google Drive integration
- Multi-user company accounts
- PDF export
- Real-time sync
- Accountant portal

## License

MIT License - feel free to use for personal or commercial projects.

## Contributing

Contributions welcome! Please read contributing guidelines before submitting PRs.

---

Built with ❤️ by Sebastijan Bogdan
