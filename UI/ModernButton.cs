using System.Drawing;
using System.Windows.Forms;

namespace wmine.UI
{
    /// <summary>
    /// Factory pour créer des boutons plats modernes uniformes
    /// </summary>
    public static class ModernButton
    {
        /// <summary>
        /// Crée un bouton plat moderne avec les couleurs standard WMine
        /// </summary>
        public static Button Create(string text, Color backgroundColor, int width = 130, int height = 50)
        {
            var button = new Button
            {
                Text = text,
                Width = width,
                Height = height,
                FlatStyle = FlatStyle.Flat,
                BackColor = backgroundColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = LightenColor(backgroundColor, 20);
            button.FlatAppearance.MouseDownBackColor = DarkenColor(backgroundColor, 20);

            return button;
        }

        /// <summary>
        /// Couleurs standards WMine
        /// </summary>
        public static class Colors
        {
            public static Color Primary => Color.FromArgb(0, 150, 136);      // Vert Teal
            public static Color Secondary => Color.FromArgb(33, 150, 243);    // Bleu
            public static Color Danger => Color.FromArgb(244, 67, 54);        // Rouge
            public static Color Warning => Color.FromArgb(255, 152, 0);       // Orange
            public static Color Success => Color.FromArgb(76, 175, 80);       // Vert clair
            public static Color Info => Color.FromArgb(100, 181, 246);        // Bleu clair
            public static Color Purple => Color.FromArgb(156, 39, 176);       // Violet
            public static Color Dark => Color.FromArgb(60, 60, 60);           // Gris foncé
        }

        private static Color LightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, color.R + amount),
                Math.Min(255, color.G + amount),
                Math.Min(255, color.B + amount)
            );
        }

        private static Color DarkenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Max(0, color.R - amount),
                Math.Max(0, color.G - amount),
                Math.Max(0, color.B - amount)
            );
        }
    }
}

