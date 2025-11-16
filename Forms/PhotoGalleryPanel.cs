using System.Drawing;
using wmine.Services;
using wmine.Models;

namespace wmine.Forms
{
    public class PhotoGalleryPanel : Panel
    {
        private readonly PhotoService _photoService;
        private readonly MineralType? _mineralType;
        private readonly int? _filonId;
        private FlowLayoutPanel _thumbPanel;

        public PhotoGalleryPanel(PhotoService photoService, MineralType? mineralType = null, int? filonId = null)
        {
            _photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));
            _mineralType = mineralType;
            _filonId = filonId;
            Initialize();
            LoadPhotos();
        }

        private void Initialize()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(30, 35, 45);

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(25, 25, 35)
            };

            var lblTitle = new Label
            {
                Text = _mineralType.HasValue ? $"Photos: {MineralColors.GetDisplayName(_mineralType.Value)}" : (_filonId.HasValue ? $"Photos Filon #{_filonId}" : "Photos"),
                Location = new Point(12, 12),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold)
            };
            toolbar.Controls.Add(lblTitle);

            var btnOpenFolder = new Button
            {
                Text = "Ouvrir dossier",
                Location = new Point(300, 12),
                Width = 120,
                Height = 32,
                BackColor = Color.FromArgb(60, 65, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOpenFolder.FlatAppearance.BorderSize = 0;
            btnOpenFolder.Click += (s, e) =>
            {
                if (_filonId.HasValue)
                    _photoService.OpenFilonPhotosFolder(_filonId.Value);
                else
                    _photo_service_open_minerals();
            };
            toolbar.Controls.Add(btnOpenFolder);

            _thumbPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(12)
            };

            this.Controls.Add(_thumbPanel);
            this.Controls.Add(toolbar);
        }

        private void _photo_service_open_minerals()
        {
            _photoService.OpenMineralsPhotosFolder();
        }

        private void LoadPhotos()
        {
            _thumbPanel.Controls.Clear();

            List<string> photos;
            if (_filonId.HasValue)
            {
                photos = _photoService.GetFilonPhotos(_filonId.Value);
            }
            else if (_mineralType.HasValue)
            {
                var path = _photoService.GetMineralPhotoPath(_mineralType.Value);
                photos = path != null ? new List<string> { path } : new List<string>();
            }
            else
            {
                photos = new List<string>();
            }

            foreach (var photoPath in photos)
            {
                var card = CreateThumbCard(photoPath);
                _thumbPanel.Controls.Add(card);
            }

            if (photos.Count == 0)
            {
                var lbl = new Label
                {
                    Text = "Aucune photo disponible.",
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(20, 20)
                };
                _thumb_panel_add(lbl);
            }
        }

        private void _thumb_panel_add(Control c)
        {
            _thumbPanel.Controls.Add(c);
        }

        private Control CreateThumbCard(string photoPath)
        {
            var panel = new Panel
            {
                Width = 220,
                Height = 260,
                Margin = new Padding(8),
                BackColor = Color.FromArgb(40, 45, 55)
            };

            var pic = new PictureBox
            {
                Width = 200,
                Height = 200,
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = _photoService.CreateThumbnail(photoPath, 200, 200)
            };
            pic.Cursor = Cursors.Hand;
            pic.Click += (s, e) => _photoService.OpenPhoto(photoPath);

            var lbl = new Label
            {
                Text = Path.GetFileName(photoPath),
                Location = new Point(10, 216),
                Width = 200,
                Height = 28,
                ForeColor = Color.White
            };

            panel.Controls.Add(pic);
            panel.Controls.Add(lbl);
            return panel;
        }
    }
}
