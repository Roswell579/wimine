using wmine.Core.Interfaces;

namespace wmine.Utils
{
    /// <summary>
    /// Implémentation simple d'un systéme de logging vers fichier
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();
        private readonly bool _enabled;

        public FileLogger(string? logDirectory = null, bool enabled = true)
        {
            _enabled = enabled;

            if (!_enabled)
            {
                _logFilePath = string.Empty;
                return;
            }

            logDirectory ??= Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "wmine", "Logs");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            var logFileName = $"wmine_{DateTime.Now:yyyyMMdd}.log";
            _logFilePath = Path.Combine(logDirectory, logFileName);

            // Nettoyer les anciens logs (garder seulement 30 jours)
            CleanOldLogs(logDirectory, 30);
        }

        public void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogWarning(string message)
        {
            WriteLog("WARN", message);
        }

        public void LogError(string message, Exception? ex = null)
        {
            var fullMessage = ex != null
                ? $"{message}\nException: {ex.GetType().Name}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}"
                : message;

            WriteLog("ERROR", fullMessage);
        }

        public void LogDebug(string message)
        {
#if DEBUG
            WriteLog("DEBUG", message);
#endif
        }

        private void WriteLog(string level, string message)
        {
            if (!_enabled)
                return;

            try
            {
                lock (_lockObject)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logEntry = $"[{timestamp}] [{level}] {message}";

                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
            }
            catch
            {
                // En cas d'erreur de logging, ne pas planter l'application
            }
        }

        private void CleanOldLogs(string logDirectory, int daysToKeep)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var logFiles = Directory.GetFiles(logDirectory, "wmine_*.log");

                foreach (var logFile in logFiles)
                {
                    var fileInfo = new FileInfo(logFile);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(logFile);
                    }
                }
            }
            catch
            {
                // Ignorer les erreurs de nettoyage
            }
        }
    }

    /// <summary>
    /// Logger qui n'écrit rien (pour désactiver le logging)
    /// </summary>
    public class NullLogger : ILogger
    {
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message, Exception? ex = null) { }
        public void LogDebug(string message) { }
    }

    /// <summary>
    /// Logger composite qui écrit vers plusieurs destinations
    /// </summary>
    public class CompositeLogger : ILogger
    {
        private readonly List<ILogger> _loggers;

        public CompositeLogger(params ILogger[] loggers)
        {
            _loggers = new List<ILogger>(loggers);
        }

        public void AddLogger(ILogger logger)
        {
            _loggers.Add(logger);
        }

        public void LogInfo(string message)
        {
            foreach (var logger in _loggers)
            {
                logger.LogInfo(message);
            }
        }

        public void LogWarning(string message)
        {
            foreach (var logger in _loggers)
            {
                logger.LogWarning(message);
            }
        }

        public void LogError(string message, Exception? ex = null)
        {
            foreach (var logger in _loggers)
            {
                logger.LogError(message, ex);
            }
        }

        public void LogDebug(string message)
        {
            foreach (var logger in _loggers)
            {
                logger.LogDebug(message);
            }
        }
    }
}
