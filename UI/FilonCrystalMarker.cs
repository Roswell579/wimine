using System.Drawing.Drawing2D;
using GMap.NET.WindowsForms;
using wmine.Models;

namespace wmine.UI
{
    /// <summary>
    /// Marqueur en forme de cristal hexagonal pour les filons miniers
    /// Style moderne et minéraliste avec effets de brillance
    /// </summary>
    public class FilonCrystalMarker : GMapMarker
    {
        private readonly Filon _filon;
        private readonly Color _mineralColor;
        private bool _isHovered;
        private float _glowIntensity = 0f;
        private System.Windows.Forms.Timer? _animationTimer;

        // Limites de coordonnées pour éviter OutOfMemoryException
        private const int MAX_COORD = 32000;
        private const int MIN_COORD = -32000;

        public FilonCrystalMarker(GMap.NET.PointLatLng position, Filon filon, Color mineralColor)
            : base(position)
        {
            _filon = filon;
            _mineralColor = mineralColor;
            Size = new Size(40, 56); // Plus grand pour le cristal
            Offset = new Point(-20, -56);

            // Timer pour animation de brillance
            _animationTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (_isHovered)
            {
                _glowIntensity += 0.1f;
                if (_glowIntensity > 1f) _glowIntensity = 0f;
            }
            else
            {
                _glowIntensity *= 0.9f; // Fade out progressif
            }
        }

        public override void OnRender(Graphics g)
        {
            var centerX = LocalPosition.X;
            var centerY = LocalPosition.Y;

            // Validation: éviter de dessiner si les coordonnées sont hors limites
            // Cela prévient l'OutOfMemoryException dans LinearGradientBrush et autres ressources GDI+
            if (centerX < MIN_COORD || centerX > MAX_COORD || 
                centerY < MIN_COORD || centerY > MAX_COORD)
            {
                return;
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Ombre portée (ellipse floue)
            DrawShadow(g, centerX, centerY + 50);

            // 2. Base du cristal (hexagone inférieur)
            DrawCrystalBase(g, centerX, centerY + 40);

            // 3. Corps principal (hexagone vertical)
            DrawCrystalBody(g, centerX, centerY + 15, centerY + 40);

            // 4. Pointe supérieure (triangle)
            DrawCrystalTip(g, centerX, centerY, centerY + 15);

            // 5. Facettes et reflets
            DrawFacets(g, centerX, centerY);

            // 6. Bordure brillante
            DrawBorder(g, centerX, centerY);

            // 7. Effet de brillance/glow si survolé
            if (_isHovered || _glowIntensity > 0.1f)
            {
                DrawGlow(g, centerX, centerY);
            }

            // 8. Icéne du minéral (optionnel - petit symbole au centre)
            DrawMineralIcon(g, centerX, centerY + 27);
        }

        /// <summary>
        /// Dessine l'ombre portée sous le cristal
        /// </summary>
        private void DrawShadow(Graphics g, int x, int y)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(x - 15, y - 6, 30, 12);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(120, Color.Black);
                    brush.SurroundColors = new[] { Color.FromArgb(0, Color.Black) };
                    g.FillPath(brush, path);
                }
            }
        }

        /// <summary>
        /// Dessine la base hexagonale du cristal
        /// </summary>
        private void DrawCrystalBase(Graphics g, int x, int y)
        {
            var hexagon = GetHexagonPoints(x, y, 12);

            // Validation des points avant de créer le brush
            if (!ArePointsValid(hexagon))
                return;

            // Remplissage avec dégradé foncé
            using (var brush = new LinearGradientBrush(
                new Point(x, y - 12),
                new Point(x, y + 12),
                GetDarkerColor(_mineralColor, 0.4f),
                GetDarkerColor(_mineralColor, 0.6f)))
            {
                g.FillPolygon(brush, hexagon);
            }
        }

        /// <summary>
        /// Dessine le corps principal hexagonal
        /// </summary>
        private void DrawCrystalBody(Graphics g, int x, int yTop, int yBottom)
        {
            // Créer un polygone 3D (hexagone étiré verticalement)
            var points = new Point[]
            {
                // Haut
                new Point(x - 14, yTop),
                new Point(x - 7, yTop - 4),
                new Point(x + 7, yTop - 4),
                new Point(x + 14, yTop),
                new Point(x + 7, yTop + 4),
                new Point(x - 7, yTop + 4),
                // Milieu (connexion)
                new Point(x - 14, (yTop + yBottom) / 2),
                new Point(x + 14, (yTop + yBottom) / 2),
                // Bas
                new Point(x + 14, yBottom),
                new Point(x + 7, yBottom + 4),
                new Point(x - 7, yBottom + 4),
                new Point(x - 14, yBottom)
            };

            // Utiliser un path pour créer les faces
            using (var bodyPath = new GraphicsPath())
            {
                // Face avant
                bodyPath.AddPolygon(new Point[]
                {
                    new Point(x - 14, yTop),
                    new Point(x + 14, yTop),
                    new Point(x + 14, yBottom),
                    new Point(x - 14, yBottom)
                });

                // dégradé du minéral (clair en haut, foncé en bas)
                using (var brush = new LinearGradientBrush(
                    new Point(x, yTop),
                    new Point(x, yBottom),
                    GetLighterColor(_mineralColor, 0.3f),
                    GetDarkerColor(_mineralColor, 0.2f)))
                {
                    g.FillPath(brush, bodyPath);
                }
            }

            // Face latérale gauche (plus sombre)
            using (var leftPath = new GraphicsPath())
            {
                leftPath.AddPolygon(new Point[]
                {
                    new Point(x - 14, yTop),
                    new Point(x - 7, yTop - 4),
                    new Point(x - 7, yBottom + 4),
                    new Point(x - 14, yBottom)
                });

                using (var brush = new SolidBrush(GetDarkerColor(_mineralColor, 0.3f)))
                {
                    g.FillPath(brush, leftPath);
                }
            }

            // Face latérale droite (plus claire)
            using (var rightPath = new GraphicsPath())
            {
                rightPath.AddPolygon(new Point[]
                {
                    new Point(x + 14, yTop),
                    new Point(x + 7, yTop - 4),
                    new Point(x + 7, yBottom + 4),
                    new Point(x + 14, yBottom)
                });

                using (var brush = new SolidBrush(GetLighterColor(_mineralColor, 0.1f)))
                {
                    g.FillPath(brush, rightPath);
                }
            }
        }

        /// <summary>
        /// Dessine la pointe supérieure du cristal
        /// </summary>
        private void DrawCrystalTip(Graphics g, int x, int yTip, int yBase)
        {
            // Triangle principal
            var tipPoints = new Point[]
            {
                new Point(x, yTip),           // Sommet
                new Point(x - 14, yBase),     // Base gauche
                new Point(x + 14, yBase)      // Base droite
            };

            // dégradé de la pointe (trés clair au sommet)
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(tipPoints);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterPoint = new PointF(x, yTip + 5);
                    brush.CenterColor = GetLighterColor(_mineralColor, 0.6f);
                    brush.SurroundColors = new[] { _mineralColor };
                    g.FillPath(brush, path);
                }
            }

            // Face gauche de la pointe (plus sombre)
            var leftTip = new Point[]
            {
                new Point(x, yTip),
                new Point(x - 7, yBase - 4),
                new Point(x - 14, yBase)
            };
            using (var brush = new SolidBrush(GetDarkerColor(_mineralColor, 0.2f)))
            {
                g.FillPolygon(brush, leftTip);
            }

            // Face droite de la pointe (plus claire)
            var rightTip = new Point[]
            {
                new Point(x, yTip),
                new Point(x + 7, yBase - 4),
                new Point(x + 14, yBase)
            };
            using (var brush = new SolidBrush(GetLighterColor(_mineralColor, 0.2f)))
            {
                g.FillPolygon(brush, rightTip);
            }
        }

        /// <summary>
        /// Dessine les reflets sur les facettes
        /// </summary>
        private void DrawFacets(Graphics g, int x, int y)
        {
            // Reflet principal (haut du cristal)
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(x - 6, y + 8, 12, 8);
                
                // Protection contre les points identiques qui causent OutOfMemoryException
                var point1 = new Point(x, y + 8);
                var point2 = new Point(x, y + 16);
                
                // S'assurer que les points sont différents
                if (point1.Y == point2.Y)
                {
                    point2.Y += 1; // Ajuster légérement si identiques
                }
                
                using (var brush = new LinearGradientBrush(
                    point1,
                    point2,
                    Color.FromArgb(120, Color.White),
                    Color.FromArgb(0, Color.White)))
                {
                    g.FillPath(brush, path);
                }
            }

            // Petits reflets sur les facettes
            using (var pen = new Pen(Color.FromArgb(80, Color.White), 1.5f))
            {
                // Facette gauche
                g.DrawLine(pen, x - 10, y + 18, x - 8, y + 30);
                // Facette droite
                g.DrawLine(pen, x + 10, y + 18, x + 8, y + 30);
            }
        }

        /// <summary>
        /// Dessine la bordure brillante du cristal
        /// </summary>
        private void DrawBorder(Graphics g, int x, int y)
        {
            using (var pen = new Pen(GetLighterColor(_mineralColor, 0.4f), 1.5f))
            {
                // Contour de la pointe
                g.DrawLine(pen, x, y, x - 14, y + 15);
                g.DrawLine(pen, x, y, x + 14, y + 15);

                // Contour du corps
                g.DrawLine(pen, x - 14, y + 15, x - 14, y + 40);
                g.DrawLine(pen, x + 14, y + 15, x + 14, y + 40);
            }
        }

        /// <summary>
        /// Dessine l'effet de brillance animé
        /// </summary>
        private void DrawGlow(Graphics g, int x, int y)
        {
            int glowSize = (int)(30 + _glowIntensity * 10);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(x - glowSize / 2, y + 20 - glowSize / 2, glowSize, glowSize);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb((int)(100 * _glowIntensity), _mineralColor);
                    brush.SurroundColors = new[] { Color.FromArgb(0, _mineralColor) };
                    g.FillPath(brush, path);
                }
            }
        }

        /// <summary>
        /// Dessine une petite icéne du minéral au centre
        /// </summary>
        private void DrawMineralIcon(Graphics g, int x, int y)
        {
            // Petit losange pour représenter le minéral
            var icon = new Point[]
            {
                new Point(x, y - 4),
                new Point(x + 4, y),
                new Point(x, y + 4),
                new Point(x - 4, y)
            };

            using (var brush = new SolidBrush(GetLighterColor(_mineralColor, 0.5f)))
            {
                g.FillPolygon(brush, icon);
            }

            using (var pen = new Pen(Color.White, 1f))
            {
                g.DrawPolygon(pen, icon);
            }
        }

        /// <summary>
        /// Génére les points d'un hexagone
        /// </summary>
        private Point[] GetHexagonPoints(int centerX, int centerY, int radius)
        {
            var points = new Point[6];
            for (int i = 0; i < 6; i++)
            {
                double angle = Math.PI / 3 * i - Math.PI / 6; // -30é offset pour orientation
                points[i] = new Point(
                    centerX + (int)(radius * Math.Cos(angle)),
                    centerY + (int)(radius * Math.Sin(angle))
                );
            }
            return points;
        }

        /// <summary>
        /// Vérifie si les points sont dans des limites valides
        /// </summary>
        private bool ArePointsValid(Point[] points)
        {
            foreach (var point in points)
            {
                if (point.X < MIN_COORD || point.X > MAX_COORD ||
                    point.Y < MIN_COORD || point.Y > MAX_COORD)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// éclaircit une couleur
        /// </summary>
        private Color GetLighterColor(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, (int)(color.R + (255 - color.R) * factor)),
                Math.Min(255, (int)(color.G + (255 - color.G) * factor)),
                Math.Min(255, (int)(color.B + (255 - color.B) * factor))
            );
        }

        /// <summary>
        /// Assombrit une couleur
        /// </summary>
        private Color GetDarkerColor(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                Math.Max(0, (int)(color.R * (1 - factor))),
                Math.Max(0, (int)(color.G * (1 - factor))),
                Math.Max(0, (int)(color.B * (1 - factor)))
            );
        }
    }
}

