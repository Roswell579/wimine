using System.ComponentModel;

namespace wmine.Models
{
    /// <summary>
    /// états possibles d'un filon minier
    /// </summary>
    [Flags]
    public enum FilonStatus
    {
        [Description("Aucun")]
        Aucun = 0,

        [Description("Visitable")]
        Visitable = 1,

        [Description("Pas Retrouvé")]
        PasRetrouvé = 2,

        [Description("Noyé")]
        Noyé = 4,

        [Description("Urbanisé")]
        Urbanisé = 8,

        [Description("DANGER MORTEL")]
        DangerMortel = 16,

        [Description("Porte Chauve-Souris")]
        PorteChauvesSouris = 32,

        [Description("Effondré")]
        Effondré = 64,

        [Description("Foudroyé")]
        Foudroyé = 128
    }

    /// <summary>
    /// Extensions pour le formatage spécial des statuts
    /// </summary>
    public static class FilonStatusExtensions
    {
        public static string GetDisplayName(this FilonStatus status)
        {
            var field = status.GetType().GetField(status.ToString());
            if (field == null) return status.ToString();

            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? status.ToString();
        }

        /// <summary>
        /// Retourne la couleur pour affichage
        /// </summary>
        public static Color GetDisplayColor(this FilonStatus status)
        {
            return status switch
            {
                FilonStatus.DangerMortel => Color.Red,
                FilonStatus.Noyé => Color.DodgerBlue,
                _ => Color.Black
            };
        }

        /// <summary>
        /// Retourne le tooltip de sécurité
        /// </summary>
        public static string? GetSafetyTooltip(this FilonStatus status)
        {
            return status switch
            {
                FilonStatus.DangerMortel => "ENTREE STRICTEMENT INTERDITE\nRISQUE DE MORT IMMINENT ?",
                _ => null
            };
        }
    }
}
