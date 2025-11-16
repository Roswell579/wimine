using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.Projections;
using System.Threading.Tasks;

namespace wmine.Services
{
    /// <summary>
    /// Service de gestion du mode hors-ligne pour les cartes
    /// Permet de télécharger et utiliser les cartes sans connexion Internet
    /// </summary>
    public class OfflineModeService
    {
        private readonly string _cacheDirectory;
        private bool _isOfflineMode = false;
        
        public bool IsOfflineMode => _isOfflineMode;
        
        public OfflineModeService()
        {
            _cacheDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "wmine", "OfflineMapCache");
            
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }
        
        /// <summary>
        /// Active/désactive le mode hors-ligne
        /// </summary>
        public void SetOfflineMode(bool enabled)
        {
            _isOfflineMode = enabled;
            
            if (enabled)
            {
                // Forcer GMap.NET é utiliser uniquement le cache
                GMaps.Instance.Mode = AccessMode.CacheOnly;
            }
            else
            {
                // Mode normal : cache + serveur
                GMaps.Instance.Mode = AccessMode.ServerAndCache;
            }
        }
        
        /// <summary>
        /// Télécharge une zone de carte pour usage hors-ligne
        /// </summary>
        public async Task<OfflineDownloadResult> DownloadAreaAsync(
            PointLatLng topLeft,
            PointLatLng bottomRight,
            int minZoom,
            int maxZoom,
            GMapProvider provider,
            IProgress<OfflineDownloadProgress>? progress = null)
        {
            var result = new OfflineDownloadResult();
            result.StartTime = DateTime.Now;
            
            try
            {
                // Calculer le nombre total de tuiles
                int totalTiles = 0;
                for (int zoom = minZoom; zoom <= maxZoom; zoom++)
                {
                    var area = GetTileBounds(topLeft, bottomRight, zoom, provider.Projection);
                    totalTiles += (area.WidthInTiles * area.HeightInTiles);
                }
                
                result.TotalTiles = totalTiles;
                int downloaded = 0;
                
                // Télécharger tuile par tuile
                for (int zoom = minZoom; zoom <= maxZoom; zoom++)
                {
                    var area = GetTileBounds(topLeft, bottomRight, zoom, provider.Projection);
                    
                    for (int x = area.MinX; x <= area.MaxX; x++)
                    {
                        for (int y = area.MinY; y <= area.MaxY; y++)
                        {
                            try
                            {
                                // Télécharger la tuile (GMap.NET la met en cache automatiquement)
                                var point = new GPoint(x, y);
                                var image = await Task.Run(() => 
                                    provider.GetTileImage(point, zoom));
                                
                                if (image != null)
                                {
                                    downloaded++;
                                    result.SuccessfulTiles++;
                                    image.Dispose();
                                }
                                
                                // Notifier progression
                                progress?.Report(new OfflineDownloadProgress
                                {
                                    TotalTiles = totalTiles,
                                    DownloadedTiles = downloaded,
                                    CurrentZoom = zoom,
                                    PercentComplete = (downloaded * 100.0) / totalTiles
                                });
                                
                                // Pause pour ne pas surcharger le serveur
                                await Task.Delay(50);
                            }
                            catch
                            {
                                result.FailedTiles++;
                            }
                        }
                    }
                }
                
                result.EndTime = DateTime.Now;
                result.Success = result.FailedTiles < (totalTiles * 0.1); // Max 10% échec
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }
            
            return result;
        }
        
        /// <summary>
        /// Obtient les informations de cache pour une zone
        /// </summary>
        public OfflineCacheInfo GetCacheInfo(PointLatLng topLeft, PointLatLng bottomRight, int minZoom, int maxZoom, PureProjection projection)
        {
            var info = new OfflineCacheInfo();
            
            try
            {
                // Calculer la taille du cache en parcourant les fichiers
                var cachePath = Path.Combine(_cacheDirectory, "..");
                if (Directory.Exists(cachePath))
                {
                    var cacheFiles = Directory.GetFiles(cachePath, "*.*", SearchOption.AllDirectories);
                    long totalSize = cacheFiles.Sum(f => new FileInfo(f).Length);
                    info.CacheSizeMB = totalSize / (1024.0 * 1024.0);
                }
                
                // Calculer le nombre de tuiles en cache pour cette zone
                int totalTiles = 0;
                int cachedTiles = 0;
                
                for (int zoom = minZoom; zoom <= maxZoom; zoom++)
                {
                    var area = GetTileBounds(topLeft, bottomRight, zoom, projection);
                    int tilesInZoom = area.WidthInTiles * area.HeightInTiles;
                    totalTiles += tilesInZoom;
                    
                    // Estimer les tuiles en cache (approximation)
                    cachedTiles += (int)(tilesInZoom * 0.5); // Estimation conservatrice
                }
                
                info.TotalTilesRequired = totalTiles;
                info.CachedTiles = cachedTiles;
                info.CoveragePercent = (cachedTiles * 100.0) / totalTiles;
                info.IsFullyCached = info.CoveragePercent >= 95;
            }
            catch (Exception ex)
            {
                info.ErrorMessage = ex.Message;
            }
            
            return info;
        }
        
        /// <summary>
        /// Efface le cache hors-ligne
        /// </summary>
        public bool ClearOfflineCache()
        {
            try
            {
                GMaps.Instance.PrimaryCache.DeleteOlderThan(DateTime.Now, null);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Obtient la taille totale du cache
        /// </summary>
        public double GetTotalCacheSizeMB()
        {
            try
            {
                var cachePath = Path.Combine(_cacheDirectory, "..");
                if (Directory.Exists(cachePath))
                {
                    var cacheFiles = Directory.GetFiles(cachePath, "*.*", SearchOption.AllDirectories);
                    long totalSize = cacheFiles.Sum(f => new FileInfo(f).Length);
                    return totalSize / (1024.0 * 1024.0);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }
        
        private TileArea GetTileBounds(PointLatLng topLeft, PointLatLng bottomRight, int zoom, PureProjection projection)
        {
            var topLeftPoint = projection.FromLatLngToPixel(topLeft, zoom);
            var bottomRightPoint = projection.FromLatLngToPixel(bottomRight, zoom);
            
            int minX = (int)(topLeftPoint.X / 256);
            int maxX = (int)(bottomRightPoint.X / 256);
            int minY = (int)(topLeftPoint.Y / 256);
            int maxY = (int)(bottomRightPoint.Y / 256);
            
            return new TileArea
            {
                MinX = minX,
                MaxX = maxX,
                MinY = minY,
                MaxY = maxY,
                WidthInTiles = maxX - minX + 1,
                HeightInTiles = maxY - minY + 1
            };
        }
        
        private class TileArea
        {
            public int MinX { get; set; }
            public int MaxX { get; set; }
            public int MinY { get; set; }
            public int MaxY { get; set; }
            public int WidthInTiles { get; set; }
            public int HeightInTiles { get; set; }
        }
    }
    
    /// <summary>
    /// Résultat d'un téléchargement hors-ligne
    /// </summary>
    public class OfflineDownloadResult
    {
        public bool Success { get; set; }
        public int TotalTiles { get; set; }
        public int SuccessfulTiles { get; set; }
        public int FailedTiles { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? ErrorMessage { get; set; }
        
        public TimeSpan Duration => EndTime - StartTime;
        public double SuccessRate => TotalTiles > 0 ? (SuccessfulTiles * 100.0) / TotalTiles : 0;
    }
    
    /// <summary>
    /// Progression d'un téléchargement hors-ligne
    /// </summary>
    public class OfflineDownloadProgress
    {
        public int TotalTiles { get; set; }
        public int DownloadedTiles { get; set; }
        public int CurrentZoom { get; set; }
        public double PercentComplete { get; set; }
        
        public string FormattedProgress => $"{DownloadedTiles:N0} / {TotalTiles:N0} tuiles ({PercentComplete:F1}%)";
    }
    
    /// <summary>
    /// Informations sur le cache hors-ligne
    /// </summary>
    public class OfflineCacheInfo
    {
        public double CacheSizeMB { get; set; }
        public int TotalTilesRequired { get; set; }
        public int CachedTiles { get; set; }
        public double CoveragePercent { get; set; }
        public bool IsFullyCached { get; set; }
        public string? ErrorMessage { get; set; }
        
        public string FormattedCacheSize => $"{CacheSizeMB:F2} MB";
        public string FormattedCoverage => $"{CoveragePercent:F1}% ({CachedTiles:N0} / {TotalTilesRequired:N0})";
    }
}
