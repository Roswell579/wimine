using System.Drawing.Imaging;

namespace wmine.Core.Services
{
    /// <summary>
    /// Service de gestion des photos pour les filons
    /// </summary>
    public class PhotoManager
    {
        private readonly string _basePhotoDirectory;
        private readonly int _thumbnailSize = 200;
        private readonly int _maxPhotoSize = 1920;

        public PhotoManager(string? baseDirectory = null)
        {
            _basePhotoDirectory = baseDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "wmine", "Photos");

            EnsureDirectoryExists(_basePhotoDirectory);
        }

        /// <summary>
        /// Ajoute une photo pour un filon
        /// </summary>
        public async Task<string> AddPhotoAsync(string sourcePath, int filonId)
        {
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Le fichier source n'existe pas", sourcePath);
            }

            var filonFolder = Path.Combine(_basePhotoDirectory, $"Filon_{filonId}");
            EnsureDirectoryExists(filonFolder);

            var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(sourcePath)}";
            var destinationPath = Path.Combine(filonFolder, fileName);

            // Copier et optimiser l'image
            await OptimizeAndSaveImageAsync(sourcePath, destinationPath);

            // Créer la miniature
            await CreateThumbnailAsync(destinationPath, filonFolder);

            // Extraire les métadonnées EXIF si disponibles
            ExtractExifData(sourcePath, out var latitude, out var longitude, out var dateTaken);

            return filonFolder;
        }

        /// <summary>
        /// Optimise une image et la sauvegarde
        /// </summary>
        private async Task OptimizeAndSaveImageAsync(string sourcePath, string destinationPath)
        {
            await Task.Run(() =>
            {
                using (var originalImage = Image.FromFile(sourcePath))
                {
                    var needsResize = originalImage.Width > _maxPhotoSize || originalImage.Height > _maxPhotoSize;

                    if (needsResize)
                    {
                        var ratio = Math.Min(
                            (double)_maxPhotoSize / originalImage.Width,
                            (double)_maxPhotoSize / originalImage.Height
                        );

                        var newWidth = (int)(originalImage.Width * ratio);
                        var newHeight = (int)(originalImage.Height * ratio);

                        using (var resizedImage = new Bitmap(newWidth, newHeight))
                        using (var graphics = Graphics.FromImage(resizedImage))
                        {
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                            graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                            resizedImage.Save(destinationPath, ImageFormat.Jpeg);
                        }
                    }
                    else
                    {
                        File.Copy(sourcePath, destinationPath, true);
                    }
                }
            });
        }

        /// <summary>
        /// Crée une miniature de l'image
        /// </summary>
        private async Task CreateThumbnailAsync(string imagePath, string thumbnailFolder)
        {
            await Task.Run(() =>
            {
                var thumbnailPath = Path.Combine(thumbnailFolder, "thumbnails");
                EnsureDirectoryExists(thumbnailPath);

                var thumbnailFile = Path.Combine(
                    thumbnailPath,
                    $"thumb_{Path.GetFileName(imagePath)}"
                );

                using (var originalImage = Image.FromFile(imagePath))
                {
                    var ratio = Math.Min(
                        (double)_thumbnailSize / originalImage.Width,
                        (double)_thumbnailSize / originalImage.Height
                    );

                    var newWidth = (int)(originalImage.Width * ratio);
                    var newHeight = (int)(originalImage.Height * ratio);

                    using (var thumbnail = new Bitmap(newWidth, newHeight))
                    using (var graphics = Graphics.FromImage(thumbnail))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                        thumbnail.Save(thumbnailFile, ImageFormat.Jpeg);
                    }
                }
            });
        }

        /// <summary>
        /// Extrait les métadonnées EXIF d'une image
        /// </summary>
        private void ExtractExifData(string imagePath, out double? latitude, out double? longitude, out DateTime? dateTaken)
        {
            latitude = null;
            longitude = null;
            dateTaken = null;

            try
            {
                using (var image = Image.FromFile(imagePath))
                {
                    // Date de prise de vue (PropertyTagDateTime = 0x0132)
                    if (image.PropertyIdList.Contains(0x0132))
                    {
                        var dateBytes = image.GetPropertyItem(0x0132);
                        var dateString = System.Text.Encoding.ASCII.GetString(dateBytes.Value).TrimEnd('\0');
                        if (DateTime.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var date))
                        {
                            dateTaken = date;
                        }
                    }

                    // GPS Latitude et Longitude (codes EXIF GPS)
                    // PropertyTagGpsLatitude = 0x0002
                    // PropertyTagGpsLongitude = 0x0004
                    // Note: L'extraction compléte des coordonnées GPS nécessiterait plus de logique
                    // pour convertir les valeurs rationnelles EXIF en degrés décimaux
                }
            }
            catch
            {
                // Si erreur lecture EXIF, on ignore simplement
            }
        }

        /// <summary>
        /// Obtient toutes les photos d'un filon
        /// </summary>
        public List<string> GetFilonPhotos(int filonId)
        {
            var filonFolder = Path.Combine(_basePhotoDirectory, $"Filon_{filonId}");
            
            if (!Directory.Exists(filonFolder))
            {
                return new List<string>();
            }

            return Directory.GetFiles(filonFolder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => !f.Contains("thumbnails") && IsImageFile(f))
                .OrderByDescending(f => File.GetCreationTime(f))
                .ToList();
        }

        /// <summary>
        /// Obtient les miniatures des photos d'un filon
        /// </summary>
        public List<string> GetFilonThumbnails(int filonId)
        {
            var thumbnailFolder = Path.Combine(_basePhotoDirectory, $"Filon_{filonId}", "thumbnails");
            
            if (!Directory.Exists(thumbnailFolder))
            {
                return new List<string>();
            }

            return Directory.GetFiles(thumbnailFolder, "thumb_*.*")
                .OrderByDescending(f => File.GetCreationTime(f))
                .ToList();
        }

        /// <summary>
        /// Supprime une photo
        /// </summary>
        public void DeletePhoto(string photoPath)
        {
            if (File.Exists(photoPath))
            {
                File.Delete(photoPath);

                // Supprimer aussi la miniature associée
                var fileName = Path.GetFileName(photoPath);
                var directory = Path.GetDirectoryName(photoPath);
                var thumbnailPath = Path.Combine(directory!, "thumbnails", $"thumb_{fileName}");
                
                if (File.Exists(thumbnailPath))
                {
                    File.Delete(thumbnailPath);
                }
            }
        }

        /// <summary>
        /// Supprime toutes les photos d'un filon
        /// </summary>
        public void DeleteAllFilonPhotos(int filonId)
        {
            var filonFolder = Path.Combine(_basePhotoDirectory, $"Filon_{filonId}");
            
            if (Directory.Exists(filonFolder))
            {
                Directory.Delete(filonFolder, true);
            }
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private bool IsImageFile(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension == ".jpg" || extension == ".jpeg" || 
                   extension == ".png" || extension == ".bmp" || 
                   extension == ".gif";
        }
    }
}
