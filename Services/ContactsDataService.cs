using System.Text.Json;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service de gestion des contacts professionnels
    /// </summary>
    public class ContactsDataService
    {
        private readonly string _dataFilePath;
        private List<Contact> _contacts;

        public ContactsDataService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WMine");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _dataFilePath = Path.Combine(appDataPath, "contacts.json");
            _contacts = LoadContacts();

            // Créer 6 contacts pré-remplis si aucun n'existe
            if (_contacts.Count == 0)
            {
                InitializeDefaultContacts();
            }
        }

        private void InitializeDefaultContacts()
        {
            _contacts = new List<Contact>
            {
                new Contact
                {
                    Id = 1,
                    Nom = "BRGM",
                    Entreprise = "Bureau de Recherches Géologiques et Miniéres",
                    Fonction = "Service Géologique National",
                    Telephone = "02 38 64 34 34",
                    Email = "contact@brgm.fr",
                    Adresse = "3 avenue Claude-Guillemin, 45060 Orléans",
                    Specialite = "Géologie, Ressources minérales, Risques géologiques",
                    Notes = "Contact principal pour informations géologiques en France"
                },
                new Contact
                {
                    Id = 2,
                    Nom = "DREAL PACA",
                    Entreprise = "Direction Régionale de l'Environnement",
                    Fonction = "Autorité environnementale régionale",
                    Telephone = "04 91 28 40 40",
                    Email = "dreal-paca@developpement-durable.gouv.fr",
                    Adresse = "Le Tholonet, 13100 Aix-en-Provence",
                    Specialite = "Réglementation mines et carriéres, Environnement",
                    Notes = "Autorisation et suivi des activités extractives"
                },
                new Contact
                {
                    Id = 3,
                    Nom = "Mindat.org",
                    Entreprise = "Base de données minéralogique mondiale",
                    Fonction = "Ressource en ligne",
                    Email = "info@mindat.org",
                    Specialite = "Identification minéraux, Localités miniéres",
                    Notes = "Référence mondiale pour localisation des gisements"
                },
                new Contact
                {
                    Id = 4,
                    Nom = "Club de Minéralogie du Var",
                    Entreprise = "Association locale",
                    Fonction = "Groupe d'amateurs et experts",
                    Specialite = "Minéralogie du Var, Sorties terrain",
                    Notes = "Contact à compléter - Association locale de passionnés"
                },
                new Contact
                {
                    Id = 5,
                    Nom = "Géologue consultant",
                    Fonction = "Expert indépendant",
                    Specialite = "Prospection, Analyse de terrain",
                    Notes = "À compléter avec vos contacts personnels"
                },
                new Contact
                {
                    Id = 6,
                    Nom = "Contact personnel",
                    Notes = "Emplacement libre pour vos propres contacts"
                }
            };

            SaveContacts();
        }

        private List<Contact> LoadContacts()
        {
            if (!File.Exists(_dataFilePath))
            {
                return new List<Contact>();
            }

            try
            {
                var json = File.ReadAllText(_dataFilePath);
                return JsonSerializer.Deserialize<List<Contact>>(json) ?? new List<Contact>();
            }
            catch
            {
                return new List<Contact>();
            }
        }

        private void SaveContacts()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_contacts, options);
            File.WriteAllText(_dataFilePath, json);
        }

        public List<Contact> GetAllContacts()
        {
            return _contacts.OrderBy(c => c.Id).ToList();
        }

        public Contact? GetContactById(int id)
        {
            return _contacts.FirstOrDefault(c => c.Id == id);
        }

        public void AddContact(Contact contact)
        {
            contact.Id = _contacts.Any() ? _contacts.Max(c => c.Id) + 1 : 1;
            contact.DateCreation = DateTime.Now;
            _contacts.Add(contact);
            SaveContacts();
        }

        public void UpdateContact(Contact contact)
        {
            var existing = _contacts.FirstOrDefault(c => c.Id == contact.Id);
            if (existing != null)
            {
                _contacts.Remove(existing);
                contact.DateModification = DateTime.Now;
                _contacts.Add(contact);
                SaveContacts();
            }
        }

        public void DeleteContact(int id)
        {
            var contact = _contacts.FirstOrDefault(c => c.Id == id);
            if (contact != null)
            {
                _contacts.Remove(contact);
                SaveContacts();
            }
        }
    }
}
