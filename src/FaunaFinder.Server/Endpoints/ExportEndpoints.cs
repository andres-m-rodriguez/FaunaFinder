using FaunaFinder.Server.Services.Export;
using FaunaFinder.Wildlife.DataAccess.Interfaces;

namespace FaunaFinder.Server.Endpoints;

public static class ExportEndpoints
{
    public static void MapExportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/export").WithTags("Export");

        group.MapGet("/municipality/{municipalityId:int}/pdf", ExportMunicipalityPdf).WithName("ExportMunicipalityPdf");
        group.MapGet("/municipality/{municipalityId:int}/csv", ExportMunicipalityCsv).WithName("ExportMunicipalityCsv");
        group.MapGet("/species/{speciesId:int}/pdf", ExportSpeciesPdf).WithName("ExportSpeciesPdf");
    }

    private static async Task<IResult> ExportMunicipalityPdf(
        int municipalityId,
        string? speciesIds,
        IMunicipalityRepository municipalityRepository,
        ISpeciesRepository speciesRepository,
        IMunicipalityReportService reportService,
        CancellationToken ct)
    {
        var municipality = await municipalityRepository.GetMunicipalityDetailAsync(municipalityId, ct);
        if (municipality is null)
            return Results.NotFound();

        var species = await speciesRepository.GetSpeciesByMunicipalityAsync(municipalityId, ct);

        if (!string.IsNullOrEmpty(speciesIds))
        {
            var ids = speciesIds.Split(',').Select(int.Parse).ToHashSet();
            species = species.Where(s => ids.Contains(s.Id)).ToList();
        }

        var pdfBytes = reportService.GeneratePdfReport(municipality.Name, species.ToList());
        var fileName = $"{municipality.Name.Replace(" ", "_")}_Species_Report.pdf";

        return Results.File(pdfBytes, "application/pdf", fileName);
    }

    private static async Task<IResult> ExportMunicipalityCsv(
        int municipalityId,
        string? speciesIds,
        IMunicipalityRepository municipalityRepository,
        ISpeciesRepository speciesRepository,
        IMunicipalityReportService reportService,
        CancellationToken ct)
    {
        var municipality = await municipalityRepository.GetMunicipalityDetailAsync(municipalityId, ct);
        if (municipality is null)
            return Results.NotFound();

        var species = await speciesRepository.GetSpeciesByMunicipalityAsync(municipalityId, ct);

        if (!string.IsNullOrEmpty(speciesIds))
        {
            var ids = speciesIds.Split(',').Select(int.Parse).ToHashSet();
            species = species.Where(s => ids.Contains(s.Id)).ToList();
        }

        var csvBytes = reportService.GenerateCsvReport(municipality.Name, species.ToList());
        var fileName = $"{municipality.Name.Replace(" ", "_")}_Species_Report.csv";

        return Results.File(csvBytes, "text/csv", fileName);
    }

    private static async Task<IResult> ExportSpeciesPdf(
        int speciesId,
        ISpeciesRepository speciesRepository,
        ISpeciesReportService reportService,
        CancellationToken ct)
    {
        var species = await speciesRepository.GetSpeciesDetailAsync(speciesId, ct);
        if (species is null)
            return Results.NotFound();

        var pdfBytes = reportService.GeneratePdfReport(species);
        var commonName = species.CommonName.FirstOrDefault()?.Value ?? species.ScientificName;
        var fileName = $"{commonName.Replace(" ", "_")}_Report.pdf";

        return Results.File(pdfBytes, "application/pdf", fileName);
    }
}
