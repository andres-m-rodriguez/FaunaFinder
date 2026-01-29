namespace FaunaFinder.Wildlife.Application.Client;

public class ExportApiService : IExportService
{
    private readonly HttpClient _httpClient;

    public ExportApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> ExportMunicipalityPdfAsync(
        int municipalityId,
        IEnumerable<int>? speciesIds = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/export/municipality/{municipalityId}/pdf";
        if (speciesIds?.Any() == true)
        {
            url += "?speciesIds=" + string.Join(",", speciesIds);
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<byte[]> ExportMunicipalityCsvAsync(
        int municipalityId,
        IEnumerable<int>? speciesIds = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/export/municipality/{municipalityId}/csv";
        if (speciesIds?.Any() == true)
        {
            url += "?speciesIds=" + string.Join(",", speciesIds);
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
