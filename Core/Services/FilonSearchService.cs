using wmine.Models;

namespace wmine.Core.Services
{
    /// <summary>
    /// Options de recherche
    /// </summary>
    public class SearchOptions
    {
        public bool SearchInName { get; set; } = true;
        public bool SearchInNotes { get; set; } = true;
        public MineralType? FilterByMineral { get; set; }
        public FilonStatus? FilterByStatus { get; set; }
        public bool OnlyWithCoordinates { get; set; } = false;
        public bool OnlyWithPhotos { get; set; } = false;
        public double? SearchRadiusKm { get; set; }
        public double? CenterLatitude { get; set; }
        public double? CenterLongitude { get; set; }
    }

    /// <summary>
    /// Service de recherche avancée de filons
    /// </summary>
    public class FilonSearchService
    {
        private readonly List<Filon> _filons;

        public FilonSearchService(List<Filon> filons)
        {
            _filons = filons ?? new List<Filon>();
        }

        /// <summary>
        /// Recherche des filons selon les critéres
        /// </summary>
        public List<Filon> Search(string query, SearchOptions? options = null)
        {
            options ??= new SearchOptions();
            query = query?.ToLowerInvariant() ?? string.Empty;

            var results = _filons.AsEnumerable();

            // Recherche textuelle
            if (!string.IsNullOrWhiteSpace(query))
            {
                results = results.Where(f =>
                {
                    bool matches = false;

                    if (options.SearchInName && !string.IsNullOrEmpty(f.Nom))
                    {
                        matches |= f.Nom.ToLowerInvariant().Contains(query);
                    }

                    if (options.SearchInNotes && !string.IsNullOrEmpty(f.Notes))
                    {
                        matches |= f.Notes.ToLowerInvariant().Contains(query);
                    }

                    return matches;
                });
            }

            // Filtre par minéral
            if (options.FilterByMineral.HasValue)
            {
                results = results.Where(f => f.MatierePrincipale == options.FilterByMineral.Value);
            }

            // Filtre par statut
            if (options.FilterByStatus.HasValue)
            {
                results = results.Where(f => f.Statut.HasFlag(options.FilterByStatus.Value));
            }

            // Filtre: seulement avec coordonnées
            if (options.OnlyWithCoordinates)
            {
                results = results.Where(f => f.HasCoordinates());
            }

            // Filtre: seulement avec photos
            if (options.OnlyWithPhotos)
            {
                results = results.Where(f => !string.IsNullOrEmpty(f.PhotoPath));
            }

            // Recherche géographique par rayon
            if (options.SearchRadiusKm.HasValue &&
                options.CenterLatitude.HasValue &&
                options.CenterLongitude.HasValue)
            {
                results = results.Where(f =>
                {
                    if (!f.Latitude.HasValue || !f.Longitude.HasValue)
                        return false;

                    var distance = CalculateDistance(
                        options.CenterLatitude.Value,
                        options.CenterLongitude.Value,
                        f.Latitude.Value,
                        f.Longitude.Value
                    );

                    return distance <= options.SearchRadiusKm.Value;
                });
            }

            return results.ToList();
        }

        /// <summary>
        /// Recherche par proximité géographique
        /// </summary>
        public List<(Filon Filon, double DistanceKm)> SearchByProximity(
            double latitude,
            double longitude,
            double radiusKm)
        {
            var results = new List<(Filon, double)>();

            foreach (var filon in _filons)
            {
                if (!filon.Latitude.HasValue || !filon.Longitude.HasValue)
                    continue;

                var distance = CalculateDistance(
                    latitude,
                    longitude,
                    filon.Latitude.Value,
                    filon.Longitude.Value
                );

                if (distance <= radiusKm)
                {
                    results.Add((filon, distance));
                }
            }

            return results.OrderBy(r => r.Item2).ToList();
        }

        /// <summary>
        /// Calcule la distance entre deux points GPS (formule de Haversine)
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Rayon de la Terre en km

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c;

            return distance;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// Obtient des statistiques sur les filons
        /// </summary>
        public FilonStatistics GetStatistics()
        {
            return new FilonStatistics
            {
                TotalCount = _filons.Count,
                WithCoordinatesCount = _filons.Count(f => f.HasCoordinates()),
                WithPhotosCount = _filons.Count(f => !string.IsNullOrEmpty(f.PhotoPath)),
                WithDocumentationCount = _filons.Count(f => !string.IsNullOrEmpty(f.DocumentationPath)),
                ByMineral = _filons
                    .GroupBy(f => f.MatierePrincipale)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ByStatus = _filons
                    .SelectMany(f => GetIndividualStatuses(f.Statut))
                    .GroupBy(s => s)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        private IEnumerable<FilonStatus> GetIndividualStatuses(FilonStatus status)
        {
            var statuses = new List<FilonStatus>();

            foreach (FilonStatus value in Enum.GetValues<FilonStatus>())
            {
                if (value != FilonStatus.Aucun && status.HasFlag(value))
                {
                    statuses.Add(value);
                }
            }

            return statuses.Any() ? statuses : new List<FilonStatus> { FilonStatus.Aucun };
        }
    }

    /// <summary>
    /// Statistiques sur les filons
    /// </summary>
    public class FilonStatistics
    {
        public int TotalCount { get; set; }
        public int WithCoordinatesCount { get; set; }
        public int WithPhotosCount { get; set; }
        public int WithDocumentationCount { get; set; }
        public Dictionary<MineralType, int> ByMineral { get; set; } = new();
        public Dictionary<FilonStatus, int> ByStatus { get; set; } = new();

        public double PercentageWithCoordinates => TotalCount > 0
            ? (WithCoordinatesCount * 100.0 / TotalCount)
            : 0;

        public double PercentageWithPhotos => TotalCount > 0
            ? (WithPhotosCount * 100.0 / TotalCount)
            : 0;

        public double PercentageWithDocumentation => TotalCount > 0
            ? (WithDocumentationCount * 100.0 / TotalCount)
            : 0;
    }
}
