namespace FaunaFinder.Client.Services.Api;

public interface IExportService
{
    Task<byte[]> ExportMunicipalityPdfAsync(
        int municipalityId,
        IEnumerable<int>? speciesIds = null,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportMunicipalityCsvAsync(
        int municipalityId,
        IEnumerable<int>? speciesIds = null,
        CancellationToken cancellationToken = default);
}
