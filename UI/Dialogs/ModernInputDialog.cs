using System.Drawing.Drawing2D;

namespace wmine.UI.Dialogs
{
    /// <summary>
    /// Dialog de confirmation moderne avec input optionnel
    /// </summary>
    public class ModernInputDialog : Form
    {
        private readonly string _prompt;
        private readonly string _title;
        private readonly string _defaultValue;
        private TextBox? _txtInput;
        private Label? _lblPrompt;
        
        public string InputValue { get; private set; } = string.Empty;
        public DialogResult Result { get; private set; } = DialogResult.Cancel;

        private ModernInputDialog(string prompt, string title, string defaultValue = "")
        {
            _prompt = prompt;
            _title = title;
            _defaultValue = defaultValue;

            InitializeDialog();
        }

        private void InitializeDialog()
        {
            // Configuration du formulaire
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 35, 45);
            this.Size = new Size(450, 220);
            this.ShowInTaskbar = false;
            this.TopMost = true;

            // Double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                         ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.UserPaint, true);

            // Panel titre
            var titlePanel = CreateTitlePanel();
            
            // Panel contenu
            var contentPanel = CreateContentPanel();
            
            // Panel boutons
            var buttonPanel = CreateButtonPanel();

            this.Controls.Add(titlePanel);
            this.Controls.Add(contentPanel);
            this.Controls.Add(buttonPanel);

            // Drag
            titlePanel.MouseDown += TitlePanel_MouseDown;
            titlePanel.MouseMove += TitlePanel_MouseMove;
            titlePanel.MouseUp += TitlePanel_MouseUp;

            this.Paint += ModernInputDialog_Paint;
        }

        private Panel CreateTitlePanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(0, 150, 136)
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
                Padding = new Padding(30, 20, 30, 20)
            };

            _lblPrompt = new Label
            {
                Text = _prompt,
                Location = new Point(30, 20),
                Width = panel.Width - 60,
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 11),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _txtInput = new TextBox
            {
                Location = new Point(30, 60),
                Width = panel.Width - 60,
                Height = 35,
                Font = new Font("Segoe UI Emoji", 11),
                BackColor = Color.FromArgb(40, 45, 55),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = _defaultValue,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _txtInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    InputValue = _txtInput.Text;
                    Result = DialogResult.OK;
                    this.Close();
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    Result = DialogResult.Cancel;
                    this.Close();
                }
            };

            panel.Controls.Add(_lblPrompt);
            panel.Controls.Add(_txtInput);

            return panel;
        }

        private Panel CreateButtonPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(25, 30, 40),
                Padding = new Padding(20, 15, 20, 15)
            };

            var btnOK = new Button
            {
                Text = "OK",
                Size = new Size(100, 40),
                Location = new Point(panel.Width - 230, 15),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 180, 166);
            btnOK.Click += (s, e) =>
            {
                InputValue = _txtInput?.Text ?? string.Empty;
                Result = DialogResult.OK;
                this.Close();
            };

            var btnCancel = new Button
            {
                Text = "Annuler",
                Size = new Size(100, 40),
                Location = new Point(panel.Width - 120, 15),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(50, 55, 65),
                ForeColor = Color.White
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 65, 75);
            btnCancel.Click += (s, e) =>
            {
                Result = DialogResult.Cancel;
                this.Close();
            };

            panel.Controls.Add(btnOK);
            panel.Controls.Add(btnCancel);

            return panel;
        }

        private void ModernInputDialog_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Ombre
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

        // Drag
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _txtInput?.Focus();
            _txtInput?.SelectAll();
        }

        /// <summary>
        /// Affiche un dialog d'input moderne
        /// </summary>
        public static (bool Success, string Value) Show(string prompt, string title = "Saisir", string defaultValue = "")
        {
            using var dialog = new ModernInputDialog(prompt, title, defaultValue);
            dialog.ShowDialog();
            return (dialog.Result == DialogResult.OK, dialog.InputValue);
        }
    }

    /// <summary>
    /// Dialog de progression moderne
    /// </summary>
    public class ModernProgressDialog : Form
    {
        private ProgressBar? _progressBar;
        private Label? _lblStatus;
        private Label? _lblPercentage;

        public ModernProgressDialog(string title)
        {
            InitializeDialog(title);
        }

        private void InitializeDialog(string title)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 35, 45);
            this.Size = new Size(450, 180);
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.ControlBox = false;

            // Titre
            var titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(0, 150, 136)
            };

            var lblTitle = new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 14, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };

            titlePanel.Controls.Add(lblTitle);

            // Contenu
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 35, 45),
                Padding = new Padding(30)
            };

            _lblStatus = new Label
            {
                Text = "Initialisation...",
                Location = new Point(30, 20),
                Width = 390,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10)
            };

            _progressBar = new ProgressBar
            {
                Location = new Point(30, 50),
                Width = 330,
                Height = 25,
                Style = ProgressBarStyle.Continuous
            };

            _lblPercentage = new Label
            {
                Text = "0%",
                Location = new Point(370, 50),
                Width = 50,
                Height = 25,
                ForeColor = Color.FromArgb(0, 150, 136),
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            contentPanel.Controls.Add(_lblStatus);
            contentPanel.Controls.Add(_progressBar);
            contentPanel.Controls.Add(_lblPercentage);

            this.Controls.Add(titlePanel);
            this.Controls.Add(contentPanel);
        }

        public void UpdateProgress(int percentage, string status)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => UpdateProgress(percentage, status));
                return;
            }

            if (_progressBar != null)
                _progressBar.Value = Math.Min(100, Math.Max(0, percentage));

            if (_lblStatus != null)
                _lblStatus.Text = status;

            if (_lblPercentage != null)
                _lblPercentage.Text = $"{percentage}%";
        }
    }
}

