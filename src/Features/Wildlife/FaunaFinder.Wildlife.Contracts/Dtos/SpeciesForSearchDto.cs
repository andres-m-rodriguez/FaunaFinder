using FaunaFinder.i18n.Contracts;

namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record SpeciesForSearchDto(
    int Id,
    List<LocaleValue> CommonName,
    string ScientificName,
    IReadOnlyList<string> MunicipalityNames,
    bool IsFauna,
    bool HasProfileImage
)
{
    public string GetCommonName(string locale = "en") =>
        CommonName.FirstOrDefault(x => x.Code == locale)?.Value
        ?? CommonName.FirstOrDefault()?.Value
        ?? ScientificName;
}
