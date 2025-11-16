using System.Drawing;

namespace wmine.UI
{
    public static class IconFactory
    {
        public static Image CreateGlyphIcon(string glyph, int size, Color foreground)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var font = new Font("Segoe UI Emoji", size * 0.6f, FontStyle.Regular, GraphicsUnit.Pixel))
                using (var brush = new SolidBrush(foreground))
                {
                    g.DrawString(glyph, font, brush, new RectangleF(0, 0, size, size), sf);
                }
            }
            return bmp;
        }
    }
}
