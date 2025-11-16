namespace wmine.Models
{
    /// <summary>
    /// Représente un contact professionnel lié aux activités miniéres
    /// </summary>
    public class Contact
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Prenom { get; set; }
        public string? Entreprise { get; set; }
        public string? Fonction { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Adresse { get; set; }
        public string? Specialite { get; set; }
        public string? Notes { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateModification { get; set; }

        public string NomComplet => string.IsNullOrWhiteSpace(Prenom) 
            ? Nom 
            : $"{Prenom} {Nom}";

        public string DisplayName => string.IsNullOrWhiteSpace(Entreprise)
            ? NomComplet
            : $"{NomComplet} - {Entreprise}";
    }
}
