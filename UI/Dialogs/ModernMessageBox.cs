using System.Drawing.Drawing2D;

namespace wmine.UI.Dialogs
{
    /// <summary>
    /// Type de message box moderne
    /// </summary>
    public enum ModernMessageBoxIcon
    {
        None,
        Information,
        Success,
        Warning,
        Error,
        Question
    }

    /// <summary>
    /// Boutons disponibles dans la MessageBox
    /// </summary>
    public enum ModernMessageBoxButtons
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    /// <summary>
    /// MessageBox moderne et stylée pour WMine
    /// </summary>
    public class ModernMessageBox : Form
    {
        private readonly string _message;
        private readonly string _title;
        private readonly ModernMessageBoxIcon _icon;
        private Panel? _contentPanel;
        private Label? _lblMessage;
        private Panel? _iconPanel;
        private Panel? _buttonPanel;

        public DialogResult Result { get; private set; } = DialogResult.Cancel;

        private ModernMessageBox(string message, string title, ModernMessageBoxIcon icon, ModernMessageBoxButtons buttons)
        {
            _message = message;
            _title = title;
            _icon = icon;

            InitializeDialog(buttons);
        }

        private void InitializeDialog(ModernMessageBoxButtons buttons)
        {
            // Configuration du formulaire
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 35, 45);
            this.Size = new Size(500, 250);
            this.MinimumSize = new Size(400, 200);
            this.MaximumSize = new Size(700, 400);
            this.ShowInTaskbar = false;
            this.TopMost = true;

            // Double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                         ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.UserPaint, true);

            // Panel pour le titre
            var titlePanel = CreateTitlePanel();
            
            // Panel pour le contenu
            _contentPanel = CreateContentPanel();
            
            // Panel pour les boutons
            _buttonPanel = CreateButtonPanel(buttons);

            this.Controls.Add(titlePanel);
            this.Controls.Add(_contentPanel);
            this.Controls.Add(_buttonPanel);

            // Gestion du drag pour déplacer la fenétre
            titlePanel.MouseDown += TitlePanel_MouseDown;
            titlePanel.MouseMove += TitlePanel_MouseMove;
            titlePanel.MouseUp += TitlePanel_MouseUp;

            // Painting
            this.Paint += ModernMessageBox_Paint;

            // Calculer la taille optimale
            AdjustSize();
        }

        private Panel CreateTitlePanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = GetIconColor()
            };

            var lblTitle = new Label
            {
                Text = _title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 14, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };

            var btnClose = new Button
            {
                Text = "?",
                Size = new Size(40, 40),
                Location = new Point(panel.Width - 45, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 16),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 255, 255, 255);
            btnClose.Click += (s, e) =>
            {
                Result = DialogResult.Cancel;
                this.Close();
            };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(btnClose);

            return panel;
        }

        private Panel CreateContentPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 35, 45),
                Padding = new Padding(20)
            };

            // Icéne
            _iconPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(60, 60),
                BackColor = Color.Transparent
            };
            _iconPanel.Paint += IconPanel_Paint;

            // Message
            _lblMessage = new Label
            {
                Text = _message,
                Location = new Point(100, 20),
                Width = panel.Width - 140,
                AutoSize = true,
                MaximumSize = new Size(panel.Width - 140, 0),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 11),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            panel.Controls.Add(_iconPanel);
            panel.Controls.Add(_lblMessage);

            return panel;
        }

        private Panel CreateButtonPanel(ModernMessageBoxButtons buttons)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(25, 30, 40),
                Padding = new Padding(20, 15, 20, 15)
            };

            var buttonList = GetButtonsForType(buttons);
            int totalWidth = buttonList.Count * 110 + (buttonList.Count - 1) * 10;
            int startX = (panel.Width - totalWidth) / 2;

            for (int i = 0; i < buttonList.Count; i++)
            {
                var btn = CreateModernButton(
                    buttonList[i].Text,
                    buttonList[i].Result,
                    buttonList[i].IsPrimary
                );
                btn.Location = new Point(startX + i * 120, 15);
                panel.Controls.Add(btn);
            }

            return panel;
        }

        private Button CreateModernButton(string text, DialogResult result, bool isPrimary)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(110, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = isPrimary ? GetIconColor() : Color.FromArgb(50, 55, 65),
                ForeColor = Color.White
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = isPrimary 
                ? Color.FromArgb(Math.Min(255, GetIconColor().R + 30), 
                                Math.Min(255, GetIconColor().G + 30), 
                                Math.Min(255, GetIconColor().B + 30))
                : Color.FromArgb(60, 65, 75);

            btn.Click += (s, e) =>
            {
                Result = result;
                this.Close();
            };

            return btn;
        }

        private List<(string Text, DialogResult Result, bool IsPrimary)> GetButtonsForType(ModernMessageBoxButtons buttons)
        {
            return buttons switch
            {
                ModernMessageBoxButtons.OK => new List<(string, DialogResult, bool)>
                {
                    ("OK", DialogResult.OK, true)
                },
                ModernMessageBoxButtons.OKCancel => new List<(string, DialogResult, bool)>
                {
                    ("OK", DialogResult.OK, true),
                    ("Annuler", DialogResult.Cancel, false)
                },
                ModernMessageBoxButtons.YesNo => new List<(string, DialogResult, bool)>
                {
                    ("Oui", DialogResult.Yes, true),
                    ("Non", DialogResult.No, false)
                },
                ModernMessageBoxButtons.YesNoCancel => new List<(string, DialogResult, bool)>
                {
                    ("Oui", DialogResult.Yes, true),
                    ("Non", DialogResult.No, false),
                    ("Annuler", DialogResult.Cancel, false)
                },
                _ => new List<(string, DialogResult, bool)> { ("OK", DialogResult.OK, true) }
            };
        }

        private Color GetIconColor()
        {
            return _icon switch
            {
                ModernMessageBoxIcon.Success => Color.FromArgb(76, 175, 80),
                ModernMessageBoxIcon.Information => Color.FromArgb(33, 150, 243),
                ModernMessageBoxIcon.Warning => Color.FromArgb(255, 152, 0),
                ModernMessageBoxIcon.Error => Color.FromArgb(244, 67, 54),
                ModernMessageBoxIcon.Question => Color.FromArgb(156, 39, 176),
                _ => Color.FromArgb(100, 181, 246)
            };
        }

        private string GetIconSymbol()
        {
            return _icon switch
            {
                ModernMessageBoxIcon.Success => "?",
                ModernMessageBoxIcon.Information => "i",
                ModernMessageBoxIcon.Warning => "!",
                ModernMessageBoxIcon.Error => "?",
                ModernMessageBoxIcon.Question => "?",
                _ => "i"
            };
        }

        private void IconPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (_iconPanel == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = _iconPanel.ClientRectangle;
            var color = GetIconColor();

            // Cercle de fond
            using (var brush = new SolidBrush(Color.FromArgb(50, color)))
            {
                e.Graphics.FillEllipse(brush, rect);
            }

            // Bordure
            using (var pen = new Pen(color, 3))
            {
                e.Graphics.DrawEllipse(pen, 3, 3, rect.Width - 6, rect.Height - 6);
            }

            // Symbole
            using (var font = new Font("Segoe UI Emoji", 24, FontStyle.Bold))
            using (var brush = new SolidBrush(color))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(GetIconSymbol(), font, brush, rect, sf);
            }
        }

        private void ModernMessageBox_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Ombre portée
            using (var shadowPath = new GraphicsPath())
            {
                shadowPath.AddRectangle(new Rectangle(5, 5, this.Width - 10, this.Height - 10));
                using (var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                {
                    e.Graphics.FillPath(shadowBrush, shadowPath);
                }
            }

            // Bordure
            using (var pen = new Pen(Color.FromArgb(60, 65, 75), 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        private void AdjustSize()
        {
            if (_lblMessage == null || _contentPanel == null) return;

            // Calculer la hauteur nécessaire pour le message
            int messageHeight = _lblMessage.Height;
            int minContentHeight = 100;
            int contentHeight = Math.Max(minContentHeight, messageHeight + 60);

            // Ajuster la taille totale
            int totalHeight = 50 + contentHeight + 70; // Titre + Contenu + Boutons
            this.Height = Math.Min(totalHeight, this.MaximumSize.Height);
        }

        // Drag pour déplacer la fenétre
        private Point _dragStart;
        private bool _isDragging;

        private void TitlePanel_MouseDown(object? sender, MouseEventArgs e)
        {
            _isDragging = true;
            _dragStart = e.Location;
        }

        private void TitlePanel_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var newLocation = this.Location;
                newLocation.X += e.X - _dragStart.X;
                newLocation.Y += e.Y - _dragStart.Y;
                this.Location = newLocation;
            }
        }

        private void TitlePanel_MouseUp(object? sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        /// <summary>
        /// Affiche une MessageBox moderne
        /// </summary>
        public static DialogResult Show(string message, string title = "WMine", 
            ModernMessageBoxIcon icon = ModernMessageBoxIcon.Information,
            ModernMessageBoxButtons buttons = ModernMessageBoxButtons.OK)
        {
            using var msgBox = new ModernMessageBox(message, title, icon, buttons);
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Affiche un message de succés
        /// </summary>
        public static void ShowSuccess(string message, string title = "Succés")
        {
            Show(message, title, ModernMessageBoxIcon.Success, ModernMessageBoxButtons.OK);
        }

        /// <summary>
        /// Affiche un message d'information
        /// </summary>
        public static void ShowInformation(string message, string title = "Information")
        {
            Show(message, title, ModernMessageBoxIcon.Information, ModernMessageBoxButtons.OK);
        }

        /// <summary>
        /// Affiche un avertissement
        /// </summary>
        public static void ShowWarning(string message, string title = "Attention")
        {
            Show(message, title, ModernMessageBoxIcon.Warning, ModernMessageBoxButtons.OK);
        }

        /// <summary>
        /// Affiche une erreur
        /// </summary>
        public static void ShowError(string message, string title = "Erreur")
        {
            Show(message, title, ModernMessageBoxIcon.Error, ModernMessageBoxButtons.OK);
        }

        /// <summary>
        /// Affiche une question avec Oui/Non
        /// </summary>
        public static bool ShowQuestion(string message, string title = "Confirmation")
        {
            return Show(message, title, ModernMessageBoxIcon.Question, ModernMessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        /// <summary>
        /// Affiche une confirmation
        /// </summary>
        public static bool ShowConfirmation(string message, string title = "Confirmer")
        {
            return Show(message, title, ModernMessageBoxIcon.Question, ModernMessageBoxButtons.OKCancel) == DialogResult.OK;
        }
    }
}


