# AGENTS.md

This file provides repository-specific guidance for coding agents working in this project.

## Repository Context

- Project type: ASP.NET Core Web API
- Target framework: `net10.0`
- Data access: EF Core with SQL Server
- API docs: Swagger/OpenAPI

## High-Value Commands

Run from project root (`src/ECommereceAPI`):

```powershell
dotnet restore
dotnet build
dotnet run
```

For EF Core:

```powershell
dotnet ef migrations add <Name>
dotnet ef database update
```

## Code Guidelines

- Keep startup wiring in `Program.cs` minimal and explicit.
- Register new dependencies through DI in `Program.cs`.
- Keep EF relationship rules inside `ApplicationDbContext.OnModelCreating`.
- Prefer small, focused controllers and move business logic into `Services/`.
- Avoid changing generated folders (`bin/`, `obj/`).

## Configuration Standards

Use these standard sections in `appsettings.json`:

- `ConnectionStrings`
- `Jwt`
- `Logging`
- `AllowedHosts`

Do not place logging keys under `Jwt`.

## Safety and Validation

Before finalizing changes:

1. Run `dotnet restore` if package references changed.
2. Run `dotnet build` and confirm no new compile errors.
3. If binary lock errors occur during build, stop the running app process and rebuild.

## Known Cleanup Opportunities

- Resolve nullable warnings in model classes.
- Normalize namespace spelling (`ECommerceAPI` vs `ECommereceAPI`).
