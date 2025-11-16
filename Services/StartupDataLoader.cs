using wmine.Models;
using wmine.Services;

namespace wmine.Services
{
    /// <summary>
    /// Service d'initialisation des données au premier démarrage
    /// </summary>
    public class StartupDataLoader
    {
        private readonly FilonDataService _filonService;

        public StartupDataLoader(FilonDataService filonService)
        {
            _filonService = filonService;
        }

        /// <summary>
        /// Charge automatiquement les mines historiques du Var si la base est vide
        /// </summary>
        public void LoadInitialDataIfEmpty()
        {
            var existingFilons = _filonService.GetAllFilons();

            // Si déjé des filons, ne rien faire
            if (existingFilons.Any())
            {
                return;
            }

            // Charger automatiquement toutes les mines historiques
            var mines = MinesVarDataService.GetAllMines();

            int addedCount = 0;

            foreach (var mine in mines)
            {
                var filon = new Filon
                {
                    Nom = mine.Nom,
                    Latitude = mine.Latitude,
                    Longitude = mine.Longitude,
                    MatierePrincipale = mine.MinéralPrincipal,
                    Notes = $"{mine.Description}\n\n" +
                           $"Commune: {mine.Commune}\n" +
                           $"Période: {mine.PériodeExploitation}\n" +
                           $"Statut: {mine.Statut}\n\n" +
                           $"Minéraux secondaires: {string.Join(", ", mine.MinérauxSecondaires)}\n\n" +
                           $"Source: {mine.Source}",
                    DateCreation = DateTime.Now,
                    Statut = FilonStatus.Aucun
                };

                _filonService.AddFilon(filon);
                addedCount++;
            }

            // Log silencieux (pas de MessageBox au démarrage)
            System.Diagnostics.Debug.WriteLine($"? {addedCount} mines historiques chargées automatiquement");
        }

        /// <summary>
        /// Vérifie si les mines historiques sont déjé chargées
        /// </summary>
        public bool AreHistoricalMinesLoaded()
        {
            var allFilons = _filonService.GetAllFilons();
            var minesNames = MinesVarDataService.GetAllMines().Select(m => m.Nom).ToHashSet();

            // Vérifier si au moins 50% des mines historiques sont présentes
            int matchCount = allFilons.Count(f => minesNames.Contains(f.Nom));
            return matchCount >= (minesNames.Count / 2);
        }
    }
}
