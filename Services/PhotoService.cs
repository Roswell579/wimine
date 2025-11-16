using System.Diagnostics;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service de gestion des photos de minéraux et filons
    /// </summary>
    public class PhotoService
    {
        private readonly string _photosBasePath;
        private readonly string _mineralsPhotosPath;
        private readonly string _filonsPhotosPath;

        public PhotoService()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _photosBasePath = Path.Combine(localAppData, "WMine", "Photos");
            _mineralsPhotosPath = Path.Combine(_photosBasePath, "Minerals");
            _filonsPhotosPath = Path.Combine(_photosBasePath, "Filons");

            EnsureDirectoriesExist();
        }

        private void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(_photosBasePath);
            Directory.CreateDirectory(_mineralsPhotosPath);
            Directory.CreateDirectory(_filonsPhotosPath);
        }

        /// <summary>
        /// Obtient le chemin de la photo d'un minéral
        /// </summary>
        public string? GetMineralPhotoPath(MineralType mineralType)
        {
            var fileName = $"{mineralType}.jpg";
            var fullPath = Path.Combine(_mineralsPhotosPath, fileName);

            if (File.Exists(fullPath))
                return fullPath;

            // Chercher aussi en .png
            fileName = $"{mineralType}.png";
            fullPath = Path.Combine(_mineralsPhotosPath, fileName);

            if (File.Exists(fullPath))
                return fullPath;

            return null;
        }

        /// <summary>
        /// Vérifie si une photo existe pour un minéral
        /// </summary>
        public bool HasMineralPhoto(MineralType mineralType)
        {
            return GetMineralPhotoPath(mineralType) != null;
        }

        /// <summary>
        /// Sauvegarde une photo pour un minéral
        /// </summary>
        public string SaveMineralPhoto(MineralType mineralType, string sourceFilePath)
        {
            var extension = Path.GetExtension(sourceFilePath);
            var fileName = $"{mineralType}{extension}";
            var destinationPath = Path.Combine(_mineralsPhotosPath, fileName);

            File.Copy(sourceFilePath, destinationPath, overwrite: true);
            return destinationPath;
        }

        /// <summary>
        /// Obtient toutes les photos d'un filon
        /// </summary>
        public List<string> GetFilonPhotos(int filonId)
        {
            var filonFolder = Path.Combine(_filonsPhotosPath, filonId.ToString());

            if (!Directory.Exists(filonFolder))
                return new List<string>();

            return Directory.GetFiles(filonFolder, "*.*")
                .Where(f => IsImageFile(f))
                .OrderBy(f => f)
                .ToList();
        }

        /// <summary>
        /// Ajoute une photo é un filon
        /// </summary>
        public string AddFilonPhoto(int filonId, string sourceFilePath)
        {
            var filonFolder = Path.Combine(_filonsPhotosPath, filonId.ToString());
            Directory.CreateDirectory(filonFolder);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var extension = Path.GetExtension(sourceFilePath);
            var fileName = $"photo_{timestamp}{extension}";
            var destinationPath = Path.Combine(filonFolder, fileName);

            File.Copy(sourceFilePath, destinationPath);
            return destinationPath;
        }

        /// <summary>
        /// Supprime une photo d'un filon
        /// </summary>
        public void DeleteFilonPhoto(string photoPath)
        {
            if (File.Exists(photoPath))
            {
                File.Delete(photoPath);
            }
        }

        /// <summary>
        /// Ouvre une photo dans la visionneuse par défaut
        /// </summary>
        public void OpenPhoto(string photoPath)
        {
            if (File.Exists(photoPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = photoPath,
                    UseShellExecute = true
                });
            }
        }

        /// <summary>
        /// Crée une miniature d'une image
        /// </summary>
        public Image? CreateThumbnail(string imagePath, int width, int height)
        {
            if (!File.Exists(imagePath))
                return null;

            try
            {
                using (var original = Image.FromFile(imagePath))
                {
                    var thumbnail = new Bitmap(width, height);
                    using (var graphics = Graphics.FromImage(thumbnail))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                        // Calculer le ratio pour conserver les proportions
                        float ratioX = (float)width / original.Width;
                        float ratioY = (float)height / original.Height;
                        float ratio = Math.Min(ratioX, ratioY);

                        int newWidth = (int)(original.Width * ratio);
                        int newHeight = (int)(original.Height * ratio);

                        int posX = (width - newWidth) / 2;
                        int posY = (height - newHeight) / 2;

                        graphics.Clear(Color.Transparent);
                        graphics.DrawImage(original, posX, posY, newWidth, newHeight);
                    }
                    return thumbnail;
                }
            }
            catch
            {
                return null;
            }
        }

        private bool IsImageFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".jpg" || extension == ".jpeg" || 
                   extension == ".png" || extension == ".bmp" || 
                   extension == ".gif";
        }

        /// <summary>
        /// Ouvre le dossier des photos d'un filon
        /// </summary>
        public void OpenFilonPhotosFolder(int filonId)
        {
            var filonFolder = Path.Combine(_filonsPhotosPath, filonId.ToString());
            Directory.CreateDirectory(filonFolder);

            Process.Start(new ProcessStartInfo
            {
                FileName = filonFolder,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Ouvre le dossier des photos de minéraux
        /// </summary>
        public void OpenMineralsPhotosFolder()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _mineralsPhotosPath,
                UseShellExecute = true
            });
        }
    }
}
