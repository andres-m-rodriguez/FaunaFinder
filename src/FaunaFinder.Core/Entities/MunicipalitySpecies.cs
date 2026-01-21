namespace FaunaFinder.Core.Entities;

public class MunicipalitySpecies
{
    public int Id { get; set; }
    public int MunicipalityId { get; set; }
    public int SpeciesId { get; set; }

    public Municipality Municipality { get; set; } = null!;
    public Species Species { get; set; } = null!;
}
