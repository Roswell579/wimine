using System.Drawing.Drawing2D;
using wmine.Models;

namespace wmine.UI
{
    /// <summary>
    /// Panneau de sélection de fond de carte avec prévisualisation
    /// </summary>
    public class MapSelectorPanel : Panel
    {
        private ComboBox _cmbMapType;
        private Label _lblDescription;
        private PictureBox _picPreview;
        private Models.MapType _currentMapType = Models.MapType.OpenStreetMap;
        private bool _mapTypesLoaded = false;

        public event EventHandler<Models.MapType>? MapTypeChanged;

        public Models.MapType CurrentMapType
        {
            get => _currentMapType;
            set
            {
                _currentMapType = value;
                UpdateUI();
            }
        }

        public MapSelectorPanel()
        {
            InitializeComponent();
            this.HandleCreated += MapSelectorPanel_HandleCreated;
        }

        private void MapSelectorPanel_HandleCreated(object? sender, EventArgs e)
        {
            if (!_mapTypesLoaded)
            {
                LoadMapTypes();
                _mapTypesLoaded = true;
            }
        }

        private void InitializeComponent()
        {
            this.Width = 350;
            this.Height = 250;
            this.BackColor = Color.FromArgb(30, 35, 45);
            this.Padding = new Padding(10);

            // Titre
            var lblTitle = new Label
            {
                Text = "Type de carte",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblTitle);

            // ComboBox
            _cmbMapType = new ComboBox
            {
                Location = new Point(10, 35),
                Width = 330,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _cmbMapType.SelectedIndexChanged += CmbMapType_SelectedIndexChanged;
            this.Controls.Add(_cmbMapType);

            // Description
            _lblDescription = new Label
            {
                Location = new Point(10, 65),
                Width = 330,
                Height = 40,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopLeft
            };
            this.Controls.Add(_lblDescription);

            // Prévisualisation (icône)
            _picPreview = new PictureBox
            {
                Location = new Point(10, 110),
                Width = 330,
                Height = 130,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(40, 45, 55),
                SizeMode = PictureBoxSizeMode.CenterImage
            };
            this.Controls.Add(_picPreview);
        }

        private void LoadMapTypes()
        {
            var mapTypes = new List<object>();

            foreach (Models.MapType mapType in Enum.GetValues(typeof(Models.MapType)))
            {
                mapTypes.Add(new
                {
                    Value = mapType,
                    Display = mapType.GetDisplayName()
                });
            }

            _cmbMapType.DataSource = mapTypes;
            _cmbMapType.DisplayMember = "Display";
            _cmbMapType.ValueMember = "Value";
            _cmbMapType.SelectedIndex = 0;
        }

        private void CmbMapType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_cmbMapType.SelectedValue is Models.MapType mapType)
            {
                _currentMapType = mapType;
                UpdateUI();
                MapTypeChanged?.Invoke(this, mapType);
            }
        }

        private void UpdateUI()
        {
            _lblDescription.Text = _currentMapType.GetDescription();
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            // Créer une icône simple pour la prévisualisation
            var bitmap = new Bitmap(320, 120);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.FromArgb(40, 45, 55));

                // Couleur selon le type
                var baseColor = _currentMapType switch
                {
                    Models.MapType.OpenStreetMap => Color.FromArgb(200, 200, 200),
                    Models.MapType.OpenTopoMap => Color.FromArgb(210, 180, 140),
                    Models.MapType.GoogleMaps => Color.FromArgb(220, 220, 220),
                    Models.MapType.GoogleTerrain => Color.FromArgb(180, 200, 160),
                    Models.MapType.EsriSatellite => Color.FromArgb(100, 150, 100),
                    Models.MapType.EsriWorldTopo => Color.FromArgb(180, 200, 180),
                    Models.MapType.BingSatellite => Color.FromArgb(80, 120, 80),
                    Models.MapType.GoogleSatellite => Color.FromArgb(90, 130, 90),
                    Models.MapType.GoogleHybrid => Color.FromArgb(120, 140, 120),
                    _ => Color.Gray
                };

                // Dessiner un motif simple
                using (var brush = new SolidBrush(baseColor))
                {
                    // Fond
                    g.FillRectangle(brush, 20, 20, 280, 80);

                    // Lignes pour simuler des détails
                    using (var pen = new Pen(Color.FromArgb(Math.Max(0, baseColor.R - 40), Math.Max(0, baseColor.G - 40), Math.Max(0, baseColor.B - 40)), 2))
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            g.DrawLine(pen, 20, 20 + i * 20, 300, 20 + i * 20);
                        }
                    }
                }

                // Texte du nom
                using (var font = new Font("Segoe UI", 14, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(_currentMapType.GetDisplayName(), font, brush,
                        new Rectangle(0, 0, 320, 120), sf);
                }
            }

            _picPreview.Image?.Dispose();
            _picPreview.Image = bitmap;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _picPreview.Image?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}


