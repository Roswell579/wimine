using wmine.Services;

namespace wmine.UI
{
    /// <summary>
    /// Widget météo personnalisé pour afficher les conditions météorologiques
    /// Hérite de DraggablePanel pour permettre le déplacement
    /// VERSION RÉTRACTABLE
    /// </summary>
    public class WeatherWidget : DraggablePanel
    {
        private Label lblTitle;
        private Label lblMainInfo;
        private Label lblDetails;
        private Label lblLastUpdate;
        private Button btnRefresh;
        private Button _btnToggle;
        private Panel _contentPanel;
        private bool _isExpanded = true;
        private decimal? _latitude;
        private decimal? _longitude;
        private readonly WeatherService _weatherService;

        public WeatherWidget()
        {
            _weatherService = new WeatherService();
            InitializeComponent();

            // ? INITIALISATION PAR DÉFAUT avec coordonnées du Var
            _ = SetLocationAsync(43.4m, 6.3m);
        }

        private void InitializeComponent()
        {
            // Panel principal
            this.Width = 280;
            this.Height = 60; // Rétracté par défaut
            this.BackColor = Color.FromArgb(180, 40, 45, 55);
            this.BorderStyle = BorderStyle.None;
            this.Padding = new Padding(10);

            // Header avec titre et toggle
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Width = this.Width,
                Height = 35,
                BackColor = Color.Transparent
            };

            // Titre
            lblTitle = new Label
            {
                Text = "Météo",
                Location = new Point(10, 10),
                Width = 180,
                Height = 25,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 181, 246),
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblTitle);

            // Bouton toggle
            _btnToggle = new Button
            {
                Text = "?",
                Location = new Point(190, 5),
                Width = 35,
                Height = 30,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnToggle.FlatAppearance.BorderSize = 0;
            _btnToggle.Click += BtnToggle_Click;
            headerPanel.Controls.Add(_btnToggle);

            // Bouton rafraîchir
            btnRefresh = new Button
            {
                Text = "??",
                Location = new Point(235, 5),
                Width = 35,
                Height = 30,
                BackColor = Color.FromArgb(60, 65, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += async (s, e) => await RefreshWeatherAsync();
            headerPanel.Controls.Add(btnRefresh);

            this.Controls.Add(headerPanel);

            // Panel contenu (détails météo)
            _contentPanel = new Panel
            {
                Location = new Point(0, 40),
                Width = this.Width,
                Height = 140,
                BackColor = Color.Transparent,
                Visible = false // Rétracté par défaut
            };

            // Info principale (température et conditions)
            lblMainInfo = new Label
            {
                Text = "Chargement...",
                Location = new Point(10, 5),
                Width = 260,
                Height = 50,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _contentPanel.Controls.Add(lblMainInfo);

            // Détails (humidité, vent)
            lblDetails = new Label
            {
                Text = "",
                Location = new Point(10, 55),
                Width = 260,
                Height = 50,
                Font = new Font("Segoe UI Emoji", 9),
                ForeColor = Color.FromArgb(200, 200, 200),
                BackColor = Color.Transparent
            };
            _contentPanel.Controls.Add(lblDetails);

            // Dernière mise à jour
            lblLastUpdate = new Label
            {
                Text = "",
                Location = new Point(10, 110),
                Width = 260,
                Height = 20,
                Font = new Font("Segoe UI", 7, FontStyle.Italic),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent
            };
            _contentPanel.Controls.Add(lblLastUpdate);

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
                _btnToggle.Text = "?";
            }
            else
            {
                // Collapse
                _contentPanel.Visible = false;
                this.Height = 60;
                _btnToggle.Text = "?";
            }
        }

        /// <summary>
        /// Définit les coordonnées GPS et charge la météo
        /// </summary>
        public async Task SetLocationAsync(decimal latitude, decimal longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
            await RefreshWeatherAsync();
        }

        /// <summary>
        /// Rafraîchit les données météo
        /// </summary>
        public async Task RefreshWeatherAsync()
        {
            if (!_latitude.HasValue || !_longitude.HasValue)
            {
                lblMainInfo.Text = "Aucune position";
                lblDetails.Text = "Sélectionnez un filon\navec coordonnées GPS";
                lblLastUpdate.Text = "";
                return;
            }

            try
            {
                // Animation de chargement
                btnRefresh.Enabled = false;
                lblMainInfo.Text = "Chargement...";
                lblDetails.Text = "";

                var weather = await _weatherService.GetCurrentWeatherAsync(_latitude.Value, _longitude.Value);

                if (weather != null)
                {
                    // Affichage des données
                    lblMainInfo.Text = $"{weather.Icon} {weather.Temperature:F1}°C\n{weather.Conditions}";

                    lblDetails.Text = $"Humidité: {weather.Humidity}%\n" +
                                    $"Vent: {weather.WindSpeed:F1} km/h\n" +
                                    $" Ressenti: {weather.FeelsLike:F1}°C";

                    lblLastUpdate.Text = $"Mis à jour: {weather.Timestamp:HH:mm}";
                    lblLastUpdate.ForeColor = Color.FromArgb(100, 181, 246);
                }
                else
                {
                    lblMainInfo.Text = "Erreur météo";
                    lblDetails.Text = "Impossible de récupérer\nles données météo";
                    lblLastUpdate.Text = "Service temporairement indisponible";
                    lblLastUpdate.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblMainInfo.Text = "Erreur";
                lblDetails.Text = $"Erreur: {ex.Message}";
                lblLastUpdate.Text = "";
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        /// <summary>
        /// Réinitialise le widget
        /// </summary>
        public void Clear()
        {
            _latitude = null;
            _longitude = null;
            lblMainInfo.Text = "Aucun filon sélectionné";
            lblDetails.Text = "";
            lblLastUpdate.Text = "";
        }
    }
}
