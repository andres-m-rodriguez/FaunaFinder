using FaunaFinder.Wildlife.Contracts.Dtos;

namespace FaunaFinder.Server.Services.Export;

public interface ISpeciesReportService
{
    byte[] GeneratePdfReport(SpeciesForDetailDto species);
}
