# FaunaFinder - Claude Code Context

## Project Overview

FaunaFinder is a wildlife observation and reporting platform for Puerto Rico. Students can report wildlife sightings, and teachers can review and approve them.

## Tech Stack

- **Backend**: .NET 10, Minimal APIs
- **Frontend**: Blazor WebAssembly
- **UI Components**: MudBlazor
- **Database**: PostgreSQL with EF Core
- **Orchestration**: .NET Aspire
- **Localization**: Custom i18n with Spanish/English support

## Architecture

Vertical slice / modular monolith architecture. Each feature is isolated and could be extracted into a microservice.

### Feature Structure

```
src/Features/{Feature}/
├── FaunaFinder.{Feature}.Api/              # Minimal API endpoints
├── FaunaFinder.{Feature}.Application/       # Business logic, services
├── FaunaFinder.{Feature}.Application.Client/ # HTTP client for Blazor WASM
├── FaunaFinder.{Feature}.Contracts/         # DTOs, requests, responses, validators (ZERO dependencies)
├── FaunaFinder.{Feature}.Database/          # Models, DbContext, migrations
└── FaunaFinder.{Feature}.DataAccess/        # Repositories (return DTOs, not entities)
```

### Key Rules

- `.Contracts` projects have NO project dependencies (usable by WASM)
- Repositories return DTOs from Contracts, not entities
- Cross-feature references by ID only (no navigation properties)
- Each feature has its own database and DbContext

### Current Features

- **Identity** - Authentication, user management
- **Wildlife** - Sighting reports, species search, review queue

## Project Structure

```
src/
├── Common/                          # Shared contracts (Pagination, i18n)
├── Features/                        # Feature modules
├── FaunaFinder.Api/                 # Host application
├── FaunaFinder.Client/              # Blazor WASM UI
├── FaunaFinder.AppHost/             # Aspire orchestration
└── FaunaFinder.ServiceDefaults/     # Shared Aspire config
```

## Common Commands

```bash
# Run the application (from repo root)
dotnet run --project src/FaunaFinder.AppHost

# Build solution
dotnet build

# Run specific project
dotnet run --project src/FaunaFinder.Api

# Add migration (replace {Feature} and {MigrationName})
dotnet ef migrations add {MigrationName} --project src/Features/{Feature}/FaunaFinder.{Feature}.Database

# Update database
dotnet ef database update --project src/Features/{Feature}/FaunaFinder.{Feature}.Database
```

## Coding Conventions

### Naming

- Features: PascalCase singular (`Wildlife`, `Identity`)
- Endpoints: `{Feature}Endpoints.cs`
- DbContext: `{Feature}DbContext.cs`
- Repositories: `I{Entity}Repository.cs` / `{Entity}Repository.cs`
- Client services: `I{Feature}Client.cs` / `{Feature}Client.cs`

### API Endpoints

- Use Minimal APIs with `MapGroup`
- Group by feature: `/api/{feature}/...`
- Use `RequireAuthorization()` for protected endpoints
- Return DTOs from Contracts, not entities

### Blazor Pages

- Located in `FaunaFinder.Client/Pages/{Feature}/`
- Use `@inject IAppLocalizer L` for translations
- Use MudBlazor components

### Localization

- Keys format: `{Feature}_{Context}_{Name}` (e.g., `Sighting_Form_Submit`)
- Add translations to both `FaunaFinder.Api/Services/Localization/Translations.cs` and `FaunaFinder.Client/Services/Localization/Translations.cs`

## Database

- PostgreSQL via Aspire
- Each feature has its own database
- Connection strings managed by Aspire

## Git Workflow

- Branch naming: `feature/issue-{number}-{short-description}`
- Commit messages: Descriptive, no co-author needed
- PRs target `master` branch
