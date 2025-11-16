using System.Text;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service d'export de filons au format CSV
    /// </summary>
    public class CsvExportService
    {
        /// <summary>
        /// Exporte une liste de filons vers un fichier CSV
        /// </summary>
        public void ExportFilonsToCsv(List<Filon> filons, string filePath)
        {
            try
            {
                var csv = new StringBuilder();

                // En-tétes CSV
                csv.AppendLine("Nom,Minéral Principal,Latitude,Longitude,Lambert X,Lambert Y,Statut,Notes,Date Création,Date Modification,Chemin Photos,Chemin Documentation");

                // Données
                foreach (var filon in filons)
                {
                    csv.AppendLine(FormatFilonToCsvLine(filon));
                }

                // écriture du fichier avec encodage UTF-8 (support des accents)
                File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'export CSV : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Formate un filon en ligne CSV (échappement des virgules et guillemets)
        /// </summary>
        private string FormatFilonToCsvLine(Filon filon)
        {
            return string.Join(",",
                EscapeCsvField(filon.Nom ?? ""),
                EscapeCsvField(MineralColors.GetDisplayName(filon.MatierePrincipale)),
                filon.Latitude?.ToString("F6") ?? "",
                filon.Longitude?.ToString("F6") ?? "",
                filon.LambertX?.ToString("F2") ?? "",
                filon.LambertY?.ToString("F2") ?? "",
                EscapeCsvField(GetStatusSummary(filon.Statut)),
                EscapeCsvField(filon.Notes ?? ""),
                filon.DateCreation.ToString("yyyy-MM-dd HH:mm:ss"),
                filon.DateModification.ToString("yyyy-MM-dd HH:mm:ss"),
                EscapeCsvField(filon.PhotoPath ?? ""),
                EscapeCsvField(filon.DocumentationPath ?? "")
            );
        }

        /// <summary>
        /// échappe les champs CSV (gestion des virgules, guillemets, retours é la ligne)
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // Si le champ contient des virgules, guillemets ou retours é la ligne
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                // Doubler les guillemets et entourer le champ de guillemets
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        /// <summary>
        /// Génére le résumé du statut
        /// </summary>
        private string GetStatusSummary(FilonStatus status)
        {
            if (status == FilonStatus.Aucun)
                return "Aucun";

            var statuses = new List<string>();
            foreach (FilonStatus value in Enum.GetValues<FilonStatus>())
            {
                if (value != FilonStatus.Aucun && status.HasFlag(value))
                {
                    statuses.Add(value.GetDisplayName());
                }
            }

            return string.Join(" | ", statuses);
        }
    }
}
