using Newtonsoft.Json;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service de gestion des données des filons
    /// </summary>
    public class FilonDataService
    {
        private readonly string _dataFilePath;
        private List<Filon> _filons;

        public FilonDataService()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WMine"
            );
            
            Directory.CreateDirectory(appDataPath);
            _dataFilePath = Path.Combine(appDataPath, "filons.json");
            _filons = LoadFilons();
        }

        public List<Filon> GetAllFilons() => _filons;

        public Filon? GetFilonById(Guid id) => _filons.FirstOrDefault(f => f.Id == id);

        public void AddFilon(Filon filon)
        {
            filon.DateCreation = DateTime.Now;
            filon.DateModification = DateTime.Now;
            _filons.Add(filon);
            SaveFilons();
        }

        public void UpdateFilon(Filon filon)
        {
            var existing = _filons.FirstOrDefault(f => f.Id == filon.Id);
            if (existing != null)
            {
                var index = _filons.IndexOf(existing);
                filon.DateModification = DateTime.Now;
                _filons[index] = filon;
                SaveFilons();
            }
        }

        public void DeleteFilon(Guid id)
        {
            var filon = _filons.FirstOrDefault(f => f.Id == id);
            if (filon != null)
            {
                _filons.Remove(filon);
                SaveFilons();
            }
        }

        public List<Filon> FilterByMainMineral(MineralType? mineralType)
        {
            if (!mineralType.HasValue)
                return _filons;

            return _filons.Where(f => f.MatierePrincipale == mineralType.Value).ToList();
        }

        public List<Filon> SearchFilons(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _filons;

            searchTerm = searchTerm.ToLowerInvariant();
            return _filons.Where(f => 
                f.Nom.ToLowerInvariant().Contains(searchTerm) ||
                f.Notes.ToLowerInvariant().Contains(searchTerm)
            ).ToList();
        }

        private List<Filon> LoadFilons()
        {
            if (!File.Exists(_dataFilePath))
                return new List<Filon>();

            try
            {
                string json = File.ReadAllText(_dataFilePath);
                return JsonConvert.DeserializeObject<List<Filon>>(json) ?? new List<Filon>();
            }
            catch
            {
                return new List<Filon>();
            }
        }

        private void SaveFilons()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_filons, Formatting.Indented);
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde: {ex.Message}", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string GetDataFilePath() => _dataFilePath;
    }
}
