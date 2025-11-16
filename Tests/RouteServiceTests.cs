using System;
using System.Threading.Tasks;
using GMap.NET;
using wmine.Models;
using wmine.Services;

namespace wmine.Tests
{
    /// <summary>
    /// Tests simples pour le RouteService
    /// </summary>
    public static class RouteServiceTests
    {
        public static async Task RunAllTests()
        {
            Console.WriteLine("???????????????????????????????????????????????????????");
            Console.WriteLine("         TEST DU SERVICE DE ROUTING (OSRM)");
            Console.WriteLine("???????????????????????????????????????????????????????");
            Console.WriteLine();

            await TestCarRoute();
            await TestWalkingRoute();
            await TestCyclingRoute();
            await TestShortRoute();
            await TestLongRoute();

            Console.WriteLine();
            Console.WriteLine("???????????????????????????????????????????????????????");
            Console.WriteLine("         TOUS LES TESTS TERMINéS ?");
            Console.WriteLine("???????????????????????????????????????????????????????");
            Console.ReadLine();
        }

        private static async Task TestCarRoute()
        {
            Console.WriteLine("?? TEST 1: Itinéraire en voiture (Toulon ? Nice)");
            Console.WriteLine("???????????????????????????????????????????????????????");

            var service = new RouteService();
            var toulon = new PointLatLng(43.1242, 5.9280);
            var nice = new PointLatLng(43.7102, 7.2620);

            try
            {
                var route = await service.CalculateRouteAsync(toulon, nice, TransportType.Car);

                if (route != null)
                {
                    Console.WriteLine($"? Distance: {route.FormattedDistance}");
                    Console.WriteLine($"? Durée: {route.FormattedDuration}");
                    Console.WriteLine($"? Nombre de points: {route.Points.Count}");
                    Console.WriteLine($"? Instructions: {route.Instructions.Count}");
                    
                    if (route.Instructions.Count > 0)
                    {
                        Console.WriteLine($"   Premiére instruction: {route.Instructions[0]}");
                    }
                }
                else
                {
                    Console.WriteLine("? éCHEC: Aucun itinéraire trouvé");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERREUR: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task TestWalkingRoute()
        {
            Console.WriteLine("?? TEST 2: Itinéraire é pied (courte distance)");
            Console.WriteLine("???????????????????????????????????????????????????????");

            var service = new RouteService();
            var pointA = new PointLatLng(43.1242, 5.9280);
            var pointB = new PointLatLng(43.1300, 5.9350); // ~1km

            try
            {
                var route = await service.CalculateRouteAsync(pointA, pointB, TransportType.Walking);

                if (route != null)
                {
                    Console.WriteLine($"? Distance: {route.FormattedDistance}");
                    Console.WriteLine($"? Durée: {route.FormattedDuration}");
                    Console.WriteLine($"? Mode: é pied");
                }
                else
                {
                    Console.WriteLine("? éCHEC: Aucun itinéraire trouvé");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERREUR: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task TestCyclingRoute()
        {
            Console.WriteLine("?? TEST 3: Itinéraire é vélo (Marseille ? Aix)");
            Console.WriteLine("???????????????????????????????????????????????????????");

            var service = new RouteService();
            var marseille = new PointLatLng(43.2965, 5.3698);
            var aix = new PointLatLng(43.5297, 5.4474);

            try
            {
                var route = await service.CalculateRouteAsync(marseille, aix, TransportType.Cycling);

                if (route != null)
                {
                    Console.WriteLine($"? Distance: {route.FormattedDistance}");
                    Console.WriteLine($"? Durée: {route.FormattedDuration}");
                    Console.WriteLine($"? Points de passage: {route.Points.Count}");
                }
                else
                {
                    Console.WriteLine("? éCHEC: Aucun itinéraire trouvé");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERREUR: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task TestShortRoute()
        {
            Console.WriteLine("?? TEST 4: Itinéraire trés court (< 500m)");
            Console.WriteLine("???????????????????????????????????????????????????????");

            var service = new RouteService();
            var pointA = new PointLatLng(43.1242, 5.9280);
            var pointB = new PointLatLng(43.1250, 5.9290); // ~300m

            try
            {
                var route = await service.CalculateRouteAsync(pointA, pointB, TransportType.Walking);

                if (route != null)
                {
                    Console.WriteLine($"? Distance: {route.FormattedDistance}");
                    Console.WriteLine($"? Durée: {route.FormattedDuration}");
                    
                    // Vérifier que la distance est bien en métres
                    if (route.DistanceKm < 1)
                    {
                        Console.WriteLine($"? Format correct: Distance affichée en métres");
                    }
                }
                else
                {
                    Console.WriteLine("? éCHEC: Aucun itinéraire trouvé");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERREUR: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task TestLongRoute()
        {
            Console.WriteLine("???  TEST 5: Itinéraire longue distance (Toulon ? Lyon)");
            Console.WriteLine("???????????????????????????????????????????????????????");

            var service = new RouteService();
            var toulon = new PointLatLng(43.1242, 5.9280);
            var lyon = new PointLatLng(45.7640, 4.8357);

            try
            {
                var route = await service.CalculateRouteAsync(toulon, lyon, TransportType.Car);

                if (route != null)
                {
                    Console.WriteLine($"? Distance: {route.FormattedDistance}");
                    Console.WriteLine($"? Durée: {route.FormattedDuration}");
                    Console.WriteLine($"? Nombre d'instructions: {route.Instructions.Count}");
                    
                    // Vérifier le format de durée (devrait étre en heures)
                    if (route.DurationMinutes > 60)
                    {
                        Console.WriteLine($"? Format correct: Durée affichée en heures et minutes");
                    }
                }
                else
                {
                    Console.WriteLine("? éCHEC: Aucun itinéraire trouvé");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERREUR: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Test interactif depuis la position actuelle
        /// </summary>
        public static async Task TestFromCurrentLocation()
        {
            Console.WriteLine("?? TEST: Itinéraire depuis la position actuelle");
            Console.WriteLine("???????????????????????????????????????????????????????");

            var service = new RouteService();
            var destination = new PointLatLng(43.7102, 7.2620); // Nice

            try
            {
                var route = await service.CalculateRouteFromCurrentLocationAsync(destination);

                if (route != null)
                {
                    Console.WriteLine($"? Depuis: {route.Start.Lat:F4}, {route.Start.Lng:F4} (position simulée)");
                    Console.WriteLine($"? Vers: {route.End.Lat:F4}, {route.End.Lng:F4}");
                    Console.WriteLine($"? Distance: {route.FormattedDistance}");
                    Console.WriteLine($"? Durée: {route.FormattedDuration}");
                }
                else
                {
                    Console.WriteLine("? éCHEC: Aucun itinéraire trouvé");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERREUR: {ex.Message}");
            }
        }
    }
}
