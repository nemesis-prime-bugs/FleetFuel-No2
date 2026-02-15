# Database Migrations Guide

## Initial Setup

The database uses SQLite for MVP, designed for migration to PostgreSQL later.

## Commands

### Create Migration (after model changes)
```bash
cd backend
~/.dotnet/dotnet ef migrations add MigrationName
```

### Apply Migrations (creates/updates database)
```bash
~/.dotnet/dotnet ef database update
```

### Remove Last Migration (if not applied)
```bash
~/.dotnet/dotnet ef migrations remove
```

### Generate SQL Script (for production)
```bash
~/.dotnet/dotnet ef migrations script
```

## Database File
- Location: `backend/fleetfuel.db`
- Connection: `Data Source=fleetfuel.db`

## Entity Relationships

```
User (1) ───> (N) Vehicle
Vehicle (1) ───> (N) Trip
Vehicle (1) ───> (N) Receipt
User (1) ───> (N) Trip
User (1) ───> (N) Receipt
```

## Soft Delete
All entities have `IsDeleted` property with global query filter — deleted records are filtered by default.

## Current Status
- Entities defined: User, Vehicle, Trip, Receipt
- DbContext configured
- Ready for initial migration
