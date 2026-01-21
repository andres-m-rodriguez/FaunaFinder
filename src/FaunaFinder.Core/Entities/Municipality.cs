namespace FaunaFinder.Core.Entities;

public class Municipality
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string GeoJsonId { get; set; }

    public ICollection<MunicipalitySpecies> MunicipalitySpecies { get; set; } = new List<MunicipalitySpecies>();
}
