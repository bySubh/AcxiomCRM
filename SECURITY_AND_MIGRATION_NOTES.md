# Security and migration notes

The existing project uses `EnsureCreated()` and does not contain an EF Core migration baseline. **Do not switch directly to `Migrate()` against an existing SQLite database**: that can attempt to recreate tables and damage availability. Back up the SQLite database first; generate a baseline migration with the EF CLI in a .NET 8 environment, review its SQL and reconcile existing schema/data before applying it.

New model-level foreign keys and indexes only take effect after a reviewed migration. Existing deployments remain structurally unchanged until migrated.

Important unresolved requirements: full audit old/new snapshots and atomicity, robust error handling, report types and exports, full pagination and filter controls, role management and user workflows, complete DTO architecture, configuration-driven numeric limits, acceptance/integration tests, and production deployment review.

No acceptance tests have been executed in this environment because `dotnet` is not installed.
