using System;
using System.Drawing;
using System.Windows.Forms;

namespace wmine.UI
{
    /// <summary>
    /// Panneau météo désactivé : placeholder sans dépendance au service météo.
    /// </summary>
    public class WeatherPanel : Panel
    {
        private Label _lblTitle;
        private Label _lblInfo;
        private Button _btnRefresh;

        public WeatherPanel()
        {
            InitializeBase();
            BuildContent();
        }

        private void InitializeBase()
        {
            Width = 300;
            Height = 120;
            BackColor = Color.FromArgb(220, 30, 35, 45);
            Padding = new Padding(10);
            ForeColor = Color.White;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        private void BuildContent()
        {
            _lblTitle = new Label
            {
                Text = "Météo (désactivée)",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 181, 246),
                Location = new Point(8, 8),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(_lblTitle);

            _lblInfo = new Label
            {
                Name = "lblWeatherInfo",
                Text = "La fonctionnalité météo a été désactivée dans cette build.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Location = new Point(8, 40),
                Size = new Size(Width - 16, 60),
                BackColor = Color.Transparent
            };
            Controls.Add(_lblInfo);

            _btnRefresh = new Button
            {
                Text = "🔄",
                Width = 36,
                Height = 36,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 12),
                Location = new Point(Width - 46, 8),
                Enabled = false,
                Cursor = Cursors.Default
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            Controls.Add(_btnRefresh);
        }
    }
}
