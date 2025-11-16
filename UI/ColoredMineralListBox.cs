using System.Drawing.Drawing2D;
using wmine.Models;

namespace wmine.UI
{
    /// <summary>
    /// CheckedListBox avec couleurs pour les types de minéraux
    /// </summary>
    public class ColoredMineralListBox : CheckedListBox
    {
        public ColoredMineralListBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 24;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var item = Items[e.Index];
            if (item is MineralType mineral)
            {
                // Couleur du minéral
                Color mineralColor = MineralColors.GetColor(mineral);
                Color textColor = MineralColors.GetTextColor(mineralColor);

                // Fond avec dégradé si sélectionné/hover
                bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                if (isSelected)
                {
                    using (var brush = new LinearGradientBrush(
                        e.Bounds,
                        MineralColors.Lighten(mineralColor, 0.3f),
                        mineralColor,
                        LinearGradientMode.Horizontal))
                    {
                        e.Graphics.FillRectangle(brush, e.Bounds);
                    }
                }
                else
                {
                    using (var brush = new SolidBrush(BackColor))
                    {
                        e.Graphics.FillRectangle(brush, e.Bounds);
                    }
                }

                // Pastille de couleur é gauche
                var colorBoxBounds = new Rectangle(e.Bounds.X + 24, e.Bounds.Y + 4, 16, 16);
                using (var path = GetRoundedRectangle(colorBoxBounds, 3))
                {
                    // dégradé pour effet 3D
                    using (var brush = new LinearGradientBrush(
                        colorBoxBounds,
                        MineralColors.Lighten(mineralColor, 0.2f),
                        MineralColors.Darken(mineralColor, 0.2f),
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillPath(brush, path);
                    }

                    // Bordure
                    using (var pen = new Pen(MineralColors.Darken(mineralColor, 0.3f), 1.5f))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }

                    // Reflet (effet verre)
                    var glowBounds = new Rectangle(
                        colorBoxBounds.X + 2,
                        colorBoxBounds.Y + 2,
                        colorBoxBounds.Width - 4,
                        colorBoxBounds.Height / 2
                    );
                    using (var glowPath = GetRoundedRectangle(glowBounds, 2))
                    using (var glowBrush = new LinearGradientBrush(
                        glowBounds,
                        Color.FromArgb(80, 255, 255, 255),
                        Color.FromArgb(0, 255, 255, 255),
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillPath(glowBrush, glowPath);
                    }
                }

                // Checkbox
                var checkBounds = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 4, 16, 16);
                var isChecked = GetItemChecked(e.Index);
                
                if (isChecked)
                {
                    using (var brush = new SolidBrush(mineralColor))
                    {
                        e.Graphics.FillRectangle(brush, checkBounds);
                    }
                    e.Graphics.DrawRectangle(Pens.White, checkBounds);
                    
                    // Checkmark
                    using (var pen = new Pen(textColor, 2f))
                    {
                        e.Graphics.DrawLines(pen, new[]
                        {
                            new Point(checkBounds.X + 3, checkBounds.Y + 8),
                            new Point(checkBounds.X + 6, checkBounds.Y + 11),
                            new Point(checkBounds.X + 13, checkBounds.Y + 4)
                        });
                    }
                }
                else
                {
                    e.Graphics.FillRectangle(new SolidBrush(BackColor), checkBounds);
                    e.Graphics.DrawRectangle(Pens.Gray, checkBounds);
                }

                // Texte
                var textBounds = new Rectangle(
                    e.Bounds.X + 46,
                    e.Bounds.Y,
                    e.Bounds.Width - 46,
                    e.Bounds.Height
                );

                using (var brush = new SolidBrush(ForeColor))
                {
                    var sf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Near
                    };
                    e.Graphics.DrawString(MineralColors.GetDisplayName(mineral), Font, brush, textBounds, sf);
                }
            }

            e.DrawFocusRectangle();
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

