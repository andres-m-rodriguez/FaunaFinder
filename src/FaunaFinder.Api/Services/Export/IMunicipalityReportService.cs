using FaunaFinder.Contracts.Dtos.Species;

namespace FaunaFinder.Api.Services.Export;

public interface IMunicipalityReportService
{
    byte[] GeneratePdfReport(string municipalityName, IReadOnlyList<SpeciesForListDto> species);
    byte[] GenerateCsvReport(string municipalityName, IReadOnlyList<SpeciesForListDto> species);
}
