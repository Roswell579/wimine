using System.Drawing;
using System.Windows.Forms;

namespace wmine.UI
{
    /// <summary>
    /// PictureBox zoomable avec prévisualisation au survol
    /// </summary>
    public class ZoomablePictureBox : PictureBox
    {
        private Form? _zoomForm;
        private bool _isHovering = false;

        public ZoomablePictureBox()
        {
            this.SizeMode = PictureBoxSizeMode.Zoom;
            this.Cursor = Cursors.Hand;
            this.BorderStyle = BorderStyle.FixedSingle;

            this.MouseEnter += OnMouseEnterHandler;
            this.MouseLeave += OnMouseLeaveHandler;
            this.Click += OnClickHandler;
        }

        private void OnMouseEnterHandler(object? sender, EventArgs e)
        {
            _isHovering = true;

            if (this.Image == null)
                return;

            // Créer une fenétre de prévisualisation au survol
            _zoomForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                Size = new Size(300, 300),
                BackColor = Color.FromArgb(30, 35, 45),
                ShowInTaskbar = false,
                TopMost = true,
                Opacity = 0.95
            };

            var pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = this.Image,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            _zoomForm.Controls.Add(pic);

            // Positionner é cété de la miniature
            var screenPos = this.PointToScreen(new Point(this.Width + 10, 0));
            _zoomForm.Location = screenPos;

            _zoomForm.Show();
        }

        private void OnMouseLeaveHandler(object? sender, EventArgs e)
        {
            _isHovering = false;

            if (_zoomForm != null)
            {
                _zoomForm.Close();
                _zoomForm.Dispose();
                _zoomForm = null;
            }
        }

        private void OnClickHandler(object? sender, EventArgs e)
        {
            if (this.Image == null)
            {
                // Permettre de choisir une image
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                    ofd.Title = "Sélectionner une photo";

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            this.Image = Image.FromFile(ofd.FileName);
                            this.Tag = ofd.FileName; // Stocker le chemin
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Erreur lors du chargement de l'image: {ex.Message}",
                                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                // Ouvrir en plein écran
                OpenFullScreen();
            }
        }

        private void OpenFullScreen()
        {
            if (this.Image == null)
                return;

            var fullScreenForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                WindowState = FormWindowState.Maximized,
                BackColor = Color.Black,
                StartPosition = FormStartPosition.CenterScreen
            };

            var pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = this.Image,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black
            };

            pic.Click += (s, e) => fullScreenForm.Close();

            var lblClose = new Label
            {
                Text = "? Cliquer pour fermer",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(150, 0, 0, 0),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 14, FontStyle.Bold),
                Padding = new Padding(15)
            };

            lblClose.Location = new Point(
                (fullScreenForm.Width - lblClose.Width) / 2,
                fullScreenForm.Height - 100
            );

            fullScreenForm.Controls.Add(pic);
            fullScreenForm.Controls.Add(lblClose);
            lblClose.BringToFront();

            fullScreenForm.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    fullScreenForm.Close();
            };

            fullScreenForm.ShowDialog();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_zoomForm != null)
                {
                    _zoomForm.Close();
                    _zoomForm.Dispose();
                    _zoomForm = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}

