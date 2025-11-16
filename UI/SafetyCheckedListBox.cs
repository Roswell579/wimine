using System.Drawing.Drawing2D;
using wmine.Models;

namespace wmine.UI
{
    /// <summary>
    /// CheckedListBox avec formatage coloré et tooltips pour les statuts de sécurité
    /// </summary>
    public class SafetyCheckedListBox : CheckedListBox
    {
        private ModernToolTip _tooltip;

        public SafetyCheckedListBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 22;
            _tooltip = new ModernToolTip
            {
                IsDanger = true,  // Par défaut en mode danger
                AutoPopDelay = 10000,
                InitialDelay = 300,
                ReshowDelay = 100
            };
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            var item = Items[e.Index];
            if (item is FilonStatus status)
            {
                // Couleur selon le statut
                Color textColor = status.GetDisplayColor();
                if (textColor == Color.Black)
                {
                    textColor = ForeColor; // Utiliser la couleur par défaut
                }

                // Font en gras pour Danger Mortel
                Font font = status == FilonStatus.DangerMortel 
                    ? new Font(Font, FontStyle.Bold) 
                    : Font;

                // Dessiner la checkbox
                var checkBounds = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, 16, 16);
                var isChecked = GetItemChecked(e.Index);
                
                if (isChecked)
                {
                    e.Graphics.FillRectangle(Brushes.DodgerBlue, checkBounds);
                    e.Graphics.DrawRectangle(Pens.White, checkBounds);
                    // Checkmark
                    using (var pen = new Pen(Color.White, 2f))
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

                // Dessiner le texte
                var textBounds = new Rectangle(
                    e.Bounds.X + 22,
                    e.Bounds.Y,
                    e.Bounds.Width - 22,
                    e.Bounds.Height
                );

                using (var brush = new SolidBrush(textColor))
                {
                    var sf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center
                    };
                    e.Graphics.DrawString(status.GetDisplayName(), font, brush, textBounds, sf);
                }

                // Icéne téte de mort pour Danger Mortel
                if (status == FilonStatus.DangerMortel)
                {
                    var skullBounds = new Rectangle(e.Bounds.Right - 25, e.Bounds.Y + 3, 20, 16);
                    DrawSkullIcon(e.Graphics, skullBounds);
                }
            }

            e.DrawFocusRectangle();
        }

        private void DrawSkullIcon(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Créne simplifié (cercle + rectangles pour dents)
            using (var brush = new SolidBrush(Color.Red))
            using (var pen = new Pen(Color.DarkRed, 1.5f))
            {
                // Téte
                var headRect = new Rectangle(bounds.X + 2, bounds.Y, 16, 12);
                g.FillEllipse(brush, headRect);
                g.DrawEllipse(pen, headRect);

                // Yeux (X)
                g.DrawLine(pen, bounds.X + 5, bounds.Y + 3, bounds.X + 8, bounds.Y + 6);
                g.DrawLine(pen, bounds.X + 8, bounds.Y + 3, bounds.X + 5, bounds.Y + 6);
                g.DrawLine(pen, bounds.X + 12, bounds.Y + 3, bounds.X + 15, bounds.Y + 6);
                g.DrawLine(pen, bounds.X + 15, bounds.Y + 3, bounds.X + 12, bounds.Y + 6);

                // Dents (petits rectangles)
                for (int i = 0; i < 4; i++)
                {
                    var toothRect = new Rectangle(bounds.X + 4 + (i * 3), bounds.Y + 10, 2, 3);
                    g.FillRectangle(Brushes.White, toothRect);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int index = IndexFromPoint(e.Location);
            if (index >= 0 && index < Items.Count)
            {
                var item = Items[index];
                if (item is FilonStatus status)
                {
                    var tooltip = status.GetSafetyTooltip();
                    if (!string.IsNullOrEmpty(tooltip))
                    {
                        var currentTip = _tooltip.GetToolTip(this);
                        if (currentTip != tooltip)
                        {
                            _tooltip.IsDanger = (status == FilonStatus.DangerMortel);
                            _tooltip.Show(tooltip, this, e.X, e.Y + 20, 5000);
                        }
                    }
                    else
                    {
                        _tooltip.Hide(this);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tooltip?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

