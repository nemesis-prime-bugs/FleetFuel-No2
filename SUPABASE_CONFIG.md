# Supabase Configuration for FleetFuel

## Email Confirmation Issue
**Problem:** Confirmation emails use `localhost:3000` instead of Vercel URL.

**Solution:** Configure Supabase Dashboard settings below.

---

## Step 1: Update Site URL

1. Go to: https://brbvzgpemmpytwofcrpu.supabase.co
2. Login → **Authentication** → **Configuration**
3. Find **Site URL** field
4. Change to: `https://frontend-iota-lac-88.vercel.app`
5. **Save**

## Step 2: Add Redirect URLs

In the same **Configuration** section, find **Redirect URLs**:

Add these URLs (one per line):
```
https://frontend-iota-lac-88.vercel.app/auth/callback
http://localhost:3000/auth/callback
http://localhost:3001/auth/callback
```

## Step 3: Verify Email Provider

1. Go to **Authentication** → **Providers**
2. Click **Email** (or **Email / Password**)
3. Verify **Enabled** is toggled ON
4. Settings should include:
   - Confirm email: **ON** (required for email confirmation)
   - Secure protective mode: **OFF** (optional)

## Step 4: Email Template (Optional)

1. Go to **Authentication** → **Emails** → **Templates**
2. Find **Confirmation template**
3. The default template should work, but you can customize:
   - Subject: "Confirm your FleetFuel account"
   - Body: "Click the link below to confirm your email: {{ .ConfirmationURL }}"

**Note:** The `{{ .ConfirmationURL }}` variable will be generated based on your Site URL + redirect URLs.

## Step 5: Test

1. Register a new user at `/register`
2. Check confirmation email
3. Verify the link has `frontend-iota-lac-88.vercel.app` NOT `localhost:3000`
4. Click the link
5. Should redirect to `/dashboard`

---

## Alternative: Custom Email Template

If you need full control over the email, you can disable Supabase's default emails and send your own using the Supabase Auth Admin API.

But for MVP, the dashboard configuration above should work.

---

## Environment Variables

Verify these are set in **Render Dashboard** (backend):
```
SUPABASE_URL=https://brbvzgpemmpytwofcrpu.supabase.co
SUPABASE_ANON_KEY=<your-anon-key-from-supabase-settings-api>
```

Verify these are set in **Vercel Dashboard** (frontend):
```
NEXT_PUBLIC_SUPABASE_URL=https://brbvzgpemmpytwofcrpu.supabase.co
NEXT_PUBLIC_SUPABASE_ANON_KEY=<your-anon-key>
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Email says "localhost" | Update Site URL in Supabase Dashboard |
| 404 on callback | Check Redirect URLs include production URL |
| Token expired | Request new confirmation email |
| Invalid code | Clear browser cache, try incognito mode |

---

## Supabase CLI (Optional)

For programmatic configuration:

```bash
# Install Supabase CLI
npm install -g supabase

# Login
supabase login

# Link project
supabase link --project-ref <your-project-ref>

# Update site URL (if supported)
supabase db update ...
```

Note: Site URL configuration may require manual dashboard setup.