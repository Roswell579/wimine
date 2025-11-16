using System.Media;
using wmine.UI;

namespace wmine.Forms
{
    /// <summary>
    /// Dialogue pour saisir ou définir un code PIN a 4 chiffres
    /// </summary>
    public class PinDialog : Form
    {
        private TextBox txt1, txt2, txt3, txt4;
        private Label lblMessage;
        private Label lblAttempts;
        private Button btnValidate;
        private Button btnCancel;
        private int _attemptsLeft = 3;
        private bool _isSettingNewPin = false;

        /// <summary>
        /// PIN saisi par l'utilisateur
        /// </summary>
        public string EnteredPin { get; private set; } = string.Empty;

        /// <summary>
        /// Crée un dialogue de saisie PIN
        /// </summary>
        /// <param name="message">Message é afficher</param>
        /// <param name="isSettingNewPin">True si on définit un nouveau PIN, False si on vérifie</param>
        public PinDialog(string message = "Entrez votre code PIN (4 chiffres)", bool isSettingNewPin = false)
        {
            _isSettingNewPin = isSettingNewPin;
            InitializeComponents(message);
        }

        private void InitializeComponents(string message)
        {
            this.Text = _isSettingNewPin ? "définir un code PIN" : "Code PIN requis";
            this.Size = new Size(500, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(25, 25, 35);
            this.KeyPreview = true;

            // Icéne de cadenas
            var lblIcon = new Label
            {
                Text = "??",
                Location = new Point(220, 20),
                Width = 60,
                Height = 60,
                Font = new Font("Segoe UI Emoji", 36),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblIcon);

            // Message principal
            lblMessage = new Label
            {
                Text = message,
                Location = new Point(30, 90),
                Width = 440,
                Height = 40,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblMessage);

            // Compteur de tentatives
            lblAttempts = new Label
            {
                Text = _isSettingNewPin ? " Le PIN sera requis pour voir la premiére photo" : $"?? {_attemptsLeft} tentative(s) restante(s)",
                Location = new Point(30, 130),
                Width = 440,
                Height = 20,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Italic),
                ForeColor = _isSettingNewPin ? Color.FromArgb(255, 152, 0) : Color.FromArgb(244, 67, 54),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblAttempts);

            // 4 TextBox pour les 4 chiffres
            int startX = 110;
            txt1 = CreatePinTextBox(startX);
            txt2 = CreatePinTextBox(startX + 70);
            txt3 = CreatePinTextBox(startX + 140);
            txt4 = CreatePinTextBox(startX + 210);

            // Navigation automatique entre les champs
            txt1.TextChanged += (s, e) => { if (txt1.Text.Length == 1) txt2.Focus(); };
            txt2.TextChanged += (s, e) =>
            {
                if (txt2.Text.Length == 1)
                    txt3.Focus();
                else if (txt2.Text.Length == 0 && string.IsNullOrEmpty(txt3.Text))
                    txt1.Focus();
            };
            txt3.TextChanged += (s, e) =>
            {
                if (txt3.Text.Length == 1)
                    txt4.Focus();
                else if (txt3.Text.Length == 0 && string.IsNullOrEmpty(txt4.Text))
                    txt2.Focus();
            };
            txt4.TextChanged += (s, e) =>
            {
                if (txt4.Text.Length == 1)
                    btnValidate.Focus();
                else if (txt4.Text.Length == 0)
                    txt3.Focus();
            };

            // Bouton Valider
            btnValidate = new TransparentGlassButton
            {
                Text = _isSettingNewPin ? "définir PIN" : "Valider",
                Location = new Point(100, 230),
                Width = 150,
                Height = 50,
                BaseColor = Color.FromArgb(0, 150, 136),
                Transparency = 220,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold)
            };
            btnValidate.Click += BtnValidate_Click;
            this.Controls.Add(btnValidate);

            // Bouton Annuler
            btnCancel = new TransparentGlassButton
            {
                Text = "Annuler",
                Location = new Point(270, 230),
                Width = 150,
                Height = 50,
                BaseColor = Color.FromArgb(244, 67, 54),
                Transparency = 220,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold)
            };
            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnValidate;
            this.CancelButton = btnCancel;

            // Focus sur le premier champ
            this.Shown += (s, e) => txt1.Focus();
        }

        private TextBox CreatePinTextBox(int x)
        {
            var txt = new TextBox
            {
                Location = new Point(x, 160),
                Width = 50,
                Height = 50,
                MaxLength = 1,
                Font = new Font("Segoe UI Emoji", 24, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center,
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            // N'accepter que les chiffres
            txt.KeyPress += (s, e) =>
            {
                if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                {
                    e.Handled = true;
                    SystemSounds.Beep.Play();
                }
            };

            // Effet de focus
            txt.Enter += (s, e) =>
            {
                txt.BackColor = Color.FromArgb(60, 65, 75);
            };
            txt.Leave += (s, e) =>
            {
                txt.BackColor = Color.FromArgb(45, 50, 60);
            };

            this.Controls.Add(txt);
            return txt;
        }

        private void BtnValidate_Click(object? sender, EventArgs e)
        {
            EnteredPin = txt1.Text + txt2.Text + txt3.Text + txt4.Text;

            if (EnteredPin.Length != 4)
            {
                MessageBox.Show("Le PIN doit contenir exactement 4 chiffres.",
                    "PIN invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ClearPinFields();
                txt1.Focus();
                return;
            }

            if (_isSettingNewPin)
            {
                // Mode définition : demander confirmation
                var confirmDialog = new PinDialog("Confirmez votre PIN", false);
                if (confirmDialog.ShowDialog() == DialogResult.OK)
                {
                    if (confirmDialog.EnteredPin == EnteredPin)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Les deux PIN ne correspondent pas.\nVeuillez recommencer.",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ClearPinFields();
                        txt1.Focus();
                    }
                }
            }
            else
            {
                // Mode vérification : valider directement
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void ClearPinFields()
        {
            txt1.Text = string.Empty;
            txt2.Text = string.Empty;
            txt3.Text = string.Empty;
            txt4.Text = string.Empty;
        }

        /// <summary>
        /// décrémente le compteur de tentatives et met é jour l'affichage
        /// </summary>
        /// <returns>True s'il reste des tentatives</returns>
        public bool DecrementAttempts()
        {
            _attemptsLeft--;

            if (_attemptsLeft <= 0)
            {
                lblAttempts.Text = "Nombre de tentatives épuisé !";
                lblAttempts.ForeColor = Color.Red;
                btnValidate.Enabled = false;
                ClearPinFields();
                return false;
            }
            else
            {
                lblAttempts.Text = $"?? {_attemptsLeft} tentative(s) restante(s)";
                ClearPinFields();
                txt1.Focus();
                return true;
            }
        }
    }
}

