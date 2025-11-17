using System.Text.Json;
using GMap.NET;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service de calcul d'itinéraires routiers
    /// Utilise OpenRouteService (gratuit, open-source)
    /// </summary>
    public class RouteService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        // Alternative : OSRM (OpenStreetMap Routing Machine) - 100% gratuit, pas de clé requise
        private const string OSRM_API = "https://router.project-osrm.org/route/v1";

        static RouteService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WMine-FilonLocator/1.0");
        }

        /// <summary>
        /// Calcule un itinéraire entre deux points
        /// </summary>
        public async Task<RouteInfo?> CalculateRouteAsync(
            PointLatLng start,
            PointLatLng end,
            TransportType transportType = TransportType.Car)
        {
            try
            {
                var profile = GetOSRMProfile(transportType);

                // Format OSRM : lon,lat (attention é l'ordre !)
                // IMPORTANT: Utiliser InvariantCulture pour forcer le POINT décimal
                var startLng = start.Lng.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                var startLat = start.Lat.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                var endLng = end.Lng.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                var endLat = end.Lat.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);

                var url = $"{OSRM_API}/{profile}/{startLng},{startLat};{endLng},{endLat}?overview=full&geometries=geojson&steps=true";

                System.Diagnostics.Debug.WriteLine($"=== APPEL OSRM ===");
                System.Diagnostics.Debug.WriteLine($"URL: {url}");

                var response = await _httpClient.GetAsync(url);

                System.Diagnostics.Debug.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Erreur OSRM: {errorContent}");
                    throw new HttpRequestException($"OSRM erreur {response.StatusCode}: {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON reéu (premiers 200 chars): {json.Substring(0, Math.Min(200, json.Length))}");

                var result = JsonSerializer.Deserialize<OSRMResponse>(json);

                if (result?.routes == null || result.routes.Length == 0)
                    return null;

                var route = result.routes[0];

                var routeInfo = new RouteInfo
                {
                    Start = start,
                    End = end,
                    TransportType = transportType,
                    DistanceKm = route.distance / 1000.0, // Conversion métres ? km
                    DurationMinutes = route.duration / 60.0 // Conversion secondes ? minutes
                };

                // décoder la géométrie (GeoJSON LineString)
                if (route.geometry?.coordinates != null)
                {
                    foreach (var coord in route.geometry.coordinates)
                    {
                        // GeoJSON : [lon, lat]
                        if (coord.Length >= 2)
                        {
                            routeInfo.Points.Add(new PointLatLng(coord[1], coord[0]));
                        }
                    }
                }

                // Extraire les instructions de navigation
                if (route.legs != null && route.legs.Length > 0)
                {
                    foreach (var leg in route.legs)
                    {
                        if (leg.steps != null)
                        {
                            foreach (var step in leg.steps)
                            {
                                if (!string.IsNullOrWhiteSpace(step.name) && step.name != "-")
                                {
                                    var instruction = $"?? {GetManeuverIcon(step.maneuver?.type)} " +
                                                    $"{step.name} ({(step.distance / 1000.0):F1} km)";
                                    routeInfo.Instructions.Add(instruction);
                                }
                            }
                        }
                    }
                }

                return routeInfo;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur calcul d'itinéraire: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Calcule un itinéraire depuis la position actuelle (GPS simulé)
        /// </summary>
        public async Task<RouteInfo?> CalculateRouteFromCurrentLocationAsync(
            PointLatLng destination,
            TransportType transportType = TransportType.Car)
        {
            // Position par défaut : Centre du Var (Toulon)
            var currentLocation = new PointLatLng(43.1242, 5.9280);

            return await CalculateRouteAsync(currentLocation, destination, transportType);
        }

        private string GetOSRMProfile(TransportType transportType)
        {
            return transportType switch
            {
                TransportType.Car => "driving",
                TransportType.Walking => "walking",   // CORRIGÉ
                TransportType.Cycling => "cycling",   // CORRIGÉ
                _ => "driving"
            };
        }

        private string GetManeuverIcon(string? maneuverType)
        {
            if (string.IsNullOrEmpty(maneuverType))
                return "??";

            return maneuverType.ToLower() switch
            {
                "turn" => "??",
                "new name" => "??",
                "depart" => "??",
                "arrive" => "??",
                "merge" => "??",
                "on ramp" => "??",
                "off ramp" => "??",
                "fork" => "??",
                "end of road" => "??",
                "continue" => "??",
                "roundabout" => "??",
                "rotary" => "??",
                "notification" => "??",
                _ => "??"
            };
        }

        // Classes pour la désérialisation JSON OSRM
        private class OSRMResponse
        {
            public string? code { get; set; }
            public OSRMRoute[]? routes { get; set; }
        }

        private class OSRMRoute
        {
            public double distance { get; set; } // en métres
            public double duration { get; set; } // en secondes
            public OSRMGeometry? geometry { get; set; }
            public OSRMLeg[]? legs { get; set; }
        }

        private class OSRMGeometry
        {
            public string? type { get; set; } // "LineString"
            public double[][]? coordinates { get; set; } // [[lon, lat], ...]
        }

        private class OSRMLeg
        {
            public OSRMStep[]? steps { get; set; }
            public double distance { get; set; }
            public double duration { get; set; }
        }

        private class OSRMStep
        {
            public string? name { get; set; }
            public double distance { get; set; }
            public double duration { get; set; }
            public OSRMManeuver? maneuver { get; set; }
        }

        private class OSRMManeuver
        {
            public string? type { get; set; }
            public double[]? location { get; set; } // [lon, lat]
        }
    }
}
