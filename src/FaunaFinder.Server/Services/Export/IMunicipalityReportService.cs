using FaunaFinder.Wildlife.Contracts.Dtos;

namespace FaunaFinder.Server.Services.Export;

public interface IMunicipalityReportService
{
    byte[] GeneratePdfReport(string municipalityName, IReadOnlyList<SpeciesForListDto> species);
    byte[] GenerateCsvReport(string municipalityName, IReadOnlyList<SpeciesForListDto> species);
}
