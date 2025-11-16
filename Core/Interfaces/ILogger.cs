namespace wmine.Core.Interfaces
{
    /// <summary>
    /// Interface pour le systéme de logging
    /// </summary>
    public interface ILogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception? ex = null);
        void LogDebug(string message);
    }
}
