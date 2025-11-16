using System.Text.Json;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service de gestion des documents et notes techniques
    /// </summary>
    public class TechniquesDataService
    {
        private readonly string _dataFilePath;
        private List<TechniqueDocument> _documents;

        public TechniquesDataService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WMine");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _dataFilePath = Path.Combine(appDataPath, "techniques.json");
            _documents = LoadDocuments();
        }

        private List<TechniqueDocument> LoadDocuments()
        {
            if (!File.Exists(_dataFilePath))
            {
                return new List<TechniqueDocument>();
            }

            try
            {
                var json = File.ReadAllText(_dataFilePath);
                return JsonSerializer.Deserialize<List<TechniqueDocument>>(json) ?? new List<TechniqueDocument>();
            }
            catch
            {
                return new List<TechniqueDocument>();
            }
        }

        private void SaveDocuments()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_documents, options);
            File.WriteAllText(_dataFilePath, json);
        }

        public List<TechniqueDocument> GetAllDocuments()
        {
            return _documents.OrderByDescending(d => d.DateCreation).ToList();
        }

        public List<TechniqueDocument> GetDocumentsByCategorie(string categorie)
        {
            return _documents
                .Where(d => d.CategorieTechnique == categorie)
                .OrderByDescending(d => d.DateCreation)
                .ToList();
        }

        public TechniqueDocument? GetDocumentById(int id)
        {
            return _documents.FirstOrDefault(d => d.Id == id);
        }

        public void AddDocument(TechniqueDocument document)
        {
            document.Id = _documents.Any() ? _documents.Max(d => d.Id) + 1 : 1;
            document.DateCreation = DateTime.Now;
            _documents.Add(document);
            SaveDocuments();
        }

        public void UpdateDocument(TechniqueDocument document)
        {
            var existing = _documents.FirstOrDefault(d => d.Id == document.Id);
            if (existing != null)
            {
                _documents.Remove(existing);
                document.DateModification = DateTime.Now;
                _documents.Add(document);
                SaveDocuments();
            }
        }

        public void DeleteDocument(int id)
        {
            var document = _documents.FirstOrDefault(d => d.Id == id);
            if (document != null)
            {
                _documents.Remove(document);
                SaveDocuments();
            }
        }

        public List<string> GetAllCategories()
        {
            return _documents
                .Where(d => !string.IsNullOrWhiteSpace(d.CategorieTechnique))
                .Select(d => d.CategorieTechnique!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }
    }
}
