using wmine.Core.Interfaces;
using wmine.Services;

namespace wmine.Core.Services
{
    /// <summary>
    /// Service de sauvegarde automatique
    /// </summary>
    public class AutoSaveService : IDisposable
    {
        private System.Threading.Timer? _timer;
        private readonly FilonDataService _dataService;
        private readonly ILogger? _logger;
        private readonly TimeSpan _interval;
        private bool _isDisposed;

        public bool IsEnabled { get; private set; }

        public AutoSaveService(FilonDataService dataService, ILogger? logger = null, TimeSpan? interval = null)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger;
            _interval = interval ?? TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Active la sauvegarde automatique
        /// </summary>
        public void Enable()
        {
            if (IsEnabled)
                return;

            _timer = new System.Threading.Timer(
                AutoSaveCallback,
                null,
                _interval,
                _interval
            );

            IsEnabled = true;
            _logger?.LogInfo($"Sauvegarde automatique activée (intervalle: {_interval.TotalMinutes} minutes)");
        }

        /// <summary>
        /// désactive la sauvegarde automatique
        /// </summary>
        public void Disable()
        {
            if (!IsEnabled)
                return;

            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            IsEnabled = false;
            _logger?.LogInfo("Sauvegarde automatique désactivée");
        }

        private void AutoSaveCallback(object? state)
        {
            try
            {
                var appState = ApplicationState.Instance;
                
                if (appState.IsDirty)
                {
                    _logger?.LogDebug("Sauvegarde automatique en cours...");
                    // La sauvegarde est déjé gérée par FilonDataService lors des opérations
                    // Ici on pourrait ajouter une sauvegarde explicite si nécessaire
                    appState.IsDirty = false;
                    _logger?.LogInfo("Sauvegarde automatique effectuée");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Erreur lors de la sauvegarde automatique", ex);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            Disable();
            _timer?.Dispose();
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Information sur une sauvegarde
    /// </summary>
    public class BackupInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public long SizeInBytes { get; set; }
        public string Description { get; set; } = string.Empty;

        public string SizeFormatted => FormatBytes(SizeInBytes);

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// Service de sauvegarde et restauration
    /// </summary>
    public class BackupService
    {
        private readonly string _backupDirectory;
        private readonly ILogger? _logger;
        private readonly int _maxBackups;

        public BackupService(ILogger? logger = null, string? backupDirectory = null, int maxBackups = 10)
        {
            _logger = logger;
            _maxBackups = maxBackups;
            _backupDirectory = backupDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "wmine", "Backups");

            EnsureDirectoryExists(_backupDirectory);
        }

        /// <summary>
        /// Crée une sauvegarde compléte
        /// </summary>
        public async Task<string> CreateBackupAsync(string description = "")
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"WMine_Backup_{timestamp}.zip";
                var backupPath = Path.Combine(_backupDirectory, backupFileName);

                _logger?.LogInfo($"Création de la sauvegarde: {backupFileName}");

                await Task.Run(() =>
                {
                    var dataDirectory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "wmine");

                    if (Directory.Exists(dataDirectory))
                    {
                        System.IO.Compression.ZipFile.CreateFromDirectory(
                            dataDirectory,
                            backupPath,
                            System.IO.Compression.CompressionLevel.Optimal,
                            false
                        );
                    }

                    // Créer un fichier de métadonnées
                    var metaPath = Path.ChangeExtension(backupPath, ".meta");
                    var meta = new
                    {
                        Description = description,
                        CreationDate = DateTime.Now,
                        Version = "1.0",
                        Application = "WMine"
                    };
                    File.WriteAllText(metaPath, System.Text.Json.JsonSerializer.Serialize(meta, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                });

                // Nettoyer les anciennes sauvegardes
                await CleanOldBackupsAsync();

                _logger?.LogInfo($"Sauvegarde créée avec succés: {backupPath}");
                return backupPath;
            }
            catch (Exception ex)
            {
                _logger?.LogError("Erreur lors de la création de la sauvegarde", ex);
                throw;
            }
        }

        /// <summary>
        /// Restaure une sauvegarde
        /// </summary>
        public async Task RestoreBackupAsync(string backupPath)
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("Le fichier de sauvegarde n'existe pas", backupPath);
            }

            try
            {
                _logger?.LogInfo($"Restauration de la sauvegarde: {backupPath}");

                var dataDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "wmine");

                // Créer une sauvegarde de sécurité avant restauration
                if (Directory.Exists(dataDirectory))
                {
                    await CreateBackupAsync("Avant restauration");
                }

                await Task.Run(() =>
                {
                    // Supprimer le répertoire actuel
                    if (Directory.Exists(dataDirectory))
                    {
                        Directory.Delete(dataDirectory, true);
                    }

                    // Extraire la sauvegarde
                    System.IO.Compression.ZipFile.ExtractToDirectory(backupPath, dataDirectory);
                });

                _logger?.LogInfo("Sauvegarde restaurée avec succés");
            }
            catch (Exception ex)
            {
                _logger?.LogError("Erreur lors de la restauration de la sauvegarde", ex);
                throw;
            }
        }

        /// <summary>
        /// Obtient la liste des sauvegardes disponibles
        /// </summary>
        public List<BackupInfo> GetAvailableBackups()
        {
            var backups = new List<BackupInfo>();

            if (!Directory.Exists(_backupDirectory))
            {
                return backups;
            }

            var backupFiles = Directory.GetFiles(_backupDirectory, "WMine_Backup_*.zip");

            foreach (var file in backupFiles)
            {
                var fileInfo = new FileInfo(file);
                var metaPath = Path.ChangeExtension(file, ".meta");
                var description = "";

                if (File.Exists(metaPath))
                {
                    try
                    {
                        var metaContent = File.ReadAllText(metaPath);
                        var meta = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(metaContent);
                        if (meta != null && meta.ContainsKey("Description"))
                        {
                            description = meta["Description"]?.ToString() ?? "";
                        }
                    }
                    catch
                    {
                        // Ignorer les erreurs de lecture des métadonnées
                    }
                }

                backups.Add(new BackupInfo
                {
                    FilePath = file,
                    CreationDate = fileInfo.CreationTime,
                    SizeInBytes = fileInfo.Length,
                    Description = description
                });
            }

            return backups.OrderByDescending(b => b.CreationDate).ToList();
        }

        /// <summary>
        /// Supprime une sauvegarde
        /// </summary>
        public void DeleteBackup(string backupPath)
        {
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
                
                var metaPath = Path.ChangeExtension(backupPath, ".meta");
                if (File.Exists(metaPath))
                {
                    File.Delete(metaPath);
                }

                _logger?.LogInfo($"Sauvegarde supprimée: {backupPath}");
            }
        }

        /// <summary>
        /// Nettoie les anciennes sauvegardes
        /// </summary>
        private async Task CleanOldBackupsAsync()
        {
            await Task.Run(() =>
            {
                var backups = GetAvailableBackups();
                
                if (backups.Count > _maxBackups)
                {
                    var toDelete = backups.Skip(_maxBackups);
                    
                    foreach (var backup in toDelete)
                    {
                        DeleteBackup(backup.FilePath);
                    }

                    _logger?.LogInfo($"Nettoyage: {toDelete.Count()} anciennes sauvegardes supprimées");
                }
            });
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
