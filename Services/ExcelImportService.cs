using ClosedXML.Excel;
using wmine.Models;
using wmine.Services;

namespace wmine.Services
{
    /// <summary>
    /// Service d'import de filons depuis des fichiers Excel (.xlsx)
    /// </summary>
    public class ExcelImportService
    {
        /// <summary>
        /// Importe les filons depuis un fichier Excel
        /// </summary>
        /// <param name="excelFilePath">Chemin du fichier Excel</param>
        /// <returns>Liste des résultats d'import</returns>
        public List<FilonImportResult> ImportFromExcel(string excelFilePath)
        {
            var results = new List<FilonImportResult>();

            try
            {
                using var workbook = new XLWorkbook(excelFilePath);
                
                // Chercher la premiére feuille avec des données
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    results.Add(new FilonImportResult
                    {
                        OriginalLine = "Fichier Excel vide",
                        IsValid = false,
                        ErrorMessage = "Aucune feuille trouvée dans le fichier Excel",
                        SourceFile = Path.GetFileName(excelFilePath)
                    });
                    return results;
                }

                // détecter automatiquement les colonnes
                var columnMapping = DetectColumns(worksheet);
                
                if (!columnMapping.IsValid)
                {
                    results.Add(new FilonImportResult
                    {
                        OriginalLine = "Structure Excel invalide",
                        IsValid = false,
                        ErrorMessage = columnMapping.ErrorMessage ?? "Colonnes Nom, Lambert X et Lambert Y non trouvées",
                        SourceFile = Path.GetFileName(excelFilePath)
                    });
                    return results;
                }

                // Lire les données é partir de la ligne suivant l'en-téte
                var firstDataRow = columnMapping.HeaderRow + 1;
                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? firstDataRow;

                for (int row = firstDataRow; row <= lastRow; row++)
                {
                    try
                    {
                        var result = ParseExcelRow(worksheet, row, columnMapping, Path.GetFileName(excelFilePath));
                        if (result != null)
                        {
                            results.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        results.Add(new FilonImportResult
                        {
                            OriginalLine = $"Ligne {row}",
                            IsValid = false,
                            ErrorMessage = $"Erreur de lecture: {ex.Message}",
                            SourceFile = Path.GetFileName(excelFilePath)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add(new FilonImportResult
                {
                    OriginalLine = "Erreur fichier Excel",
                    IsValid = false,
                    ErrorMessage = $"Impossible de lire le fichier: {ex.Message}",
                    SourceFile = Path.GetFileName(excelFilePath)
                });
            }

            return results;
        }

        /// <summary>
        /// détecte automatiquement les colonnes dans la feuille Excel
        /// </summary>
        private ColumnMapping DetectColumns(IXLWorksheet worksheet)
        {
            var mapping = new ColumnMapping { HeaderRow = 1 };
            
            // Chercher l'en-téte dans les 10 premiéres lignes
            for (int row = 1; row <= Math.Min(10, worksheet.LastRowUsed()?.RowNumber() ?? 1); row++)
            {
                var rowData = worksheet.Row(row);
                var cells = rowData.CellsUsed().ToList();
                
                foreach (var cell in cells)
                {
                    var headerText = cell.GetString().Trim().ToLowerInvariant();
                    var colIndex = cell.Address.ColumnNumber;

                    // détecter la colonne Nom
                    if ((headerText.Contains("nom") || headerText.Contains("filon") || 
                         headerText.Contains("mine") || headerText.Contains("site")) && 
                        mapping.NomColumn == 0)
                    {
                        mapping.NomColumn = colIndex;
                        mapping.HeaderRow = row;
                    }
                    
                    // détecter la colonne Lambert X
                    if ((headerText.Contains("lambert") && headerText.Contains("x")) ||
                        headerText == "x" || headerText.Contains("est") || 
                        headerText.Contains("easting"))
                    {
                        mapping.LambertXColumn = colIndex;
                        mapping.HeaderRow = row;
                    }
                    
                    // détecter la colonne Lambert Y
                    if ((headerText.Contains("lambert") && headerText.Contains("y")) ||
                        headerText == "y" || headerText.Contains("nord") || 
                        headerText.Contains("northing"))
                    {
                        mapping.LambertYColumn = colIndex;
                        mapping.HeaderRow = row;
                    }

                    // détecter la colonne Latitude (optionnel)
                    if (headerText.Contains("lat") && !headerText.Contains("lambert"))
                    {
                        mapping.LatitudeColumn = colIndex;
                    }

                    // détecter la colonne Longitude (optionnel)
                    if (headerText.Contains("lon") && !headerText.Contains("lambert"))
                    {
                        mapping.LongitudeColumn = colIndex;
                    }
                }

                // Si on a trouvé les colonnes essentielles, arréter la recherche
                if (mapping.IsValid)
                    break;
            }

            // Si pas de colonnes détectées, essayer la premiére ligne sans en-téte
            if (!mapping.IsValid)
            {
                // Format sans en-téte: colonne 1=Nom, 2=X, 3=Y
                var firstRow = worksheet.Row(1);
                var cellCount = firstRow.CellsUsed().Count();
                
                if (cellCount >= 3)
                {
                    mapping.NomColumn = 1;
                    mapping.LambertXColumn = 2;
                    mapping.LambertYColumn = 3;
                    mapping.HeaderRow = 0; // Pas d'en-téte
                }
            }

            if (!mapping.IsValid)
            {
                mapping.ErrorMessage = 
                    "Colonnes non détectées. Format attendu:\n" +
                    "- Colonne 'Nom' (ou 'Filon', 'Mine', 'Site')\n" +
                    "- Colonne 'Lambert X' (ou 'X', 'Est')\n" +
                    "- Colonne 'Lambert Y' (ou 'Y', 'Nord')\n\n" +
                    "Ou format sans en-téte: Nom | X | Y";
            }

            return mapping;
        }

        /// <summary>
        /// Parse une ligne Excel et retourne un FilonImportResult
        /// </summary>
        private FilonImportResult? ParseExcelRow(IXLWorksheet worksheet, int rowNumber, 
            ColumnMapping mapping, string sourceFile)
        {
            var row = worksheet.Row(rowNumber);
            
            // Lire les valeurs des cellules
            var nom = row.Cell(mapping.NomColumn).GetString().Trim();
            var xText = row.Cell(mapping.LambertXColumn).GetString().Trim();
            var yText = row.Cell(mapping.LambertYColumn).GetString().Trim();

            // Ignorer les lignes vides
            if (string.IsNullOrWhiteSpace(nom) && string.IsNullOrWhiteSpace(xText) && string.IsNullOrWhiteSpace(yText))
            {
                return null;
            }

            var originalLine = $"{nom} | {xText} | {yText}";

            // Essayer de parser les coordonnées Lambert
            if (!double.TryParse(xText.Replace(" ", "").Replace(",", "."), 
                System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out double lambertX))
            {
                return new FilonImportResult
                {
                    OriginalLine = originalLine,
                    IsValid = false,
                    ErrorMessage = $"Lambert X invalide: '{xText}'",
                    SourceFile = sourceFile
                };
            }

            if (!double.TryParse(yText.Replace(" ", "").Replace(",", "."), 
                System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out double lambertY))
            {
                return new FilonImportResult
                {
                    OriginalLine = originalLine,
                    IsValid = false,
                    ErrorMessage = $"Lambert Y invalide: '{yText}'",
                    SourceFile = sourceFile
                };
            }

            // Vérifier que les coordonnées sont dans la zone Var/Alpes-Maritimes
            if (!IsValidLambertCoordinates(lambertX, lambertY))
            {
                return new FilonImportResult
                {
                    OriginalLine = originalLine,
                    IsValid = false,
                    ErrorMessage = "Coordonnées hors zone géographique (Var/Alpes-Maritimes)",
                    SourceFile = sourceFile
                };
            }

            // Convertir Lambert -> GPS
            var (lat, lon) = CoordinateConverter.Lambert3ToWGS84(lambertX, lambertY);

            return new FilonImportResult
            {
                Nom = string.IsNullOrWhiteSpace(nom) ? $"Filon ligne {rowNumber}" : nom,
                LambertX = lambertX,
                LambertY = lambertY,
                Latitude = lat,
                Longitude = lon,
                OriginalLine = originalLine,
                IsValid = true,
                SourceFile = sourceFile
            };
        }

        /// <summary>
        /// Vérifie que les coordonnées Lambert sont cohérentes avec la région
        /// </summary>
        private bool IsValidLambertCoordinates(double x, double y)
        {
            // Zone Var/Alpes-Maritimes (Lambert 3 Zone Sud)
            return x >= 850000 && x <= 1150000 && 
                   y >= 2950000 && y <= 3250000;
        }

        /// <summary>
        /// Mapping des colonnes Excel
        /// </summary>
        private class ColumnMapping
        {
            public int NomColumn { get; set; }
            public int LambertXColumn { get; set; }
            public int LambertYColumn { get; set; }
            public int LatitudeColumn { get; set; }
            public int LongitudeColumn { get; set; }
            public int HeaderRow { get; set; }
            public string? ErrorMessage { get; set; }

            public bool IsValid => NomColumn > 0 && LambertXColumn > 0 && LambertYColumn > 0;
        }
    }
}
