using System;
using System.Drawing;
using System.Windows.Forms;
using wmine.Models;
using wmine.UI;

namespace wmine
{
    /// <summary>
    /// Formulaire de démonstration pour comparer visuellement les différents markers
    /// </summary>
    public class MarkerVisualizationForm : Form
    {
        private Panel panelCrystal;
        private Panel panelCurrent;
        private Label lblCrystal;
        private Label lblCurrent;

        public MarkerVisualizationForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Comparaison Visuelle des Markers";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 35, 45);

            // Titre principal
            var lblTitle = new Label
            {
                Text = "?? Comparaison des Markers de Filon",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 18, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblTitle);

            // Description
            var lblDesc = new Label
            {
                Text = "Cliquez sur les panels pour voir l'animation de survol",
                Location = new Point(20, 60),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.FromArgb(200, 200, 200),
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblDesc);

            // Panel gauche - Cristal
            panelCrystal = new Panel
            {
                Location = new Point(30, 100),
                Size = new Size(400, 520),
                BackColor = Color.FromArgb(40, 45, 55),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblCrystal = new Label
            {
                Text = "?? CHOIX 1 : FilonCrystalMarker\n(Cristal Hexagonal)",
                Location = new Point(10, 10),
                Size = new Size(380, 50),
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 181, 246),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelCrystal.Controls.Add(lblCrystal);

            // Ajouter exemples de cristaux
            AddCrystalExamples(panelCrystal);

            this.Controls.Add(panelCrystal);

            // Panel droit - Actuel
            panelCurrent = new Panel
            {
                Location = new Point(450, 100),
                Size = new Size(400, 520),
                BackColor = Color.FromArgb(40, 45, 55),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblCurrent = new Label
            {
                Text = "?? CHOIX 2 : FilonMarker3D (Actuel)\n(Piquet + Sphére)",
                Location = new Point(10, 10),
                Size = new Size(380, 50),
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 167, 38),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelCurrent.Controls.Add(lblCurrent);

            // Ajouter exemples actuels
            AddCurrentExamples(panelCurrent);

            this.Controls.Add(panelCurrent);

            // Bouton de sélection
            var btnSelectCrystal = new Button
            {
                Text = "? Utiliser les Cristaux",
                Location = new Point(30, 630),
                Size = new Size(400, 40),
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSelectCrystal.FlatAppearance.BorderSize = 0;
            btnSelectCrystal.Click += (s, e) =>
            {
                MessageBox.Show(
                    "? FilonCrystalMarker sélectionné!\n\n" +
                    "Pour l'utiliser, remplacez dans Form1.cs (UpdateMapMarkers):\n\n" +
                    "var marker = new FilonMarker3D(...)\n\n" +
                    "par:\n\n" +
                    "var marker = new FilonCrystalMarker(...)",
                    "Marker Sélectionné",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            };
            this.Controls.Add(btnSelectCrystal);

            var btnKeepCurrent = new Button
            {
                Text = "? Garder l'Actuel",
                Location = new Point(450, 630),
                Size = new Size(400, 40),
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnKeepCurrent.FlatAppearance.BorderSize = 0;
            btnKeepCurrent.Click += (s, e) =>
            {
                MessageBox.Show(
                    "? FilonMarker3D conservé!\n\n" +
                    "Le marker actuel (piquet + sphére) continuera d'étre utilisé.",
                    "Marker Conservé",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            };
            this.Controls.Add(btnKeepCurrent);
        }

        private void AddCrystalExamples(Panel parent)
        {
            var minerals = new[]
            {
                (MineralType.Argent, "Argent", 80),
                (MineralType.Cuivre, "Cuivre", 180),
                (MineralType.Fer, "Fer", 280),
                (MineralType.Améthyste, "Améthyste", 80),
                (MineralType.Fluor, "Fluor", 180),
                (MineralType.Grenats, "Grenats", 280)
            };

            int row = 0;
            for (int i = 0; i < minerals.Length; i++)
            {
                var (type, name, xPos) = minerals[i];
                int yPos = 80 + (row * 140);

                // Label du minéral
                var label = new Label
                {
                    Text = name,
                    Location = new Point(xPos - 30, yPos - 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };
                parent.Controls.Add(label);

                // Panel pour dessiner le marker
                var markerPanel = new CrystalMarkerPanel(type)
                {
                    Location = new Point(xPos, yPos),
                    Size = new Size(80, 100),
                    BackColor = Color.Transparent
                };
                parent.Controls.Add(markerPanel);

                if (i % 3 == 2) row++;
            }
        }

        private void AddCurrentExamples(Panel parent)
        {
            var minerals = new[]
            {
                (MineralType.Argent, "Argent", 80),
                (MineralType.Cuivre, "Cuivre", 180),
                (MineralType.Fer, "Fer", 280),
                (MineralType.Améthyste, "Améthyste", 80),
                (MineralType.Fluor, "Fluor", 180),
                (MineralType.Grenats, "Grenats", 280)
            };

            int row = 0;
            for (int i = 0; i < minerals.Length; i++)
            {
                var (type, name, xPos) = minerals[i];
                int yPos = 80 + (row * 140);

                // Label du minéral
                var label = new Label
                {
                    Text = name,
                    Location = new Point(xPos - 30, yPos - 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };
                parent.Controls.Add(label);

                // Panel pour dessiner le marker
                var markerPanel = new CurrentMarkerPanel(type)
                {
                    Location = new Point(xPos, yPos),
                    Size = new Size(80, 100),
                    BackColor = Color.Transparent
                };
                parent.Controls.Add(markerPanel);

                if (i % 3 == 2) row++;
            }
        }

        // Panel personnalisé pour dessiner le cristal
        private class CrystalMarkerPanel : Panel
        {
            private readonly Color _color;
            private bool _isHovered;

            public CrystalMarkerPanel(MineralType type)
            {
                _color = MineralColors.GetColor(type);
                this.MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
                this.MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int cx = Width / 2;
                int cy = Height / 2 + 10;

                // Dessiner un cristal simplifié
                DrawSimplifiedCrystal(g, cx, cy, _isHovered);
            }

            private void DrawSimplifiedCrystal(Graphics g, int x, int y, bool hover)
            {
                float scale = hover ? 1.1f : 1.0f;
                int size = (int)(20 * scale);

                // Ombre
                using (var shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
                {
                    g.FillEllipse(shadowBrush, x - 12, y + 20, 24, 8);
                }

                // Pointe
                var tip = new Point[]
                {
                    new Point(x, y - size),
                    new Point(x - (int)(size * 0.7), y),
                    new Point(x + (int)(size * 0.7), y)
                };

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(x, y - size),
                    new Point(x, y),
                    GetLighterColor(_color, 0.4f),
                    _color))
                {
                    g.FillPolygon(brush, tip);
                }

                // Corps
                var body = new Rectangle(x - (int)(size * 0.7), y, (int)(size * 1.4), size);
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(x, y),
                    new Point(x, y + size),
                    _color,
                    GetDarkerColor(_color, 0.3f)))
                {
                    g.FillRectangle(brush, body);
                }

                // Reflet
                using (var pen = new Pen(Color.FromArgb(150, Color.White), 1.5f))
                {
                    g.DrawLine(pen, x - 6, y + 5, x - 6, y + 15);
                }

                // Effet de brillance si survolé
                if (hover)
                {
                    using (var glowBrush = new SolidBrush(Color.FromArgb(50, Color.White)))
                    {
                        g.FillEllipse(glowBrush, x - 15, y - 15, 30, 30);
                    }
                }
            }

            private Color GetLighterColor(Color color, float factor)
            {
                return Color.FromArgb(
                    Math.Min(255, (int)(color.R + (255 - color.R) * factor)),
                    Math.Min(255, (int)(color.G + (255 - color.G) * factor)),
                    Math.Min(255, (int)(color.B + (255 - color.B) * factor))
                );
            }

            private Color GetDarkerColor(Color color, float factor)
            {
                return Color.FromArgb(
                    Math.Max(0, (int)(color.R * (1 - factor))),
                    Math.Max(0, (int)(color.G * (1 - factor))),
                    Math.Max(0, (int)(color.B * (1 - factor)))
                );
            }
        }

        // Panel personnalisé pour dessiner le marker actuel
        private class CurrentMarkerPanel : Panel
        {
            private readonly Color _color;
            private bool _isHovered;

            public CurrentMarkerPanel(MineralType type)
            {
                _color = MineralColors.GetColor(type);
                this.MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
                this.MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int cx = Width / 2;
                int cy = Height / 2 + 10;

                // Dessiner le marker actuel simplifié
                DrawSimplifiedCurrentMarker(g, cx, cy, _isHovered);
            }

            private void DrawSimplifiedCurrentMarker(Graphics g, int x, int y, bool hover)
            {
                float scale = hover ? 1.2f : 1.0f;
                int sphereSize = (int)(16 * scale);
                int poleHeight = (int)(30 * scale);

                // Ombre
                using (var shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
                {
                    g.FillEllipse(shadowBrush, x - 8, y + 15, 16, 6);
                }

                // Piquet
                var poleRect = new Rectangle(x - 4, y - 10, 8, poleHeight);
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(x - 4, y),
                    new Point(x + 4, y),
                    Color.FromArgb(80, 80, 90),
                    Color.FromArgb(140, 140, 150)))
                {
                    g.FillRectangle(brush, poleRect);
                }

                // Sphére minérale
                var sphereRect = new Rectangle(x - sphereSize / 2, y - 25 - sphereSize / 2, sphereSize, sphereSize);
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(sphereRect);
                    using (var brush = new System.Drawing.Drawing2D.PathGradientBrush(path))
                    {
                        brush.CenterColor = GetLighterColor(_color, 0.3f);
                        brush.SurroundColors = new[] { _color };
                        g.FillPath(brush, path);
                    }
                }

                // Reflet sur la sphére
                var glowRect = new Rectangle(x - sphereSize / 4, y - 25 - sphereSize / 3, sphereSize / 2, sphereSize / 3);
                using (var brush = new SolidBrush(Color.FromArgb(120, Color.White)))
                {
                    g.FillEllipse(brush, glowRect);
                }
            }

            private Color GetLighterColor(Color color, float factor)
            {
                return Color.FromArgb(
                    Math.Min(255, (int)(color.R + (255 - color.R) * factor)),
                    Math.Min(255, (int)(color.G + (255 - color.G) * factor)),
                    Math.Min(255, (int)(color.B + (255 - color.B) * factor))
                );
            }
        }
    }
}
