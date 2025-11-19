using wmine.Services;

namespace wmine.Forms
{
    public partial class OcrImportForm : Form
    {
        private readonly OcrImportService _ocrService;
        private readonly FilonDataService _dataService;
        private List<FilonImportResult> _importResults = new();
        private string[]? _selectedImagePaths;

        public OcrImportForm(FilonDataService dataService)
        {
            _ocrService = new OcrImportService();
            _dataService = dataService;

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Configuration du formulaire
            Text = "Import de Filons - OCR & Excel";
            Size = new Size(1200, 800);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(25, 25, 35);
            ForeColor = Color.White;

            // Panel principal
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(25, 25, 35)
            };

            // Titre
            var lblTitle = new Label
            {
                Text = "Import de Filons - OCR & Excel",
                Location = new Point(20, 20),
                Width = 1140,
                Height = 40,
                Font = new Font("Segoe UI Emoji", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 181, 246),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Instructions
            var lblInstructions = new Label
            {
                Text = "Importez vos filons depuis:\né Images scannées (OCR) - JPG, PNG, TIFF, BMP\né Fichiers Excel (.xlsx, .xls) - Colonnes: Nom, Lambert X, Lambert Y",
                Location = new Point(20, 70),
                Width = 1140,
                Height = 50,
                Font = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.FromArgb(180, 180, 180),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Bouton sélection fichiers
            var btnSelectFiles = new Button
            {
                Text = "Sélectionner des fichiers (Excel ou Images)",
                Location = new Point(20, 130),
                Width = 350,
                Height = 50,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSelectFiles.FlatAppearance.BorderSize = 0;
            btnSelectFiles.Click += BtnSelectFiles_Click;

            // Bouton lancer OCR
            var btnRunOcr = new Button
            {
                Text = "Lancer l'import",
                Location = new Point(380, 130),
                Width = 260,
                Height = 50,
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false,
                Name = "btnRunOcr"
            };
            btnRunOcr.FlatAppearance.BorderSize = 0;
            btnRunOcr.Click += BtnRunOcr_Click;

            // Bouton scan IA (URL mindat.org)
            var btnScanIa = new Button
            {
                Text = "scan IA",
                Location = new Point(btnRunOcr.Right + 20, btnRunOcr.Top),
                Width = 160,
                Height = 50,
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Name = "btnScanIa"
            };
            btnScanIa.FlatAppearance.BorderSize = 0;
            btnScanIa.Click += (s, e) =>
            {
                using var f = new MineralUrlImportForm();
                f.ShowDialog(this);
            };

            // ListView pour résultats
            var listViewResults = new ListView
            {
                Location = new Point(20, 200),
                Width = 1140,
                Height = 480,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(35, 40, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10),
                MultiSelect = true,
                CheckBoxes = true,
                Name = "listViewResults"
            };

            listViewResults.Columns.Add("", 40);
            listViewResults.Columns.Add("Nom du Filon", 250);
            listViewResults.Columns.Add("Lambert X", 120);
            listViewResults.Columns.Add("Lambert Y", 120);
            listViewResults.Columns.Add("Latitude", 120);
            listViewResults.Columns.Add("Longitude", 120);
            listViewResults.Columns.Add("Statut", 150);
            listViewResults.Columns.Add("Source", 180);

            // Panel de boutons en bas
            var bottomPanel = new Panel
            {
                Location = new Point(20, 690),
                Width = 1140,
                Height = 70,
                BackColor = Color.FromArgb(30, 35, 45)
            };

            var btnImport = new Button
            {
                Text = "Importer les filons sélectionnés",
                Location = new Point(20, 10),
                Width = 280,
                Height = 50,
                BackColor = Color.FromArgb(0, 200, 83),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false,
                Name = "btnImport"
            };
            btnImport.FlatAppearance.BorderSize = 0;
            btnImport.Click += BtnImport_Click;

            var btnCancel = new Button
            {
                Text = "Annuler",
                Location = new Point(320, 10),
                Width = 150,
                Height = 50,
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => Close();

            var lblInfo = new Label
            {
                Text = "Cochez les filons a importer, puis cliquez sur 'Importer'",
                Location = new Point(490, 25),
                Width = 630,
                Height = 20,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 180, 180)
            };

            bottomPanel.Controls.Add(btnImport);
            bottomPanel.Controls.Add(btnCancel);
            bottomPanel.Controls.Add(lblInfo);

            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(lblInstructions);
            mainPanel.Controls.Add(btnSelectFiles);
            mainPanel.Controls.Add(btnRunOcr);
            mainPanel.Controls.Add(btnScanIa);
            mainPanel.Controls.Add(listViewResults);
            mainPanel.Controls.Add(bottomPanel);

            Controls.Add(mainPanel);
        }

        private void BtnSelectFiles_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Sélectionner les fichiers a importer",
                Filter = "Tous les fichiers supportés|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.tif;*.xlsx;*.xls|" +
                         "Images (OCR)|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.tif|" +
                         "Fichiers Excel|*.xlsx;*.xls|" +
                         "Tous les fichiers|*.*",
                Multiselect = true
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _selectedImagePaths = ofd.FileNames;

                var btnRunOcr = Controls.Find("btnRunOcr", true).FirstOrDefault() as Button;
                if (btnRunOcr != null)
                {
                    btnRunOcr.Enabled = true;

                    // Adapter le texte du bouton selon le type de fichier
                    var hasExcel = ofd.FileNames.Any(f => f.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                                                           f.EndsWith(".xls", StringComparison.OrdinalIgnoreCase));
                    var hasImages = ofd.FileNames.Any(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                            f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                                            f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                                                            f.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase));

                    if (hasExcel && hasImages)
                        btnRunOcr.Text = "Lancer l'analyse (OCR + Excel)";
                    else if (hasExcel)
                        btnRunOcr.Text = "Lancer l'import Excel";
                    else
                        btnRunOcr.Text = "Lancer l'analyse OCR";
                }

                MessageBox.Show($"{ofd.FileNames.Length} fichier(s) sélectionné(s)",
                    "Fichiers", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void BtnRunOcr_Click(object? sender, EventArgs e)
        {
            if (_selectedImagePaths == null || _selectedImagePaths.Length == 0)
                return;

            var btnRunOcr = sender as Button;
            if (btnRunOcr == null) return;

            btnRunOcr.Enabled = false;
            var originalText = btnRunOcr.Text;
            btnRunOcr.Text = "Analyse en cours...";

            try
            {
                _importResults = new List<FilonImportResult>();

                // Séparer les fichiers Excel des images
                var excelFiles = _selectedImagePaths.Where(f =>
                    f.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".xls", StringComparison.OrdinalIgnoreCase)).ToArray();

                var imageFiles = _selectedImagePaths.Where(f =>
                    f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".tif", StringComparison.OrdinalIgnoreCase)).ToArray();

                // Traiter les fichiers Excel
                if (excelFiles.Length > 0)
                {
                    var excelService = new ExcelImportService();
                    foreach (var excelFile in excelFiles)
                    {
                        var excelResults = await Task.Run(() => excelService.ImportFromExcel(excelFile));
                        _importResults.AddRange(excelResults);
                    }
                }

                // Traiter les images avec OCR
                if (imageFiles.Length > 0)
                {
                    // Initialiser l'OCR
                    _ocrService.InitializeOcr();

                    // Lancer l'analyse en arriére-plan
                    var ocrResults = await Task.Run(() => _ocrService.ImportFromMultipleImages(imageFiles));
                    _importResults.AddRange(ocrResults);
                }

                // Afficher les résultats
                DisplayResults(_importResults);

                var excelCount = _importResults.Count(r => r.SourceFile?.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) == true ||
                                                            r.SourceFile?.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) == true);
                var ocrCount = _importResults.Count - excelCount;

                MessageBox.Show($"Analyse terminée !\n\n" +
                    $"Filons détectés: {_importResults.Count(r => r.IsValid)}\n" +
                    $"  é Depuis Excel: {_importResults.Count(r => r.IsValid && (r.SourceFile?.EndsWith(\".xlsx\", StringComparison.OrdinalIgnoreCase) == true || r.SourceFile?.EndsWith(\".xls\", StringComparison.OrdinalIgnoreCase) == true))}\n" +
                    $"  é Depuis OCR: {_importResults.Count(r => r.IsValid && !(r.SourceFile?.EndsWith(\".xlsx\", StringComparison.OrdinalIgnoreCase) == true || r.SourceFile?.EndsWith(\".xls\", StringComparison.OrdinalIgnoreCase) == true))}\n\n" +
                    $"Lignes non reconnues: {_importResults.Count(r => !r.IsValid)}",
                    "Résultats", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur:\n\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRunOcr.Enabled = true;
                btnRunOcr.Text = originalText;
            }
        }

        private void DisplayResults(List<FilonImportResult> results)
        {
            var listView = Controls.Find("listViewResults", true).FirstOrDefault() as ListView;
            if (listView == null) return;

            listView.Items.Clear();

            foreach (var result in results)
            {
                var item = new ListViewItem("");
                item.UseItemStyleForSubItems = false;

                if (result.IsValid)
                {
                    item.Checked = true; // Coché par défaut si valide
                    item.SubItems.Add(result.Nom ?? "Sans nom");
                    item.SubItems.Add(result.LambertX?.ToString("F2") ?? "N/A");
                    item.SubItems.Add(result.LambertY?.ToString("F2") ?? "N/A");
                    item.SubItems.Add(result.Latitude?.ToString("F6") ?? "N/A");
                    item.SubItems.Add(result.Longitude?.ToString("F6") ?? "N/A");
                    item.SubItems.Add("Valide");
                    item.SubItems.Add(result.SourceFile ?? "");
                    item.ForeColor = Color.FromArgb(0, 200, 83);
                }
                else
                {
                    item.Checked = false;
                    item.SubItems.Add(result.OriginalLine ?? "Erreur");
                    item.SubItems.Add("-");
                    item.SubItems.Add("-");
                    item.SubItems.Add("-");
                    item.SubItems.Add("-");
                    item.SubItems.Add(result.ErrorMessage ?? "Invalide");
                    item.SubItems.Add(result.SourceFile ?? "");
                    item.ForeColor = Color.FromArgb(244, 67, 54);
                }

                item.Tag = result;
                listView.Items.Add(item);
            }

            // Activer le bouton d'import
            var btnImport = Controls.Find("btnImport", true).FirstOrDefault() as Button;
            if (btnImport != null && results.Any(r => r.IsValid))
            {
                btnImport.Enabled = true;
            }
        }

        private void BtnImport_Click(object? sender, EventArgs e)
        {
            var listView = Controls.Find("listViewResults", true).FirstOrDefault() as ListView;
            if (listView == null) return;

            var selectedResults = listView.CheckedItems.Cast<ListViewItem>()
                .Select(item => item.Tag as FilonImportResult)
                .Where(r => r != null && r.IsValid)
                .ToList();

            if (!selectedResults.Any())
            {
                MessageBox.Show("Aucun filon sélectionné !", "Attention",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Confirmer l'import de {selectedResults.Count} filon(s) ?",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int imported = 0;
                foreach (var filonResult in selectedResults)
                {
                    try
                    {
                        var filon = filonResult!.ToFilon();
                        _dataService.AddFilon(filon);
                        imported++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur import '{filonResult!.Nom}': {ex.Message}",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                MessageBox.Show($"{imported} filon(s) importé(s) avec succés !",
                    "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ocrService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
