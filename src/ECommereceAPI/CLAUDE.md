# CLAUDE.md

Repository guide for Claude or Claude-style coding assistants.

## Mission

Make safe, minimal, verifiable changes to this .NET 10 Web API.

## Quick Orientation

- App entry: `Program.cs`
- Data model and mappings: `Data/ApplicationDbContext.cs`
- HTTP layer: `Controllers/`
- Domain entities: `Models/`
- Config: `appsettings.json`, `appsettings.Development.json`

## Expected Workflow

1. Read relevant files before editing.
2. Prefer targeted edits over broad refactors.
3. Preserve existing behavior unless asked to change it.
4. Validate with `dotnet build` after edits.

## Dependency Rules

- Add only required NuGet packages.
- Keep package versions compatible with .NET 10.
- Prefer official Microsoft packages for platform features when possible.

## API and Data Conventions

- Keep request pipeline and middleware order intentional.
- Keep EF Core relationship definitions centralized in DbContext.
- Add new services in `Services/` and register them in DI.

## Configuration Conventions

- Use standard key casing and section names (`Jwt`, `Logging`, etc.).
- Keep secrets and environment-specific values out of source-controlled defaults when possible.

## Validation Checklist

- Build passes (`dotnet build`).
- No new compile errors introduced.
- Config files remain valid JSON.
- New files follow existing project naming and structure.
