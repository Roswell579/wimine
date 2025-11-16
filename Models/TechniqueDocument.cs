namespace wmine.Models
{
    /// <summary>
    /// Représente un document technique ou une note sur les techniques d'extraction
    /// </summary>
    public class TechniqueDocument
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CategorieTechnique { get; set; } // Ex: "Extraction", "Forage", "Sécurité", etc.
        public string? CheminPdf { get; set; }
        public string? ContenuTexte { get; set; } // Pour les notes manuelles
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateModification { get; set; }

        public bool HasPdf => !string.IsNullOrWhiteSpace(CheminPdf) && File.Exists(CheminPdf);
        public bool HasTexte => !string.IsNullOrWhiteSpace(ContenuTexte);

        public string TypeDocument => HasPdf ? "PDF" : "Note";
    }
}
