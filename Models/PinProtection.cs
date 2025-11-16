using System.Security.Cryptography;
using System.Text;

namespace wmine.Models
{
    /// <summary>
    /// Gestion de la protection PIN pour la premiére photo d'un filon
    /// </summary>
    public class PinProtection
    {
        /// <summary>
        /// PIN crypté avec SHA256
        /// </summary>
        public string? EncryptedPin { get; set; }

        /// <summary>
        /// Indique si la photo est actuellement verrouillée
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Date et heure du dernier déverrouillage
        /// </summary>
        public DateTime? LastUnlockTime { get; set; }

        /// <summary>
        /// Crypte un PIN avec SHA256
        /// </summary>
        /// <param name="pin">PIN à 4 chiffres</param>
        /// <returns>Hash SHA256 en Base64</returns>
        /// <exception cref="ArgumentException">Si le PIN n'est pas valide</exception>
        public static string EncryptPin(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
                throw new ArgumentException("Le PIN ne peut pas étre vide", nameof(pin));

            if (pin.Length != 4)
                throw new ArgumentException("Le PIN doit contenir exactement 4 chiffres", nameof(pin));

            if (!pin.All(char.IsDigit))
                throw new ArgumentException("Le PIN doit contenir uniquement des chiffres", nameof(pin));

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(pin);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Valide un PIN saisi par l'utilisateur
        /// </summary>
        /// <param name="inputPin">PIN à vérifier</param>
        /// <returns>True si le PIN est correct</returns>
        public bool ValidatePin(string inputPin)
        {
            // Si aucun PIN n'est défini, accés libre
            if (string.IsNullOrWhiteSpace(EncryptedPin))
                return true;

            try
            {
                var inputHash = EncryptPin(inputPin);
                return inputHash == EncryptedPin;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// définit un nouveau PIN
        /// </summary>
        /// <param name="newPin">Nouveau PIN à 4 chiffres</param>
        public void SetPin(string newPin)
        {
            EncryptedPin = EncryptPin(newPin);
            IsLocked = true;
        }

        /// <summary>
        /// déverrouille la protection PIN
        /// </summary>
        public void Unlock()
        {
            IsLocked = false;
            LastUnlockTime = DateTime.Now;
        }

        /// <summary>
        /// Verrouille la protection PIN
        /// </summary>
        public void Lock()
        {
            IsLocked = true;
        }

        /// <summary>
        /// Vérifie si un PIN est défini
        /// </summary>
        public bool HasPin()
        {
            return !string.IsNullOrWhiteSpace(EncryptedPin);
        }

        /// <summary>
        /// Supprime le PIN (désactive la protection)
        /// </summary>
        public void RemovePin()
        {
            EncryptedPin = null;
            IsLocked = false;
            LastUnlockTime = null;
        }
    }
}
