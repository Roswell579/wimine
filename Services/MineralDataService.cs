using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Informations détaillées sur un minéral
    /// </summary>
    public class MineralInfo
    {
        public MineralType Type { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string FormuleChimique { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> LocalitesVar { get; set; } = new();
        public string Utilisation { get; set; } = string.Empty;
        public string ProprietesPhysiques { get; set; } = string.Empty;
        public string DureteMohs { get; set; } = string.Empty;
        public string Densite { get; set; } = string.Empty;
        public string SystemeCristallin { get; set; } = string.Empty;
        public List<string> SourcesWeb { get; set; } = new();
        public DateTime DerniereMiseAJour { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Service fournissant des données enrichies sur les minéraux du Var
    /// Cette classe délègue la gestion des données à un repository (MineralRepository)
    /// </summary>
    public static class MineralDataService
    {
        private static IMineralRepository? _repo;

        private static IMineralRepository Repo
        {
            get
            {
                if (_repo != null) return _repo;
                _repo = Services.AppServiceProvider.GetService<IMineralRepository>() ?? new MineralRepository();
                return _repo;
            }
        }

        public static MineralInfo? GetMineralInfo(MineralType type)
        {
            return Repo.GetMineralInfo(type);
        }

        public static Dictionary<MineralType, MineralInfo> GetAllMineralInfo()
        {
            return Repo.GetAllMineralInfo();
        }

        public static List<MineralInfo> SearchByLocality(string locality)
        {
            return Repo.SearchByLocality(locality);
        }

        public static List<MineralInfo> GetMineralsByRegion(string region)
        {
            return Repo.GetMineralsByRegion(region);
        }

        public static string GetStatisticsSummary()
        {
            return Repo.GetStatisticsSummary();
        }
    }
}
