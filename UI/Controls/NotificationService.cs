using System.Drawing.Drawing2D;
using wmine.Core.Interfaces;

namespace wmine.UI.Controls
{
    /// <summary>
    /// Notification toast pour feedback utilisateur
    /// </summary>
    public class ToastNotification : Form
    {
        private readonly string _message;
        private readonly Color _backgroundColor;
        private readonly Color _textColor;
        private System.Windows.Forms.Timer? _closeTimer;
        private float _opacity = 0f;
        private const int AnimationSteps = 10;

        public ToastNotification(string message, Color backgroundColor, Color textColor, int durationMs)
        {
            _message = message;
            _backgroundColor = backgroundColor;
            _textColor = textColor;

            InitializeToast(durationMs);
        }

        private void InitializeToast(int durationMs)
        {
            // Configuration du formulaire
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.Opacity = 0;

            // Taille et position
            var screen = Screen.PrimaryScreen;
            var width = 350;
            var height = 80;
            var x = screen.WorkingArea.Right - width - 20;
            var y = screen.WorkingArea.Bottom - height - 20;

            this.Size = new Size(width, height);
            this.Location = new Point(x, y);

            // Double buffering pour éviter le scintillement
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                         ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.UserPaint, true);

            // Timer pour fermeture automatique
            _closeTimer = new System.Windows.Forms.Timer();
            _closeTimer.Interval = durationMs;
            _closeTimer.Tick += (s, e) => CloseToast();

            // Gestion des événements
            this.Paint += ToastNotification_Paint;
            this.Shown += ToastNotification_Shown;
            this.Click += (s, e) => CloseToast();
        }

        private async void ToastNotification_Shown(object? sender, EventArgs e)
        {
            await FadeIn();
            _closeTimer?.Start();
        }

        private async Task FadeIn()
        {
            for (int i = 0; i <= AnimationSteps; i++)
            {
                _opacity = i / (float)AnimationSteps;
                this.Invalidate();
                await Task.Delay(20);
            }
        }

        private async void CloseToast()
        {
            _closeTimer?.Stop();
            await FadeOut();
            this.Close();
        }

        private async Task FadeOut()
        {
            for (int i = AnimationSteps; i >= 0; i--)
            {
                _opacity = i / (float)AnimationSteps;
                this.Invalidate();
                await Task.Delay(15);
            }
        }

        private void ToastNotification_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var alpha = (int)(_opacity * 255);
            var bgColor = Color.FromArgb(alpha, _backgroundColor);
            var textColorWithAlpha = Color.FromArgb(alpha, _textColor);

            // Dessiner le fond arrondi
            using (var path = GetRoundedRectangle(this.ClientRectangle, 15))
            using (var brush = new SolidBrush(bgColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            // Dessiner l'ombre
            using (var shadowPath = GetRoundedRectangle(new Rectangle(2, 2, Width - 4, Height - 4), 15))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(alpha / 3, 0, 0, 0)))
            {
                e.Graphics.FillPath(shadowBrush, shadowPath);
            }

            // Dessiner le texte
            var textRect = new Rectangle(20, 15, Width - 40, Height - 30);
            using (var textBrush = new SolidBrush(textColorWithAlpha))
            using (var font = new Font("Segoe UI Emoji", 10, FontStyle.Regular))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };
                e.Graphics.DrawString(_message, font, textBrush, textRect, sf);
            }
        }

        private GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _closeTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Service de notification pour afficher des messages toast
    /// </summary>
    public class NotificationService : INotificationService
    {
        public void ShowSuccess(string message, int durationMs = 3000)
        {
            var toast = new ToastNotification(
                message,
                Color.FromArgb(76, 175, 80),  // Vert
                Color.White,
                durationMs
            );
            toast.Show();
        }

        public void ShowError(string message, int durationMs = 5000)
        {
            var toast = new ToastNotification(
                message,
                Color.FromArgb(244, 67, 54),  // Rouge
                Color.White,
                durationMs
            );
            toast.Show();
        }

        public void ShowInfo(string message, int durationMs = 3000)
        {
            var toast = new ToastNotification(
                message,
                Color.FromArgb(33, 150, 243),  // Bleu
                Color.White,
                durationMs
            );
            toast.Show();
        }

        public void ShowWarning(string message, int durationMs = 4000)
        {
            var toast = new ToastNotification(
                message,
                Color.FromArgb(255, 152, 0),  // Orange
                Color.White,
                durationMs
            );
            toast.Show();
        }
    }
}


