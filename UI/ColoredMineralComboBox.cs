using System.Drawing.Drawing2D;
using wmine.Models;

namespace wmine.UI
{
    /// <summary>
    /// ComboBox avec couleurs pour afficher les minéraux avec leur couleur
    /// </summary>
    public class ColoredMineralComboBox : ComboBox
    {
        public ColoredMineralComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            ItemHeight = 24;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Fond
            e.DrawBackground();

            var item = Items[e.Index];
            
            // Si c'est un objet anonyme avec Display et Value
            var displayText = "";
            Color? mineralColor = null;

            if (item != null)
            {
                var itemType = item.GetType();
                var displayProp = itemType.GetProperty("Display");
                var valueProp = itemType.GetProperty("Value");

                if (displayProp != null)
                {
                    displayText = displayProp.GetValue(item)?.ToString() ?? "";
                }

                if (valueProp != null)
                {
                    var value = valueProp.GetValue(item);
                    if (value is MineralType mineral)
                    {
                        mineralColor = MineralColors.GetColor(mineral);
                    }
                }
            }

            // Si on a une couleur, dessiner la pastille
            if (mineralColor.HasValue)
            {
                var colorBoxBounds = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 4, 16, 16);
                using (var path = GetRoundedRectangle(colorBoxBounds, 3))
                {
                    // dégradé pour effet 3D
                    using (var brush = new LinearGradientBrush(
                        colorBoxBounds,
                        MineralColors.Lighten(mineralColor.Value, 0.2f),
                        MineralColors.Darken(mineralColor.Value, 0.2f),
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillPath(brush, path);
                    }

                    // Bordure
                    using (var pen = new Pen(MineralColors.Darken(mineralColor.Value, 0.3f), 1.5f))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }

            // Texte
            var textBounds = new Rectangle(
                e.Bounds.X + (mineralColor.HasValue ? 26 : 6),
                e.Bounds.Y,
                e.Bounds.Width - (mineralColor.HasValue ? 30 : 10),
                e.Bounds.Height
            );

            using (var brush = new SolidBrush(ForeColor))
            {
                var sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near
                };
                e.Graphics.DrawString(displayText, Font, brush, textBounds, sf);
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

