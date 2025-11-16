using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using wmine.Models;
using wmine.Services;

namespace wmine.UI
{
    /// <summary>
    /// Service pour dessiner des zones/polygones sur la carte
    /// </summary>
    public class ZoneDrawingService
    {
        private readonly GMapControl _mapControl;
        private readonly List<PointLatLng> _currentZonePoints = new();
        private GMapOverlay? _drawingOverlay;
        private GMapPolygon? _currentPolygon;
        private bool _isDrawing = false;
        private readonly MeasurementService _measurementService = new();

        public event EventHandler<Zone>? ZoneCompleted;

        public bool IsDrawing => _isDrawing;

        public ZoneDrawingService(GMapControl mapControl)
        {
            _mapControl = mapControl;
        }

        /// <summary>
        /// D�marre le mode dessin de zone
        /// </summary>
        public void StartDrawing()
        {
            _isDrawing = true;
            _currentZonePoints.Clear();

            // Cr�er l'overlay de dessin s'il n'existe pas
            if (_drawingOverlay == null)
            {
                _drawingOverlay = new GMapOverlay("zone_drawing");
                _mapControl.Overlays.Add(_drawingOverlay);
            }
            else
            {
                _drawingOverlay.Clear();
            }

            _mapControl.Cursor = Cursors.Cross;
            MessageBox.Show(
                "Mode Tra�age de Zone activ�\n\n" +
                "� Cliquez pour ajouter des points\n" +
                "� Double-clic pour terminer la zone\n" +
                "� Echap pour annuler",
                "Tracer Zone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        /// <summary>
        /// Ajoute un point � la zone en cours
        /// </summary>
        public void AddPoint(PointLatLng point)
        {
            if (!_isDrawing) return;

            _currentZonePoints.Add(point);

            // Ajouter un marqueur pour chaque point
            var marker = new GMarkerGoogle(point, GMarkerGoogleType.blue_small);
            _drawingOverlay?.Markers.Add(marker);

            // Dessiner le polygone en temps r�el
            if (_currentZonePoints.Count >= 2)
            {
                UpdatePolygon();
            }
        }

        /// <summary>
        /// Met � jour le polygone en cours
        /// </summary>
        private void UpdatePolygon()
        {
            if (_drawingOverlay == null) return;

            // Supprimer l'ancien polygone
            if (_currentPolygon != null)
            {
                _drawingOverlay.Polygons.Remove(_currentPolygon);
            }

            // Cr�er le nouveau polygone
            _currentPolygon = new GMapPolygon(_currentZonePoints, "zone_temp")
            {
                Fill = new SolidBrush(Color.FromArgb(80, 76, 175, 80)),
                Stroke = new Pen(Color.FromArgb(200, 76, 175, 80), 3)
            };

            _drawingOverlay.Polygons.Add(_currentPolygon);
            _mapControl.Refresh();
        }

        /// <summary>
        /// Termine le dessin de la zone
        /// </summary>
        public Zone? CompleteZone(string name)
        {
            if (!_isDrawing || _currentZonePoints.Count < 3)
            {
                MessageBox.Show(
                    "Une zone doit contenir au moins 3 points.",
                    "Zone invalide",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return null;
            }

            _isDrawing = false;
            _mapControl.Cursor = Cursors.Default;

            // Calculer les statistiques de la zone
            var area = _measurementService.CalculatePolygonArea(_currentZonePoints);
            var perimeter = _measurementService.CalculatePathDistance(_currentZonePoints);

            var zone = new Zone
            {
                Id = Guid.NewGuid(),
                Name = name,
                Points = new List<PointLatLng>(_currentZonePoints),
                Area = area,
                Perimeter = perimeter,
                CreatedDate = DateTime.Now,
                Color = Color.FromArgb(80, 76, 175, 80)
            };

            // Notifier la compl�tion
            ZoneCompleted?.Invoke(this, zone);

            // Nettoyer
            _currentZonePoints.Clear();
            _drawingOverlay?.Clear();

            return zone;
        }

        /// <summary>
        /// Annule le dessin en cours
        /// </summary>
        public void CancelDrawing()
        {
            _isDrawing = false;
            _mapControl.Cursor = Cursors.Default;
            _currentZonePoints.Clear();
            _drawingOverlay?.Clear();
            _mapControl.Refresh();
        }

        /// <summary>
        /// Affiche une zone sauvegard�e sur la carte
        /// </summary>
        public void ShowZone(Zone zone)
        {
            if (_drawingOverlay == null)
            {
                _drawingOverlay = new GMapOverlay("zone_drawing");
                _mapControl.Overlays.Add(_drawingOverlay);
            }

            var polygon = new GMapPolygon(zone.Points, zone.Name)
            {
                Fill = new SolidBrush(zone.Color),
                Stroke = new Pen(Color.FromArgb(200, zone.Color), 3)
            };

            _drawingOverlay.Polygons.Add(polygon);
            _mapControl.Refresh();
        }

        /// <summary>
        /// Trouve tous les filons dans une zone
        /// </summary>
        public List<Filon> FindFilonsInZone(Zone zone, List<Filon> allFilons)
        {
            var filonsInZone = new List<Filon>();

            foreach (var filon in allFilons)
            {
                if (!filon.Latitude.HasValue || !filon.Longitude.HasValue)
                    continue;

                var point = new PointLatLng(filon.Latitude.Value, filon.Longitude.Value);

                if (IsPointInPolygon(point, zone.Points))
                {
                    filonsInZone.Add(filon);
                }
            }

            return filonsInZone;
        }

        /// <summary>
        /// V�rifie si un point est dans un polygone (algorithme Ray Casting)
        /// </summary>
        private bool IsPointInPolygon(PointLatLng point, List<PointLatLng> polygon)
        {
            int count = polygon.Count;
            bool inside = false;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                if (((polygon[i].Lng > point.Lng) != (polygon[j].Lng > point.Lng)) &&
                    (point.Lat < (polygon[j].Lat - polygon[i].Lat) * (point.Lng - polygon[i].Lng) /
                    (polygon[j].Lng - polygon[i].Lng) + polygon[i].Lat))
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }

    /// <summary>
    /// Mod�le repr�sentant une zone trac�e
    /// </summary>
    public class Zone
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<PointLatLng> Points { get; set; } = new();
        public double Area { get; set; } // km�
        public double Perimeter { get; set; } // km
        public DateTime CreatedDate { get; set; }
        public Color Color { get; set; }
        public string? Description { get; set; }
    }
}
