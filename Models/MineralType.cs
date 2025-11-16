using System.Drawing;

namespace wmine.Models
{
    /// <summary>
    /// énumération des types de minéraux exploités
    /// </summary>
    public enum MineralType
    {
        Améthyste,
        Andalousite,
        Antimoine,
        Apatite,
        Argent,
        Baryum,
        Bulles,
        CombustiblesMinéraux,
        Cuivre,
        Disthéne,
        Estérellite,
        Fer,
        Fluor,
        Grenats,
        Lithophyses,
        Orthose,
        Plomb,
        Septarias,
        Staurotite,
        Tourmaline,
        Uranifères,
        Zinc
    }

    /// <summary>
    /// Configuration des couleurs pour chaque type de minéral
    /// </summary>
    public static class MineralColors
    {
        private static readonly Dictionary<MineralType, Color> _colors = new()
        {
            { MineralType.Améthyste, Color.FromArgb(147, 112, 219) },      // Violet améthyste
            { MineralType.Andalousite, Color.FromArgb(205, 133, 63) },     // Brun rosé
            { MineralType.Antimoine, Color.FromArgb(112, 128, 144) },      // Gris ardoise
            { MineralType.Apatite, Color.FromArgb(32, 178, 170) },         // Turquoise
            { MineralType.Argent, Color.FromArgb(192, 192, 192) },         // Argent métallique
            { MineralType.Baryum, Color.FromArgb(245, 245, 220) },         // Beige clair
            { MineralType.Bulles, Color.FromArgb(135, 206, 250) },         // Bleu ciel
            { MineralType.CombustiblesMinéraux, Color.FromArgb(0, 0, 0) }, // Noir (charbon)
            { MineralType.Cuivre, Color.FromArgb(184, 115, 51) },          // Cuivre orangé
            { MineralType.Disthéne, Color.FromArgb(70, 130, 180) },        // Bleu acier
            { MineralType.Estérellite, Color.FromArgb(255, 182, 193) },    // Rose
            { MineralType.Fer, Color.FromArgb(178, 34, 34) },              // Rouge brique
            { MineralType.Fluor, Color.FromArgb(0, 255, 127) },            // Vert fluorescent
            { MineralType.Grenats, Color.FromArgb(153, 27, 27) },          // Rouge grenat
            { MineralType.Lithophyses, Color.FromArgb(210, 180, 140) },    // Brun clair
            { MineralType.Orthose, Color.FromArgb(255, 182, 193) },        // Rose péle
            { MineralType.Plomb, Color.FromArgb(96, 96, 96) },             // Gris plomb
            { MineralType.Septarias, Color.FromArgb(188, 143, 143) },      // Brun rosé
            { MineralType.Staurotite, Color.FromArgb(139, 69, 19) },       // Brun foncé
            { MineralType.Tourmaline, Color.FromArgb(34, 139, 34) },       // Vert forét
            { MineralType.Uranifères, Color.FromArgb(255, 255, 0) },       // Jaune (radioactif)
            { MineralType.Zinc, Color.FromArgb(138, 141, 143) }            // Gris zinc
        };

        public static Color GetColor(MineralType mineral) => _colors.ContainsKey(mineral) ? _colors[mineral] : Color.Gray;

        public static string GetDisplayName(MineralType mineral)
        {
            return mineral switch
            {
                MineralType.CombustiblesMinéraux => "Combustibles minéraux",
                MineralType.Améthyste => "Améthyste",
                MineralType.Disthéne => "Disthéne",
                MineralType.Estérellite => "Estérellite",
                MineralType.Uranifères => "Uranifères",
                _ => mineral.ToString()
            };
        }

        /// <summary>
        /// Retourne la couleur de texte optimale pour un fond donné (noir ou blanc)
        /// </summary>
        public static Color GetTextColor(Color backgroundColor)
        {
            // Calcul de la luminosité relative
            double luminance = (0.299 * backgroundColor.R + 
                               0.587 * backgroundColor.G + 
                               0.114 * backgroundColor.B) / 255;

            return luminance > 0.5 ? Color.Black : Color.White;
        }

        /// <summary>
        /// Retourne une version plus claire de la couleur pour les effets hover
        /// </summary>
        public static Color Lighten(Color color, float amount = 0.2f)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, (int)(color.R + (255 - color.R) * amount)),
                Math.Min(255, (int)(color.G + (255 - color.G) * amount)),
                Math.Min(255, (int)(color.B + (255 - color.B) * amount))
            );
        }

        /// <summary>
        /// Retourne une version plus foncée de la couleur
        /// </summary>
        public static Color Darken(Color color, float amount = 0.2f)
        {
            return Color.FromArgb(
                color.A,
                Math.Max(0, (int)(color.R * (1 - amount))),
                Math.Max(0, (int)(color.G * (1 - amount))),
                Math.Max(0, (int)(color.B * (1 - amount)))
            );
        }
    }
}
