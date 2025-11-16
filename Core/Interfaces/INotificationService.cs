namespace wmine.Core.Interfaces
{
    /// <summary>
    /// Interface pour les services de notification
    /// </summary>
    public interface INotificationService
    {
        void ShowSuccess(string message, int durationMs = 3000);
        void ShowError(string message, int durationMs = 5000);
        void ShowInfo(string message, int durationMs = 3000);
        void ShowWarning(string message, int durationMs = 4000);
    }
}
