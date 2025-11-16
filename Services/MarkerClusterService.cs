using GMap.NET;
using GMap.NET.WindowsForms;
using wmine.Models;
using wmine.UI;

namespace wmine.Services
{
    /// <summary>
    /// Service de gestion du clustering des marqueurs de filons
    /// Regroupe automatiquement les marqueurs proches selon le niveau de zoom
    /// </summary>
    public class MarkerClusterService
    {
        private readonly GMapControl _mapControl;
        private GMapOverlay _clusterOverlay;
        private GMapOverlay _individualOverlay;
        private List<Filon> _allFilons;
        private const double CLUSTER_DISTANCE_KM = 2.0; // Distance en km pour regrouper

        public MarkerClusterService(GMapControl mapControl)
        {
            _mapControl = mapControl;
            _clusterOverlay = new GMapOverlay("clusters");
            _individualOverlay = new GMapOverlay("filons");
            _allFilons = new List<Filon>();

            // Événement quand le zoom change
            _mapControl.OnMapZoomChanged += () => UpdateClusters();
        }

        /// <summary>
        /// Charge les filons et crée les clusters
        /// </summary>
        public void LoadFilons(List<Filon> filons)
        {
            _allFilons = filons.Where(f => f.HasCoordinates()).ToList();
            UpdateClusters();
        }

        /// <summary>
        /// Met à jour l'affichage des clusters selon le zoom actuel
        /// </summary>
        private void UpdateClusters()
        {
            _clusterOverlay.Markers.Clear();
            _individualOverlay.Markers.Clear();

            // Si zoom élevé (> 13), afficher les marqueurs individuels
            if (_mapControl.Zoom >= 13)
            {
                ShowIndividualMarkers();
            }
            else
            {
                ShowClusteredMarkers();
            }

            _mapControl.Refresh();
        }

        /// <summary>
        /// Affiche les marqueurs individuels (zoom élevé)
        /// </summary>
        private void ShowIndividualMarkers()
        {
            foreach (var filon in _allFilons)
            {
                var point = new PointLatLng(filon.Latitude!.Value, filon.Longitude!.Value);
                var color = MineralColors.GetColor(filon.MatierePrincipale);

                var marker = new FilonCrystalMarker(point, filon, color)
                {
                    ToolTipText = $"{filon.Nom}\n{MineralColors.GetDisplayName(filon.MatierePrincipale)}",
                    Tag = filon
                };

                _individualOverlay.Markers.Add(marker);
            }

            if (!_mapControl.Overlays.Contains(_individualOverlay))
            {
                _mapControl.Overlays.Add(_individualOverlay);
            }

            if (_mapControl.Overlays.Contains(_clusterOverlay))
            {
                _mapControl.Overlays.Remove(_clusterOverlay);
            }
        }

        /// <summary>
        /// Affiche les marqueurs regroupés en clusters (zoom faible)
        /// </summary>
        private void ShowClusteredMarkers()
        {
            var clusters = CreateClusters(_allFilons, CLUSTER_DISTANCE_KM);

            foreach (var cluster in clusters)
            {
                if (cluster.Count == 1)
                {
                    // Un seul filon, afficher normalement
                    var filon = cluster[0];
                    var point = new PointLatLng(filon.Latitude!.Value, filon.Longitude!.Value);
                    var color = MineralColors.GetColor(filon.MatierePrincipale);

                    var marker = new FilonCrystalMarker(point, filon, color)
                    {
                        ToolTipText = $"{filon.Nom}\n{MineralColors.GetDisplayName(filon.MatierePrincipale)}",
                        Tag = filon
                    };

                    _clusterOverlay.Markers.Add(marker);
                }
                else
                {
                    // Plusieurs filons, créer un cluster
                    var centerLat = cluster.Average(f => f.Latitude!.Value);
                    var centerLon = cluster.Average(f => f.Longitude!.Value);
                    var centerPoint = new PointLatLng(centerLat, centerLon);

                    var clusterMarker = new ClusterMarker(centerPoint, cluster);
                    _clusterOverlay.Markers.Add(clusterMarker);
                }
            }

            if (!_mapControl.Overlays.Contains(_clusterOverlay))
            {
                _mapControl.Overlays.Add(_clusterOverlay);
            }

            if (_mapControl.Overlays.Contains(_individualOverlay))
            {
                _mapControl.Overlays.Remove(_individualOverlay);
            }
        }

        /// <summary>
        /// Crée des groupes de filons proches
        /// </summary>
        private List<List<Filon>> CreateClusters(List<Filon> filons, double distanceKm)
        {
            var clusters = new List<List<Filon>>();
            var processed = new HashSet<Guid>();

            foreach (var filon in filons)
            {
                if (processed.Contains(filon.Id))
                    continue;

                var cluster = new List<Filon> { filon };
                processed.Add(filon.Id);

                // Trouver tous les filons proches
                foreach (var other in filons)
                {
                    if (processed.Contains(other.Id))
                        continue;

                    var distance = CalculateDistance(
                        filon.Latitude!.Value, filon.Longitude!.Value,
                        other.Latitude!.Value, other.Longitude!.Value);

                    if (distance <= distanceKm)
                    {
                        cluster.Add(other);
                        processed.Add(other.Id);
                    }
                }

                clusters.Add(cluster);
            }

            return clusters;
        }

        /// <summary>
        /// Calcule la distance entre deux points GPS en kilomètres (formule de Haversine)
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

            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// Nettoie les overlays
        /// </summary>
        public void Clear()
        {
            _clusterOverlay.Markers.Clear();
            _individualOverlay.Markers.Clear();
            _allFilons.Clear();
            _mapControl.Refresh();
        }
    }

    /// <summary>
    /// Marqueur de cluster personnalisé
    /// </summary>
    public class ClusterMarker : GMapMarker
    {
        private readonly List<Filon> _filons;
        private readonly Bitmap _bitmap;

        public List<Filon> Filons => _filons;

        public ClusterMarker(PointLatLng pos, List<Filon> filons)
            : base(pos)
        {
            _filons = filons;
            Size = new Size(50, 50);
            Offset = new Point(-25, -25);

            // Créer l'image du cluster
            _bitmap = CreateClusterBitmap();

            // Tooltip avec la liste des filons
            var tooltip = $"📍 {_filons.Count} filons\n\n";
            tooltip += string.Join("\n", _filons.Take(5).Select(f => $"• {f.Nom}"));
            if (_filons.Count > 5)
                tooltip += $"\n... et {_filons.Count - 5} autres";

            tooltip += "\n\n💡 Cliquez pour zoomer sur cette zone";

            ToolTipText = tooltip;

            // Rendre cliquable
            IsHitTestVisible = true;
        }

        private Bitmap CreateClusterBitmap()
        {
            var bmp = new Bitmap(50, 50);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Cercle extérieur (contour)
                using (var pen = new Pen(Color.White, 3))
                {
                    g.DrawEllipse(pen, 5, 5, 40, 40);
                }

                // Cercle intérieur (fond)
                using (var brush = new SolidBrush(Color.FromArgb(200, 0, 150, 136)))
                {
                    g.FillEllipse(brush, 8, 8, 34, 34);
                }

                // Nombre de filons
                var text = _filons.Count.ToString();
                using (var font = new Font("Segoe UI Emoji", 14, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var textSize = g.MeasureString(text, font);
                    var x = (50 - textSize.Width) / 2;
                    var y = (50 - textSize.Height) / 2;

                    // Ombre du texte
                    using (var shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                    {
                        g.DrawString(text, font, shadowBrush, x + 1, y + 1);
                    }

                    g.DrawString(text, font, brush, x, y);
                }
            }

            return bmp;
        }

        public override void OnRender(Graphics g)
        {
            if (_bitmap != null)
            {
                g.DrawImage(_bitmap, LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
            }
        }

        // Cleanup du bitmap
        ~ClusterMarker()
        {
            _bitmap?.Dispose();
        }
    }
}
