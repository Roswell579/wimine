using System.Reflection;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using wmine.Core.Services;
using wmine.Forms;
using wmine.Models;
using wmine.Services;
using wmine.UI;
using wmine.Utils;

namespace wmine
{
    public partial class Form1 : Form
    {
        private readonly FilonDataService _dataService;
        private readonly IMineralRepository _mineralRepository;
        private readonly PhotoService _photoService;
        private readonly PdfExportService _pdfService;
        private readonly EmailService _emailService;
        private readonly MapProviderService _mapProviderService;
        private readonly ThemeService _themeService;
        private readonly PerformanceOptimizer _perfOptimizer;
        private readonly AutoSaveService? _autoSaveService;
        private readonly MeasurementService _measurementService;
        private MarkerClusterService? _clusterService;
        private SearchLocationControl? _searchControl;
        private GMapOverlay _markersOverlay;
        private List<Filon> _currentFilons;
        private bool _isAddPinMode = false;
        private GMapOverlay _tempMarkerOverlay;

        // Champs pour la rotation de carte
        private bool _isRotating = false;
        private Point _rotationStartPoint;
        private float _startBearing = 0f;

        // Champs pour la mesure de distance
        private PointLatLng? _measurePoint1;
        private PointLatLng? _measurePoint2;
        private bool _isMeasureMode = false;

        // Event pour refresh
        public event EventHandler? FilonsRefreshRequested;

        public Form1(FilonDataService dataService, IMineralRepository mineralRepository, PhotoService photoService)
        {
            _themeService = new ThemeService();

            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _mineralRepository = mineralRepository ?? throw new ArgumentNullException(nameof(mineralRepository));
            _photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));

            _perfOptimizer = new PerformanceOptimizer();
            _autoSaveService = new AutoSaveService(_dataService, new FileLogger());
            _measurementService = new MeasurementService();

            InitializeComponent();

            try
            {
                var existing = tabPageMinerals.Controls.OfType<wmine.Forms.MineralsPanel>().FirstOrDefault();
                if (existing != null)
                    tabPageMinerals.Controls.Remove(existing);

                var mineralsPanel = new wmine.Forms.MineralsPanel(_dataService, _mineralRepository, _photoService);
                mineralsPanel.Dock = DockStyle.Fill;
                tabPageMinerals.Controls.Add(mineralsPanel);
            }
            catch { }

            var importPanel = new wmine.Forms.ImportPanel(_dataService);
            tabPageImport.Controls.Add(importPanel);

            _pdfService = new PdfExportService();
            _emailService = new EmailService(_pdfService);
            _markersOverlay = new GMapOverlay("filons");
            _tempMarkerOverlay = new GMapOverlay("temp");
            _currentFilons = new List<Filon>();
            _mapProviderService = new MapProviderService(gMapControl);

            this.Load += Form1_LoadAsync;
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _perfOptimizer?.Dispose();
            _autoSaveService?.Disable();
            _autoSaveService?.Dispose();
        }

        private async void Form1_LoadAsync(object? sender, EventArgs e)
        {
            PerformanceOptimizer.EnableHardwareAcceleration();

            var originalText = this.Text;
            this.Text = "🔄 Chargement...";

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

            _searchControl = new SearchLocationControl();
            _searchControl.LocationSelected += SearchControl_LocationSelected;
            this.Controls.Add(_searchControl);
            _searchControl.BringToFront();

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

                _clusterService = new MarkerClusterService(gMapControl);

                _themeService.ApplyTheme(this, _themeService.CurrentTheme);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'initialisation de la carte: {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GMapControl_MouseClick(object? sender, MouseEventArgs e)
        {
            // Mode mesure de distance
            if (_isMeasureMode && e.Button == MouseButtons.Left)
            {
                var point = gMapControl.FromLocalToLatLng(e.X, e.Y);

                if (_measurePoint1 == null)
                {
                    _measurePoint1 = point;
                    MessageBox.Show("Premier point sélectionné.\n\nCliquez sur le deuxième point.", "Mesure", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (_measurePoint2 == null)
                {
                    _measurePoint2 = point;
                    var measureService = new MeasurementService();
                    var summary = measureService.GetMeasurementSummary(_measurePoint1.Value, _measurePoint2.Value);
                    MessageBox.Show(summary, "Résultat de la Mesure", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _isMeasureMode = false;
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
                var contextMenu = new ContextMenuStrip
                {
                    BackColor = Color.FromArgb(35, 40, 50),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Emoji", 10),
                    Renderer = new ToolStripProfessionalRenderer(new DarkColorTable())
                };

                var menuAddFilon = new ToolStripMenuItem("📍 Nouveau filon ici");
                menuAddFilon.Click += (s, args) => CreateFilonAtPosition(point.Lat, point.Lng);

                var menuCopyCoords = new ToolStripMenuItem($"📋 Copier coordonnées");
                menuCopyCoords.Click += (s, args) =>
                {
                    var coords = $"Lat: {point.Lat:F6}°, Lon: {point.Lng:F6}°";
                    Clipboard.SetText(coords);
                    MessageBox.Show($"Coordonnées copiées:\n{coords}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                contextMenu.Items.Add(menuAddFilon);
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add(menuCopyCoords);
                contextMenu.Show(gMapControl, e.Location);
            }
        }

        private void GMapControl_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
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
            var contextMenu = new ContextMenuStrip
            {
                BackColor = Color.FromArgb(35, 40, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10),
                Renderer = new ToolStripProfessionalRenderer(new DarkColorTable())
            };

            var menuVoirFiche = new ToolStripMenuItem($"👁️ Voir fiche '{filon.Nom}'");
            menuVoirFiche.Click += (s, e) => OpenFilonFicheComplete(filon);

            var menuEdit = new ToolStripMenuItem($"✏️ Éditer '{filon.Nom}'");
            menuEdit.Click += (s, e) => EditFilon(filon);

            var menuDelete = new ToolStripMenuItem($"🗑️ Supprimer '{filon.Nom}'");
            menuDelete.Click += (s, e) => DeleteFilon(filon);

            var menuExport = new ToolStripMenuItem($"📄 Exporter en PDF");
            menuExport.Click += (s, e) => ExportFilonToPdf(filon);

            var menuEmail = new ToolStripMenuItem($"📧 Partager par email");
            menuEmail.Click += (s, e) =>
            {
                try
                {
                    var subject = $"Filon: {filon.Nom}";
                    var body = $"Informations sur le filon {filon.Nom}:\n\n" +
                              $"Minéral: {MineralColors.GetDisplayName(filon.MatierePrincipale)}\n" +
                              $"Statut: {filon.Statut}\n";

                    if (filon.TryGetWgs84(out double mailLat, out double mailLon))
                    {
                        body += $"Position: {mailLat:F6}°, {mailLon:F6}°\n";
                    }

                    var mailto = $"mailto:?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(mailto) { UseShellExecute = true });
                    MessageBox.Show("Email préparé avec succès!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var menuCopyCoords = new ToolStripMenuItem($"📋 Copier coordonnées");
            menuCopyCoords.Click += (s, e) =>
            {
                if (filon.TryGetWgs84(out double copyLat, out double copyLon))
                {
                    var coords = $"Lat: {copyLat:F6}°, Lon: {copyLon:F6}°";
                    Clipboard.SetText(coords);
                    MessageBox.Show($"Coordonnées copiées:\n{coords}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            contextMenu.Items.Add(menuVoirFiche);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(menuEdit);
            contextMenu.Items.Add(menuDelete);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(menuExport);
            contextMenu.Items.Add(menuEmail);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(menuCopyCoords);
            contextMenu.Show(gMapControl, location);
        }

        private void GMapControl_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_perfOptimizer.Throttle("map_mousemove", 100))
                return;

            if (_isAddPinMode)
            {
                _tempMarkerOverlay.Markers.Clear();
                var point = gMapControl.FromLocalToLatLng(e.X, e.Y);
                var tempMarker = new GMarkerGoogle(point, GMarkerGoogleType.red_pushpin)
                {
                    ToolTipText = $"Cliquez pour placer le filon ici\nLat: {point.Lat:F6}°\nLon: {point.Lng:F6}°"
                };
                _tempMarkerOverlay.Markers.Add(tempMarker);
                gMapControl.Cursor = Cursors.Cross;
                gMapControl.Refresh();
                return;
            }

            var marker = _markersOverlay.Markers.FirstOrDefault(m =>
            {
                var markerLocal = gMapControl.FromLatLngToLocal(m.Position);
                var markerRect = new Rectangle((int)(markerLocal.X - 20), (int)(markerLocal.Y - 50), 40, 50);
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
            MessageBox.Show("Mode placement de pin activé\n\nCliquez sur la carte pour placer un nouveau filon.", "Mode Placement", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            var newFilon = new Filon { Latitude = latitude, Longitude = longitude };
            var (x, y) = CoordinateConverter.WGS84ToLambert3(latitude, longitude);
            newFilon.LambertX = x;
            newFilon.LambertY = y;

            using var form = new FilonEditForm(newFilon, _dataService);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _dataService.AddFilon(form.Filon);
                LoadFilons(null);
                await Task.Delay(100);
                var comboItem = cmbSelectFilon.Items.Cast<FilonComboItem>().FirstOrDefault(i => i.Filon?.Id == form.Filon.Id);
                if (comboItem != null) cmbSelectFilon.SelectedItem = comboItem;
                MessageBox.Show("Filon créé avec succès!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void CreateFilonWithoutCoordinates()
        {
            var newFilon = new Filon { Nom = "Nouveau filon", MatierePrincipale = MineralType.Fer };

            using var form = new FilonEditForm(newFilon, _dataService);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _dataService.AddFilon(form.Filon);
                LoadFilons(null);
                await Task.Delay(100);
                var comboItem = cmbSelectFilon.Items.Cast<FilonComboItem>().FirstOrDefault(i => i.Filon?.Id == form.Filon.Id);
                if (comboItem != null) cmbSelectFilon.SelectedItem = comboItem;
                MessageBox.Show("Filon créé avec succès!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void EditFilon(Filon filon)
        {
            using var form = new FilonEditForm(filon, _dataService);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _dataService.UpdateFilon(form.Filon);
                LoadFilons(null);
                MessageBox.Show("Filon mis à jour!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeleteFilon(Filon filon)
        {
            if (MessageBox.Show($"Etes-vous sûr de vouloir supprimer le filon '{filon.Nom}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _dataService.DeleteFilon(filon.Id);
                LoadFilons(null);
                MessageBox.Show("Filon supprimé!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ExportFilonToPdf(Filon filon)
        {
            using var sfd = new SaveFileDialog { Filter = "PDF|*.pdf", FileName = $"Filon_{filon.Nom}.pdf" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _pdfService.ExportFilonToPdf(filon, sfd.FileName);
                    MessageBox.Show("Export PDF réussi!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'export PDF: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // REMPLACER LoadFilons() pour utiliser le clustering

        private void LoadFilons(MineralType? filter = null)
        {
            _currentFilons = filter.HasValue
                ? _dataService.FilterByMainMineral(filter.Value)
                : _dataService.GetAllFilons();

            UpdateFilonComboBox();

            // Utiliser le clustering au lieu de UpdateMapMarkers()
            if (_clusterService != null)
            {
                _clusterService.LoadFilons(_currentFilons);
            }

            btnViewFiches.Text = $"Fiches ({_currentFilons.Count})";
        }

        private void PopulateFilterComboBox()
        {
            using (_perfOptimizer.SuspendLayout(cmbFilterMineral))
            {
                cmbFilterMineral.Items.Clear();
                cmbFilterMineral.Items.Add(new { Display = "Tous les minéraux", Value = (MineralType?)null });
                foreach (MineralType mineral in Enum.GetValues(typeof(MineralType)))
                {
                    cmbFilterMineral.Items.Add(new { Display = MineralColors.GetDisplayName(mineral), Value = (MineralType?)mineral });
                }
                cmbFilterMineral.SelectedIndex = 0;
            }
        }

        public void RefreshFilonsList()
        {
            if (IsDisposed) return;
            try
            {
                FilonsRefreshRequested?.Invoke(this, EventArgs.Empty);
                foreach (var ctrl in GetAllControls(this))
                {
                    if (ctrl is wmine.Forms.MineralsPanel)
                    {
                        var mi = ctrl.GetType().GetMethod("LoadMinerals", BindingFlags.Instance | BindingFlags.NonPublic);
                        mi?.Invoke(ctrl, null);
                    }
                    var reload = ctrl.GetType().GetMethod("Reload", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
                                ctrl.GetType().GetMethod("RefreshList", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
                                ctrl.GetType().GetMethod("LoadData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    reload?.Invoke(ctrl, null);
                }
                Invalidate(true);
                Update();
            }
            catch { }
        }

        private IEnumerable<Control> GetAllControls(Control root)
        {
            foreach (Control c in root.Controls)
            {
                yield return c;
                foreach (var child in GetAllControls(c)) yield return child;
            }
        }

        private async void BtnIa_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Images|*.jpg;*.jpeg;*.png" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var bytes = File.ReadAllBytes(ofd.FileName);
                using var iaForm = new wmine.Forms.MineralAiForm();
                iaForm.Show();

                var mime = Path.GetExtension(ofd.FileName).ToLowerInvariant() == ".png" ? "image/png" : "image/jpeg";
                var preds = await iaForm.ClassifyAsync(bytes, mime);
                iaForm.Close();

                var msg = string.Join(Environment.NewLine, preds.Select(p => $"{p.Label} - {p.Prob * 100f:F1}%"));
                MessageBox.Show($"Résultats IA:\n\n{msg}", "Classification IA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur IA: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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
                gMapControl.Bearing = _startBearing + (deltaX * 0.5f);
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
            gMapControl.Position = result.ToPointLatLng();
            gMapControl.Zoom = 14;
            MessageBox.Show($"Lieu trouvé :\n\n{result.DisplayName}\n\nLatitude : {result.Latitude:F6}°\nLongitude : {result.Longitude:F6}°", "Recherche géographique", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnRoute_Click(object? sender, EventArgs e)
        {
            if (_currentFilons == null || _currentFilons.Count == 0)
            {
                MessageBox.Show("Aucun filon disponible pour calculer un itinéraire.\n\nVeuillez d'abord créer des filons avec des coordonnées GPS.", "Aucun filon", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var routeDialog = new wmine.Forms.RouteDialog(gMapControl, _currentFilons);
            routeDialog.ShowDialog(this);
        }

        // ========== MÉTHODES MANQUANTES ==========

        private void ShowModernMessageBox(string message, string title, MessageBoxIcon icon)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

        private bool ShowModernConfirmation(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void UpdateMapMarkers()
        {
            _markersOverlay.Markers.Clear();

            foreach (var filon in _currentFilons)
            {
                if (!filon.TryGetWgs84(out double lat, out double lon))
                    continue;

                var position = new PointLatLng(lat, lon);
                var color = MineralColors.GetColor(filon.MatierePrincipale);

                // Créer le marker cristal hexagonal
                var marker = new HexagonalCrystalMarker(position, color, filon.Nom)
                {
                    ToolTipText = $"{filon.Nom}\n{MineralColors.GetDisplayName(filon.MatierePrincipale)}",
                    ToolTipMode = MarkerTooltipMode.OnMouseOver,
                    Tag = filon
                };

                _markersOverlay.Markers.Add(marker);
            }

            // S'assurer que l'overlay est visible et rafraîchir
            _markersOverlay.IsVisibile = true;
            gMapControl.Refresh();
        }

        private void UpdateFilonComboBox()
        {
            using (_perfOptimizer.SuspendLayout(cmbSelectFilon))
            {
                cmbSelectFilon.Items.Clear();
                cmbSelectFilon.Items.Add(new FilonComboItem { Display = "-- Sélectionner un filon --", Filon = null });
                foreach (var filon in _currentFilons.OrderBy(f => f.Nom))
                {
                    cmbSelectFilon.Items.Add(new FilonComboItem { Display = filon.Nom, Filon = filon });
                }
                cmbSelectFilon.SelectedIndex = 0;
            }
        }

        private void OpenFilonFicheComplete(Filon filon)
        {
            var message = $"Filon: {filon.Nom}\nMinéral: {MineralColors.GetDisplayName(filon.MatierePrincipale)}\nStatut: {filon.Statut}\n";
            if (filon.TryGetWgs84(out double fLat, out double fLon)) message += $"Position: {fLat:F6}°, {fLon:F6}°\n";
            if (!string.IsNullOrEmpty(filon.Notes)) message += $"\nNotes: {filon.Notes}";
            MessageBox.Show(message, $"Fiche de {filon.Nom}", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ChangeMapType(wmine.Models.MapType mapType)
        {
            gMapControl.MapProvider = mapType switch
            {
                wmine.Models.MapType.GoogleSatellite => GMapProviders.GoogleSatelliteMap,
                wmine.Models.MapType.GoogleHybrid => GMapProviders.GoogleHybridMap,
                wmine.Models.MapType.BingSatellite => GMapProviders.BingSatelliteMap,
                _ => GMapProviders.OpenStreetMap
            };
            gMapControl.ReloadMap();
        }

        // ========== EVENT HANDLERS DU DESIGNER ==========

        private void BtnAddFilon_Click(object? sender, EventArgs e)
        {
            if (_isAddPinMode) { ExitAddPinMode(); return; }

            using var choiceForm = new Form
            {
                Text = "Nouveau Filon",
                Size = new Size(400, 200),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(25, 25, 35),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var btnPlacePin = new Button
            {
                Text = "📍 Placer un pin sur la carte",
                Location = new Point(50, 30),
                Width = 300,
                Height = 50,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPlacePin.FlatAppearance.BorderSize = 0;
            btnPlacePin.Click += (s, ev) => { choiceForm.DialogResult = DialogResult.Yes; choiceForm.Close(); };

            var btnCreateDirect = new Button
            {
                Text = "➕ Créer directement",
                Location = new Point(50, 90),
                Width = 300,
                Height = 50,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCreateDirect.FlatAppearance.BorderSize = 0;
            btnCreateDirect.Click += (s, ev) => { choiceForm.DialogResult = DialogResult.No; choiceForm.Close(); };

            choiceForm.Controls.AddRange(new Control[] { btnPlacePin, btnCreateDirect });
            var result = choiceForm.ShowDialog(this);
            if (result == DialogResult.Yes) EnterAddPinMode();
            else if (result == DialogResult.No) CreateFilonWithoutCoordinates();
        }

        private void BtnEditFilon_Click(object? sender, EventArgs e)
        {
            if (cmbSelectFilon.SelectedItem is FilonComboItem item && item.Filon != null) EditFilon(item.Filon);
            else MessageBox.Show("Veuillez sélectionner un filon à éditer.", "Aucun filon sélectionné", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BtnDeleteFilon_Click(object? sender, EventArgs e)
        {
            if (cmbSelectFilon.SelectedItem is FilonComboItem item && item.Filon != null) DeleteFilon(item.Filon);
            else MessageBox.Show("Veuillez sélectionner un filon à supprimer.", "Aucun filon sélectionné", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BtnExportPdf_Click(object? sender, EventArgs e)
        {
            if (cmbSelectFilon.SelectedItem is FilonComboItem item && item.Filon != null) ExportFilonToPdf(item.Filon);
            else MessageBox.Show("Veuillez sélectionner un filon à exporter.", "Aucun filon sélectionné", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BtnShareEmail_Click(object? sender, EventArgs e)
        {
            if (cmbSelectFilon.SelectedItem is FilonComboItem item && item.Filon != null)
            {
                try
                {
                    var subject = $"Filon: {item.Filon.Nom}";
                    var body = $"Informations sur le filon {item.Filon.Nom}:\n\nMinéral: {MineralColors.GetDisplayName(item.Filon.MatierePrincipale)}\nStatut: {item.Filon.Statut}\n";
                    if (item.Filon.Latitude.HasValue && item.Filon.Longitude.HasValue) body += $"Position: {item.Filon.Latitude:F6}°, {item.Filon.Longitude:F6}°\n";
                    var mailto = $"mailto:?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(mailto) { UseShellExecute = true });
                    MessageBox.Show("Email préparé avec succès!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) { MessageBox.Show($"Erreur lors du partage: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            else MessageBox.Show("Veuillez sélectionner un filon à partager.", "Aucun filon sélectionné", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BtnViewFiches_Click(object? sender, EventArgs e)
        {
            // Afficher une liste simple des filons
            var listForm = new Form { Text = "Liste des filons", Size = new Size(600, 400), StartPosition = FormStartPosition.CenterParent, BackColor = Color.FromArgb(25, 25, 35) };
            var listBox = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(35, 40, 50), ForeColor = Color.White, Font = new Font("Segoe UI", 11) };
            foreach (var filon in _currentFilons) listBox.Items.Add($"{filon.Nom} - {MineralColors.GetDisplayName(filon.MatierePrincipale)}");
            listBox.DoubleClick += (s, ev) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    var filon = _currentFilons[listBox.SelectedIndex];
                    var item = cmbSelectFilon.Items.Cast<FilonComboItem>().FirstOrDefault(i => i.Filon?.Id == filon.Id);
                    if (item != null) cmbSelectFilon.SelectedItem = item;
                    listForm.Close();
                }
            };
            listForm.Controls.Add(listBox);
            listForm.ShowDialog(this);
        }

        private void CmbFilterMineral_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbFilterMineral.SelectedItem == null) return;
            var selected = cmbFilterMineral.SelectedItem;
            var valueProperty = selected.GetType().GetProperty("Value");
            if (valueProperty != null)
            {
                var filterValue = (MineralType?)valueProperty.GetValue(selected);
                LoadFilons(filterValue);
            }
        }

        private void CmbSelectFilon_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbSelectFilon.SelectedItem is FilonComboItem item && item.Filon != null)
            {
                var filon = item.Filon;
                if (filon.TryGetWgs84(out double lat, out double lon))
                {
                    gMapControl.Position = new PointLatLng(lat, lon);
                    gMapControl.Zoom = 14;
                }
            }
        }
    }

    public class FilonComboItem
    {
        public string Display { get; set; } = string.Empty;
        public Filon? Filon { get; set; }
        public override string ToString() => Display;
    }

    public class DarkColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Color.FromArgb(60, 60, 70);
        public override Color MenuItemBorder => Color.FromArgb(80, 80, 90);
        public override Color MenuBorder => Color.FromArgb(50, 50, 60);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(60, 60, 70);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(60, 60, 70);
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(40, 40, 50);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(40, 40, 50);
    }
}
