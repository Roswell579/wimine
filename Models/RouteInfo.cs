using GMap.NET;

namespace wmine.Models
{
    /// <summary>
    /// Informations sur un itinéraire calculé
    /// </summary>
    public class RouteInfo
    {
        /// <summary>
        /// Points de l'itinéraire (lat/lng)
        /// </summary>
        public List<PointLatLng> Points { get; set; } = new List<PointLatLng>();

        /// <summary>
        /// Distance totale en kilométres
        /// </summary>
        public double DistanceKm { get; set; }

        /// <summary>
        /// Durée estimée en minutes
        /// </summary>
        public double DurationMinutes { get; set; }

        /// <summary>
        /// Type de transport
        /// </summary>
        public TransportType TransportType { get; set; }

        /// <summary>
        /// Instructions de navigation
        /// </summary>
        public List<string> Instructions { get; set; } = new List<string>();

        /// <summary>
        /// Point de départ
        /// </summary>
        public PointLatLng Start { get; set; }

        /// <summary>
        /// Point d'arrivée
        /// </summary>
        public PointLatLng End { get; set; }

        /// <summary>
        /// Nom du lieu de départ
        /// </summary>
        public string StartName { get; set; } = "départ";

        /// <summary>
        /// Nom du lieu d'arrivée
        /// </summary>
        public string EndName { get; set; } = "Arrivée";

        /// <summary>
        /// Formatage de la distance
        /// </summary>
        public string FormattedDistance => DistanceKm < 1
            ? $"{(int)(DistanceKm * 1000)} m"
            : $"{DistanceKm:F2} km";

        /// <summary>
        /// Formatage de la durée
        /// </summary>
        public string FormattedDuration
        {
            get
            {
                if (DurationMinutes < 60)
                    return $"{(int)DurationMinutes} min";

                var hours = (int)(DurationMinutes / 60);
                var minutes = (int)(DurationMinutes % 60);
                return $"{hours}h{minutes:D2}";
            }
        }
    }

    /// <summary>
    /// Type de transport pour le calcul d'itinéraire
    /// </summary>
    public enum TransportType
    {
        /// <summary>
        /// En voiture (route)
        /// </summary>
        Car,

        /// <summary>
        /// à pied (chemin)
        /// </summary>
        Walking,

        /// <summary>
        /// à vélo
        /// </summary>
        Cycling
    }
}
