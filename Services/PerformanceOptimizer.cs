using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace wmine.Services
{
    /// <summary>
    /// Service d'optimisation des performances de l'application
    /// Gére: débouncing, throttling, lazy loading, CancellationTokens, GPU acceleration
    /// </summary>
    public class PerformanceOptimizer : IDisposable
    {
        private readonly Dictionary<string, System.Timers.Timer> _debounceTimers = new();
        private readonly Dictionary<string, DateTime> _throttleLastExecution = new();
        private readonly Dictionary<string, CancellationTokenSource> _cancellationSources = new();
        
        #region débouncing (300ms)
        
        /// <summary>
        /// Exécute l'action aprés 300ms de délai sans nouveau déclenchement
        /// Idéal pour: recherche, filtres, validation en temps réel
        /// </summary>
        public void Debounce(string key, Action action, int delayMs = 300)
        {
            // Annuler le timer précédent si existant
            if (_debounceTimers.ContainsKey(key))
            {
                _debounceTimers[key].Stop();
                _debounceTimers[key].Dispose();
            }
            
            // Créer nouveau timer
            var timer = new System.Timers.Timer(delayMs);
            timer.Elapsed += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();
                _debounceTimers.Remove(key);
                
                // Exécuter sur le thread UI
                if (action.Target is Control control)
                {
                    if (control.InvokeRequired)
                        control.Invoke(action);
                    else
                        action();
                }
                else
                {
                    action();
                }
            };
            
            _debounceTimers[key] = timer;
            timer.Start();
        }
        
        #endregion
        
        #region Throttling
        
        /// <summary>
        /// Limite l'exécution é une fois par période
        /// Idéal pour: MouseMove, Scroll, Resize
        /// </summary>
        public bool Throttle(string key, int intervalMs = 100)
        {
            if (_throttleLastExecution.TryGetValue(key, out var lastTime))
            {
                var elapsed = (DateTime.Now - lastTime).TotalMilliseconds;
                if (elapsed < intervalMs)
                    return false; // Trop tét, skip
            }
            
            _throttleLastExecution[key] = DateTime.Now;
            return true; // OK, exécuter
        }
        
        #endregion
        
        #region CancellationToken Management
        
        /// <summary>
        /// Crée ou récupére un CancellationTokenSource pour une opération
        /// Annule automatiquement l'opération précédente
        /// </summary>
        public CancellationToken GetCancellationToken(string operationKey)
        {
            // Annuler l'opération précédente si existante
            if (_cancellationSources.TryGetValue(operationKey, out var existingCts))
            {
                existingCts.Cancel();
                existingCts.Dispose();
            }
            
            // Créer nouveau CTS
            var cts = new CancellationTokenSource();
            _cancellationSources[operationKey] = cts;
            return cts.Token;
        }
        
        /// <summary>
        /// Libére un CancellationTokenSource aprés utilisation
        /// </summary>
        public void ReleaseCancellationToken(string operationKey)
        {
            if (_cancellationSources.TryGetValue(operationKey, out var cts))
            {
                cts.Dispose();
                _cancellationSources.Remove(operationKey);
            }
        }
        
        #endregion
        
        #region GPU Acceleration
        
        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmEnableComposition(uint uCompositionAction);
        
        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern bool DwmIsCompositionEnabled();
        
        /// <summary>
        /// Active l'accélération GPU pour Windows (DWM)
        /// Améliore performances de rendu graphique
        /// </summary>
        public static bool EnableHardwareAcceleration()
        {
            try
            {
                if (Environment.OSVersion.Version.Major >= 6) // Windows Vista+
                {
                    if (!DwmIsCompositionEnabled())
                    {
                        DwmEnableComposition(1); // 1 = Enable
                    }
                    return true;
                }
            }
            catch
            {
                // échoue silencieusement si non supporté
            }
            return false;
        }
        
        #endregion
        
        #region Rendering Optimization
        
        /// <summary>
        /// Suspend le layout d'un contréle pour optimiser les modifications multiples
        /// Utilisation: using (optimizer.SuspendLayout(control)) { ... modifications ... }
        /// </summary>
        public IDisposable SuspendLayout(Control control)
        {
            return new LayoutSuspender(control);
        }
        
        private class LayoutSuspender : IDisposable
        {
            private readonly Control _control;
            private bool _wasVisible;
            
            public LayoutSuspender(Control control)
            {
                _control = control;
                _wasVisible = _control.Visible;
                
                if (_wasVisible)
                    _control.Visible = false;
                
                _control.SuspendLayout();
            }
            
            public void Dispose()
            {
                _control.ResumeLayout(true);
                
                if (_wasVisible)
                    _control.Visible = true;
                
                _control.Refresh();
            }
        }
        
        #endregion
        
        #region Lazy Loading Helper
        
        /// <summary>
        /// Charge une image de maniére asynchrone avec gestion d'erreur
        /// </summary>
        public static async Task<Image?> LoadImageAsync(string path, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;
            
            try
            {
                return await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                    return Image.FromStream(fs);
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        #endregion
        
        #region Memory Management
        
        /// <summary>
        /// Force la collecte des objets inutilisés (é utiliser avec parcimonie)
        /// </summary>
        public static void OptimizeMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        
        /// <summary>
        /// Obtient les statistiques mémoire actuelles
        /// </summary>
        public static (long TotalMemoryMB, long GCMemoryMB) GetMemoryStats()
        {
            var process = Process.GetCurrentProcess();
            var totalMemory = process.WorkingSet64 / (1024 * 1024);
            var gcMemory = GC.GetTotalMemory(false) / (1024 * 1024);
            return (totalMemory, gcMemory);
        }
        
        #endregion
        
        #region Cleanup
        
        /// <summary>
        /// Nettoie toutes les ressources (timers, CTS, etc.)
        /// </summary>
        public void Dispose()
        {
            // Dispose des timers
            foreach (var timer in _debounceTimers.Values)
            {
                timer.Stop();
                timer.Dispose();
            }
            _debounceTimers.Clear();
            
            // Cancel et dispose des CTS
            foreach (var cts in _cancellationSources.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _cancellationSources.Clear();
            
            _throttleLastExecution.Clear();
        }
        
        #endregion
    }
}
