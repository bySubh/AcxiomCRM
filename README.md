# AcxiomCRM

**AcxiomCRM** is a role-based Customer Relationship Management (CRM) web application built with ASP.NET Core MVC, ASP.NET Core Identity, Entity Framework Core, and SQLite. It provides a foundation for managing customers, leads, opportunities, follow-ups, activities, users, and sales reporting.

> **Project status:** Active development / partially complete. Core registration, login, dashboard, and selected CRM workflows have been tested locally. The latest source changes have **not** been independently compiled or regression-tested. See [Current limitations](#current-limitations) before deploying.

## Contents

- [Features](#features)
- [Technology stack](#technology-stack)
- [Project structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting started](#getting-started)
- [Initial Admin setup](#initial-admin-setup)
- [Roles and access control](#roles-and-access-control)
- [API overview](#api-overview)
- [Validation and security](#validation-and-security)
- [Testing](#testing)
- [Current limitations](#current-limitations)
- [Development notes](#development-notes)

## Features

### Implemented in source

- **Authentication:** User registration, login, logout, password policy, lockout configuration, and secure authentication cookies through ASP.NET Core Identity.
- **Role-based access:** `Admin`, `Manager`, and `SalesExecutive`, with server-side ownership/team filters in the main CRM controllers.
- **Customers:** Create, list, edit, delete, and basic text search.
- **Leads:** Create, list, edit, delete, status tracking, and basic search.
- **Opportunities:** CRUD, stages, amount/probability fields, expected-close-date validation, and related customer/lead checks.
- **Follow-ups:** CRUD, scheduling, status fields, and related-record validation, including scoped `OpportunityId` checks in the latest source.
- **Activities:** CRUD with activity types, statuses, and related customer/lead associations.
- **Dashboard:** Eight KPI cards and Chart.js visualizations for lead status, opportunity pipeline, and monthly sales.
- **Reports:** Pipeline reporting with Admin-wide and Manager team-scoped opportunity data.
- **Administration:** Admin-only user-role management and audit-log access.
- **Audit trail:** Records selected authentication, CRUD, and role-management events.
- **REST API:** Authenticated customer, lead, and opportunity read endpoints and selected write endpoints.
- **Pagination:** Database-side paging on the five main CRM list pages (25 records per page).

**Implementation does not imply that every feature has passed runtime tests.**

## Technology stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| Language | C# |
| Authentication | ASP.NET Core Identity |
| ORM | Entity Framework Core 8 |
| Database | SQLite |
| Views | Razor |
| UI | Bootstrap 5 |
| Charts | Chart.js |
| API | ASP.NET Core controllers / JSON |
| Development | Visual Studio Code or Visual Studio |

## Project structure

```text
AcxiomCRM/
├── Controllers/
│   ├── AccountController.cs
│   ├── CustomersController.cs
│   ├── LeadsController.cs
│   ├── OpportunitiesController.cs
│   ├── FollowUpsController.cs
│   ├── ActivitiesController.cs
│   ├── HomeController.cs
│   ├── ReportsController.cs
│   ├── UsersController.cs
│   ├── AuditController.cs
│   ├── ApiController.cs
│   └── CustomerWriteApiController.cs
├── Data/
│   └── AppDbContext.cs
├── Models/
├── Services/
│   ├── AuditService.cs
│   └── InitialAdminBootstrap.cs
├── Views/
├── wwwroot/
├── Program.cs
├── appsettings.json
├── AcxiomCRM.csproj
└── README.md
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio Code with the C# Dev Kit extension, or Visual Studio
- Internet access to restore NuGet packages and load UI assets served from CDNs

Check the SDK:

```bash
dotnet --version
```

## Getting started

1. Clone the repository:

   ```bash
   git clone https://github.com/YOUR-USERNAME/YOUR-REPOSITORY.git
   cd YOUR-REPOSITORY
   ```

   If the repository contains an `AcxiomCRM` subfolder, change into it before running the commands below.

2. Restore packages and build:

   ```bash
   dotnet restore
   dotnet build
   ```

3. Start the application:

   ```bash
   dotnet run
   ```

4. Open the local HTTPS URL shown in the terminal.

5. Register a user through the registration page. New public registrations are assigned the `SalesExecutive` role.

### Database configuration

The current `appsettings.json` uses:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=acxiomcrm.db"
  }
}
```

The application currently initializes the database with `EnsureCreatedAsync()`. **Do not delete or recreate an existing SQLite database to troubleshoot schema issues.** Back up the database before making any schema or Identity changes. EF Core migrations have not yet been established as a safe upgrade path for existing installations.

## Initial Admin setup

The application uses an **explicit command-line bootstrap** to promote one already registered `SalesExecutive` account to `Admin`. It does not provide a public Admin registration endpoint.

1. Register the intended Admin account normally.
2. Start the application at least once so the database and roles are initialized, then stop it.
3. Set the bootstrap environment variables and run the one-time command.

**PowerShell (Windows):**

```powershell
$env:ACXIOMCRM_ADMIN_BOOTSTRAP = "YES"
$env:ACXIOMCRM_ADMIN_EMAIL = "admin@example.com"
dotnet run -- --bootstrap-initial-admin
Remove-Item Env:ACXIOMCRM_ADMIN_BOOTSTRAP
Remove-Item Env:ACXIOMCRM_ADMIN_EMAIL
```

**Bash (macOS/Linux):**

```bash
ACXIOMCRM_ADMIN_BOOTSTRAP=YES \
ACXIOMCRM_ADMIN_EMAIL=admin@example.com \
dotnet run -- --bootstrap-initial-admin
```

Replace `admin@example.com` with the email of the existing registered account. The command refuses provisioning if an Admin already exists or if the target account does not have exactly the `SalesExecutive` role. Sign out and sign in again after promotion.

> Run the bootstrap only in a trusted environment with access to the intended database. Do not commit credentials, private environment files, or database files containing real user data.

## Roles and access control

| Role | Intended access |
|---|---|
| **Admin** | All CRM records; user management; audit logs; administrative reporting |
| **Manager** | Own records and records owned by users whose `ManagerId` matches the Manager's user ID |
| **SalesExecutive** | Records assigned to that user |

Access is enforced by controller authorization and database-query ownership predicates, not merely by hiding navigation links. The Reports controller is restricted to Admin and Manager; its latest implementation applies team scoping before materializing opportunity data.

Authorization and IDOR behavior should still be verified with multiple test users, especially for related record IDs and write endpoints.

## API overview

The project includes authenticated REST-style endpoints for:

| Resource | Existing operations |
|---|---|
| Customers | List, get by ID, create, update, delete |
| Leads | List, create |
| Opportunities | List, create |

API requests require authentication and are subject to the application's authorization rules. The API is **not yet a complete public integration contract**: some required endpoints, consistent error formats, API documentation, and automated API tests remain outstanding.

## Validation and security

The project uses:

- ASP.NET Core Identity for credential management and password hashing.
- Role-based authorization and ownership/team predicates for CRM data access.
- Model validation and additional business-rule checks in selected controllers.
- Anti-forgery validation for MVC form submissions.
- HTTPS-oriented cookie configuration (`HttpOnly`, `Secure`, and `SameSite`).
- An audit service backed by the application's EF Core database context.
- An explicit, opt-in initial Admin bootstrap command.

These measures are implemented in source; they are **not a substitute for deployment security review or penetration testing**.

## Testing

### Previously confirmed by local manual testing

- Application builds and runs in the developer's environment.
- Registration and login.
- Initial Admin bootstrap using a registered account.
- Admin access to User Management and Audit Logs.
- SalesExecutive denied access to Admin-only pages.
- Dashboard loads with SQLite-compatible decimal aggregation.
- Customer create/list/edit.
- Lead create/list/edit.
- Activity creation and customer association.

### Still to verify

1. Run `dotnet build` against the latest source archive.
2. Run existing automated tests, if available; no dedicated test project was present in the inspected archive.
3. Verify Opportunity and Follow-Up create/edit/delete and status transitions.
4. Test Admin/all, Manager/own-team, and SalesExecutive/own-only visibility.
5. Attempt direct URL ID changes and crafted POSTs with out-of-scope `CustomerId`, `LeadId`, and `OpportunityId`.
6. Verify customer duplicates, amount/probability/date validation, and API error codes.
7. Verify dashboard KPI/chart totals and report aggregation using known sample data.
8. Verify audit entries for create/update/delete, role changes, and failed operations.
9. Check pagination and navigation with more than 25 records.
10. Execute the assignment's 14 acceptance scenarios.

**Build status for latest source changes:** Not independently verified in the environment that produced this README.

## Current limitations

The following assignment requirements remain incomplete or unverified:

- Lead conversion to customer/opportunity.
- Full follow-up completion, missed, rescheduling, and overdue workflows.
- Complete user creation/editing, team assignment, and password-reset administration.
- Comprehensive audit old/new snapshots and audit filtering.
- Complete customer, lead, follow-up, opportunity, sales/conversion, and user-activity reports.
- Additional REST API endpoints and standardized DTO/error handling.
- Full search, filter, and safe sorting across all modules.
- Comprehensive automated authorization, validation, and acceptance tests.
- Data-preserving EF Core migration baseline for existing SQLite databases.

This repository should be treated as a **work-in-progress academic project**, not a production-ready CRM.

## Development notes

- Preserve the existing ASP.NET Core MVC, Identity, EF Core, and SQLite architecture.
- Avoid modifying or resetting the database without a backup and migration plan.
- Reuse the existing `ManagerId` team hierarchy when adding scoped queries.
- Apply ownership filters in SQL **before** `ToListAsync()` or other materialization.
- Validate related record IDs server-side.
- Avoid SQLite-incompatible decimal `Sum()` translations; preserve the existing compatible aggregation approach.
- Run build and regression tests after every change.

## License

No project license has been specified. Add a `LICENSE` file only if you intend to grant reuse rights.

---

**Maintainer:** Add your name and GitHub profile here.  
**Project type:** Academic CRM assignment / ASP.NET Core MVC.