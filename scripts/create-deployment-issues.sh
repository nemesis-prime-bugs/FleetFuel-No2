#!/bin/bash
# GitHub Issue Creator for FleetFuel Deployment
# Usage: ./create-issues.sh <github-token>

set -e

if [ -z "$1" ]; then
    echo "Usage: $0 <github-token>"
    echo "Create deployment issues for FleetFuel"
    exit 1
fi

GITHUB_TOKEN="$1"
REPO="nemesis-prime-bugs/FleetFuel-No2"
BASE_URL="https://api.github.com/repos/$REPO"

# Create Issue function
create_issue() {
    local title="$1"
    local body="$2"
    local labels="$3"

    echo "Creating issue: $title"
    curl -s -X POST "$BASE_URL/issues" \
        -H "Authorization: token $GITHUB_TOKEN" \
        -H "Accept: application/vnd.github.v3+json" \
        -d "{\"title\":\"$title\",\"body\":\"$body\",\"labels\":[$labels]}"
}

# Issue 1: Configure Environment Variables
create_issue "Configure Environment Variables" \
    "Configure all required environment variables for production deployment.

## Tasks
- [ ] Frontend: NEXT_PUBLIC_SUPABASE_URL
- [ ] Frontage: NEXT_PUBLIC_SUPABASE_ANON_KEY
- [ ] Frontend: NEXT_PUBLIC_API_URL
- [ ] Backend: SUPABASE_URL
- [ ] Backend: SUPABASE_KEY

## Acceptance Criteria
All environment variables are set in Vercel and Render dashboards." \
    "\"deployment\""

# Issue 2: Deploy Frontend to Vercel
create_issue "[CRITICAL] Deploy Frontend to Vercel" \
    "Deploy Next.js frontend to Vercel with production environment variables.

## Tasks
- [ ] Connect GitHub repo to Vercel
- [ ] Set root directory to \`frontend\`
- [ ] Add environment variables
- [ ] Deploy to production

## Acceptance Criteria
Frontend accessible and working without errors." \
    "\"deployment\", \"frontend\", \"critical\""

# Issue 3: Deploy Backend to Render
create_issue "[CRITICAL] Deploy Backend to Render" \
    "Deploy ASP.NET Core backend to Render free tier.

## Tasks
- [ ] Connect GitHub repo to Render
- [ ] Configure build: \`dotnet publish -c Release -o out\`
- [ ] Configure start: \`dotnet out/FleetFuel.Api.dll\`
- [ ] Add environment variables
- [ ] Verify health endpoint

## Acceptance Criteria
Backend accessible and health check returns 200." \
    "\"deployment\", \"backend\", \"critical\""

# Issue 4: Test Full User Flow
create_issue "Test Full User Flow in Production" \
    "End-to-end testing of all MVP features in production.

## Tasks
- [ ] Register new user
- [ ] Create vehicle
- [ ] Log trip
- [ ] Upload receipt
- [ ] View dashboard
- [ ] Generate yearly summary
- [ ] Export CSV

## Acceptance Criteria
All user flows work without errors." \
    "\"testing\", \"deployment\""

echo "Deployment issues created successfully!"
