# FaunaFinder Custom Skills

## /feature

Scaffold a new feature with all required projects following the vertical slice architecture.

### Usage
```
/feature {FeatureName}
```

### Creates
```
src/Features/{Feature}/
├── FaunaFinder.{Feature}.Api/
│   ├── {Feature}Endpoints.cs
│   ├── DependencyInjection.cs
│   └── FaunaFinder.{Feature}.Api.csproj
├── FaunaFinder.{Feature}.Application/
│   ├── Services/
│   ├── DependencyInjection.cs
│   └── FaunaFinder.{Feature}.Application.csproj
├── FaunaFinder.{Feature}.Application.Client/
│   ├── I{Feature}Client.cs
│   ├── {Feature}Client.cs
│   ├── DependencyInjection.cs
│   └── FaunaFinder.{Feature}.Application.Client.csproj
├── FaunaFinder.{Feature}.Contracts/
│   ├── Requests/
│   ├── Responses/
│   └── FaunaFinder.{Feature}.Contracts.csproj
├── FaunaFinder.{Feature}.Database/
│   ├── Models/
│   ├── {Feature}DbContext.cs
│   └── FaunaFinder.{Feature}.Database.csproj
└── FaunaFinder.{Feature}.DataAccess/
    ├── Repositories/
    ├── DependencyInjection.cs
    └── FaunaFinder.{Feature}.DataAccess.csproj
```

### Steps
1. Create all project directories and .csproj files
2. Add projects to solution with `dotnet sln add`
3. Set up project references following dependency flow
4. Create placeholder files with correct namespaces
5. Register feature in FaunaFinder.Api Program.cs
6. Register client in FaunaFinder.Client Program.cs
7. Add database to AppHost

---

## /endpoint

Add a new API endpoint to an existing feature.

### Usage
```
/endpoint {Feature} {EndpointName} {HttpMethod}
```

### Example
```
/endpoint Wildlife GetSpeciesById GET
```

### Steps
1. Add endpoint method to `{Feature}Endpoints.cs`
2. Create request/response DTOs in Contracts if needed
3. Add service method in Application layer
4. Add repository method in DataAccess if needed
5. Wire up dependencies

---

## /migration

Create a new EF Core migration for a feature.

### Usage
```
/migration {Feature} {MigrationName}
```

### Example
```
/migration Wildlife AddSightingAuditFields
```

### Command
```bash
dotnet ef migrations add {MigrationName} \
  --project src/Features/{Feature}/FaunaFinder.{Feature}.Database \
  --startup-project src/FaunaFinder.Api
```

---

## /component

Create a new Blazor component.

### Usage
```
/component {Feature} {ComponentName}
```

### Example
```
/component Wildlife SightingCard
```

### Creates
```
src/FaunaFinder.Client/Pages/{Feature}/{ComponentName}.razor
```

### Template
- Includes `@inject IAppLocalizer L`
- Uses MudBlazor components
- Follows existing component patterns

---

## /page

Create a new Blazor page with route.

### Usage
```
/page {Feature} {PageName} {Route}
```

### Example
```
/page Wildlife SightingDetails /sightings/{id:int}
```

### Creates
```
src/FaunaFinder.Client/Pages/{Feature}/{PageName}.razor
```

### Template
- Includes `@page "{Route}"`
- Includes `@attribute [Authorize]` if authenticated
- Injects required services
- Uses MudBlazor layout components

---

## /repository

Add a new repository to a feature's DataAccess layer.

### Usage
```
/repository {Feature} {EntityName}
```

### Example
```
/repository Wildlife Species
```

### Creates
- `I{Entity}Repository.cs` - Interface
- `{Entity}Repository.cs` - Implementation

### Rules
- Returns DTOs from Contracts, not entities
- Registered in DependencyInjection.cs

---

## /service

Add a new service to a feature's Application layer.

### Usage
```
/service {Feature} {ServiceName}
```

### Example
```
/service Wildlife SightingReview
```

### Creates
- `I{ServiceName}Service.cs` - Interface
- `{ServiceName}Service.cs` - Implementation

### Rules
- Uses repositories from DataAccess
- Returns DTOs from Contracts
- Registered in DependencyInjection.cs

---

## /dto

Create a new DTO in Contracts.

### Usage
```
/dto {Feature} {DtoName} {Type}
```

Where `{Type}` is: `request`, `response`, or `shared`

### Example
```
/dto Wildlife CreateSighting request
```

### Creates
- For `request`: `Requests/{DtoName}Request.cs` + `{DtoName}RequestValidator.cs`
- For `response`: `Responses/{DtoName}Response.cs`
- For `shared`: `{DtoName}.cs`

---

## /translate

Add a new translation key to both API and Client.

### Usage
```
/translate {Key} {English} {Spanish}
```

### Example
```
/translate Sighting_Submit "Submit Report" "Enviar Reporte"
```

### Updates
- `src/FaunaFinder.Api/Services/Localization/Translations.cs`
- `src/FaunaFinder.Client/Services/Localization/Translations.cs`
