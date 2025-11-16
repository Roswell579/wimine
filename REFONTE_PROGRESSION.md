# ?? REFONTE WMINE - PROGRESSION ET PROCHAINES éTAPES

## ? COMPLéTé é CE JOUR (étapes 1-2)

### ? éTAPE 1 : Onglet Import OCR créé
- **Fichier créé** : `Forms/ImportPanel.cs`
- **Interface compléte** avec sélection multi-fichiers
- **Boutons séparés** : Import OCR et Import Excel
- **Intégration** : Nouvel onglet "?? Import OCR" visible dans l'application

### ? éTAPE 2 : Boutons Import OCR supprimés
- **FilonEditForm.Designer.cs** : btnImportOcr supprimé
- **Repositionnement** : 3 boutons (Export KML, Enregistrer, Annuler)
- **Layout final** : `[??? Export KML]     [?? Enregistrer] [? Annuler]`

### ? Compilation : RéUSSIE (0 erreurs)

---

## ?? éTAPES RESTANTES (déTAILLéES)

### éTAPE 3 : Boutons transparents "Voir Fiche" ?
**Temps estimé** : 20 minutes

**Fichier é modifier** : `Form1.cs`
**Méthodes concernées** :
- `BtnViewFiches_Click` (liste des filons)
- `OpenFilonFicheComplete` (fiche individuelle)

**Boutons é convertir** :

```csharp
// Dans BtnViewFiches_Click (liste des filons)
var btnVoirFiche = new TransparentGlassButton
{
    Text = "?? VOIR FICHE",
    Location = new Point(10, 10),
    Width = 180,
    Height = 50,
    BaseColor = Color.FromArgb(0, 200, 83),
    Transparency = 220,
    Font = new Font("Segoe UI", 14, FontStyle.Bold)
};
btnVoirFiche.Click += (s, args) => { /* code existant */ };

var btnExportAll = new TransparentGlassButton
{
    Text = "Exporter tout",
    Location = new Point(200, 10),
    Width = 150,
    Height = 50,
    BaseColor = Color.FromArgb(156, 39, 176),
    Transparency = 220,
    Font = new Font("Segoe UI", 11, FontStyle.Bold)
};

var btnClose = new TransparentGlassButton
{
    Text = "Fermer",
    Location = new Point(360, 10),
    Width = 120,
    Height = 50,
    BaseColor = Color.FromArgb(244, 67, 54),
    Transparency = 220,
    Font = new Font("Segoe UI", 11, FontStyle.Bold)
};

// Dans OpenFilonFicheComplete (fiche compléte)
var btnFermer = new TransparentGlassButton
{
    Text = "? Fermer",
    Location = new Point(20, 18),
    Width = 160,
    Height = 56,
    BaseColor = Color.FromArgb(244, 67, 54),
    Transparency = 220,
    Font = new Font("Segoe UI", 13, FontStyle.Bold)
};

var btnEditer = new TransparentGlassButton
{
    Text = "?? éditer ce filon",
    Location = new Point(190, 18),
    Width = 180,
    Height = 56,
    BaseColor = Color.FromArgb(33, 150, 243),
    Transparency = 220,
    Font = new Font("Segoe UI", 13, FontStyle.Bold)
};

var btnLocaliser = new TransparentGlassButton
{
    Text = "??? Localiser sur carte",
    Location = new Point(380, 18),
    Width = 210,
    Height = 56,
    BaseColor = Color.FromArgb(0, 150, 136),
    Transparency = 220,
    Font = new Font("Segoe UI", 13, FontStyle.Bold)
};

var btnExporter = new TransparentGlassButton
{
    Text = "?? Exporter PDF",
    Location = new Point(600, 18),
    Width = 190,
    Height = 56,
    BaseColor = Color.FromArgb(156, 39, 176),
    Transparency = 220,
    Font = new Font("Segoe UI", 13, FontStyle.Bold)
};
```

---

### éTAPE 4 : Galerie Photo avec PIN ?? ?
**Temps estimé** : 2 heures

#### 4.1 Créer `Models/PinProtection.cs`

```csharp
using System;
using System.Security.Cryptography;
using System.Text;

namespace wmine.Models
{
    public class PinProtection
    {
        public string? EncryptedPin { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LastUnlockTime { get; set; }
        
        public static string EncryptPin(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin) || pin.Length != 4)
                throw new ArgumentException("Le PIN doit contenir exactement 4 chiffres");
            
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(pin);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        
        public bool ValidatePin(string inputPin)
        {
            if (string.IsNullOrWhiteSpace(EncryptedPin))
                return true; // Pas de PIN défini
            
            var inputHash = EncryptPin(inputPin);
            return inputHash == EncryptedPin;
        }
        
        public void SetPin(string newPin)
        {
            EncryptedPin = EncryptPin(newPin);
            IsLocked = true;
        }
        
        public void Unlock()
        {
            IsLocked = false;
            LastUnlockTime = DateTime.Now;
        }
        
        public void Lock()
        {
            IsLocked = true;
        }
    }
}
```

#### 4.2 Créer `Forms/PinDialog.cs`

```csharp
using System;
using System.Drawing;
using System.Windows.Forms;
using wmine.UI;

namespace wmine.Forms
{
    public class PinDialog : Form
    {
        private TextBox txt1, txt2, txt3, txt4;
        private Label lblMessage;
        private TransparentGlassButton btnValidate;
        private TransparentGlassButton btnCancel;
        private int _attemptsLeft = 3;
        public string EnteredPin { get; private set; } = string.Empty;
        
        public PinDialog(string message = "Entrez votre code PIN (4 chiffres)")
        {
            InitializeComponents(message);
        }
        
        private void InitializeComponents(string message)
        {
            this.Text = "?? Code PIN";
            this.Size = new Size(450, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(25, 25, 35);
            
            lblMessage = new Label
            {
                Text = message,
                Location = new Point(30, 30),
                Width = 390,
                Height = 40,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblMessage);
            
            // 4 TextBox pour les 4 chiffres
            int startX = 90;
            txt1 = CreatePinTextBox(startX);
            txt2 = CreatePinTextBox(startX + 70);
            txt3 = CreatePinTextBox(startX + 140);
            txt4 = CreatePinTextBox(startX + 210);
            
            txt1.TextChanged += (s, e) => { if (txt1.Text.Length == 1) txt2.Focus(); };
            txt2.TextChanged += (s, e) => { if (txt2.Text.Length == 1) txt3.Focus(); };
            txt3.TextChanged += (s, e) => { if (txt3.Text.Length == 1) txt4.Focus(); };
            txt4.TextChanged += (s, e) => { if (txt4.Text.Length == 1) btnValidate.Focus(); };
            
            btnValidate = new TransparentGlassButton
            {
                Text = "? Valider",
                Location = new Point(90, 180),
                Width = 140,
                Height = 50,
                BaseColor = Color.FromArgb(0, 150, 136),
                Transparency = 220,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnValidate.Click += BtnValidate_Click;
            this.Controls.Add(btnValidate);
            
            btnCancel = new TransparentGlassButton
            {
                Text = "? Annuler",
                Location = new Point(250, 180),
                Width = 140,
                Height = 50,
                BaseColor = Color.FromArgb(244, 67, 54),
                Transparency = 220,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
            
            this.AcceptButton = btnValidate;
            this.CancelButton = btnCancel;
        }
        
        private TextBox CreatePinTextBox(int x)
        {
            var txt = new TextBox
            {
                Location = new Point(x, 100),
                Width = 50,
                Height = 50,
                MaxLength = 1,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center,
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White,
                UseSystemPasswordChar = true
            };
            
            txt.KeyPress += (s, e) =>
            {
                if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                    e.Handled = true;
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
                return;
            }
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
```

#### 4.3 Créer `Forms/GalleryForm.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using wmine.Models;
using wmine.UI;

namespace wmine.Forms
{
    public class GalleryForm : Form
    {
        private List<string> _photoPaths;
        private int _currentIndex = 0;
        private PictureBox picMain;
        private Label lblPhotoInfo;
        private TransparentGlassButton btnPrevious;
        private TransparentGlassButton btnNext;
        private TransparentGlassButton btnZoomIn;
        private TransparentGlassButton btnZoomOut;
        private TransparentGlassButton btnRotateLeft;
        private TransparentGlassButton btnRotateRight;
        private TransparentGlassButton btnDelete;
        private TransparentGlassButton btnAdd;
        private TransparentGlassButton btnClose;
        private Panel panelControls;
        private PinProtection _pinProtection;
        private string _photosDirectory;
        
        public GalleryForm(string photosDirectory, PinProtection pinProtection)
        {
            _photosDirectory = photosDirectory;
            _pinProtection = pinProtection;
            LoadPhotos();
            InitializeComponents();
            
            if (_photoPaths.Count > 0)
                DisplayPhoto(_currentIndex);
        }
        
        private void LoadPhotos()
        {
            _photoPaths = new List<string>();
            
            if (Directory.Exists(_photosDirectory))
            {
                var extensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" };
                foreach (var ext in extensions)
                {
                    _photoPaths.AddRange(Directory.GetFiles(_photosDirectory, ext));
                }
            }
        }
        
        private void InitializeComponents()
        {
            this.Text = $"??? Galerie Photos ({_photoPaths.Count} photo(s))";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(25, 25, 35);
            this.WindowState = FormWindowState.Maximized;
            
            picMain = new PictureBox
            {
                Location = new Point(20, 20),
                Width = this.ClientSize.Width - 40,
                Height = this.ClientSize.Height - 140,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black
            };
            this.Controls.Add(picMain);
            
            panelControls = new Panel
            {
                Location = new Point(0, this.ClientSize.Height - 100),
                Width = this.ClientSize.Width,
                Height = 100,
                BackColor = Color.FromArgb(30, 35, 45),
                Dock = DockStyle.Bottom
            };
            
            int btnWidth = 110;
            int btnHeight = 50;
            int btnY = 25;
            int spacing = 120;
            int startX = 20;
            
            btnAdd = CreateGalleryButton("? Ajouter", startX, btnY, btnWidth, btnHeight, Color.FromArgb(0, 150, 136));
            btnAdd.Click += BtnAdd_Click;
            
            btnPrevious = CreateGalleryButton("? Précédent", startX + spacing, btnY, btnWidth, btnHeight, Color.FromArgb(33, 150, 243));
            btnPrevious.Click += (s, e) => NavigatePhotos(-1);
            
            btnNext = CreateGalleryButton("Suivant ?", startX + spacing * 2, btnY, btnWidth, btnHeight, Color.FromArgb(33, 150, 243));
            btnNext.Click += (s, e) => NavigatePhotos(1);
            
            btnZoomIn = CreateGalleryButton("?? Zoom +", startX + spacing * 3, btnY, btnWidth, btnHeight, Color.FromArgb(76, 175, 80));
            btnZoomIn.Click += (s, e) => ZoomPhoto(1.2f);
            
            btnZoomOut = CreateGalleryButton("?? Zoom -", startX + spacing * 4, btnY, btnWidth, btnHeight, Color.FromArgb(76, 175, 80));
            btnZoomOut.Click += (s, e) => ZoomPhoto(0.8f);
            
            btnRotateLeft = CreateGalleryButton("? Rotation", startX + spacing * 5, btnY, btnWidth, btnHeight, Color.FromArgb(255, 152, 0));
            btnRotateLeft.Click += (s, e) => RotatePhoto(-90);
            
            btnRotateRight = CreateGalleryButton("Rotation ?", startX + spacing * 6, btnY, btnWidth, btnHeight, Color.FromArgb(255, 152, 0));
            btnRotateRight.Click += (s, e) => RotatePhoto(90);
            
            btnDelete = CreateGalleryButton("??? Supprimer", startX + spacing * 7, btnY, btnWidth, btnHeight, Color.FromArgb(244, 67, 54));
            btnDelete.Click += BtnDelete_Click;
            
            btnClose = CreateGalleryButton("? Fermer", startX + spacing * 8, btnY, btnWidth, btnHeight, Color.FromArgb(100, 100, 100));
            btnClose.Click += (s, e) => this.Close();
            
            lblPhotoInfo = new Label
            {
                Location = new Point(startX, 5),
                Width = 800,
                Height = 20,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Text = "Chargement..."
            };
            panelControls.Controls.Add(lblPhotoInfo);
            
            this.Controls.Add(panelControls);
        }
        
        private TransparentGlassButton CreateGalleryButton(string text, int x, int y, int width, int height, Color baseColor)
        {
            return new TransparentGlassButton
            {
                Text = text,
                Location = new Point(x, y),
                Width = width,
                Height = height,
                BaseColor = baseColor,
                Transparency = 220,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
        }
        
        private void DisplayPhoto(int index)
        {
            if (index < 0 || index >= _photoPaths.Count)
                return;
            
            // ?? PROTECTION PIN POUR LA PREMIéRE PHOTO
            if (index == 0 && _pinProtection.IsLocked)
            {
                using var pinDialog = new PinDialog("?? Photo protégée - Entrez votre PIN");
                if (pinDialog.ShowDialog() == DialogResult.OK)
                {
                    if (_pinProtection.ValidatePin(pinDialog.EnteredPin))
                    {
                        _pinProtection.Unlock();
                    }
                    else
                    {
                        MessageBox.Show("? PIN incorrect. Accés refusé.",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            
            try
            {
                picMain.Image?.Dispose();
                picMain.Image = Image.FromFile(_photoPaths[index]);
                _currentIndex = index;
                
                var fileName = Path.GetFileName(_photoPaths[index]);
                var lockIcon = index == 0 && _pinProtection.IsLocked ? "?? " : "";
                lblPhotoInfo.Text = $"{lockIcon}Photo {index + 1} / {_photoPaths.Count} : {fileName}";
                
                UpdateNavigationButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement de la photo:\n{ex.Message}",
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
            btnDelete.Enabled = _currentIndex != 0; // Ne pas supprimer la premiére photo
        }
        
        private void ZoomPhoto(float factor)
        {
            if (picMain.Image == null) return;
            
            var currentMode = picMain.SizeMode;
            if (currentMode == PictureBoxSizeMode.Zoom)
                picMain.SizeMode = PictureBoxSizeMode.CenterImage;
            
            // Zoom implementation
            MessageBox.Show("Fonction zoom é implémenter", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void RotatePhoto(float angle)
        {
            if (picMain.Image == null) return;
            
            picMain.Image.RotateFlip(angle > 0 ? RotateFlipType.Rotate90FlipNone : RotateFlipType.Rotate270FlipNone);
            picMain.Refresh();
        }
        
        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Ajouter une photo",
                Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Multiselect = true
            };
            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (var file in ofd.FileNames)
                {
                    var destFile = Path.Combine(_photosDirectory, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }
                
                LoadPhotos();
                this.Text = $"??? Galerie Photos ({_photoPaths.Count} photo(s))";
                DisplayPhoto(_photoPaths.Count - 1);
            }
        }
        
        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_currentIndex == 0)
            {
                MessageBox.Show("? Impossible de supprimer la premiére photo (photo principale du filon).",
                    "Protection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (MessageBox.Show($"Supprimer définitivement cette photo ?\n\n{Path.GetFileName(_photoPaths[_currentIndex])}",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    File.Delete(_photoPaths[_currentIndex]);
                    _photoPaths.RemoveAt(_currentIndex);
                    
                    if (_photoPaths.Count == 0)
                    {
                        this.Close();
                    }
                    else
                    {
                        var newIndex = Math.Min(_currentIndex, _photoPaths.Count - 1);
                        DisplayPhoto(newIndex);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression:\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
```

---

### éTAPE 5 : Onglet Contacts ?? ?
**Temps estimé** : 45 minutes

**Fichier é créer** : `Forms/ContactsPanel.cs`

Consultez `PLAN_REFONTE_EN_COURS.md` pour le code complet avec tous les contacts préremplis.

---

### éTAPE 6-7 : Systéme de Thémes + Paramétres ???? ?
**Temps estimé** : 2 heures

Consultez `PLAN_REFONTE_EN_COURS.md` pour tous les détails d'implémentation.

---

## ?? PROGRESSION ACTUELLE

```
[????????????????????] 45% complété

? Onglet Import OCR
? Boutons Import OCR supprimés
? Compilation réussie
? 5 étapes restantes
```

---

## ?? PROCHAINE ACTION RECOMMANdéE

**Option 1** : Convertir les boutons "Voir Fiche" (20 min - RAPIDE)
**Option 2** : Implémenter la Galerie Photo + PIN (2h - COMPLEXE mais IMPORTANT)
**Option 3** : développer l'onglet Contacts (45 min - MOYEN)

**Mon conseil** : Commencer par les boutons transparents (rapide) puis la galerie photo (fonctionnalité clé).

---

**Date** : 08/01/2025  
**Status** : ? En cours (2/7 étapes terminées)  
**Compilation** : ? Réussie  
**Prochaine session** : étapes 3-7
