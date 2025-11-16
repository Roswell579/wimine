using GMap.NET;
using GMap.NET.WindowsForms;

namespace wmine.Services
{
    /// <summary>
    /// Service pour restreindre les déplacements de la carte é une zone géographique spécifique
    /// (Zone Lambert III Sud - Sud de la France)
    /// </summary>
    public class MapBoundsRestrictor
    {
        // Limites approximatives Lambert III Zone Sud (Sud-Est de la France)
        // Couvre approximativement: Provence, Céte d'Azur, Languedoc-Roussillon
        private const double LAMBERT_III_SUD_MIN_LAT = 42.0;  // Corse sud / Pyrénées
        private const double LAMBERT_III_SUD_MAX_LAT = 45.5;  // Lyon / Alpes
        private const double LAMBERT_III_SUD_MIN_LNG = 0.0;   // Toulouse
        private const double LAMBERT_III_SUD_MAX_LNG = 8.0;   // Frontiére italienne

        private readonly GMapControl _mapControl;
        private bool _restrictionEnabled = true;

        public bool RestrictionEnabled
        {
            get => _restrictionEnabled;
            set => _restrictionEnabled = value;
        }

        public MapBoundsRestrictor(GMapControl mapControl)
        {
            _mapControl = mapControl ?? throw new ArgumentNullException(nameof(mapControl));
        }

        /// <summary>
        /// Restreint la position actuelle de la carte aux limites définies
        /// </summary>
        public void RestrictMapBounds()
        {
            if (!_restrictionEnabled)
                return;

            var pos = _mapControl.Position;
            bool needsUpdate = false;
            double newLat = pos.Lat;
            double newLng = pos.Lng;

            // Vérifier les limites de latitude
            if (pos.Lat < LAMBERT_III_SUD_MIN_LAT)
            {
                newLat = LAMBERT_III_SUD_MIN_LAT;
                needsUpdate = true;
            }
            else if (pos.Lat > LAMBERT_III_SUD_MAX_LAT)
            {
                newLat = LAMBERT_III_SUD_MAX_LAT;
                needsUpdate = true;
            }

            // Vérifier les limites de longitude
            if (pos.Lng < LAMBERT_III_SUD_MIN_LNG)
            {
                newLng = LAMBERT_III_SUD_MIN_LNG;
                needsUpdate = true;
            }
            else if (pos.Lng > LAMBERT_III_SUD_MAX_LNG)
            {
                newLng = LAMBERT_III_SUD_MAX_LNG;
                needsUpdate = true;
            }

            // Appliquer la correction si nécessaire
            if (needsUpdate)
            {
                _mapControl.Position = new PointLatLng(newLat, newLng);
            }
        }

        /// <summary>
        /// Vérifie si une position est dans les limites autorisées
        /// </summary>
        public bool IsPositionValid(PointLatLng position)
        {
            return position.Lat >= LAMBERT_III_SUD_MIN_LAT &&
                   position.Lat <= LAMBERT_III_SUD_MAX_LAT &&
                   position.Lng >= LAMBERT_III_SUD_MIN_LNG &&
                   position.Lng <= LAMBERT_III_SUD_MAX_LNG;
        }

        /// <summary>
        /// Retourne la position la plus proche dans les limites autorisées
        /// </summary>
        public PointLatLng ClampPosition(PointLatLng position)
        {
            double lat = Math.Max(LAMBERT_III_SUD_MIN_LAT, Math.Min(LAMBERT_III_SUD_MAX_LAT, position.Lat));
            double lng = Math.Max(LAMBERT_III_SUD_MIN_LNG, Math.Min(LAMBERT_III_SUD_MAX_LNG, position.Lng));
            return new PointLatLng(lat, lng);
        }

        /// <summary>
        /// Centre la carte sur la zone Lambert III Sud
        /// </summary>
        public void CenterOnZone()
        {
            double centerLat = (LAMBERT_III_SUD_MIN_LAT + LAMBERT_III_SUD_MAX_LAT) / 2;
            double centerLng = (LAMBERT_III_SUD_MIN_LNG + LAMBERT_III_SUD_MAX_LNG) / 2;
            _mapControl.Position = new PointLatLng(centerLat, centerLng);
        }
    }
}
