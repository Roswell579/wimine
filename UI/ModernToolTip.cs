using System.Drawing.Drawing2D;

namespace wmine.UI
{
    /// <summary>
    /// Tooltip moderne avec design glassmorphism
    /// </summary>
    public class ModernToolTip : ToolTip
    {
        private Color _backgroundColor = Color.FromArgb(240, 30, 35, 45);
        private Color _borderColor = Color.FromArgb(200, 100, 150, 255);
        private Color _textColor = Color.White;
        private bool _isDanger = false;

        public bool IsDanger
        {
            get => _isDanger;
            set
            {
                _isDanger = value;
                if (value)
                {
                    _backgroundColor = Color.FromArgb(240, 80, 20, 20);
                    _borderColor = Color.FromArgb(255, 255, 50, 50);
                    _textColor = Color.FromArgb(255, 255, 220, 220);
                }
            }
        }

        public ModernToolTip()
        {
            OwnerDraw = true;
            IsBalloon = false;
            BackColor = Color.FromArgb(30, 35, 45);
            ForeColor = Color.White;
            
            Draw += OnDraw;
            Popup += OnPopup;
        }

        private void OnPopup(object? sender, PopupEventArgs e)
        {
            // Calculer la taille nécessaire
            using (var g = e.AssociatedControl?.CreateGraphics())
            {
                if (g != null)
                {
                    var text = GetToolTip(e.AssociatedControl);
                    var font = new Font("Segoe UI Emoji", 10f, FontStyle.Bold);
                    var size = g.MeasureString(text, font, 400);
                    
                    e.ToolTipSize = new Size(
                        (int)size.Width + 30,
                        (int)size.Height + 25
                    );
                }
            }
        }

        private void OnDraw(object? sender, DrawToolTipEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var bounds = e.Bounds;
            
            // Fond avec coins arrondis et transparence
            using (var path = GetRoundedRectangle(bounds, 12))
            {
                // dégradé de fond
                using (var brush = new LinearGradientBrush(
                    bounds, 
                    _backgroundColor,
                    Color.FromArgb(_backgroundColor.A - 40, _backgroundColor),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Bordure brillante
                using (var pen = new Pen(_borderColor, 2f))
                {
                    e.Graphics.DrawPath(pen, path);
                }

                // Reflet en haut (effet glass)
                var glowBounds = new Rectangle(
                    bounds.X + 4, 
                    bounds.Y + 4, 
                    bounds.Width - 8, 
                    bounds.Height / 3
                );
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

            // Icéne téte de mort si danger
            if (_isDanger)
            {
                var skullBounds = new Rectangle(bounds.X + 10, bounds.Y + 10, 24, 24);
                DrawSkullIcon(e.Graphics, skullBounds);
            }

            // Texte
            var textBounds = new Rectangle(
                bounds.X + (_isDanger ? 45 : 15),
                bounds.Y + 12,
                bounds.Width - (_isDanger ? 60 : 30),
                bounds.Height - 24
            );

            var sf = new StringFormat
            {
                LineAlignment = StringAlignment.Near,
                Alignment = StringAlignment.Near
            };

            using (var textBrush = new SolidBrush(_textColor))
            using (var font = new Font("Segoe UI Emoji", 10f, FontStyle.Bold))
            {
                e.Graphics.DrawString(e.ToolTipText, font, textBrush, textBounds, sf);
            }
        }

        private void DrawSkullIcon(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Créne (cercle principal)
            using (var brush = new SolidBrush(Color.FromArgb(255, 255, 80, 80)))
            using (var pen = new Pen(Color.FromArgb(200, 0, 0), 2f))
            {
                var headRect = new Rectangle(bounds.X + 2, bounds.Y, 20, 16);
                g.FillEllipse(brush, headRect);
                g.DrawEllipse(pen, headRect);

                // Yeux (X)
                using (var eyePen = new Pen(Color.Black, 2f))
                {
                    // Oeil gauche
                    g.DrawLine(eyePen, bounds.X + 6, bounds.Y + 4, bounds.X + 10, bounds.Y + 8);
                    g.DrawLine(eyePen, bounds.X + 10, bounds.Y + 4, bounds.X + 6, bounds.Y + 8);
                    // Oeil droit
                    g.DrawLine(eyePen, bounds.X + 14, bounds.Y + 4, bounds.X + 18, bounds.Y + 8);
                    g.DrawLine(eyePen, bounds.X + 18, bounds.Y + 4, bounds.X + 14, bounds.Y + 8);
                }

                // Dents (rectangles)
                using (var toothBrush = new SolidBrush(Color.White))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        var toothRect = new Rectangle(bounds.X + 4 + (i * 3), bounds.Y + 13, 2, 4);
                        g.FillRectangle(toothBrush, toothRect);
                    }
                }

                // Os croisés en dessous
                using (var bonePen = new Pen(Color.White, 2f))
                {
                    g.DrawLine(bonePen, bounds.X + 2, bounds.Y + 20, bounds.X + 22, bounds.Y + 18);
                    g.DrawLine(bonePen, bounds.X + 22, bounds.Y + 20, bounds.X + 2, bounds.Y + 18);
                }
            }
        }

        private GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}

