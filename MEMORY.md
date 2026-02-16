# MEMORY.md - Core Long-Term Memory

---

## 🚨 CRITICAL SECURITY RULE: NEVER Deploy Secrets

**Added: 2026-02-16**
**Priority: ABSOLUTE - No exceptions**

### Golden Rules
```
❌ NEVER commit: API keys, tokens, passwords, private keys
❌ NEVER commit: Environment variables (.env files)
❌ NEVER commit: Supabase keys, database credentials
❌ NEVER commit: GitHub PATs

✅ ONLY in: .env (local), CI/CD secrets (GitHub/Render)
✅ ONLY in: Environment variables on deployment platform
```

### Before Every Commit
1. Check `git diff --cached` for secrets
2. Run `git log --all --oneline --grep="pat\|token\|key\|secret"`
3. Verify no .env files staged
4. Use pre-commit hook to catch secrets

### If Leaked
1. IMMEDIATELY revoke the token/secret
2. Rotate all related credentials
3. Audit access logs
4. Document incident

---

## CRITICAL RULE: Post-Push Build Verification

**Added: 2026-02-16**

**AFTER EVERY COMMIT AND PUSH TO MAIN BRANCH:**

1. Wait for GitHub Actions to complete (5-10 seconds)
2. Check CI/CD workflow status
3. Verify build success before doing anything else
4. **If build fails:**
   - Create GitHub issue immediately
   - NOTIFY Sebastijan
   - Fix before continuing normal work

---

## GitHub PAT
- **SECURITY NOTE:** PAT is stored securely in environment variables
- Never commit PAT to code or documentation
- Get PAT from: GitHub Settings → Developer settings → Personal access tokens

---

## Production URLs
- **Frontend:** https://frontend-iota-lac-88.vercel.app
- **Backend:** https://fleetfuel.onrender.com
- **Health Check:** https://fleetfuel.onrender.com/health

---

## Project Structure
- Backend: .NET 8 API
- Frontend: Next.js 15 (React)
- Database: SQLite (MVP), PostgreSQL (Production via Supabase)
- Deployment: Render (Backend), Vercel (Frontend)

---

## Issue Tracking
- Use GitHub API for issue management
- Set issues to "in progress" before working
- Close issues when complete
- Create TEST-xxx issues linked to PROD-xxx features

---

## Testing Standards
- AAA Pattern (Arrange-Act-Assert)
- Descriptive test names: `Method_Scenario_ExpectedResult`
- Target: 70% coverage minimum
- Run tests before pushing

---

## HEARTBEAT Checks (Every 15 minutes)
1. Task presence check
2. Progress evaluation
3. Drift detection
4. **Production monitoring (CRITICAL)**
5. Memory update

### Production Monitoring Checklist
| Check | URL | Expected |
|-------|-----|----------|
| Backend Health | https://fleetfuel.onrender.com/health | `{"status":"healthy"}` |
| Frontend Health | https://frontend-iota-lac-88.vercel.app | 200 OK |
| GitHub Actions | Workflow runs | No failures |

---

## Never Forget
- ✅ Check build after every push
- ✅ Close issues when complete
- ✅ Set issues to "in progress" before working
- ✅ Write unit tests for all new features
- ✅ Monitor production on every heartbeat
- ✅ **NEVER commit secrets**