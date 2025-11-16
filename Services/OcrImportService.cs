using System.Text.RegularExpressions;
using Tesseract;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service d'import de filons via OCR depuis des documents papier scannés
    /// </summary>
    public class OcrImportService : IDisposable
    {
        private TesseractEngine? _ocrEngine;
        private readonly string _tessDataPath;

        public OcrImportService()
        {
            // Chemin vers les données Tesseract (é télécharger)
            _tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

            // Créer le dossier si nécessaire
            if (!Directory.Exists(_tessDataPath))
            {
                Directory.CreateDirectory(_tessDataPath);
            }
        }

        /// <summary>
        /// Initialise le moteur OCR avec la langue franéaise
        /// </summary>
        public bool InitializeOcr()
        {
            try
            {
                // Vérifier si les données franéaises sont présentes
                var frDataPath = Path.Combine(_tessDataPath, "fra.traineddata");

                if (!File.Exists(frDataPath))
                {
                    // Vérifier aussi dans le dossier du projet
                    var projectTessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "tessdata", "fra.traineddata");
                    var fullProjectPath = Path.GetFullPath(projectTessDataPath);

                    if (File.Exists(fullProjectPath))
                    {
                        // Copier depuis le dossier du projet
                        Directory.CreateDirectory(_tessDataPath);
                        File.Copy(fullProjectPath, frDataPath, true);
                    }
                    else
                    {
                        throw new FileNotFoundException(
                            $"Les données OCR franéaises sont manquantes.\n\n" +
                            $"Dossier recherché :\n{_tessDataPath}\n\n" +
                            $"Solution :\n" +
                            $"1. Téléchargez 'fra.traineddata' depuis :\n" +
                            $"   https://github.com/tesseract-ocr/tessdata/raw/main/fra.traineddata\n\n" +
                            $"2. Placez-le dans l'un de ces dossiers :\n" +
                            $"   é {_tessDataPath}\n" +
                            $"   é {Path.GetDirectoryName(fullProjectPath)}\n\n" +
                            $"3. Relancez l'application");
                    }
                }

                _ocrEngine = new TesseractEngine(_tessDataPath, "fra", EngineMode.Default);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur initialisation OCR: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extrait le texte d'une image scannée
        /// </summary>
        public string ExtractTextFromImage(string imagePath)
        {
            if (_ocrEngine == null)
            {
                InitializeOcr();
            }

            try
            {
                using var img = Pix.LoadFromFile(imagePath);
                using var page = _ocrEngine!.Process(img);
                return page.GetText();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur extraction OCR: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Parse le texte OCR pour extraire les filons avec leurs coordonnées Lambert
        /// </summary>
        public List<FilonImportResult> ParseFilonsFromText(string ocrText)
        {
            var results = new List<FilonImportResult>();
            var lines = ocrText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var cleanLine = line.Trim();
                if (string.IsNullOrWhiteSpace(cleanLine) || cleanLine.Length < 10)
                    continue;

                var result = TryParseFilonLine(cleanLine);
                if (result != null)
                {
                    results.Add(result);
                }
            }

            return results;
        }

        /// <summary>
        /// Tente de parser une ligne pour extraire nom + coordonnées Lambert
        /// </summary>
        private FilonImportResult? TryParseFilonLine(string line)
        {
            // Pattern 1: "Nom du filon X Y" (séparés par espaces)
            var pattern1 = @"^(.+?)\s+(\d{6,7})[,\s]+(\d{6,7})";

            // Pattern 2: "Nom X=123456 Y=234567"
            var pattern2 = @"^(.+?)\s+X\s*[=:]\s*(\d{6,7})[,\s]+Y\s*[=:]\s*(\d{6,7})";

            // Pattern 3: "Nom Lambert: 123456, 234567"
            var pattern3 = @"^(.+?)\s+[Ll]ambert\s*[:\-]?\s*(\d{6,7})[,\s]+(\d{6,7})";

            foreach (var pattern in new[] { pattern1, pattern2, pattern3 })
            {
                var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var nom = CleanFilonName(match.Groups[1].Value);
                    var lambertX = double.Parse(match.Groups[2].Value);
                    var lambertY = double.Parse(match.Groups[3].Value);

                    // Vérifier que les coordonnées sont dans la zone Var/Alpes-Maritimes
                    if (IsValidLambertCoordinates(lambertX, lambertY))
                    {
                        // Convertir Lambert -> GPS
                        var (lat, lon) = CoordinateConverter.Lambert3ToWGS84(lambertX, lambertY);

                        return new FilonImportResult
                        {
                            Nom = nom,
                            LambertX = lambertX,
                            LambertY = lambertY,
                            Latitude = lat,
                            Longitude = lon,
                            OriginalLine = line,
                            IsValid = true
                        };
                    }
                }
            }

            // Ligne non reconnue
            return new FilonImportResult
            {
                OriginalLine = line,
                IsValid = false,
                ErrorMessage = "Format non reconnu"
            };
        }

        /// <summary>
        /// Nettoie le nom du filon (supprime caractéres spéciaux OCR)
        /// </summary>
        private string CleanFilonName(string name)
        {
            // Supprimer les caractéres parasites de l'OCR
            name = Regex.Replace(name, @"[^\w\s\-'éééééééééééééééééééééééééééܟé]", "");
            name = name.Trim();

            // Capitaliser la premiére lettre de chaque mot
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
        }

        /// <summary>
        /// Vérifie que les coordonnées Lambert sont cohérentes avec la région
        /// </summary>
        private bool IsValidLambertCoordinates(double x, double y)
        {
            // Zone Var/Alpes-Maritimes (Lambert 3 Zone Sud)
            // X: environ 900000 é 1100000
            // Y: environ 3000000 é 3200000
            return x >= 850000 && x <= 1150000 &&
                   y >= 2950000 && y <= 3250000;
        }

        /// <summary>
        /// Import depuis plusieurs images (pour documents multi-pages)
        /// </summary>
        public List<FilonImportResult> ImportFromMultipleImages(string[] imagePaths)
        {
            var allResults = new List<FilonImportResult>();

            foreach (var imagePath in imagePaths)
            {
                try
                {
                    var text = ExtractTextFromImage(imagePath);
                    var filons = ParseFilonsFromText(text);

                    // Ajouter le nom du fichier source
                    foreach (var filon in filons)
                    {
                        filon.SourceFile = Path.GetFileName(imagePath);
                    }

                    allResults.AddRange(filons);
                }
                catch (Exception ex)
                {
                    allResults.Add(new FilonImportResult
                    {
                        OriginalLine = $"Erreur fichier: {Path.GetFileName(imagePath)}",
                        IsValid = false,
                        ErrorMessage = ex.Message,
                        SourceFile = Path.GetFileName(imagePath)
                    });
                }
            }

            return allResults;
        }

        public void Dispose()
        {
            _ocrEngine?.Dispose();
        }
    }

    /// <summary>
    /// Résultat de l'import d'un filon via OCR
    /// </summary>
    public class FilonImportResult
    {
        public string? Nom { get; set; }
        public double? LambertX { get; set; }
        public double? LambertY { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? OriginalLine { get; set; }
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SourceFile { get; set; }

        /// <summary>
        /// Convertit en objet Filon pour insertion en base
        /// </summary>
        public Filon ToFilon()
        {
            return new Filon
            {
                Id = Guid.NewGuid(),
                Nom = Nom ?? "Filon sans nom",
                MatierePrincipale = MineralType.Fer, // Valeur par défaut
                Latitude = Latitude,
                Longitude = Longitude,
                LambertX = LambertX,
                LambertY = LambertY,
                Notes = $"Importé via OCR\n?? Source: {SourceFile}\n?? Ligne originale: {OriginalLine}",
                DateCreation = DateTime.Now,
                DateModification = DateTime.Now
            };
        }
    }
}
