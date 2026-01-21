using FaunaFinder.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(FaunaFinderDbContext context)
    {
        if (await context.Municipalities.AnyAsync())
            return;

        // === MUNICIPALITIES ===
        // Grouped by region for species distribution
        var municipalities = new List<Municipality>
        {
            // Northeast (San Juan Metro)
            new() { Name = "San Juan", GeoJsonId = "72127" },
            new() { Name = "Bayamon", GeoJsonId = "72021" },
            new() { Name = "Carolina", GeoJsonId = "72031" },
            new() { Name = "Guaynabo", GeoJsonId = "72061" },

            // East
            new() { Name = "Fajardo", GeoJsonId = "72053" },
            new() { Name = "Humacao", GeoJsonId = "72069" },
            new() { Name = "Caguas", GeoJsonId = "72025" },

            // South
            new() { Name = "Ponce", GeoJsonId = "72113" },
            new() { Name = "Guayama", GeoJsonId = "72057" },
            new() { Name = "Yauco", GeoJsonId = "72153" },

            // West
            new() { Name = "Mayaguez", GeoJsonId = "72097" },
            new() { Name = "Cabo Rojo", GeoJsonId = "72023" },
            new() { Name = "Rincon", GeoJsonId = "72117" },
            new() { Name = "Aguadilla", GeoJsonId = "72003" },

            // North
            new() { Name = "Arecibo", GeoJsonId = "72013" },
            new() { Name = "Vega Baja", GeoJsonId = "72145" },
            new() { Name = "Manati", GeoJsonId = "72091" },
            new() { Name = "Isabela", GeoJsonId = "72071" },

            // Islands
            new() { Name = "Vieques", GeoJsonId = "72147" },
            new() { Name = "Culebra", GeoJsonId = "72049" },
        };
        context.Municipalities.AddRange(municipalities);
        await context.SaveChangesAsync();

        // === NRCS PRACTICES ===
        var practices = new List<NrcsPractice>
        {
            new() { Code = "314", Name = "Brush Management" },
            new() { Code = "338", Name = "Prescribed Burning" },
            new() { Code = "342", Name = "Critical Area Planting" },
            new() { Code = "391", Name = "Riparian Forest Buffer" },
            new() { Code = "420", Name = "Wildlife Habitat Planting" },
            new() { Code = "472", Name = "Access Control" },
            new() { Code = "528", Name = "Prescribed Grazing" },
            new() { Code = "580", Name = "Streambank and Shoreline Protection" },
            new() { Code = "612", Name = "Tree/Shrub Establishment" },
            new() { Code = "643", Name = "Restoration of Rare or Declining Natural Communities" },
            new() { Code = "644", Name = "Wetland Wildlife Habitat Management" },
            new() { Code = "645", Name = "Upland Wildlife Habitat Management" },
            new() { Code = "647", Name = "Early Successional Habitat Development" },
            new() { Code = "649", Name = "Structures for Wildlife" },
            new() { Code = "666", Name = "Forest Stand Improvement" }
        };
        context.NrcsPractices.AddRange(practices);
        await context.SaveChangesAsync();

        // === FWS ACTIONS ===
        var actions = new List<FwsAction>
        {
            new() { Code = "1.1", Name = "Protect existing habitat" },
            new() { Code = "1.2", Name = "Restore degraded habitat" },
            new() { Code = "2.1", Name = "Control invasive species" },
            new() { Code = "2.2", Name = "Predator management" },
            new() { Code = "2.3", Name = "Reduce human disturbance" },
            new() { Code = "3.1", Name = "Population monitoring" },
            new() { Code = "3.2", Name = "Species biology research" },
            new() { Code = "4.1", Name = "Captive breeding program" },
            new() { Code = "4.2", Name = "Translocation efforts" },
            new() { Code = "5.1", Name = "Public education and outreach" },
            new() { Code = "6.1", Name = "Enforce protection regulations" },
            new() { Code = "7.1", Name = "Climate adaptation planning" }
        };
        context.FwsActions.AddRange(actions);
        await context.SaveChangesAsync();

        // === SPECIES WITH GEOGRAPHIC DISTRIBUTIONS ===
        // Each species has specific municipalities where it lives
        var speciesData = new List<(string Common, string Scientific, string[] Regions)>
        {
            // Puerto Rican Parrot - Lives in El Yunque (East) AND Rio Abajo (North) - TWO SEPARATE AREAS
            ("Puerto Rican Parrot", "Amazona vittata", new[] { "Fajardo", "Humacao", "Caguas", "Arecibo", "Vega Baja", "Manati" }),

            // Puerto Rican Boa - Found across the island but mainly forests
            ("Puerto Rican Boa", "Chilabothrus inornatus", new[] { "Caguas", "Humacao", "Fajardo", "Arecibo", "Mayaguez", "Ponce" }),

            // Coqui Llanero - ONLY in Toa Baja wetlands area (very restricted) - modeled as Vega Baja/Manati
            ("Coqui Llanero", "Eleutherodactylus juanariveroi", new[] { "Vega Baja", "Manati" }),

            // Puerto Rican Crested Toad - South coast AND North coast - TWO SEPARATE AREAS
            ("Puerto Rican Crested Toad", "Peltophryne lemur", new[] { "Guayama", "Ponce", "Isabela", "Aguadilla" }),

            // Leatherback Sea Turtle - Nests on beaches, east and west coasts
            ("Leatherback Sea Turtle", "Dermochelys coriacea", new[] { "Fajardo", "Humacao", "Rincon", "Aguadilla", "Vieques", "Culebra" }),

            // Hawksbill Sea Turtle - Beaches and coral reefs around the island
            ("Hawksbill Sea Turtle", "Eretmochelys imbricata", new[] { "Fajardo", "Culebra", "Vieques", "Rincon", "Cabo Rojo", "Guayama" }),

            // West Indian Manatee - Coastal waters, bays
            ("West Indian Manatee", "Trichechus manatus", new[] { "San Juan", "Carolina", "Fajardo", "Guayama", "Ponce", "Mayaguez", "Vieques" }),

            // Yellow-shouldered Blackbird - Southwest coast AND Mona Island (modeled as Cabo Rojo/Mayaguez)
            ("Yellow-shouldered Blackbird", "Agelaius xanthomus", new[] { "Cabo Rojo", "Mayaguez", "Yauco", "Ponce" }),

            // Puerto Rican Nightjar - Dry forests in southwest
            ("Puerto Rican Nightjar", "Antrostomus noctitherus", new[] { "Cabo Rojo", "Yauco", "Ponce", "Guayama" }),

            // Ponce Guabairo (dummy species for testing)
            ("Ponce Cave Bat", "Mormoops blainvillei", new[] { "Ponce", "Yauco", "Guayama" }),

            // Puerto Rican Racer - widespread but including south coast
            ("Puerto Rican Racer", "Borikenophis portoricensis", new[] { "Ponce", "Guayama", "Caguas", "Mayaguez", "Arecibo" }),

            // Elfin-woods Warbler - High elevation forests (El Yunque and Maricao)
            ("Elfin-woods Warbler", "Setophaga angelae", new[] { "Fajardo", "Humacao", "Caguas", "Mayaguez", "Yauco" }),

            // Puerto Rican Sharp-shinned Hawk - Mountain forests
            ("Puerto Rican Sharp-shinned Hawk", "Accipiter striatus venator", new[] { "Caguas", "Humacao", "Arecibo", "Mayaguez" }),

            // Green Sea Turtle - Coastal areas
            ("Green Sea Turtle", "Chelonia mydas", new[] { "Culebra", "Vieques", "Fajardo", "Humacao", "Rincon" }),
        };

        var savedMunicipalities = await context.Municipalities.ToDictionaryAsync(m => m.Name, m => m);
        var savedPractices = await context.NrcsPractices.ToListAsync();
        var savedActions = await context.FwsActions.ToListAsync();

        foreach (var (commonName, scientificName, regions) in speciesData)
        {
            var species = new Species
            {
                CommonName = commonName,
                ScientificName = scientificName
            };
            context.Species.Add(species);
            await context.SaveChangesAsync();

            // Link species to municipalities
            foreach (var region in regions)
            {
                if (savedMunicipalities.TryGetValue(region, out var municipality))
                {
                    context.MunicipalitySpecies.Add(new MunicipalitySpecies
                    {
                        MunicipalityId = municipality.Id,
                        SpeciesId = species.Id
                    });
                }
            }
            await context.SaveChangesAsync();

            // Add conservation links based on species type
            var links = GetConservationLinks(commonName, savedPractices, savedActions);
            foreach (var (practice, action, justification) in links)
            {
                context.FwsLinks.Add(new FwsLink
                {
                    SpeciesId = species.Id,
                    NrcsPracticeId = practice.Id,
                    FwsActionId = action.Id,
                    Justification = justification
                });
            }
            await context.SaveChangesAsync();
        }
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
