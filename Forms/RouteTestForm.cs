using GMap.NET;
using wmine.Models;
using wmine.Services;

namespace wmine.Forms
{
    /// <summary>
    /// Formulaire de test du service de calcul d'itinéraires
    /// </summary>
    public partial class RouteTestForm : Form
    {
        private readonly RouteService _routeService = new RouteService();
        private TextBox txtStartLat;
        private TextBox txtStartLng;
        private TextBox txtEndLat;
        private TextBox txtEndLng;
        private ComboBox cmbTransportType;
        private Button btnCalculate;
        private RichTextBox txtResults;
        private Label lblStatus;

        public RouteTestForm()
        {
            InitializeComponent();
            LoadDefaultLocations();
        }

        private void InitializeComponent()
        {
            this.Text = "Test Service de Routing";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 5
            };

            // Section départ
            var startPanel = CreateLocationPanel("Point de départ", out txtStartLat, out txtStartLng);
            mainPanel.Controls.Add(startPanel);

            // Section Arrivée
            var endPanel = CreateLocationPanel("Point d'Arrivée", out txtEndLat, out txtEndLng);
            mainPanel.Controls.Add(endPanel);

            // Type de transport
            var transportPanel = new FlowLayoutPanel
            {
                Height = 50,
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight
            };

            var lblTransport = new Label
            {
                Text = "Type de Transport:",
                AutoSize = true,
                ForeColor = Color.White,
                Margin = new Padding(5, 10, 10, 5)
            };

            cmbTransportType = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White,
                Margin = new Padding(5, 5, 5, 5)
            };
            cmbTransportType.Items.AddRange(new object[] { "Voiture", "à pied", "Vélo" });
            cmbTransportType.SelectedIndex = 0;

            btnCalculate = new Button
            {
                Text = "Calculer l'Itinéraire",
                Width = 180,
                Height = 35,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(20, 5, 5, 5),
                Cursor = Cursors.Hand
            };
            btnCalculate.FlatAppearance.BorderSize = 0;
            btnCalculate.Click += BtnCalculate_Click;

            transportPanel.Controls.Add(lblTransport);
            transportPanel.Controls.Add(cmbTransportType);
            transportPanel.Controls.Add(btnCalculate);
            mainPanel.Controls.Add(transportPanel);

            // Label de statut
            lblStatus = new Label
            {
                Text = "Prét a calculer",
                AutoSize = true,
                ForeColor = Color.LightGray,
                Dock = DockStyle.Top,
                Padding = new Padding(5)
            };
            mainPanel.Controls.Add(lblStatus);

            // Zone de résultats
            txtResults = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10),
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtResults);

            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            this.Controls.Add(mainPanel);
        }

        private GroupBox CreateLocationPanel(string title, out TextBox latBox, out TextBox lngBox)
        {
            var panel = new GroupBox
            {
                Text = title,
                Height = 80,
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Padding = new Padding(10)
            };

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };

            var lblLat = new Label { Text = "Latitude:", AutoSize = true, Margin = new Padding(5, 8, 5, 5) };
            latBox = new TextBox
            {
                Width = 150,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 5, 15, 5)
            };

            var lblLng = new Label { Text = "Longitude:", AutoSize = true, Margin = new Padding(5, 8, 5, 5) };
            lngBox = new TextBox
            {
                Width = 150,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 5, 5, 5)
            };

            flowPanel.Controls.AddRange(new Control[] { lblLat, latBox, lblLng, lngBox });
            panel.Controls.Add(flowPanel);

            return panel;
        }

        private void LoadDefaultLocations()
        {
            // Par défaut : Toulon / Nice
            txtStartLat.Text = "43.1242";
            txtStartLng.Text = "5.9280";
            txtEndLat.Text = "43.7102";
            txtEndLng.Text = "7.2620";
        }

        private async void BtnCalculate_Click(object? sender, EventArgs e)
        {
            try
            {
                btnCalculate.Enabled = false;
                lblStatus.Text = "Calcul en cours...";
                lblStatus.ForeColor = Color.Orange;
                txtResults.Clear();
                Application.DoEvents();

                // Validation des coordonnées
                if (!double.TryParse(txtStartLat.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double startLat) ||
                    !double.TryParse(txtStartLng.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double startLng) ||
                    !double.TryParse(txtEndLat.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double endLat) ||
                    !double.TryParse(txtEndLng.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double endLng))
                {
                    MessageBox.Show("Coordonnées invalides. Utilisez le format : 43.1242", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var start = new PointLatLng(startLat, startLng);
                var end = new PointLatLng(endLat, endLng);

                var transportType = cmbTransportType.SelectedIndex switch
                {
                    0 => TransportType.Car,
                    1 => TransportType.Walking,
                    2 => TransportType.Cycling,
                    _ => TransportType.Car
                };

                var route = await _routeService.CalculateRouteAsync(start, end, transportType);

                if (route == null)
                {
                    lblStatus.Text = "Aucun itinéraire trouvé";
                    lblStatus.ForeColor = Color.Red;
                    txtResults.Text = "Aucun itinéraire trouvé entre ces deux points.";
                    return;
                }

                lblStatus.Text = "Itinéraire calculé avec succés !";
                lblStatus.ForeColor = Color.LimeGreen;

                // Affichage des résultats
                DisplayResults(route);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erreur lors du calcul";
                lblStatus.ForeColor = Color.Red;
                txtResults.Text = $"ERREUR:\n{ex.Message}\n\n{ex.StackTrace}";
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCalculate.Enabled = true;
            }
        }

        private void DisplayResults(RouteInfo route)
        {
            txtResults.Clear();

            AppendColored("???????????????????????????????????????????????\n", Color.Cyan);
            AppendColored($"???  ITINéRAIRE CALCULé\n", Color.Yellow, true);
            AppendColored("???????????????????????????????????????????????\n\n", Color.Cyan);

            // Informations générales
            AppendColored("?? ", Color.Orange);
            AppendText($"départ: {route.Start.Lat:F6}, {route.Start.Lng:F6}\n");

            AppendColored("?? ", Color.Orange);
            AppendText($"Arrivée: {route.End.Lat:F6}, {route.End.Lng:F6}\n\n");

            AppendColored("?? ", Color.LightBlue);
            AppendText($"Mode de transport: {GetTransportName(route.TransportType)}\n");

            AppendColored("?? ", Color.LightGreen);
            AppendText($"Distance: {route.FormattedDistance}\n");

            AppendColored("??  ", Color.LightGreen);
            AppendText($"Durée estimée: {route.FormattedDuration}\n\n");

            // Points de l'itinéraire
            AppendColored($"?? Points de l'itinéraire: {route.Points.Count}\n", Color.Yellow);
            if (route.Points.Count > 0)
            {
                AppendText($"   Premier point: {route.Points[0].Lat:F6}, {route.Points[0].Lng:F6}\n");
                if (route.Points.Count > 1)
                    AppendText($"   Dernier point: {route.Points[^1].Lat:F6}, {route.Points[^1].Lng:F6}\n");
            }

            // Instructions
            if (route.Instructions.Count > 0)
            {
                AppendColored("\n\n?? INSTRUCTIONS DE NAVIGATION:\n", Color.Yellow, true);
                AppendColored("???????????????????????????????????????????????\n", Color.DarkGray);

                for (int i = 0; i < route.Instructions.Count; i++)
                {
                    AppendColored($"{i + 1}. ", Color.LightGray);
                    AppendText($"{route.Instructions[i]}\n");
                }
            }

            AppendColored("\n???????????????????????????????????????????????\n", Color.Cyan);
            AppendColored("? Test réussi !", Color.LimeGreen, true);
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
