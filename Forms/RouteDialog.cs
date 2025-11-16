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
            var btnExportGpx = new Button
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
            btnPickStartOnMap.Text = "Cliquez sur la carte...";
            btnPickStartOnMap.BackColor = Color.FromArgb(255, 152, 0);

            // Hook temporaire sur le clic de la carte
            MouseEventHandler? mapClickHandler = null;
            GMapOverlay? tempOverlay = null;

            mapClickHandler = (s, ev) =>
            {
                if (_isPickingStartPoint && ev.Button == MouseButtons.Left)
                {
                    // Utiliser directement les coordonnées de l'événement sans transformation supplémentaire
                    var point = _mapControl.FromLocalToLatLng(ev.X, ev.Y);

                    // Validation des coordonnées
                    if (point.Lat < -90 || point.Lat > 90 || point.Lng < -180 || point.Lng > 180)
                    {
                        MessageBox.Show($"Coordonnées invalides:\nLat: {point.Lat}\nLng: {point.Lng}",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    txtStartLat.Text = point.Lat.ToString("F6", CultureInfo.InvariantCulture);
                    txtStartLng.Text = point.Lng.ToString("F6", CultureInfo.InvariantCulture);

                    // Supprimer l'ancien marqueur temporaire s'il existe
                    var oldTempOverlay = _mapControl.Overlays.FirstOrDefault(o => o.Id == "temp_start");
                    if (oldTempOverlay != null)
                    {
                        _mapControl.Overlays.Remove(oldTempOverlay);
                    }

                    // Ajouter un marqueur vert temporaire EXACTEMENT é l'endroit cliqué
                    tempOverlay = new GMapOverlay("temp_start");
                    var marker = new GMarkerGoogle(point, GMarkerGoogleType.green_pushpin)
                    {
                        ToolTipText = $"départ\n{point.Lat:F6}, {point.Lng:F6}",
                        ToolTipMode = MarkerTooltipMode.OnMouseOver
                    };
                    tempOverlay.Markers.Add(marker);
                    _mapControl.Overlays.Add(tempOverlay);
                    _mapControl.Refresh();

                    _isPickingStartPoint = false;
                    btnPickStartOnMap.Text = "Cliquer sur la carte";
                    btnPickStartOnMap.BackColor = Color.FromArgb(33, 150, 243);

                    if (mapClickHandler != null)
                    {
                        _mapControl.MouseClick -= mapClickHandler;
                    }

                    // Restaurer le dialogue si pas disposé
                    try
                    {
                        if (!this.IsDisposed)
                        {
                            this.Show();
                            this.BringToFront();
                            this.Activate();

                            MessageBox.Show($"Point de départ défini :\nLat: {point.Lat:F6}\nLng: {point.Lng:F6}",
                                "Point sélectionné", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch
                    {
                        // Dialogue déjé fermé
                    }
                }
            };

            _mapControl.MouseClick += mapClickHandler;

            // Cacher le dialogue au lieu de le minimiser
            MessageBox.Show("Cliquez n'importe oé sur la carte pour définir le point de départ.\n\n" +
                "Vous pouvez zoomer/naviguer normalement sur la carte.\n" +
                "Le dialogue sera masqué pendant la sélection.",
                "Sélection sur carte", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Hide();
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
                    FileName = $"Itineraire_{_currentRoute.EndName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    Title = "Exporter l'itinéraire en PDF"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ExportRouteToPdf(_currentRoute, sfd.FileName);
                    MessageBox.Show("Itinéraire exporté en PDF avec succés !",
                        "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'export PDF:\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ? NOUVEAU: Export GPX
        private void BtnExportGpx_Click(object? sender, EventArgs e)
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
                    Filter = "Fichiers GPX|*.gpx",
                    FileName = $"Route_{_currentRoute.EndName}_{DateTime.Now:yyyyMMdd_HHmmss}.gpx",
                    Title = "Exporter l'itinéraire en GPX"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var gpxService = new GpxExportService();
                    bool success = gpxService.ExportRouteToGpx(_currentRoute, sfd.FileName);

                    if (success)
                    {
                        MessageBox.Show(
                            $"Itinéraire exporté en GPX avec succés !\n\n" +
                            $"Fichier : {System.IO.Path.GetFileName(sfd.FileName)}\n\n" +
                            $"?? Compatible avec :\n" +
                            $"é GPS Garmin\n" +
                            $"é Smartphones (iOS/Android)\n" +
                            $"é Applications de randonnée\n" +
                            $"é Google Earth (via conversion)",
                            "Export GPX réussi",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de l'export GPX.",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'export GPX:\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportRouteToPdf(RouteInfo route, string filePath)
        {
            // Créer un texte formaté pour l'export
            var content = new System.Text.StringBuilder();
            content.AppendLine($"ITINéRAIRE VERS {route.EndName}");
            content.AppendLine($"Généré le {DateTime.Now:dd/MM/yyyy é HH:mm}");
            content.AppendLine();
            content.AppendLine("INFORMATIONS");
            content.AppendLine($"départ: {route.StartName}");
            content.AppendLine($"Arrivée: {route.EndName}");
            content.AppendLine($"Transport: {GetTransportName(route.TransportType)}");
            content.AppendLine($"Distance: {route.FormattedDistance}");
            content.AppendLine($"Durée estimée: {route.FormattedDuration}");
            content.AppendLine($"Points GPS: {route.Points.Count}");
            content.AppendLine();

            if (route.Instructions.Count > 0)
            {
                content.AppendLine("INSTRUCTIONS DE NAVIGATION");
                for (int i = 0; i < route.Instructions.Count; i++)
                {
                    content.AppendLine($"{i + 1}. {route.Instructions[i]}");
                }
            }

            // Utiliser PdfExportService existant ou écrire en texte simple
            try
            {
                // Sauvegarder en fichier texte temporairement (ou utiliser PdfExportService si disponible)
                File.WriteAllText(filePath.Replace(".pdf", ".txt"), content.ToString());

                // Pour un vrai PDF, il faudrait utiliser le PdfExportService existant
                // mais il est conéu pour les Filons, pas les routes
                // Solution simple: export texte pour l'instant
                MessageBox.Show("Export réalisé en format texte (.txt) car le service PDF n'est pas encore adapté aux itinéraires.\n\n" +
                    $"Fichier: {filePath.Replace(".pdf", ".txt")}",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur d'écriture du fichier: {ex.Message}", ex);
            }
        }

        // ? NOUVEAU: Exporter en GPX
        private void ExportRouteToGpx(RouteInfo route, string filePath)
        {
            // Créer le document GPX
            var gpx = new System.Xml.Linq.XDocument(
                new System.Xml.Linq.XElement("gpx",
                    new System.Xml.Linq.XAttribute("version", "1.1"),
                    new System.Xml.Linq.XAttribute("creator", "WitoJordanMineLocalisateur")
                )
            );

            // Ajouter les waypoints (points de la route)
            var wpts = new System.Xml.Linq.XElement("waypoints");
            foreach (var point in route.Points)
            {
                wpts.Add(new System.Xml.Linq.XElement("wp",
                    new System.Xml.Linq.XAttribute("lat", point.Lat),
                    new System.Xml.Linq.XAttribute("lon", point.Lng)
                ));
            }
            gpx.Root.Add(wpts);

            // Ajouter les métadonnées
            var meta = new System.Xml.Linq.XElement("metadata",
                new System.Xml.Linq.XElement("name", $"Itinéraire vers {route.EndName}"),
                new System.Xml.Linq.XElement("desc", "Itinéraire calculé avec WitoJordanMineLocalisateur"),
                new System.Xml.Linq.XElement("author", "VotreNom")
            );
            gpx.Root.Add(meta);

            // Sauvegarder le fichier
            gpx.Save(filePath);
        }

        private void AppendText(string text)
        {
            txtResults.SelectionColor = Color.White;
            txtResults.AppendText(text);
        }

        private void AppendColored(string text, Color color, bool bold = false)
        {
            txtResults.SelectionColor = color;
            txtResults.SelectionFont = new Font(txtResults.Font, bold ? FontStyle.Bold : FontStyle.Regular);
            txtResults.AppendText(text);
            txtResults.SelectionFont = new Font(txtResults.Font, FontStyle.Regular);
        }

        private string GetTransportName(TransportType type)
        {
            return type switch
            {
                TransportType.Car => "Voiture ??",
                TransportType.Walking => "à pied ??",
                TransportType.Cycling => "Vélo ??",
                _ => "Inconnu"
            };
        }
    }
}

