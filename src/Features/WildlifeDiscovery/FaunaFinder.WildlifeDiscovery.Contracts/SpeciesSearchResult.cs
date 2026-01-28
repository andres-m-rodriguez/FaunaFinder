using FaunaFinder.i18n.Contracts;

namespace FaunaFinder.WildlifeDiscovery.Contracts;

public record SpeciesSearchResult(int Id, List<LocaleValue> CommonName, string ScientificName)
{
    public string GetCommonName(string locale = "en") =>
        CommonName.FirstOrDefault(x => x.Code == locale)?.Value
        ?? CommonName.FirstOrDefault()?.Value
        ?? ScientificName;
}
