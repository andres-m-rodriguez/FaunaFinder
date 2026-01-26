using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FaunaFinder.Contracts.Localization;
using FaunaFinder.Database;
using FaunaFinder.Database.Models.Conservation;
using FaunaFinder.Database.Models.Municipalities;
using FaunaFinder.Database.Models.Species;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Converters;

namespace FaunaFinder.Seeder;

public static class DatabaseSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task SeedAsync(FaunaFinderContext context, CancellationToken cancellationToken = default)
    {
        // Seed municipalities first (from GeoJSON - all 78 with boundaries)
        await SeedMunicipalitiesAsync(context, cancellationToken);

        // Seed NRCS practices
        await SeedNrcsPracticesAsync(context, cancellationToken);

        // Seed FWS actions
        await SeedFwsActionsAsync(context, cancellationToken);

        // Seed species (with images, locations, and municipality links)
        await SeedSpeciesAsync(context, cancellationToken);

        // Seed FWS links (species-practice-action relationships)
        await SeedFwsLinksAsync(context, cancellationToken);
    }

    private static async Task SeedMunicipalitiesAsync(FaunaFinderContext context, CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "FaunaFinder.Seeder.Data.pr-municipios.geojson";

        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        }

        var geoJsonOptions = new JsonSerializerOptions();
        geoJsonOptions.Converters.Add(new GeoJsonConverterFactory());

        var featureCollection = await JsonSerializer.DeserializeAsync<FeatureCollection>(stream, geoJsonOptions, cancellationToken);
        if (featureCollection is null)
        {
            throw new InvalidOperationException("Failed to deserialize GeoJSON feature collection.");
        }

        // Get existing municipalities by name (unique constraint is on name)
        var existingMunicipalities = await context.Municipalities
            .ToDictionaryAsync(m => m.Name, m => m, cancellationToken);

        // Track municipalities we're adding in this batch by name
        var pendingMunicipalities = new Dictionary<string, Municipality>();

        foreach (var feature in featureCollection)
        {
            var state = feature.Attributes["STATE"]?.ToString() ?? "";
            var county = feature.Attributes["COUNTY"]?.ToString() ?? "";
            var name = feature.Attributes["NAME"]?.ToString() ?? "";
            var geoJsonId = state + county;

            // Skip if not Puerto Rico or empty name
            if (state != "72" || string.IsNullOrEmpty(name))
                continue;

            if (existingMunicipalities.TryGetValue(name, out var existing))
            {
                // Update boundary if needed
                if (existing.Boundary is null)
                {
                    existing.Boundary = feature.Geometry;
                }
            }
            else if (pendingMunicipalities.TryGetValue(name, out var pending))
            {
                // Already adding this municipality, skip duplicate
                continue;
            }
            else
            {
                // Create new municipality with boundary
                var municipality = new Municipality
                {
                    Id = 0,
                    Name = name,
                    GeoJsonId = geoJsonId,
                    Boundary = feature.Geometry
                };
                context.Municipalities.Add(municipality);
                pendingMunicipalities[name] = municipality;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedNrcsPracticesAsync(FaunaFinderContext context, CancellationToken cancellationToken)
    {
        var practices = await LoadJsonResourceAsync<List<NrcsPracticeDto>>("nrcs_practices.json", cancellationToken);
        if (practices is null) return;

        var existing = await context.NrcsPractices
            .ToDictionaryAsync(p => p.Code, p => p, cancellationToken);

        foreach (var dto in practices)
        {
            if (!existing.ContainsKey(dto.Code))
            {
                context.NrcsPractices.Add(new NrcsPractice
                {
                    Id = 0,
                    Code = dto.Code,
                    Name = dto.Name
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedFwsActionsAsync(FaunaFinderContext context, CancellationToken cancellationToken)
    {
        var actions = await LoadJsonResourceAsync<List<FwsActionDto>>("fws_actions.json", cancellationToken);
        if (actions is null) return;

        var existing = await context.FwsActions
            .ToDictionaryAsync(a => a.Code, a => a, cancellationToken);

        foreach (var dto in actions)
        {
            if (!existing.ContainsKey(dto.Code))
            {
                context.FwsActions.Add(new FwsAction
                {
                    Id = 0,
                    Code = dto.Code,
                    Name = dto.Name
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedSpeciesAsync(FaunaFinderContext context, CancellationToken cancellationToken)
    {
        var speciesList = await LoadJsonResourceAsync<List<SpeciesDto>>("species.json", cancellationToken);
        if (speciesList is null) return;

        var existingSpecies = await context.Species
            .ToDictionaryAsync(s => s.ScientificName, s => s, cancellationToken);

        var municipalities = await context.Municipalities
            .ToDictionaryAsync(m => m.GeoJsonId, m => m, cancellationToken);

        foreach (var dto in speciesList)
        {
            Species species;

            if (existingSpecies.TryGetValue(dto.ScientificName, out var existing))
            {
                species = existing;

                // Update image if we have one and the existing doesn't
                if (species.ProfileImageData is null && !string.IsNullOrEmpty(dto.ImageBase64))
                {
                    species.ProfileImageData = Convert.FromBase64String(dto.ImageBase64);
                    species.ProfileImageContentType = dto.ImageContentType;
                }
            }
            else
            {
                // Create new species
                species = new Species
                {
                    Id = 0,
                    CommonName =
                    [
                        new LocaleValue(SupportedLocales.English, dto.CommonNameEn),
                        new LocaleValue(SupportedLocales.Spanish, dto.CommonNameEs)
                    ],
                    ScientificName = dto.ScientificName,
                    ProfileImageData = string.IsNullOrEmpty(dto.ImageBase64)
                        ? null
                        : Convert.FromBase64String(dto.ImageBase64),
                    ProfileImageContentType = dto.ImageContentType
                };
                context.Species.Add(species);
                await context.SaveChangesAsync(cancellationToken);
                existingSpecies[dto.ScientificName] = species;
            }

            // Link to municipalities
            if (dto.MunicipalityGeoJsonIds is { Count: > 0 })
            {
                var existingLinks = await context.MunicipalitySpecies
                    .Where(ms => ms.SpeciesId == species.Id)
                    .Select(ms => ms.MunicipalityId)
                    .ToHashSetAsync(cancellationToken);

                foreach (var geoJsonId in dto.MunicipalityGeoJsonIds)
                {
                    if (municipalities.TryGetValue(geoJsonId, out var municipality) &&
                        !existingLinks.Contains(municipality.Id))
                    {
                        context.MunicipalitySpecies.Add(new MunicipalitySpecies
                        {
                            Id = 0,
                            MunicipalityId = municipality.Id,
                            SpeciesId = species.Id
                        });
                    }
                }
            }

            // Add locations
            if (dto.Locations is { Count: > 0 })
            {
                var hasLocations = await context.SpeciesLocations
                    .AnyAsync(sl => sl.SpeciesId == species.Id, cancellationToken);

                if (!hasLocations)
                {
                    foreach (var loc in dto.Locations)
                    {
                        context.SpeciesLocations.Add(new SpeciesLocation
                        {
                            Id = 0,
                            SpeciesId = species.Id,
                            Latitude = loc.Latitude,
                            Longitude = loc.Longitude,
                            RadiusMeters = loc.RadiusMeters,
                            Description = loc.Description
                        });
                    }
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedFwsLinksAsync(FaunaFinderContext context, CancellationToken cancellationToken)
    {
        var links = await LoadJsonResourceAsync<List<FwsLinkDto>>("fws_links.json", cancellationToken);
        if (links is null) return;

        var speciesMap = await context.Species
            .ToDictionaryAsync(s => s.ScientificName, s => s.Id, cancellationToken);

        var practiceMap = await context.NrcsPractices
            .ToDictionaryAsync(p => p.Code, p => p.Id, cancellationToken);

        var actionMap = await context.FwsActions
            .ToDictionaryAsync(a => a.Code, a => a.Id, cancellationToken);

        var existingLinks = await context.FwsLinks
            .Select(l => new { l.SpeciesId, l.NrcsPracticeId, l.FwsActionId })
            .ToListAsync(cancellationToken);

        var existingLinkSet = existingLinks
            .Select(l => (l.SpeciesId, l.NrcsPracticeId, l.FwsActionId))
            .ToHashSet();

        var batchCount = 0;
        foreach (var dto in links)
        {
            if (!speciesMap.TryGetValue(dto.SpeciesScientificName, out var speciesId))
                continue;

            if (!practiceMap.TryGetValue(dto.NrcsPracticeCode, out var practiceId))
                continue;

            if (!actionMap.TryGetValue(dto.FwsActionCode, out var actionId))
                continue;

            var linkKey = (speciesId, practiceId, actionId);
            if (existingLinkSet.Contains(linkKey))
                continue;

            context.FwsLinks.Add(new FwsLink
            {
                Id = 0,
                SpeciesId = speciesId,
                NrcsPracticeId = practiceId,
                FwsActionId = actionId,
                Justification = dto.Justification
            });

            existingLinkSet.Add(linkKey);
            batchCount++;

            // Save in batches to avoid memory issues
            if (batchCount >= 100)
            {
                await context.SaveChangesAsync(cancellationToken);
                batchCount = 0;
            }
        }

        if (batchCount > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task<T?> LoadJsonResourceAsync<T>(string resourceFileName, CancellationToken cancellationToken)
        where T : class
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"FaunaFinder.Seeder.Data.{resourceFileName}";

        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            Console.WriteLine($"Warning: Embedded resource '{resourceName}' not found. Skipping.");
            return null;
        }

        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
    }

    #region DTOs for JSON deserialization

    private sealed class NrcsPracticeDto
    {
        [JsonPropertyName("code")]
        public required string Code { get; init; }

        [JsonPropertyName("name")]
        public required string Name { get; init; }
    }

    private sealed class FwsActionDto
    {
        [JsonPropertyName("code")]
        public required string Code { get; init; }

        [JsonPropertyName("name")]
        public required string Name { get; init; }
    }

    private sealed class SpeciesDto
    {
        [JsonPropertyName("scientificName")]
        public required string ScientificName { get; init; }

        [JsonPropertyName("commonNameEn")]
        public required string CommonNameEn { get; init; }

        [JsonPropertyName("commonNameEs")]
        public required string CommonNameEs { get; init; }

        [JsonPropertyName("municipalityGeoJsonIds")]
        public List<string>? MunicipalityGeoJsonIds { get; init; }

        [JsonPropertyName("locations")]
        public List<LocationDto>? Locations { get; init; }

        [JsonPropertyName("imageBase64")]
        public string? ImageBase64 { get; init; }

        [JsonPropertyName("imageContentType")]
        public string? ImageContentType { get; init; }
    }

    private sealed class LocationDto
    {
        [JsonPropertyName("latitude")]
        public required double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public required double Longitude { get; init; }

        [JsonPropertyName("radiusMeters")]
        public required double RadiusMeters { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }
    }

    private sealed class FwsLinkDto
    {
        [JsonPropertyName("speciesScientificName")]
        public required string SpeciesScientificName { get; init; }

        [JsonPropertyName("nrcsPracticeCode")]
        public required string NrcsPracticeCode { get; init; }

        [JsonPropertyName("fwsActionCode")]
        public required string FwsActionCode { get; init; }

        [JsonPropertyName("justification")]
        public string? Justification { get; init; }
    }

    #endregion
}
