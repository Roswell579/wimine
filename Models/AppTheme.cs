namespace wmine.Models
{
    /// <summary>
    /// Types de thémes disponibles
    /// </summary>
    public enum ThemeType
    {
        Dark,       // Théme sombre (par défaut)
        Light,      // Théme clair
        Blue,       // Théme bleu
        Green,      // Théme vert
        Mineral     // Théme couleurs minérales
    }

    /// <summary>
    /// définition d'un théme d'apparence
    /// </summary>
    public class AppTheme
    {
        public ThemeType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;

        // Couleurs de fond
        public Color BackgroundPrimary { get; set; }
        public Color BackgroundSecondary { get; set; }
        public Color BackgroundTertiary { get; set; }

        // Couleurs de texte
        public Color TextPrimary { get; set; }
        public Color TextSecondary { get; set; }

        // Couleur d'accent
        public Color AccentColor { get; set; }

        // Couleurs des boutons
        public Color ButtonSuccess { get; set; }
        public Color ButtonDanger { get; set; }
        public Color ButtonInfo { get; set; }
        public Color ButtonWarning { get; set; }

        /// <summary>
        /// Obtient un théme prédéfini
        /// </summary>
        public static AppTheme GetTheme(ThemeType type)
        {
            return type switch
            {
                ThemeType.Dark => CreateDarkTheme(),
                ThemeType.Light => CreateLightTheme(),
                ThemeType.Blue => CreateBlueTheme(),
                ThemeType.Green => CreateGreenTheme(),
                ThemeType.Mineral => CreateMineralTheme(),
                _ => CreateDarkTheme()
            };
        }

        private static AppTheme CreateDarkTheme()
        {
            return new AppTheme
            {
                Type = ThemeType.Dark,
                Name = "Dark (Sombre)",
                Icon = "??",
                BackgroundPrimary = Color.FromArgb(25, 25, 35),
                BackgroundSecondary = Color.FromArgb(30, 35, 45),
                BackgroundTertiary = Color.FromArgb(40, 45, 55),
                TextPrimary = Color.White,
                TextSecondary = Color.FromArgb(180, 180, 180),
                AccentColor = Color.FromArgb(0, 150, 136),
                ButtonSuccess = Color.FromArgb(0, 150, 136),
                ButtonDanger = Color.FromArgb(244, 67, 54),
                ButtonInfo = Color.FromArgb(33, 150, 243),
                ButtonWarning = Color.FromArgb(255, 152, 0)
            };
        }

        private static AppTheme CreateLightTheme()
        {
            return new AppTheme
            {
                Type = ThemeType.Light,
                Name = "Light (Clair)",
                Icon = "??",
                BackgroundPrimary = Color.FromArgb(245, 245, 245),
                BackgroundSecondary = Color.White,
                BackgroundTertiary = Color.FromArgb(235, 235, 235),
                TextPrimary = Color.FromArgb(33, 33, 33),
                TextSecondary = Color.FromArgb(100, 100, 100),
                AccentColor = Color.FromArgb(0, 150, 136),
                ButtonSuccess = Color.FromArgb(76, 175, 80),
                ButtonDanger = Color.FromArgb(244, 67, 54),
                ButtonInfo = Color.FromArgb(33, 150, 243),
                ButtonWarning = Color.FromArgb(255, 152, 0)
            };
        }

        private static AppTheme CreateBlueTheme()
        {
            return new AppTheme
            {
                Type = ThemeType.Blue,
                Name = "Blue (Bleu)",
                Icon = "??",
                BackgroundPrimary = Color.FromArgb(13, 27, 42),
                BackgroundSecondary = Color.FromArgb(27, 38, 54),
                BackgroundTertiary = Color.FromArgb(37, 56, 88),
                TextPrimary = Color.FromArgb(236, 239, 244),
                TextSecondary = Color.FromArgb(136, 146, 176),
                AccentColor = Color.FromArgb(82, 171, 250),
                ButtonSuccess = Color.FromArgb(52, 152, 219),
                ButtonDanger = Color.FromArgb(231, 76, 60),
                ButtonInfo = Color.FromArgb(52, 152, 219),
                ButtonWarning = Color.FromArgb(241, 196, 15)
            };
        }

        private static AppTheme CreateGreenTheme()
        {
            return new AppTheme
            {
                Type = ThemeType.Green,
                Name = "Green (Vert)",
                Icon = "??",
                BackgroundPrimary = Color.FromArgb(18, 31, 23),
                BackgroundSecondary = Color.FromArgb(27, 43, 33),
                BackgroundTertiary = Color.FromArgb(39, 60, 47),
                TextPrimary = Color.FromArgb(232, 245, 233),
                TextSecondary = Color.FromArgb(165, 214, 167),
                AccentColor = Color.FromArgb(76, 175, 80),
                ButtonSuccess = Color.FromArgb(76, 175, 80),
                ButtonDanger = Color.FromArgb(229, 57, 53),
                ButtonInfo = Color.FromArgb(41, 182, 246),
                ButtonWarning = Color.FromArgb(255, 167, 38)
            };
        }

        private static AppTheme CreateMineralTheme()
        {
            return new AppTheme
            {
                Type = ThemeType.Mineral,
                Name = "Mineral (Minéral)",
                Icon = "??",
                BackgroundPrimary = Color.FromArgb(30, 25, 20),
                BackgroundSecondary = Color.FromArgb(45, 38, 30),
                BackgroundTertiary = Color.FromArgb(60, 50, 40),
                TextPrimary = Color.FromArgb(255, 248, 220),
                TextSecondary = Color.FromArgb(210, 180, 140),
                AccentColor = Color.FromArgb(255, 193, 7),  // Or
                ButtonSuccess = Color.FromArgb(139, 195, 74),  // Vert minéral
                ButtonDanger = Color.FromArgb(255, 87, 34),    // Orange/Cuivre
                ButtonInfo = Color.FromArgb(156, 204, 101),    // Jade
                ButtonWarning = Color.FromArgb(255, 193, 7)    // Or
            };
        }

        /// <summary>
        /// Obtient la description du théme
        /// </summary>
        public string GetDescription()
        {
            return Type switch
            {
                ThemeType.Dark => "Théme sombre élégant avec fond noir",
                ThemeType.Light => "Théme clair et lumineux",
                ThemeType.Blue => "Théme bleu océan apaisant",
                ThemeType.Green => "Théme vert nature relaxant",
                ThemeType.Mineral => "Théme inspiré des couleurs minérales",
                _ => "Théme par défaut"
            };
        }
    }
}
