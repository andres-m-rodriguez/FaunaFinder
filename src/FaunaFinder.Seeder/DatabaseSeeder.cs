using FaunaFinder.Database;
using FaunaFinder.Database.Models.Conservation;
using FaunaFinder.Database.Models.Municipalities;
using FaunaFinder.Database.Models.Species;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Seeder;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(FaunaFinderContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Municipalities.AnyAsync(cancellationToken))
            return;

        // === MUNICIPALITIES ===
        // Grouped by region for species distribution
        // GeoJsonId uses full FIPS code format: STATE (72 = Puerto Rico) + COUNTY
        var municipalities = new List<Municipality>
        {
            // Northeast (San Juan Metro)
            new() { Id = 0, Name = "San Juan", GeoJsonId = "72127" },
            new() { Id = 0, Name = "Bayamon", GeoJsonId = "72021" },
            new() { Id = 0, Name = "Carolina", GeoJsonId = "72031" },
            new() { Id = 0, Name = "Guaynabo", GeoJsonId = "72061" },

            // East
            new() { Id = 0, Name = "Fajardo", GeoJsonId = "72053" },
            new() { Id = 0, Name = "Humacao", GeoJsonId = "72069" },
            new() { Id = 0, Name = "Caguas", GeoJsonId = "72025" },

            // South
            new() { Id = 0, Name = "Ponce", GeoJsonId = "72113" },
            new() { Id = 0, Name = "Guayama", GeoJsonId = "72057" },
            new() { Id = 0, Name = "Yauco", GeoJsonId = "72153" },

            // West
            new() { Id = 0, Name = "Mayaguez", GeoJsonId = "72097" },
            new() { Id = 0, Name = "Cabo Rojo", GeoJsonId = "72023" },
            new() { Id = 0, Name = "Rincon", GeoJsonId = "72117" },
            new() { Id = 0, Name = "Aguadilla", GeoJsonId = "72003" },

            // North
            new() { Id = 0, Name = "Arecibo", GeoJsonId = "72013" },
            new() { Id = 0, Name = "Vega Baja", GeoJsonId = "72145" },
            new() { Id = 0, Name = "Manati", GeoJsonId = "72091" },
            new() { Id = 0, Name = "Isabela", GeoJsonId = "72071" },

            // Islands
            new() { Id = 0, Name = "Vieques", GeoJsonId = "72147" },
            new() { Id = 0, Name = "Culebra", GeoJsonId = "72049" },
        };
        context.Municipalities.AddRange(municipalities);
        await context.SaveChangesAsync(cancellationToken);

        // === NRCS PRACTICES ===
        var practices = new List<NrcsPractice>
        {
            new() { Id = 0, Code = "314", Name = "Brush Management" },
            new() { Id = 0, Code = "338", Name = "Prescribed Burning" },
            new() { Id = 0, Code = "342", Name = "Critical Area Planting" },
            new() { Id = 0, Code = "391", Name = "Riparian Forest Buffer" },
            new() { Id = 0, Code = "420", Name = "Wildlife Habitat Planting" },
            new() { Id = 0, Code = "472", Name = "Access Control" },
            new() { Id = 0, Code = "528", Name = "Prescribed Grazing" },
            new() { Id = 0, Code = "580", Name = "Streambank and Shoreline Protection" },
            new() { Id = 0, Code = "612", Name = "Tree/Shrub Establishment" },
            new() { Id = 0, Code = "643", Name = "Restoration of Rare or Declining Natural Communities" },
            new() { Id = 0, Code = "644", Name = "Wetland Wildlife Habitat Management" },
            new() { Id = 0, Code = "645", Name = "Upland Wildlife Habitat Management" },
            new() { Id = 0, Code = "647", Name = "Early Successional Habitat Development" },
            new() { Id = 0, Code = "649", Name = "Structures for Wildlife" },
            new() { Id = 0, Code = "666", Name = "Forest Stand Improvement" }
        };
        context.NrcsPractices.AddRange(practices);
        await context.SaveChangesAsync(cancellationToken);

        // === FWS ACTIONS ===
        var actions = new List<FwsAction>
        {
            new() { Id = 0, Code = "1.1", Name = "Protect existing habitat" },
            new() { Id = 0, Code = "1.2", Name = "Restore degraded habitat" },
            new() { Id = 0, Code = "2.1", Name = "Control invasive species" },
            new() { Id = 0, Code = "2.2", Name = "Predator management" },
            new() { Id = 0, Code = "2.3", Name = "Reduce human disturbance" },
            new() { Id = 0, Code = "3.1", Name = "Population monitoring" },
            new() { Id = 0, Code = "3.2", Name = "Species biology research" },
            new() { Id = 0, Code = "4.1", Name = "Captive breeding program" },
            new() { Id = 0, Code = "4.2", Name = "Translocation efforts" },
            new() { Id = 0, Code = "5.1", Name = "Public education and outreach" },
            new() { Id = 0, Code = "6.1", Name = "Enforce protection regulations" },
            new() { Id = 0, Code = "7.1", Name = "Climate adaptation planning" }
        };
        context.FwsActions.AddRange(actions);
        await context.SaveChangesAsync(cancellationToken);

        // === SPECIES WITH GEOGRAPHIC DISTRIBUTIONS ===
        // Each species has specific municipalities where it lives
        // Tuple: (English Common Name, Scientific Name, Spanish Common Name, Regions)
        var speciesData = new List<(string Common, string Scientific, string Spanish, string[] Regions)>
        {
            // Puerto Rican Parrot - Lives in El Yunque (East) AND Rio Abajo (North) - TWO SEPARATE AREAS
            ("Puerto Rican Parrot", "Amazona vittata", "Cotorra Puertorriqueña", ["Fajardo", "Humacao", "Caguas", "Arecibo", "Vega Baja", "Manati"]),

            // Puerto Rican Boa - Found across the island but mainly forests
            ("Puerto Rican Boa", "Chilabothrus inornatus", "Boa Puertorriqueña", ["Caguas", "Humacao", "Fajardo", "Arecibo", "Mayaguez", "Ponce"]),

            // Coqui Llanero - ONLY in Toa Baja wetlands area (very restricted) - modeled as Vega Baja/Manati
            ("Plains Coqui", "Eleutherodactylus juanariveroi", "Coquí Llanero", ["Vega Baja", "Manati"]),

            // Puerto Rican Crested Toad - South coast AND North coast - TWO SEPARATE AREAS
            ("Puerto Rican Crested Toad", "Peltophryne lemur", "Sapo Concho Puertorriqueño", ["Guayama", "Ponce", "Isabela", "Aguadilla"]),

            // Leatherback Sea Turtle - Nests on beaches, east and west coasts
            ("Leatherback Sea Turtle", "Dermochelys coriacea", "Tortuga Laúd", ["Fajardo", "Humacao", "Rincon", "Aguadilla", "Vieques", "Culebra"]),

            // Hawksbill Sea Turtle - Beaches and coral reefs around the island
            ("Hawksbill Sea Turtle", "Eretmochelys imbricata", "Tortuga Carey", ["Fajardo", "Culebra", "Vieques", "Rincon", "Cabo Rojo", "Guayama"]),

            // West Indian Manatee - Coastal waters, bays
            ("West Indian Manatee", "Trichechus manatus", "Manatí Antillano", ["San Juan", "Carolina", "Fajardo", "Guayama", "Ponce", "Mayaguez", "Vieques"]),

            // Yellow-shouldered Blackbird - Southwest coast AND Mona Island (modeled as Cabo Rojo/Mayaguez)
            ("Yellow-shouldered Blackbird", "Agelaius xanthomus", "Mariquita de Puerto Rico", ["Cabo Rojo", "Mayaguez", "Yauco", "Ponce"]),

            // Puerto Rican Nightjar - Dry forests in southwest
            ("Puerto Rican Nightjar", "Antrostomus noctitherus", "Guabairo Puertorriqueño", ["Cabo Rojo", "Yauco", "Ponce", "Guayama"]),

            // Ponce Cave Bat - Cave-dwelling bat
            ("Ponce Cave Bat", "Mormoops blainvillei", "Murciélago de Cueva de Ponce", ["Ponce", "Yauco", "Guayama"]),

            // Puerto Rican Racer - widespread but including south coast
            ("Puerto Rican Racer", "Borikenophis portoricensis", "Culebra Corredora Puertorriqueña", ["Ponce", "Guayama", "Caguas", "Mayaguez", "Arecibo"]),

            // Elfin-woods Warbler - High elevation forests (El Yunque and Maricao)
            ("Elfin-woods Warbler", "Setophaga angelae", "Reinita de Bosque Enano", ["Fajardo", "Humacao", "Caguas", "Mayaguez", "Yauco"]),

            // Puerto Rican Sharp-shinned Hawk - Mountain forests
            ("Puerto Rican Sharp-shinned Hawk", "Accipiter striatus venator", "Falcón de Sierra Puertorriqueño", ["Caguas", "Humacao", "Arecibo", "Mayaguez"]),

            // Green Sea Turtle - Coastal areas
            ("Green Sea Turtle", "Chelonia mydas", "Tortuga Verde", ["Culebra", "Vieques", "Fajardo", "Humacao", "Rincon"]),
        };

        var savedMunicipalities = await context.Municipalities.ToDictionaryAsync(m => m.Name, m => m, cancellationToken);
        var savedPractices = await context.NrcsPractices.ToListAsync(cancellationToken);
        var savedActions = await context.FwsActions.ToListAsync(cancellationToken);

        foreach (var (commonName, scientificName, spanishName, regions) in speciesData)
        {
            var species = new Species
            {
                Id = 0,
                CommonName = commonName,
                ScientificName = scientificName
            };
            context.Species.Add(species);
            await context.SaveChangesAsync(cancellationToken);

            // Add Spanish translation for the species
            context.SpeciesTranslations.Add(new SpeciesTranslation
            {
                Id = 0,
                SpeciesId = species.Id,
                LanguageCode = "es",
                CommonName = spanishName
            });
            await context.SaveChangesAsync(cancellationToken);

            // Link species to municipalities
            foreach (var region in regions)
            {
                if (savedMunicipalities.TryGetValue(region, out var municipality))
                {
                    context.MunicipalitySpecies.Add(new MunicipalitySpecies
                    {
                        Id = 0,
                        MunicipalityId = municipality.Id,
                        SpeciesId = species.Id
                    });
                }
            }
            await context.SaveChangesAsync(cancellationToken);

            // Add conservation links based on species type
            var links = GetConservationLinks(commonName, savedPractices, savedActions);
            foreach (var (practice, action, justification) in links)
            {
                context.FwsLinks.Add(new FwsLink
                {
                    Id = 0,
                    SpeciesId = species.Id,
                    NrcsPracticeId = practice.Id,
                    FwsActionId = action.Id,
                    Justification = justification
                });
            }
            await context.SaveChangesAsync(cancellationToken);

            // Add species locations for some species
            var locations = GetSpeciesLocations(commonName);
            foreach (var (lat, lng, radius, description) in locations)
            {
                context.SpeciesLocations.Add(new SpeciesLocation
                {
                    Id = 0,
                    SpeciesId = species.Id,
                    Latitude = lat,
                    Longitude = lng,
                    RadiusMeters = radius,
                    Description = description
                });
            }
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static List<(double Lat, double Lng, double Radius, string Description)> GetSpeciesLocations(string speciesName)
    {
        return speciesName switch
        {
            // Puerto Rican Parrot - El Yunque and Rio Abajo populations
            "Puerto Rican Parrot" =>
            [
                (18.2959, -65.7872, 5000, "El Yunque National Forest - Primary wild population"),
                (18.3500, -66.7200, 4000, "Rio Abajo State Forest - Reintroduced population"),
            ],

            // Coqui Llanero - Very restricted to coastal wetlands
            "Coqui Llanero" =>
            [
                (18.4450, -66.2500, 1500, "Sabana Seca wetlands - Only known population"),
            ],

            // Puerto Rican Crested Toad - Guánica and northern populations
            "Puerto Rican Crested Toad" =>
            [
                (17.9700, -66.8600, 3000, "Guánica State Forest - Southern population"),
                (18.4800, -67.0800, 2500, "Isabela region - Northern population"),
            ],

            // Leatherback Sea Turtle - Major nesting beaches
            "Leatherback Sea Turtle" =>
            [
                (18.3100, -65.6300, 2000, "Fajardo beaches - Nesting area"),
                (18.1500, -65.5000, 2500, "Humacao coast - Nesting area"),
                (18.3400, -67.2500, 2000, "Rincón beaches - Nesting area"),
            ],

            // West Indian Manatee - Coastal areas
            "West Indian Manatee" =>
            [
                (18.4200, -66.0500, 4000, "San Juan Bay - Regular sightings"),
                (18.3300, -65.6500, 3500, "Fajardo coast - Seagrass foraging area"),
                (17.9600, -66.6100, 3000, "Guayanilla Bay - Warm water refuge"),
            ],

            // Yellow-shouldered Blackbird - Southwest coast
            "Yellow-shouldered Blackbird" =>
            [
                (17.9500, -67.1500, 2500, "Cabo Rojo mangroves - Breeding colony"),
                (17.9700, -66.8500, 2000, "Roosevelt Roads area - Population cluster"),
            ],

            // Puerto Rican Nightjar - Dry forests
            "Puerto Rican Nightjar" =>
            [
                (17.9800, -66.8800, 4000, "Guánica dry forest - Core habitat"),
                (18.0200, -67.0500, 3000, "Cabo Rojo forests - Secondary habitat"),
            ],

            _ => []
        };
    }

    private static List<(NrcsPractice, FwsAction, string)> GetConservationLinks(
        string speciesName,
        List<NrcsPractice> practices,
        List<FwsAction> actions)
    {
        var links = new List<(NrcsPractice, FwsAction, string)>();

        // Helper to find practice/action by code
        NrcsPractice P(string code) => practices.First(p => p.Code == code);
        FwsAction A(string code) => actions.First(a => a.Code == code);

        switch (speciesName)
        {
            case "Puerto Rican Parrot":
                links.Add((P("612"), A("1.2"), "Tree establishment provides nesting cavities and food sources for parrots."));
                links.Add((P("666"), A("1.1"), "Forest stand improvement maintains mature forest habitat essential for nesting."));
                links.Add((P("645"), A("2.1"), "Upland habitat management helps control invasive species that compete with native food plants."));
                links.Add((P("649"), A("4.1"), "Artificial nest structures support captive breeding and wild population recovery."));
                break;

            case "Puerto Rican Boa":
                links.Add((P("472"), A("2.3"), "Access control reduces human disturbance and illegal collection of boas."));
                links.Add((P("643"), A("1.2"), "Restoring native forest communities provides essential boa habitat."));
                links.Add((P("666"), A("1.1"), "Maintaining forest structure protects boa hunting and denning areas."));
                break;

            case "Coqui Llanero":
                links.Add((P("644"), A("1.1"), "Wetland management protects the only known habitat for this critically endangered frog."));
                links.Add((P("580"), A("1.2"), "Shoreline protection maintains wetland hydrology critical for breeding."));
                links.Add((P("472"), A("6.1"), "Access control prevents habitat degradation from unauthorized activities."));
                break;

            case "Puerto Rican Crested Toad":
                links.Add((P("644"), A("1.2"), "Wetland restoration creates breeding ponds for toad reproduction."));
                links.Add((P("643"), A("4.2"), "Habitat restoration supports translocation efforts to establish new populations."));
                links.Add((P("314"), A("2.1"), "Brush management controls invasive plants that degrade breeding habitat."));
                break;

            case "Leatherback Sea Turtle":
            case "Hawksbill Sea Turtle":
            case "Green Sea Turtle":
                links.Add((P("580"), A("1.1"), "Beach and shoreline protection preserves nesting habitat."));
                links.Add((P("472"), A("2.3"), "Access control during nesting season reduces disturbance to nesting females."));
                links.Add((P("342"), A("1.2"), "Critical area planting stabilizes dunes and maintains natural beach profiles."));
                break;

            case "West Indian Manatee":
                links.Add((P("391"), A("1.1"), "Riparian buffers protect seagrass beds that manatees depend on for food."));
                links.Add((P("580"), A("1.2"), "Shoreline protection reduces sedimentation that degrades seagrass habitat."));
                links.Add((P("644"), A("3.1"), "Wetland management areas serve as monitoring sites for population tracking."));
                break;

            case "Yellow-shouldered Blackbird":
                links.Add((P("643"), A("1.2"), "Restoring native mangrove and dry forest provides nesting and foraging habitat."));
                links.Add((P("645"), A("2.2"), "Upland management includes predator control to protect nests from rats and mongooses."));
                links.Add((P("649"), A("4.1"), "Nest structures support breeding success in areas with limited natural cavities."));
                break;

            case "Puerto Rican Nightjar":
                links.Add((P("645"), A("1.1"), "Upland dry forest management maintains ground-nesting habitat."));
                links.Add((P("314"), A("2.1"), "Brush management controls invasive species while maintaining open understory."));
                links.Add((P("472"), A("2.3"), "Access control protects ground nests from human disturbance."));
                break;

            case "Elfin-woods Warbler":
                links.Add((P("666"), A("1.1"), "Maintaining elfin woodland structure is critical for this habitat specialist."));
                links.Add((P("643"), A("7.1"), "Restoration efforts include climate-resilient plantings at higher elevations."));
                links.Add((P("612"), A("1.2"), "Native tree establishment expands available warbler habitat."));
                break;

            case "Puerto Rican Sharp-shinned Hawk":
                links.Add((P("666"), A("1.1"), "Forest management maintains hunting territory and nesting sites."));
                links.Add((P("645"), A("3.1"), "Habitat management areas facilitate population monitoring."));
                links.Add((P("472"), A("6.1"), "Access control near nest sites enforces protection during breeding season."));
                break;

            case "Ponce Cave Bat":
                links.Add((P("472"), A("1.1"), "Access control protects cave roosting sites from disturbance."));
                links.Add((P("643"), A("1.2"), "Restoring native forest near caves provides foraging habitat."));
                links.Add((P("645"), A("3.1"), "Upland management supports population monitoring efforts."));
                break;

            case "Puerto Rican Racer":
                links.Add((P("643"), A("1.2"), "Restoring native habitat provides hunting grounds for this snake."));
                links.Add((P("314"), A("2.1"), "Brush management controls invasive species while maintaining racer habitat."));
                links.Add((P("472"), A("5.1"), "Access control areas serve as sites for public education about native snakes."));
                break;
        }

        return links;
    }
}
