using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using wmine.Models;
using System.Drawing;
using System.Drawing.Imaging;

namespace wmine.Services
{
    /// <summary>
    /// Service d'export de fiches de filons en PDF
    /// </summary>
    public class PdfExportService
    {
        public void ExportFilonToPdf(Filon filon, string outputPath)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Text($"Fiche Filon - {filon.Nom}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            // Informations principales
                            column.Item().Text(text =>
                            {
                                text.Span("Nom: ").Bold();
                                text.Span(filon.Nom);
                            });

                            column.Item().Text(text =>
                            {
                                text.Span("Matiére principale: ").Bold();
                                text.Span(MineralColors.GetDisplayName(filon.MatierePrincipale));
                            });

                            if (filon.MatieresSecondaires.Any())
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("Matiéres secondaires: ").Bold();
                                    text.Span(string.Join(", ", 
                                        filon.MatieresSecondaires.Select(m => MineralColors.GetDisplayName(m))));
                                });
                            }

                            // état du filon
                            column.Item().Text(text =>
                            {
                                text.Span("état: ").Bold();
                                var statuts = GetStatusList(filon.Statut);
                                text.Span(string.Join(", ", statuts));
                            });

                            // Ancrage
                            if (filon.AnneeAncrage.HasValue)
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("Ancrage: ").Bold();
                                    text.Span(filon.AnneeAncrage.Value.ToString());
                                });
                            }
                            else
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("Ancrage: ").Bold();
                                    text.Span("Non renseigné");
                                });
                            }

                            // Coordonnées
                            if (filon.Latitude.HasValue && filon.Longitude.HasValue)
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("Coordonnées GPS: ").Bold();
                                    text.Span($"{filon.Latitude:F6}éN, {filon.Longitude:F6}éE");
                                });
                            }

                            if (filon.LambertX.HasValue && filon.LambertY.HasValue)
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("Lambert 3 Sud: ").Bold();
                                    text.Span($"X={filon.LambertX:F2}, Y={filon.LambertY:F2}");
                                });
                            }

                            // Carte de localisation
                            if (filon.Latitude.HasValue && filon.Longitude.HasValue)
                            {
                                column.Item().PaddingTop(10).Column(mapColumn =>
                                {
                                    mapColumn.Item().Text("Localisation:").Bold();
                                    mapColumn.Item().PaddingTop(5).Text("Une carte interactive est disponible dans l'application.")
                                        .FontSize(9).Italic();
                                });
                            }

                            // Notes
                            if (!string.IsNullOrWhiteSpace(filon.Notes))
                            {
                                column.Item().PaddingTop(10).Column(notesColumn =>
                                {
                                    notesColumn.Item().Text("Notes:").Bold();
                                    notesColumn.Item().Text(filon.Notes);
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Généré le ").FontSize(8);
                            text.Span(DateTime.Now.ToString("dd/MM/yyyy é HH:mm")).FontSize(8);
                            text.Span(" - WMine Localisateur").FontSize(8);
                        });
                });
            })
            .GeneratePdf(outputPath);
        }

        private List<string> GetStatusList(FilonStatus status)
        {
            var statuses = new List<string>();

            if (status == FilonStatus.Aucun)
            {
                statuses.Add("Aucun état défini");
                return statuses;
            }

            foreach (FilonStatus value in Enum.GetValues(typeof(FilonStatus)))
            {
                if (value != FilonStatus.Aucun && status.HasFlag(value))
                {
                    statuses.Add(value.GetDisplayName());
                }
            }

            return statuses;
        }

        public void ExportMultipleFilonsToPdf(List<Filon> filons, string outputPath)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text($"Liste des Filons - {filons.Count} filon(s)")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            foreach (var filon in filons)
                            {
                                column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Column(filonColumn =>
                                {
                                    filonColumn.Item().Text(filon.Nom).Bold().FontSize(12);
                                    filonColumn.Item().Text($"Matiére: {MineralColors.GetDisplayName(filon.MatierePrincipale)}");
                                    
                                    var statuts = GetStatusList(filon.Statut);
                                    filonColumn.Item().Text($"état: {string.Join(", ", statuts)}");
                                    
                                    if (filon.Latitude.HasValue && filon.Longitude.HasValue)
                                    {
                                        filonColumn.Item().Text($"GPS: {filon.Latitude:F6}éN, {filon.Longitude:F6}éE");
                                    }
                                });

                                column.Item().PaddingBottom(10);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Page ").FontSize(8);
                            text.CurrentPageNumber().FontSize(8);
                            text.Span(" / ").FontSize(8);
                            text.TotalPages().FontSize(8);
                            text.Span(" - Généré le ").FontSize(8);
                            text.Span(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(8);
                        });
                });
            })
            .GeneratePdf(outputPath);
        }
    }
}
