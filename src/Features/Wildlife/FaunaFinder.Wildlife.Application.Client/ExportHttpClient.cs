namespace FaunaFinder.Wildlife.Application.Client;

public sealed class ExportHttpClient(HttpClient httpClient) : IExportHttpClient
{
    public async Task<byte[]> ExportMunicipalityPdfAsync(
        int municipalityId,
        IEnumerable<int>? speciesIds = null,
        CancellationToken cancellationToken = default
    )
    {
        var url = $"/api/export/municipality/{municipalityId}/pdf";
        if (speciesIds?.Any() == true)
        {
            url += "?speciesIds=" + string.Join(",", speciesIds);
        }

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<byte[]> ExportMunicipalityCsvAsync(
        int municipalityId,
        IEnumerable<int>? speciesIds = null,
        CancellationToken cancellationToken = default
    )
    {
        var url = $"/api/export/municipality/{municipalityId}/csv";
        if (speciesIds?.Any() == true)
        {
            url += "?speciesIds=" + string.Join(",", speciesIds);
        }

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<byte[]> ExportSpeciesPdfAsync(
        int speciesId,
        CancellationToken cancellationToken = default
    )
    {
        var url = $"/api/export/species/{speciesId}/pdf";
        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
