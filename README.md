# FaunaFinder

FaunaFinder is a web application designed to help discover and explore Puerto Rican wildlife and conservation information. It provides an interactive map-based interface where users can click on municipalities to view species found in that area, along with detailed conservation management information linking endangered and protected species to NRCS (Natural Resources Conservation Service) practices and FWS (Fish & Wildlife Service) actions.

## Running Locally

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL container)

### Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/andres-m-rodriguez/FaunaFinder.git
   cd FaunaFinder
   ```

2. Run the application using .NET Aspire:
   ```bash
   dotnet run --project src/FaunaFinder.AppHost
   ```

3. The Aspire dashboard will open automatically in your browser (typically at `http://localhost:17000`), showing all running services.

### What Happens on Startup

- **PostgreSQL** container starts automatically with a persistent data volume
- **Database Seeder** runs migrations and seeds initial data (municipalities, species, conservation links)
- **Blazor Server API** starts once seeding is complete

The application will be available at the URL shown in the Aspire dashboard (typically `http://localhost:5000`).

### Tech Stack

- .NET 10 / C# 13
- Blazor Server with MudBlazor components
- PostgreSQL with Entity Framework Core
- Leaflet.js for interactive mapping
- .NET Aspire for orchestration
