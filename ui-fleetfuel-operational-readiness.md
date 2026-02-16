# FleetFuel – Production Governance & Operational Readiness Specification

This document defines the missing enterprise-grade layers required to make FleetFuel a production-ready, scalable, monetizable SaaS product.

This is the layer that transforms:

Functional SaaS  
into  
Professional, investment-grade SaaS.

---

# 1. Data Governance & Integrity Specification

## 1.1 Odometer Integrity Rules

- Trips must not overlap for the same vehicle.
- EndKm must always be greater than StartKm.
- StartKm must match the previous trip's EndKm unless:
  - User explicitly overrides with reason.
- Editing historical trips requires:
  - Edit reason field (mandatory)
  - Audit log entry created

## 1.2 Year Lock Mechanism

- When user exports yearly summary:
  - System offers to "Lock Year".
- Locked year:
  - No further edits allowed.
  - Unlock requires admin confirmation.
  - All unlock actions logged.

## 1.3 Audit Log System

AuditLog table:

- Id
- EntityName
- EntityId
- ActionType (Create / Update / Delete / Unlock)
- OldValues (JSON)
- NewValues (JSON)
- UserId
- Timestamp

Audit logs:
- Cannot be deleted
- Retained minimum 7 years (configurable)

## 1.4 Data Retention Policy

- User data retained until account deletion.
- Deleted accounts:
  - Soft delete immediately
  - Hard delete after 30 days grace period
- Backups retained for 30 days rolling window.
- Logs retained 12–24 months minimum.

---

# 2. Subscription Enforcement Logic

## 2.1 Account States

- Trial
- Active
- GracePeriod
- PaymentFailed
- Suspended
- Cancelled

## 2.2 Limit Enforcement

Free Tier Limits:
- 1 vehicle
- 100 receipts per year
- No OCR
- No PDF export

When limit reached:
- Soft warning at 80%
- Hard block at 100%
- Upgrade CTA displayed

## 2.3 Payment Failure Logic

- 7-day grace period
- Reminder emails day 1, 3, 6
- On day 8 → account suspended
- Data remains accessible read-only
- Reactivation restores full access

---

# 3. Performance Standards (SLA Targets)

API:
- Average response time < 300ms
- P95 < 500ms

Dashboard:
- Fully rendered under 1.5 seconds

Image Upload:
- Max 5MB
- Upload under 3 seconds on 4G

Cold Start:
- Acceptable max 2 seconds (free tier)
- Production target < 800ms

Pagination:
- Default 20 items
- Max 100 items per request

---

# 4. Scalability Strategy

## 4.1 Database

- PostgreSQL production
- Indexed:
  - UserId
  - VehicleId
  - Date
  - Year

## 4.2 Storage

- S3 or Blob storage
- CDN for receipt images
- Image compression pipeline

## 4.3 Background Processing

- OCR handled via background job
- Summary caching via Redis
- Heavy exports processed async

## 4.4 Migration Strategy

- SQLite (dev only)
- Production PostgreSQL
- Data migration scripts maintained

---

# 5. Sync & Conflict Resolution

## 5.1 Multi-Device Conflict Handling

Policy:
- Last-write-wins
- Timestamp comparison
- Conflict logged

For enterprise:
- Optional edit locking

## 5.2 Offline Mode Sync

- Offline entries stored locally
- Sync queue
- Conflict resolution dialog if mismatch

---

# 6. Backup & Disaster Recovery

## 6.1 Backup Policy

- Daily full DB backup
- Hourly incremental backups
- 30-day retention

## 6.2 Recovery Targets

- RPO (Recovery Point Objective): 1 hour
- RTO (Recovery Time Objective): 4 hours

## 6.3 Incident Response

- Incident detection via monitoring
- Automated alert to admin
- Status page update
- Post-mortem documentation required

---

# 7. Observability & Monitoring

## 7.1 Logging

- Structured logs
- Error-level alerts
- Failed login tracking
- Suspicious activity monitoring

## 7.2 Metrics

Track internally:

- DAU
- WAU
- Trips per user per week
- Receipts per vehicle
- Export frequency
- Feature usage
- Churn rate
- Conversion rate (Free → Pro)

---

# 8. Notification System

## 8.1 System Notifications

- Receipt limit warning (80%)
- Trip reminder (optional)
- Year-end export reminder
- Subscription renewal
- Payment failure
- Storage nearing capacity

## 8.2 Delivery Channels

- In-app notifications
- Email notifications

Push notifications (optional later)

---

# 9. Analytics & Growth Instrumentation

Track:

- Time to first vehicle
- Time to first trip
- Time to first receipt
- Drop-off points in onboarding
- Feature adoption
- Most used filters

Use this data to optimize UX.

---

# 10. Edge Case Handling Matrix

Defined behaviors:

- Deleting vehicle with trips → block or require reassignment
- Editing initial mileage → recalculation confirmation
- Timezone consistency enforced per user
- Currency locked per company
- Negative fuel values blocked
- Duplicate receipt detection warning

---

# 11. Support Infrastructure

## 11.1 In-App Support

- Help center link
- Submit support ticket
- Feature request form

## 11.2 Response Targets

- Free tier: 72 hours
- Pro: 24 hours
- Enterprise: SLA defined

---

# 12. Legal & Compliance

- GDPR export (JSON/ZIP)
- Account deletion endpoint
- Consent logging
- Terms & Privacy versioning
- Data processing agreement (for enterprise)

---

# 13. Psychological Trust Indicators

Display:

- Last sync time
- Last backup timestamp
- Export confirmation checksum
- Year lock status badge
- Secure connection indicator

These increase perceived reliability.

---

# 14. Operational Checklist Before Production

✔ Backup system operational  
✔ Monitoring active  
✔ Rate limiting enabled  
✔ Audit logs immutable  
✔ Subscription enforcement tested  
✔ Limits enforced correctly  
✔ Pagination on all lists  
✔ Export matches dashboard totals  
✔ Performance targets validated  
✔ Data retention policy enforced  

---

# Final Definition

FleetFuel is production-ready when:

- Data integrity rules are enforced.
- Subscription logic is reliable.
- Performance meets defined SLA.
- Backup and recovery tested.
- Monitoring and alerts active.
- UX friction minimized.
- Edge cases handled explicitly.
- Compliance mechanisms operational.

This document defines the operational backbone of FleetFuel.
