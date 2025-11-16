using System;
using wmine.Services;

namespace wmine
{
    /// <summary>
    /// Classe de test pour vérifier la conversion de coordonnées
    /// </summary>
    public class TestCoordinates
    {
        public static void RunTests()
        {
            Console.WriteLine("=== Test de conversion Lambert 3 Sud -> WGS84 ===\n");

            // Test avec les coordonnées d'exemple (format km sans préfixe)
            TestConversion(969.29, 144.46, "Exemple 1 (km sans préfixe)");

            // Test avec les coordonnées en métres sans préfixe
            TestConversion(969290, 144460, "Exemple 2 (m sans préfixe)");

            // Test avec les coordonnées en métres avec préfixe complet
            TestConversion(969290, 3144460, "Exemple 3 (m avec préfixe complet)");

            // Test de conversion aller-retour
            Console.WriteLine("\n=== Test de conversion aller-retour ===\n");
            TestRoundTrip(43.509, 6.904);

            Console.WriteLine("\nTests terminés !");
        }

        private static void TestConversion(double x, double y, string testName)
        {
            Console.WriteLine($"{testName}:");
            Console.WriteLine($"  Lambert 3 entrée: X={x:F2}, Y={y:F2}");

            var (xNorm, yNorm) = CoordinateConverter.NormalizeToMeters(x, y);
            Console.WriteLine($"  Lambert 3 normalisé: X={xNorm:F0}, Y={yNorm:F0}");

            var (lat, lon) = CoordinateConverter.Lambert3ToWGS84(x, y);
            Console.WriteLine($"  WGS84: Lat={lat:F6}é, Lon={lon:F6}é");

            bool validLambert = CoordinateConverter.IsValidLambert3Var(x, y);
            bool validWGS84 = CoordinateConverter.IsValidWGS84Var(lat, lon);

            Console.WriteLine($"  Validation Lambert: {(validLambert ? "?" : "?")}");
            Console.WriteLine($"  Validation WGS84: {(validWGS84 ? "?" : "?")}");
            Console.WriteLine();
        }

        private static void TestRoundTrip(double lat, double lon)
        {
            Console.WriteLine($"Test aller-retour:");
            Console.WriteLine($"  WGS84 départ: Lat={lat:F6}é, Lon={lon:F6}é");

            var (x, y) = CoordinateConverter.WGS84ToLambert3(lat, lon);
            Console.WriteLine($"  Lambert 3: X={x:F2}, Y={y:F2}");

            var (lat2, lon2) = CoordinateConverter.Lambert3ToWGS84(x, y);
            Console.WriteLine($"  WGS84 retour: Lat={lat2:F6}é, Lon={lon2:F6}é");

            double diffLat = Math.Abs(lat - lat2);
            double diffLon = Math.Abs(lon - lon2);
            Console.WriteLine($"  Différence: ?Lat={diffLat:E3}é, ?Lon={diffLon:E3}é");

            if (diffLat < 1e-5 && diffLon < 1e-5)
                Console.WriteLine($"  Précision: ? (< 1m)");
            else
                Console.WriteLine($"  Précision: ?? Différence notable");
        }
    }
}
