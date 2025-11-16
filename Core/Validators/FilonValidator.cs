using wmine.Core.Interfaces;
using wmine.Models;

namespace wmine.Core.Validators
{
    /// <summary>
    /// Validateur pour les objets Filon
    /// </summary>
    public class FilonValidator : IFilonValidator
    {
        public ValidationResult Validate(Filon filon)
        {
            var errors = new List<string>();

            if (filon == null)
            {
                errors.Add("Le filon ne peut pas étre null");
                return new ValidationResult(errors);
            }

            // Validation du nom
            if (string.IsNullOrWhiteSpace(filon.Nom))
            {
                errors.Add("Le nom du filon est requis");
            }
            else if (filon.Nom.Length > 200)
            {
                errors.Add("Le nom du filon ne peut pas dépasser 200 caractéres");
            }

            // Validation des coordonnées GPS
            if (filon.Latitude.HasValue)
            {
                if (filon.Latitude < -90 || filon.Latitude > 90)
                {
                    errors.Add("La latitude doit étre comprise entre -90 et 90 degrés");
                }
            }

            if (filon.Longitude.HasValue)
            {
                if (filon.Longitude < -180 || filon.Longitude > 180)
                {
                    errors.Add("La longitude doit étre comprise entre -180 et 180 degrés");
                }
            }

            // Validation des coordonnées Lambert
            if (filon.LambertX.HasValue && filon.LambertY.HasValue)
            {
                // Vérifier que les coordonnées Lambert sont dans une plage raisonnable pour la France
                if (filon.LambertX < 0 || filon.LambertX > 1300000)
                {
                    errors.Add("La coordonnée Lambert X semble invalide");
                }
                if (filon.LambertY < 0 || filon.LambertY > 1200000)
                {
                    errors.Add("La coordonnée Lambert Y semble invalide");
                }
            }

            // Validation des notes
            if (!string.IsNullOrEmpty(filon.Notes) && filon.Notes.Length > 5000)
            {
                errors.Add("Les notes ne peuvent pas dépasser 5000 caractéres");
            }

            // Validation du chemin photo
            if (!string.IsNullOrEmpty(filon.PhotoPath))
            {
                if (filon.PhotoPath.Length > 500)
                {
                    errors.Add("Le chemin de la photo est trop long");
                }
            }

            // Validation du chemin documentation
            if (!string.IsNullOrEmpty(filon.DocumentationPath))
            {
                if (filon.DocumentationPath.Length > 500)
                {
                    errors.Add("Le chemin de la documentation est trop long");
                }
                else if (!string.IsNullOrEmpty(filon.DocumentationPath) && 
                         !filon.DocumentationPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add("La documentation doit étre un fichier PDF");
                }
            }

            return new ValidationResult(errors);
        }

        /// <summary>
        /// Valide si le filon a au moins une position définie
        /// </summary>
        public bool HasValidPosition(Filon filon)
        {
            return (filon.Latitude.HasValue && filon.Longitude.HasValue) ||
                   (filon.LambertX.HasValue && filon.LambertY.HasValue);
        }

        /// <summary>
        /// Valide si les fichiers référencés existent
        /// </summary>
        public List<string> ValidateFiles(Filon filon)
        {
            var errors = new List<string>();

            if (!string.IsNullOrEmpty(filon.PhotoPath))
            {
                if (Directory.Exists(filon.PhotoPath))
                {
                    var files = Directory.GetFiles(filon.PhotoPath);
                    if (files.Length == 0)
                    {
                        errors.Add("Le dossier photos est vide");
                    }
                }
                else if (File.Exists(filon.PhotoPath))
                {
                    // C'est un fichier individuel, OK
                }
                else
                {
                    errors.Add($"Le chemin photo n'existe pas: {filon.PhotoPath}");
                }
            }

            if (!string.IsNullOrEmpty(filon.DocumentationPath))
            {
                if (!File.Exists(filon.DocumentationPath))
                {
                    errors.Add($"Le fichier de documentation n'existe pas: {filon.DocumentationPath}");
                }
            }

            return errors;
        }
    }
}
