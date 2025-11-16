using System.Drawing.Drawing2D;

namespace wmine.UI
{
    /// <summary>
    /// Contréle flottant affichant l'échelle de la carte
    /// </summary>
    public class FloatingScaleBar : Panel
    {
        private double _zoom;
        private double _scaleMeters;
        private string _scaleText = "1 km";

        public FloatingScaleBar()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Width = 200;
            this.Height = 60;
            this.BackColor = Color.FromArgb(220, 30, 35, 45); // Semi-transparent

            // Bordure arrondie avec effet glass
            this.Paint += OnPaint;
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Fond arrondi
            using (var path = GetRoundedRectangle(this.ClientRectangle, 10))
            {
                // dégradé de fond
                using (var brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(220, 35, 40, 50),
                    Color.FromArgb(220, 25, 30, 40),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Bordure brillante
                using (var pen = new Pen(Color.FromArgb(200, 150, 150, 200), 2f))
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

            // Titre
            using (var font = new Font("Segoe UI Emoji", 8, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(150, 150, 200)))
            {
                e.Graphics.DrawString("Echelle", font, brush, 10, 8);
            }

            // Barre d'échelle
            DrawScaleBar(e.Graphics);
        }

        private void DrawScaleBar(Graphics g)
        {
            int barWidth = 150;
            int barHeight = 8;
            int barX = 25;
            int barY = 35;

            // Fond de la barre
            using (var brush = new SolidBrush(Color.White))
            {
                g.FillRectangle(brush, barX, barY, barWidth, barHeight);
            }

            // Segments alternés (noir/blanc)
            int segmentWidth = barWidth / 5;
            for (int i = 0; i < 5; i++)
            {
                if (i % 2 == 0)
                {
                    using (var brush = new SolidBrush(Color.Black))
                    {
                        g.FillRectangle(brush, barX + i * segmentWidth, barY, segmentWidth, barHeight);
                    }
                }
            }

            // Bordure de la barre
            using (var pen = new Pen(Color.Black, 1.5f))
            {
                g.DrawRectangle(pen, barX, barY, barWidth, barHeight);
            }

            // Texte de l'échelle
            using (var font = new Font("Segoe UI Emoji", 8, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Far };
                g.DrawString(_scaleText, font, brush, barX + barWidth, barY + barHeight + 2, sf);
            }
        }

        public void UpdateScale(double zoom, double latitude)
        {
            _zoom = zoom;

            // Calculer la distance représentée par 150 pixels
            // Formule approximative: 156543.03392 * Math.Cos(lat * PI/180) / Math.Pow(2, zoom)
            double metersPerPixel = 156543.03392 * Math.Cos(latitude * Math.PI / 180) / Math.Pow(2, zoom);
            _scaleMeters = metersPerPixel * 150;

            // Arrondir é des valeurs lisibles
            if (_scaleMeters >= 1000)
            {
                double km = _scaleMeters / 1000;
                if (km >= 100) _scaleText = $"{Math.Round(km / 100) * 100} km";
                else if (km >= 10) _scaleText = $"{Math.Round(km / 10) * 10} km";
                else if (km >= 1) _scaleText = $"{Math.Round(km)} km";
                else _scaleText = $"{Math.Round(km, 1)} km";
            }
            else
            {
                if (_scaleMeters >= 100) _scaleText = $"{Math.Round(_scaleMeters / 100) * 100} m";
                else if (_scaleMeters >= 10) _scaleText = $"{Math.Round(_scaleMeters / 10) * 10} m";
                else _scaleText = $"{Math.Round(_scaleMeters)} m";
            }

            this.Invalidate();
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


