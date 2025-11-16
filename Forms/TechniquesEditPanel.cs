using wmine.Models;
using wmine.Services;

namespace wmine.Forms
{
    public class TechniquesEditPanel : Panel
    {
        private readonly TechniquesDataService _dataService;
        private ListBox lstDocuments;
        private Button btnAddNote;
        private Button btnAddPdf;
        private Button btnDelete;
        private Label lblInfo;

        public TechniquesEditPanel()
        {
            _dataService = new TechniquesDataService();
            InitializeComponents();
            LoadDocuments();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(25, 25, 35);
            this.AutoScroll = true;
            this.Padding = new Padding(40);

            // Titre
            var lblTitle = new Label
            {
                Text = "Techniques d'Extraction Miniére",
                Location = new Point(40, 40),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 150, 136)
            };
            this.Controls.Add(lblTitle);

            // Info
            lblInfo = new Label
            {
                Text = "Gérez vos documents techniques (notes manuelles et PDF). Double-clic pour ouvrir.",
                Location = new Point(40, 100),
                Width = 800,
                Height = 30,
                Font = new Font("Segoe UI Emoji", 11),
                ForeColor = Color.FromArgb(180, 180, 180)
            };
            this.Controls.Add(lblInfo);

            // Boutons
            btnAddNote = new Button
            {
                Text = "Nouvelle Note",
                Location = new Point(40, 150),
                Width = 180,
                Height = 50,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddNote.FlatAppearance.BorderSize = 0;
            btnAddNote.Click += BtnAddNote_Click;
            this.Controls.Add(btnAddNote);

            btnAddPdf = new Button
            {
                Text = "Ajouter PDF",
                Location = new Point(240, 150),
                Width = 180,
                Height = 50,
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddPdf.FlatAppearance.BorderSize = 0;
            btnAddPdf.Click += BtnAddPdf_Click;
            this.Controls.Add(btnAddPdf);

            btnDelete = new Button
            {
                Text = "Supprimer",
                Location = new Point(440, 150),
                Width = 150,
                Height = 50,
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;
            this.Controls.Add(btnDelete);

            // Liste
            lstDocuments = new ListBox
            {
                Location = new Point(40, 220),
                Width = 900,
                Height = 500,
                BackColor = Color.FromArgb(35, 40, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            lstDocuments.DoubleClick += LstDocuments_DoubleClick;
            this.Controls.Add(lstDocuments);
        }

        private void LoadDocuments()
        {
            lstDocuments.Items.Clear();
            var docs = _dataService.GetAllDocuments();

            if (docs.Count == 0)
            {
                lblInfo.Text = "Aucun document. Ajoutez une note ou un PDF pour commencer.";
                lblInfo.ForeColor = Color.FromArgb(255, 152, 0);
            }
            else
            {
                lblInfo.Text = $"{docs.Count} document(s). Double-clic pour ouvrir, sélectionnez puis Supprimer.";
                lblInfo.ForeColor = Color.FromArgb(180, 180, 180);
            }

            foreach (var doc in docs)
            {
                lstDocuments.Items.Add($"{doc.TypeDocument} - {doc.Titre}");
            }
        }

        private void BtnAddNote_Click(object? sender, EventArgs e)
        {
            var form = new Form
            {
                Text = "Nouvelle Note Technique",
                Size = new Size(700, 600),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(30, 35, 45),
                ForeColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            var lblTitre = new Label
            {
                Text = "Titre :",
                Location = new Point(20, 20),
                Width = 80,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold)
            };

            var txtTitre = new TextBox
            {
                Location = new Point(110, 17),
                Width = 550,
                BackColor = Color.FromArgb(40, 45, 55),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 11),
                Text = "Nouvelle technique"
            };

            var lblCat = new Label
            {
                Text = "Catégorie :",
                Location = new Point(20, 60),
                Width = 90,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold)
            };

            var cmbCategorie = new ComboBox
            {
                Location = new Point(110, 57),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(40, 45, 55),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10)
            };
            cmbCategorie.Items.AddRange(new object[] {
                "Extraction",
                "Forage",
                "Sécurité",
                "Prospection",
                "Géologie",
                "Matériel",
                "Réglementation",
                "Autre"
            });
            cmbCategorie.SelectedIndex = 0;

            var lblContenu = new Label
            {
                Text = "Contenu :",
                Location = new Point(20, 100),
                Width = 80,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold)
            };

            var txtContenu = new TextBox
            {
                Location = new Point(20, 130),
                Width = 640,
                Height = 350,
                Multiline = true,
                BackColor = Color.FromArgb(40, 45, 55),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10),
                ScrollBars = ScrollBars.Vertical,
                Text = "Décrivez ici la technique d'extraction miniére..."
            };

            var btnSave = new Button
            {
                Text = "Enregistrer",
                Location = new Point(20, 500),
                Width = 150,
                Height = 45,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtTitre.Text))
                {
                    MessageBox.Show("Le titre est obligatoire.", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var doc = new TechniqueDocument
                {
                    Titre = txtTitre.Text,
                    ContenuTexte = txtContenu.Text,
                    CategorieTechnique = cmbCategorie.SelectedItem?.ToString() ?? "Autre",
                    Description = $"Note créée le {DateTime.Now:dd/MM/yyyy HH:mm}"
                };

                _dataService.AddDocument(doc);
                LoadDocuments();
                form.Close();

                MessageBox.Show("Note enregistrée avec succés!", "Succés",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            var btnCancel = new Button
            {
                Text = "Annuler",
                Location = new Point(190, 500),
                Width = 120,
                Height = 45,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 11),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, ev) => form.Close();

            form.Controls.Add(lblTitre);
            form.Controls.Add(txtTitre);
            form.Controls.Add(lblCat);
            form.Controls.Add(cmbCategorie);
            form.Controls.Add(lblContenu);
            form.Controls.Add(txtContenu);
            form.Controls.Add(btnSave);
            form.Controls.Add(btnCancel);

            form.ShowDialog(this);
        }

        private void BtnAddPdf_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "PDF|*.pdf",
                Title = "Sélectionner un document PDF"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var doc = new TechniqueDocument
                {
                    Titre = Path.GetFileNameWithoutExtension(ofd.FileName),
                    CheminPdf = ofd.FileName,
                    CategorieTechnique = "Document PDF",
                    Description = $"PDF ajouté le {DateTime.Now:dd/MM/yyyy HH:mm}"
                };

                _dataService.AddDocument(doc);
                LoadDocuments();

                MessageBox.Show($"PDF '{doc.Titre}' ajouté avec succés!", "Succés",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (lstDocuments.SelectedIndex < 0)
            {
                MessageBox.Show("Sélectionnez un document à supprimer.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var docs = _dataService.GetAllDocuments();
            var doc = docs[lstDocuments.SelectedIndex];

            var result = MessageBox.Show(
                $"Supprimer '{doc.Titre}' ?\n\nCette action est irréversible.",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _dataService.DeleteDocument(doc.Id);
                LoadDocuments();
                MessageBox.Show("Document supprimé.", "Succés",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LstDocuments_DoubleClick(object? sender, EventArgs e)
        {
            if (lstDocuments.SelectedIndex < 0) return;

            var docs = _dataService.GetAllDocuments();
            var doc = docs[lstDocuments.SelectedIndex];

            if (doc.HasPdf)
            {
                if (File.Exists(doc.CheminPdf))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = doc.CheminPdf!,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de l'ouverture du PDF:\n{ex.Message}",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Le fichier PDF est introuvable:\n{doc.CheminPdf}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (doc.HasTexte)
            {
                // Afficher le contenu de la note
                var viewForm = new Form
                {
                    Text = doc.Titre,
                    Size = new Size(800, 600),
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.FromArgb(30, 35, 45),
                    ForeColor = Color.White
                };

                var txtView = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    ReadOnly = true,
                    Text = doc.ContenuTexte,
                    BackColor = Color.FromArgb(40, 45, 55),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Emoji", 11),
                    BorderStyle = BorderStyle.None,
                    ScrollBars = ScrollBars.Vertical,
                    Padding = new Padding(20)
                };

                viewForm.Controls.Add(txtView);
                viewForm.ShowDialog(this);
            }
        }
    }
}

