using System.Drawing.Drawing2D;

namespace wmine.UI
{
    /// <summary>
    /// Contrôle flottant de sélection de carte qui s'affiche sur la carte
    /// </summary>
    public class FloatingMapSelector : Panel
    {
        private ComboBox _cmbMapType;
        private bool _isExpanded = false;
        private Button _btnToggle;
        private Panel _contentPanel;
        private const int CollapsedWidth = 50;
        private const int ExpandedWidth = 280;

        public event EventHandler<Models.MapType>? MapTypeChanged;

        public Models.MapType CurrentMapType { get; private set; } = Models.MapType.OpenStreetMap;

        public FloatingMapSelector()
        {
            // Configuration pour assurer la visibilité avec transparence
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Panel principal
            this.Width = CollapsedWidth;
            this.Height = 50;
            this.BackColor = Color.FromArgb(220, 30, 35, 45); // Semi-transparent
            this.Cursor = Cursors.Hand;

            // Bouton toggle avec icône Segoe MDL2 Assets
            _btnToggle = new Button
            {
                Width = 40,
                Height = 40,
                Location = new Point(5, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 18),
                Text = "\uE707", // Icône carte moderne (Map)
                Cursor = Cursors.Hand
            };
            _btnToggle.FlatAppearance.BorderSize = 0;
            _btnToggle.Click += BtnToggle_Click;
            this.Controls.Add(_btnToggle);

            // Panel de contenu (caché par défaut)
            _contentPanel = new Panel
            {
                Location = new Point(55, 5),
                Width = ExpandedWidth - 65,
                Height = 40,
                BackColor = Color.Transparent,
                Visible = false
            };

            // Label
            var lblTitle = new Label
            {
                Text = "Type:",
                Location = new Point(5, 12),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            _contentPanel.Controls.Add(lblTitle);

            // ComboBox
            _cmbMapType = new ComboBox
            {
                Location = new Point(50, 8),
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI Emoji", 8),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _cmbMapType.SelectedIndexChanged += CmbMapType_SelectedIndexChanged;
            _contentPanel.Controls.Add(_cmbMapType);

            this.Controls.Add(_contentPanel);

            LoadMapTypes();
        }

        private void LoadMapTypes()
        {
            var mapTypes = new List<MapTypeItem>();

            foreach (Models.MapType mapType in Enum.GetValues(typeof(Models.MapType)))
            {
                // Filtrer les cartes topographiques non fonctionnelles
                if (mapType == Models.MapType.OpenTopoMap ||
                    mapType == Models.MapType.EsriWorldTopo)
                {
                    continue;
                }

                mapTypes.Add(new MapTypeItem
                {
                    Type = mapType,
                    DisplayName = GetShortDisplayName(mapType)
                });
            }

            _cmbMapType.DataSource = mapTypes;
            _cmbMapType.DisplayMember = "DisplayName";
            _cmbMapType.ValueMember = "Type";
            _cmbMapType.SelectedValue = Models.MapType.OpenStreetMap;
        }

        private string GetShortDisplayName(Models.MapType mapType)
        {
            return mapType switch
            {
                Models.MapType.OpenStreetMap => "OSM Standard",
                Models.MapType.GoogleMaps => "Google Maps",
                Models.MapType.GoogleTerrain => "Google Terrain",
                Models.MapType.EsriSatellite => "Esri Satellite",
                Models.MapType.BingSatellite => "Bing Satellite",
                Models.MapType.GoogleSatellite => "Google Satellite",
                Models.MapType.GoogleHybrid => "Google Hybride",
                _ => mapType.ToString()
            };
        }

        private void BtnToggle_Click(object? sender, EventArgs e)
        {
            _isExpanded = !_isExpanded;

            if (_isExpanded)
            {
                // Expand
                this.Width = ExpandedWidth;
                _contentPanel.Visible = true;
                _btnToggle.Text = "\uE00F"; // Icône flèche gauche moderne (ChevronLeft)
            }
            else
            {
                // Collapse
                this.Width = CollapsedWidth;
                _contentPanel.Visible = false;
                _btnToggle.Text = "\uE707"; // Icône carte moderne (Map)
            }

            this.Invalidate();
        }

        private void CmbMapType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_cmbMapType.SelectedItem is MapTypeItem item)
            {
                CurrentMapType = item.Type;
                MapTypeChanged?.Invoke(this, item.Type);
            }
        }

        private GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            if (diameter > bounds.Width) diameter = bounds.Width;
            if (diameter > bounds.Height) diameter = bounds.Height;

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        // Classe helper pour le ComboBox
        private class MapTypeItem
        {
            public Models.MapType Type { get; set; }
            public string DisplayName { get; set; } = "";

            public override string ToString() => DisplayName;
        }
    }
}

