# FaunaFinder Architecture

## Overview

FaunaFinder uses a clean 5-layer architecture designed for maintainability, testability, and performance. The architecture follows projection-based data access patterns and avoids Entity Framework's `.Include()` in favor of explicit `.Select()` projections.

## Layer Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    FaunaFinder.Api                          │
│                   (Blazor Components)                       │
│         Injects: IMunicipalityRepository                    │
│                  ISpeciesRepository                         │
└─────────────────────────┬───────────────────────────────────┘
                          │ uses DTOs from
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                 FaunaFinder.Contracts                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │    DTOs     │  │ Parameters  │  │     Pagination      │  │
│  │ ForListDto  │  │SpeciesParams│  │KeysetPagination     │  │
│  │ ForDetailDto│  │             │  │                     │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└─────────────────────────▲───────────────────────────────────┘
                          │ projects to
┌─────────────────────────┴───────────────────────────────────┐
│                 FaunaFinder.DataAccess                      │
│  ┌───────────────────────────────────────────────────────┐  │
│  │            Repositories (NO .Include!)                │  │
│  │   .Select(s => new SpeciesForDetailDto(...))          │  │
│  │   Uses: IDbContextFactory<FaunaFinderContext>         │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────┬───────────────────────────────────┘
                          │ queries
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                  FaunaFinder.Database                       │
│  ┌─────────────┐  ┌─────────────────────────────────────┐   │
│  │  DbContext  │  │    Models (with nested config)      │   │
│  │  (sealed)   │  │    Species, Municipality, etc.      │   │
│  │  NoTracking │  │    Snake_case naming convention     │   │
│  └─────────────┘  └─────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
                    ┌───────────┐
                    │ PostgreSQL│
                    └───────────┘
```

## Projects

### FaunaFinder.Api
**Purpose**: Blazor Server UI and API endpoints

**Dependencies**:
- FaunaFinder.Database
- FaunaFinder.DataAccess
- FaunaFinder.Contracts
- FaunaFinder.ServiceDefaults

**Key Files**:
- `Program.cs` - Application startup and DI configuration
- `Components/Pages/Home.razor` - Main map interface

### FaunaFinder.Contracts
**Purpose**: Shared DTOs, parameters, and pagination types

**Dependencies**: None (leaf project)

**Structure**:
```
Contracts/
├── Dtos/
│   ├── Municipalities/
│   │   ├── MunicipalityForListDto.cs
│   │   └── MunicipalityForDetailDto.cs
│   ├── Species/
│   │   ├── SpeciesForListDto.cs
│   │   ├── SpeciesForDetailDto.cs
│   │   └── SpeciesForSearchDto.cs
│   ├── FwsLinks/
│   │   └── FwsLinkDto.cs
│   ├── FwsActions/
│   │   └── FwsActionDto.cs
│   └── NrcsPractices/
│       └── NrcsPracticeDto.cs
├── Parameters/
│   └── SpeciesParameters.cs
└── Pagination/
    └── KeysetPaginationParameters.cs
```

**Key Patterns**:
- All DTOs are `sealed record` types
- DTOs are organized by domain
- Parameters inherit from pagination base types

### FaunaFinder.Database
**Purpose**: Entity models, DbContext, and migrations

**Dependencies**:
- Npgsql.EntityFrameworkCore.PostgreSQL
- EFCore.NamingConventions
- Aspire.Npgsql.EntityFrameworkCore.PostgreSQL

**Structure**:
```
Database/
├── FaunaFinderContext.cs
├── Extensions/
│   └── DatabaseConfigurator.cs
├── Models/
│   ├── Municipalities/
│   │   ├── Municipality.cs
│   │   └── MunicipalitySpecies.cs
│   ├── Species/
│   │   └── Species.cs
│   └── Conservation/
│       ├── FwsAction.cs
│       ├── FwsLink.cs
│       └── NrcsPractice.cs
└── Migrations/
    └── (generated)
```

**Key Patterns**:
- Models contain nested `EntityConfiguration` classes implementing `IEntityTypeConfiguration<T>`
- Static lambdas in all EF configurations for better performance
- Snake_case database naming convention via `UseSnakeCaseNamingConvention()`
- NoTracking by default via `UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)`
- `IDbContextFactory` pattern for repository usage

### FaunaFinder.DataAccess
**Purpose**: Repository implementations with projection-based queries

**Dependencies**:
- FaunaFinder.Database
- FaunaFinder.Contracts

**Structure**:
```
DataAccess/
├── Extensions/
│   └── DataAccessConfigurator.cs
├── Interfaces/
│   ├── IMunicipalityRepository.cs
│   └── ISpeciesRepository.cs
└── Repositories/
    ├── MunicipalityRepository.cs
    └── SpeciesRepository.cs
```

**Key Patterns**:
- **NO `.Include()` anywhere** - Always use `.Select()` projection to DTOs
- Repositories use `IDbContextFactory<FaunaFinderContext>` for scoped context creation
- All queries use `AsNoTracking()` explicitly
- Keyset pagination support

### FaunaFinder.Seeder
**Purpose**: Database migration and seeding worker

**Dependencies**:
- FaunaFinder.Database
- FaunaFinder.ServiceDefaults

**Key Files**:
- `Program.cs` - Worker host setup
- `DatabaseSeederWorker.cs` - Background service that runs migrations and seeds
- `DatabaseSeeder.cs` - Seeding logic

**Behavior**:
- Runs as a worker service before the API starts
- Applies pending migrations
- Seeds initial data if database is empty
- Stops automatically after completion

### FaunaFinder.AppHost
**Purpose**: .NET Aspire orchestration

**Dependencies**:
- FaunaFinder.Api
- FaunaFinder.Seeder

**Orchestration Order**:
1. PostgreSQL database starts
2. Seeder runs (waits for database)
3. API starts (waits for seeder)

### FaunaFinder.ServiceDefaults
**Purpose**: Shared Aspire service configuration

**Provides**:
- OpenTelemetry configuration
- Health checks
- Service discovery
- HTTP resilience

## Key Architectural Decisions

### 1. Projection-Only Data Access
Instead of loading entities with `.Include()` and mapping later:
```csharp
// ❌ DON'T DO THIS
var species = await context.Species
    .Include(s => s.FwsLinks)
        .ThenInclude(fl => fl.NrcsPractice)
    .FirstOrDefaultAsync(s => s.Id == id);
return MapToDto(species);
```

We project directly to DTOs:
```csharp
// ✅ DO THIS
return await context.Species
    .AsNoTracking()
    .Where(s => s.Id == speciesId)
    .Select(static s => new SpeciesForDetailDto(
        s.Id,
        s.CommonName,
        s.ScientificName,
        s.FwsLinks.Select(static fl => new FwsLinkDto(
            fl.Id,
            new NrcsPracticeDto(fl.NrcsPractice.Id, fl.NrcsPractice.Code, fl.NrcsPractice.Name),
            new FwsActionDto(fl.FwsAction.Id, fl.FwsAction.Code, fl.FwsAction.Name),
            fl.Justification
        )).ToList()
    ))
    .FirstOrDefaultAsync(cancellationToken);
```

**Benefits**:
- Single SQL query with JOINs (no N+1)
- Only requested columns are fetched
- No entity tracking overhead
- Cleaner generated SQL

### 2. Nested Entity Configuration
Each model contains its own configuration:
```csharp
public sealed class Species
{
    public required int Id { get; set; }
    public required string CommonName { get; set; }
    // ...

    public sealed class EntityConfiguration : IEntityTypeConfiguration<Species>
    {
        public void Configure(EntityTypeBuilder<Species> builder)
        {
            builder.ToTable("species");
            builder.HasKey(static e => e.Id);
            // ...
        }
    }
}
```

**Benefits**:
- Configuration lives with the model
- Easy to find and maintain
- Static lambdas for performance

### 3. DbContextFactory Pattern
Repositories use factory instead of direct injection:
```csharp
public sealed class SpeciesRepository(
    IDbContextFactory<FaunaFinderContext> contextFactory
) : ISpeciesRepository
{
    public async Task<SpeciesForDetailDto?> GetSpeciesDetailAsync(int speciesId, ...)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        // ...
    }
}
```

**Benefits**:
- Each operation gets a fresh context
- Better for Blazor Server (long-lived connections)
- Avoids context lifetime issues

### 4. Sealed Records for DTOs
All DTOs are immutable sealed records:
```csharp
public sealed record SpeciesForListDto(
    int Id,
    string CommonName,
    string ScientificName,
    List<FwsLinkDto> FwsLinks
);
```

**Benefits**:
- Immutability by default
- Value equality
- Concise syntax
- `sealed` prevents inheritance overhead

### 5. Keyset Pagination
Parameters support cursor-based pagination:
```csharp
public sealed record SpeciesParameters(
    int? Limit = 50,
    int? FromCursor = null,
    string? Search = null,
    int? MunicipalityId = null
) : KeysetPaginationParameters(Limit, FromCursor);
```

**Benefits**:
- Consistent performance regardless of page depth
- Works well with real-time data
- No offset-based issues

## Database Schema

Tables use snake_case naming:
- `municipalities`
- `municipality_species`
- `species`
- `fws_actions`
- `fws_links`
- `nrcs_practices`

## Running the Application

```bash
# From solution root
dotnet run --project src/FaunaFinder.AppHost
```

This starts:
1. PostgreSQL with pgAdmin
2. Database seeder (applies migrations + seeds)
3. Blazor Server API

## Adding Migrations

```bash
dotnet ef migrations add <MigrationName> \
    --project src/FaunaFinder.Database \
    --startup-project src/FaunaFinder.Api
```
