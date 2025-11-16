using GMap.NET;
using System.Net.Http;
using System.Text.Json;

namespace wmine.Services
{
    /// <summary>
    /// Service de géocodage pour rechercher des lieux par nom
    /// Utilise l'API Nominatim d'OpenStreetMap (gratuite, pas de clé requise)
    /// </summary>
    public class GeocodingService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        
        private const string NOMINATIM_API = "https://nominatim.openstreetmap.org/search";
        
        // Cache en mémoire pour les recherches fréquentes
        private static readonly Dictionary<string, (DateTime timestamp, List<GeocodingResult> results)> _cache 
            = new Dictionary<string, (DateTime, List<GeocodingResult>)>();
        private static readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24);
        
        static GeocodingService()
        {
            // User-Agent obligatoire pour Nominatim
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WMine-FilonLocator/1.0");
        }
        
        /// <summary>
        /// Recherche un lieu par nom
        /// </summary>
        public async Task<List<GeocodingResult>> SearchPlaceAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<GeocodingResult>();
            
            // Normaliser la requéte pour le cache
            var cacheKey = query.Trim().ToLowerInvariant();
            
            // Vérifier le cache
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                if (DateTime.Now - cached.timestamp < _cacheExpiration)
                {
                    return cached.results;
                }
                else
                {
                    _cache.Remove(cacheKey);
                }
            }
            
            try
            {
                // Ajouter "France" ou "Var" pour des résultats plus pertinents
                var enhancedQuery = $"{query}, Var, France";
                
                var url = $"{NOMINATIM_API}?q={Uri.EscapeDataString(enhancedQuery)}&format=json&limit=10";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<NominatimResult>>(json);
                
                var geocodingResults = results?.Select(r => new GeocodingResult
                {
                    DisplayName = r.display_name ?? "Inconnu",
                    Latitude = double.Parse(r.lat ?? "0"),
                    Longitude = double.Parse(r.lon ?? "0"),
                    Type = r.type ?? "lieu",
                    Importance = r.importance ?? 0
                })
                .OrderByDescending(r => r.Importance)
                .ToList() ?? new List<GeocodingResult>();
                
                // Mettre en cache
                _cache[cacheKey] = (DateTime.Now, geocodingResults);
                
                return geocodingResults;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la recherche géographique: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Recherche inverse : obtenir le nom d'un lieu é partir de coordonnées
        /// </summary>
        public async Task<string?> ReverseGeocodeAsync(double latitude, double longitude)
        {
            try
            {
                var url = $"https://nominatim.openstreetmap.org/reverse?lat={latitude}&lon={longitude}&format=json";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<NominatimResult>(json);
                
                return result?.display_name;
            }
            catch
            {
                return null;
            }
        }
        
        // Classes pour la désérialisation JSON de Nominatim
        private class NominatimResult
        {
            public string? display_name { get; set; }
            public string? lat { get; set; }
            public string? lon { get; set; }
            public string? type { get; set; }
            public double? importance { get; set; }
        }
    }
    
    /// <summary>
    /// Résultat d'une recherche géographique
    /// </summary>
    public class GeocodingResult
    {
        public string DisplayName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Type { get; set; } = string.Empty;
        public double Importance { get; set; }
        
        public PointLatLng ToPointLatLng() => new PointLatLng(Latitude, Longitude);
        
        public override string ToString() => DisplayName;
    }
}
