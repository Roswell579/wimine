using GMap.NET;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service de mesure de distances et surfaces sur la carte
    /// Utilise la formule de Haversine pour les calculs GPS
    /// </summary>
    public class MeasurementService
    {
        private const double EARTH_RADIUS_KM = 6371.0;

        /// <summary>
        /// Calcule la distance entre deux points GPS en kilom�tres
        /// Utilise la formule de Haversine
        /// </summary>
        public double CalculateDistance(PointLatLng point1, PointLatLng point2)
        {
            return CalculateDistance(point1.Lat, point1.Lng, point2.Lat, point2.Lng);
        }

        /// <summary>
        /// Calcule la distance entre deux coordonn�es GPS
        /// </summary>
        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Convertir en radians
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            lat1 = DegreesToRadians(lat1);
            lat2 = DegreesToRadians(lat2);

            // Formule de Haversine
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) *
                    Math.Cos(lat1) * Math.Cos(lat2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EARTH_RADIUS_KM * c;
        }

        /// <summary>
        /// Calcule la distance entre plusieurs points (chemin)
        /// </summary>
        public double CalculatePathDistance(List<PointLatLng> points)
        {
            if (points == null || points.Count < 2)
                return 0;

            double totalDistance = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                totalDistance += CalculateDistance(points[i], points[i + 1]);
            }

            return totalDistance;
        }

        /// <summary>
        /// Calcule la surface approximative d'un polygone en km�
        /// Utilise la m�thode des trap�zo�des
        /// </summary>
        public double CalculatePolygonArea(List<PointLatLng> points)
        {
            if (points == null || points.Count < 3)
                return 0;

            double area = 0;
            int n = points.Count;

            for (int i = 0; i < n; i++)
            {
                var p1 = points[i];
                var p2 = points[(i + 1) % n];

                var x1 = DegreesToRadians(p1.Lng);
                var y1 = DegreesToRadians(p1.Lat);
                var x2 = DegreesToRadians(p2.Lng);
                var y2 = DegreesToRadians(p2.Lat);

                area += (x2 - x1) * (2 + Math.Sin(y1) + Math.Sin(y2));
            }

            area = Math.Abs(area * EARTH_RADIUS_KM * EARTH_RADIUS_KM / 2);

            return area;
        }

        /// <summary>
        /// Calcule le cap (azimut) entre deux points en degr�s (0-360)
        /// 0� = Nord, 90� = Est, 180� = Sud, 270� = Ouest
        /// </summary>
        public double CalculateBearing(PointLatLng point1, PointLatLng point2)
        {
            var lat1 = DegreesToRadians(point1.Lat);
            var lat2 = DegreesToRadians(point2.Lat);
            var dLon = DegreesToRadians(point2.Lng - point1.Lng);

            var y = Math.Sin(dLon) * Math.Cos(lat2);
            var x = Math.Cos(lat1) * Math.Sin(lat2) -
                    Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

            var bearing = Math.Atan2(y, x);
            bearing = RadiansToDegrees(bearing);
            bearing = (bearing + 360) % 360; // Normaliser entre 0-360

            return bearing;
        }

        /// <summary>
        /// Retourne une description textuelle de la distance
        /// Ex: "1.5 km", "350 m", "2.8 km"
        /// </summary>
        public string GetReadableDistance(double distanceKm)
        {
            if (distanceKm < 1.0)
            {
                return $"{(int)(distanceKm * 1000)} m";
            }
            else
            {
                return $"{distanceKm:F1} km";
            }
        }

        /// <summary>
        /// Retourne une description textuelle de la surface
        /// Ex: "2.5 km�", "5000 m�"
        /// </summary>
        public string GetReadableArea(double areaKm2)
        {
            if (areaKm2 < 0.01)
            {
                return $"{(int)(areaKm2 * 1_000_000)} m�";
            }
            else
            {
                return $"{areaKm2:F2} km�";
            }
        }

        /// <summary>
        /// Retourne la direction cardinale du cap
        /// Ex: "Nord", "Nord-Est", "Est", etc.
        /// </summary>
        public string GetCardinalDirection(double bearing)
        {
            var directions = new[]
            {
                "Nord", "Nord-Nord-Est", "Nord-Est", "Est-Nord-Est",
                "Est", "Est-Sud-Est", "Sud-Est", "Sud-Sud-Est",
                "Sud", "Sud-Sud-Ouest", "Sud-Ouest", "Ouest-Sud-Ouest",
                "Ouest", "Ouest-Nord-Ouest", "Nord-Ouest", "Nord-Nord-Ouest"
            };

            var index = (int)Math.Round(bearing / 22.5) % 16;
            return directions[index];
        }

        /// <summary>
        /// Estime le temps de trajet en minutes selon le mode de transport
        /// </summary>
        public double EstimateTravelTime(double distanceKm, TravelMode mode)
        {
            double averageSpeedKmh = mode switch
            {
                TravelMode.Walking => 5.0,  // 5 km/h
                TravelMode.Cycling => 15.0, // 15 km/h
                TravelMode.Driving => 50.0, // 50 km/h (moyenne avec routes sinueuses)
                _ => 5.0
            };

            return (distanceKm / averageSpeedKmh) * 60; // Retourne en minutes
        }

        /// <summary>
        /// Retourne une description compl�te de la mesure entre deux points
        /// </summary>
        public string GetMeasurementSummary(PointLatLng point1, PointLatLng point2)
        {
            var distance = CalculateDistance(point1, point2);
            var bearing = CalculateBearing(point1, point2);
            var direction = GetCardinalDirection(bearing);
            var readableDistance = GetReadableDistance(distance);

            var walkTime = EstimateTravelTime(distance, TravelMode.Walking);
            var bikeTime = EstimateTravelTime(distance, TravelMode.Cycling);
            var carTime = EstimateTravelTime(distance, TravelMode.Driving);

            return $"?? Distance: {readableDistance}\n" +
                   $"?? Direction: {direction} ({bearing:F0}�)\n\n" +
                   $"?? Temps estim�:\n" +
                   $"  ?? � pied: {walkTime:F0} min\n" +
                   $"  ?? � v�lo: {bikeTime:F0} min\n" +
                   $"  ?? En voiture: {carTime:F0} min";
        }

        /// <summary>
        /// Trouve le filon le plus proche d'un point donn�
        /// </summary>
        public (Filon? filon, double distance) FindNearestFilon(PointLatLng point, List<Filon> filons)
        {
            Filon? nearest = null;
            double minDistance = double.MaxValue;

            foreach (var filon in filons)
            {
                if (!filon.Latitude.HasValue || !filon.Longitude.HasValue)
                    continue;

                var filonPoint = new PointLatLng(filon.Latitude.Value, filon.Longitude.Value);
                var distance = CalculateDistance(point, filonPoint);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = filon;
                }
            }

            return (nearest, minDistance);
        }

        /// <summary>
        /// Trouve tous les filons dans un rayon donn�
        /// </summary>
        public List<(Filon filon, double distance)> FindFilonsInRadius(
            PointLatLng center, 
            double radiusKm, 
            List<Filon> filons)
        {
            var results = new List<(Filon, double)>();

            foreach (var filon in filons)
            {
                if (!filon.Latitude.HasValue || !filon.Longitude.HasValue)
                    continue;

                var filonPoint = new PointLatLng(filon.Latitude.Value, filon.Longitude.Value);
                var distance = CalculateDistance(center, filonPoint);

                if (distance <= radiusKm)
                {
                    results.Add((filon, distance));
                }
            }

            return results.OrderBy(r => r.Item2).ToList(); // r.Item2 = distance
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        public enum TravelMode
        {
            Walking,
            Cycling,
            Driving
        }
    }
}
