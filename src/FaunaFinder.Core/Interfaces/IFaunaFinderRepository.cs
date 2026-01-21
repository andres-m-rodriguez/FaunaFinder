using FaunaFinder.Core.Entities;

namespace FaunaFinder.Core.Interfaces;

public interface IFaunaFinderRepository
{
    // Municipalities
    Task<IReadOnlyList<Municipality>> GetAllMunicipalitiesAsync();
    Task<Municipality?> GetMunicipalityByIdAsync(int id);
    Task<Municipality?> GetMunicipalityWithSpeciesAsync(int id);

    // Species
    Task<IReadOnlyList<Species>> GetSpeciesByMunicipalityAsync(int municipalityId);
    Task<Species?> GetSpeciesWithDetailsAsync(int speciesId);
    Task<IReadOnlyList<Species>> SearchSpeciesAsync(string searchTerm);

    // Practices & Actions
    Task<IReadOnlyList<NrcsPractice>> GetAllPracticesAsync();
    Task<IReadOnlyList<FwsAction>> GetAllActionsAsync();
}
