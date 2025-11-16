using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service pour gérer les différents fonds de carte
    /// </summary>
    public class MapProviderService
    {
        private readonly GMapControl _mapControl;

        public MapProviderService(GMapControl mapControl)
        {
            _mapControl = mapControl;
        }

        /// <summary>
        /// Change le fond de carte
        /// </summary>
        public void SetMapType(Models.MapType mapType)
        {
            try
            {
                // Sauvegarder position et zoom
                var currentPosition = _mapControl.Position;
                var currentZoom = _mapControl.Zoom;

                // Changer le provider
                _mapControl.MapProvider = GetProvider(mapType);

                // Restaurer position et zoom
                _mapControl.Position = currentPosition;
                _mapControl.Zoom = currentZoom;

                // Gérer l'overlay géologique si nécessaire
                // (Désactivé - fonctionnalité BRGM supprimée)

                _mapControl.Refresh();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du changement de carte: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retourne le provider GMap.NET correspondant
        /// </summary>
        private GMapProvider GetProvider(Models.MapType mapType)
        {
            return mapType switch
            {
                Models.MapType.OpenStreetMap => GMapProviders.OpenStreetMap,
                Models.MapType.GoogleMaps => GMapProviders.GoogleMap,
                Models.MapType.GoogleTerrain => GMapProviders.GoogleTerrainMap,
                Models.MapType.OpenTopoMap => OpenTopoMapProvider.Instance,
                Models.MapType.EsriSatellite => GMapProviders.BingHybridMap,
                Models.MapType.EsriWorldTopo => GMapProviders.OpenStreetMap,
                Models.MapType.BingSatellite => GMapProviders.BingSatelliteMap,
                Models.MapType.GoogleSatellite => GMapProviders.GoogleSatelliteMap,
                Models.MapType.GoogleHybrid => GMapProviders.GoogleHybridMap,
                _ => GMapProviders.OpenStreetMap
            };
        }

        /// <summary>
        /// Vérifie si un type de carte est disponible
        /// </summary>
        public static bool IsMapTypeAvailable(Models.MapType mapType)
        {
            return mapType switch
            {
                Models.MapType.GoogleSatellite => true,
                Models.MapType.GoogleHybrid => true,
                Models.MapType.BingSatellite => true,
                _ => true
            };
        }
    }

    /// <summary>
    /// Provider custom pour OpenTopoMap
    /// </summary>
    public class OpenTopoMapProvider : GMapProvider
    {
        public static readonly OpenTopoMapProvider Instance = new OpenTopoMapProvider();

        private OpenTopoMapProvider()
        {
            MaxZoom = 17;
            MinZoom = 1;
        }

        public override Guid Id => new Guid("A5B8F6E7-2D4C-4E9A-8B1C-3F7A9D6E5C2B");
        public override string Name => "OpenTopoMap";
        public override PureProjection Projection => GMap.NET.Projections.MercatorProjection.Instance;
        public override GMapProvider[] Overlays => null;

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, string.Empty);
            return GetTileImageUsingHttp(url);
        }

        private string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            // OpenTopoMap tile server
            return string.Format("https://tile.opentopomap.org/{0}/{1}/{2}.png", zoom, pos.X, pos.Y);
        }
    }
}
