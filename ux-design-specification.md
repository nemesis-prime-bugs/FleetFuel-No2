# FleetFuel – Complete UX & Interaction Engineering Specification
(Production-Level, Premium, Financial-Grade)

This document defines the UX layer that sits on top of the structural product specification.

It ensures FleetFuel is not only functional, but:

- Cognitively efficient
- Fast to use
- Emotionally calm
- Trustworthy
- Executive-grade
- Premium

This specification governs flow, micro-interactions, defaults, behavior, error handling, and usability standards.

---

# 1. UX Philosophy

FleetFuel must:

- Reduce cognitive load
- Minimize steps to complete tasks
- Guide users implicitly
- Never surprise users
- Never overwhelm users
- Feel engineered, not decorated

Primary principle:

> The user should never hesitate.

---

# 2. Core User Flows (Optimized)

## 2.1 First-Time User Flow

Step 1: Register  
Step 2: Immediately prompted to add first vehicle  
Step 3: After vehicle creation → prompted to log first trip  
Step 4: After trip creation → subtle suggestion to upload receipt  

Rules:

- No dashboard overload for first-time users
- Guided onboarding
- Clear next step
- Skip option always available

Goal:
First value delivered within 2 minutes.

---

## 2.2 Trip Logging Flow (Optimized for Speed)

Default Behavior:

- Date auto-filled with today
- Vehicle auto-selected (last used vehicle)
- Start KM auto-filled with last End KM
- Business toggle default = last used value

Validation:

- End KM must be greater than Start KM
- No overlapping trips allowed
- Inline validation only

Keyboard Behavior:

- Enter moves to next field
- Enter on last field submits form
- Escape cancels (desktop)

Goal:
Trip logged in under 15 seconds.

---

## 2.3 Receipt Upload Flow

Mobile-first.

Default Behavior:

- Camera prioritized
- Date auto-detected if possible
- Last used vehicle pre-selected

Flow:

1. Capture photo
2. Preview
3. Enter amount
4. Save

Advanced fields collapsed by default:
- Liters
- Fuel type
- VAT
- Station

Progressive disclosure:
Only show advanced fields if user expands.

Goal:
Receipt saved in under 20 seconds.

---

# 3. Smart Defaults Strategy

Every input must have a reason to be empty.

Defaults required:

- Date → Today
- Vehicle → Last selected
- Business/private → Last used
- Year selector → Current year
- Currency → User default
- Storage provider → Plan default

Never force unnecessary decisions.

---

# 4. Interaction Standards

## 4.1 Primary Action Rules

Each screen must have:

- Exactly one primary action
- Positioned top-right (desktop)
- Floating bottom-right (mobile, when applicable)

Never multiple filled buttons.

---

## 4.2 Confirmation Strategy

Do NOT overuse confirmation dialogs.

Use confirmation only for:

- Deleting vehicle
- Deleting receipt
- Deleting account

For trip deletion:
Provide “Undo” toast instead of confirmation modal.

Premium UX avoids friction.

---

## 4.3 Toast Behavior

Position:
Top-right (desktop)
Top-center (mobile)

Duration:
3 seconds

Style:
Minimal
No large icons
Muted background

---

# 5. Error Handling Standards

Errors must be:

- Specific
- Actionable
- Calm
- Inline whenever possible

Bad:
“Something went wrong.”

Good:
“End KM must be greater than Start KM.”

Server Errors:
Non-intrusive toast + log internally.

Never block entire UI unless critical.

---

# 6. Loading State Strategy

Never show blank screen.

Use:

- Skeleton loaders for tables
- Skeleton cards for dashboard
- Inline button loading state for actions
- Never block entire page for small updates

Loading animation duration:
150–250ms transition

---

# 7. Data Presentation Rules

## 7.1 Financial Data

- Right-aligned
- Two decimal places
- Thousands separator
- Currency symbol consistent

Example:
€ 1,245.30

Never inconsistent formatting.

---

## 7.2 KM Data

- Right-aligned
- No decimals unless necessary

Example:
12,450 km

---

## 7.3 Percentages

- One decimal max
- Example: 67.5%

---

# 8. Density Strategy

Two display modes:

Standard Mode (default)
- Spacious
- Executive-friendly

Compact Mode (optional)
- Reduced row height
- More rows visible
- For power users

Spacing system:
8px grid
24px section spacing
32px major section separation

Never inconsistent spacing values.

---

# 9. Accessibility Standards

- All buttons keyboard accessible
- Tab navigation logical
- Focus states visible
- Contrast ratio compliant
- Click/tap targets minimum 44px (mobile)

Accessibility increases trust and professionalism.

---

# 10. Microinteraction Standards

Hover:

- Subtle elevation
- Slight shadow increase
- No color explosion

Button press:

- Slight scale down (0.98)
- Fast release

Dropdown:

- 150ms fade + slide
- No bounce

Modals:

- Soft fade-in
- Slight vertical motion

No dramatic animations.

---

# 11. Trust Engineering Layer

FleetFuel must feel reliable.

Trust signals:

- Accurate totals always match exports
- No rounding inconsistencies
- Clear audit timestamps
- No visual jitter
- Consistent formatting
- No broken states

Visual trust is created through precision.

---

# 12. Empty State Standards

Every empty state must contain:

- Minimal icon (monochrome)
- Clear title
- Short explanation
- One primary action

Example:

Title:
“No receipts yet”

Description:
“Upload your first fuel receipt to begin tracking expenses.”

Button:
“+ Upload Receipt”

Never use humor or emojis.

---

# 13. Dashboard Design Rules

Dashboard is executive.

Must include:

- Clear KPIs
- Minimal charts
- No clutter
- No floating buttons

Charts:

- Thin lines
- Muted colors
- No gradient backgrounds

Dashboard is read-only summary.

---

# 14. Navigation Standards

Sidebar:

- Fixed width
- Icons optional but subtle
- Active state subtle highlight
- No collapsible chaos

Mobile:

- Bottom tabs max 5
- No overloaded navigation

Predictability > creativity.

---

# 15. UX Quality Checklist (Must Pass Before Release)

✔ User logs a trip in under 15 seconds  
✔ User uploads receipt in under 20 seconds  
✔ No screen has more than one primary button  
✔ No inconsistent spacing  
✔ Financial values formatted consistently  
✔ No modal used unnecessarily  
✔ Errors are inline and specific  
✔ All lists paginated  
✔ Loading states always visible  
✔ No layout shift on content load  

---

# 16. Definition of Good UI for FleetFuel

FleetFuel UI is good when:

- Users never hesitate
- Tasks feel fast
- Data feels trustworthy
- Interface feels calm
- Nothing feels decorative
- Everything feels deliberate

It should feel like professional financial software.

Not like a startup experiment.
