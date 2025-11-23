using System.ComponentModel;
using wmine.Services;

namespace wmine.Models
{
    /// <summary>
    /// Représente un filon minier avec toutes ses propriétés
    /// </summary>
    public class Filon : INotifyPropertyChanged
    {
        private string _nom = string.Empty;
        private MineralType _matierePrincipale;
        private List<MineralType> _matieresSecondaires = new();
        private FilonStatus _statut;
        private double? _latitude;
        private double? _longitude;
        private double? _lambertX;
        private double? _lambertY;
        private string? _photoPath;
        private string? _documentationPath;
        private int? _anneeAncrage;
        private string _notes = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Nom
        {
            get => _nom;
            set
            {
                _nom = value;
                OnPropertyChanged(nameof(Nom));
            }
        }

        public MineralType MatierePrincipale
        {
            get => _matierePrincipale;
            set
            {
                _matierePrincipale = value;
                OnPropertyChanged(nameof(MatierePrincipale));
            }
        }

        public List<MineralType> MatieresSecondaires
        {
            get => _matieresSecondaires;
            set
            {
                _matieresSecondaires = value;
                OnPropertyChanged(nameof(MatieresSecondaires));
            }
        }

        public FilonStatus Statut
        {
            get => _statut;
            set
            {
                _statut = value;
                OnPropertyChanged(nameof(Statut));
            }
        }

        // Coordonnées GPS (WGS84)
        public double? Latitude
        {
            get => _latitude;
            set
            {
                _latitude = value;
                OnPropertyChanged(nameof(Latitude));
            }
        }

        public double? Longitude
        {
            get => _longitude;
            set
            {
                _longitude = value;
                OnPropertyChanged(nameof(Longitude));
            }
        }

        // Coordonnées Lambert 3 Zone Sud
        public double? LambertX
        {
            get => _lambertX;
            set
            {
                _lambertX = value;
                OnPropertyChanged(nameof(LambertX));
            }
        }

        public double? LambertY
        {
            get => _lambertY;
            set
            {
                _lambertY = value;
                OnPropertyChanged(nameof(LambertY));
            }
        }

        public string? PhotoPath
        {
            get => _photoPath;
            set
            {
                _photoPath = value;
                OnPropertyChanged(nameof(PhotoPath));
            }
        }

        public string? DocumentationPath
        {
            get => _documentationPath;
            set
            {
                _documentationPath = value;
                OnPropertyChanged(nameof(DocumentationPath));
            }
        }

        public int? AnneeAncrage
        {
            get => _anneeAncrage;
            set
            {
                _anneeAncrage = value;
                OnPropertyChanged(nameof(AnneeAncrage));
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime DateModification { get; set; } = DateTime.Now;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool HasCoordinates()
        {
            return (Latitude.HasValue && Longitude.HasValue) ||
                   (LambertX.HasValue && LambertY.HasValue);
        }

        /// <summary>
        /// Normalise les coordonnées Lambert et assure que les coordonnées GPS sont présentes
        /// </summary>
        public void NormalizeAndSyncCoordinates()
        {
            // Normaliser les coordonnées Lambert si présentes
            if (LambertX.HasValue && LambertY.HasValue)
            {
                var (xNorm, yNorm) = CoordinateConverter.NormalizeToMeters(LambertX.Value, LambertY.Value);
                LambertX = xNorm;
                LambertY = yNorm;

                // Si les coordonnées GPS manquent, les calculer à partir du Lambert
                if (!Latitude.HasValue || !Longitude.HasValue)
                {
                    var (lat, lon) = CoordinateConverter.Lambert3ToWGS84(LambertX.Value, LambertY.Value);
                    Latitude = lat;
                    Longitude = lon;
                }
            }
            // Si on a seulement les coordonnées GPS, calculer Lambert
            else if (Latitude.HasValue && Longitude.HasValue && (!LambertX.HasValue || !LambertY.HasValue))
            {
                var (x, y) = CoordinateConverter.WGS84ToLambert3(Latitude.Value, Longitude.Value);
                LambertX = x;
                LambertY = y;
            }
        }

        /// <summary>
        /// Tentative sûre pour obtenir des coordonnées WGS84
        /// Normalise d'abord les coordonnées Lambert si présentes
        /// Retourne true si latitude/longitude valides sont disponibles
        /// </summary>
        public bool TryGetWgs84(out double latitude, out double longitude)
        {
            // S'assurer que la conversion Lambert -> WGS84 a été exécutée si nécessaire
            NormalizeAndSyncCoordinates();

            if (Latitude.HasValue && Longitude.HasValue)
            {
                latitude = Latitude.Value;
                longitude = Longitude.Value;
                return true;
            }

            latitude = 0;
            longitude = 0;
            return false;
        }

        /// <summary>
        /// Vérifie si les coordonnées sont cohérentes avec la région Var/Alpes-Maritimes
        /// </summary>
        public bool AreCoordinatesValid()
        {
            if (Latitude.HasValue && Longitude.HasValue)
            {
                return CoordinateConverter.IsValidWGS84Var(Latitude.Value, Longitude.Value);
            }

            if (LambertX.HasValue && LambertY.HasValue)
            {
                return CoordinateConverter.IsValidLambert3Var(LambertX.Value, LambertY.Value);
            }

            return false;
        }
    }
}
