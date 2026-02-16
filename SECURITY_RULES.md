# 🚨 CORE SECURITY RULES - NEVER VIOLATE 🚨

**Priority: ABSOLUTE - No exceptions, no excuses, no shortcuts**

---

## GOLDEN RULES - Never Break These

### 1. NEVER Commit Secrets
```
❌ NEVER commit: API keys, tokens, passwords, private keys
❌ NEVER commit: Environment variables (.env files)
❌ NEVER commit: Connection strings
❌ NEVER commit: Supabase keys
❌ NEVER commit: Database credentials

✅ ONLY commit: .env.example (template without values)
```

### 2. Secrets Management
```
Secrets go ONLY in:
- Environment variables (local development)
- CI/CD secrets (GitHub Secrets, Render Secrets)
- Supabase Dashboard (environment variables)

NEVER in:
- Code files
- Documentation
- Comments
- Git history
```

### 3. GitHub PAT Security
```
✅ Store in: GitHub Settings → Developer settings → Personal access tokens
✅ Use: Fine-grained PATs with minimal permissions
✅ Rotate: Every 90 days or immediately if compromised

❌ NEVER in: Any file that could be committed
❌ NEVER in: Documentation
❌ NEVER in: Memory files
```

### 4. Supabase Security
```
✅ NEXT_PUBLIC_SUPABASE_URL - Can be public (anon key)
✅ NEXT_PUBLIC_SUPABASE_ANON_KEY - Can be public (restricted)
✅ SUPABASE_SERVICE_ROLE_KEY - NEVER expose, use server-side only

❌ NEVER commit: Any service role key
❌ NEVER commit: Master keys
```

---

## Verification Checklist (Before Every Commit)

Before `git commit`:

- [ ] No API keys in code
- [ ] No tokens in comments
- [ ] No passwords in strings
- [ ] .env files not staged
- [ ] Check `git diff --cached` for secrets
- [ ] Run `git log --all --oneline --grep="pat\|token\|key\|secret"` - ensure none

Before `git push`:

- [ ] CI/CD secrets configured in GitHub
- [ ] Render secrets configured
- [ ] All environment variables documented in .env.example
- [ ] No secrets in commit history

---

## Automated Protection

Add pre-commit hook to catch secrets:

```bash
#!/bin/bash
# Add to .git/hooks/pre-commit

# Check for common secret patterns
patterns=(
  "ghp_[a-zA-Z0-9]{36}"
  "github_pat_[a-zA-Z0-9]{22}_[a-zA-Z0-9]{59}"
  "ey[a-zA-Z0-9_-]{20,}\.[a-zA-Z0-9_-]{20,}"
  "sk_live_[a-zA-Z0-9]{24}"
)

for pattern in "${patterns[@]}"; do
  if git diff --cached | grep -iE "$pattern" > /dev/null; then
    echo "🚨 SECRET DETECTED IN STAGED CHANGES!"
    echo "Do NOT commit secrets!"
    exit 1
  fi
done
```

---

## If Secrets Are Leaked

1. **IMMEDIATELY revoke the secret**
2. **Rotate all related credentials**
3. **Audit access logs**
4. **Notify affected users if needed**
5. **Document incident**

---

## Production Deployment Checklist

Before deploying to production:

- [ ] All secrets in environment variables
- [ ] No hardcoded credentials in code
- [ ] API rate limiting enabled
- [ ] CORS configured for specific origins only
- [ ] HTTPS enforced
- [ ] Security headers configured

---

## Remember

> **One leaked token can compromise the entire project, user data, and infrastructure.**

**Security is not optional. Security is not a feature. Security is a requirement.**

---

**Violation of these rules is a fireable offense.**

---

*Last Updated: 2026-02-16*
*Author: Madara Uchiha*
*Priority: ABSOLUTE*