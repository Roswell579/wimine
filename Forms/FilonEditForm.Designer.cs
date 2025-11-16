using wmine.UI;

namespace wmine.Forms
{
    partial class FilonEditForm
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtNom;
        private ColoredMineralListBox clbMatierePrincipale;
        private ColoredMineralListBox clbMatieresSecondaires;
        private SafetyCheckedListBox clbStatuts;
        private TextBox txtLambertX;
        private TextBox txtLambertY;
        private TextBox txtLatitude;
        private TextBox txtLongitude;
        private Button btnConvertLambert;
        private TextBox txtAnneeAncrage;
        private PictureBox picPhoto;
        private Button btnChoosePhoto;
        private Button btnRemovePhoto;
        private Button btnChooseDoc;
        private Label lblDocPath;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private Label label10;
        private Panel grpCoordinates;
        private Panel grpStatus;
        private Panel grpMedia;
        private Panel grpNotes;
        private Button btnToggleNotes;
        private Panel panelButtons;
        private Panel panelLeft;
        private Panel panelRight;
        private ModernToolTip tooltipAncrage;
        private ModernToolTip tooltipPdf;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            
            // Modern Tooltip pour l'ancrage (DANGER)
            tooltipAncrage = new ModernToolTip
            {
                IsDanger = true,
                AutoPopDelay = 10000,
                InitialDelay = 300,
                ReshowDelay = 100
            };

            // Modern Tooltip pour le PDF
            tooltipPdf = new ModernToolTip
            {
                IsDanger = false,
                AutoPopDelay = 5000,
                InitialDelay = 300,
                ReshowDelay = 100
            };
            
            // Form - Mode plein écran maximisé avec AutoScroll
            this.Text = "Edition de Filon Minier";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.AutoScroll = true;
            this.BackColor = Color.FromArgb(25, 25, 35);
            this.Opacity = 0;

            // Calcul dynamique basé sur la taille de l'écran
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            
            int leftCol = 40;
            int rightCol = screenWidth / 2 + 20;
            int labelWidth = 180;
            int controlWidth = Math.Min(450, screenWidth / 2 - leftCol - labelWidth - 80);

            // ==== COLONNE GAUCHE AVEC SCROLL ====
            panelLeft = new Panel
            {
                Location = new Point(leftCol, 20),
                Width = controlWidth + labelWidth + 40,
                Height = screenHeight - 80,
                BackColor = Color.FromArgb(30, 35, 45),
                AutoScroll = true
            };

            int innerYPos = 20;

            // Nom
            label1 = new Label 
            { 
                Text = "Nom du filon:", 
                Location = new Point(15, innerYPos), 
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            txtNom = new TextBox 
            { 
                Location = new Point(15 + labelWidth, innerYPos - 3), 
                Width = controlWidth,
                Font = new Font("Segoe UI Emoji", 10),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White
            };
            panelLeft.Controls.Add(label1);
            panelLeft.Controls.Add(txtNom);

            innerYPos += 45;

            // Matiére principale
            label2 = new Label 
            { 
                Text = "Matiere principale:", 
                Location = new Point(15, innerYPos), 
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            clbMatierePrincipale = new ColoredMineralListBox 
            { 
                Location = new Point(15 + labelWidth, innerYPos - 3), 
                Width = controlWidth,
                Height = 200,
                CheckOnClick = true,
                Font = new Font("Segoe UI Emoji", 9),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White
            };
            clbMatierePrincipale.ItemCheck += ClbMatierePrincipale_ItemCheck;
            panelLeft.Controls.Add(label2);
            panelLeft.Controls.Add(clbMatierePrincipale);

            innerYPos += 215;

            // Matiéres secondaires
            label3 = new Label 
            { 
                Text = "Matieres secondaires:", 
                Location = new Point(15, innerYPos), 
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            clbMatieresSecondaires = new ColoredMineralListBox 
            { 
                Location = new Point(15 + labelWidth, innerYPos - 3), 
                Width = controlWidth,
                Height = 200,
                CheckOnClick = true,
                Font = new Font("Segoe UI Emoji", 9),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White
            };
            panelLeft.Controls.Add(label3);
            panelLeft.Controls.Add(clbMatieresSecondaires);

            innerYPos += 215;

            // Groupe Coordonnées
            grpCoordinates = new Panel
            { 
                Location = new Point(15, innerYPos), 
                Width = controlWidth + labelWidth, 
                Height = 180,
                BackColor = Color.FromArgb(40, 45, 55)
            };
            
            var lblCoordTitle = new Label
            {
                Text = "Coordonnees",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                ForeColor = Color.Cyan,
                BackColor = Color.Transparent
            };
            grpCoordinates.Controls.Add(lblCoordTitle);

            // Lambert
            label4 = new Label 
            { 
                Text = "Lambert 3 X:", 
                Location = new Point(15, 40), 
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 9),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            txtLambertX = new TextBox 
            { 
                Location = new Point(130, 37), 
                Width = 120,
                Font = new Font("Segoe UI Emoji", 9),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White
            };
            label5 = new Label 
            { 
                Text = "Y:", 
                Location = new Point(260, 40), 
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 9),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            txtLambertY = new TextBox 
            { 
                Location = new Point(285, 37), 
                Width = 120,
                Font = new Font("Segoe UI Emoji", 9),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White
            };
            btnConvertLambert = new Button
            {
                Text = "GPS",
                Location = new Point(415, 35),
                Width = 100,
                Height = 35,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConvertLambert.FlatAppearance.BorderSize = 0;
            btnConvertLambert.Click += BtnConvertLambert_Click;

            // GPS
            label6 = new Label 
            { 
                Text = "GPS Latitude:", 
                Location = new Point(15, 85), 
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 9),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            txtLatitude = new TextBox 
            { 
                Location = new Point(130, 82), 
                Width = 120,
                Font = new Font("Segoe UI Emoji", 9),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White
            };
            label7 = new Label 
            { 
                Text = "Longitude:", 
                Location = new Point(260, 85), 
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 9),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            txtLongitude = new TextBox 
            { 
                Location = new Point(345, 82), 
                Width = 120,
                Font = new Font("Segoe UI Emoji", 9),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White
            };

            Label lblCoordInfo = new Label 
            { 
                Text = "Saisissez Lambert 3 Sud et cliquez -> GPS pour convertir", 
                Location = new Point(15, 125), 
                Width = 500,
                ForeColor = Color.FromArgb(100, 181, 246),
                Font = new Font("Segoe UI Emoji", 8, FontStyle.Italic),
                BackColor = Color.Transparent
            };

            grpCoordinates.Controls.AddRange(new Control[] 
            { 
                label4, txtLambertX, label5, txtLambertY, btnConvertLambert,
                label6, txtLatitude, label7, txtLongitude, lblCoordInfo
            });
            panelLeft.Controls.Add(grpCoordinates);

            innerYPos += 195;

            this.Controls.Add(panelLeft);

            // ==== COLONNE DROITE AVEC SCROLL === =
            int rightColWidth = Math.Min(550, screenWidth - rightCol - 60);
            
            panelRight = new Panel
            {
                Location = new Point(rightCol, 20),
                Width = rightColWidth + 20,
                Height = screenHeight - 80,
                BackColor = Color.FromArgb(30, 35, 45),
                AutoScroll = true
            };

            innerYPos = 20;

            // Statuts
            grpStatus = new Panel
            { 
                Location = new Point(15, innerYPos), 
                Width = rightColWidth - 30, 
                Height = 290,
                BackColor = Color.FromArgb(40, 45, 55)
            };
            
            var lblStatusTitle = new Label
            {
                Text = "Etat du filon",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                ForeColor = Color.Orange,
                BackColor = Color.Transparent
            };
            grpStatus.Controls.Add(lblStatusTitle);
            
            clbStatuts = new SafetyCheckedListBox 
            { 
                Location = new Point(15, 40), 
                Width = rightColWidth - 60,
                Height = 180,
                CheckOnClick = true,
                Font = new Font("Segoe UI Emoji", 9),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White
            };
            grpStatus.Controls.Add(clbStatuts);
            
            // ⚠️ ANCRAGE ANNÉE
            label8 = new Label 
            { 
                Text = "Ancrage (annee):", 
                Location = new Point(15, 230),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                ForeColor = Color.Red,
                BackColor = Color.Transparent
            };
            txtAnneeAncrage = new TextBox 
            { 
                Location = new Point(200, 227),
                Width = 150,
                PlaceholderText = "Laisser vide si inconnu",
                Font = new Font("Segoe UI Emoji", 10),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White
            };
            
            tooltipAncrage.SetToolTip(label8, wmine.Models.SafetyTooltips.ANCRAGE_WARNING);
            
            grpStatus.Controls.Add(label8);
            grpStatus.Controls.Add(txtAnneeAncrage);
            
            panelRight.Controls.Add(grpStatus);

            innerYPos += 305;

            // Médias
            grpMedia = new Panel
            { 
                Location = new Point(15, innerYPos), 
                Width = rightColWidth - 30, 
                Height = 350,
                BackColor = Color.FromArgb(40, 45, 55)
            };

            var lblMediaTitle = new Label
            {
                Text = "Photo et Documentation",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                ForeColor = Color.Magenta,
                BackColor = Color.Transparent
            };
            grpMedia.Controls.Add(lblMediaTitle);

            label9 = new Label 
            { 
                Text = "Photo:", 
                Location = new Point(15, 40), 
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            picPhoto = new PictureBox 
            { 
                Location = new Point(15, 65), 
                Width = 260, 
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(30, 30, 40)
            };
            btnChoosePhoto = new Button
            {
                Text = "Choisir",
                Location = new Point(285, 65),
                Width = 120,
                Height = 38,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnChoosePhoto.FlatAppearance.BorderSize = 0;
            btnChoosePhoto.Click += BtnChoosePhoto_Click;
            
            btnRemovePhoto = new Button
            {
                Text = "Supprimer",
                Location = new Point(285, 110),
                Width = 120,
                Height = 38,
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRemovePhoto.FlatAppearance.BorderSize = 0;
            btnRemovePhoto.Click += BtnRemovePhoto_Click;

            label10 = new Label 
            { 
                Text = "Documentation PDF:", 
                Location = new Point(285, 160), 
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            btnChooseDoc = new Button
            {
                Text = "Choisir PDF",
                Location = new Point(285, 185),
                Width = 180,
                Height = 38,
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnChooseDoc.FlatAppearance.BorderSize = 0;
            btnChooseDoc.Click += BtnChooseDoc_Click;
            
            lblDocPath = new Label 
            { 
                Text = "Aucun document", 
                Location = new Point(285, 230), 
                Width = 200,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI Emoji", 8, FontStyle.Italic | FontStyle.Underline),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            lblDocPath.Click += LblDocPath_Click;
            tooltipPdf.SetToolTip(lblDocPath, "Cliquez pour ouvrir le document PDF");

            // 📷 BOUTON GALERIE PHOTO
            var btnPhotoGallery = new Button
            {
                Text = "Galerie",
                Location = new Point(15, 275),
                Width = 130,
                Height = 38,
                BackColor = Color.FromArgb(103, 58, 183),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPhotoGallery.FlatAppearance.BorderSize = 0;
            btnPhotoGallery.Click += BtnPhotoGallery_Click;

            grpMedia.Controls.AddRange(new Control[] 
            { 
                label9, picPhoto, btnChoosePhoto, btnRemovePhoto, 
                label10, btnChooseDoc, lblDocPath, btnPhotoGallery
            });
            panelRight.Controls.Add(grpMedia);

            innerYPos += 365;

            // Notes
            grpNotes = new Panel
            { 
                Location = new Point(15, innerYPos), 
                Width = rightColWidth - 30,
                Height = 60,
                BackColor = Color.FromArgb(40, 45, 55)
            };

            var lblNotesTitle = new Label
            {
                Text = "Notes",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                ForeColor = Color.Cyan,
                BackColor = Color.Transparent
            };
            grpNotes.Controls.Add(lblNotesTitle);

            btnToggleNotes = new Button
            {
                Text = "Afficher Notes",
                Location = new Point(rightColWidth - 160, 8),
                Width = 140,
                Height = 35,
                BackColor = Color.FromArgb(96, 125, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnToggleNotes.FlatAppearance.BorderSize = 0;
            btnToggleNotes.Click += BtnToggleNotes_Click;

            txtNotes = new TextBox 
            { 
                Location = new Point(15, 50), 
                Width = rightColWidth - 60,
                Height = 120,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI Emoji", 9),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White,
                Visible = false
            };

            grpNotes.Controls.Add(btnToggleNotes);
            grpNotes.Controls.Add(txtNotes);
            panelRight.Controls.Add(grpNotes);

            innerYPos += 75;

            // Panel pour les boutons (SANS Import OCR - déplacé dans l'onglet)
            panelButtons = new Panel
            {
                Location = new Point(15, innerYPos),
                Width = rightColWidth - 30,
                Height = 80,
                BackColor = Color.FromArgb(35, 40, 50)
            };

            // BOUTON EXPORT KML (repositionné au début)
            var btnExportKml = new Button
            {
                Text = "Export KML",
                Location = new Point(10, 15),
                Width = 150,
                Height = 50,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExportKml.FlatAppearance.BorderSize = 0;
            btnExportKml.Click += BtnExportKml_Click;

            btnSave = new Button
            {
                Text = "Enregistrer",
                Location = new Point(rightColWidth - 310, 15),
                Width = 150,
                Height = 50,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Annuler",
                Location = new Point(rightColWidth - 150, 15),
                Width = 140,
                Height = 50,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.DialogResult = DialogResult.Cancel;

            //SEULEMENT 3 BOUTONS (Import OCR supprimé)
            panelButtons.Controls.Add(btnExportKml);
            panelButtons.Controls.Add(btnSave);
            panelButtons.Controls.Add(btnCancel);
            panelRight.Controls.Add(panelButtons);

            innerYPos += 100;
            
            this.Controls.Add(panelRight);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;

            // Tooltips
            var tooltipButtons = new ToolTip(this.components)
            {
                InitialDelay = 300,
                AutoPopDelay = 5000,
                ReshowDelay = 100,
                ShowAlways = true
            };

            tooltipButtons.SetToolTip(btnExportKml, "Exporter le filon au format KML pour Google Earth");
            tooltipButtons.SetToolTip(btnSave, "Enregistrer les modifications du filon");
            tooltipButtons.SetToolTip(btnCancel, "Annuler et fermer sans sauvegarder");
            tooltipButtons.SetToolTip(btnConvertLambert, "Convertir les coordonnées Lambert 3 en GPS WGS84");
            tooltipButtons.SetToolTip(btnChoosePhoto, "Sélectionner une photo pour ce filon");
            tooltipButtons.SetToolTip(btnRemovePhoto, "Supprimer la photo actuelle");
            tooltipButtons.SetToolTip(btnChooseDoc, "Sélectionner un document PDF de référence");
            tooltipButtons.SetToolTip(btnToggleNotes, "Afficher/masquer le champ de notes");
            tooltipButtons.SetToolTip(btnPhotoGallery, "Ouvrir la galerie de photos du filon");
        }

        private void BtnToggleNotes_Click(object? sender, EventArgs e)
        {
            if (txtNotes.Visible)
            {
                txtNotes.Visible = false;
                grpNotes.Height = 60;
                btnToggleNotes.Text = "Afficher Notes";
                panelButtons.Top = grpNotes.Bottom + 15;
            }
            else
            {
                txtNotes.Visible = true;
                grpNotes.Height = 190;
                btnToggleNotes.Text = " Masquer Notes";
                panelButtons.Top = grpNotes.Bottom + 15;
            }
        }
    }
}

