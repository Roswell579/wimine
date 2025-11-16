using wmine.Services;

namespace wmine.Forms
{
    public class ImportPanel : Panel
    {
        private readonly FilonDataService _dataService;
        private Button btnSelectFiles;
        private Button btnImportOcr;
        private Button btnImportExcel;
        private Button btnClearSelection;
        private Label lblTitle;
        private Label lblInfo;
        private ListBox lstSelectedFiles;
        private Label lblFileCount;

        public ImportPanel(FilonDataService dataService)
        {
            _dataService = dataService;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(25, 25, 35);
            this.AutoScroll = true;
            this.Padding = new Padding(40);

            // Titre principal
            lblTitle = new Label
            {
                Text = "Import de Filons",
                Location = new Point(40, 40),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 150, 136)
            };
            this.Controls.Add(lblTitle);

            // Info descriptive
            lblInfo = new Label
            {
                Text = "Importez vos filons depuis des fichiers Excel ou des images scannées (OCR).\n" +
                       "Sélectionnez vos fichiers puis cliquez sur le bouton d'import correspondant.",
                Location = new Point(40, 100),
                Width = 800,
                Height = 60,
                Font = new Font("Segoe UI Emoji", 12),
                ForeColor = Color.FromArgb(180, 180, 180)
            };
            this.Controls.Add(lblInfo);

            // Bouton sélection fichiers
            btnSelectFiles = new Button
            {
                Text = "Sélectionner des fichiers",
                Location = new Point(40, 180),
                Width = 300,
                Height = 60,
                BackColor = Color.FromArgb(33, 150, 243),
                Font = new Font("Segoe UI Emoji", 14, FontStyle.Bold)
            };
            btnSelectFiles.Click += BtnSelectFiles_Click;
            this.Controls.Add(btnSelectFiles);

            // Bouton effacer sélection
            btnClearSelection = new Button
            {
                Text = "Effacer",
                Location = new Point(360, 180),
                Width = 150,
                Height = 60,
                BackColor = Color.FromArgb(244, 67, 54),
                Font = new Font("Segoe UI Emoji", 14, FontStyle.Bold)
            };
            btnClearSelection.Click += (s, e) => ClearFileSelection();
            this.Controls.Add(btnClearSelection);

            // Liste des fichiers sélectionnés
            lstSelectedFiles = new ListBox
            {
                Location = new Point(40, 260),
                Width = 800,
                Height = 300,
                BackColor = Color.FromArgb(35, 40, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 11),
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = SelectionMode.MultiExtended
            };
            this.Controls.Add(lstSelectedFiles);

            // Label compteur de fichiers
            lblFileCount = new Label
            {
                Text = "Aucun fichier sélectionné",
                Location = new Point(40, 570),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 150, 150)
            };
            this.Controls.Add(lblFileCount);

            // Panel pour les boutons d'import
            var panelImportButtons = new Panel
            {
                Location = new Point(40, 610),
                Width = 800,
                Height = 100,
                BackColor = Color.FromArgb(30, 35, 45)
            };

            // Bouton Import OCR
            btnImportOcr = new Button
            {
                Text = "Import OCR (Images)",
                Location = new Point(20, 20),
                Width = 350,
                Height = 60,
                BackColor = Color.FromArgb(255, 152, 0),
                Font = new Font("Segoe UI Emoji", 13, FontStyle.Bold)
            };
            btnImportOcr.Click += BtnImportOcr_Click;
            panelImportButtons.Controls.Add(btnImportOcr);

            // Bouton Import Excel
            btnImportExcel = new Button
            {
                Text = "Import Excel",
                Location = new Point(390, 20),
                Width = 350,
                Height = 60,
                BackColor = Color.FromArgb(0, 150, 136),
                Font = new Font("Segoe UI Emoji", 13, FontStyle.Bold)
            };
            btnImportExcel.Click += BtnImportExcel_Click;
            panelImportButtons.Controls.Add(btnImportExcel);

            this.Controls.Add(panelImportButtons);

            // Séparateur
            var separator = new Panel
            {
                Location = new Point(40, 730),
                Width = 800,
                Height = 2,
                BackColor = Color.FromArgb(60, 65, 75)
            };
            this.Controls.Add(separator);

            // Section données pré-remplies
            var lblPreFill = new Label
            {
                Text = "DONNéES PRé-REMPLIES",
                Location = new Point(40, 750),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 39, 176)
            };
            this.Controls.Add(lblPreFill);

            var lblPreFillInfo = new Label
            {
                Text = "Chargez les données des mines historiques du Var avec coordonnées GPS vérifiées\n" +
                       "Sources : BRGM, Mindat.org, Google Maps, SIG Mines",
                Location = new Point(40, 790),
                Width = 800,
                Height = 50,
                Font = new Font("Segoe UI Emoji", 11),
                ForeColor = Color.FromArgb(180, 180, 180)
            };
            this.Controls.Add(lblPreFillInfo);

            // Bouton charger mines historiques
            var btnLoadHistoricalMines = new Button
            {
                Text = "Charger Mines Historiques du Var (30+ sites)",
                Location = new Point(40, 860),
                Width = 800,
                Height = 70,
                BackColor = Color.FromArgb(156, 39, 176),
                Font = new Font("Segoe UI Emoji", 15, FontStyle.Bold)
            };
            btnLoadHistoricalMines.Click += BtnLoadHistoricalMines_Click;
            this.Controls.Add(btnLoadHistoricalMines);

            // Info supplémentaire
            var lblHelp = new Label
            {
                Text = "Astuce Import fichiers : Vous pouvez sélectionner plusieurs fichiers é la fois.\n" +
                       "Les fichiers Excel (.xlsx, .xls) et images (.jpg, .png, .tiff, .bmp) sont supportés.",
                Location = new Point(40, 950),
                Width = 800,
                Height = 50,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 181, 246)
            };
            this.Controls.Add(lblHelp);
        }

        private void BtnSelectFiles_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Sélectionner des fichiers a importer",
                Filter = "Tous les fichiers supportés|*.xlsx;*.xls;*.jpg;*.jpeg;*.png;*.tiff;*.bmp|" +
                        "Fichiers Excel|*.xlsx;*.xls|" +
                        "Images|*.jpg;*.jpeg;*.png;*.tiff;*.bmp|" +
                        "Tous les fichiers|*.*",
                Multiselect = true
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (var file in ofd.FileNames)
                {
                    if (!lstSelectedFiles.Items.Contains(file))
                    {
                        lstSelectedFiles.Items.Add(file);
                    }
                }
                UpdateFileCount();
            }
        }

        private void ClearFileSelection()
        {
            lstSelectedFiles.Items.Clear();
            UpdateFileCount();
        }

        private void UpdateFileCount()
        {
            int count = lstSelectedFiles.Items.Count;
            lblFileCount.Text = count == 0
                ? "Aucun fichier sélectionné"
                : $"{count} fichier(s) sélectionné(s)";
        }

        private void BtnImportOcr_Click(object? sender, EventArgs e)
        {
            if (lstSelectedFiles.Items.Count == 0)
            {
                MessageBox.Show("Veuillez d'abord sélectionner des fichiers images.",
                    "Aucun fichier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var imageFiles = lstSelectedFiles.Items.Cast<string>()
                .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (imageFiles.Count == 0)
            {
                MessageBox.Show("Aucune image n'a été sélectionnée.\n" +
                              "L'import OCR nécessite des fichiers images (.jpg, .png, .tiff, .bmp)",
                    "Aucune image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var importForm = new OcrImportForm(_dataService);
                // Pré-charger les fichiers sélectionnés
                if (importForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Import OCR terminé avec succés !",
                        "Import réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFileSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'import OCR:\n\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnImportExcel_Click(object? sender, EventArgs e)
        {
            if (lstSelectedFiles.Items.Count == 0)
            {
                MessageBox.Show("Veuillez d'abord sélectionner des fichiers Excel.",
                    "Aucun fichier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var excelFiles = lstSelectedFiles.Items.Cast<string>()
                .Where(f => f.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (excelFiles.Count == 0)
            {
                MessageBox.Show("Aucun fichier Excel n'a été sélectionné.\n" +
                              "L'import Excel nécessite des fichiers .xlsx ou .xls",
                    "Aucun fichier Excel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var importForm = new OcrImportForm(_dataService);
                if (importForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Import Excel terminé avec succés !",
                        "Import réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFileSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'import Excel:\n\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLoadHistoricalMines_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "??Charger les 30+ mines historiques du Var ?\n\n" +
                "Cette action va ajouter :\n" +
                "é Mine du Cap Garonne (Le Pradet)\n" +
                "é Mines de Tanneron (Plomb-Zinc-Fer)\n" +
                "é Gisements de Collobriéres (Grenats, Tourmaline)\n" +
                "é Carriéres d'Estérellite (Saint-Raphaél)\n" +
                "é Et 25+ autres sites\n\n" +
                "Toutes avec coordonnées GPS vérifiées (BRGM, Mindat.org)\n\n" +
                "Les sites déjé présents ne seront pas dupliqués.\n\n" +
                "Continuer ?",
                "Charger Mines Historiques",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                var mines = MinesVarDataService.GetAllMines();
                int addedCount = 0;
                int skippedCount = 0;

                foreach (var mine in mines)
                {
                    // Vérifier si le filon existe déjé (par nom ou coordonnées proches)
                    var existingFilons = _dataService.GetAllFilons();
                    bool exists = existingFilons.Any(f =>
                        f.Nom.Equals(mine.Nom, StringComparison.OrdinalIgnoreCase) ||
                        (f.Latitude.HasValue && f.Longitude.HasValue &&
                         Math.Abs(f.Latitude.Value - mine.Latitude) < 0.001 &&
                         Math.Abs(f.Longitude.Value - mine.Longitude) < 0.001)
                    );

                    if (exists)
                    {
                        skippedCount++;
                        continue;
                    }

                    // Créer le filon
                    var filon = new Models.Filon
                    {
                        Nom = mine.Nom,
                        Latitude = mine.Latitude,
                        Longitude = mine.Longitude,
                        MatierePrincipale = mine.MinéralPrincipal,
                        Notes = $"{mine.Description}\n\n" +
                               $"Commune: {mine.Commune}\n" +
                               $"Période: {mine.PériodeExploitation}\n" +
                               $"Statut: {mine.Statut}\n\n" +
                               $"Source: {mine.Source}",
                        DateCreation = DateTime.Now
                    };

                    _dataService.AddFilon(filon);
                    addedCount++;
                }

                var message = $"Import terminé !\n\n" +
                             $"{addedCount} mines ajoutées\n" +
                             $" {skippedCount} sites déjé présents (ignorés)\n\n" +
                             $"Total : {addedCount + skippedCount} sites traités\n\n" +
                             MinesVarDataService.GetStatistics();

                MessageBox.Show(message, "Import Réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Rafraéchir l'interface parent si possible
                var form1 = this.FindForm() as Form1;
                form1?.RefreshFilonsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de l'import:\n\n{ex.Message}",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}

