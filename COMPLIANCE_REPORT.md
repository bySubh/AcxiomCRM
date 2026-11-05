# AcxiomCRM compliance report — source review, 2026-11-05

**Verification environment:** source review only. `dotnet` SDK is not installed in this execution environment. No build, integration test, database migration, browser test, or live API test was executed. Therefore **nothing is classified as Implemented and verified**. The original database is not included in the ZIP, so existing data preservation cannot be tested.

| Requirement | Status | Source evidence / outstanding work |
|---|---|---|
| A Identity register/login/logout, hashing, policy, lockout | Implemented but not fully verified | `Program.cs`, `Controllers/AccountController.cs`; cookie secure policy and auth rate limiting configured; end-to-end testing outstanding |
| B Roles and resource authorization | Partially implemented | `Controllers/UsersController.cs`, CRM controllers, `CustomerWriteApiController.cs`; manager ownership filtering added, but team assignment UI and comprehensive access tests missing |
| C Dashboard | Implemented but not fully verified | `Controllers/HomeController.cs`, `Views/Home/Index.cshtml`; eight KPIs, three Chart.js charts and date filters added; monthly sales uses won opportunity creation month, not true close date |
| C Customer module | Partially implemented | `Controllers/CustomersController.cs`, `Views/Customers`; CRUD and expanded text search; details, pagination, sorting incomplete |
| C Lead module | Partially implemented | `Controllers/LeadsController.cs`, `Views/Leads`; CRUD, status; conversion workflow incomplete |
| C Opportunity module | Partially implemented | `Controllers/OpportunitiesController.cs`, `Views/Opportunities`; CRUD, validation, weighted KPI; pipeline workflow incomplete |
| C Follow-up module | Partially implemented | `Controllers/FollowUpsController.cs`, `Views/FollowUps`; CRUD and planned-date rule; explicit completion/reschedule audit workflow missing |
| C Activity module | Partially implemented | `Controllers/ActivitiesController.cs`, `Models/Entities.cs`; four allowed types, CRUD; type/date/status/assigned filtering incomplete |
| C User/role administration | Partially implemented | `Controllers/UsersController.cs`; admin-only role and account status actions; secure password reset, full user management missing |
| C Audit module | Partially implemented | `Services/AuditService.cs`, `Controllers/AuditController.cs`; writes login and CRUD events, IP capture; old/new snapshots, security immutability controls and filters incomplete |
| C REST API | Partially implemented | `Controllers/ApiController.cs`, `Controllers/CustomerWriteApiController.cs`; required routes added, DTO input, basic status codes; error standardization, robust concurrency and automated tests missing |
| C Reports | Partially implemented | `Controllers/ReportsController.cs`, `Views/Reports/Index.cshtml`; only stage report; other report categories missing |
| D Required fields and relationships | Partially implemented | `Models/Entities.cs`, `Data/AppDbContext.cs`; owner/date aliases present; required named fields, navigation/FK constraints and EF migrations missing |
| E Business validation | Partially implemented | `Models/Entities.cs`, CRM controllers and API; critical validations exist; consistency across MVC/API, uniqueness normalization and related-record authorization tests missing |
| F KPI/chart/date filters | Implemented but not fully verified | `HomeController.cs`, `Views/Home/Index.cshtml`; data-driven queries; date attribution and date boundary testing outstanding |
| G Search/filter/sort/page | Partially implemented | CRM controllers; text searches expanded; sorting/pagination and full per-field filters missing |
| H API requirements | Partially implemented | `ApiController.cs`, `CustomerWriteApiController.cs`; authenticated routes, DTOs, input validation; missing systematic error middleware, concurrency handling, API-specific rate limiting |
| I Audit logging | Partially implemented | `AuditService.cs`; basic events and IP; old/new, immutable append-only storage enforcement missing |
| J Security | Partially implemented | `Program.cs`, Account and CRM controllers; cookie HTTPS, antiforgery, identity, login rate limiter; HTTPS deployment, migration, role/security testing outstanding |
| K Acceptance scenarios 1–14 | Not implemented | No executable acceptance test suite in delivered archive; no test execution performed |
| Database migration and existing data preservation | Blocked | Original ZIP lacks database and migration history; `EnsureCreated` remains until safe migration baseline can be determined |

## Known deployment concerns

1. This archive is a **work-in-progress**, not a verified compliant release.
2. Set `ConnectionStrings:Default` via environment-specific secure configuration before deployment. Do not commit production secrets.
3. `ManagerId` was added to `AppUser` but requires a migration and administrator-controlled team assignment; it is not yet usable with an existing schema without migration.
4. Do not run destructive database creation or migration commands against a production database. Back up existing data and review migration SQL first.
5. Run `dotnet restore`, `dotnet build`, migration generation, and role/validation/API acceptance tests in a .NET 8 environment.
