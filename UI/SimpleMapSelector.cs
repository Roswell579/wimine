using wmine.Models;

namespace wmine.UI
{
    /// <summary>
    /// Sélecteur de type de carte draggable et harmonisé
    /// Version menu déroulant simple
    /// </summary>
    public class SimpleMapSelector : DraggablePanel
    {
        private ComboBox _cmbMapType;
        private Label _lblTitle;

        public event EventHandler<MapType>? MapTypeChanged;
        public MapType CurrentMapType { get; private set; } = MapType.OpenStreetMap;

        public SimpleMapSelector()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Panel principal
            this.Width = 250;
            this.Height = 80;
            this.BackColor = Color.FromArgb(180, 40, 45, 55); // Semi-transparent comme les autres
            this.BorderStyle = BorderStyle.None;
            this.Padding = new Padding(10);

            // Titre
            _lblTitle = new Label
            {
                Text = "Type de carte",
                Location = new Point(10, 10),
                Width = 220,
                Height = 25,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 181, 246),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(_lblTitle);

            // ComboBox
            _cmbMapType = new ComboBox
            {
                Location = new Point(10, 40),
                Width = 230,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI Emoji", 9),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _cmbMapType.SelectedIndexChanged += CmbMapType_SelectedIndexChanged;
            this.Controls.Add(_cmbMapType);

            LoadMapTypes();
        }

        private void LoadMapTypes()
        {
            var mapTypes = new List<MapTypeItem>();

            foreach (MapType mapType in Enum.GetValues(typeof(MapType)))
            {
                // Filtrer les cartes topographiques non fonctionnelles
                if (mapType == MapType.OpenTopoMap ||
                    mapType == MapType.EsriWorldTopo)
                {
                    continue;
                }

                mapTypes.Add(new MapTypeItem
                {
                    Type = mapType,
                    DisplayName = GetDisplayName(mapType)
                });
            }

            _cmbMapType.DataSource = mapTypes;
            _cmbMapType.DisplayMember = "DisplayName";
            _cmbMapType.ValueMember = "Type";
            _cmbMapType.SelectedValue = MapType.OpenStreetMap;
        }

        private string GetDisplayName(MapType mapType)
        {
            return mapType switch
            {
                MapType.OpenStreetMap => "📍 OpenStreetMap",
                MapType.GoogleMaps => "🗺️ Google Maps",
                MapType.GoogleTerrain => "⛰️ Google Terrain",
                MapType.EsriSatellite => "🛰️ Esri Satellite",
                MapType.BingSatellite => "🌍 Bing Satellite",
                MapType.GoogleSatellite => "🛰️ Google Satellite",
                MapType.GoogleHybrid => "🗺️ Google Hybride",
                _ => mapType.ToString()
            };
        }

        private void CmbMapType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_cmbMapType.SelectedItem is MapTypeItem item)
            {
                CurrentMapType = item.Type;
                MapTypeChanged?.Invoke(this, item.Type);
            }
        }

        /// <summary>
        /// Définit le type de carte sélectionné
        /// </summary>
        public void SetMapType(MapType mapType)
        {
            _cmbMapType.SelectedValue = mapType;
        }

        // Classe helper pour le ComboBox
        private class MapTypeItem
        {
            public MapType Type { get; set; }
            public string DisplayName { get; set; } = "";

            public override string ToString() => DisplayName;
        }
    }
}
