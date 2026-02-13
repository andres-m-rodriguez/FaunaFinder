using System.Globalization;
using FaunaFinder.Server.Services.Localization;
using FaunaFinder.Wildlife.Contracts.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FaunaFinder.Server.Services.Export;

public class SpeciesReportService(IAppLocalizer localizer) : ISpeciesReportService
{
    private readonly IAppLocalizer _localizer = localizer;

    public byte[] GeneratePdfReport(SpeciesForDetailDto species)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Element(c => ComposeContent(c, species));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeContent(IContainer container, SpeciesForDetailDto species)
    {
        container.Column(column =>
        {
            // Title section (only appears once at the top)
            column
                .Item()
                .Row(row =>
                {
                    row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item()
                                .Text("FaunaFinder")
                                .Bold()
                                .FontSize(20)
                                .FontColor(Colors.Green.Darken2);
                            col.Item()
                                .Text("Species Conservation Report")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    row.ConstantItem(120)
                        .AlignRight()
                        .Column(col =>
                        {
                            col.Item()
                                .Text(
                                    DateTime.Now.ToString(
                                        "MMMM d, yyyy",
                                        CultureInfo.InvariantCulture
                                    )
                                )
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        });
                });

            column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Green.Darken2);

            // Species name
            column
                .Item()
                .PaddingTop(15)
                .Text(_localizer.GetLocalizedValue(species.CommonName))
                .Bold()
                .FontSize(16);

            column
                .Item()
                .PaddingTop(3)
                .Text(species.ScientificName)
                .Italic()
                .FontSize(12)
                .FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(15);

            // Municipalities section
            if (species.Municipalities.Count > 0)
            {
                column
                    .Item()
                    .Text("Municipalities")
                    .FontSize(12)
                    .Bold()
                    .FontColor(Colors.Green.Darken3);

                column.Item().PaddingTop(5);

                column
                    .Item()
                    .Text(string.Join(", ", species.Municipalities.Select(m => m.Name)))
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken2);

                column.Item().PaddingTop(15);
            }

            // Conservation Links section
            if (species.FwsLinks.Count > 0)
            {
                column
                    .Item()
                    .Text($"Conservation Links ({species.FwsLinks.Count})")
                    .FontSize(12)
                    .Bold()
                    .FontColor(Colors.Green.Darken3);

                column.Item().PaddingTop(10);

                foreach (var link in species.FwsLinks)
                {
                    column.Item().Element(c => ComposeConservationLink(c, link));
                }
            }
            else
            {
                column
                    .Item()
                    .Text("No conservation links available for this species.")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);
            }
        });
    }

    private void ComposeConservationLink(IContainer container, FwsLinkDto link)
    {
        container
            .PaddingBottom(10)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten5)
            .Padding(10)
            .Column(column =>
            {
                column
                    .Item()
                    .Row(row =>
                    {
                        row.ConstantItem(70)
                            .Text($"NRCS {link.NrcsPractice.Code}")
                            .FontSize(9)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);
                        row.RelativeItem().Text(link.NrcsPractice.Name).FontSize(10);
                    });

                column
                    .Item()
                    .PaddingTop(4)
                    .Row(row =>
                    {
                        row.ConstantItem(70)
                            .Text($"FWS {link.FwsAction.Code}")
                            .FontSize(9)
                            .Bold()
                            .FontColor(Colors.Orange.Darken2);
                        row.RelativeItem().Text(link.FwsAction.Name).FontSize(10);
                    });

                if (!string.IsNullOrEmpty(link.Justification))
                {
                    column
                        .Item()
                        .PaddingTop(6)
                        .Text(link.Justification)
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                }
            });
    }

    private void ComposeFooter(IContainer container)
    {
        container
            .BorderTop(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingTop(5)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text("Generated by FaunaFinder - faunafinder.pr")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);

                row.RelativeItem()
                    .AlignRight()
                    .Text(text =>
                    {
                        text.Span("Page ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
            });
    }
}
