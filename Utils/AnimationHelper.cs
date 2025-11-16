using System.Drawing.Drawing2D;

namespace wmine.Utils
{
    /// <summary>
    /// Helper pour les animations et effets visuels
    /// </summary>
    public static class AnimationHelper
    {
        /// <summary>
        /// Anime l'apparition d'un contréle (Form seulement, Form a Opacity)
        /// </summary>
        public static async Task FadeInAsync(Form form, int durationMs = 300)
        {
            if (form == null)
                return;

            form.Opacity = 0;
            form.Show();

            var steps = 20;
            var increment = 1.0 / steps;
            var delay = durationMs / steps;

            for (int i = 0; i <= steps; i++)
            {
                form.Opacity = i * increment;
                await Task.Delay(delay);
            }

            form.Opacity = 1.0;
        }

        /// <summary>
        /// Anime la disparition d'un formulaire
        /// </summary>
        public static async Task FadeOutAsync(Form form, int durationMs = 300)
        {
            if (form == null)
                return;

            var steps = 20;
            var decrement = 1.0 / steps;
            var delay = durationMs / steps;

            for (int i = steps; i >= 0; i--)
            {
                form.Opacity = i * decrement;
                await Task.Delay(delay);
            }

            form.Opacity = 0;
            form.Hide();
        }

        /// <summary>
        /// Anime le déplacement d'un contréle
        /// </summary>
        public static async Task SlideToAsync(Control control, Point targetLocation, int durationMs = 300)
        {
            if (control == null)
                return;

            var startLocation = control.Location;
            var steps = 20;
            var delay = durationMs / steps;

            var deltaX = (targetLocation.X - startLocation.X) / (double)steps;
            var deltaY = (targetLocation.Y - startLocation.Y) / (double)steps;

            for (int i = 0; i <= steps; i++)
            {
                control.Location = new Point(
                    startLocation.X + (int)(deltaX * i),
                    startLocation.Y + (int)(deltaY * i)
                );
                await Task.Delay(delay);
            }

            control.Location = targetLocation;
        }

        /// <summary>
        /// Anime le changement de taille d'un contréle
        /// </summary>
        public static async Task ResizeToAsync(Control control, Size targetSize, int durationMs = 300)
        {
            if (control == null)
                return;

            var startSize = control.Size;
            var steps = 20;
            var delay = durationMs / steps;

            var deltaWidth = (targetSize.Width - startSize.Width) / (double)steps;
            var deltaHeight = (targetSize.Height - startSize.Height) / (double)steps;

            for (int i = 0; i <= steps; i++)
            {
                control.Size = new Size(
                    startSize.Width + (int)(deltaWidth * i),
                    startSize.Height + (int)(deltaHeight * i)
                );
                await Task.Delay(delay);
            }

            control.Size = targetSize;
        }

        /// <summary>
        /// Crée un effet de pulsation sur un contréle
        /// </summary>
        public static async Task PulseAsync(Control control, int durationMs = 600, int pulses = 2)
        {
            if (control == null)
                return;

            var originalSize = control.Size;
            var scaleFactor = 1.1;

            for (int p = 0; p < pulses; p++)
            {
                // Agrandir
                await ResizeToAsync(control, new Size(
                    (int)(originalSize.Width * scaleFactor),
                    (int)(originalSize.Height * scaleFactor)
                ), durationMs / 2);

                // Rétrécir
                await ResizeToAsync(control, originalSize, durationMs / 2);
            }
        }

        /// <summary>
        /// Crée un effet de shake sur un contréle
        /// </summary>
        public static async Task ShakeAsync(Control control, int intensity = 10, int durationMs = 400)
        {
            if (control == null)
                return;

            var originalLocation = control.Location;
            var steps = 10;
            var delay = durationMs / steps;

            for (int i = 0; i < steps; i++)
            {
                var offsetX = (i % 2 == 0 ? intensity : -intensity) * (1 - i / (double)steps);
                control.Location = new Point(
                    originalLocation.X + (int)offsetX,
                    originalLocation.Y
                );
                await Task.Delay(delay);
            }

            control.Location = originalLocation;
        }

        /// <summary>
        /// Crée un dégradé linéaire avec plusieurs couleurs
        /// </summary>
        public static LinearGradientBrush CreateMultiColorGradient(Rectangle bounds, params Color[] colors)
        {
            if (colors.Length < 2)
                throw new ArgumentException("Au moins 2 couleurs sont requises", nameof(colors));

            var blend = new ColorBlend();
            blend.Colors = colors;
            blend.Positions = new float[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                blend.Positions[i] = i / (float)(colors.Length - 1);
            }

            var brush = new LinearGradientBrush(bounds, colors[0], colors[^1], LinearGradientMode.Vertical);
            brush.InterpolationColors = blend;

            return brush;
        }

        /// <summary>
        /// Crée un chemin arrondi pour un rectangle
        /// </summary>
        public static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
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

        /// <summary>
        /// Applique un effet de lueur autour d'un chemin
        /// </summary>
        public static void DrawGlow(Graphics g, GraphicsPath path, Color glowColor, int glowSize = 10)
        {
            for (int i = glowSize; i > 0; i--)
            {
                var alpha = (int)(255 * (1 - i / (double)glowSize) * 0.3);
                using (var pen = new Pen(Color.FromArgb(alpha, glowColor), i * 2))
                {
                    pen.LineJoin = LineJoin.Round;
                    g.DrawPath(pen, path);
                }
            }
        }

        /// <summary>
        /// Interpole entre deux couleurs
        /// </summary>
        public static Color InterpolateColors(Color color1, Color color2, double factor)
        {
            factor = Math.Clamp(factor, 0, 1);

            var r = (int)(color1.R + (color2.R - color1.R) * factor);
            var g = (int)(color1.G + (color2.G - color1.G) * factor);
            var b = (int)(color1.B + (color2.B - color1.B) * factor);
            var a = (int)(color1.A + (color2.A - color1.A) * factor);

            return Color.FromArgb(a, r, g, b);
        }
    }
}
