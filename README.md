# AcxiomCRM — VS Code project (ASP.NET Core MVC)

## Requirements
- .NET 8 SDK
- VS Code + C# Dev Kit extension
- Internet for NuGet and Bootstrap/Chart.js CDNs

## Run
```bash
dotnet restore
dotnet run
```
Open the localhost URL printed by `dotnet run`. SQLite database is created automatically.

## First admin setup
Public registration creates SalesExecutive users only. To promote a trusted account to Admin **in a local development database only**, use SQLite tooling to inspect `AspNetUsers` and `AspNetRoles`, then insert the matching `AspNetUserRoles` association. Do not expose a public admin-registration endpoint. Alternatively implement a controlled one-time admin seed via environment variables before deployment.

## Features
Identity login/register/logout and lockout, scoped CRUD, validation, audit events, pipeline dashboard, manager/admin reports, admin user roles, read-only secured JSON API.

## Known limitations / work remaining
This is a **starter implementation, not a fully verified assignment submission**. The project has not been compiled in this environment (the .NET SDK is unavailable). Additional work is required: lead conversion workflow, robust team-based Manager scoping, complete APIs with DTOs and POST/PUT/DELETE, report exports, full dashboard charts, stronger foreign-key and workflow validation, admin provisioning, pagination, automated tests, and full acceptance testing. Avoid using it with real customer data until reviewed and secured.
