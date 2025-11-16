using System.Diagnostics;
using System.Text.Json;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service de synchronisation cloud via GitHub
    /// Permet de partager les données de filons entre plusieurs utilisateurs
    /// </summary>
    public class CloudSyncService
    {
        private readonly string _localDataPath;
        private readonly string _repoUrl;
        private readonly string _repoPath;
        private bool _isInitialized = false;

        public bool IsEnabled { get; set; }
        public string LastSyncTime { get; private set; } = "Jamais";
        public int ConflictsCount { get; private set; } = 0;

        public CloudSyncService(string repoUrl = "https://github.com/Roswell579/wmine-data.git")
        {
            _repoUrl = repoUrl;
            _localDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "wmine");
            _repoPath = Path.Combine(_localDataPath, "cloud-repo");
            IsEnabled = File.Exists(Path.Combine(_localDataPath, ".cloud_enabled"));
        }

        /// <summary>
        /// Active la synchronisation cloud
        /// </summary>
        public async Task<(bool success, string message)> EnableAsync()
        {
            try
            {
                // Vérifier que Git est installé
                if (!await IsGitInstalledAsync())
                {
                    return (false, "Git n'est pas installé sur ce systéme.\n\n" +
                                 "Téléchargez-le sur : https://git-scm.com/download");
                }

                // Cloner le repository si nécessaire
                if (!Directory.Exists(_repoPath))
                {
                    var cloneResult = await RunGitCommandAsync($"clone {_repoUrl} \"{_repoPath}\"", _localDataPath);
                    if (!cloneResult.success)
                    {
                        return (false, $"Impossible de cloner le repository:\n{cloneResult.output}");
                    }
                }

                // Marquer comme activé
                File.WriteAllText(Path.Combine(_localDataPath, ".cloud_enabled"), DateTime.Now.ToString());
                IsEnabled = true;
                _isInitialized = true;

                return (true, "Synchronisation cloud activée !\n\n" +
                            $"Repository : {_repoUrl}\n" +
                            $"Dossier local : {_repoPath}");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de l'activation:\n{ex.Message}");
            }
        }

        /// <summary>
        /// désactive la synchronisation cloud
        /// </summary>
        public (bool success, string message) Disable()
        {
            try
            {
                var markerFile = Path.Combine(_localDataPath, ".cloud_enabled");
                if (File.Exists(markerFile))
                {
                    File.Delete(markerFile);
                }

                IsEnabled = false;
                return (true, "Synchronisation cloud désactivée.");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur:\n{ex.Message}");
            }
        }

        /// <summary>
        /// Pull (récupération) des données depuis le cloud
        /// </summary>
        public async Task<(bool success, string message, int newFilons)> PullAsync()
        {
            if (!IsEnabled)
                return (false, "Synchronisation cloud désactivée.", 0);

            try
            {
                // Pull depuis GitHub
                var pullResult = await RunGitCommandAsync("pull origin main", _repoPath);
                if (!pullResult.success && !pullResult.output.Contains("Already up to date"))
                {
                    return (false, $"Erreur lors du pull:\n{pullResult.output}", 0);
                }

                // Charger les filons du cloud
                var cloudFilonsPath = Path.Combine(_repoPath, "filons.json");
                if (!File.Exists(cloudFilonsPath))
                {
                    return (true, "Aucune donnée dans le cloud.", 0);
                }

                var cloudJson = await File.ReadAllTextAsync(cloudFilonsPath);
                var cloudFilons = JsonSerializer.Deserialize<List<Filon>>(cloudJson) ?? new List<Filon>();

                // Charger les filons locaux
                var localFilonsPath = Path.Combine(_localDataPath, "filons.json");
                var localFilons = new List<Filon>();
                if (File.Exists(localFilonsPath))
                {
                    var localJson = await File.ReadAllTextAsync(localFilonsPath);
                    localFilons = JsonSerializer.Deserialize<List<Filon>>(localJson) ?? new List<Filon>();
                }

                // Merger les filons (cloud wins pour les conflits)
                var merged = MergeFilons(localFilons, cloudFilons);
                var newCount = merged.Count - localFilons.Count;

                // Sauvegarder localement
                var mergedJson = JsonSerializer.Serialize(merged, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(localFilonsPath, mergedJson);

                LastSyncTime = DateTime.Now.ToString("HH:mm");

                return (true, $"Pull réussi !\n{newCount} nouveau(x) filon(s).", newCount);
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors du pull:\n{ex.Message}", 0);
            }
        }

        /// <summary>
        /// Push (envoi) des données vers le cloud
        /// </summary>
        public async Task<(bool success, string message)> PushAsync()
        {
            if (!IsEnabled)
                return (false, "Synchronisation cloud désactivée.");

            try
            {
                // Copier les filons locaux vers le repo
                var localFilonsPath = Path.Combine(_localDataPath, "filons.json");
                var cloudFilonsPath = Path.Combine(_repoPath, "filons.json");

                if (File.Exists(localFilonsPath))
                {
                    File.Copy(localFilonsPath, cloudFilonsPath, true);
                }

                // Commit
                await RunGitCommandAsync("add .", _repoPath);
                var commitMsg = $"Update from {Environment.MachineName} at {DateTime.Now:yyyy-MM-dd HH:mm}";
                await RunGitCommandAsync($"commit -m \"{commitMsg}\"", _repoPath);

                // Push
                var pushResult = await RunGitCommandAsync("push origin main", _repoPath);
                if (!pushResult.success && !pushResult.output.Contains("up-to-date"))
                {
                    return (false, $"Erreur lors du push:\n{pushResult.output}");
                }

                LastSyncTime = DateTime.Now.ToString("HH:mm");

                return (true, "Push réussi !\nVos données sont maintenant sur le cloud.");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors du push:\n{ex.Message}");
            }
        }

        /// <summary>
        /// Synchronisation compléte (Pull puis Push)
        /// </summary>
        public async Task<(bool success, string message)> SyncAsync()
        {
            var pullResult = await PullAsync();
            if (!pullResult.success)
                return (false, pullResult.message);

            var pushResult = await PushAsync();
            return pushResult;
        }

        /// <summary>
        /// Merge intelligent des filons locaux et cloud
        /// </summary>
        private List<Filon> MergeFilons(List<Filon> local, List<Filon> cloud)
        {
            var merged = new Dictionary<Guid, Filon>();

            // Ajouter les filons locaux
            foreach (var filon in local)
            {
                merged[filon.Id] = filon;
            }

            // Merger avec les filons cloud (cloud wins si conflit)
            foreach (var cloudFilon in cloud)
            {
                if (merged.TryGetValue(cloudFilon.Id, out var localFilon))
                {
                    // Conflit : garder le plus récent
                    if (cloudFilon.DateModification > localFilon.DateModification)
                    {
                        merged[cloudFilon.Id] = cloudFilon;
                        ConflictsCount++;
                    }
                }
                else
                {
                    // Nouveau filon du cloud
                    merged[cloudFilon.Id] = cloudFilon;
                }
            }

            return merged.Values.OrderBy(f => f.Nom).ToList();
        }

        /// <summary>
        /// Vérifie si Git est installé
        /// </summary>
        private async Task<bool> IsGitInstalledAsync()
        {
            try
            {
                var result = await RunGitCommandAsync("--version", Directory.GetCurrentDirectory());
                return result.success;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Exécute une commande Git
        /// </summary>
        private async Task<(bool success, string output)> RunGitCommandAsync(string arguments, string workingDirectory)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process == null)
                    return (false, "Impossible de démarrer git");

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                var result = string.IsNullOrWhiteSpace(error) ? output : error;
                return (process.ExitCode == 0, result);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Obtient le status de la synchronisation
        /// </summary>
        public string GetStatus()
        {
            if (!IsEnabled)
                return "Synchronisation désactivée";

            return $"Synchronisation activée\nDerniére sync : {LastSyncTime}";
        }
    }
}
