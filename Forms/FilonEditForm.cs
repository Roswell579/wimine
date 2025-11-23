using System.Diagnostics;
using System.Globalization;
using wmine.Models;
using wmine.Services;
using wmine.UI;

namespace wmine.Forms
{
    public partial class FilonEditForm : Form
    {
        private Filon _filon;
        private string? _photoPath;
        private string? _documentPath;
        private readonly FilonDataService _dataService;

        public Filon Filon => _filon;

        public FilonEditForm(Filon? filon = null, FilonDataService? dataService = null)
        {
            InitializeComponent();

            _filon = filon ?? new Filon();
            _dataService = dataService ?? new FilonDataService();
            LoadData();
            ApplyGlassEffects();
        }

        private async void ApplyGlassEffects()
        {
            await Task.Delay(100);

            // Effet de flou sur le formulaire
            if (Environment.OSVersion.Version.Major >= 10)
            {
                GlassEffects.EnableBlur(this, 220);
            }

            // Animation d'entrée avec slide
            this.Opacity = 0;
            this.Visible = true;
            await GlassEffects.FadeIn(this, 300);
        }

        private void LoadData()
        {
            // Charger les types de minéraux pour matiére principale
            var minerals = Enum.GetValues<MineralType>()
                .OrderBy(m => MineralColors.GetDisplayName(m))
                .ToList();

            foreach (var mineral in minerals)
            {
                clbMatierePrincipale.Items.Add(mineral);
                clbMatieresSecondaires.Items.Add(mineral);
            }

            // Charger les statuts
            foreach (FilonStatus status in Enum.GetValues<FilonStatus>())
            {
                if (status != FilonStatus.Aucun)
                {
                    clbStatuts.Items.Add(status);
                }
            }

            // Remplir les données du filon
            if (_filon.Id != Guid.Empty)
            {
                txtNom.Text = _filon.Nom;

                // Matiére principale - cocher l'item correspondant
                for (int i = 0; i < clbMatierePrincipale.Items.Count; i++)
                {
                    if ((MineralType)clbMatierePrincipale.Items[i] == _filon.MatierePrincipale)
                    {
                        clbMatierePrincipale.SetItemChecked(i, true);
                        break;
                    }
                }

                // Matiéres secondaires
                for (int i = 0; i < clbMatieresSecondaires.Items.Count; i++)
                {
                    if (_filon.MatieresSecondaires.Contains((MineralType)clbMatieresSecondaires.Items[i]))
                    {
                        clbMatieresSecondaires.SetItemChecked(i, true);
                    }
                }

                // Statuts
                for (int i = 0; i < clbStatuts.Items.Count; i++)
                {
                    var status = (FilonStatus)clbStatuts.Items[i];
                    if (_filon.Statut.HasFlag(status))
                    {
                        clbStatuts.SetItemChecked(i, true);
                    }
                }

                // Coordonnées
                if (_filon.LambertX.HasValue)
                    txtLambertX.Text = _filon.LambertX.Value.ToString("F2");
                if (_filon.LambertY.HasValue)
                    txtLambertY.Text = _filon.LambertY.Value.ToString("F2");

                // Use TryGetWgs84 to safely display GPS coords
                if (_filon.TryGetWgs84(out double lat, out double lon))
                {
                    txtLatitude.Text = lat.ToString("F6");
                    txtLongitude.Text = lon.ToString("F6");
                }

                // Ancrage
                if (_filon.AnneeAncrage.HasValue)
                    txtAnneeAncrage.Text = _filon.AnneeAncrage.Value.ToString();

                // Photo
                if (!string.IsNullOrEmpty(_filon.PhotoPath) && File.Exists(_filon.PhotoPath))
                {
                    _photoPath = _filon.PhotoPath;
                    LoadPhoto(_photoPath);
                }

                // Documentation
                if (!string.IsNullOrEmpty(_filon.DocumentationPath) && File.Exists(_filon.DocumentationPath))
                {
                    _documentPath = _filon.DocumentationPath;
                    UpdateDocPathLabel();
                }

                txtNotes.Text = _filon.Notes;
            }

            // Mise en forme spéciale pour "Danger mortel"
            UpdateDangerMortelFormat();
            clbStatuts.ItemCheck += ClbStatuts_ItemCheck;
        }

        private void UpdateDocPathLabel()
        {
            if (!string.IsNullOrEmpty(_documentPath) && File.Exists(_documentPath))
            {
                lblDocPath.Text = $"?? {Path.GetFileName(_documentPath)}";
                lblDocPath.ForeColor = Color.Cyan;
                lblDocPath.Font = new Font(lblDocPath.Font, FontStyle.Underline | FontStyle.Bold);
                tooltipPdf.SetToolTip(lblDocPath, $"Cliquez pour ouvrir:\n{Path.GetFileName(_documentPath)}");
            }
            else
            {
                lblDocPath.Text = "Aucun document";
                lblDocPath.ForeColor = Color.Gray;
                lblDocPath.Font = new Font(lblDocPath.Font, FontStyle.Italic);
                tooltipPdf.SetToolTip(lblDocPath, "Aucun document PDF");
            }
        }

        private void LblDocPath_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_documentPath) && File.Exists(_documentPath))
            {
                try
                {
                    // Ouvrir le PDF avec l'application par défaut
                    var psi = new ProcessStartInfo
                    {
                        FileName = _documentPath,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Impossible d'ouvrir le document:\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Aucun document PDF associé é ce filon.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ClbMatierePrincipale_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            // Assurer qu'une seule matiére principale est sélectionnée (comportement radio button)
            if (e.NewValue == CheckState.Checked)
            {
                for (int i = 0; i < clbMatierePrincipale.Items.Count; i++)
                {
                    if (i != e.Index && clbMatierePrincipale.GetItemChecked(i))
                    {
                        clbMatierePrincipale.SetItemChecked(i, false);
                    }
                }
            }
        }

        private void ClbStatuts_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            // Forcer la couleur rouge pour "Danger mortel"
            BeginInvoke(new Action(UpdateDangerMortelFormat));
        }

        private void UpdateDangerMortelFormat()
        {
            // Le CheckedListBox ne supporte pas facilement la coloration individuelle
            // Cette méthode est préte pour une future amélioration visuelle
        }

        private void BtnConvertLambert_Click(object? sender, EventArgs e)
        {
            if (double.TryParse(txtLambertX.Text, out double lambertX) &&
                double.TryParse(txtLambertY.Text, out double lambertY))
            {
                // Afficher les coordonnées normalisées pour information
                string normalizedInfo = CoordinateConverter.GetNormalizedCoordinatesInfo(lambertX, lambertY);

                if (!CoordinateConverter.IsValidLambert3Var(lambertX, lambertY))
                {
                    var result = MessageBox.Show(
                        "Les coordonnées Lambert 3 Zone Sud semblent incorrectes pour la région du Var/Alpes-Maritimes.\n\n" +
                        normalizedInfo + "\n\n" +
                        "Voulez-vous continuer la conversion quand méme?",
                        "Attention - Coordonnées hors zone",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.No)
                        return;
                }

                var (latitude, longitude) = CoordinateConverter.Lambert3ToWGS84(lambertX, lambertY);

                txtLatitude.Text = latitude.ToString("F6");
                txtLongitude.Text = longitude.ToString("F6");

                // Vérifier si les coordonnées GPS sont cohérentes
                if (CoordinateConverter.IsValidWGS84Var(latitude, longitude))
                {
                    MessageBox.Show(
                        $"? Conversion effectuée avec succés!\n\n" +
                        $"GPS: {latitude:F6}éN, {longitude:F6}éE\n\n" +
                        normalizedInfo,
                        "Conversion réussie",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        $"?? Conversion effectuée, mais les coordonnées GPS résultantes\n" +
                        $"ne correspondent pas é la région Var/Alpes-Maritimes.\n\n" +
                        $"GPS: {latitude:F6}éN, {longitude:F6}éE\n\n" +
                        $"Vérifiez les valeurs Lambert saisies.\n\n" +
                        normalizedInfo,
                        "Attention - Résultat inhabituel",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            else
            {
                MessageBox.Show(
                    "?? Veuillez saisir des valeurs numériques valides pour les coordonnées Lambert.\n\n" +
                    "Formats acceptés:\n" +
                    "é Métres: 969260 / 3144460\n" +
                    "é Kilométres: 969.26 / 144.46 (le préfixe '3' pour Y est ajouté automatiquement)",
                    "Erreur de saisie",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void BtnChoosePhoto_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Choisir une photo",
                Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Tous les fichiers|*.*"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _photoPath = ofd.FileName;
                LoadPhoto(_photoPath);
            }
        }

        private void LoadPhoto(string path)
        {
            try
            {
                // Charger l'image sans verrouiller le fichier
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    picPhoto.Image = Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement de la photo: {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRemovePhoto_Click(object? sender, EventArgs e)
        {
            picPhoto.Image = null;
            _photoPath = null;
        }

        private void BtnChooseDoc_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Choisir un document PDF",
                Filter = "Documents PDF|*.pdf|Tous les fichiers|*.*"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _documentPath = ofd.FileName;
                UpdateDocPathLabel();
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                MessageBox.Show("?? Veuillez saisir un nom pour le filon.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNom.Focus();
                return;
            }

            // Vérifier qu'une matiére principale est sélectionnée
            if (clbMatierePrincipale.CheckedItems.Count == 0)
            {
                MessageBox.Show("?? Veuillez sélectionner une matiére principale.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Sauvegarder les données
            _filon.Nom = txtNom.Text.Trim();
            _filon.MatierePrincipale = (MineralType)clbMatierePrincipale.CheckedItems[0]!;

            // Matiéres secondaires
            _filon.MatieresSecondaires.Clear();
            foreach (MineralType item in clbMatieresSecondaires.CheckedItems)
            {
                _filon.MatieresSecondaires.Add(item);
            }

            // Statuts
            _filon.Statut = FilonStatus.Aucun;
            foreach (var item in clbStatuts.CheckedItems)
            {
                _filon.Statut |= (FilonStatus)item;
            }

            // Coordonnées - Parser les valeurs saisies
            if (double.TryParse(txtLambertX.Text, out double lambertX))
                _filon.LambertX = lambertX;
            else
                _filon.LambertX = null;

            if (double.TryParse(txtLambertY.Text, out double lambertY))
                _filon.LambertY = lambertY;
            else
                _filon.LambertY = null;

            if (double.TryParse(txtLatitude.Text, out double latitude))
                _filon.Latitude = latitude;
            else
                _filon.Latitude = null;

            if (double.TryParse(txtLongitude.Text, out double longitude))
                _filon.Longitude = longitude;
            else
                _filon.Longitude = null;

            // Normaliser et synchroniser les coordonnées
            _filon.NormalizeAndSyncCoordinates();

            // Vérifier la cohérence des coordonnées
            if (_filon.HasCoordinates() && !_filon.AreCoordinatesValid())
            {
                var result = MessageBox.Show(
                    "?? Attention: Les coordonnées saisies ne correspondent pas é la région Var/Alpes-Maritimes.\n\n" +
                    "Voulez-vous sauvegarder quand méme?",
                    "Coordonnées hors zone",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                    return;
            }

            // Ancrage
            if (int.TryParse(txtAnneeAncrage.Text, out int annee))
                _filon.AnneeAncrage = annee;
            else
                _filon.AnneeAncrage = null;

            _filon.PhotoPath = _photoPath;
            _filon.DocumentationPath = _documentPath;
            _filon.Notes = txtNotes.Text.Trim();

            // Mettre é jour la date de modification
            _filon.DateModification = DateTime.Now;

            this.DialogResult = DialogResult.OK;
        }

        private void BtnImportOcr_Click(object? sender, EventArgs e)
        {
            try
            {
                using var importForm = new OcrImportForm(_dataService);
                if (importForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("? Import OCR terminé avec succés !\n\n" +
                        "Les filons ont été ajoutés é la base de données.",
                        "Import réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (FileNotFoundException fnfEx)
            {
                MessageBox.Show(fnfEx.Message,
                    "Données OCR manquantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"? Erreur lors de l'import OCR:\n\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPhotoGallery_Click(object? sender, EventArgs e)
        {
            // Créer le dossier de photos pour ce filon si nécessaire
            string photosDirectory;
            
            if (!string.IsNullOrEmpty(_filon.PhotoPath) && Directory.Exists(_filon.PhotoPath))
            {
                // Si PhotoPath pointe vers un dossier, l'utiliser
                photosDirectory = _filon.PhotoPath;
            }
            else if (!string.IsNullOrEmpty(_filon.PhotoPath) && File.Exists(_filon.PhotoPath))
            {
                // Si PhotoPath est un fichier, utiliser son dossier parent
                photosDirectory = Path.GetDirectoryName(_filon.PhotoPath) ?? 
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), 
                    "WMine", "Filons", _filon.Id.ToString());
            }
            else
            {
                // Créer un nouveau dossier pour ce filon
                photosDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    "WMine", "Filons", _filon.Id.ToString());
            }
            
            // Créer le dossier s'il n'existe pas
            if (!Directory.Exists(photosDirectory))
            {
                try
                {
                    Directory.CreateDirectory(photosDirectory);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Impossible de créer le dossier de photos:\n{ex.Message}",
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }
            
            // Créer un objet PinProtection pour ce filon
            // Note: Dans une version future, stocker le PIN dans le filon lui-méme
            var pinProtection = new PinProtection();
            
            // Ouvrir la galerie
            try
            {
                using var galleryForm = new GalleryForm(photosDirectory, pinProtection);
                galleryForm.ShowDialog(this);
                
                // Rafraéchir l'affichage de la photo principale si nécessaire
                if (!string.IsNullOrEmpty(_photoPath) && File.Exists(_photoPath))
                {
                    LoadPhoto(_photoPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de l'ouverture de la galerie:\n{ex.Message}",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnExportKml_Click(object? sender, EventArgs e)
        {
            if (Filon == null)
            {
                MessageBox.Show(
                    "Aucun filon é exporter.",
                    "Export KML",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if ((!Filon.Latitude.HasValue || Filon.Latitude == 0) && (!Filon.Longitude.HasValue || Filon.Longitude == 0))
            {
                MessageBox.Show(
                    "Le filon ne posséde pas de coordonnées GPS valides.",
                    "Export KML",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Fichier KML (*.kml)|*.kml",
                    FileName = $"{Filon.Nom.Replace(" ", "_")}_Export.kml",
                    Title = "Exporter en KML"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string kml = GenerateKml(Filon);
                    System.IO.File.WriteAllText(saveDialog.FileName, kml, System.Text.Encoding.UTF8);

                    MessageBox.Show(
                        "? Export KML réussi !",
                        "Export KML",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"? Erreur : {ex.Message}",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private string GenerateKml(Filon filon)
        {
            string minPrinc = filon.MatierePrincipale.ToString();
            string statuts = filon.Statut.ToString();

            // Use TryGetWgs84 to safely obtain coordinates
            string coordLon = "0";
            string coordLat = "0";
            if (filon.TryGetWgs84(out double gLat, out double gLon))
            {
                coordLat = gLat.ToString("F6", CultureInfo.InvariantCulture);
                coordLon = gLon.ToString("F6", CultureInfo.InvariantCulture);
            }

            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<kml xmlns=""http://www.opengis.net/kml/2.2"">
  <Document>
    <name>{filon.Nom}</name>
    <Placemark>
      <name>{filon.Nom}</name>
      <description>
Matiére: {minPrinc}
Statuts: {statuts}
Ancrage: {filon.AnneeAncrage?.ToString() ?? "Inconnu"}
      </description>
      <Point>
        <coordinates>{coordLon},{coordLat},0</coordinates>
      </Point>
    </Placemark>
  </Document>
</kml>";
        }
    }
}
