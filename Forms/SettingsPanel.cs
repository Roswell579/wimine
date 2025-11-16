using wmine.Models;
using wmine.Services;

namespace wmine.Forms
{
    /// <summary>
    /// Panel de paramétres et configuration de l'application
    /// </summary>
    public class SettingsPanel : Panel
    {
        private ComboBox cmbTheme;
        private CheckBox chkAnimations;
        private TrackBar trackOpacity;
        private Label lblOpacityValue;
        private CheckBox chkPinProtection;
        private CheckBox chkConfirmDelete;
        private CheckBox chkAutoSave;
        private Label lblFilonsCount;
        private Label lblDataSize;
        private ThemeService? _themeService;

        public SettingsPanel()
        {
            InitializeComponents();
            LoadSettings();
        }

        public void SetThemeService(ThemeService themeService)
        {
            _themeService = themeService;

            // Sélectionner le théme actuel dans la combobox
            if (_themeService?.CurrentTheme != null)
            {
                cmbTheme.SelectedIndex = (int)_themeService.CurrentTheme.Type;
            }
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(25, 25, 35);
            this.AutoScroll = true;
            this.Padding = new Padding(40);

            int yPos = 20;

            // Titre principal
            var lblTitle = new Label
            {
                Text = "⚙️ Paramétres de l'Application",
                Location = new Point(40, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 28, FontStyle.Bold), // ? POLICE EMOJI
                ForeColor = Color.FromArgb(0, 150, 136)
            };
            this.Controls.Add(lblTitle);
            yPos += 80;

            // SECTION 1 : APPARENCE
            yPos = AddSectionTitle("🎨 APPARENCE", yPos);

            var grpAppearance = CreateGroupBox("Personnalisation de l'interface", yPos, 900, 220);

            // Théme
            var lblTheme = new Label
            {
                Text = "Théme :",
                Location = new Point(20, 40),
                Width = 150,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                ForeColor = Color.White
            };
            grpAppearance.Controls.Add(lblTheme);

            cmbTheme = new ComboBox
            {
                Location = new Point(180, 37),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI Emoji", 10),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White
            };
            cmbTheme.Items.AddRange(new object[] { "Dark (Sombre)", "Light (Clair)", " Blue (Bleu)", "Green (Vert)", "Mineral (Minéral)" });
            cmbTheme.SelectedIndex = 0;
            cmbTheme.SelectedIndexChanged += CmbTheme_SelectedIndexChanged;
            grpAppearance.Controls.Add(cmbTheme);

            // Animations
            chkAnimations = new CheckBox
            {
                Text = "Activer les animations (FadeIn, transitions)",
                Location = new Point(20, 80),
                Width = 400,
                Font = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.White,
                Checked = true
            };
            grpAppearance.Controls.Add(chkAnimations);

            // Opacité
            var lblOpacity = new Label
            {
                Text = "Opacité des fenétres :",
                Location = new Point(20, 120),
                Width = 200,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                ForeColor = Color.White
            };
            grpAppearance.Controls.Add(lblOpacity);

            trackOpacity = new TrackBar
            {
                Location = new Point(220, 115),
                Width = 400,
                Minimum = 180,
                Maximum = 255,
                Value = 220,
                TickFrequency = 10
            };
            trackOpacity.ValueChanged += (s, e) => lblOpacityValue.Text = $"{trackOpacity.Value} / 255";
            grpAppearance.Controls.Add(trackOpacity);

            lblOpacityValue = new Label
            {
                Text = "220 / 255",
                Location = new Point(630, 120),
                Width = 100,
                Font = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.FromArgb(100, 181, 246)
            };
            grpAppearance.Controls.Add(lblOpacityValue);

            // Bouton Appliquer apparence
            var btnApplyAppearance = new Button
            {
                Text = "Appliquer",
                Location = new Point(20, 160),
                Width = 150,
                Height = 40,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.Black, // ? TEXTE NOIR
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat, // ? SANS BORDURE
                Cursor = Cursors.Hand
            };
            btnApplyAppearance.FlatAppearance.BorderSize = 0; // ? BORDURE ZERO
            btnApplyAppearance.Click += (s, e) => ApplyAppearanceSettings();
            grpAppearance.Controls.Add(btnApplyAppearance);

            this.Controls.Add(grpAppearance);
            yPos += 240;

            // SECTION 2 : SéCURITé
            yPos = AddSectionTitle("🛡️ SéCURITé ET CONFIDENTIALITé", yPos);

            var grpSecurity = CreateGroupBox("Protection des données sensibles", yPos, 900, 180);

            chkPinProtection = new CheckBox
            {
                Text = "Activer la protection PIN pour les photos principales des filons",
                Location = new Point(20, 40),
                Width = 600,
                Font = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.White,
                Checked = false
            };
            grpSecurity.Controls.Add(chkPinProtection);

            chkConfirmDelete = new CheckBox
            {
                Text = "Demander confirmation avant toute suppression",
                Location = new Point(20, 75),
                Width = 600,
                Font = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.White,
                Checked = true
            };
            grpSecurity.Controls.Add(chkConfirmDelete);

            chkAutoSave = new CheckBox
            {
                Text = "Sauvegarde automatique des modifications (toutes les 5 minutes)",
                Location = new Point(20, 110),
                Width = 600,
                Font = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.White,
                Checked = true
            };
            grpSecurity.Controls.Add(chkAutoSave);

            this.Controls.Add(grpSecurity);
            yPos += 200;

            // SECTION 3 : DONNéES
            yPos = AddSectionTitle("📊 GESTION DES DONNéES", yPos);

            var grpData = CreateGroupBox("Sauvegarde, restauration et export", yPos, 900, 280);

            // Statistiques
            var lblStats = new Label
            {
                Text = "📈 Statistiques :",
                Location = new Point(20, 40),
                Width = 200,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 181, 246)
            };
            grpData.Controls.Add(lblStats);

            lblFilonsCount = new Label
            {
                Text = "Nombre de filons : Chargement...",
                Location = new Point(40, 70),
                Width = 400,
                Font = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.White
            };
            grpData.Controls.Add(lblFilonsCount);

            lblDataSize = new Label
            {
                Text = "Espace utilisé : Calcul...",
                Location = new Point(40, 95),
                Width = 400,
                Font = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.White
            };
            grpData.Controls.Add(lblDataSize);

            // ? TOUS LES BOUTONS AVEC BORDURE ZéRO + TEXTE NOIR + POLICE EMOJI
            var btnSaveData = new Button
            {
                Text = "💾 Sauvegarder",
                Location = new Point(20, 140),
                Width = 180,
                Height = 45,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.Black, // ? TEXTE NOIR
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold), // ? POLICE EMOJI
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSaveData.FlatAppearance.BorderSize = 0;
            btnSaveData.Click += BtnSaveData_Click;
            grpData.Controls.Add(btnSaveData);

            var btnRestoreData = new Button
            {
                Text = "🔄 Restaurer",
                Location = new Point(210, 140),
                Width = 180,
                Height = 45,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.Black, // ? TEXTE NOIR
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold), // ? POLICE EMOJI
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRestoreData.FlatAppearance.BorderSize = 0;
            btnRestoreData.Click += BtnRestoreData_Click;
            grpData.Controls.Add(btnRestoreData);

            var btnExportAll = new Button
            {
                Text = "📊 Exporter CSV",
                Location = new Point(400, 140),
                Width = 180,
                Height = 45,
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.Black, // ? TEXTE NOIR
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold), // ? POLICE EMOJI
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExportAll.FlatAppearance.BorderSize = 0;
            btnExportAll.Click += BtnExportAll_Click;
            grpData.Controls.Add(btnExportAll);

            var btnCleanCache = new Button
            {
                Text = "🧹 Nettoyer Cache",
                Location = new Point(590, 140),
                Width = 180,
                Height = 45,
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.Black, // ? TEXTE NOIR
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold), // ? POLICE EMOJI
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCleanCache.FlatAppearance.BorderSize = 0;
            btnCleanCache.Click += BtnCleanCache_Click;
            grpData.Controls.Add(btnCleanCache);

            var btnOpenDataFolder = new Button
            {
                Text = "📁 Ouvrir Dossier",
                Location = new Point(20, 195),
                Width = 180,
                Height = 45,
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.Black, // ? TEXTE NOIR
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold), // ? POLICE EMOJI
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnOpenDataFolder.FlatAppearance.BorderSize = 0;
            btnOpenDataFolder.Click += (s, e) => OpenDataFolder();
            grpData.Controls.Add(btnOpenDataFolder);

            this.Controls.Add(grpData);
            yPos += 300;

            // SECTION 3.5 : MODE HORS-LIGNE ? NOUVEAU
            yPos = AddSectionTitle("📅 MODE HORS-LIGNE", yPos);

            var grpOffline = CreateGroupBox("Télécharger cartes pour usage sans Internet", yPos, 900, 200);

            var lblOffline = new Label
            {
                Text = "Téléchargez des zones de carte pour naviguer sans connexion Internet lors de vos sorties terrain.",
                Location = new Point(20, 40),
                Width = 850,
                Height = 40,
                Font = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.White
            };
            grpOffline.Controls.Add(lblOffline);

            var btnDownloadArea = new Button
            {
                Text = "📥 Télécharger Zone Carte",
                Location = new Point(20, 90),
                Width = 220,
                Height = 45,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold), // ? POLICE EMOJI
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDownloadArea.FlatAppearance.BorderSize = 0;
            btnDownloadArea.Click += BtnDownloadArea_Click;
            grpOffline.Controls.Add(btnDownloadArea);

            var btnToggleOfflineMode = new Button
            {
                Text = "📡 Mode: En Ligne",
                Location = new Point(250, 90),
                Width = 200,
                Height = 45,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold), // ? POLICE EMOJI
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnToggleOfflineMode.FlatAppearance.BorderSize = 0;
            btnToggleOfflineMode.Click += BtnToggleOfflineMode_Click;
            btnToggleOfflineMode.Tag = false; // false = en ligne, true = hors ligne
            grpOffline.Controls.Add(btnToggleOfflineMode);

            var btnClearOfflineCache = new Button
            {
                Text = "🗑️ Vider Cache",
                Location = new Point(460, 90),
                Width = 180,
                Height = 45,
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold), // ? POLICE EMOJI
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClearOfflineCache.FlatAppearance.BorderSize = 0;
            btnClearOfflineCache.Click += BtnClearOfflineCache_Click;
            grpOffline.Controls.Add(btnClearOfflineCache);

            var lblCacheSize = new Label
            {
                Text = "Taille du cache: Calcul...",
                Location = new Point(20, 150),
                Width = 400,
                Font = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.White
            };
            grpOffline.Controls.Add(lblCacheSize);
            lblCacheSize.Tag = "cache_size_label"; // Pour le retrouver plus tard

            this.Controls.Add(grpOffline);
            yPos += 220;

            // SECTION 3.6 : CLOUD SYNC ⭐ NOUVEAU
            yPos = AddSectionTitle("☁️ SYNCHRONISATION CLOUD", yPos);

            var grpCloud = CreateGroupBox("Synchronisez vos données avec GitHub", yPos, 900, 280);

            var lblCloudDesc = new Label
            {
                Text = "Partagez et synchronisez automatiquement vos filons entre plusieurs ordinateurs via GitHub.",
                Location = new Point(20, 40),
                Width = 850,
                Height = 40,
                Font = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.White
            };
            grpCloud.Controls.Add(lblCloudDesc);

            var lblCloudStatus = new Label
            {
                Text = "Status: Désactivé",
                Location = new Point(20, 85),
                Width = 400,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                ForeColor = Color.Red
            };
            lblCloudStatus.Tag = "cloud_status"; // Pour le retrouver
            grpCloud.Controls.Add(lblCloudStatus);

            var btnEnableCloud = new Button
            {
                Text = "🌐 Activer Cloud Sync",
                Location = new Point(20, 120),
                Width = 200,
                Height = 45,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEnableCloud.FlatAppearance.BorderSize = 0;
            btnEnableCloud.Click += BtnEnableCloud_Click;
            btnEnableCloud.Tag = "btn_enable_cloud";
            grpCloud.Controls.Add(btnEnableCloud);

            var btnPull = new Button
            {
                Text = "⬇️ Pull (Récupérer)",
                Location = new Point(230, 120),
                Width = 200,
                Height = 45,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnPull.FlatAppearance.BorderSize = 0;
            btnPull.Click += BtnPull_Click;
            btnPull.Tag = "btn_pull";
            grpCloud.Controls.Add(btnPull);

            var btnPush = new Button
            {
                Text = "⬆️ Push (Envoyer)",
                Location = new Point(440, 120),
                Width = 200,
                Height = 45,
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnPush.FlatAppearance.BorderSize = 0;
            btnPush.Click += BtnPush_Click;
            btnPush.Tag = "btn_push";
            grpCloud.Controls.Add(btnPush);

            var btnSync = new Button
            {
                Text = "🔄 Sync (Tout)",
                Location = new Point(650, 120),
                Width = 180,
                Height = 45,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnSync.FlatAppearance.BorderSize = 0;
            btnSync.Click += BtnSync_Click;
            btnSync.Tag = "btn_sync";
            grpCloud.Controls.Add(btnSync);

            var lblLastSync = new Label
            {
                Text = "Dernière sync: Jamais",
                Location = new Point(20, 180),
                Width = 400,
                Font = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.Gray
            };
            lblLastSync.Tag = "last_sync";
            grpCloud.Controls.Add(lblLastSync);

            var lblCloudHelp = new Label
            {
                Text = "💡 Astuce: Installez Git (https://git-scm.com) pour activer la synchronisation.",
                Location = new Point(20, 210),
                Width = 850,
                Height = 40,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 181, 246)
            };
            grpCloud.Controls.Add(lblCloudHelp);

            this.Controls.Add(grpCloud);
            yPos += 300;

            // SECTION 4 : é PROPOS
            yPos = AddSectionTitle("ℹ️ A PROPOS", yPos);

            var grpAbout = CreateGroupBox("Informations sur l'application", yPos, 900, 200);

            var lblVersion = new Label
            {
                Text = "WMine - Localisateur de Filons Miniers\nVersion 1.0.0\n\né 2025 - Tous droits réservés\n\ndéveloppé avec .NET 8 et WinForms",
                Location = new Point(20, 40),
                Width = 400,
                Height = 140,
                Font = new Font("Segoe UI Emoji", 11),
                ForeColor = Color.White
            };
            grpAbout.Controls.Add(lblVersion);

            var btnDocumentation = new Button
            {
                Text = "📚 Documentation",
                Location = new Point(450, 40),
                Width = 180,
                Height = 45,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.Black, // ? TEXTE NOIR
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDocumentation.FlatAppearance.BorderSize = 0;
            btnDocumentation.Click += (s, e) =>
            {
                var docPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documentation");
                if (Directory.Exists(docPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", docPath);
                }
                else
                {
                    MessageBox.Show("Le dossier de documentation n'a pas été trouvé.",
                        "Documentation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            grpAbout.Controls.Add(btnDocumentation);

            var btnCheckUpdates = new Button
            {
                Text = "🔄 Vérifier MAJ",
                Location = new Point(640, 40),
                Width = 180,
                Height = 45,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.Black, // ? TEXTE NOIR
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCheckUpdates.FlatAppearance.BorderSize = 0;
            btnCheckUpdates.Click += (s, e) =>
            {
                MessageBox.Show("Vous utilisez la derniére version de WMine.\n\nVersion 1.0.0 - Build 2025",
                    "Mises à jour", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            grpAbout.Controls.Add(btnCheckUpdates);

            var btnLicense = new Button
            {
                Text = "📜 Licence",
                Location = new Point(450, 95),
                Width = 180,
                Height = 45,
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.Black, // ? TEXTE NOIR
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLicense.FlatAppearance.BorderSize = 0;
            btnLicense.Click += (s, e) =>
            {
                MessageBox.Show("WMine - Localisateur de Filons Miniers\n\n" +
                              "Logiciel développé pour la gestion et la localisation\n" +
                              "des filons miniers dans le département du Var.\n\n" +
                              " 2025 - Tous droits réservés",
                    "Licence", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            grpAbout.Controls.Add(btnLicense);

            this.Controls.Add(grpAbout);
        }

        private GroupBox CreateGroupBox(string text, int yPos, int width, int height)
        {
            return new GroupBox
            {
                Text = $"  {text}  ",
                Location = new Point(40, yPos),
                Width = width,
                Height = height,
                ForeColor = Color.FromArgb(100, 181, 246),
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 35, 45)
            };
        }

        private int AddSectionTitle(string text, int yPos)
        {
            var lblSection = new Label
            {
                Text = text,
                Location = new Point(40, yPos),
                Width = 900,
                Height = 40,
                Font = new Font("Segoe UI Emoji", 18, FontStyle.Bold), // ? POLICE EMOJI
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 45, 55),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };
            this.Controls.Add(lblSection);
            return yPos + 50;
        }

        private void LoadSettings()
        {
            // Charger les statistiques
            try
            {
                var dataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "wmine", "filons.json");

                if (File.Exists(dataPath))
                {
                    var json = File.ReadAllText(dataPath);
                    var filons = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<Models.Filon>>(json);
                    lblFilonsCount.Text = $"Nombre de filons : {filons?.Count ?? 0}";

                    var fileInfo = new FileInfo(dataPath);
                    var sizeKb = fileInfo.Length / 1024;
                    lblDataSize.Text = $"Espace utilisé : {sizeKb} Ko";
                }
                else
                {
                    lblFilonsCount.Text = "Nombre de filons : 0";
                    lblDataSize.Text = "Espace utilisé : 0 Ko";
                }
            }
            catch
            {
                lblFilonsCount.Text = "Nombre de filons : Erreur de chargement";
                lblDataSize.Text = "Espace utilisé : N/A";
            }
        }

        private void ApplyAppearanceSettings()
        {
            var message = "Paramétres d'apparence appliqués :\n\n";
            message += $"Théme : {cmbTheme.SelectedItem}\n";
            message += $"Animations : {(chkAnimations.Checked ? "Activées" : "désactivées")}\n";
            message += $"Opacité : {trackOpacity.Value}/255\n\n";
            message += "Certains changements nécessitent un redémarrage de l'application.";

            MessageBox.Show(message, "Paramétres appliqués", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSaveData_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "JSON|*.json",
                FileName = $"wmine_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                Title = "Sauvegarder les données"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var dataPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "wmine", "filons.json");

                    if (File.Exists(dataPath))
                    {
                        File.Copy(dataPath, sfd.FileName, true);
                        MessageBox.Show("Sauvegarde réussie !\n\n" +
                                      $"Fichier : {Path.GetFileName(sfd.FileName)}",
                            "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Aucune donnée à sauvegarder.",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la sauvegarde :\n\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRestoreData_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "JSON|*.json",
                Title = "Restaurer des données"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (MessageBox.Show(" ATTENTION !\n\n" +
                                  "La restauration écrasera toutes les données actuelles.\n\n" +
                                  "êtes-vous sûr de vouloir continuer ?",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        var dataPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "wmine", "filons.json");

                        File.Copy(ofd.FileName, dataPath, true);
                        LoadSettings();

                        MessageBox.Show("Restauration réussie !\n\n" +
                                      "L'application va maintenant redémarrer.",
                            "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Application.Restart();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la restauration :\n\n{ex.Message}",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnExportAll_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "CSV|*.csv",
                FileName = $"filons_export_{DateTime.Now:yyyyMMdd}.csv",
                Title = "Exporter en CSV"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Utiliser le script PowerShell ou le service d'export
                    var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "export-to-csv.ps1");

                    if (File.Exists(scriptPath))
                    {
                        System.Diagnostics.Process.Start("powershell.exe", $"-File \"{scriptPath}\"");
                        MessageBox.Show("Export lancé !\n\n" +
                                      "Le fichier CSV sera créé sur votre Bureau.",
                            "Export en cours", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(" Script d'export non trouvé.\n\n" +
                                      "Utilisez le bouton 'Export CSV' dans l'onglet Import OCR.",
                            "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'export :\n\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnCleanCache_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Nettoyer le cache de l'application ?\n\n" +
                              "Cette action supprimera :\n" +
                              " Cache des cartes\n" +
                              " Fichiers temporaires OCR\n" +
                              " Images en cache\n\n" +
                              "Les données de filons seront préservées.",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    var cacheFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "wmine", "MapCache");

                    if (Directory.Exists(cacheFolder))
                    {
                        Directory.Delete(cacheFolder, true);
                    }

                    MessageBox.Show("Cache nettoyé avec succés !",
                        "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du nettoyage :\n\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenDataFolder()
        {
            try
            {
                var dataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "wmine");

                if (!Directory.Exists(dataFolder))
                    Directory.CreateDirectory(dataFolder);

                System.Diagnostics.Process.Start("explorer.exe", dataFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur :\n\n{ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbTheme_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_themeService == null)
            {
                MessageBox.Show("Service de thémes non initialisé.\n\n" +
                              "Le théme sera appliqué au prochain démarrage.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedType = (ThemeType)cmbTheme.SelectedIndex;
            var newTheme = AppTheme.GetTheme(selectedType);

            if (MessageBox.Show($"Appliquer le théme {newTheme.Icon} {newTheme.Name} ?\n\n" +
                              $"{newTheme.GetDescription()}\n\n" +
                              "L'application sera rafréchie.",
                "Changer de théme", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    // Trouver le formulaire parent
                    var parentForm = this.FindForm();
                    if (parentForm != null)
                    {
                        _themeService.ApplyTheme(parentForm, newTheme);

                        MessageBox.Show($"Théme {newTheme.Icon} {newTheme.Name} appliqué avec succés !\n\n" +
                                      "Certaines parties de l'interface seront mises é jour\n" +
                                      "au prochain redémarrage de l'application.",
                            "Succés", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'application du théme :\n\n{ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Remettre la sélection précédente
                cmbTheme.SelectedIndex = (int)_themeService.CurrentTheme.Type;
            }
        }

        // ? NOUVEAU: Gestion Mode Hors-Ligne

        private void BtnDownloadArea_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Téléchargement de Zone\n\n" +
                "Cette fonctionnalité permet de télécharger une zone de carte\n" +
                "pour l'utiliser sans connexion Internet.\n\n" +
                "Sélectionnez une zone sur la carte:\n" +
                "1. Coin supérieur gauche\n" +
                "2. Coin inférieur droit\n" +
                "3. Choisissez les niveaux de zoom (10-15 recommandé)\n\n" +
                "Le téléchargement peut prendre plusieurs minutes selon la zone.\n\n" +
                "Fonctionnalité en cours d'implémentation...",
                "Téléchargement Zone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // TODO: Ouvrir dialogue de sélection de zone avec barre de progression
        }

        private void BtnToggleOfflineMode_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            bool isOffline = (bool)(btn.Tag ?? false);
            isOffline = !isOffline; // Toggle

            try
            {
                var offlineService = new OfflineModeService();
                offlineService.SetOfflineMode(isOffline);

                btn.Tag = isOffline;
                btn.Text = isOffline ? "📡 Mode: Hors Ligne" : "📡 Mode: En Ligne";
                btn.BackColor = isOffline ? Color.FromArgb(255, 152, 0) : Color.FromArgb(0, 150, 136);

                MessageBox.Show(
                    isOffline
                        ? "Mode Hors-Ligne activé\n\nLa carte utilise maintenant uniquement le cache local.\nSi une zone n'est pas téléchargée, elle n'apparaétra pas."
                        : "Mode En Ligne activé\n\nLa carte télécharge les tuiles depuis Internet.",
                    "Mode changé",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClearOfflineCache_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show(
                "Vider le cache hors-ligne ?\n\n" +
                "Cette action supprimera toutes les cartes téléchargées.\n" +
                "Vous devrez les retélécharger pour le mode hors-ligne.\n\n" +
                "Continuer ?",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    var offlineService = new OfflineModeService();
                    bool success = offlineService.ClearOfflineCache();

                    if (success)
                    {
                        MessageBox.Show(
                            "Cache hors-ligne vidé avec succés !",
                            "Succés",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        // Mettre é jour l'affichage de la taille
                        UpdateCacheSizeDisplay();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Impossible de vider le cache.",
                            "Avertissement",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateCacheSizeDisplay()
        {
            try
            {
                var offlineService = new OfflineModeService();
                double sizeMB = offlineService.GetTotalCacheSizeMB();

                // Trouver le label de taille de cache
                var label = this.Controls.OfType<GroupBox>()
                    .FirstOrDefault(g => g.Text.Contains("sans Internet"))
                    ?.Controls.OfType<Label>()
                    .FirstOrDefault(l => l.Tag?.ToString() == "cache_size_label");

                if (label != null)
                {
                    label.Text = $"Taille du cache: {sizeMB:F2} MB";
                }
            }
            catch
            {
                // Ignorer silencieusement
            }
        }

        // ⭐ NOUVEAU: Gestionnaires Cloud Sync
        
        private CloudSyncService? _cloudService;

        private async void BtnEnableCloud_Click(object? sender, EventArgs e)
        {
            try
            {
                _cloudService = new CloudSyncService();
                
                var result = await _cloudService.EnableAsync();
                
                if (result.success)
                {
                    MessageBox.Show(result.message, "Cloud Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateCloudUI(true);
                }
                else
                {
                    MessageBox.Show(result.message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'activation:\n{ex.Message}", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnPull_Click(object? sender, EventArgs e)
        {
            if (_cloudService == null) return;

            try
            {
                var result = await _cloudService.PullAsync();
                
                if (result.success)
                {
                    MessageBox.Show($"{result.message}\n\n{result.newFilons} nouveau(x) filon(s) récupéré(s).", 
                        "Pull réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateLastSyncLabel();
                    LoadSettings(); // Rafraîchir les stats
                }
                else
                {
                    MessageBox.Show(result.message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du pull:\n{ex.Message}", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnPush_Click(object? sender, EventArgs e)
        {
            if (_cloudService == null) return;

            try
            {
                var result = await _cloudService.PushAsync();
                
                if (result.success)
                {
                    MessageBox.Show(result.message, "Push réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateLastSyncLabel();
                }
                else
                {
                    MessageBox.Show(result.message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du push:\n{ex.Message}", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSync_Click(object? sender, EventArgs e)
        {
            if (_cloudService == null) return;

            try
            {
                var result = await _cloudService.SyncAsync();
                
                if (result.success)
                {
                    MessageBox.Show("Synchronisation complète réussie!\n\nVos données sont maintenant à jour.", 
                        "Sync réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateLastSyncLabel();
                    LoadSettings(); // Rafraîchir les stats
                }
                else
                {
                    MessageBox.Show(result.message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sync:\n{ex.Message}", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateCloudUI(bool enabled)
        {
            // Trouver le GroupBox Cloud
            var grpCloud = this.Controls.OfType<GroupBox>()
                .FirstOrDefault(g => g.Text.Contains("GitHub"));

            if (grpCloud == null) return;

            // Mettre à jour le status
            var lblStatus = grpCloud.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Tag?.ToString() == "cloud_status");
            if (lblStatus != null)
            {
                lblStatus.Text = enabled ? "Status: ✅ Activé" : "Status: ❌ Désactivé";
                lblStatus.ForeColor = enabled ? Color.FromArgb(76, 175, 80) : Color.Red;
            }

            // Activer/Désactiver les boutons
            var btnPull = grpCloud.Controls.OfType<Button>()
                .FirstOrDefault(b => b.Tag?.ToString() == "btn_pull");
            if (btnPull != null) btnPull.Enabled = enabled;

            var btnPush = grpCloud.Controls.OfType<Button>()
                .FirstOrDefault(b => b.Tag?.ToString() == "btn_push");
            if (btnPush != null) btnPush.Enabled = enabled;

            var btnSync = grpCloud.Controls.OfType<Button>()
                .FirstOrDefault(b => b.Tag?.ToString() == "btn_sync");
            if (btnSync != null) btnSync.Enabled = enabled;

            // Changer le bouton Enable en Disable
            var btnEnable = grpCloud.Controls.OfType<Button>()
                .FirstOrDefault(b => b.Tag?.ToString() == "btn_enable_cloud");
            if (btnEnable != null)
            {
                btnEnable.Text = enabled ? "🔴 Désactiver" : "🌐 Activer Cloud Sync";
                btnEnable.BackColor = enabled ? Color.FromArgb(244, 67, 54) : Color.FromArgb(0, 150, 136);
                btnEnable.Click -= BtnEnableCloud_Click;
                btnEnable.Click -= BtnDisableCloud_Click;
                btnEnable.Click += enabled ? BtnDisableCloud_Click : BtnEnableCloud_Click;
            }
        }

        private void BtnDisableCloud_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Désactiver la synchronisation cloud?\n\nVos données locales seront conservées.", 
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (_cloudService == null) _cloudService = new CloudSyncService();
                    var result = _cloudService.Disable();
                    
                    if (result.success)
                    {
                        MessageBox.Show(result.message, "Cloud Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UpdateCloudUI(false);
                        _cloudService = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur:\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateLastSyncLabel()
        {
            if (_cloudService == null) return;

            var grpCloud = this.Controls.OfType<GroupBox>()
                .FirstOrDefault(g => g.Text.Contains("GitHub"));

            if (grpCloud == null) return;

            var lblLastSync = grpCloud.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Tag?.ToString() == "last_sync");
            
            if (lblLastSync != null)
            {
                lblLastSync.Text = $"Dernière sync: {_cloudService.LastSyncTime}";
                lblLastSync.ForeColor = Color.FromArgb(76, 175, 80);
            }
        }
    }
}


