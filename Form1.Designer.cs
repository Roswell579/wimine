using GMap.NET.WindowsForms;
using wmine.Models;
using wmine.UI;
using wmine.Services;

namespace wmine
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private Panel panelTop;
        private Button btnAddFilon;        // ? CHANGÉ de TransparentGlassButton
        private Button btnEditFilon;       // ? CHANGÉ
        private Button btnDeleteFilon;     // ? CHANGÉ
        private Button btnExportPdf;       // ? CHANGÉ
        private Button btnShareEmail;      // ? CHANGÉ
        private ColoredMineralComboBox cmbFilterMineral;
        private ComboBox cmbSelectFilon;
        private Label lblFilter;
        private Label lblSelectFilon;
        private Button btnViewFiches;      // ? CHANGÉ
        
        // ? NOUVEAUX CHAMPS POUR LES ONGLETS
        private TabControl mainTabControl;
        private TabPage tabPageMap;  // ? NOUVEL ONGLET POUR LA CARTE
        private TabPage tabPageMinerals;
        private TabPage tabPageImport;
        private TabPage tabPageTechniques;
        private TabPage tabPageContacts;
        private TabPage tabPageSettings;
        
        private GMapControl gMapControl;
        private ToolStrip toolStrip1;
        private ToolStripButton btnZoomIn;
        private ToolStripButton btnZoomOut;
        private ToolStripButton btnResetZoom;
        private ToolStripButton btnFullScreen;
        private Panel panelMap;
        private Panel panelTopBar;
        private Panel panelButtons;
        private Panel panelSelectors;
        private bool _isFullScreen = false;
        private SimpleMapSelector _simpleMapSelector; // ? NOUVEAU - remplace FloatingMapSelector
        private FloatingPositionIndicator _floatingPosition;
        private FloatingScaleBar _floatingScale;
        private MapBoundsRestrictor _mapBoundsRestrictor;
        private Button _btnToggleTopBar;
        private bool _isTopBarVisible = true;
        private Button _btnToggleTabs;
        private bool _areTabsVisible = true;
        private WeatherWidget _weatherWidget; // ? NOUVEAU
        private FloatingToolsPanel _floatingToolsPanel; // ?? NOUVEAU
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            
            // Form
            this.Text = "WMine - Localisateur de Filons Miniers";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(25, 25, 35);
            this.Opacity = 0; // Pour l'animation FadeIn

            // ??? INITIALISER panelMap ET panelTopBar EN PREMIER ???
            panelMap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 35)
            };

            panelTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 130,
                BackColor = Color.FromArgb(30, 35, 45),
                Padding = new Padding(20, 10, 20, 10)
            };

            // ??? CRÉER LES CONTRÔLES DU PANEL TOP BAR ???
            panelButtons = new Panel
            {
                Location = new Point(20, 10),
                Width = 700,
                Height = 50
            };

            btnAddFilon = new Button
            {
                Text = "Nouveau",
                Width = 120,
                Height = 40,
                Location = new Point(0, 5),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddFilon.FlatAppearance.BorderSize = 0;
            btnAddFilon.Click += BtnAddFilon_Click;

            btnEditFilon = new Button
            {
                Text = "Éditer",
                Width = 100,
                Height = 40,
                Location = new Point(130, 5),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEditFilon.FlatAppearance.BorderSize = 0;
            btnEditFilon.Click += BtnEditFilon_Click;

            btnDeleteFilon = new Button
            {
                Text = "Supprimer",
                Width = 120,
                Height = 40,
                Location = new Point(240, 5),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeleteFilon.FlatAppearance.BorderSize = 0;
            btnDeleteFilon.Click += BtnDeleteFilon_Click;

            btnExportPdf = new Button
            {
                Text = "PDF",
                Width = 80,
                Height = 40,
                Location = new Point(370, 5),
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExportPdf.FlatAppearance.BorderSize = 0;
            btnExportPdf.Click += BtnExportPdf_Click;

            btnShareEmail = new Button
            {
                Text = "Email",
                Width = 90,
                Height = 40,
                Location = new Point(460, 5),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnShareEmail.FlatAppearance.BorderSize = 0;
            btnShareEmail.Click += BtnShareEmail_Click;

            btnViewFiches = new Button
            {
                Text = "Fiches (0)",
                Width = 130,
                Height = 40,
                Location = new Point(560, 5),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnViewFiches.FlatAppearance.BorderSize = 0;
            btnViewFiches.Click += BtnViewFiches_Click;

            // Bouton de recherche géographique
            var btnSearchLocation = new Button
            {
                Text = "Lieu",
                Width = 100,
                Height = 40,
                Location = new Point(700, 5),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearchLocation.FlatAppearance.BorderSize = 0;
            btnSearchLocation.Click += (s, e) =>
            {
                if (_searchControl != null)
                {
                    var location = new Point(
                        (panelMap.Width - _searchControl.Width) / 2,
                        50
                    );
                    _searchControl.Show(location);
                }
            };

            panelButtons.Controls.AddRange(new Control[] { btnAddFilon, btnEditFilon, btnDeleteFilon, btnExportPdf, btnShareEmail, btnViewFiches, btnSearchLocation });

            panelSelectors = new Panel
            {
                Location = new Point(20, 65),
                Width = 900,
                Height = 50
            };

            lblFilter = new Label
            {
                Text = "Filtrer par minéral:",
                Location = new Point(0, 8),
                Width = 120,
                Height = 20,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold)
            };

            cmbFilterMineral = new ColoredMineralComboBox
            {
                Location = new Point(130, 5),
                Width = 200,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI Emoji", 10)
            };
            cmbFilterMineral.SelectedIndexChanged += CmbFilterMineral_SelectedIndexChanged;

            lblSelectFilon = new Label
            {
                Text = "Sélectionner un filon:",
                Location = new Point(350, 8),
                Width = 130,
                Height = 20,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold)
            };

            cmbSelectFilon = new ComboBox
            {
                Location = new Point(490, 5),
                Width = 300,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI Emoji", 10),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cmbSelectFilon.SelectedIndexChanged += CmbSelectFilon_SelectedIndexChanged;

            panelSelectors.Controls.AddRange(new Control[] { lblFilter, cmbFilterMineral, lblSelectFilon, cmbSelectFilon });

            panelTopBar.Controls.AddRange(new Control[] { panelButtons, panelSelectors });

            // ??? CRÉER LE TABCONTROL EN PREMIER ???
            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                ItemSize = new Size(220, 45),
                SizeMode = TabSizeMode.Fixed,
                Appearance = TabAppearance.Normal,
                BackColor = Color.FromArgb(25, 25, 35),
                ForeColor = Color.White,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                Alignment = TabAlignment.Bottom
            };
            mainTabControl.DrawItem += MainTabControl_DrawItem;
            mainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;

            // ??? ONGLET 1 : CARTE INTERACTIVE (NOUVEL ONGLET) ???
            tabPageMap = new TabPage
            {
                Text = "Carte",
                BackColor = Color.FromArgb(25, 25, 35),
                AutoScroll = false
            };

            // ??? ONGLET 2 : MINÉRAUX (CORRECTION - AJOUT DU PANEL!) ???
            tabPageMinerals = new TabPage
            {
                Text = "Minéraux",
                BackColor = Color.FromArgb(25, 25, 35),
                AutoScroll = true
            };
            // ? AJOUT DU MINERALS PANEL QUI MANQUAIT!
            var mineralsRepo = Services.AppServiceProvider.GetService<IMineralRepository>() ?? new MineralRepository();
            var photoService = Services.AppServiceProvider.GetService<PhotoService>() ?? new PhotoService();
            var filonDataService = Services.AppServiceProvider.GetService<FilonDataService>() ?? new FilonDataService();
            var mineralsPanel = new Forms.MineralsPanel(filonDataService, mineralsRepo, photoService);
            tabPageMinerals.Controls.Add(mineralsPanel);

            // ONGLET 3 : Import OCR
            tabPageImport = new TabPage
            {
                Text = "Import",
                BackColor = Color.FromArgb(25, 25, 35),
                AutoScroll = true
            };

            // ONGLET 4 : Techniques
            tabPageTechniques = new TabPage
            {
                Text = "Techniques",
                BackColor = Color.FromArgb(25, 25, 35),
                AutoScroll = true
            };
            var panelTechniques = new Forms.TechniquesEditPanel();
            tabPageTechniques.Controls.Add(panelTechniques);

            // ONGLET 5 : Contacts
            tabPageContacts = new TabPage
            {
                Text = "Contacts",
                BackColor = Color.FromArgb(25, 25, 35),
                AutoScroll = true
            };
            var panelContacts = new Forms.ContactsPanel();
            tabPageContacts.Controls.Add(panelContacts);

            // ONGLET 6 : Paramètres
            tabPageSettings = new TabPage
            {
                Text = "Paramètres",
                BackColor = Color.FromArgb(25, 25, 35),
                AutoScroll = true
            };
            var panelSettings = new Forms.SettingsPanel();
            tabPageSettings.Controls.Add(panelSettings);

            // ? AJOUTER LES ONGLETS AU TABCONTROL
            mainTabControl.TabPages.Add(tabPageMap);
            mainTabControl.TabPages.Add(tabPageMinerals);
            mainTabControl.TabPages.Add(tabPageImport);
            mainTabControl.TabPages.Add(tabPageTechniques);
            mainTabControl.TabPages.Add(tabPageContacts);
            mainTabControl.TabPages.Add(tabPageSettings);

            // ToolStrip pour les contrôles de carte
            toolStrip1 = new ToolStrip 
            { 
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(35, 40, 50),
                ForeColor = Color.White,
                Renderer = new ToolStripProfessionalRenderer(new DarkColorTable())
            };

            btnZoomIn = new ToolStripButton("Zoom +");
            btnZoomIn.Click += (s, e) => { if (gMapControl.Zoom < gMapControl.MaxZoom) gMapControl.Zoom++; };

            btnZoomOut = new ToolStripButton("Zoom -");
            btnZoomOut.Click += (s, e) => { if (gMapControl.Zoom > gMapControl.MinZoom) gMapControl.Zoom--; };

            btnResetZoom = new ToolStripButton("Reinitialiser");
            btnResetZoom.Click += (s, e) => 
            { 
                gMapControl.Position = new GMap.NET.PointLatLng(43.4, 6.3);
                gMapControl.Zoom = 10;
            };

            btnFullScreen = new ToolStripButton("Plein ecran");
            btnFullScreen.Click += BtnFullScreen_Click;

            var btnMapSelector = new ToolStripButton("Type de carte");
            btnMapSelector.Click += BtnMapSelector_Click;

            toolStrip1.Items.AddRange(new ToolStripItem[] 
            { 
                btnZoomIn, 
                btnZoomOut, 
                btnResetZoom, 
                new ToolStripSeparator(), 
                btnMapSelector,
                new ToolStripSeparator(),
                btnFullScreen 
            });

            // Carte
            gMapControl = new GMapControl
            {
                Dock = DockStyle.Fill,
                Bearing = 0f,
                CanDragMap = true,
                EmptyTileColor = Color.FromArgb(30, 30, 40),
                BackColor = Color.FromArgb(25, 25, 35),
                GrayScaleMode = false,
                HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow,
                LevelsKeepInMemory = 5,
                MarkersEnabled = true,
                MaxZoom = 18,
                MinZoom = 9,
                MouseWheelZoomEnabled = true,
                MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter,
                NegativeMode = false,
                PolygonsEnabled = true,
                RetryLoadTile = 0,
                RoutesEnabled = true,
                ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer,
                SelectedAreaFillColor = Color.FromArgb(33, 65, 105, 225),
                ShowTileGridLines = false,
                Zoom = 10D
            };

            // Initialiser le restricteur de limites
            _mapBoundsRestrictor = new Services.MapBoundsRestrictor(gMapControl);

            // Initialiser les indicateurs flottants
            _floatingPosition = new FloatingPositionIndicator
            {
                Location = new Point(this.ClientSize.Width - 220, this.ClientSize.Height - 120),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            panelMap.Controls.Add(_floatingPosition);
            _floatingPosition.BringToFront();

            _floatingScale = new FloatingScaleBar
            {
                Location = new Point(this.ClientSize.Width - 220, this.ClientSize.Height - 60),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            panelMap.Controls.Add(_floatingScale);
            _floatingScale.BringToFront();

            // Événement pour restreindre les déplacements
            gMapControl.OnPositionChanged += (point) =>
            {
                _mapBoundsRestrictor.RestrictMapBounds();
                _floatingPosition.UpdatePosition(point.Lat, point.Lng, gMapControl.Zoom);
                _floatingScale.UpdateScale(gMapControl.Zoom, point.Lat);
            };

            gMapControl.OnMapZoomChanged += () =>
            {
                _floatingPosition.UpdatePosition(gMapControl.Position.Lat, gMapControl.Position.Lng, gMapControl.Zoom);
                _floatingScale.UpdateScale(gMapControl.Zoom, gMapControl.Position.Lat);
            };

            panelMap.Controls.Add(gMapControl);
            panelMap.Controls.Add(toolStrip1);

            // Nouveau sélecteur de carte simple et harmonisé
            _simpleMapSelector = new SimpleMapSelector
            {
                Location = new Point(20, 150),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _simpleMapSelector.MapTypeChanged += (s, mapType) =>
            {
                ChangeMapType(mapType);
            };
            panelMap.Controls.Add(_simpleMapSelector);
            _simpleMapSelector.BringToFront();

            // Activer les événements de rotation
            gMapControl.MouseDown += GMapControl_MouseDown;
            gMapControl.MouseMove += GMapControl_MouseMove_Rotation;
            gMapControl.MouseUp += GMapControl_MouseUp_Rotation;

            // Bouton rotation 90°
            var btnRotateMap = new Button
            {
                Text = "",
                Width = 50,
                Height = 50,
                Location = new Point(20, 220),
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 18),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnRotateMap.FlatAppearance.BorderSize = 0;
            // Use generated icon for rotation
            try
            {
                btnRotateMap.Image = wmine.UI.IconFactory.CreateGlyphIcon("↻", 28, Color.White);
                btnRotateMap.ImageAlign = ContentAlignment.MiddleCenter;
            }
            catch { btnRotateMap.Text = "↻"; }
            btnRotateMap.Click += (s, e) => { gMapControl.Bearing = (gMapControl.Bearing + 90) % 360; };
            panelMap.Controls.Add(btnRotateMap);
            btnRotateMap.BringToFront();

            // Bouton reset orientation
            var btnResetOrientation = new Button
            {
                Text = "",
                Width = 50,
                Height = 50,
                Location = new Point(80, 220),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 18),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnResetOrientation.FlatAppearance.BorderSize = 0;
            try
            {
                btnResetOrientation.Image = wmine.UI.IconFactory.CreateGlyphIcon("🧭", 28, Color.White);
                btnResetOrientation.ImageAlign = ContentAlignment.MiddleCenter;
            }
            catch { btnResetOrientation.Text = "N"; }
            btnResetOrientation.Click += (s, e) => { gMapControl.Bearing = 0f; };
            panelMap.Controls.Add(btnResetOrientation);
            btnResetOrientation.BringToFront();

            // Bouton Itinéraire (route)
            var btnRoute = new Button
            {
                Text = "",
                Width = 50,
                Height = 50,
                Location = new Point(140, 220),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 18),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnRoute.FlatAppearance.BorderSize = 0;
            try
            {
                btnRoute.Image = wmine.UI.IconFactory.CreateGlyphIcon("🧭", 28, Color.White); // reuse compass for route
                btnRoute.ImageAlign = ContentAlignment.MiddleCenter;
            }
            catch { btnRoute.Text = "R"; }
            btnRoute.Click += BtnRoute_Click;
            panelMap.Controls.Add(btnRoute);
            btnRoute.BringToFront();

            // ??? WIDGET MÉTÉO (sous les boutons) ???
            _weatherWidget = new WeatherWidget
            {
                Location = new Point(20, 280),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            panelMap.Controls.Add(_weatherWidget);
            _weatherWidget.BringToFront();

            // ??? PANNEAU OUTILS (sous le widget météo) ???
            _floatingToolsPanel = new FloatingToolsPanel
            {
                Location = new Point(20, 470), // Sous le widget météo
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _floatingToolsPanel.MeasureDistanceClicked += FloatingToolsPanel_MeasureDistanceClicked;
            _floatingToolsPanel.ImportPhotosGPSClicked += FloatingToolsPanel_ImportPhotosGPSClicked;
            _floatingToolsPanel.FindNearbyFilonsClicked += FloatingToolsPanel_FindNearbyFilonsClicked;
            _floatingToolsPanel.DrawZoneClicked += FloatingToolsPanel_DrawZoneClicked;
            _floatingToolsPanel.ExportKMZClicked += FloatingToolsPanel_ExportKMZClicked;
            panelMap.Controls.Add(_floatingToolsPanel);
            _floatingToolsPanel.BringToFront();

            // Bouton toggle TopBar
            _btnToggleTopBar = new Button
            {
                Text = "CACHER",
                Width = 120,
                Height = 30,
                Location = new Point(this.ClientSize.Width / 2 - 60, 140),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.None
            };
            _btnToggleTopBar.FlatAppearance.BorderSize = 0;
            _btnToggleTopBar.Click += BtnToggleTopBar_Click;
            panelMap.Controls.Add(_btnToggleTopBar);
            _btnToggleTopBar.BringToFront();

            // Bouton toggle Tabs
            _btnToggleTabs = new Button
            {
                Text = "Masquer",
                Width = 120,
                Height = 30,
                Location = new Point(this.ClientSize.Width - 140, this.ClientSize.Height - 110),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            _btnToggleTabs.FlatAppearance.BorderSize = 0;
            _btnToggleTabs.Click += BtnToggleTabs_Click;
            panelMap.Controls.Add(_btnToggleTabs);
            _btnToggleTabs.BringToFront();

            // Tooltips pour les boutons
            var tooltipMain = new ToolTip(this.components)
            {
                InitialDelay = 300,
                AutoPopDelay = 5000,
                ReshowDelay = 100,
                ShowAlways = true
            };

            tooltipMain.SetToolTip(btnRotateMap, "Rotation 90° (ou Ctrl+Glisser sur la carte)");
            tooltipMain.SetToolTip(btnResetOrientation, "Réinitialiser l'orientation Nord (le zoom ne change pas)");
            tooltipMain.SetToolTip(btnRoute, "Calculer un itinéraire vers un filon");
            tooltipMain.SetToolTip(_btnToggleTopBar, "Afficher/masquer le panneau de commandes");
            tooltipMain.SetToolTip(_btnToggleTabs, "Afficher/masquer les onglets");
            tooltipMain.SetToolTip(btnAddFilon, "Créer un nouveau filon minier");
            tooltipMain.SetToolTip(btnEditFilon, "Modifier le filon sélectionné");
            tooltipMain.SetToolTip(btnDeleteFilon, "Supprimer définitivement le filon");
            tooltipMain.SetToolTip(btnExportPdf, "Exporter la fiche du filon en PDF");
            tooltipMain.SetToolTip(btnShareEmail, "Partager le filon par email");
            tooltipMain.SetToolTip(btnViewFiches, "Afficher la liste complète des filons");
            tooltipMain.SetToolTip(cmbSelectFilon, "Sélectionner un filon dans la liste");
            tooltipMain.SetToolTip(cmbFilterMineral, "Filtrer les filons par type de minéral");

            // Ajouter les contrôles au formulaire
            tabPageMap.Controls.Add(panelMap);
            this.Controls.Add(panelTopBar);
            this.Controls.Add(mainTabControl);
        }

        private void BtnFullScreen_Click(object? sender, EventArgs e)
        {
            _isFullScreen = !_isFullScreen;

            if (_isFullScreen)
            {
                panelTopBar.Visible = false;
                mainTabControl.Visible = false;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                btnFullScreen.Text = "Quitter plein ecran";
            }
            else
            {
                panelTopBar.Visible = true;
                mainTabControl.Visible = true;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                btnFullScreen.Text = "Plein ecran";
            }
        }

        private void BtnMapSelector_Click(object? sender, EventArgs e)
        {
            using (var selectorForm = new Form())
            {
                selectorForm.Text = "Selectionner le type de carte";
                selectorForm.Size = new Size(380, 320);
                selectorForm.StartPosition = FormStartPosition.CenterParent;
                selectorForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                selectorForm.MaximizeBox = false;
                selectorForm.MinimizeBox = false;
                selectorForm.BackColor = Color.FromArgb(25, 25, 35);

                var mapSelector = new MapSelectorPanel
                {
                    Location = new Point(10, 10),
                    CurrentMapType = Models.MapType.OpenStreetMap
                };

                mapSelector.MapTypeChanged += (s, mapType) =>
                {
                    ChangeMapType(mapType);
                };

                selectorForm.Controls.Add(mapSelector);
                selectorForm.ShowDialog(this);
            }
        }

        private void BtnToggleTopBar_Click(object? sender, EventArgs e)
        {
            _isTopBarVisible = !_isTopBarVisible;

            if (_isTopBarVisible)
            {
                panelTopBar.Visible = true;
                _btnToggleTopBar.Text = "CACHER";
                _btnToggleTopBar.Location = new Point(this.ClientSize.Width / 2 - 60, 140);
                _simpleMapSelector.Location = new Point(20, 170);
            }
            else
            {
                panelTopBar.Visible = false;
                _btnToggleTopBar.Text = "AFFICHER";
                _btnToggleTopBar.Location = new Point(this.ClientSize.Width / 2 - 60, 10);
                _simpleMapSelector.Location = new Point(20, 60);
            }
        }

        private void BtnToggleTabs_Click(object? sender, EventArgs e)
        {
            _areTabsVisible = !_areTabsVisible;

            if (_areTabsVisible)
            {
                // Afficher les onglets
                mainTabControl.ItemSize = new Size(220, 45);
                _btnToggleTabs.Text = "Masquer";
                _btnToggleTabs.Location = new Point(this.ClientSize.Width - 140, this.ClientSize.Height - 110);
            }
            else
            {
                // Masquer les onglets (réduire leur hauteur à 0)
                mainTabControl.ItemSize = new Size(220, 1);
                _btnToggleTabs.Text = "Afficher";
                _btnToggleTabs.Location = new Point(this.ClientSize.Width - 140, this.ClientSize.Height - 60);
            }
        }

        private void MainTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = (TabControl)sender;
            TabPage tabPage = tabControl.TabPages[e.Index];
            Rectangle tabRect = tabControl.GetTabRect(e.Index);

            Color backColor;
            Color borderColor;
            Color textColor;
            
            if (e.Index == tabControl.SelectedIndex)
            {
                backColor = Color.FromArgb(200, 0, 150, 136);
                borderColor = Color.FromArgb(255, 255, 255, 255);
                textColor = Color.White;
            }
            else
            {
                backColor = Color.FromArgb(120, 40, 45, 55);
                borderColor = Color.FromArgb(150, 100, 100, 100);
                textColor = Color.FromArgb(200, 200, 200);
            }

            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, tabRect);
            }

            using (Pen pen = new Pen(borderColor, 2))
            {
                e.Graphics.DrawRectangle(pen, 
                    tabRect.X, 
                    tabRect.Y, 
                    tabRect.Width - 1, 
                    tabRect.Height - 1);
            }

            if (e.Index == tabControl.SelectedIndex)
            {
                using (Pen glowPen = new Pen(Color.FromArgb(100, 0, 255, 200), 4))
                {
                    e.Graphics.DrawRectangle(glowPen, 
                        tabRect.X + 2, 
                        tabRect.Y + 2, 
                        tabRect.Width - 5, 
                        tabRect.Height - 5);
                }
            }

            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
            {
                Rectangle shadowRect = new Rectangle(
                    tabRect.X + 2, 
                    tabRect.Y + 2, 
                    tabRect.Width, 
                    tabRect.Height);
                e.Graphics.DrawString(tabPage.Text, tabControl.Font, shadowBrush, shadowRect, sf);
            }

            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(tabPage.Text, tabControl.Font, textBrush, tabRect, sf);
            }
        }

        private void MainTabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Afficher le TopBar seulement pour l'onglet Carte (index 0)
            if (mainTabControl.SelectedIndex == 0) // Onglet Carte
            {
                panelTopBar.Visible = true;
                _isTopBarVisible = true;
                if (_btnToggleTopBar != null)
                {
                    _btnToggleTopBar.Text = "CACHER";
                }
            }
            else // Autres onglets (Minéraux, Import, Techniques, Contacts, Paramètres)
            {
                panelTopBar.Visible = false;
                _isTopBarVisible = false;
            }
        }
    }
}

