using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Runtime.Serialization; // ? NOUVEAU pour FormatterServices
using GMap.NET;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service de g�olocalisation des photos
    /// Lit et �crit les donn�es EXIF GPS des images
    /// </summary>
    public class PhotoGeotagService
    {
        // IDs des propri�t�s EXIF GPS
        private const int PropertyTagGpsLatitudeRef = 0x0001;
        private const int PropertyTagGpsLatitude = 0x0002;
        private const int PropertyTagGpsLongitudeRef = 0x0003;
        private const int PropertyTagGpsLongitude = 0x0004;

        /// <summary>
        /// Extrait les coordonn�es GPS d'une photo
        /// Retourne null si la photo n'a pas de donn�es GPS
        /// </summary>
        public (double? latitude, double? longitude) ExtractGPS(string photoPath)
        {
            if (!File.Exists(photoPath))
                return (null, null);

            try
            {
                using var img = Image.FromFile(photoPath);
                
                // V�rifier si l'image a des propri�t�s EXIF
                if (!img.PropertyIdList.Contains(PropertyTagGpsLatitude) ||
                    !img.PropertyIdList.Contains(PropertyTagGpsLongitude))
                {
                    return (null, null);
                }

                // Lire latitude
                var latRef = GetPropertyString(img, PropertyTagGpsLatitudeRef);
                var latData = GetPropertyRational(img, PropertyTagGpsLatitude);
                var latitude = ConvertToDecimal(latData, latRef);

                // Lire longitude
                var lonRef = GetPropertyString(img, PropertyTagGpsLongitudeRef);
                var lonData = GetPropertyRational(img, PropertyTagGpsLongitude);
                var longitude = ConvertToDecimal(lonData, lonRef);

                return (latitude, longitude);
            }
            catch
            {
                return (null, null);
            }
        }

        /// <summary>
        /// �crit les coordonn�es GPS dans une photo
        /// Note: Cr�e une nouvelle image car on ne peut pas modifier l'EXIF directement
        /// </summary>
        public bool WriteGPS(string photoPath, double latitude, double longitude)
        {
            if (!File.Exists(photoPath))
                return false;

            try
            {
                using var img = Image.FromFile(photoPath);
                using var newImg = new Bitmap(img);
                
                // Latitude
                var latRef = latitude >= 0 ? "N" : "S";
                var latRational = ConvertToRational(Math.Abs(latitude));
                SetProperty(newImg, PropertyTagGpsLatitudeRef, Encoding.ASCII.GetBytes(latRef + "\0"));
                SetProperty(newImg, PropertyTagGpsLatitude, latRational);

                // Longitude
                var lonRef = longitude >= 0 ? "E" : "W";
                var lonRational = ConvertToRational(Math.Abs(longitude));
                SetProperty(newImg, PropertyTagGpsLongitudeRef, Encoding.ASCII.GetBytes(lonRef + "\0"));
                SetProperty(newImg, PropertyTagGpsLongitude, lonRational);

                // Cr�er un backup de l'original
                var backupPath = photoPath + ".backup";
                File.Copy(photoPath, backupPath, true);

                // Sauvegarder la nouvelle image
                var tempPath = photoPath + ".temp";
                newImg.Save(tempPath, ImageFormat.Jpeg);
                
                // Remplacer l'original
                File.Delete(photoPath);
                File.Move(tempPath, photoPath);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Associe automatiquement des photos aux filons par proximit� GPS
        /// </summary>
        public List<(string photoPath, Filon filon, double distance)> AutoMatchPhotosToFilons(
            List<string> photoPaths, 
            List<Filon> filons,
            double maxDistanceKm = 0.5)
        {
            var matches = new List<(string, Filon, double)>();
            var measurementService = new MeasurementService();

            foreach (var photoPath in photoPaths)
            {
                var (lat, lon) = ExtractGPS(photoPath);
                
                if (!lat.HasValue || !lon.HasValue)
                    continue;

                var photoPoint = new PointLatLng(lat.Value, lon.Value);

                // Trouver le filon le plus proche
                var (nearestFilon, distance) = measurementService.FindNearestFilon(photoPoint, filons);

                if (nearestFilon != null && distance <= maxDistanceKm)
                {
                    matches.Add((photoPath, nearestFilon, distance));
                }
            }

            return matches.OrderBy(m => m.Item3).ToList(); // m.Item3 = distance
        }

        /// <summary>
        /// Importe des photos depuis un dossier et les associe aux filons
        /// </summary>
        public async Task<ImportPhotosResult> ImportPhotosFromFolderAsync(
            string folderPath, 
            List<Filon> filons,
            double maxDistanceKm = 0.5)
        {
            var result = new ImportPhotosResult();

            if (!Directory.Exists(folderPath))
            {
                result.ErrorMessage = "Le dossier n'existe pas.";
                return result;
            }

            // Trouver toutes les photos
            var extensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" };
            var photoPaths = new List<string>();

            foreach (var ext in extensions)
            {
                photoPaths.AddRange(Directory.GetFiles(folderPath, ext, SearchOption.TopDirectoryOnly));
            }

            result.TotalPhotos = photoPaths.Count;

            // Analyser chaque photo
            foreach (var photoPath in photoPaths)
            {
                try
                {
                    var (lat, lon) = ExtractGPS(photoPath);

                    if (!lat.HasValue || !lon.HasValue)
                    {
                        result.PhotosWithoutGPS.Add(photoPath);
                        continue;
                    }

                    result.PhotosWithGPS.Add(photoPath);

                    // Trouver le filon correspondant
                    var photoPoint = new PointLatLng(lat.Value, lon.Value);
                    var measurementService = new MeasurementService();
                    var (nearestFilon, distance) = measurementService.FindNearestFilon(photoPoint, filons);

                    if (nearestFilon != null && distance <= maxDistanceKm)
                    {
                        result.MatchedPhotos.Add((photoPath, nearestFilon, distance));
                        
                        // Copier la photo dans le dossier du filon
                        var filonPhotoDir = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                            "WMine", "Filons", nearestFilon.Id.ToString());

                        if (!Directory.Exists(filonPhotoDir))
                            Directory.CreateDirectory(filonPhotoDir);

                        var destPath = Path.Combine(filonPhotoDir, Path.GetFileName(photoPath));
                        File.Copy(photoPath, destPath, false);
                    }
                    else
                    {
                        result.UnmatchedPhotos.Add((photoPath, lat.Value, lon.Value));
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add((photoPath, ex.Message));
                }
            }

            return result;
        }

        /// <summary>
        /// Obtient un r�sum� texte de l'import
        /// </summary>
        public string GetImportSummary(ImportPhotosResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"?? Import de Photos avec GPS");
            sb.AppendLine($"???????????????????????????????");
            sb.AppendLine();
            sb.AppendLine($"Total photos analys�es: {result.TotalPhotos}");
            sb.AppendLine($"  ? Avec GPS: {result.PhotosWithGPS.Count}");
            sb.AppendLine($"  ? Sans GPS: {result.PhotosWithoutGPS.Count}");
            sb.AppendLine();
            sb.AppendLine($"?? Association aux filons:");
            sb.AppendLine($"  ? Match�es: {result.MatchedPhotos.Count}");
            sb.AppendLine($"  ?? Non match�es: {result.UnmatchedPhotos.Count}");
            sb.AppendLine($"  ? Erreurs: {result.Errors.Count}");

            if (result.MatchedPhotos.Any())
            {
                sb.AppendLine();
                sb.AppendLine($"Photos associ�es:");
                foreach (var (photo, filon, distance) in result.MatchedPhotos.Take(10))
                {
                    var fileName = Path.GetFileName(photo);
                    sb.AppendLine($"  � {fileName} ? {filon.Nom} ({distance:F2} km)");
                }

                if (result.MatchedPhotos.Count > 10)
                    sb.AppendLine($"  ... et {result.MatchedPhotos.Count - 10} autres");
            }

            return sb.ToString();
        }

        // M�thodes priv�es pour manipuler EXIF

        private string GetPropertyString(Image img, int propertyId)
        {
            try
            {
                var item = img.GetPropertyItem(propertyId);
                return Encoding.ASCII.GetString(item.Value).TrimEnd('\0');
            }
            catch
            {
                return string.Empty;
            }
        }

        private double[] GetPropertyRational(Image img, int propertyId)
        {
            try
            {
                var item = img.GetPropertyItem(propertyId);
                var values = new double[3];

                for (int i = 0; i < 3; i++)
                {
                    var numerator = BitConverter.ToUInt32(item.Value, i * 8);
                    var denominator = BitConverter.ToUInt32(item.Value, i * 8 + 4);
                    values[i] = denominator > 0 ? (double)numerator / denominator : 0;
                }

                return values;
            }
            catch
            {
                return new double[3];
            }
        }

        private double ConvertToDecimal(double[] rational, string reference)
        {
            var degrees = rational[0];
            var minutes = rational[1];
            var seconds = rational[2];

            var decimal_degrees = degrees + (minutes / 60.0) + (seconds / 3600.0);

            if (reference == "S" || reference == "W")
                decimal_degrees = -decimal_degrees;

            return decimal_degrees;
        }

        private byte[] ConvertToRational(double value)
        {
            var degrees = (int)value;
            var minutes = (int)((value - degrees) * 60);
            var seconds = ((value - degrees) * 60 - minutes) * 60;

            var data = new byte[24];

            // Degrees
            BitConverter.GetBytes((uint)degrees).CopyTo(data, 0);
            BitConverter.GetBytes((uint)1).CopyTo(data, 4);

            // Minutes
            BitConverter.GetBytes((uint)minutes).CopyTo(data, 8);
            BitConverter.GetBytes((uint)1).CopyTo(data, 12);

            // Seconds
            BitConverter.GetBytes((uint)(seconds * 1000)).CopyTo(data, 16);
            BitConverter.GetBytes((uint)1000).CopyTo(data, 20);

            return data;
        }

        private void SetProperty(Image img, int propertyId, byte[] value)
        {
            try
            {
                var item = (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));
                item.Id = propertyId;
                item.Type = propertyId == PropertyTagGpsLatitudeRef || propertyId == PropertyTagGpsLongitudeRef ? (short)2 : (short)5;
                item.Len = value.Length;
                item.Value = value;
                img.SetPropertyItem(item);
            }
            catch
            {
                // Ignorer si impossible de d�finir la propri�t�
            }
        }
    }

    /// <summary>
    /// R�sultat de l'import de photos avec g�olocalisation
    /// </summary>
    public class ImportPhotosResult
    {
        public int TotalPhotos { get; set; }
        public List<string> PhotosWithGPS { get; set; } = new();
        public List<string> PhotosWithoutGPS { get; set; } = new();
        public List<(string photoPath, Filon filon, double distance)> MatchedPhotos { get; set; } = new();
        public List<(string photoPath, double lat, double lon)> UnmatchedPhotos { get; set; } = new();
        public List<(string photoPath, string error)> Errors { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}
