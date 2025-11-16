using System.Drawing;
using System.Windows.Forms;

namespace wmine.UI
{
    /// <summary>
    /// Classe de base pour créer des panels déplaçables (draggable)
    /// Les panels héritant de cette classe peuvent être déplacés avec la souris
    /// </summary>
    public class DraggablePanel : Panel
    {
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private Point _originalLocation;
        private Label? _dragHandle;
        private bool _showDragIndicator = true;

        public DraggablePanel()
        {
            InitializeDragging();
        }

        /// <summary>
        /// Active ou désactive l'indicateur de déplacement (icône ??)
        /// </summary>
        public bool ShowDragIndicator
        {
            get => _showDragIndicator;
            set
            {
                _showDragIndicator = value;
                if (_dragHandle != null)
                    _dragHandle.Visible = value;
            }
        }

        private void InitializeDragging()
        {
            // Créer l'indicateur de déplacement (icône en haut à droite)
            _dragHandle = new Label
            {
                // Use Segoe MDL2 Assets glyph for move/drag if available
                Text = "\uE762", // 'GlobalNavigationButton' / generic handle glyph
                Width = 25,
                Height = 25,
                Font = new Font("Segoe MDL2 Assets", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 181, 246),
                BackColor = Color.Transparent,
                Cursor = Cursors.SizeAll,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Positionner en haut à droite
            this.Resize += (s, e) => PositionDragHandle();
            this.Controls.Add(_dragHandle);
            _dragHandle.BringToFront();

            // Événements de déplacement sur l'indicateur
            _dragHandle.MouseDown += DragHandle_MouseDown;
            _dragHandle.MouseMove += DragHandle_MouseMove;
            _dragHandle.MouseUp += DragHandle_MouseUp;

            // Événements de déplacement sur le panel (en maintenant Ctrl)
            this.MouseDown += Panel_MouseDown;
            this.MouseMove += Panel_MouseMove;
            this.MouseUp += Panel_MouseUp;

            PositionDragHandle();
        }

        private void PositionDragHandle()
        {
            if (_dragHandle != null)
            {
                _dragHandle.Location = new Point(this.Width - 30, 5);
            }
        }

        private void DragHandle_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _dragStartPoint = e.Location;
                _originalLocation = this.Location;
                this.Cursor = Cursors.SizeAll;

                // Changer la couleur de l'indicateur
                if (_dragHandle != null)
                    _dragHandle.ForeColor = Color.Orange;
            }
        }

        private void DragHandle_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var newLocation = new Point(
                    _originalLocation.X + (e.Location.X - _dragStartPoint.X),
                    _originalLocation.Y + (e.Location.Y - _dragStartPoint.Y)
                );

                // Limiter au parent
                if (this.Parent != null)
                {
                    newLocation.X = Math.Max(0, Math.Min(newLocation.X, this.Parent.Width - this.Width));
                    newLocation.Y = Math.Max(0, Math.Min(newLocation.Y, this.Parent.Height - this.Height));
                }

                this.Location = newLocation;
            }
        }

        private void DragHandle_MouseUp(object? sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                this.Cursor = Cursors.Default;

                // Restaurer la couleur de l'indicateur
                if (_dragHandle != null)
                    _dragHandle.ForeColor = Color.FromArgb(100, 181, 246);

                // Sauvegarder la position (optionnel)
                SavePosition();
            }
        }

        private void Panel_MouseDown(object? sender, MouseEventArgs e)
        {
            // Déplacement avec Ctrl + Clic gauche sur le panel
            if (e.Button == MouseButtons.Left && (ModifierKeys & Keys.Control) == Keys.Control)
            {
                _isDragging = true;
                _dragStartPoint = e.Location;
                _originalLocation = this.Location;
                this.Cursor = Cursors.SizeAll;
            }
        }

        private void Panel_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var newLocation = new Point(
                    _originalLocation.X + (e.X - _dragStartPoint.X),
                    _originalLocation.Y + (e.Y - _dragStartPoint.Y)
                );

                // Limiter au parent
                if (this.Parent != null)
                {
                    newLocation.X = Math.Max(0, Math.Min(newLocation.X, this.Parent.Width - this.Width));
                    newLocation.Y = Math.Max(0, Math.Min(newLocation.Y, this.Parent.Height - this.Height));
                }

                this.Location = newLocation;
            }
        }

        private void Panel_MouseUp(object? sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                this.Cursor = Cursors.Default;
                SavePosition();
            }
        }

        /// <summary>
        /// Sauvegarde la position du panel (peut être surchargée)
        /// </summary>
        protected virtual void SavePosition()
        {
            // Par défaut ne fait rien
            // Les classes dérivées peuvent surcharger pour sauvegarder dans les paramètres
        }

        /// <summary>
        /// Restaure la position sauvegardée (peut être surchargée)
        /// </summary>
        protected virtual void RestorePosition()
        {
            // Par défaut ne fait rien
            // Les classes dérivées peuvent surcharger pour restaurer depuis les paramètres
        }

        /// <summary>
        /// Réinitialise la position à la position par défaut
        /// </summary>
        public void ResetPosition(Point defaultPosition)
        {
            this.Location = defaultPosition;
            SavePosition();
        }

        /// <summary>
        /// Active ou désactive le déplacement
        /// </summary>
        public void EnableDragging(bool enabled)
        {
            if (_dragHandle != null)
                _dragHandle.Visible = enabled;
        }
    }
}
