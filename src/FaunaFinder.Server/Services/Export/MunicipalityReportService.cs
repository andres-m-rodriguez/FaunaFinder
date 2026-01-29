using System.Globalization;
using System.Text;
using FaunaFinder.Server.Services.Localization;
using FaunaFinder.Wildlife.Contracts.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FaunaFinder.Server.Services.Export;

public class MunicipalityReportService(IAppLocalizer localizer) : IMunicipalityReportService
{
    private readonly IAppLocalizer _localizer = localizer;
    public byte[] GeneratePdfReport(string municipalityName, IReadOnlyList<SpeciesForListDto> species)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, municipalityName));
                page.Content().Element(c => ComposeContent(c, species));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, string municipalityName)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("FaunaFinder")
                        .Bold().FontSize(20).FontColor(Colors.Green.Darken2);
                    col.Item().Text("Puerto Rico Conservation Report")
                        .FontSize(12).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(120).AlignRight().Column(col =>
                {
                    col.Item().Text(DateTime.Now.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture))
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });

            column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Green.Darken2);

            column.Item().PaddingTop(15).Text(municipalityName)
                .Bold().FontSize(16);

            column.Item().PaddingTop(5).PaddingBottom(15);
        });
    }

    private void ComposeContent(IContainer container, IReadOnlyList<SpeciesForListDto> species)
    {
        container.Column(column =>
        {
            column.Item().Text($"Total Species: {species.Count}")
                .FontSize(11).Bold().FontColor(Colors.Grey.Darken2);

            column.Item().PaddingTop(15);

            foreach (var s in species)
            {
                column.Item().Element(c => ComposeSpeciesCard(c, s));
            }
        });
    }

    private void ComposeSpeciesCard(IContainer container, SpeciesForListDto species)
    {
        container.PaddingBottom(12).Border(1).BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten5).Padding(10).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(_localizer.GetLocalizedValue(species.CommonName))
                            .Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                        col.Item().Text(species.ScientificName)
                            .Italic().FontSize(10).FontColor(Colors.Grey.Darken1);
                    });
                });

                if (species.FwsLinks.Count > 0)
                {
                    column.Item().PaddingTop(8).Text("Conservation Links")
                        .FontSize(9).Bold().FontColor(Colors.Grey.Darken2);

                    foreach (var link in species.FwsLinks)
                    {
                        column.Item().PaddingTop(5).Background(Colors.White)
                            .Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6).Column(linkCol =>
                            {
                                linkCol.Item().Row(row =>
                                {
                                    row.ConstantItem(70).Text($"NRCS {link.NrcsPractice.Code}")
                                        .FontSize(8).Bold().FontColor(Colors.Blue.Darken2);
                                    row.RelativeItem().Text(link.NrcsPractice.Name)
                                        .FontSize(9);
                                });

                                linkCol.Item().PaddingTop(3).Row(row =>
                                {
                                    row.ConstantItem(70).Text($"FWS {link.FwsAction.Code}")
                                        .FontSize(8).Bold().FontColor(Colors.Orange.Darken2);
                                    row.RelativeItem().Text(link.FwsAction.Name)
                                        .FontSize(9);
                                });

                                if (!string.IsNullOrEmpty(link.Justification))
                                {
                                    linkCol.Item().PaddingTop(4).Text(link.Justification)
                                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                                }
                            });
                    }
                }
            });
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(row =>
        {
            row.RelativeItem().Text("Generated by FaunaFinder - faunafinder.pr")
                .FontSize(8).FontColor(Colors.Grey.Darken1);

            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Page ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    public byte[] GenerateCsvReport(string municipalityName, IReadOnlyList<SpeciesForListDto> species)
    {
        var sb = new StringBuilder();

        // Header row
        sb.AppendLine("Municipality,Common Name,Scientific Name,NRCS Practice Code,NRCS Practice Name,FWS Action Code,FWS Action Name,Justification");

        foreach (var s in species)
        {
            if (s.FwsLinks.Count == 0)
            {
                // Species with no conservation links
                sb.AppendLine($"{EscapeCsv(municipalityName)},{EscapeCsv(_localizer.GetLocalizedValue(s.CommonName))},{EscapeCsv(s.ScientificName)},,,,");
            }
            else
            {
                // One row per conservation link
                foreach (var link in s.FwsLinks)
                {
                    sb.AppendLine($"{EscapeCsv(municipalityName)},{EscapeCsv(_localizer.GetLocalizedValue(s.CommonName))},{EscapeCsv(s.ScientificName)},{EscapeCsv(link.NrcsPractice.Code)},{EscapeCsv(link.NrcsPractice.Name)},{EscapeCsv(link.FwsAction.Code)},{EscapeCsv(link.FwsAction.Name)},{EscapeCsv(link.Justification ?? "")}");
                }
            }
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
