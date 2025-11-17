using wmine.Models;

namespace wmine.UI
{
    /// <summary>
    /// Contrôle flottant de sélection de type de carte (aligné à droite).
    /// </summary>
    public class FloatingMapSelector : Panel
    {
        private ComboBox _cmbMapType;
        private bool _isExpanded;
        private Button _btnToggle;
        private Panel _contentPanel;

        private const int CollapsedWidth = 50;
        private const int ExpandedWidth = 280;

        // Alignement
        private bool _alignRight = true;
        private int _topMargin = 20;

        public event EventHandler<MapType>? MapTypeChanged;
        public MapType CurrentMapType { get; private set; } = MapType.OpenStreetMap;

        public bool AlignRight
        {
            get => _alignRight;
            set { _alignRight = value; RealignRight(); }
        }

        public int TopMargin
        {
            get => _topMargin;
            set { _topMargin = value; RealignRight(); }
        }

        public FloatingMapSelector()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Width = CollapsedWidth;
            Height = 50;
            BackColor = Color.FromArgb(220, 30, 35, 45);
            Cursor = Cursors.Hand;

            _btnToggle = new Button
            {
                Width = 40,
                Height = 40,
                Location = new Point(5, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 18),
                Text = "\uE707", // Icône carte
                Cursor = Cursors.Hand
            };
            _btnToggle.FlatAppearance.BorderSize = 0;
            _btnToggle.Click += BtnToggle_Click;
            Controls.Add(_btnToggle);

            _contentPanel = new Panel
            {
                Location = new Point(55, 5),
                Width = ExpandedWidth - 65,
                Height = 40,
                BackColor = Color.Transparent,
                Visible = false
            };

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

            Controls.Add(_contentPanel);

            LoadMapTypes();
        }

        private void LoadMapTypes()
        {
            var list = new List<MapTypeItem>();
            foreach (MapType mapType in Enum.GetValues(typeof(MapType)))
            {
                if (mapType == MapType.OpenTopoMap || mapType == MapType.EsriWorldTopo)
                    continue;

                list.Add(new MapTypeItem
                {
                    Type = mapType,
                    DisplayName = GetShortDisplayName(mapType)
                });
            }

            _cmbMapType.DataSource = list;
            _cmbMapType.DisplayMember = "DisplayName";
            _cmbMapType.ValueMember = "Type";
            _cmbMapType.SelectedValue = MapType.OpenStreetMap;
        }

        private string GetShortDisplayName(MapType mapType) =>
            mapType switch
            {
                MapType.OpenStreetMap => "OSM Standard",
                MapType.GoogleMaps => "Google Maps",
                MapType.GoogleTerrain => "Google Terrain",
                MapType.EsriSatellite => "Esri Satellite",
                MapType.BingSatellite => "Bing Satellite",
                MapType.GoogleSatellite => "Google Satellite",
                MapType.GoogleHybrid => "Google Hybride",
                _ => mapType.ToString()
            };

        private void BtnToggle_Click(object? sender, EventArgs e)
        {
            _isExpanded = !_isExpanded;
            if (_isExpanded)
            {
                Width = ExpandedWidth;
                _contentPanel.Visible = true;
                _btnToggle.Text = "\uE00F"; // Chevron gauche
            }
            else
            {
                Width = CollapsedWidth;
                _contentPanel.Visible = false;
                _btnToggle.Text = "\uE707"; // Icône carte
            }

            Invalidate();
            RealignRight();         // réalignement après changement de largeur
            ClampChildrenInside();  // garde les sous-contrôles visibles
        }

        private void CmbMapType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_cmbMapType.SelectedItem is MapTypeItem item)
            {
                CurrentMapType = item.Type;
                MapTypeChanged?.Invoke(this, item.Type);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            HookParentResize();
            RealignRight();
            ClampChildrenInside();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            HookParentResize();
            RealignRight();
            ClampChildrenInside();
        }

        private void HookParentResize()
        {
            if (Parent == null) return;
            Parent.Resize -= Parent_Resize;
            Parent.Resize += Parent_Resize;
        }

        private void Parent_Resize(object? sender, EventArgs e)
        {
            RealignRight();
            ClampChildrenInside();
        }

        private void RealignRight()
        {
            if (!AlignRight || Parent == null) return;
            int x = Parent.ClientSize.Width - Width - 20;
            int y = TopMargin;
            Location = new Point(Math.Max(0, x), Math.Max(0, y));
            Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }

        private void ClampChildrenInside(int padding = 6)
        {
            foreach (Control c in Controls)
            {
                int nl = Math.Min(Math.Max(c.Left, padding), ClientSize.Width - c.Width - padding);
                int nt = Math.Min(Math.Max(c.Top, padding), ClientSize.Height - c.Height - padding);
                if (nl != c.Left || nt != c.Top)
                    c.Location = new Point(nl, nt);

                if (c is Panel p)
                {
                    foreach (Control cc in p.Controls)
                    {
                        int pnl = Math.Min(Math.Max(cc.Left, padding), p.ClientSize.Width - cc.Width - padding);
                        int pnt = Math.Min(Math.Max(cc.Top, padding), p.ClientSize.Height - cc.Height - padding);
                        if (pnl != cc.Left || pnt != cc.Top)
                            cc.Location = new Point(pnl, pnt);
                    }
                }
            }
        }

        private class MapTypeItem
        {
            public MapType Type { get; set; }
            public string DisplayName { get; set; } = "";
            public override string ToString() => DisplayName;
        }
    }
}

