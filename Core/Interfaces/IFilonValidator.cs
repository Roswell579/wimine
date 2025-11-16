using wmine.Models;

namespace wmine.Core.Interfaces
{
    /// <summary>
    /// Interface pour la validation des filons
    /// </summary>
    public interface IFilonValidator
    {
        ValidationResult Validate(Filon filon);
    }

    /// <summary>
    /// Résultat de validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; }

        public ValidationResult(List<string> errors)
        {
            Errors = errors ?? new List<string>();
        }
    }
}
