using System.Drawing.Drawing2D;

namespace wmine.UI
{
    /// <summary>
    /// Contréle flottant affichant la position actuelle sur la carte
    /// </summary>
    public class FloatingPositionIndicator : Panel
    {
        private Label _lblLatitude;
        private Label _lblLongitude;
        private Label _lblZoom;
        private double _latitude;
        private double _longitude;
        private double _zoom;

        public FloatingPositionIndicator()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Width = 200;
            this.Height = 90;
            this.BackColor = Color.FromArgb(220, 30, 35, 45); // Semi-transparent

            // Bordure arrondie avec effet glass
            this.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = GetRoundedRectangle(this.ClientRectangle, 10))
                {
                    // Fond avec dégradé
                    using (var brush = new LinearGradientBrush(
                        this.ClientRectangle,
                        Color.FromArgb(220, 35, 40, 50),
                        Color.FromArgb(220, 25, 30, 40),
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillPath(brush, path);
                    }

                    // Bordure brillante
                    using (var pen = new Pen(Color.FromArgb(200, 100, 200, 100), 2f))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }

                    // Reflet en haut
                    var glowBounds = new Rectangle(4, 4, this.Width - 8, this.Height / 3);
                    using (var glowPath = GetRoundedRectangle(glowBounds, 8))
                    using (var glowBrush = new LinearGradientBrush(
                        glowBounds,
                        Color.FromArgb(60, 255, 255, 255),
                        Color.FromArgb(0, 255, 255, 255),
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillPath(glowBrush, glowPath);
                    }
                }
            };

            // Titre
            var lblTitle = new Label
            {
                Text = "Position GPS",
                Location = new Point(10, 8),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 100),
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblTitle);

            // Latitude
            _lblLatitude = new Label
            {
                Location = new Point(10, 28),
                Width = 180,
                Font = new Font("Segoe UI Emoji", 9),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Text = "Lat: 0.000000é"
            };
            this.Controls.Add(_lblLatitude);

            // Longitude
            _lblLongitude = new Label
            {
                Location = new Point(10, 48),
                Width = 180,
                Font = new Font("Segoe UI Emoji", 9),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Text = "Lon: 0.000000é"
            };
            this.Controls.Add(_lblLongitude);

            // Zoom
            _lblZoom = new Label
            {
                Location = new Point(10, 68),
                Width = 180,
                Font = new Font("Segoe UI Emoji", 8),
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                Text = "Zoom: 10"
            };
            this.Controls.Add(_lblZoom);
        }

        public void UpdatePosition(double latitude, double longitude, double zoom)
        {
            _latitude = latitude;
            _longitude = longitude;
            _zoom = zoom;

            _lblLatitude.Text = $"Lat: {latitude:F6}é";
            _lblLongitude.Text = $"Lon: {longitude:F6}é";
            _lblZoom.Text = $"Zoom: {zoom:F1}";
        }

        private GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            if (diameter > bounds.Width) diameter = bounds.Width;
            if (diameter > bounds.Height) diameter = bounds.Height;

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}

