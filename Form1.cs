using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using wmine.Core.Services; // ? NOUVEAU
using wmine.Forms;
using wmine.Models;
using wmine.Services;
using wmine.UI;
using wmine.Utils; // ? NOUVEAU

namespace wmine
{
    public partial class Form1 : Form
    {
        private readonly FilonDataService _dataService;
        private readonly IMineralRepository _mineralRepository; // injected
        private readonly PhotoService _photoService; // injected
        private readonly PdfExportService _pdfService;
        private readonly EmailService _emailService;
        private readonly MapProviderService _mapProviderService;
        private readonly ThemeService _themeService;
        private readonly PerformanceOptimizer _perfOptimizer; // ? NOUVEAU
        private readonly AutoSaveService? _autoSaveService; // ? NOUVEAU
        private readonly MeasurementService _measurementService; // ? NOUVEAU pour zones
        private MarkerClusterService? _clusterService;
        private SearchLocationControl? _searchControl;
        private GMapOverlay _markersOverlay;
        private List<Filon> _currentFilons;
        private bool _isAddPinMode = false;
        private GMapOverlay _tempMarkerOverlay;
        private ZoneDrawingService? _zoneDrawingService; // ? NOUVEAU

        // Champs pour la rotation de carte
        private bool _isRotating = false;
        private Point _rotationStartPoint;
        private float _startBearing = 0f;

        public Form1(FilonDataService dataService, IMineralRepository mineralRepository, PhotoService photoService)
        {
            _themeService = new ThemeService();

            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _mineralRepository = mineralRepository ?? throw new ArgumentNullException(nameof(mineralRepository));
            _photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));

            _perfOptimizer = new PerformanceOptimizer(); // ? INIT
            _autoSaveService = new AutoSaveService(_dataService, new FileLogger()); // ? INIT
            _measurementService = new MeasurementService(); // ? INIT

            InitializeComponent();

            // Replace Designer-created MineralsPanel with injected-services instance (if present)
            try
            {
                var existing = tabPageMinerals.Controls.OfType<Forms.MineralsPanel>().FirstOrDefault();
                if (existing != null)
                    tabPageMinerals.Controls.Remove(existing);

                var mineralsPanel = new Forms.MineralsPanel(_dataService, _mineralRepository, _photoService);
                mineralsPanel.Dock = DockStyle.Fill;
                tabPageMinerals.Controls.Add(mineralsPanel);
            }
            catch { /* ignore if tabPageMinerals not initialized yet */ }

            // Import panel uses injected _dataService
            var importPanel = new Forms.ImportPanel(_dataService);
            tabPageImport.Controls.Add(importPanel);

            _pdfService = new PdfExportService();
            _emailService = new EmailService(_pdfService);
            _markersOverlay = new GMapOverlay("filons");
            _tempMarkerOverlay = new GMapOverlay("temp");
            _currentFilons = new List<Filon>();
            _mapProviderService = new MapProviderService(gMapControl);

            this.Load += Form1_LoadAsync;
            this.FormClosing += Form1_FormClosing; // ? NOUVEAU
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // ? Nettoyage des ressources de performance
            _perfOptimizer?.Dispose();

            // ? Désactiver auto-save avant fermeture
            _autoSaveService?.Disable();
            _autoSaveService?.Dispose();
        }

        private async void Form1_LoadAsync(object? sender, EventArgs e)
        {
            // ? Activer GPU Hardware Acceleration dés le démarrage
            PerformanceOptimizer.EnableHardwareAcceleration();

            var originalText = this.Text;
            this.Text = "? Chargement...";

            await Task.Delay(50);

            await Task.Run(() =>
            {
                var startupLoader = new StartupDataLoader(_dataService);
                startupLoader.LoadInitialDataIfEmpty();
            });

            await Task.Run(() =>
            {
                this.Invoke(InitializeMap);
                this.Invoke(() => LoadFilons());
                this.Invoke(() => PopulateFilterComboBox());
            });

            // Initialiser le contrôle de recherche géographique
            _searchControl = new SearchLocationControl();
            _searchControl.LocationSelected += SearchControl_LocationSelected;
            this.Controls.Add(_searchControl);
            _searchControl.BringToFront();

            // ? Activer la sauvegarde automatique (toutes les 5 minutes)
            _autoSaveService?.Enable();

            await ApplyGlassEffects();

            this.Text = originalText;
        }

        private async Task ApplyGlassEffects()
        {
            await Task.Delay(100);
            if (Environment.OSVersion.Version.Major >= 10)
            {
                GlassEffects.EnableBlur(this, 220);
            }
            await GlassEffects.FadeIn(this, 400);
        }

        private void InitializeMap()
        {
            try
            {
                GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;

                var cacheFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "wmine", "MapCache");

                if (!Directory.Exists(cacheFolder))
                {
                    Directory.CreateDirectory(cacheFolder);
                }

                GMap.NET.GMaps.Instance.PrimaryCache = new GMap.NET.CacheProviders.SQLitePureImageCache
                {
                    CacheLocation = cacheFolder
                };

                gMapControl.MapProvider = GMapProviders.OpenStreetMap;
                gMapControl.Position = new PointLatLng(43.4, 6.3);
                gMapControl.MinZoom = 5;
                gMapControl.MaxZoom = 18;
                gMapControl.Zoom = 10;
                gMapControl.ShowCenter = false;
                gMapControl.DragButton = MouseButtons.Left;
                gMapControl.EmptyTileColor = Color.FromArgb(30, 30, 40);
                gMapControl.BackColor = Color.FromArgb(25, 25, 35);
                gMapControl.Overlays.Add(_markersOverlay);
                gMapControl.Overlays.Add(_tempMarkerOverlay);
                gMapControl.MouseClick += GMapControl_MouseClick;
                gMapControl.OnMarkerClick += GMapControl_OnMarkerClick;
                gMapControl.MouseMove += GMapControl_MouseMove;

                // Initialiser le service de clustering
                _clusterService = new MarkerClusterService(gMapControl);
                // ? Initialiser le service de couches géologiques - DÉSACTIVÉ`n
                // InitializeGeologicalLayers();

                _themeService.ApplyTheme(this, _themeService.CurrentTheme);
            }
            catch (Exception ex)
            {
                ShowModernMessageBox($"Erreur lors de l'initialisation de la carte: {ex.Message}",
                    "Erreur", MessageBoxIcon.Error);
            }
        }

        private void GMapControl_MouseClick(object? sender, MouseEventArgs e)
        {
            // Mode traçage de zone
            if (_zoneDrawingService != null && _zoneDrawingService.IsDrawing && e.Button == MouseButtons.Left)
            {
                var point = gMapControl.FromLocalToLatLng(e.X, e.Y);
                _zoneDrawingService.AddPoint(point);
                return;
            }

            // Mode mesure de distance
            if (_isMeasureMode && e.Button == MouseButtons.Left)
            {
                var point = gMapControl.FromLocalToLatLng(e.X, e.Y);

                if (_measurePoint1 == null)
                {
                    _measurePoint1 = point;
                    ShowModernMessageBox(
                        "Premier point sélectionné.\n\nCliquez sur le deuxième point.",
                        "Mesure",
                        MessageBoxIcon.Information);
                }
                else if (_measurePoint2 == null)
                {
                    _measurePoint2 = point;

                    // Calculer et afficher la distance
                    var measureService = new MeasurementService();
                    var summary = measureService.GetMeasurementSummary(_measurePoint1.Value, _measurePoint2.Value);

                    MessageBox.Show(summary, "Résultat de la Mesure", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Désactiver le mode mesure
                    _isMeasureMode = false;
                    _floatingToolsPanel?.SetButtonActive(0, false);
                    gMapControl.Cursor = Cursors.Default;
                    _measurePoint1 = null;
                    _measurePoint2 = null;
                }
                return;
            }

            if (_isAddPinMode && e.Button == MouseButtons.Left)
            {
                var point = gMapControl.FromLocalToLatLng(e.X, e.Y);
                CreateFilonAtPosition(point.Lat, point.Lng);
                ExitAddPinMode();
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                var point = gMapControl.FromLocalToLatLng(e.X, e.Y);
                var contextMenu = new ContextMenuStrip();
                contextMenu.BackColor = Color.FromArgb(35, 40, 50);
                contextMenu.ForeColor = Color.White;
                contextMenu.Font = new Font("Segoe UI Emoji", 10);
                contextMenu.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());

                var menuAddFilon = new ToolStripMenuItem("?? Nouveau filon ici");
                menuAddFilon.Font = new Font("Segoe UI Emoji", 10);
                menuAddFilon.Click += (s, args) => CreateFilonAtPosition(point.Lat, point.Lng);

                var menuCopyCoords = new ToolStripMenuItem($"?? Copier coordonnées");
                menuCopyCoords.Font = new Font("Segoe UI Emoji", 10);
                menuCopyCoords.Click += (s, args) =>
                {
                    var coords = $"Lat: {point.Lat:F6}°, Lon: {point.Lng:F6}°";
                    Clipboard.SetText(coords);
                    ShowModernMessageBox($"Coordonnées copiées:\n{coords}", "Info", MessageBoxIcon.Information);
                };

                contextMenu.Items.Add(menuAddFilon);
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add(menuCopyCoords);
                contextMenu.Show(gMapControl, e.Location);
            }
        }

        private void GMapControl_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            // Clic sur un cluster ? Zoomer sur la zone
            if (item is ClusterMarker clusterMarker && e.Button == MouseButtons.Left)
            {
                gMapControl.Position = clusterMarker.Position;
                gMapControl.Zoom = Math.Min(gMapControl.Zoom + 3, gMapControl.MaxZoom);
                return;
            }

            if (item.Tag is Filon filon)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (e.Clicks == 2)
                    {
                        EditFilon(filon);
                    }
                    else
                    {
                        var comboItem = cmbSelectFilon.Items.Cast<FilonComboItem>()
                            .FirstOrDefault(i => i.Filon?.Id == filon.Id);
                        if (comboItem != null)
                        {
                            cmbSelectFilon.SelectedItem = comboItem;
                        }
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    ShowMarkerContextMenu(filon, e.Location);
                }
            }
        }

        private void ShowMarkerContextMenu(Filon filon, Point location)
        {
            var contextMenu = new ContextMenuStrip();
            contextMenu.BackColor = Color.FromArgb(35, 40, 50);
            contextMenu.ForeColor = Color.White;
            contextMenu.Font = new Font("Segoe UI Emoji", 10);
            contextMenu.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());

            var menuVoirFiche = new ToolStripMenuItem($"?? Voir fiche de '{filon.Nom}'");
            menuVoirFiche.Font = new Font("Segoe UI Emoji", 10);
            menuVoirFiche.Click += (s, e) => OpenFilonFicheComplete(filon);

            var menuEdit = new ToolStripMenuItem($"?? Éditer '{filon.Nom}'");
            menuEdit.Font = new Font("Segoe UI Emoji", 10);
            menuEdit.Click += (s, e) => EditFilon(filon);

            var menuDelete = new ToolStripMenuItem($"??? Supprimer '{filon.Nom}'");
            menuDelete.Font = new Font("Segoe UI Emoji", 10);
            menuDelete.Click += (s, e) => DeleteFilon(filon);

            var menuExport = new ToolStripMenuItem($"?? Exporter en PDF");
            menuExport.Font = new Font("Segoe UI Emoji", 10);
            menuExport.Click += (s, e) => ExportFilonToPdf(filon);

            var menuCopyCoords = new ToolStripMenuItem($"?? Copier coordonnées");
            menuCopyCoords.Font = new Font("Segoe UI Emoji", 10);
            menuCopyCoords.Click += (s, e) =>
            {
                if (filon.Latitude.HasValue && filon.Longitude.HasValue)
                {
                    var coords = $"Lat: {filon.Latitude:F6}°, Lon: {filon.Longitude:F6}°";
                    Clipboard.SetText(coords);
                    ShowModernMessageBox($"Coordonnées copiées:\n{coords}", "Info", MessageBoxIcon.Information);
                }
            };

            contextMenu.Items.Add(menuVoirFiche);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(menuEdit);
            contextMenu.Items.Add(menuDelete);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(menuExport);
            contextMenu.Items.Add(menuCopyCoords);
            contextMenu.Show(gMapControl, location);
        }

        private void GMapControl_MouseMove(object? sender, MouseEventArgs e)
        {
            // ? THROTTLING : Limiter é 1 exécution toutes les 100ms
            if (!_perfOptimizer.Throttle("map_mousemove", 100))
                return;

            if (_isAddPinMode)
            {
                _tempMarkerOverlay.Markers.Clear();
                var point = gMapControl.FromLocalToLatLng(e.X, e.Y);
                var tempMarker = new GMarkerGoogle(point, GMarkerGoogleType.red_pushpin)
                {
                    ToolTipText = $"Cliquez pour placer le filon ici\nLat: {point.Lat:F6}é\nLon: {point.Lng:F6}é"
                };
                _tempMarkerOverlay.Markers.Add(tempMarker);
                gMapControl.Cursor = Cursors.Cross;
                gMapControl.Refresh();
                return;
            }

            var marker = _markersOverlay.Markers.FirstOrDefault(m =>
            {
                var markerLocal = gMapControl.FromLatLngToLocal(m.Position);
                var markerRect = new Rectangle(
                    (int)(markerLocal.X - 20),
                    (int)(markerLocal.Y - 50),
                    40, 50
                );
                return markerRect.Contains(e.Location);
            });

            gMapControl.Cursor = marker != null ? Cursors.Hand : Cursors.Default;
        }

        private void EnterAddPinMode()
        {
            _isAddPinMode = true;
            btnAddFilon.Text = "Annuler";
            btnAddFilon.BackColor = Color.FromArgb(244, 67, 54);
            gMapControl.Cursor = Cursors.Cross;
            gMapControl.DragButton = MouseButtons.None;
            ShowModernMessageBox("Mode placement de pin activé\n\nCliquez sur la carte pour placer un nouveau filon.",
                "Mode Placement", MessageBoxIcon.Information);
        }

        private void ExitAddPinMode()
        {
            _isAddPinMode = false;
            _tempMarkerOverlay.Markers.Clear();
            btnAddFilon.Text = "+ Nouveau";
            btnAddFilon.BackColor = Color.FromArgb(0, 150, 136);
            gMapControl.Cursor = Cursors.Default;
            gMapControl.DragButton = MouseButtons.Left;
            gMapControl.Refresh();
        }

        private async void CreateFilonAtPosition(double latitude, double longitude)
        {
            var newFilon = new Filon
            {
                Latitude = latitude,
                Longitude = longitude
            };

            var (x, y) = CoordinateConverter.WGS84ToLambert3(latitude, longitude);
            newFilon.LambertX = x;
            newFilon.LambertY = y;

            using var form = new FilonEditForm(newFilon, _dataService);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _dataService.AddFilon(form.Filon);
                LoadFilons(null);

                await Task.Delay(100);
                var comboItem = cmbSelectFilon.Items.Cast<FilonComboItem>()
                    .FirstOrDefault(i => i.Filon?.Id == form.Filon.Id);
                if (comboItem != null)
                {
                    cmbSelectFilon.SelectedItem = comboItem;
                }

                ShowModernMessageBox("Filon créé avec succés!", "Succés", MessageBoxIcon.Information);
            }
        }

        private async void CreateFilonWithoutCoordinates()
        {
            var newFilon = new Filon
            {
                Nom = "Nouveau filon",
                MatierePrincipale = MineralType.Fer
            };

            using var form = new FilonEditForm(newFilon, _dataService);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _dataService.AddFilon(form.Filon);
                LoadFilons(null);

                await Task.Delay(100);
                var comboItem = cmbSelectFilon.Items.Cast<FilonComboItem>()
                    .FirstOrDefault(i => i.Filon?.Id == form.Filon.Id);
                if (comboItem != null)
                {
                    cmbSelectFilon.SelectedItem = comboItem;
                }

                ShowModernMessageBox("Filon créé avec succés!", "Succés", MessageBoxIcon.Information);
            }
        }

        private void EditFilon(Filon filon)
        {
            using var form = new FilonEditForm(filon, _dataService);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _dataService.UpdateFilon(form.Filon);
                LoadFilons(null);
                ShowModernMessageBox("? Filon mis à jour!", "Succès", MessageBoxIcon.Information);
            }
        }

        private void DeleteFilon(Filon filon)
        {
            if (ShowModernConfirmation($"étes-vous sûr de vouloir supprimer le filon '{filon.Nom}' ?",
                "Confirmation"))
            {
                _dataService.DeleteFilon(filon.Id);
                LoadFilons(null);
                ShowModernMessageBox("Filon supprimé!", "Succés", MessageBoxIcon.Information);
            }
        }

        private void ExportFilonToPdf(Filon filon)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "PDF|*.pdf",
                FileName = $"Filon_{filon.Nom}.pdf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _pdfService.ExportFilonToPdf(filon, sfd.FileName);
                    ShowModernMessageBox("Export PDF réussi!", "Succés", MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    ShowModernMessageBox($"Erreur lors de l'export PDF: {ex.Message}",
                        "Erreur", MessageBoxIcon.Error);
                }
            }
        }

        private void FloatingToolsPanel_ExportKMZClicked(object? sender, EventArgs e)
        {
            var filons = _dataService.GetAllFilons();
            var filonsWithCoords = filons.Where(f => f.Latitude.HasValue && f.Longitude.HasValue).ToList();

            if (filonsWithCoords.Count == 0)
            {
                ShowModernMessageBox(
                    "Aucun filon avec coordonnées GPS à exporter.\n\n" +
                    "Ajoutez d'abord des filons avec coordonnées GPS.",
                    "Export KMZ",
                    MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Fichier KMZ (*.kmz)|*.kmz",
                FileName = $"WMine_Filons_{DateTime.Now:yyyyMMdd}.kmz",
                Title = "Exporter en KMZ (Google Earth)"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var kmzService = new KmzExportService();
                    var success = kmzService.ExportToKmz(filonsWithCoords, sfd.FileName);

                    if (success)
                    {
                        var summary = kmzService.GetExportSummary(filonsWithCoords, sfd.FileName);
                        MessageBox.Show(summary, "Export KMZ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Proposer d'ouvrir le fichier
                        if (MessageBox.Show(
                            "Voulez-vous ouvrir le fichier KMZ dans Google Earth ?",
                            "Ouvrir",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = sfd.FileName,
                                UseShellExecute = true
                            });
                        }
                    }
                    else
                    {
                        ShowModernMessageBox(
                            "Erreur lors de l'export KMZ.",
                            "Erreur",
                            MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    ShowModernMessageBox(
                        $"Erreur lors de l'export:\n\n{ex.Message}",
                        "Erreur",
                        MessageBoxIcon.Error);
                }
            }
        }

        private void LoadFilons(MineralType? filter = null)
        {
            _currentFilons = filter.HasValue
                ? _dataService.FilterByMainMineral(filter.Value)
                : _dataService.GetAllFilons();

            UpdateFilonComboBox();
            UpdateMapMarkers();
            btnViewFiches.Text = $"Fiches ({_currentFilons.Count})";
        }

        private void PopulateFilterComboBox()
        {
            // ? SUSPEND LAYOUT pendant ajout multiple
            using (_perfOptimizer.SuspendLayout(cmbFilterMineral))
            {
                cmbFilterMineral.Items.Clear();

                // Ajouter "Tous" en premier
                cmbFilterMineral.Items.Add(new
                {
                    Display = "Tous les minéraux",
                    Value = (MineralType?)null
                });

                // Ajouter tous les types de minéraux
                foreach (MineralType mineral in Enum.GetValues(typeof(MineralType)))
                {
                    cmbFilterMineral.Items.Add(new
                    {
                        Display = MineralColors.GetDisplayName(mineral),
                        Value = (MineralType?)mineral
                    });
                }

                // Sélectionner "Tous" par défaut
                cmbFilterMineral.SelectedIndex = 0;
            }
        }

        public void RefreshFilonsList()
        {
            LoadFilons();
        }

        private void UpdateFilonComboBox()
        {
            var selectedFilon = cmbSelectFilon.SelectedItem as FilonComboItem;

            // ? SUSPEND LAYOUT pendant ajout multiple
            using (_perfOptimizer.SuspendLayout(cmbSelectFilon))
            {
                cmbSelectFilon.Items.Clear();
                cmbSelectFilon.Items.Add(new FilonComboItem(null, "-- Sélectionner un filon --"));

                foreach (var filon in _currentFilons.OrderBy(f => f.Nom))
                {
                    var item = new FilonComboItem(filon,
                        $"{filon.Nom} ({MineralColors.GetDisplayName(filon.MatierePrincipale)})" +
                        (filon.HasCoordinates() ? " [GPS]" : ""));
                    cmbSelectFilon.Items.Add(item);
                }

                if (selectedFilon?.Filon != null)
                {
                    var itemToSelect = cmbSelectFilon.Items.Cast<FilonComboItem>()
                        .FirstOrDefault(i => i.Filon?.Id == selectedFilon.Filon.Id);
                    cmbSelectFilon.SelectedItem = itemToSelect ?? cmbSelectFilon.Items[0];
                }
                else
                {
                    cmbSelectFilon.SelectedIndex = 0;
                }
            }
        }

        private void UpdateMapMarkers()
        {
            if (_clusterService != null)
            {
                // Utiliser le service de clustering
                _clusterService.LoadFilons(_currentFilons);
            }
            else
            {
                // Fallback : affichage classique sans clustering
                _markersOverlay.Markers.Clear();

                foreach (var filon in _currentFilons)
                {
                    if (filon.Latitude.HasValue && filon.Longitude.HasValue)
                    {
                        var point = new PointLatLng(filon.Latitude.Value, filon.Longitude.Value);
                        var color = MineralColors.GetColor(filon.MatierePrincipale);

                        var marker = new FilonCrystalMarker(point, filon, color)
                        {
                            ToolTipText = $"{filon.Nom}\n{MineralColors.GetDisplayName(filon.MatierePrincipale)}",
                            Tag = filon
                        };

                        _markersOverlay.Markers.Add(marker);
                    }
                }
            }

            gMapControl.Refresh();
        }

        private void ChangeMapType(Models.MapType mapType)
        {
            try
            {
                _mapProviderService.SetMapType(mapType);
            }
            catch (Exception ex)
            {
                ShowModernMessageBox($"Erreur changement carte: {ex.Message}",
                    "Erreur", MessageBoxIcon.Error);
            }
        }

        private string GetStatusSummary(FilonStatus status)
        {
            if (status == FilonStatus.Aucun)
                return "Aucun";

            var statuses = new List<string>();
            foreach (FilonStatus value in Enum.GetValues<FilonStatus>())
            {
                if (value != FilonStatus.Aucun && status.HasFlag(value))
                {
                    statuses.Add(value.GetDisplayName());
                }
            }

            return string.Join(", ", statuses);
        }

        private async void BtnAddFilon_Click(object? sender, EventArgs e)
        {
            if (_isAddPinMode)
            {
                ExitAddPinMode();
                return;
            }

            var choiceForm = new Form
            {
                Text = "Création d'un nouveau filon",
                Size = new Size(500, 250),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(25, 25, 35),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI Emoji", 10),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblQuestion = new Label
            {
                Text = "Comment souhaitez-vous créer le nouveau filon ?",
                Location = new Point(30, 30),
                Width = 440,
                Height = 40,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 181, 246),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var btnPinMode = new Button
            {
                Text = "Placer un pin sur la carte",
                Location = new Point(30, 90),
                Width = 200,
                Height = 60,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPinMode.FlatAppearance.BorderSize = 0;
            btnPinMode.Click += (s, ev) =>
            {
                choiceForm.DialogResult = DialogResult.Yes;
                choiceForm.Close();
            };

            var btnDirectForm = new Button
            {
                Text = "Créer directement",
                Location = new Point(250, 90),
                Width = 200,
                Height = 60,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDirectForm.FlatAppearance.BorderSize = 0;
            btnDirectForm.Click += (s, ev) =>
            {
                choiceForm.DialogResult = DialogResult.No;
                choiceForm.Close();
            };

            var btnCancel = new Button
            {
                Text = "Annuler",
                Location = new Point(180, 170),
                Width = 120,
                Height = 35,
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, ev) =>
            {
                choiceForm.DialogResult = DialogResult.Cancel;
                choiceForm.Close();
            };

            choiceForm.Controls.Add(lblQuestion);
            choiceForm.Controls.Add(btnPinMode);
            choiceForm.Controls.Add(btnDirectForm);
            choiceForm.Controls.Add(btnCancel);

            var result = choiceForm.ShowDialog();

            if (result == DialogResult.Yes)
            {
                EnterAddPinMode();
            }
            else if (result == DialogResult.No)
            {
                CreateFilonWithoutCoordinates();
            }
        }

        private void BtnEditFilon_Click(object? sender, EventArgs e)
        {
            var selectedItem = cmbSelectFilon.SelectedItem as FilonComboItem;
            if (selectedItem?.Filon == null)
            {
                ShowModernMessageBox("Veuillez sélectionner un filon a éditer.",
                    "Information", MessageBoxIcon.Information);
                return;
            }

            EditFilon(selectedItem.Filon);
        }

        private void BtnDeleteFilon_Click(object? sender, EventArgs e)
        {
            var selectedItem = cmbSelectFilon.SelectedItem as FilonComboItem;
            if (selectedItem?.Filon == null)
            {
                ShowModernMessageBox("Veuillez sélectionner un filon a supprimer.",
                    "Information", MessageBoxIcon.Information);
                return;
            }

            DeleteFilon(selectedItem.Filon);
        }

        private void BtnExportPdf_Click(object? sender, EventArgs e)
        {
            var selectedItem = cmbSelectFilon.SelectedItem as FilonComboItem;
            if (selectedItem?.Filon == null)
            {
                ShowModernMessageBox("Veuillez sélectionner un filon a exporter.",
                    "Information", MessageBoxIcon.Information);
                return;
            }

            ExportFilonToPdf(selectedItem.Filon);
        }

        private void BtnShareEmail_Click(object? sender, EventArgs e)
        {
            var selectedItem = cmbSelectFilon.SelectedItem as FilonComboItem;
            if (selectedItem?.Filon == null)
            {
                ShowModernMessageBox("Veuillez sélectionner un filon a partager.",
                    "Information", MessageBoxIcon.Information);
                return;
            }

            _emailService.ShareFilonByEmail(selectedItem.Filon);
        }

        private void CmbFilterMineral_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // ? DEBOUNCING : Attendre 300ms aprés le dernier changement
            _perfOptimizer.Debounce("filter_mineral", () =>
            {
                if (cmbFilterMineral.SelectedItem == null) return;

                // Récupérer la valeur (peut étre null pour "Tous")
                var itemType = cmbFilterMineral.SelectedItem.GetType();
                var valueProp = itemType.GetProperty("Value");

                if (valueProp != null)
                {
                    var value = valueProp.GetValue(cmbFilterMineral.SelectedItem);

                    if (value is MineralType mineral)
                    {
                        // Filtrer par minéral spécifique
                        LoadFilons(mineral);
                    }
                    else
                    {
                        // Afficher tous les filons
                        LoadFilons(null);
                    }
                }
            }, 300);
        }

        private void CmbSelectFilon_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selectedItem = cmbSelectFilon.SelectedItem as FilonComboItem;
            if (selectedItem?.Filon != null)
            {
                var filon = selectedItem.Filon;
                if (filon.Latitude.HasValue && filon.Longitude.HasValue)
                {
                    gMapControl.Position = new PointLatLng(filon.Latitude.Value, filon.Longitude.Value);
                    gMapControl.Zoom = 14;

                    // ? Mettre à jour la météo du widget (conversion double -> decimal)
                    _ = _weatherWidget?.SetLocationAsync((decimal)filon.Latitude.Value, (decimal)filon.Longitude.Value);
                }
            }
        }

        private void ShowModernMessageBox(string message, string title, MessageBoxIcon icon)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

        private bool ShowModernConfirmation(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void BtnViewFiches_Click(object? sender, EventArgs e)
        {
            if (_currentFilons == null || _currentFilons.Count == 0)
            {
                ShowModernMessageBox("Aucun filon a afficher.",
                    "Information", MessageBoxIcon.Information);
                return;
            }

            // Suite du code dans le prochain message car trop long...
            // Pour l'instant créons un simple ListView
            ShowSimpleFilonsList();
        }

        private void ShowSimpleFilonsList()
        {
            var fichesForm = new Form
            {
                Text = $"?? Liste des Filons ({_currentFilons.Count})",
                Size = new Size(1150, 650),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(25, 25, 35),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10)
            };

            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(35, 40, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10)
            };

            // Menu contextuel pour la liste
            var contextMenu = new ContextMenuStrip();
            contextMenu.BackColor = Color.FromArgb(35, 40, 50);
            contextMenu.ForeColor = Color.White;
            contextMenu.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());

            var menuVoirFiche = new ToolStripMenuItem("Voir fiche");
            menuVoirFiche.Click += (s, e) =>
            {
                if (listView.SelectedItems.Count > 0 && listView.SelectedItems[0].Tag is Filon filon)
                {
                    OpenFilonFicheComplete(filon);
                }
            };

            var menuVoirCarte = new ToolStripMenuItem("Voir sur la carte");
            menuVoirCarte.Click += (s, e) =>
            {
                if (listView.SelectedItems.Count > 0 && listView.SelectedItems[0].Tag is Filon filon)
                {
                    fichesForm.Close();
                    if (filon.Latitude.HasValue && filon.Longitude.HasValue)
                    {
                        gMapControl.Position = new PointLatLng(filon.Latitude.Value, filon.Longitude.Value);
                        gMapControl.Zoom = 14;
                        mainTabControl.SelectedTab = tabPageMap;

                        // Sélectionner dans le combo
                        var comboItem = cmbSelectFilon.Items.Cast<FilonComboItem>()
                            .FirstOrDefault(i => i.Filon?.Id == filon.Id);
                        if (comboItem != null)
                        {
                            cmbSelectFilon.SelectedItem = comboItem;
                        }
                    }
                    else
                    {
                        ShowModernMessageBox("Ce filon n'a pas de coordonnées GPS.",
                            "Information", MessageBoxIcon.Information);
                    }
                }
            };

            contextMenu.Items.Add(menuVoirFiche);
            contextMenu.Items.Add(menuVoirCarte);
            listView.ContextMenuStrip = contextMenu;

            // Double-clic pour voir sur la carte
            listView.DoubleClick += (s, e) =>
            {
                if (listView.SelectedItems.Count > 0 && listView.SelectedItems[0].Tag is Filon filon)
                {
                    fichesForm.Close();
                    if (filon.Latitude.HasValue && filon.Longitude.HasValue)
                    {
                        gMapControl.Position = new PointLatLng(filon.Latitude.Value, filon.Longitude.Value);
                        gMapControl.Zoom = 14;
                        mainTabControl.SelectedTab = tabPageMap;

                        // Sélectionner dans le combo
                        var comboItem = cmbSelectFilon.Items.Cast<FilonComboItem>()
                            .FirstOrDefault(i => i.Filon?.Id == filon.Id);
                        if (comboItem != null)
                        {
                            cmbSelectFilon.SelectedItem = comboItem;
                        }
                    }
                }
            };

            listView.Columns.Add("Nom", 200);
            listView.Columns.Add("Minéral", 150);
            listView.Columns.Add("Latitude", 120);
            listView.Columns.Add("Longitude", 120);

            foreach (var filon in _currentFilons)
            {
                var item = new ListViewItem(filon.Nom ?? "Sans nom");
                item.SubItems.Add(MineralColors.GetDisplayName(filon.MatierePrincipale));
                item.SubItems.Add(filon.Latitude?.ToString("F6") ?? "N/A");
                item.SubItems.Add(filon.Longitude?.ToString("F6") ?? "N/A");
                item.Tag = filon;
                listView.Items.Add(item);
            }

            fichesForm.Controls.Add(listView);
            fichesForm.ShowDialog(this);
        }

        private void OpenFilonFicheComplete(Filon filon)
        {
            // Ouvrir le formulaire d'édition en mode lecture seule (consultation)
            using var form = new FilonEditForm(filon, _dataService);

            // Le dialogue s'ouvre avec toutes les informations du filon
            // L'utilisateur peut voir tous les détails comme dans "édition"
            form.ShowDialog(this);

            // Si l'utilisateur a modifié quelque chose (via le bouton Enregistrer)
            if (form.DialogResult == DialogResult.OK)
            {
                _dataService.UpdateFilon(form.Filon);
                LoadFilons(null);
                ShowModernMessageBox("Filon mis à jour!", "Succés", MessageBoxIcon.Information);
            }
        }

        private class FilonComboItem
        {
            public Filon? Filon { get; }
            public string DisplayText { get; }

            public FilonComboItem(Filon? filon, string displayText)
            {
                Filon = filon;
                DisplayText = displayText;
            }

            public override string ToString() => DisplayText;
        }

        private class DarkColorTable : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground => Color.FromArgb(35, 40, 50);
            public override Color ImageMarginGradientBegin => Color.FromArgb(35, 40, 50);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(35, 40, 50);
            public override Color ImageMarginGradientEnd => Color.FromArgb(35, 40, 50);
            public override Color MenuBorder => Color.FromArgb(60, 65, 75);
            public override Color MenuItemBorder => Color.FromArgb(60, 65, 75);
            public override Color MenuItemSelected => Color.FromArgb(50, 55, 65);
            public override Color MenuStripGradientBegin => Color.FromArgb(35, 40, 50);
            public override Color MenuStripGradientEnd => Color.FromArgb(35, 40, 50);
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(50, 55, 65);
            public override Color MenuItemSelectedGradientEnd => Color.FromArgb(50, 55, 65);
            public override Color MenuItemPressedGradientBegin => Color.FromArgb(40, 45, 55);
            public override Color MenuItemPressedGradientEnd => Color.FromArgb(40, 45, 55);
        }

        // Méthodes pour la rotation de carte
        private void GMapControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && (ModifierKeys & Keys.Control) == Keys.Control)
            {
                _isRotating = true;
                _rotationStartPoint = e.Location;
                _startBearing = gMapControl.Bearing;
                gMapControl.CanDragMap = false;
                Cursor = Cursors.SizeAll;
            }
        }

        private void GMapControl_MouseMove_Rotation(object sender, MouseEventArgs e)
        {
            if (_isRotating)
            {
                int deltaX = e.Location.X - _rotationStartPoint.X;
                float rotation = deltaX * 0.5f;
                gMapControl.Bearing = _startBearing + rotation;
            }
        }

        private void GMapControl_MouseUp_Rotation(object sender, MouseEventArgs e)
        {
            if (_isRotating)
            {
                _isRotating = false;
                gMapControl.CanDragMap = true;
                Cursor = Cursors.Default;
            }
        }

        private void SearchControl_LocationSelected(object? sender, GeocodingResult result)
        {
            // Centrer la carte sur le lieu sélectionné
            gMapControl.Position = result.ToPointLatLng();
            gMapControl.Zoom = 14;

            ShowModernMessageBox(
                $"Lieu trouvé :\n\n{result.DisplayName}\n\n" +
                $"Latitude : {result.Latitude:F6}é\n" +
                $"Longitude : {result.Longitude:F6}é",
                "Recherche géographique",
                MessageBoxIcon.Information);
        }

        private void BtnRoute_Click(object? sender, EventArgs e)
        {
            if (_currentFilons == null || _currentFilons.Count == 0)
            {
                ShowModernMessageBox(
                    "Aucun filon disponible pour calculer un itinéraire.\n\n" +
                    "Veuillez d'abord créer des filons avec des coordonnées GPS.",
                    "Aucun filon",
                    MessageBoxIcon.Information);
                return;
            }

            // Ouvrir le dialogue de routing
            using var routeDialog = new Forms.RouteDialog(gMapControl, _currentFilons);
            routeDialog.ShowDialog(this);
        }

        // ??? GESTIONNAIRES DU PANNEAU OUTILS ???

        private PointLatLng? _measurePoint1;
        private PointLatLng? _measurePoint2;
        private bool _isMeasureMode = false;

        private void FloatingToolsPanel_MeasureDistanceClicked(object? sender, EventArgs e)
        {
            _isMeasureMode = !_isMeasureMode;

            if (_isMeasureMode)
            {
                // Activer le mode mesure
                _measurePoint1 = null;
                _measurePoint2 = null;
                _floatingToolsPanel?.SetButtonActive(0, true);
                gMapControl.Cursor = Cursors.Cross;

                ShowModernMessageBox(
                    "Mode Mesure activé\n\n" +
                    "Cliquez sur 2 points de la carte pour mesurer la distance.",
                    "Mesure de Distance",
                    MessageBoxIcon.Information);
            }
            else
            {
                // Désactiver le mode mesure
                _floatingToolsPanel?.SetButtonActive(0, false);
                gMapControl.Cursor = Cursors.Default;
            }
        }

        private void FloatingToolsPanel_ImportPhotosGPSClicked(object? sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = "Sélectionnez le dossier contenant vos photos géolocalisées",
                ShowNewFolderButton = false
            };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                ImportPhotosWithGPS(fbd.SelectedPath);
            }
        }

        private async void ImportPhotosWithGPS(string folderPath)
        {
            try
            {
                var geotagService = new PhotoGeotagService();
                var result = await geotagService.ImportPhotosFromFolderAsync(
                    folderPath,
                    _dataService.GetAllFilons(),
                    maxDistanceKm: 0.5
                );

                var summary = geotagService.GetImportSummary(result);

                MessageBox.Show(
                    summary,
                    "Import Photos GPS",
                    MessageBoxButtons.OK,
                    result.MatchedPhotos.Count > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                // Rafraîchir si des photos ont été associées
                if (result.MatchedPhotos.Count > 0)
                {
                    LoadFilons();
                }
            }
            catch (Exception ex)
            {
                ShowModernMessageBox(
                    $"Erreur lors de l'import:\n\n{ex.Message}",
                    "Erreur",
                    MessageBoxIcon.Error);
            }
        }

        private void FloatingToolsPanel_FindNearbyFilonsClicked(object? sender, EventArgs e)
        {
            // Demander le rayon
            using var inputForm = new Form
            {
                Text = "Recherche par Proximité",
                Size = new Size(400, 200),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(25, 25, 35),
                ForeColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblPrompt = new Label
            {
                Text = "Rayon de recherche (en km) :",
                Location = new Point(20, 30),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                ForeColor = Color.White
            };

            var txtRadius = new TextBox
            {
                Location = new Point(20, 60),
                Width = 150,
                Text = "5",
                Font = new Font("Segoe UI Emoji", 12),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White
            };

            var btnOk = new Button
            {
                Text = "Rechercher",
                Location = new Point(190, 55),
                Width = 150,
                Height = 35,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, ev) => inputForm.DialogResult = DialogResult.OK;

            inputForm.Controls.AddRange(new Control[] { lblPrompt, txtRadius, btnOk });

            if (inputForm.ShowDialog() == DialogResult.OK && double.TryParse(txtRadius.Text, out double radius))
            {
                FindNearbyFilons(gMapControl.Position, radius);
            }
        }

        private void FindNearbyFilons(PointLatLng center, double radiusKm)
        {
            var measureService = new MeasurementService();
            var nearbyFilons = measureService.FindFilonsInRadius(
                center,
                radiusKm,
                _dataService.GetAllFilons()
            );

            if (nearbyFilons.Count == 0)
            {
                ShowModernMessageBox(
                    $"Aucun filon trouvé dans un rayon de {radiusKm} km.",
                    "Recherche Proximité",
                    MessageBoxIcon.Information);
                return;
            }

            // Afficher les résultats
            var resultText = $"?? {nearbyFilons.Count} filon(s) trouvé(s) dans {radiusKm} km:\n\n";
            foreach (var (filon, distance) in nearbyFilons.Take(10))
            {
                resultText += $"?? {filon.Nom} - {measureService.GetReadableDistance(distance)}\n";
            }

            if (nearbyFilons.Count > 10)
                resultText += $"\n... et {nearbyFilons.Count - 10} autres";

            MessageBox.Show(resultText, "Filons Proches", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FloatingToolsPanel_DrawZoneClicked(object? sender, EventArgs e)
        {
            if (_zoneDrawingService == null)
            {
                _zoneDrawingService = new ZoneDrawingService(gMapControl);
                _zoneDrawingService.ZoneCompleted += ZoneDrawingService_ZoneCompleted;
            }

            if (_zoneDrawingService.IsDrawing)
            {
                // Demander le nom de la zone et terminer
                using var inputForm = new Form
                {
                    Text = "Nom de la Zone",
                    Size = new Size(400, 150),
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.FromArgb(25, 25, 35),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var lblPrompt = new Label
                {
                    Text = "Nom de la zone :",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                    ForeColor = Color.White
                };

                var txtName = new TextBox
                {
                    Location = new Point(20, 50),
                    Width = 250,
                    Text = $"Zone_{DateTime.Now:yyyyMMdd_HHmm}",
                    Font = new Font("Segoe UI Emoji", 11),
                    BackColor = Color.FromArgb(45, 50, 60),
                    ForeColor = Color.White
                };

                var btnOk = new Button
                {
                    Text = "Terminer",
                    Location = new Point(290, 48),
                    Width = 80,
                    Height = 30,
                    BackColor = Color.FromArgb(0, 150, 136),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnOk.FlatAppearance.BorderSize = 0;
                btnOk.Click += (s, ev) => inputForm.DialogResult = DialogResult.OK;

                inputForm.Controls.AddRange(new Control[] { lblPrompt, txtName, btnOk });

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    _zoneDrawingService.CompleteZone(txtName.Text);
                    _floatingToolsPanel?.SetButtonActive(3, false);
                }
            }
            else
            {
                // Démarrer le dessin
                _zoneDrawingService.StartDrawing();
                _floatingToolsPanel?.SetButtonActive(3, true);
            }
        }

        private void ZoneDrawingService_ZoneCompleted(object? sender, Zone zone)
        {
            var filonsInZone = _zoneDrawingService?.FindFilonsInZone(zone, _dataService.GetAllFilons());

            if (filonsInZone == null || filonsInZone.Count == 0)
            {
                MessageBox.Show(
                    $"Zone '{zone.Name}' créée !\n\n" +
                    $"Surface: {_measurementService.GetReadableArea(zone.Area)}\n" +
                    $"Périmètre: {_measurementService.GetReadableDistance(zone.Perimeter)}\n\n" +
                    "Aucun filon dans cette zone.",
                    "Zone Terminée",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                var result = $"Zone '{zone.Name}' créée !\n\n" +
                           $"Surface: {_measurementService.GetReadableArea(zone.Area)}\n" +
                           $"Périmètre: {_measurementService.GetReadableDistance(zone.Perimeter)}\n\n" +
                           $"?? {filonsInZone.Count} filon(s) dans la zone:\n\n";

                foreach (var filon in filonsInZone.Take(10))
                {
                    result += $"• {filon.Nom}\n";
                }

                if (filonsInZone.Count > 10)
                    result += $"\n... et {filonsInZone.Count - 10} autres";

                MessageBox.Show(result, "Zone Terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ??? INITIALISATION DES COUCHES GÉOLOGIQUES ???
    }
}
