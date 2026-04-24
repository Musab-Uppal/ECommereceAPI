# ECommereceAPI

ASP.NET Core Web API for an e-commerce domain, targeting .NET 10.

## Tech Stack

- .NET 10 (ASP.NET Core Web API)
- Entity Framework Core 10
- SQL Server provider for EF Core
- Swagger/OpenAPI via Swashbuckle

## Current Scope

- API bootstrapping and middleware setup
- SQL Server DbContext wiring
- Domain models for users, products, categories, orders, order items, and reviews
- A sample Product controller endpoint

## Project Structure

- `Program.cs` - app startup, DI registration, middleware pipeline
- `Data/ApplicationDbContext.cs` - EF Core DbContext and relationship configuration
- `Models/` - domain entities
- `Controllers/` - API controllers
- `Services/` - business service placeholders
- `appsettings.json` - default configuration
- `appsettings.Development.json` - development overrides

## Prerequisites

- .NET SDK 10.x
- SQL Server instance (local or remote)

## Configuration

Update `appsettings.json` for your environment:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpireMinutes`

## Run Locally

From the project directory:

```powershell
dotnet restore
dotnet build
dotnet run
```

Swagger UI is available in Development mode at:

- `https://localhost:<port>/swagger`

## EF Core Commands

Examples:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Notes

- Nullable reference warnings currently exist in model classes and can be addressed by marking required properties as `required` (or nullable where appropriate).
- Namespace spelling is currently mixed between `ECommerceAPI` and `ECommereceAPI`; keeping one convention will improve maintainability.
