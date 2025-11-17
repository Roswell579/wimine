using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wmine.UI
{
    /// <summary>
    /// Widget météo désactivé : placeholder sans dépendance au service météo.
    /// </summary>
    public class WeatherWidget : Panel
    {
        private Label _lblMainInfo;
        private Label _lblDetails;
        private Label _lblLastUpdate;
        private Button _btnRefresh;
        private bool _isInitialized;

        public WeatherWidget()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Width = 280;
            this.Height = 80;
            this.BackColor = Color.FromArgb(180, 40, 45, 55);
            this.BorderStyle = BorderStyle.None;
            this.Padding = new Padding(10);

            _lblMainInfo = new Label
            {
                Text = "Météo désactivée",
                Location = new Point(10, 10),
                Width = 260,
                Height = 20,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            Controls.Add(_lblMainInfo);

            _lblDetails = new Label
            {
                Text = "Cette build ne contient plus le module météo.",
                Location = new Point(10, 32),
                Width = 260,
                Height = 20,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(200, 200, 200),
                BackColor = Color.Transparent
            };
            Controls.Add(_lblDetails);

            _lblLastUpdate = new Label
            {
                Text = "",
                Location = new Point(10, 54),
                Width = 260,
                Height = 14,
                Font = new Font("Segoe UI", 7, FontStyle.Italic),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent
            };
            Controls.Add(_lblLastUpdate);

            _btnRefresh = new Button
            {
                Text = "🔒",
                Location = new Point(235, 8),
                Width = 35,
                Height = 30,
                BackColor = Color.FromArgb(60, 65, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            Controls.Add(_btnRefresh);
        }

        /// <summary>
        /// Remplacé : méthode neutre pour compatibilité des appels.
        /// </summary>
        public Task SetLocationAsync(decimal latitude, decimal longitude)
        {
            // ne fait rien : météo désactivée
            if (!_isInitialized)
            {
                _isInitialized = true;
                _lblLastUpdate.Text = DateTime.Now.ToString("HH:mm");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Remplacé : opération neutre.
        /// </summary>
        public Task RefreshWeatherAsync()
        {
            // ne fait rien : météo désactivée
            return Task.CompletedTask;
        }

        public void Clear()
        {
            _lblMainInfo.Text = "Aucun filon sélectionné";
            _lblDetails.Text = "";
            _lblLastUpdate.Text = "";
        }
    }
}
