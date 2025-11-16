using System.Drawing.Drawing2D;

namespace wmine.UI
{
    /// <summary>
    /// Bouton avec effet de transparence/glass moderne
    /// Compatible avec tous les systémes Windows
    /// </summary>
    public class TransparentGlassButton : Button
    {
        private Color _baseColor = Color.FromArgb(0, 120, 215);
        private bool _isHovered = false;
        private bool _isPressed = false;
        private int _transparency = 220; // Augmenté de 180 é 220 (~86% opaque au lieu de ~70%)

        public Color BaseColor
        {
            get => _baseColor;
            set
            {
                _baseColor = value;
                Invalidate();
            }
        }

        public int Transparency
        {
            get => _transparency;
            set
            {
                _transparency = Math.Clamp(value, 100, 255);
                Invalidate();
            }
        }

        public TransparentGlassButton()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
            
            BackColor = Color.Transparent;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            ForeColor = Color.White;
            Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold);
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _isPressed = true;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _isPressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Calculer la transparence selon l'état
            int alpha = _transparency;
            if (_isHovered) alpha = Math.Min(255, alpha + 40);
            if (_isPressed) alpha = Math.Max(100, alpha - 30);

            var bounds = new Rectangle(0, 0, Width, Height);

            // Dessiner l'ombre portée
            if (!_isPressed)
            {
                var shadowBounds = bounds;
                shadowBounds.Offset(0, 2);
                shadowBounds.Inflate(-1, -1);
                using (var shadowPath = GetRoundedRectangle(shadowBounds, 8))
                using (var shadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                {
                    e.Graphics.FillPath(shadowBrush, shadowPath);
                }
            }

            // Ajuster bounds si pressé
            if (_isPressed)
            {
                bounds.Offset(0, 1);
            }

            // Fond avec dégradé vertical et transparence
            using (var path = GetRoundedRectangle(bounds, 8))
            {
                // dégradé de fond
                var colorTop = Color.FromArgb(alpha, _baseColor);
                var colorBottom = Color.FromArgb(alpha, 
                    Math.Max(0, _baseColor.R - 30),
                    Math.Max(0, _baseColor.G - 30),
                    Math.Max(0, _baseColor.B - 30));

                using (var brush = new LinearGradientBrush(bounds, colorTop, colorBottom, 90f))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Bordure légérement plus claire
                var borderAlpha = Math.Min(255, alpha + 40);
                using (var pen = new Pen(Color.FromArgb(borderAlpha, 
                    Math.Min(255, _baseColor.R + 40),
                    Math.Min(255, _baseColor.G + 40),
                    Math.Min(255, _baseColor.B + 40)), 2f))
                {
                    e.Graphics.DrawPath(pen, path);
                }

                // Reflet brillant en haut (effet glass)
                if (!_isPressed)
                {
                    var glowBounds = new Rectangle(bounds.X + 4, bounds.Y + 4, bounds.Width - 8, bounds.Height / 2);
                    using (var glowPath = GetRoundedRectangle(glowBounds, 6))
                    using (var glowBrush = new LinearGradientBrush(
                        glowBounds,
                        Color.FromArgb(80, 255, 255, 255),
                        Color.FromArgb(0, 255, 255, 255),
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillPath(glowBrush, glowPath);
                    }
                }
            }

            // Dessiner le texte
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using (var textBrush = new SolidBrush(ForeColor))
            {
                e.Graphics.DrawString(Text, Font, textBrush, bounds, sf);
            }
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

