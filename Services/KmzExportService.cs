using System.IO.Compression;
using System.Text;
using System.Xml;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service d'export KMZ (KML avec photos compress�es)
    /// Compatible avec Google Earth
    /// </summary>
    public class KmzExportService
    {
        /// <summary>
        /// Exporte tous les filons en KMZ avec leurs photos
        /// </summary>
        public bool ExportToKmz(List<Filon> filons, string outputPath)
        {
            try
            {
                // Cr�er un dossier temporaire pour le KMZ
                var tempDir = Path.Combine(Path.GetTempPath(), $"wmine_kmz_{Guid.NewGuid()}");
                Directory.CreateDirectory(tempDir);

                var filesDir = Path.Combine(tempDir, "files");
                Directory.CreateDirectory(filesDir);

                // G�n�rer le fichier KML
                var kmlPath = Path.Combine(tempDir, "doc.kml");
                GenerateKml(filons, kmlPath, filesDir);

                // Cr�er le fichier KMZ (ZIP)
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                ZipFile.CreateFromDirectory(tempDir, outputPath);

                // Nettoyer
                Directory.Delete(tempDir, true);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// G�n�re le fichier KML
        /// </summary>
        private void GenerateKml(List<Filon> filons, string kmlPath, string filesDir)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            using var writer = XmlWriter.Create(kmlPath, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("kml", "http://www.opengis.net/kml/2.2");

            writer.WriteStartElement("Document");
            writer.WriteElementString("name", "WMine - Filons Miniers");
            writer.WriteElementString("description", $"Export� le {DateTime.Now:dd/MM/yyyy � HH:mm}");

            // D�finir les styles par type de min�ral
            DefineStyles(writer);

            // Ajouter chaque filon
            foreach (var filon in filons)
            {
                if (!filon.Latitude.HasValue || !filon.Longitude.HasValue)
                    continue;

                AddFilonPlacemark(writer, filon, filesDir);
            }

            writer.WriteEndElement(); // Document
            writer.WriteEndElement(); // kml
            writer.WriteEndDocument();
        }

        /// <summary>
        /// D�finit les styles KML pour chaque type de min�ral
        /// </summary>
        private void DefineStyles(XmlWriter writer)
        {
            var mineralTypes = Enum.GetValues<MineralType>();

            foreach (var mineral in mineralTypes)
            {
                var color = MineralColors.GetColor(mineral);
                var kmlColor = ColorToKmlColor(color);

                writer.WriteStartElement("Style");
                writer.WriteAttributeString("id", $"style_{mineral}");

                writer.WriteStartElement("IconStyle");
                writer.WriteElementString("scale", "1.2");
                writer.WriteElementString("color", kmlColor);
                writer.WriteStartElement("Icon");
                writer.WriteElementString("href", "http://maps.google.com/mapfiles/kml/shapes/mining.png");
                writer.WriteEndElement(); // Icon
                writer.WriteEndElement(); // IconStyle

                writer.WriteStartElement("LabelStyle");
                writer.WriteElementString("scale", "0.9");
                writer.WriteElementString("color", kmlColor);
                writer.WriteEndElement(); // LabelStyle

                writer.WriteEndElement(); // Style
            }
        }

        /// <summary>
        /// Ajoute un placemark pour un filon
        /// </summary>
        private void AddFilonPlacemark(XmlWriter writer, Filon filon, string filesDir)
        {
            writer.WriteStartElement("Placemark");
            writer.WriteElementString("name", filon.Nom);
            writer.WriteElementString("styleUrl", $"#style_{filon.MatierePrincipale}");

            // Description avec HTML
            var description = GenerateHtmlDescription(filon, filesDir);
            writer.WriteStartElement("description");
            writer.WriteCData(description);
            writer.WriteEndElement();

            // Extended Data
            writer.WriteStartElement("ExtendedData");

            AddData(writer, "Mati�re Principale", filon.MatierePrincipale.ToString());
            AddData(writer, "Mati�res Secondaires", string.Join(", ", filon.MatieresSecondaires));
            AddData(writer, "Statut", filon.Statut.ToString());
            AddData(writer, "Ann�e Ancrage", filon.AnneeAncrage?.ToString() ?? "Inconnue");
            AddData(writer, "Date Cr�ation", filon.DateCreation.ToString("dd/MM/yyyy"));

            writer.WriteEndElement(); // ExtendedData

            // Point
            writer.WriteStartElement("Point");
            writer.WriteElementString("coordinates", 
                $"{filon.Longitude},{filon.Latitude},0");
            writer.WriteEndElement(); // Point

            writer.WriteEndElement(); // Placemark
        }

        /// <summary>
        /// G�n�re la description HTML d'un filon avec photo
        /// </summary>
        private string GenerateHtmlDescription(Filon filon, string filesDir)
        {
            var html = new StringBuilder();
            html.AppendLine("<![CDATA[");
            html.AppendLine("<div style='font-family: Arial; font-size: 12px;'>");

            // Photo si disponible
            if (!string.IsNullOrEmpty(filon.PhotoPath) && File.Exists(filon.PhotoPath))
            {
                try
                {
                    var photoFileName = $"photo_{filon.Id}{Path.GetExtension(filon.PhotoPath)}";
                    var photoDestPath = Path.Combine(filesDir, photoFileName);
                    File.Copy(filon.PhotoPath, photoDestPath, true);

                    html.AppendLine($"<img src='files/{photoFileName}' width='300' style='margin-bottom: 10px;'/><br/>");
                }
                catch
                {
                    // Ignorer si erreur de copie de photo
                }
            }

            // Informations
            html.AppendLine($"<b>Mati�re Principale:</b> {MineralColors.GetDisplayName(filon.MatierePrincipale)}<br/>");

            if (filon.MatieresSecondaires.Any())
            {
                var secondaires = string.Join(", ", filon.MatieresSecondaires.Select(m => MineralColors.GetDisplayName(m)));
                html.AppendLine($"<b>Mati�res Secondaires:</b> {secondaires}<br/>");
            }

            html.AppendLine($"<b>Statut:</b> {filon.Statut}<br/>");

            if (filon.AnneeAncrage.HasValue)
                html.AppendLine($"<b>Ann�e Ancrage:</b> {filon.AnneeAncrage}<br/>");

            if (filon.LambertX.HasValue && filon.LambertY.HasValue)
                html.AppendLine($"<b>Lambert 3:</b> X={filon.LambertX:F0}, Y={filon.LambertY:F0}<br/>");

            html.AppendLine($"<b>Coordonn�es GPS:</b> {filon.Latitude:F6}�, {filon.Longitude:F6}�<br/>");

            if (!string.IsNullOrEmpty(filon.Notes))
            {
                html.AppendLine($"<br/><b>Notes:</b><br/>{filon.Notes.Replace("\n", "<br/>")}<br/>");
            }

            html.AppendLine("</div>");
            html.AppendLine("]]>");

            return html.ToString();
        }

        /// <summary>
        /// Ajoute une donn�e dans ExtendedData
        /// </summary>
        private void AddData(XmlWriter writer, string name, string value)
        {
            writer.WriteStartElement("Data");
            writer.WriteAttributeString("name", name);
            writer.WriteElementString("value", value);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Convertit une couleur .NET en couleur KML (ABGR)
        /// </summary>
        private string ColorToKmlColor(Color color)
        {
            return $"ff{color.B:x2}{color.G:x2}{color.R:x2}";
        }

        /// <summary>
        /// Obtient des statistiques sur l'export
        /// </summary>
        public string GetExportSummary(List<Filon> filons, string outputPath)
        {
            var filonsWithCoords = filons.Count(f => f.Latitude.HasValue && f.Longitude.HasValue);
            var filonsWithPhotos = filons.Count(f => !string.IsNullOrEmpty(f.PhotoPath) && File.Exists(f.PhotoPath));
            var fileSize = new FileInfo(outputPath).Length;
            var fileSizeKb = fileSize / 1024;

            return $"?? Export KMZ R�ussi !\n\n" +
                   $"Fichier : {Path.GetFileName(outputPath)}\n" +
                   $"Taille : {fileSizeKb} Ko\n\n" +
                   $"?? {filonsWithCoords} filon(s) export�(s)\n" +
                   $"?? {filonsWithPhotos} photo(s) incluse(s)\n\n" +
                   $"Ouvrez le fichier avec Google Earth\n" +
                   $"pour visualiser vos filons !";
        }
    }
}
