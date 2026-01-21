namespace FaunaFinder.Core.Entities;

public class Species
{
    public int Id { get; set; }
    public required string CommonName { get; set; }
    public required string ScientificName { get; set; }

    public ICollection<FwsLink> FwsLinks { get; set; } = new List<FwsLink>();
    public ICollection<MunicipalitySpecies> MunicipalitySpecies { get; set; } = new List<MunicipalitySpecies>();
}
