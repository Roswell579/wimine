using GMap.NET.WindowsForms;
using System.Drawing.Drawing2D;
using wmine.Models;

namespace wmine.UI
{
    /// <summary>
    /// Marqueur 3D personnalisé pour les filons miniers
    /// </summary>
    public class FilonMarker3D : GMapMarker
    {
        private readonly Filon _filon;
        private readonly Color _color;
        private bool _isHovered;
        private float _scale = 1.0f;

        public FilonMarker3D(GMap.NET.PointLatLng position, Filon filon, Color color)
            : base(position)
        {
            _filon = filon;
            _color = color;
            Size = new Size(40, 50);
            Offset = new Point(-20, -50);
        }

        public override void OnRender(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Animation de scale au survol
            int size = (int)(30 * _scale);
            int markerHeight = (int)(45 * _scale);

            // Point central
            var centerX = LocalPosition.X;
            var centerY = LocalPosition.Y;

            // Dessiner l'ombre
            DrawShadow(g, centerX, centerY + markerHeight - 5, size);

            // Dessiner le piquet de mine 3D
            DrawMiningStake3D(g, centerX, centerY, size, markerHeight);

            // Dessiner la sphére minérale au sommet
            DrawMineralSphere(g, centerX, centerY, size);

            // Tooltip au survol
            if (_isHovered)
            {
                DrawTooltip(g, centerX, centerY - 10);
            }
        }

        private void DrawShadow(Graphics g, int x, int y, int size)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(x - size / 2, y - size / 4, size, size / 2);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(100, Color.Black);
                    brush.SurroundColors = new[] { Color.FromArgb(0, Color.Black) };
                    g.FillPath(brush, path);
                }
            }
        }

        private void DrawMiningStake3D(Graphics g, int centerX, int centerY, int size, int height)
        {
            // Piquet avec effet 3D (dégradé latéral)
            var stakeWidth = size / 4;
            var stakeRect = new Rectangle(
                centerX - stakeWidth / 2,
                centerY + size / 2,
                stakeWidth,
                height - size / 2
            );

            using (var brush = new LinearGradientBrush(
                stakeRect,
                Color.FromArgb(200, 80, 60, 50),
                Color.FromArgb(200, 40, 30, 25),
                LinearGradientMode.Horizontal))
            {
                g.FillRectangle(brush, stakeRect);
            }

            // Bordure du piquet
            using (var pen = new Pen(Color.FromArgb(150, 20, 15, 10), 1))
            {
                g.DrawRectangle(pen, stakeRect);
            }
        }

        private void DrawMineralSphere(Graphics g, int centerX, int centerY, int size)
        {
            var sphereBounds = new Rectangle(
                centerX - size / 2,
                centerY - size / 2,
                size,
                size
            );

            // Ombre de la sphére
            var shadowBounds = sphereBounds;
            shadowBounds.Offset(2, 2);
            using (var shadowPath = new GraphicsPath())
            {
                shadowPath.AddEllipse(shadowBounds);
                using (var shadowBrush = new PathGradientBrush(shadowPath))
                {
                    shadowBrush.CenterColor = Color.FromArgb(80, Color.Black);
                    shadowBrush.SurroundColors = new[] { Color.FromArgb(0, Color.Black) };
                    g.FillPath(shadowBrush, shadowPath);
                }
            }

            // Sphére minérale avec dégradé radial 3D
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(sphereBounds);
                using (var brush = new PathGradientBrush(path))
                {
                    // Couleur centrale (highlight)
                    brush.CenterColor = Color.FromArgb(255, LightenColor(_color, 80));
                    brush.CenterPoint = new PointF(
                        centerX - size / 6,
                        centerY - size / 6
                    );

                    // Couleurs périphériques (ombre)
                    brush.SurroundColors = new[] { Color.FromArgb(255, DarkenColor(_color, 40)) };

                    g.FillPath(brush, path);
                }

                // Bordure brillante
                using (var pen = new Pen(Color.FromArgb(150, Color.White), 2))
                {
                    g.DrawPath(pen, path);
                }

                // Reflet brillant
                var highlightBounds = new Rectangle(
                    centerX - size / 3,
                    centerY - size / 3,
                    size / 2,
                    size / 2
                );
                using (var highlightPath = new GraphicsPath())
                {
                    highlightPath.AddEllipse(highlightBounds);
                    using (var highlightBrush = new PathGradientBrush(highlightPath))
                    {
                        highlightBrush.CenterColor = Color.FromArgb(180, Color.White);
                        highlightBrush.SurroundColors = new[] { Color.FromArgb(0, Color.White) };
                        g.FillPath(highlightBrush, highlightPath);
                    }
                }
            }

            // Indicateur de danger (si applicable)
            if (_filon.Statut.HasFlag(FilonStatus.DangerMortel))
            {
                DrawDangerIndicator(g, centerX + size / 2 - 8, centerY - size / 2 + 8);
            }
        }

        private void DrawDangerIndicator(Graphics g, int x, int y)
        {
            // Triangle d'avertissement rouge
            var points = new[]
            {
                new Point(x, y - 8),
                new Point(x - 7, y + 4),
                new Point(x + 7, y + 4)
            };

            using (var brush = new LinearGradientBrush(
                new Rectangle(x - 7, y - 8, 14, 12),
                Color.FromArgb(255, 220, 50, 50),
                Color.FromArgb(255, 180, 0, 0),
                LinearGradientMode.Vertical))
            {
                g.FillPolygon(brush, points);
            }

            using (var pen = new Pen(Color.White, 1.5f))
            {
                g.DrawPolygon(pen, points);
            }

            // Point d'exclamation
            using (var font = new Font("Segoe UI Emoji", 8, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString("!", font, brush, new PointF(x, y - 1), sf);
            }
        }

        private void DrawTooltip(Graphics g, int centerX, int centerY)
        {
            var tooltipText = $"{_filon.Nom}\n{MineralColors.GetDisplayName(_filon.MatierePrincipale)}";
            using (var font = new Font("Segoe UI Emoji", 9, FontStyle.Bold))
            {
                var textSize = g.MeasureString(tooltipText, font);
                var tooltipBounds = new Rectangle(
                    (int)(centerX - textSize.Width / 2 - 8),
                    (int)(centerY - textSize.Height - 20),
                    (int)(textSize.Width + 16),
                    (int)(textSize.Height + 10)
                );

                // Fond du tooltip avec effet glassmorphism
                GlassEffects.DrawGlassPanel(g, tooltipBounds, Color.FromArgb(20, 20, 30), 220);

                // Texte
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString(tooltipText, font, brush,
                        new PointF(tooltipBounds.X + 8, tooltipBounds.Y + 5));
                }
            }
        }

        private Color LightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                Math.Min(255, color.R + amount),
                Math.Min(255, color.G + amount),
                Math.Min(255, color.B + amount)
            );
        }

        private Color DarkenColor(Color color, int amount)
        {
            return Color.FromArgb(
                Math.Max(0, color.R - amount),
                Math.Max(0, color.G - amount),
                Math.Max(0, color.B - amount)
            );
        }

        public void SetHovered(bool hovered)
        {
            _isHovered = hovered;
            _scale = hovered ? 1.2f : 1.0f;
        }
    }
}

