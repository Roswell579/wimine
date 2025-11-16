namespace wmine.Models
{
    /// <summary>
    /// Types de fonds de carte disponibles
    /// </summary>
    public enum MapType
    {
        OpenStreetMap,
        GoogleMaps,          // NOUVEAU - Google Maps classique
        GoogleTerrain,       // NOUVEAU - Google Terrain
        OpenTopoMap,
        EsriSatellite,
        EsriWorldTopo,
        BingSatellite,
        GoogleSatellite,
        GoogleHybrid
    }

    /// <summary>
    /// Helper pour les noms d'affichage des cartes
    /// </summary>
    public static class MapTypeHelper
    {
        public static string GetDisplayName(this MapType mapType)
        {
            return mapType switch
            {
                MapType.OpenStreetMap => "OpenStreetMap (Standard)",
                MapType.GoogleMaps => "Google Maps",           // ? NOUVEAU
                MapType.GoogleTerrain => "Google Terrain",      // ? NOUVEAU
                MapType.OpenTopoMap => "OpenTopoMap (Relief)",
                MapType.EsriSatellite => "Esri Satellite",
                MapType.EsriWorldTopo => "Esri Topo Mondial",
                MapType.BingSatellite => "Bing Satellite",
                MapType.GoogleSatellite => "Google Satellite",
                MapType.GoogleHybrid => "Google Hybride",
                _ => mapType.ToString()
            };
        }

        public static string GetDescription(this MapType mapType)
        {
            return mapType switch
            {
                MapType.OpenStreetMap => "Carte classique open source avec routes et labels",
                MapType.GoogleMaps => "Carte routiére Google Maps classique",           // NOUVEAU
                MapType.GoogleTerrain => "Carte terrain Google avec relief",            // NOUVEAU
                MapType.OpenTopoMap => "Carte topographique avec relief et courbes de niveau",
                MapType.EsriSatellite => "Imagerie satellite haute résolution (open source)",
                MapType.EsriWorldTopo => "Carte topographique mondiale détaillée",
                MapType.BingSatellite => "Imagerie satellite Microsoft (haute résolution)",
                MapType.GoogleSatellite => "Imagerie satellite Google Earth",
                MapType.GoogleHybrid => "Satellite avec labels et routes",
                _ => ""
            };
        }

        public static bool IsAvailableOffline(this MapType mapType)
        {
            return mapType switch
            {
                MapType.OpenStreetMap => true,
                MapType.OpenTopoMap => true,
                MapType.EsriSatellite => true,
                _ => false
            };
        }
    }
}
