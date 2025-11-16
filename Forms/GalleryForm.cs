using wmine.Models;
using wmine.UI;

namespace wmine.Forms
{
    /// <summary>
    /// Galerie de photos avec protection PIN pour la premiére photo
    /// </summary>
    public class GalleryForm : Form
    {
        private List<string> _photoPaths;
        private int _currentIndex = 0;
        private PictureBox picMain;
        private Label lblPhotoInfo;
        private Button btnPrevious;
        private Button btnNext;
        private Button btnZoomIn;
        private Button btnZoomOut;
        private Button btnRotateLeft;
        private Button btnRotateRight;
        private Button btnDelete;
        private Button btnAdd;
        private Button btnSetPin;
        private Button btnClose;
        private Panel panelControls;
        private PinProtection _pinProtection;
        private string _photosDirectory;
        private float _zoomFactor = 1.0f;

        public GalleryForm(string photosDirectory, PinProtection pinProtection)
        {
            _photosDirectory = photosDirectory;
            _pinProtection = pinProtection ?? new PinProtection();
            LoadPhotos();
            InitializeComponents();

            if (_photoPaths.Count > 0)
                DisplayPhoto(_currentIndex);
        }

        private void LoadPhotos()
        {
            _photoPaths = new List<string>();

            if (string.IsNullOrWhiteSpace(_photosDirectory))
                return;

            // Si c'est un fichier, prendre le dossier parent
            if (File.Exists(_photosDirectory))
                _photosDirectory = Path.GetDirectoryName(_photosDirectory) ?? _photosDirectory;

            if (Directory.Exists(_photosDirectory))
            {
                var extensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.tiff" };
                foreach (var ext in extensions)
                {
                    _photoPaths.AddRange(Directory.GetFiles(_photosDirectory, ext, SearchOption.TopDirectoryOnly));
                }

                // Trier par date de modification
                _photoPaths = _photoPaths.OrderBy(f => File.GetLastWriteTime(f)).ToList();
            }
        }

        private void InitializeComponents()
        {
            this.Text = $"Galerie Photos ({_photoPaths.Count} photo(s))";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(25, 25, 35);
            this.WindowState = FormWindowState.Maximized;
            this.KeyPreview = true;

            // Raccourcis clavier
            this.KeyDown += (s, e) =>
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        NavigatePhotos(-1);
                        break;
                    case Keys.Right:
                        NavigatePhotos(1);
                        break;
                    case Keys.Escape:
                        this.Close();
                        break;
                }
            };

            // PictureBox principale
            picMain = new PictureBox
            {
                Location = new Point(20, 20),
                Width = this.ClientSize.Width - 40,
                Height = this.ClientSize.Height - 140,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Double-clic pour zoom automatique
            picMain.DoubleClick += (s, e) =>
            {
                picMain.SizeMode = picMain.SizeMode == PictureBoxSizeMode.Zoom
                    ? PictureBoxSizeMode.CenterImage
                    : PictureBoxSizeMode.Zoom;
            };

            this.Controls.Add(picMain);

            // Panel de contréles
            panelControls = new Panel
            {
                Location = new Point(0, this.ClientSize.Height - 100),
                Width = this.ClientSize.Width,
                Height = 100,
                BackColor = Color.FromArgb(30, 35, 45),
                Dock = DockStyle.Bottom
            };

            int btnWidth = 120;
            int btnHeight = 50;
            int btnY = 25;
            int spacing = 130;
            int startX = 20;

            // Bouton Ajouter
            btnAdd = CreateGalleryButton("Ajouter", startX, btnY, btnWidth, btnHeight, Color.FromArgb(0, 150, 136));
            btnAdd.Click += BtnAdd_Click;

            // Bouton Précédent
            btnPrevious = CreateGalleryButton("Précédent", startX + spacing, btnY, btnWidth, btnHeight, Color.FromArgb(33, 150, 243));
            btnPrevious.Click += (s, e) => NavigatePhotos(-1);

            // Bouton Suivant
            btnNext = CreateGalleryButton("Suivant", startX + spacing * 2, btnY, btnWidth, btnHeight, Color.FromArgb(33, 150, 243));
            btnNext.Click += (s, e) => NavigatePhotos(1);

            // Bouton Zoom +
            btnZoomIn = CreateGalleryButton("Zoom +", startX + spacing * 3, btnY, btnWidth, btnHeight, Color.FromArgb(76, 175, 80));
            btnZoomIn.Click += (s, e) => ZoomPhoto(1.2f);

            // Bouton Zoom -
            btnZoomOut = CreateGalleryButton("Zoom -", startX + spacing * 4, btnY, btnWidth, btnHeight, Color.FromArgb(76, 175, 80));
            btnZoomOut.Click += (s, e) => ZoomPhoto(0.8f);

            // Bouton Rotation gauche
            btnRotateLeft = CreateGalleryButton("Rotation", startX + spacing * 5, btnY, btnWidth, btnHeight, Color.FromArgb(255, 152, 0));
            btnRotateLeft.Click += (s, e) => RotatePhoto(-90);

            // Bouton Rotation droite
            btnRotateRight = CreateGalleryButton("Rotation", startX + spacing * 6, btnY, btnWidth, btnHeight, Color.FromArgb(255, 152, 0));
            btnRotateRight.Click += (s, e) => RotatePhoto(90);

            // Bouton PIN
            btnSetPin = CreateGalleryButton(_pinProtection.HasPin() ? "Changer PIN" : "définir PIN",
                startX + spacing * 7, btnY, btnWidth, btnHeight, Color.FromArgb(156, 39, 176));
            btnSetPin.Click += BtnSetPin_Click;

            // Bouton Supprimer
            btnDelete = CreateGalleryButton("Supprimer", startX + spacing * 8, btnY, btnWidth, btnHeight, Color.FromArgb(244, 67, 54));
            btnDelete.Click += BtnDelete_Click;

            // Bouton Fermer
            btnClose = CreateGalleryButton("Fermer", startX + spacing * 9, btnY, btnWidth, btnHeight, Color.FromArgb(100, 100, 100));
            btnClose.Click += (s, e) => this.Close();

            // Label info photo
            lblPhotoInfo = new Label
            {
                Location = new Point(startX, 5),
                Width = this.ClientSize.Width - 40,
                Height = 20,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Text = "Chargement...",
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelControls.Controls.Add(lblPhotoInfo);

            this.Controls.Add(panelControls);
        }

        private TransparentGlassButton CreateGalleryButton(string text, int x, int y, int width, int height, Color baseColor)
        {
            var btn = new TransparentGlassButton
            {
                Text = text,
                Location = new Point(x, y),
                Width = width,
                Height = height,
                BaseColor = baseColor,
                Transparency = 220,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold)
            };
            panelControls.Controls.Add(btn);
            return btn;
        }

        private void DisplayPhoto(int index)
        {
            if (index < 0 || index >= _photoPaths.Count)
                return;

            // PROTECTION PIN POUR LA PREMIéRE PHOTO
            if (index == 0 && _pinProtection.HasPin() && _pinProtection.IsLocked)
            {
                using var pinDialog = new PinDialog("Photo protégée - Entrez votre PIN");
                var attempts = 3;

                while (attempts > 0)
                {
                    if (pinDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (_pinProtection.ValidatePin(pinDialog.EnteredPin))
                        {
                            _pinProtection.Unlock();
                            break;
                        }
                        else
                        {
                            attempts--;
                            if (attempts > 0)
                            {
                                pinDialog.DecrementAttempts();
                                MessageBox.Show($" PIN incorrect.\n\n{attempts} tentative(s) restante(s).",
                                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                MessageBox.Show("Nombre de tentatives épuisé.\nAccés refusé de la photo protégée.",
                                    "Accés refusé", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                this.Close();
                                return;
                            }
                        }
                    }
                    else
                    {
                        this.Close();
                        return;
                    }
                }
            }

            try
            {
                // Libérer l'image précédente
                if (picMain.Image != null)
                {
                    var oldImage = picMain.Image;
                    picMain.Image = null;
                    oldImage.Dispose();
                }

                // Charger la nouvelle image
                using (var fs = new FileStream(_photoPaths[index], FileMode.Open, FileAccess.Read))
                {
                    picMain.Image = Image.FromStream(fs);
                }

                _currentIndex = index;
                _zoomFactor = 1.0f;

                var fileName = Path.GetFileName(_photoPaths[index]);
                var fileSize = new FileInfo(_photoPaths[index]).Length / 1024; // Ko
                var lockIcon = index == 0 && _pinProtection.HasPin() ? "?? " : "";

                lblPhotoInfo.Text = $"{lockIcon}Photo {index + 1} / {_photoPaths.Count} : {fileName} ({fileSize} Ko)";

                UpdateNavigationButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement de la photo:\n\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NavigatePhotos(int direction)
        {
            var newIndex = _currentIndex + direction;
            if (newIndex >= 0 && newIndex < _photoPaths.Count)
            {
                DisplayPhoto(newIndex);
            }
        }

        private void UpdateNavigationButtons()
        {
            btnPrevious.Enabled = _currentIndex > 0;
            btnNext.Enabled = _currentIndex < _photoPaths.Count - 1;
            btnDelete.Enabled = _currentIndex != 0 || !_pinProtection.HasPin(); // Protéger la premiére photo si PIN actif
        }

        private void ZoomPhoto(float factor)
        {
            if (picMain.Image == null) return;

            _zoomFactor *= factor;
            _zoomFactor = Math.Max(0.1f, Math.Min(_zoomFactor, 5.0f)); // Limites : 10% é 500%

            if (picMain.SizeMode == PictureBoxSizeMode.Zoom)
                picMain.SizeMode = PictureBoxSizeMode.CenterImage;

            var newWidth = (int)(picMain.Image.Width * _zoomFactor);
            var newHeight = (int)(picMain.Image.Height * _zoomFactor);

            var resized = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(picMain.Image, 0, 0, newWidth, newHeight);
            }

            var oldImage = picMain.Image;
            picMain.Image = resized;

            lblPhotoInfo.Text += $" [Zoom: {_zoomFactor:P0}]";
        }

        private void RotatePhoto(float angle)
        {
            if (picMain.Image == null) return;

            var rotateType = angle > 0 ? RotateFlipType.Rotate90FlipNone : RotateFlipType.Rotate270FlipNone;
            picMain.Image.RotateFlip(rotateType);
            picMain.Refresh();

            // Sauvegarder la rotation si souhaité
            if (MessageBox.Show("Sauvegarder cette rotation dans le fichier ?",
                "Sauvegarder", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    picMain.Image.Save(_photoPaths[_currentIndex]);
                    MessageBox.Show("Rotation sauvegardée !", "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la sauvegarde:\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Ajouter une ou plusieurs photos",
                Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff",
                Multiselect = true
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (!Directory.Exists(_photosDirectory))
                        Directory.CreateDirectory(_photosDirectory);

                    foreach (var file in ofd.FileNames)
                    {
                        var destFile = Path.Combine(_photosDirectory, Path.GetFileName(file));

                        // éviter les doublons
                        if (File.Exists(destFile))
                        {
                            var fileName = Path.GetFileNameWithoutExtension(file);
                            var extension = Path.GetExtension(file);
                            destFile = Path.Combine(_photosDirectory, $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}{extension}");
                        }

                        File.Copy(file, destFile, false);
                    }

                    LoadPhotos();
                    this.Text = $"Galerie Photos ({_photoPaths.Count} photo(s))";

                    if (_photoPaths.Count > 0)
                        DisplayPhoto(_photoPaths.Count - 1); // Afficher la derniére photo ajoutée

                    MessageBox.Show($"? {ofd.FileNames.Length} photo(s) ajoutée(s) !",
                        "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ajout:\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            // Protection : ne pas supprimer la premiére photo si PIN actif
            if (_currentIndex == 0 && _pinProtection.HasPin())
            {
                MessageBox.Show("Impossible de supprimer la photo protégée par PIN.\n\n" +
                              "Supprimez d'abord le PIN dans les paramétres du filon.",
                    "Protection active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_currentIndex < 0 || _currentIndex >= _photoPaths.Count)
                return;

            var fileName = Path.GetFileName(_photoPaths[_currentIndex]);

            if (MessageBox.Show($"Supprimer définitivement cette photo ?\n\n{fileName}\n\n" +
                              "Cette action est irréversible !",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    File.Delete(_photoPaths[_currentIndex]);
                    _photoPaths.RemoveAt(_currentIndex);
                    this.Text = $"Galerie Photos ({_photoPaths.Count} photo(s))";

                    if (_photoPaths.Count == 0)
                    {
                        MessageBox.Show("Photo supprimée.\n\nLa galerie est maintenant vide.",
                            "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else
                    {
                        var newIndex = Math.Min(_currentIndex, _photoPaths.Count - 1);
                        DisplayPhoto(newIndex);
                        MessageBox.Show("Photo supprimée !", "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression:\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSetPin_Click(object? sender, EventArgs e)
        {
            if (_pinProtection.HasPin())
            {
                // Modifier ou supprimer le PIN existant
                var choice = MessageBox.Show("Un PIN est déjé défini pour la premiére photo.\n\n" +
                                           "Voulez-vous le modifier (Oui) ou le supprimer (Non) ?",
                    "PIN existant", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (choice == DialogResult.Yes)
                {
                    // Modifier le PIN
                    using var pinDialog = new PinDialog("Entrez le PIN actuel pour le modifier");
                    if (pinDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (_pinProtection.ValidatePin(pinDialog.EnteredPin))
                        {
                            using var newPinDialog = new PinDialog("définissez le nouveau PIN", true);
                            if (newPinDialog.ShowDialog() == DialogResult.OK)
                            {
                                _pinProtection.SetPin(newPinDialog.EnteredPin);
                                btnSetPin.Text = "Changer PIN";
                                MessageBox.Show("PIN modifié avec succés !",
                                    "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            MessageBox.Show("PIN incorrect.\nImpossible de modifier le PIN.",
                                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else if (choice == DialogResult.No)
                {
                    // Supprimer le PIN
                    using var pinDialog = new PinDialog("Entrez le PIN actuel pour le supprimer");
                    if (pinDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (_pinProtection.ValidatePin(pinDialog.EnteredPin))
                        {
                            _pinProtection.RemovePin();
                            btnSetPin.Text = "définir PIN";
                            MessageBox.Show("PIN supprimé.\nLa premiére photo n'est plus protégée.",
                                "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Rafraéchir l'affichage
                            if (_currentIndex == 0)
                                DisplayPhoto(0);
                        }
                        else
                        {
                            MessageBox.Show("PIN incorrect.\nImpossible de supprimer le PIN.",
                                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                // définir un nouveau PIN
                using var pinDialog = new PinDialog("définissez un code PIN à 4 chiffres pour protéger la premiére photo", true);
                if (pinDialog.ShowDialog() == DialogResult.OK)
                {
                    _pinProtection.SetPin(pinDialog.EnteredPin);
                    btnSetPin.Text = "Changer PIN";
                    MessageBox.Show("PIN défini avec succés !\n\n" +
                                  "La premiére photo est maintenant protégée.\n" +
                                  "Le PIN sera demandé é chaque ouverture de la galerie.",
                        "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Rafraéchir l'affichage
                    if (_currentIndex == 0)
                        DisplayPhoto(0);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Libérer l'image
            if (picMain.Image != null)
            {
                picMain.Image.Dispose();
                picMain.Image = null;
            }

            base.OnFormClosing(e);
        }
    }
}

