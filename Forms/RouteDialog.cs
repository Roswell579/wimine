using System.Globalization;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using wmine.Models;
using wmine.Services;

namespace wmine.Forms
{
    /// <summary>
    /// Dialog pour calculer un itinéraire vers un filon
    /// </summary>
    public partial class RouteDialog : Form
    {
        private readonly RouteService _routeService;
        private readonly GMapControl _mapControl;
        private readonly List<Filon> _filons;
        private readonly PerformanceOptimizer _perfOptimizer; //  NOUVEAU

        private TextBox txtStartAddress;
        private TextBox txtStartLat;
        private TextBox txtStartLng;
        private ComboBox cmbDestination;
        private ComboBox cmbTransportType;
        private Button btnCalculate;
        private Button btnPickStartOnMap;
        private Button btnExportPdf;
        private Button btnExportGpx; //  NOUVEAU
        private RichTextBox txtResults;
        private Button btnShowOnMap;
        private Label lblStatus;
        private RouteInfo? _currentRoute;
        private CheckBox chkUseCurrentLocation;
        private bool _isPickingStartPoint = false;

        public RouteDialog(GMapControl mapControl, List<Filon> filons)
        {
            _routeService = new RouteService();
            _mapControl = mapControl;
            _filons = filons.Where(f => f.Latitude.HasValue && f.Longitude.HasValue).ToList();
            _perfOptimizer = new PerformanceOptimizer(); // ? INIT

            InitializeComponent();
            LoadFilons();
            LoadDefaultStart();

            // Nettoyer le pin temporaire é la fermeture
            this.FormClosing += RouteDialog_FormClosing;
        }

        private void RouteDialog_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // ? Annuler les opérations en cours
            _perfOptimizer?.Dispose();

            // Supprimer le pin temporaire de départ si présent
            var tempOverlay = _mapControl.Overlays.FirstOrDefault(o => o.Id == "temp_start");
            if (tempOverlay != null)
            {
                _mapControl.Overlays.Remove(tempOverlay);
                _mapControl.Refresh();
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Calculer un itinéraire vers un filon";
            this.Size = new Size(750, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 35);
            this.ForeColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 7
            };

            // Section déPART
            var startGroupBox = new GroupBox
            {
                Text = " Point de départ",
                Height = 180,
                Dock = DockStyle.Top,
                ForeColor = Color.LightBlue,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold)
            };

            var startPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            chkUseCurrentLocation = new CheckBox
            {
                Text = "Utiliser la position actuelle de la carte",
                Checked = true,
                ForeColor = Color.Black,
                Location = new Point(10, 10),
                AutoSize = true
            };
            chkUseCurrentLocation.CheckedChanged += ChkUseCurrentLocation_CheckedChanged;

            var lblAddress = new Label
            {
                Text = "Adresse:",
                Location = new Point(10, 40),
                AutoSize = true,
                ForeColor = Color.Black
            };

            txtStartAddress = new TextBox
            {
                Location = new Point(90, 37),
                Width = 490,
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = false,
                Text = "Position actuelle de la carte"
            };

            var lblLat = new Label
            {
                Text = "Latitude:",
                Location = new Point(10, 70),
                AutoSize = true,
                ForeColor = Color.Black
            };

            txtStartLat = new TextBox
            {
                Location = new Point(90, 67),
                Width = 150,
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = false
            };

            var lblLng = new Label
            {
                Text = "Longitude:",
                Location = new Point(260, 70),
                AutoSize = true,
                ForeColor = Color.Black
            };

            txtStartLng = new TextBox
            {
                Location = new Point(350, 67),
                Width = 150,
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = false
            };

            btnPickStartOnMap = new Button
            {
                Text = "Cliquer sur la carte",
                Location = new Point(10, 100),
                Width = 180,
                Height = 35,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnPickStartOnMap.FlatAppearance.BorderSize = 0;
            btnPickStartOnMap.Click += BtnPickStartOnMap_Click;

            var lblInfo = new Label
            {
                Text = "décochez la case ci-dessus pour saisir manuellement ou cliquer sur la carte",
                Location = new Point(10, 145),
                Width = 500,
                Height = 20,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI Emoji", 8)
            };

            startPanel.Controls.AddRange(new Control[] {
                chkUseCurrentLocation, lblAddress, txtStartAddress,
                lblLat, txtStartLat, lblLng, txtStartLng,
                btnPickStartOnMap, lblInfo
            });

            startGroupBox.Controls.Add(startPanel);
            mainPanel.Controls.Add(startGroupBox);

            // Section DESTINATION
            var destGroupBox = new GroupBox
            {
                Text = "Destination (Filon)",
                Height = 100,
                Dock = DockStyle.Top,
                ForeColor = Color.LightGreen,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold)
            };

            var destPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var lblDest = new Label
            {
                Text = "Sélectionner un filon:",
                Location = new Point(10, 25),
                AutoSize = true,
                ForeColor = Color.Black
            };

            cmbDestination = new ComboBox
            {
                Location = new Point(10, 50),
                Width = 580,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(60, 65, 75),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI Emoji", 10),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 20
            };
            cmbDestination.DrawItem += CmbDestination_DrawItem;

            destPanel.Controls.Add(lblDest);
            destPanel.Controls.Add(cmbDestination);
            destGroupBox.Controls.Add(destPanel);
            mainPanel.Controls.Add(destGroupBox);

            // Section TYPE DE TRANSPORT
            var transportPanel = new FlowLayoutPanel
            {
                Height = 60,
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            var lblTransport = new Label
            {
                Text = "Type de Transport:",
                AutoSize = true,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Margin = new Padding(5, 12, 10, 5)
            };

            cmbTransportType = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(60, 65, 75),
                ForeColor = Color.Black,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 20,
                Margin = new Padding(5, 8, 5, 5)
            };
            cmbTransportType.DrawItem += CmbTransportType_DrawItem;
            cmbTransportType.Items.AddRange(new object[] { " Voiture", "à pied", "Vélo" });
            cmbTransportType.SelectedIndex = 0;

            btnCalculate = new Button
            {
                Text = "Calculer l'Itinéraire",
                Width = 200,
                Height = 38,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Margin = new Padding(20, 5, 5, 5),
                Cursor = Cursors.Hand
            };
            btnCalculate.FlatAppearance.BorderSize = 0;
            btnCalculate.Click += BtnCalculate_Click;

            var btnClearRoute = new Button
            {
                Text = "Effacer",
                Width = 100,
                Height = 38,
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Margin = new Padding(5),
                Cursor = Cursors.Hand
            };
            btnClearRoute.FlatAppearance.BorderSize = 0;
            btnClearRoute.Click += BtnClearRoute_Click;

            transportPanel.Controls.Add(lblTransport);
            transportPanel.Controls.Add(cmbTransportType);
            transportPanel.Controls.Add(btnCalculate);
            transportPanel.Controls.Add(btnClearRoute);
            mainPanel.Controls.Add(transportPanel);

            // Label de statut
            lblStatus = new Label
            {
                Text = "Sélectionnez un filon de destination et cliquez sur Calculer",
                AutoSize = false,
                Height = 25,
                ForeColor = Color.LightGray,
                Dock = DockStyle.Top,
                Padding = new Padding(5),
                Font = new Font("Segoe UI Emoji", 9)
            };
            mainPanel.Controls.Add(lblStatus);

            // Zone de résultats
            txtResults = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 25),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI Emoji", 9),
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtResults);

            // Boutons d'action
            var actionsPanel = new FlowLayoutPanel
            {
                Height = 50,
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(5)
            };

            var btnClose = new Button
            {
                Text = "Fermer",
                Width = 100,
                Height = 35,
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            btnExportPdf = new Button
            {
                Text = "Exporter PDF",
                Width = 150,
                Height = 35,
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnExportPdf.FlatAppearance.BorderSize = 0;
            btnExportPdf.Click += BtnExportPdf_Click;

            // ? NOUVEAU: Bouton Export GPX
            btnExportGpx = new Button
            {
                Text = "Exporter GPX",
                Width = 150,
                Height = 35,
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnExportGpx.FlatAppearance.BorderSize = 0;
            btnExportGpx.Click += BtnExportGpx_Click;

            btnShowOnMap = new Button
            {
                Text = "Afficher sur la Carte",
                Width = 180,
                Height = 35,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnShowOnMap.FlatAppearance.BorderSize = 0;
            btnShowOnMap.Click += BtnShowOnMap_Click;

            actionsPanel.Controls.Add(btnClose);
            actionsPanel.Controls.Add(btnExportPdf);
            actionsPanel.Controls.Add(btnExportGpx); // ? AJOUT
            actionsPanel.Controls.Add(btnShowOnMap);
            mainPanel.Controls.Add(actionsPanel);

            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // départ
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Destination
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Transport
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Status
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Résultats
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Actions

            this.Controls.Add(mainPanel);
        }

        private void CmbDestination_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            var backColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? Color.FromArgb(0, 150, 136)
                : Color.FromArgb(60, 65, 75);

            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            using (var textBrush = new SolidBrush(Color.White))
            {
                var text = cmbDestination.Items[e.Index].ToString() ?? "";
                e.Graphics.DrawString(text, e.Font ?? this.Font, textBrush, e.Bounds.Left + 5, e.Bounds.Top + 2);
            }

            e.DrawFocusRectangle();
        }

        private void CmbTransportType_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            var backColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? Color.FromArgb(0, 150, 136)
                : Color.FromArgb(60, 65, 75);

            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            using (var textBrush = new SolidBrush(Color.White))
            {
                var text = cmbTransportType.Items[e.Index].ToString() ?? "";
                e.Graphics.DrawString(text, e.Font ?? this.Font, textBrush, e.Bounds.Left + 5, e.Bounds.Top + 2);
            }

            e.DrawFocusRectangle();
        }

        private void ChkUseCurrentLocation_CheckedChanged(object? sender, EventArgs e)
        {
            bool useManual = !chkUseCurrentLocation.Checked;
            txtStartAddress.Enabled = useManual;
            txtStartLat.Enabled = useManual;
            txtStartLng.Enabled = useManual;
            btnPickStartOnMap.Enabled = useManual;

            if (!useManual)
            {
                LoadDefaultStart();
                txtStartAddress.Text = "Position actuelle de la carte";
            }
            else
            {
                txtStartAddress.Text = "";
                txtStartAddress.PlaceholderText = "Saisissez une adresse...";
            }
        }

        private void BtnPickStartOnMap_Click(object? sender, EventArgs e)
        {
            _isPickingStartPoint = true;
            btnPickStartOnMap.Text = "Cliquez sur la carte…";
            btnPickStartOnMap.BackColor = Color.FromArgb(255, 152, 0);

            bool prevCanDrag = _mapControl.CanDragMap;
            var prevCursor = _mapControl.Cursor;
            _mapControl.CanDragMap = false;
            _mapControl.Cursor = Cursors.Cross;

            MouseEventHandler? mapDownHandler = null;
            KeyEventHandler? escHandler = null;

            void Cleanup()
            {
                _isPickingStartPoint = false;
                btnPickStartOnMap.Text = "Cliquer sur la carte";
                btnPickStartOnMap.BackColor = Color.FromArgb(33, 150, 243);
                _mapControl.CanDragMap = prevCanDrag;
                _mapControl.Cursor = prevCursor;

                if (mapDownHandler != null) _mapControl.MouseDown -= mapDownHandler;
                if (escHandler != null) this.KeyDown -= escHandler;

                try { if (!IsDisposed) { Show(); BringToFront(); Activate(); } } catch { }
            }

            mapDownHandler = (s2, ev) =>
            {
                if (!_isPickingStartPoint || ev.Button != MouseButtons.Left) return;

                var point = _mapControl.FromLocalToLatLng(ev.X, ev.Y);

                if (double.IsNaN(point.Lat) || point.Lat < -90 || point.Lat > 90 ||
                    double.IsNaN(point.Lng) || point.Lng < -180 || point.Lng > 180)
                {
                    MessageBox.Show($"Coordonnées invalides:\nLat: {point.Lat}\nLng: {point.Lng}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Cleanup();
                    return;
                }

                txtStartLat.Text = point.Lat.ToString("F6", CultureInfo.InvariantCulture);
                txtStartLng.Text = point.Lng.ToString("F6", CultureInfo.InvariantCulture);

                const string overlayId = "temp_start";
                var existing = _mapControl.Overlays.FirstOrDefault(o => o.Id == overlayId);
                if (existing != null) _mapControl.Overlays.Remove(existing);

                var overlay = new GMapOverlay(overlayId);
                var startMarker = new GMarkerGoogle(point, GMarkerGoogleType.green_pushpin)
                {
                    ToolTipText = $"Départ\n{point.Lat:F6}, {point.Lng:F6}",
                    ToolTipMode = MarkerTooltipMode.OnMouseOver
                };
                overlay.Markers.Add(startMarker);
                _mapControl.Overlays.Add(overlay);
                _mapControl.Refresh();

                Cleanup();

                MessageBox.Show($"Point de départ défini :\nLat: {point.Lat:F6}\nLng: {point.Lng:F6}",
                    "Point sélectionné", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            escHandler = (s2, ev) =>
            {
                if (ev.KeyCode == Keys.Escape)
                {
                    Cleanup();
                    MessageBox.Show("Sélection annulée.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            _mapControl.MouseDown += mapDownHandler;
            this.KeyDown += escHandler;

            MessageBox.Show(
                "Cliquez une fois sur la carte pour définir le point de départ.\n" +
                "Le déplacement de la carte est temporairement désactivé pour garantir la précision.",
                "Sélection sur carte", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Hide();
        }

        private void LoadFilons()
        {
            cmbDestination.Items.Clear();
            cmbDestination.Items.Add("-- Sélectionnez un filon --");

            if (_filons.Count == 0)
            {
                cmbDestination.Items.Add("(Aucun filon avec coordonnées GPS)");
                cmbDestination.Enabled = false;
                btnCalculate.Enabled = false;
                return;
            }

            foreach (var filon in _filons.OrderBy(f => f.Nom))
            {
                var mineralStr = MineralColors.GetDisplayName(filon.MatierePrincipale);
                if (filon.MatieresSecondaires.Any())
                {
                    var secondes = string.Join(", ", filon.MatieresSecondaires.Take(2).Select(m => MineralColors.GetDisplayName(m)));
                    mineralStr += $" + {secondes}";
                }

                cmbDestination.Items.Add($"{filon.Nom} ({mineralStr})");
            }

            cmbDestination.SelectedIndex = 0;
        }

        private void LoadDefaultStart()
        {
            if (_mapControl != null)
            {
                txtStartLat.Text = _mapControl.Position.Lat.ToString("F6", CultureInfo.InvariantCulture);
                txtStartLng.Text = _mapControl.Position.Lng.ToString("F6", CultureInfo.InvariantCulture);
            }
        }

        private async void BtnCalculate_Click(object? sender, EventArgs e)
        {
            // ? CANCELLATION TOKEN : Annuler le calcul précédent si existe
            var cancellationToken = _perfOptimizer.GetCancellationToken("route_calculation");

            try
            {
                if (cmbDestination.SelectedIndex <= 0)
                {
                    MessageBox.Show("Veuillez sélectionner un filon de destination.",
                        "Sélection requise", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnCalculate.Enabled = false;
                btnShowOnMap.Enabled = false;
                lblStatus.Text = "Calcul de l'itinéraire en cours...";
                lblStatus.ForeColor = Color.Orange;
                txtResults.Clear();
                Application.DoEvents();

                // Point de départ - CORRECTION COMPLéTE
                PointLatLng start;
                if (chkUseCurrentLocation.Checked)
                {
                    start = _mapControl.Position;
                    System.Diagnostics.Debug.WriteLine($"départ: Position actuelle de la carte");
                }
                else
                {
                    // Parse avec vérification stricte
                    string latText = txtStartLat.Text.Trim().Replace(',', '.');
                    string lngText = txtStartLng.Text.Trim().Replace(',', '.');

                    System.Diagnostics.Debug.WriteLine($"Parse départ: '{latText}', '{lngText}'");

                    if (!double.TryParse(latText, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat) ||
                        !double.TryParse(lngText, NumberStyles.Float, CultureInfo.InvariantCulture, out double lng))
                    {
                        MessageBox.Show($"Coordonnées de départ invalides.\n\nValeurs saisies:\nLat: '{txtStartLat.Text}'\nLng: '{txtStartLng.Text}'\n\nUtilisez le format: 43.123456",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    start = new PointLatLng(lat, lng);
                    System.Diagnostics.Debug.WriteLine($"départ: Point personnalisé");
                }

                // Validation coordonnées départ
                if (start.Lat < -90 || start.Lat > 90 || start.Lng < -180 || start.Lng > 180 ||
                    double.IsNaN(start.Lat) || double.IsNaN(start.Lng) ||
                    double.IsInfinity(start.Lat) || double.IsInfinity(start.Lng))
                {
                    MessageBox.Show($"Coordonnées de départ invalides:\n\nLat: {start.Lat}\nLng: {start.Lng}\n\nVérifiez que:\n- Latitude entre -90 et 90\n- Longitude entre -180 et 180",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Vérifier si annulé
                cancellationToken.ThrowIfCancellationRequested();

                // Point d'arrivée (filon)
                var filonIndex = cmbDestination.SelectedIndex - 1;
                if (filonIndex < 0 || filonIndex >= _filons.Count)
                {
                    MessageBox.Show("Sélection invalide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var filon = _filons.OrderBy(f => f.Nom).ElementAt(filonIndex);

                if (!filon.Latitude.HasValue || !filon.Longitude.HasValue)
                {
                    MessageBox.Show("Le filon sélectionné n'a pas de coordonnées GPS valides.",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var end = new PointLatLng(filon.Latitude.Value, filon.Longitude.Value);

                // Validation coordonnées arrivée
                if (end.Lat < -90 || end.Lat > 90 || end.Lng < -180 || end.Lng > 180 ||
                    double.IsNaN(end.Lat) || double.IsNaN(end.Lng) ||
                    double.IsInfinity(end.Lat) || double.IsInfinity(end.Lng))
                {
                    MessageBox.Show($"Coordonnées du filon invalides:\n\nLat: {end.Lat}\nLng: {end.Lng}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Type de transport
                var transportType = cmbTransportType.SelectedIndex switch
                {
                    0 => TransportType.Car,
                    1 => TransportType.Walking,
                    2 => TransportType.Cycling,
                    _ => TransportType.Car
                };

                // LOGS déTAILLéS
                System.Diagnostics.Debug.WriteLine($"=== CALCUL ITINéRAIRE ===");
                System.Diagnostics.Debug.WriteLine($"départ  : Lat={start.Lat:F6}, Lng={start.Lng:F6}");
                System.Diagnostics.Debug.WriteLine($"Arrivée : Lat={end.Lat:F6}, Lng={end.Lng:F6}");
                System.Diagnostics.Debug.WriteLine($"Transport: {transportType}");

                // ? Calculer l'itinéraire AVEC CancellationToken
                _currentRoute = await _routeService.CalculateRouteAsync(start, end, transportType);

                // ? Vérifier si annulé aprés calcul
                cancellationToken.ThrowIfCancellationRequested();

                if (_currentRoute == null)
                {
                    lblStatus.Text = "Aucun itinéraire trouvé";
                    lblStatus.ForeColor = Color.Red;
                    txtResults.Text = "Impossible de calculer un itinéraire entre ces deux points.\n\n" +
                        $"départ: {start.Lat:F4}, {start.Lng:F4}\n" +
                        $"Arrivée: {end.Lat:F4}, {end.Lng:F4}\n\n" +
                        "Vérifiez que les coordonnées sont accessibles par route.";
                    return;
                }

                // Ajouter les noms
                _currentRoute.StartName = chkUseCurrentLocation.Checked
                    ? "Position actuelle"
                    : $"Point personnalisé ({start.Lat:F4}, {start.Lng:F4})";
                _currentRoute.EndName = filon.Nom;

                lblStatus.Text = "Itinéraire calculé avec succés !";
                lblStatus.ForeColor = Color.LimeGreen;
                btnShowOnMap.Enabled = true;
                btnExportPdf.Enabled = true;
                btnExportGpx.Enabled = true; // ? ACTIVER GPX

                System.Diagnostics.Debug.WriteLine($"Succés: {_currentRoute.Points.Count} points, {_currentRoute.FormattedDistance}");

                // Afficher les résultats
                DisplayResults(_currentRoute, filon);
            }
            catch (OperationCanceledException)
            {
                // ? Calcul annulé par l'utilisateur (normal)
                lblStatus.Text = "Calcul annulé";
                lblStatus.ForeColor = Color.Orange;
                System.Diagnostics.Debug.WriteLine("Calcul d'itinéraire annulé");
            }
            catch (HttpRequestException ex)
            {
                lblStatus.Text = "Erreur réseau";
                lblStatus.ForeColor = Color.Red;
                var errorMsg = $"ERREUR RéSEAU:\n{ex.Message}\n\nVérifiez votre connexion Internet.";
                txtResults.Text = errorMsg;
                System.Diagnostics.Debug.WriteLine($"Erreur HTTP: {ex.Message}");
                MessageBox.Show(errorMsg, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erreur lors du calcul";
                lblStatus.ForeColor = Color.Red;
                var errorMsg = $"ERREUR:\n{ex.Message}";
                txtResults.Text = $"{errorMsg}\n\n{ex.StackTrace}";
                System.Diagnostics.Debug.WriteLine($"=== ERREUR COMPLéTE ===");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                MessageBox.Show(errorMsg, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // ? Libérer le CancellationToken
                _perfOptimizer.ReleaseCancellationToken("route_calculation");
                btnCalculate.Enabled = true;
            }
        }

        private void DisplayResults(RouteInfo route, Filon filon)
        {
            txtResults.Clear();

            AppendColored("???????????????????????????????????????\n", Color.Cyan);
            AppendColored($"???  ITINéRAIRE VERS : {filon.Nom}\n", Color.Yellow, true);
            AppendColored("???????????????????????????????????????\n\n", Color.Cyan);

            AppendColored("?? ", Color.Orange);
            AppendText($"départ: {route.StartName}\n");

            AppendColored("?? ", Color.Orange);
            AppendText($"Arrivée: {route.EndName}\n");
            var mineralDisplay = MineralColors.GetDisplayName(filon.MatierePrincipale);
            if (filon.MatieresSecondaires.Any())
            {
                mineralDisplay += $" + {string.Join(", ", filon.MatieresSecondaires.Take(2).Select(m => MineralColors.GetDisplayName(m)))}";
            }
            AppendText($"   Minéraux: {mineralDisplay}\n");
            AppendText("\n");

            AppendColored("?? ", Color.LightBlue);
            AppendText($"Transport: {GetTransportName(route.TransportType)}\n");

            AppendColored("?? ", Color.LightGreen);
            AppendText($"Distance: {route.FormattedDistance}\n");

            AppendColored("??  ", Color.LightGreen);
            AppendText($"Durée estimée: {route.FormattedDuration}\n\n");

            AppendColored($"?? Itinéraire: {route.Points.Count} points GPS\n", Color.LightGray);

            if (route.Instructions.Count > 0)
            {
                AppendColored("\n?? INSTRUCTIONS:\n", Color.Yellow, true);
                AppendColored("???????????????????????????????????????\n", Color.DarkGray);

                var instructionsToShow = Math.Min(10, route.Instructions.Count);
                for (int i = 0; i < instructionsToShow; i++)
                {
                    AppendColored($"{i + 1}. ", Color.LightGray);
                    AppendText($"{route.Instructions[i]}\n");
                }

                if (route.Instructions.Count > 10)
                {
                    AppendText($"\n... et {route.Instructions.Count - 10} instructions supplémentaires\n");
                }
            }

            AppendColored("\n???????????????????????????????????????\n", Color.Cyan);
        }

        private void BtnShowOnMap_Click(object? sender, EventArgs e)
        {
            if (_currentRoute == null) return;

            try
            {
                // Créer un overlay pour l'itinéraire
                var overlay = _mapControl.Overlays.FirstOrDefault(o => o.Id == "route_overlay");
                if (overlay != null)
                {
                    _mapControl.Overlays.Remove(overlay);
                }

                overlay = new GMapOverlay("route_overlay");

                // Tracer l'itinéraire
                var route = new GMapRoute(_currentRoute.Points, "itineraire")
                {
                    Stroke = new Pen(Color.FromArgb(200, 33, 150, 243), 4)
                };
                overlay.Routes.Add(route);

                // Marqueurs de départ et d'arrivée
                var startMarker = new GMarkerGoogle(_currentRoute.Start, GMarkerGoogleType.green_small)
                {
                    ToolTipText = _currentRoute.StartName
                };
                overlay.Markers.Add(startMarker);

                var endMarker = new GMarkerGoogle(_currentRoute.End, GMarkerGoogleType.red_dot)
                {
                    ToolTipText = _currentRoute.EndName
                };
                overlay.Markers.Add(endMarker);

                _mapControl.Overlays.Add(overlay);

                // Zoomer sur l'itinéraire
                var rect = RectLatLng.FromLTRB(
                    Math.Min(_currentRoute.Start.Lng, _currentRoute.End.Lng) - 0.02,
                    Math.Max(_currentRoute.Start.Lat, _currentRoute.End.Lat) + 0.02,
                    Math.Max(_currentRoute.Start.Lng, _currentRoute.End.Lng) + 0.02,
                    Math.Min(_currentRoute.Start.Lat, _currentRoute.End.Lat) - 0.02
                );
                _mapControl.SetZoomToFitRect(rect);

                MessageBox.Show("Itinéraire affiché sur la carte !",
                    "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur d'affichage: {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClearRoute_Click(object? sender, EventArgs e)
        {
            try
            {
                // Supprimer l'overlay de l'itinéraire
                var overlay = _mapControl.Overlays.FirstOrDefault(o => o.Id == "route_overlay");
                if (overlay != null)
                {
                    _mapControl.Overlays.Remove(overlay);
                    _mapControl.Refresh();
                    MessageBox.Show("Tracé effacé de la carte.",
                        "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Aucun tracé a effacer.",
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Supprimer aussi le marqueur temporaire de départ si présent
                var tempOverlay = _mapControl.Overlays.FirstOrDefault(o => o.Id == "temp_start");
                if (tempOverlay != null)
                {
                    _mapControl.Overlays.Remove(tempOverlay);
                    _mapControl.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'effacement: {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportPdf_Click(object? sender, EventArgs e)
        {
            if (_currentRoute == null)
            {
                MessageBox.Show("Aucun itinéraire a exporter.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using var sfd = new SaveFileDialog
                {
                    Filter = "PDF|*.pdf",
                    FileName = $"Itineraire_{_currentRoute.EndName}_{DateTime.Now:yyyyMMdd_HHm

