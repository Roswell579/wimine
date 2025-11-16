using System.Drawing;
using wmine.Services;
using wmine.Models;

namespace wmine.Forms
{
    /// <summary>
    /// Panel de contacts éditables avec 6 emplacements pré-remplis
    /// </summary>
    public class ContactsPanel : Panel
    {
        private readonly ContactsDataService _dataService;
        private readonly IMineralRepository _mineralRepository;
        private readonly PhotoService _photoService;
        private FlowLayoutPanel _contactsFlow;

        public ContactsPanel(ContactsDataService? dataService = null, IMineralRepository? mineralRepository = null, PhotoService? photoService = null)
        {
            _dataService = dataService ?? new ContactsDataService();
            _mineralRepository = mineralRepository ?? Services.AppServiceProvider.GetService<IMineralRepository>() ?? new wmine.Services.MineralRepository();
            _photoService = photoService ?? Services.AppServiceProvider.GetService<PhotoService>() ?? new PhotoService();
            InitializeComponents();
            LoadContacts();
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
                Text = "?? Contacts et Ressources",
                Location = new Point(40, 40),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 150, 136)
            };
            this.Controls.Add(lblTitle);

            // Info
            var lblInfo = new Label
            {
                Text = "6 contacts éditables. Double-cliquez sur une carte pour éditer.",
                Location = new Point(40, 100),
                Width = 800,
                Height = 30,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 180, 180)
            };
            this.Controls.Add(lblInfo);

            // FlowLayout pour les cartes
            _contactsFlow = new FlowLayoutPanel
            {
                Location = new Point(40, 150),
                Width = 1200,
                Height = 700,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            this.Controls.Add(_contactsFlow);
        }

        private void LoadContacts()
        {
            _contactsFlow.Controls.Clear();
            var contacts = _data_service_getall();

            foreach (var contact in contacts)
            {
                var card = CreateContactCard(contact);
                _contactsFlow.Controls.Add(card);
            }
        }

        // helper to get contacts list with stable ordering
        private List<Contact> _data_service_getall()
        {
            return _dataService.GetAllContacts().OrderBy(c => c.Id).Take(6).ToList();
        }

        private Panel CreateContactCard(Contact contact)
        {
            var panel = new Panel
            {
                Width = 1100,
                Height = 180,
                Margin = new Padding(0, 0, 0, 15),
                BackColor = Color.FromArgb(30, 35, 45),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };

            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(100, 181, 246), 2);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            panel.DoubleClick += (s, e) => EditContact(contact);

            // Barre colorée
            var colorBar = new Panel
            {
                Location = new Point(0, 0),
                Width = 5,
                Height = 180,
                BackColor = Color.FromArgb(0, 150, 136)
            };
            panel.Controls.Add(colorBar);

            // Nom
            var lblNom = new Label
            {
                Text = contact.DisplayName,
                Location = new Point(20, 15),
                Width = 1050,
                Height = 35,
                Font = new Font("Segoe UI Emoji", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 181, 246)
            };
            lblNom.DoubleClick += (s, e) => EditContact(contact);
            panel.Controls.Add(lblNom);

            int yInfo = 60;

            // Téléphone
            if (!string.IsNullOrWhiteSpace(contact.Telephone))
            {
                var lblTel = new Label
                {
                    Text = $"📞 {contact.Telephone}",
                    Location = new Point(20, yInfo),
                    AutoSize = true,
                    Font = new Font("Segoe UI Emoji", 11),
                    ForeColor = Color.White
                };
                lblTel.DoubleClick += (s, e) => EditContact(contact);
                panel.Controls.Add(lblTel);
                yInfo += 25;
            }

            // Email
            if (!string.IsNullOrWhiteSpace(contact.Email))
            {
                var lblEmail = new Label
                {
                    Text = $"✉️ {contact.Email}",
                    Location = new Point(20, yInfo),
                    AutoSize = true,
                    Font = new Font("Segoe UI Emoji", 11),
                    ForeColor = Color.White
                };
                lblEmail.DoubleClick += (s, e) => EditContact(contact);
                panel.Controls.Add(lblEmail);
                yInfo += 25;
            }

            // Entreprise/Fonction
            string infoLine = "";
            if (!string.IsNullOrWhiteSpace(contact.Fonction))
                infoLine = contact.Fonction;
            if (!string.IsNullOrWhiteSpace(contact.Entreprise))
                infoLine += (infoLine.Length > 0 ? " - " : "") + contact.Entreprise;

            if (!string.IsNullOrWhiteSpace(infoLine))
            {
                var lblInfo = new Label
                {
                    Text = $"🏢 {infoLine}",
                    Location = new Point(20, yInfo),
                    Width = 1050,
                    Height = 25,
                    Font = new Font("Segoe UI Emoji", 10, FontStyle.Italic),
                    ForeColor = Color.FromArgb(180, 180, 180)
                };
                lblInfo.DoubleClick += (s, e) => EditContact(contact);
                panel.Controls.Add(lblInfo);
                yInfo += 30;
            }

            // Spécialité
            if (!string.IsNullOrWhiteSpace(contact.Specialite))
            {
                var lblSpec = new Label
                {
                    Text = contact.Specialite,
                    Location = new Point(20, yInfo),
                    Width = 1050,
                    Height = 60,
                    Font = new Font("Segoe UI Emoji", 9),
                    ForeColor = Color.FromArgb(200, 200, 200)
                };
                lblSpec.DoubleClick += (s, e) => EditContact(contact);
                panel.Controls.Add(lblSpec);
            }

            // Icône édition
            var lblEdit = new Label
            {
                Text = "✏️",
                Location = new Point(1050, 10),
                Width = 30,
                Height = 30,
                Font = new Font("Segoe UI Emoji", 16),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            lblEdit.Click += (s, e) => EditContact(contact);
            panel.Controls.Add(lblEdit);

            return panel;
        }

        private void EditContact(Contact contact)
        {
            var form = new Form
            {
                Text = $"✏️ éditer: {contact.NomComplet}",
                Size = new Size(700, 700),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(30, 35, 45),
                ForeColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            int y = 20;

            var txtNom = AddTextBox(form, "Nom:", contact.Nom, 20, ref y);
            var txtPrenom = AddTextBox(form, "Prénom:", contact.Prenom ?? "", 20, ref y);
            var txtEntreprise = AddTextBox(form, "Entreprise:", contact.Entreprise ?? "", 20, ref y);
            var txtFonction = AddTextBox(form, "Fonction:", contact.Fonction ?? "", 20, ref y);
            var txtTel = AddTextBox(form, "Téléphone:", contact.Telephone ?? "", 20, ref y);
            var txtEmail = AddTextBox(form, "Email:", contact.Email ?? "", 20, ref y);
            var txtAdresse = AddTextBox(form, "Adresse:", contact.Adresse ?? "", 20, ref y, 60);
            var txtSpec = AddTextBox(form, "Spécialité:", contact.Specialite ?? "", 20, ref y, 80);
            var txtNotes = AddTextBox(form, "Notes:", contact.Notes ?? "", 20, ref y, 100);

            var btnSave = new Button
            {
                Text = "💾 Enregistrer",
                Location = new Point(20, y + 10),
                Width = 180,
                Height = 45,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, ev) =>
            {
                contact.Nom = txtNom.Text;
                contact.Prenom = txtPrenom.Text;
                contact.Entreprise = txtEntreprise.Text;
                contact.Fonction = txtFonction.Text;
                contact.Telephone = txtTel.Text;
                contact.Email = txtEmail.Text;
                contact.Adresse = txtAdresse.Text;
                contact.Specialite = txtSpec.Text;
                contact.Notes = txtNotes.Text;

                _dataService.UpdateContact(contact);
                LoadContacts();
                form.Close();

                MessageBox.Show("✅ Contact mis à jour avec succès!", "Succès",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            var btnCancel = new Button
            {
                Text = "Annuler",
                Location = new Point(220, y + 10),
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

            form.Controls.Add(btnSave);
            form.Controls.Add(btnCancel);

            form.ShowDialog(this);
        }

        private TextBox AddTextBox(Form form, string label, string value, int x, ref int y, int height = 30)
        {
            var lbl = new Label
            {
                Text = label,
                Location = new Point(x, y),
                Width = 120,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 11, FontStyle.Bold)
            };
            form.Controls.Add(lbl);

            var txt = new TextBox
            {
                Text = value,
                Location = new Point(x + 130, y - 2),
                Width = 520,
                Height = height,
                Multiline = height > 30,
                BackColor = Color.FromArgb(40, 45, 55),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10),
                ScrollBars = height > 30 ? ScrollBars.Vertical : ScrollBars.None
            };
            form.Controls.Add(txt);

            y += height + 15;
            return txt;
        }
    }
}

