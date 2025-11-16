using System.Drawing.Drawing2D;
using wmine.Models;
using wmine.Services;

namespace wmine.Forms
{
    /// <summary>
    /// Panel affichant les informations sur les minéraux
    /// </summary>
    public class MineralsPanel : UserControl
    {
        private readonly IMineralRepository _mineralRepository;
        private readonly FilonDataService _dataService;
        private readonly PhotoService _photoService;
        private Panel? _mainPanel;
        private FlowLayoutPanel? _mineralsGrid;

        public MineralsPanel(FilonDataService dataService, IMineralRepository mineralRepository, PhotoService photoService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _mineralRepository = mineralRepository ?? throw new ArgumentNullException(nameof(mineralRepository));
            _photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));
            InitializePanel();
        }

        private void InitializePanel()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(25, 25, 35);
            this.AutoScroll = true;

            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 35),
                AutoScroll = true,
                Padding = new Padding(20)
            };

            // Titre
            var lblTitle = new Label
            {
                Text = "Catalogue des Minéraux",
                Location = new Point(20, 20),
                Width = 600,
                Height = 50,
                Font = new Font("Segoe UI Emoji", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 181, 246)
            };

            // Description
            var lblDescription = new Label
            {
                Text = "Vue d'ensemble des minéraux disponibles et statistiques d'extraction",
                Location = new Point(20, 80),
                Width = 800,
                Height = 30,
                Font = new Font("Segoe UI Emoji", 11),
                ForeColor = Color.FromArgb(200, 200, 200)
            };

            // Grille de minéraux
            _mineralsGrid = new FlowLayoutPanel
            {
                Location = new Point(20, 130),
                Width = 1300,
                Height = 600,
                AutoScroll = true,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            _mainPanel.Controls.Add(lblTitle);
            _mainPanel.Controls.Add(lblDescription);
            _mainPanel.Controls.Add(_mineralsGrid);

            this.Controls.Add(_mainPanel);

            // Charger les minéraux
            LoadMinerals();
        }

        private void LoadMinerals()
        {
            if (_mineralsGrid == null)
                return;

            _mineralsGrid.Controls.Clear();

            // Obtenir les statistiques
            var allFilons = _dataService.GetAllFilons();
            var mineralStats = allFilons
                .GroupBy(f => f.MatierePrincipale)
                .ToDictionary(g => g.Key, g => g.Count());

            // Créer une carte pour chaque type de minéral
            foreach (MineralType mineralType in Enum.GetValues<MineralType>())
            {
                var count = mineralStats.ContainsKey(mineralType) ? mineralStats[mineralType] : 0;
                var card = CreateMineralCard(mineralType, count);
                _mineralsGrid.Controls.Add(card);
            }
        }

        private Panel CreateMineralCard(MineralType mineralType, int filonCount)
        {
            var card = new Panel
            {
                Width = 300,
                Height = 200,
                Margin = new Padding(10),
                BackColor = Color.FromArgb(40, 45, 55)
            };

            card.Paint += (s, e) => DrawCardBackground(e.Graphics, card.ClientRectangle);

            var color = MineralColors.GetColor(mineralType);
            var name = MineralColors.GetDisplayName(mineralType);

            // Indicateur de couleur
            var colorIndicator = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(60, 60),
                BackColor = color
            };
            colorIndicator.Paint += (s, e) => DrawColorIndicator(e.Graphics, colorIndicator.ClientRectangle, color);

            // Vignette photo zoomable
            var photoPath = _photoService.GetMineralPhotoPath(mineralType);
            if (photoPath != null)
            {
                var photoBox = new UI.ZoomablePictureBox
                {
                    Width = 60,
                    Height = 60,
                    Location = new Point(220, 20),
                    Image = _photoService.CreateThumbnail(photoPath, 60, 60),
                    BackColor = Color.Transparent
                };
                card.Controls.Add(photoBox);
            }
            else
            {
                // Bouton pour ajouter une photo
                var btnAddPhoto = new Button
                {
                    Text = "📷",
                    Width = 60,
                    Height = 60,
                    Location = new Point(220, 20),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(60, 65, 75),
                    ForeColor = Color.Black,
                    Font = new Font("Segoe UI Emoji", 20),
                    Cursor = Cursors.Hand
                };
                btnAddPhoto.FlatAppearance.BorderSize = 1;
                btnAddPhoto.FlatAppearance.BorderColor = Color.FromArgb(100, 105, 115);

                btnAddPhoto.Click += (s, e) =>
                {
                    using (var ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                        ofd.Title = $"Sélectionner une photo pour {name}";

                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                _photoService.SaveMineralPhoto(mineralType, ofd.FileName);
                                LoadMinerals(); // Recharger pour afficher la photo
                                MessageBox.Show($"Photo ajoutée pour {name}!", "Succés",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Erreur: {ex.Message}", "Erreur",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                };

                card.Controls.Add(btnAddPhoto);
            }

            // Nom du minéral
            var lblName = new Label
            {
                Text = name,
                Location = new Point(90, 20),
                Width = 120,  // Réduits pour faire place é la photo
                Height = 30,
                Font = new Font("Segoe UI Emoji", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            // Nombre de filons
            var lblCount = new Label
            {
                Text = $"{filonCount} filon{(filonCount > 1 ? "s" : "")}",
                Location = new Point(90, 50),
                Width = 120,
                Height = 25,
                Font = new Font("Segoe UI Emoji", 11),
                ForeColor = Color.FromArgb(200, 200, 200),
                BackColor = Color.Transparent
            };

            // Propriétés
            var properties = GetMineralProperties(mineralType);
            var lblProperties = new Label
            {
                Text = properties,
                Location = new Point(20, 100),
                Width = 260,
                Height = 80,
                Font = new Font("Segoe UI Emoji", 9),
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent
            };

            // événement clic sur toute la carte
            card.Click += (s, e) => OnMineralCardClick(mineralType);
            card.Cursor = Cursors.Hand;
            colorIndicator.Click += (s, e) => OnMineralCardClick(mineralType);
            lblName.Click += (s, e) => OnMineralCardClick(mineralType);
            lblCount.Click += (s, e) => OnMineralCardClick(mineralType);
            lblProperties.Click += (s, e) => OnMineralCardClick(mineralType);

            // Ajout d'un bouton "Galerie" pour ouvrir la galerie de photos du minéral
            var btnGallery = new Button
            {
                Text = "Galerie",
                Location = new Point(220, 100),
                Width = 60,
                Height = 28,
                BackColor = Color.FromArgb(60, 65, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };
            btnGallery.FlatAppearance.BorderSize = 0;
            // Use icon glyph instead of long text to avoid truncation
            btnGallery.Text = "📷";
            btnGallery.Font = new Font("Segoe UI Emoji", 14);
            btnGallery.Click += (s, e) =>
            {
                var photoService = Services.AppServiceProvider.GetService<PhotoService>() ?? new PhotoService();
                var gallery = new Forms.PhotoGalleryPanel(photoService, mineralType, null);
                var form = new Form
                {
                    Text = $"Galerie - {MineralColors.GetDisplayName(mineralType)}",
                    Size = new Size(900, 700),
                    StartPosition = FormStartPosition.CenterParent
                };
                form.Controls.Add(gallery);
                gallery.Dock = DockStyle.Fill;
                form.ShowDialog();
            };
            card.Controls.Add(btnGallery);

            // Effet hover
            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(50, 55, 65);
            card.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(40, 45, 55);

            card.Controls.Add(colorIndicator);
            card.Controls.Add(lblName);
            card.Controls.Add(lblCount);
            card.Controls.Add(lblProperties);

            return card;
        }

        private void DrawCardBackground(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = GetRoundedRectangle(bounds, 10))
            using (var brush = new SolidBrush(Color.FromArgb(40, 45, 55)))
            {
                g.FillPath(brush, path);
            }

            // Bordure
            using (var path = GetRoundedRectangle(bounds, 10))
            using (var pen = new Pen(Color.FromArgb(60, 65, 75), 2))
            {
                g.DrawPath(pen, path);
            }
        }

        private void DrawColorIndicator(Graphics g, Rectangle bounds, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var innerBounds = new Rectangle(
                bounds.X + 2,
                bounds.Y + 2,
                bounds.Width - 4,
                bounds.Height - 4
            );

            // Cercle avec dégradé
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(innerBounds);

                using (var brush = new LinearGradientBrush(
                    innerBounds,
                    Color.FromArgb(255, color),
                    Color.FromArgb(200, color),
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, path);
                }
            }

            // Bordure blanche
            using (var pen = new Pen(Color.White, 2))
            {
                g.DrawEllipse(pen, innerBounds);
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

        private string GetMineralProperties(MineralType mineralType)
        {
            // Utiliser les données enrichies du service
            var mineralInfo = _mineralRepository.GetMineralInfo(mineralType);

            if (mineralInfo != null)
            {
                return $"{mineralInfo.FormuleChimique}\nDureté: {mineralInfo.DureteMohs}\n{mineralInfo.SystemeCristallin}";
            }

            // Fallback
            return mineralType switch
            {
                MineralType.Fer => "élément métallique\nSymbole: Fe\nUtilisation: Construction",
                MineralType.Cuivre => "Métal conducteur\nSymbole: Cu\nélectricité",
                MineralType.Argent => "Métal précieux\nSymbole: Ag\nBijouterie",
                _ => "Propriétés é documenter"
            };
        }

        private void OnMineralCardClick(MineralType mineralType)
        {
            // Afficher les détails complets du minéral
            ShowMineralDetails(mineralType);

            // Notifier le parent pour filtrer les filons par ce minéral
            var args = new MineralSelectedEventArgs(mineralType);
            MineralSelected?.Invoke(this, args);
        }

        private void ShowMineralDetails(MineralType mineralType)
        {
            var mineralInfo = _mineralRepository.GetMineralInfo(mineralType);

            if (mineralInfo == null)
            {
                MessageBox.Show($"Aucune information détaillée disponible pour {MineralColors.GetDisplayName(mineralType)}",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Créer un formulaire de détails
            var detailsForm = new Form
            {
                Text = $"🔎 {mineralInfo.Nom}",
                Size = new Size(850, 750),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                BackColor = Color.FromArgb(30, 35, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10f)
            };

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(30, 35, 45)
            };

            var content = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(780, 0),
                Text = FormatMineralDetails(mineralInfo),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Emoji", 11f)
            };

            mainPanel.Controls.Add(content);
            detailsForm.Controls.Add(mainPanel);

            // Bouton fermer
            var btnClose = new Button
            {
                Text = "❌ Fermer",
                Size = new Size(120, 40),
                Location = new Point(710, 660),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => detailsForm.Close();
            detailsForm.Controls.Add(btnClose);

            detailsForm.ShowDialog();
        }

        private string FormatMineralDetails(MineralInfo info)
        {
            var text = $@"📋 IDENTIFICATION

Nom : {info.Nom}
Formule chimique : {info.FormuleChimique}
Système cristallin : {info.SystemeCristallin}

⚙️ PROPRIÉTÉS PHYSIQUES

Dureté (Mohs) : {info.DureteMohs}
Densité : {info.Densite}
{info.ProprietesPhysiques}

📝 DESCRIPTION

{info.Description}

📍 LOCALITÉS DANS LE VAR ({info.LocalitesVar.Count} sites)

{string.Join("\n", info.LocalitesVar.Select(l => $"• {l}"))}

🔧 UTILISATIONS

{info.Utilisation}

🌐 SOURCES & RÉFÉRENCES

{string.Join("\n", info.SourcesWeb.Select(s => $"• {s}"))}

📅 Dernière mise à jour : {info.DerniereMiseAJour:dd/MM/yyyy}

---
✅ Sources vérifiées : BRGM, Mindat.org, Géologie du Var";

            return text;
        }

        public event EventHandler<MineralSelectedEventArgs>? MineralSelected;
    }

    public class MineralSelectedEventArgs : EventArgs
    {
        public MineralType MineralType { get; }

        public MineralSelectedEventArgs(MineralType mineralType)
        {
            MineralType = mineralType;
        }
    }
}

