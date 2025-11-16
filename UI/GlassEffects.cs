using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace wmine.UI
{
    /// <summary>
    /// Contréles personnalisés avec effets glassmorphism (verre dépoli transparent)
    /// </summary>
    public static class GlassEffects
    {
        // Import Windows API pour effets de flou
        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        /// <summary>
        /// Active l'effet Acrylic/Blur sur un formulaire (Windows 10+)
        /// </summary>
        public static void EnableBlur(Form form, int alpha = 200)
        {
            if (Environment.OSVersion.Version.Major >= 10)
            {
                var accent = new AccentPolicy
                {
                    AccentState = 3, // ACCENT_ENABLE_BLURBEHIND
                    AccentFlags = 2,
                    GradientColor = (alpha << 24) | 0x1E1E28 // ARGB: transparent dark
                };

                var accentStructSize = Marshal.SizeOf(accent);
                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttributeData
                {
                    Attribute = 19, // WCA_ACCENT_POLICY
                    SizeOfData = accentStructSize,
                    Data = accentPtr
                };

                SetWindowCompositionAttribute(form.Handle, ref data);
                Marshal.FreeHGlobal(accentPtr);
            }
        }

        /// <summary>
        /// Dessine un panneau glassmorphism
        /// </summary>
        public static void DrawGlassPanel(Graphics g, Rectangle bounds, Color baseColor, int alpha = 180)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fond semi-transparent avec dégradé
            using (var path = GetRoundedRectangle(bounds, 15))
            {
                // dégradé vertical pour effet de profondeur
                using (var brush = new LinearGradientBrush(
                    bounds,
                    Color.FromArgb(alpha, baseColor),
                    Color.FromArgb(alpha - 30, baseColor),
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, path);
                }

                // Bordure avec effet brillant
                using (var pen = new Pen(Color.FromArgb(100, Color.White), 1.5f))
                {
                    g.DrawPath(pen, path);
                }

                // Reflet en haut (effet verre)
                var reflectBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height / 3);
                using (var reflectPath = GetRoundedRectangle(reflectBounds, 15))
                using (var reflectBrush = new LinearGradientBrush(
                    reflectBounds,
                    Color.FromArgb(40, Color.White),
                    Color.FromArgb(0, Color.White),
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(reflectBrush, reflectPath);
                }
            }
        }

        /// <summary>
        /// Dessine un bouton avec effet 3D glassmorphism
        /// </summary>
        public static void DrawGlassButton(Graphics g, Rectangle bounds, Color baseColor, bool isHovered, bool isPressed)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Ajuster pour l'effet de pression
            if (isPressed)
            {
                bounds.Inflate(-2, -2);
            }

            using (var path = GetRoundedRectangle(bounds, 10))
            {
                // Ombre portée 3D
                if (!isPressed)
                {
                    var shadowBounds = bounds;
                    shadowBounds.Offset(0, 3);
                    using (var shadowPath = GetRoundedRectangle(shadowBounds, 10))
                    using (var shadowBrush = new SolidBrush(Color.FromArgb(60, Color.Black)))
                    {
                        g.FillPath(shadowBrush, shadowPath);
                    }
                }

                // Fond du bouton avec dégradé
                var alpha = isHovered ? 220 : 180;
                using (var brush = new LinearGradientBrush(
                    bounds,
                    Color.FromArgb(alpha, baseColor),
                    Color.FromArgb(alpha - 40, baseColor),
                    isPressed ? 270f : 90f))
                {
                    g.FillPath(brush, path);
                }

                // Bordure brillante
                using (var pen = new Pen(isHovered ? Color.FromArgb(150, Color.White) : Color.FromArgb(80, Color.White), 2f))
                {
                    g.DrawPath(pen, path);
                }

                // Reflet brillant en haut
                if (!isPressed)
                {
                    var glowBounds = new Rectangle(bounds.X + 5, bounds.Y + 5, bounds.Width - 10, bounds.Height / 2);
                    using (var glowPath = GetRoundedRectangle(glowBounds, 8))
                    using (var glowBrush = new LinearGradientBrush(
                        glowBounds,
                        Color.FromArgb(60, Color.White),
                        Color.FromArgb(0, Color.White),
                        LinearGradientMode.Vertical))
                    {
                        g.FillPath(glowBrush, glowPath);
                    }
                }
            }
        }

        /// <summary>
        /// Crée un rectangle aux coins arrondis
        /// </summary>
        private static GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
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

        /// <summary>
        /// Applique une animation de fondu
        /// </summary>
        public static async Task FadeIn(Control control, int duration = 300)
        {
            control.Visible = true;
            for (double i = 0; i <= 1; i += 0.05)
            {
                if (control.IsDisposed) return;
                control.Invoke(() =>
                {
                    var form = control as Form;
                    if (form != null)
                        form.Opacity = i;
                });
                await Task.Delay(duration / 20);
            }
        }

        /// <summary>
        /// Applique une animation de slide (glissement)
        /// </summary>
        public static async Task SlideIn(Control control, int offsetX, int offsetY, int duration = 300)
        {
            var originalLocation = control.Location;
            control.Location = new Point(originalLocation.X + offsetX, originalLocation.Y + offsetY);
            control.Visible = true;

            int steps = 20;
            int stepX = offsetX / steps;
            int stepY = offsetY / steps;

            for (int i = 0; i < steps; i++)
            {
                if (control.IsDisposed) return;
                control.Invoke(() =>
                {
                    control.Location = new Point(control.Location.X - stepX, control.Location.Y - stepY);
                });
                await Task.Delay(duration / steps);
            }

            control.Location = originalLocation;
        }
    }

    /// <summary>
    /// Panel personnalisé avec effet glassmorphism
    /// </summary>
    public class GlassPanel : Panel
    {
        public Color GlassColor { get; set; } = Color.FromArgb(30, 30, 40);
        public int GlassAlpha { get; set; } = 180;

        public GlassPanel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            GlassEffects.DrawGlassPanel(e.Graphics, ClientRectangle, GlassColor, GlassAlpha);
            base.OnPaint(e);
        }
    }

    /// <summary>
    /// Bouton personnalisé avec effet glassmorphism 3D
    /// </summary>
    public class GlassButton : Button
    {
        public Color GlassColor { get; set; } = Color.FromArgb(0, 120, 215);
        private bool _isHovered = false;
        private bool _isPressed = false;

        public GlassButton()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
            BackColor = Color.Transparent;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            ForeColor = Color.White;
            Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold);
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

            GlassEffects.DrawGlassButton(e.Graphics, ClientRectangle, GlassColor, _isHovered, _isPressed);

            // Dessiner le texte
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using (var brush = new SolidBrush(ForeColor))
            {
                var textBounds = _isPressed ? 
                    new Rectangle(0, 2, Width, Height) : 
                    ClientRectangle;
                e.Graphics.DrawString(Text, Font, brush, textBounds, sf);
            }
        }
    }
}

