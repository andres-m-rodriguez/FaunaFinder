using FaunaFinder.Core.Entities;
using FaunaFinder.Core.Interfaces;
using FaunaFinder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Infrastructure.Repositories;

public class FaunaFinderRepository : IFaunaFinderRepository
{
    private readonly FaunaFinderDbContext _context;

    public FaunaFinderRepository(FaunaFinderDbContext context)
    {
        _context = context;
    }

    // Municipalities
    public async Task<IReadOnlyList<Municipality>> GetAllMunicipalitiesAsync()
    {
        return await _context.Municipalities
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<Municipality?> GetMunicipalityByIdAsync(int id)
    {
        return await _context.Municipalities.FindAsync(id);
    }

    public async Task<Municipality?> GetMunicipalityWithSpeciesAsync(int id)
    {
        return await _context.Municipalities
            .Include(m => m.MunicipalitySpecies)
                .ThenInclude(ms => ms.Species)
                    .ThenInclude(s => s.FwsLinks)
                        .ThenInclude(fl => fl.NrcsPractice)
            .Include(m => m.MunicipalitySpecies)
                .ThenInclude(ms => ms.Species)
                    .ThenInclude(s => s.FwsLinks)
                        .ThenInclude(fl => fl.FwsAction)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    // Species
    public async Task<IReadOnlyList<Species>> GetSpeciesByMunicipalityAsync(int municipalityId)
    {
        var speciesIds = await _context.MunicipalitySpecies
            .Where(ms => ms.MunicipalityId == municipalityId)
            .Select(ms => ms.SpeciesId)
            .ToListAsync();

        return await _context.Species
            .Where(s => speciesIds.Contains(s.Id))
            .Include(s => s.FwsLinks)
                .ThenInclude(fl => fl.NrcsPractice)
            .Include(s => s.FwsLinks)
                .ThenInclude(fl => fl.FwsAction)
            .OrderBy(s => s.CommonName)
            .ToListAsync();
    }

    public async Task<Species?> GetSpeciesWithDetailsAsync(int speciesId)
    {
        return await _context.Species
            .Include(s => s.FwsLinks)
                .ThenInclude(fl => fl.NrcsPractice)
            .Include(s => s.FwsLinks)
                .ThenInclude(fl => fl.FwsAction)
            .Include(s => s.MunicipalitySpecies)
                .ThenInclude(ms => ms.Municipality)
            .FirstOrDefaultAsync(s => s.Id == speciesId);
    }

    public async Task<IReadOnlyList<Species>> SearchSpeciesAsync(string searchTerm)
    {
        var query = _context.Species.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s => s.CommonName.ToLower().Contains(term) ||
                                     s.ScientificName.ToLower().Contains(term));
        }

        return await query
            .OrderBy(s => s.CommonName)
            .Take(100)
            .ToListAsync();
    }

    // Practices & Actions
    public async Task<IReadOnlyList<NrcsPractice>> GetAllPracticesAsync()
    {
        return await _context.NrcsPractices
            .OrderBy(p => p.Code)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<FwsAction>> GetAllActionsAsync()
    {
        return await _context.FwsActions
            .OrderBy(a => a.Code)
            .ToListAsync();
    }
}
