namespace wmine.UI
{
    /// <summary>
    /// Panneau flottant d'outils à gauche de la carte
    /// Contient les boutons pour Mesure, Photos GPS, Recherche proximité, etc.
    /// Hérite de DraggablePanel pour permettre le déplacement
    /// VERSION RÉTRACTABLE
    /// </summary>
    public class FloatingToolsPanel : DraggablePanel
    {
        private readonly List<Button> _toolButtons = new();
        private const int BUTTON_WIDTH = 200;
        private const int BUTTON_HEIGHT = 45;
        private const int BUTTON_SPACING = 10;
        private bool _isExpanded = true;
        private Button _btnToggle;
        private Panel _contentPanel;

        public event EventHandler? MeasureDistanceClicked;
        public event EventHandler? ImportPhotosGPSClicked;
        public event EventHandler? FindNearbyFilonsClicked;
        public event EventHandler? DrawZoneClicked;
        public event EventHandler? ExportKMZClicked;

        public FloatingToolsPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Panel principal
            this.Width = BUTTON_WIDTH + 20;
            this.Height = 60; // Rétracté par défaut
            this.BackColor = Color.FromArgb(180, 30, 35, 45);
            this.BorderStyle = BorderStyle.None;
            this.Padding = new Padding(10);

            int yPos = 15;

            // Titre avec bouton toggle
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Width = this.Width,
                Height = 35,
                BackColor = Color.Transparent
            };

            var lblTitle = new Label
            {
                Text = "OUTILS",
                Location = new Point(10, yPos),
                Width = 150,
                Height = 25,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            headerPanel.Controls.Add(lblTitle);

            // Bouton toggle (use a chevron glyph instead of '?')
            _btnToggle = new Button
            {
                Text = "\uE76C", // chevron down/up in Segoe MDL2 Assets
                Location = new Point(170, yPos - 5),
                Width = 35,
                Height = 30,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe MDL2 Assets", 12, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            _btnToggle.FlatAppearance.BorderSize = 0;
            _btnToggle.Click += BtnToggle_Click;
            headerPanel.Controls.Add(_btnToggle);

            this.Controls.Add(headerPanel);
            yPos = 50;

            // Panel contenu (boutons)
            _contentPanel = new Panel
            {
                Location = new Point(0, yPos),
                Width = this.Width,
                Height = (BUTTON_HEIGHT + BUTTON_SPACING) * 6 + 20,
                BackColor = Color.Transparent,
                Visible = false // Rétracté par défaut
            };

            int contentYPos = 0;

            // 1. Bouton Mesurer Distance
            var btnMeasure = CreateToolButton(
                "Mesurer Distance",
                "Cliquez sur 2 points pour mesurer la distance",
                Color.FromArgb(33, 150, 243),
                contentYPos,
                "\uE7C3" // ruler glyph
            );
            btnMeasure.Click += (s, e) => MeasureDistanceClicked?.Invoke(this, EventArgs.Empty);
            _toolButtons.Add(btnMeasure);
            _contentPanel.Controls.Add(btnMeasure);
            contentYPos += BUTTON_HEIGHT + BUTTON_SPACING;

            // 2. Bouton Import Photos GPS
            var btnPhotos = CreateToolButton(
                "Import Photos GPS",
                "Importer des photos géolocalisées",
                Color.FromArgb(156, 39, 176),
                contentYPos,
                "\uE8B0" // camera glyph
            );
            btnPhotos.Click += (s, e) => ImportPhotosGPSClicked?.Invoke(this, EventArgs.Empty);
            _toolButtons.Add(btnPhotos);
            _contentPanel.Controls.Add(btnPhotos);
            contentYPos += BUTTON_HEIGHT + BUTTON_SPACING;

            // 3. Bouton Filons Proches
            var btnNearby = CreateToolButton(
                "Filons Proches",
                "Trouver les filons dans un rayon",
                Color.FromArgb(255, 152, 0),
                contentYPos,
                "\uE707" // location glyph
            );
            btnNearby.Click += (s, e) => FindNearbyFilonsClicked?.Invoke(this, EventArgs.Empty);
            _toolButtons.Add(btnNearby);
            _contentPanel.Controls.Add(btnNearby);
            contentYPos += BUTTON_HEIGHT + BUTTON_SPACING;

            // 4. Bouton Tracer Zone
            var btnZone = CreateToolButton(
                "Tracer Zone",
                "Dessiner une zone sur la carte",
                Color.FromArgb(76, 175, 80),
                contentYPos,
                "\uE7C5" // draw glyph
            );
            btnZone.Click += (s, e) => DrawZoneClicked?.Invoke(this, EventArgs.Empty);
            _toolButtons.Add(btnZone);
            _contentPanel.Controls.Add(btnZone);
            contentYPos += BUTTON_HEIGHT + BUTTON_SPACING;

            // 5. Bouton Export KMZ
            var btnExportKMZ = CreateToolButton(
                "Export KMZ",
                "Exporter avec photos (Google Earth)",
                Color.FromArgb(0, 150, 136),
                contentYPos,
                "\uE74E" // export glyph
            );
            btnExportKMZ.Click += (s, e) => ExportKMZClicked?.Invoke(this, EventArgs.Empty);
            _toolButtons.Add(btnExportKMZ);
            _contentPanel.Controls.Add(btnExportKMZ);
            contentYPos += BUTTON_HEIGHT + BUTTON_SPACING;

            // 6. Bouton Aide
            var btnHelp = CreateToolButton(
                "Aide",
                "Afficher l'aide des outils",
                Color.FromArgb(96, 125, 139),
                contentYPos,
                "\uE897" // help glyph
            );
            btnHelp.Click += BtnHelp_Click;
            _toolButtons.Add(btnHelp);
            _contentPanel.Controls.Add(btnHelp);

            this.Controls.Add(_contentPanel);
        }

        private void BtnToggle_Click(object? sender, EventArgs e)
        {
            _isExpanded = !_isExpanded;

            if (_isExpanded)
            {
                // Expand
                _contentPanel.Visible = true;
                this.Height = 60 + _contentPanel.Height;
                _btnToggle.Text = "\uE76C"; // chevron up/down depending
            }
            else
            {
                // Collapse
                _contentPanel.Visible = false;
                this.Height = 60;
                _btnToggle.Text = "\uE76C";
            }
        }

        private Button CreateToolButton(string text, string tooltip, Color backColor, int yPos, string iconGlyph)
        {
            var btn = new Button
            {
                Text = $"  " + text,
                Location = new Point(10, yPos),
                Width = BUTTON_WIDTH,
                Height = BUTTON_HEIGHT,
                BackColor = backColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = AdjustBrightness(backColor, 1.2f);

            // Prepend icon as a label inside the button using Segoe MDL2 Assets
            var lblIcon = new Label
            {
                Text = iconGlyph,
                Font = new Font("Segoe MDL2 Assets", 12, FontStyle.Regular),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Width = 30,
                Height = BUTTON_HEIGHT,
                Location = new Point(10, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };

            btn.Controls.Add(lblIcon);

            // Tooltip
            var tip = new ToolTip();
            tip.SetToolTip(btn, tooltip);

            return btn;
        }

        private Color AdjustBrightness(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, (int)(color.R * factor)),
                Math.Min(255, (int)(color.G * factor)),
                Math.Min(255, (int)(color.B * factor))
            );
        }

        private void BtnHelp_Click(object? sender, EventArgs e)
        {
            var helpText = @"OUTILS DE LA CARTE

Mesurer Distance
   Cliquez sur ce bouton, puis cliquez sur 2 points
   de la carte pour mesurer la distance entre eux.
   
 Import Photos GPS
   Importe des photos depuis votre téléphone ou
   appareil photo. Les photos géolocalisées sont
   automatiquement associées aux filons proches.
   
 Filons Proches
   Entrez un rayon de recherche pour trouver tous
   les filons dans cette zone autour d'un point.
   
Tracer Zone
   Dessinez une zone personnalisée sur la carte
   pour délimiter une zone de recherche.
   
 Export KMZ
   Exporte tous vos filons avec leurs photos
   dans un fichier KMZ pour Google Earth.
   
Astuce : Maintenez Ctrl + Glisser pour
   faire pivoter la carte !";

            MessageBox.Show(helpText, "Aide - Outils de la Carte",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Active ou désactive un bouton spécifique
        /// </summary>
        public void SetButtonEnabled(int buttonIndex, bool enabled)
        {
            if (buttonIndex >= 0 && buttonIndex < _toolButtons.Count)
            {
                _toolButtons[buttonIndex].Enabled = enabled;
            }
        }

        /// <summary>
        /// Change la couleur d'un bouton (pour indiquer un mode actif)
        /// </summary>
        public void SetButtonActive(int buttonIndex, bool active)
        {
            if (buttonIndex >= 0 && buttonIndex < _toolButtons.Count)
            {
                var btn = _toolButtons[buttonIndex];
                if (active)
                {
                    btn.BackColor = Color.FromArgb(244, 67, 54); // Rouge = actif
                }
                else
                {
                    // Restaurer couleur d'origine
                    btn.BackColor = buttonIndex switch
                    {
                        0 => Color.FromArgb(33, 150, 243),
                        1 => Color.FromArgb(156, 39, 176),
                        2 => Color.FromArgb(255, 152, 0),
                        3 => Color.FromArgb(76, 175, 80),
                        4 => Color.FromArgb(0, 150, 136),
                        _ => Color.FromArgb(96, 125, 139)
                    };
                }
            }
        }
    }
}
