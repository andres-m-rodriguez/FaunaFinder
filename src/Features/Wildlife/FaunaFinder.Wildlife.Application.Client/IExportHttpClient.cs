namespace FaunaFinder.Wildlife.Application.Client;

public interface IExportHttpClient
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
