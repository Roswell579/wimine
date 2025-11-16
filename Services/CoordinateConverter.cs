using System;

namespace wmine.Services
{
    /// <summary>
    /// Service de conversion de coordonnées entre Lambert 3 Zone Sud et WGS84
    /// Spécifiquement adapté pour la région Var/Alpes-Maritimes
    /// Utilise les paramètres officiels Lambert III Sud (NTF - Nouvelle Triangulation Française)
    /// et transformation Helmert 7 paramètres IGN pour NTF -> WGS84
    /// </summary>
    public class CoordinateConverter
    {
        // Constantes Lambert III Sud (NTF) - Paramètres officiels IGN
        private const double LAMBERT3_N = 0.760405966431770;
        private const double LAMBERT3_C = 11603796.9767;
        private const double LAMBERT3_XS = 600000.0;
        private const double LAMBERT3_YS = 3200000.0; // Y avec préfixe de zone
        private const double LAMBERT3_E = 0.08248325676; // Excentricité Clarke 1880 IGN

        // Paramètres ellipsoïde Clarke 1880 IGN
        private const double A_CLARKE = 6378249.2; // Demi-grand axe
        private const double B_CLARKE = 6356515.0; // Demi-petit axe
        private const double E2_CLARKE = 0.006803511; // Première excentricité au carré

        // Paramètres ellipsoïde WGS84
        private const double A_WGS84 = 6378137.0;
        private const double E2_WGS84 = 0.00669438002290;

        // Origine Lambert III Sud
        private const double LAT0_LAMBERT3 = 0.722738287280763; // 41.4° en radians (latitude d'origine)
        private const double LON0_LAMBERT3 = 0.0407924334319869; // 2.337229166667° en radians (méridien central Paris)

        // Paramètres transformation Helmert NTF -> WGS84 (IGN officiel)
        // Valeurs pour la France métropolitaine
        private const double TX = -168.0; // Translation X en mètres
        private const double TY = -60.0;  // Translation Y en mètres
        private const double TZ = 320.0;  // Translation Z en mètres
        private const double RX = 0.0;    // Rotation X en secondes d'arc
        private const double RY = 0.0;    // Rotation Y en secondes d'arc
        private const double RZ = 0.0;    // Rotation Z en secondes d'arc
        private const double SCALE = 0.0; // Facteur d'échelle en ppm

        // Limites géographiques pour la région Var/Alpes-Maritimes
        private const double VAR_MIN_LAT = 43.0;
        private const double VAR_MAX_LAT = 44.5;
        private const double VAR_MIN_LON = 5.5;
        private const double VAR_MAX_LON = 7.7;

        // Limites Lambert 3 pour le Var/Alpes-Maritimes
        private const double VAR_MIN_X = 900000;
        private const double VAR_MAX_X = 1050000;
        private const double VAR_MIN_Y = 3100000;
        private const double VAR_MAX_Y = 3250000;

        /// <summary>
        /// Convertit des coordonnées Lambert 3 Zone Sud (NTF) en WGS84
        /// Gère automatiquement la notation avec ou sans préfixe de zone
        /// </summary>
        public static (double latitude, double longitude) Lambert3ToWGS84(double x, double y)
        {
            // Normaliser les coordonnées (conversion km -> m et ajout préfixe si nécessaire)
            var (xNorm, yNorm) = NormalizeToMeters(x, y);

            // Calculs Lambert III Sud -> NTF géographique
            double dx = xNorm - LAMBERT3_XS;
            double dy = LAMBERT3_YS - yNorm; // Inversion: Y croît vers le nord

            double r = Math.Sqrt(dx * dx + dy * dy);
            double gamma = Math.Atan2(dx, dy);

            // Longitude NTF
            double lonNTF = LON0_LAMBERT3 + gamma / LAMBERT3_N;

            // Latitude isométrique
            double latIso = -Math.Log(r / LAMBERT3_C) / LAMBERT3_N;

            // Latitude géographique NTF (méthode itérative)
            double latNTF = LatitudeFromIso(latIso, LAMBERT3_E, LAT0_LAMBERT3);

            // Conversion NTF (Clarke 1880) -> WGS84 avec transformation Helmert
            (double latWGS84, double lonWGS84) = NTFToWGS84Helmert(latNTF, lonNTF);

            return (latWGS84, lonWGS84);
        }

        /// <summary>
        /// Convertit des coordonnées WGS84 en Lambert 3 Zone Sud
        /// </summary>
        public static (double x, double y) WGS84ToLambert3(double latitude, double longitude)
        {
            // Transformation WGS84 -> NTF avec Helmert inverse
            (double latNTF, double lonNTF) = WGS84ToNTFHelmert(latitude * Math.PI / 180.0, longitude * Math.PI / 180.0);

            double latIso = LatitudeIso(latNTF, LAMBERT3_E);

            double x = LAMBERT3_XS + LAMBERT3_C * Math.Exp(-LAMBERT3_N * latIso) *
                       Math.Sin(LAMBERT3_N * (lonNTF - LON0_LAMBERT3));

            double y = LAMBERT3_YS - LAMBERT3_C * Math.Exp(-LAMBERT3_N * latIso) *
                       Math.Cos(LAMBERT3_N * (lonNTF - LON0_LAMBERT3));

            return (x, y);
        }

        /// <summary>
        /// Transformation de datum NTF (Clarke 1880) vers WGS84 avec transformation Helmert 7 paramètres
        /// Méthode officielle IGN pour la France métropolitaine
        /// </summary>
        private static (double lat, double lon) NTFToWGS84Helmert(double latRad, double lonRad)
        {
            // Conversion coordonnées géographiques NTF -> cartésiennes (X, Y, Z)
            double h = 0; // Altitude approximative (niveau mer)
            double N = A_CLARKE / Math.Sqrt(1 - E2_CLARKE * Math.Sin(latRad) * Math.Sin(latRad));
            
            double X_ntf = (N + h) * Math.Cos(latRad) * Math.Cos(lonRad);
            double Y_ntf = (N + h) * Math.Cos(latRad) * Math.Sin(lonRad);
            double Z_ntf = (N * (1 - E2_CLARKE) + h) * Math.Sin(latRad);

            // Conversion des rotations de secondes d'arc en radians
            double rx = RX * Math.PI / (180.0 * 3600.0);
            double ry = RY * Math.PI / (180.0 * 3600.0);
            double rz = RZ * Math.PI / (180.0 * 3600.0);
            double scale = 1.0 + SCALE * 1e-6;

            // Application de la transformation Helmert
            double X_wgs84 = TX + scale * (X_ntf - rz * Y_ntf + ry * Z_ntf);
            double Y_wgs84 = TY + scale * (rz * X_ntf + Y_ntf - rx * Z_ntf);
            double Z_wgs84 = TZ + scale * (-ry * X_ntf + rx * Y_ntf + Z_ntf);

            // Conversion cartésiennes WGS84 -> géographiques WGS84
            double p = Math.Sqrt(X_wgs84 * X_wgs84 + Y_wgs84 * Y_wgs84);
            double lon_wgs84 = Math.Atan2(Y_wgs84, X_wgs84);
            
            // Méthode itérative pour la latitude
            double lat_wgs84 = Math.Atan2(Z_wgs84, p * (1 - E2_WGS84));
            for (int i = 0; i < 5; i++)
            {
                double N_wgs84 = A_WGS84 / Math.Sqrt(1 - E2_WGS84 * Math.Sin(lat_wgs84) * Math.Sin(lat_wgs84));
                lat_wgs84 = Math.Atan2(Z_wgs84 + E2_WGS84 * N_wgs84 * Math.Sin(lat_wgs84), p);
            }

            // Conversion radians -> degrés
            return (lat_wgs84 * 180.0 / Math.PI, lon_wgs84 * 180.0 / Math.PI);
        }

        /// <summary>
        /// Transformation de datum WGS84 vers NTF (Clarke 1880) avec Helmert inverse
        /// </summary>
        private static (double lat, double lon) WGS84ToNTFHelmert(double latRad, double lonRad)
        {
            // Conversion coordonnées géographiques WGS84 -> cartésiennes
            double h = 0;
            double N = A_WGS84 / Math.Sqrt(1 - E2_WGS84 * Math.Sin(latRad) * Math.Sin(latRad));
            
            double X_wgs84 = (N + h) * Math.Cos(latRad) * Math.Cos(lonRad);
            double Y_wgs84 = (N + h) * Math.Cos(latRad) * Math.Sin(lonRad);
            double Z_wgs84 = (N * (1 - E2_WGS84) + h) * Math.Sin(latRad);

            // Transformation inverse (paramètres inversés)
            double rx = -RX * Math.PI / (180.0 * 3600.0);
            double ry = -RY * Math.PI / (180.0 * 3600.0);
            double rz = -RZ * Math.PI / (180.0 * 3600.0);
            double scale = 1.0 - SCALE * 1e-6;

            double X_ntf = -TX + scale * (X_wgs84 - rz * Y_wgs84 + ry * Z_wgs84);
            double Y_ntf = -TY + scale * (rz * X_wgs84 + Y_wgs84 - rx * Z_wgs84);
            double Z_ntf = -TZ + scale * (-ry * X_wgs84 + rx * Y_wgs84 + Z_wgs84);

            // Conversion cartésiennes NTF -> géographiques NTF
            double p = Math.Sqrt(X_ntf * X_ntf + Y_ntf * Y_ntf);
            double lon_ntf = Math.Atan2(Y_ntf, X_ntf);
            
            double lat_ntf = Math.Atan2(Z_ntf, p * (1 - E2_CLARKE));
            for (int i = 0; i < 5; i++)
            {
                double N_ntf = A_CLARKE / Math.Sqrt(1 - E2_CLARKE * Math.Sin(lat_ntf) * Math.Sin(lat_ntf));
                lat_ntf = Math.Atan2(Z_ntf + E2_CLARKE * N_ntf * Math.Sin(lat_ntf), p);
            }

            return (lat_ntf, lon_ntf);
        }

        /// <summary>
        /// Normalise les coordonnées en mètres et ajoute le préfixe de zone si nécessaire
        /// </summary>
        public static (double x, double y) NormalizeToMeters(double x, double y)
        {
            // Conversion km -> m si nécessaire
            if (x < 10000) x *= 1000;

            // Gestion du Y avec ou sans préfixe
            if (y < 1000)
            {
                // Format km sans préfixe (ex: 144.46)
                y = y * 1000 + 3000000;
            }
            else if (y < 10000)
            {
                // Format km avec préfixe partiel (ex: 3144.46)
                y = y * 1000;
            }
            else if (y < 1000000)
            {
                // Format m sans préfixe (ex: 144460)
                y = y + 3000000;
            }
            // Sinon, c'est déjà en mètres avec préfixe

            return (x, y);
        }

        /// <summary>
        /// Vérifie si les coordonnées Lambert 3 sont valides pour la région Var/AM
        /// </summary>
        public static bool IsValidLambert3Var(double x, double y)
        {
            var (xNorm, yNorm) = NormalizeToMeters(x, y);
            return xNorm >= VAR_MIN_X && xNorm <= VAR_MAX_X &&
                   yNorm >= VAR_MIN_Y && yNorm <= VAR_MAX_Y;
        }

        /// <summary>
        /// Vérifie si les coordonnées WGS84 sont valides pour la région Var/AM
        /// </summary>
        public static bool IsValidWGS84Var(double latitude, double longitude)
        {
            return latitude >= VAR_MIN_LAT && latitude <= VAR_MAX_LAT &&
                   longitude >= VAR_MIN_LON && longitude <= VAR_MAX_LON;
        }

        /// <summary>
        /// Retourne des informations sur les coordonnées normalisées
        /// </summary>
        public static string GetNormalizedCoordinatesInfo(double x, double y)
        {
            var (xNorm, yNorm) = NormalizeToMeters(x, y);
            bool hasPrefix = y >= 1000000 || (y >= 1000 && y < 10000);

            string info = $"Coordonnées Lambert 3 Zone Sud (NTF):\n";
            info += $"   X = {x:F2} → {xNorm:F0} m\n";
            info += $"   Y = {y:F2} → {yNorm:F0} m\n";

            if (!hasPrefix && y < 1000)
            {
                info += $"   (notation km sans préfixe détectée)\n";
            }

            bool isValid = IsValidLambert3Var(x, y);
            info += isValid ? "\n✅ Coordonnées dans la zone Var/Alpes-Maritimes" :
                             "\n⚠️ Coordonnées hors zone habituelle Var/AM";

            return info;
        }

        /// <summary>
        /// Calcule la latitude isométrique
        /// </summary>
        private static double LatitudeIso(double lat, double e)
        {
            return Math.Log(Math.Tan(Math.PI / 4.0 + lat / 2.0) *
                   Math.Pow((1 - e * Math.Sin(lat)) / (1 + e * Math.Sin(lat)), e / 2.0));
        }

        /// <summary>
        /// Calcule la latitude à partir de la latitude isométrique (méthode itérative)
        /// </summary>
        private static double LatitudeFromIso(double latIso, double e, double lat0)
        {
            double lat = lat0;
            double epsilon = 1e-11;

            for (int i = 0; i < 100; i++)
            {
                double latNew = 2.0 * Math.Atan(Math.Exp(latIso) *
                               Math.Pow((1 + e * Math.Sin(lat)) / (1 - e * Math.Sin(lat)), e / 2.0)) - Math.PI / 2.0;

                if (Math.Abs(latNew - lat) < epsilon)
                    return latNew;

                lat = latNew;
            }

            return lat;
        }
    }
}

// Exemple d'utilisation
// Déplacez ce code dans une méthode, par exemple dans Program.Main ou dans un test unitaire
/*
var (lat, lon) = wmine.Services.CoordinateConverter.Lambert3ToWGS84(969.29, 144.46);
// Devrait donner : lat ≈ 43.509°, lon ≈ 6.904°
*/
