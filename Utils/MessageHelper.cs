using wmine.UI.Dialogs;

namespace wmine.Utils
{
    /// <summary>
    /// Helper pour afficher des messages modernisés
    /// Compatible avec l'ancien systéme ShowModernMessageBox
    /// </summary>
    public static class MessageHelper
    {
        /// <summary>
        /// Affiche un message moderne (remplacement de ShowModernMessageBox)
        /// </summary>
        public static void ShowModernMessageBox(string message, string title, MessageBoxIcon icon)
        {
            var modernIcon = ConvertIcon(icon);
            ModernMessageBox.Show(message, title, modernIcon, ModernMessageBoxButtons.OK);
        }

        /// <summary>
        /// Affiche un message moderne avec DialogResult
        /// </summary>
        public static DialogResult ShowModernMessageBox(string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons)
        {
            var modernIcon = ConvertIcon(icon);
            var modernButtons = ConvertButtons(buttons);
            return ModernMessageBox.Show(message, title, modernIcon, modernButtons);
        }

        private static ModernMessageBoxIcon ConvertIcon(MessageBoxIcon icon)
        {
            return icon switch
            {
                MessageBoxIcon.Information => ModernMessageBoxIcon.Information,
                MessageBoxIcon.Warning => ModernMessageBoxIcon.Warning,
                MessageBoxIcon.Error => ModernMessageBoxIcon.Error,
                MessageBoxIcon.Question => ModernMessageBoxIcon.Question,
                _ => ModernMessageBoxIcon.None
            };
        }

        private static ModernMessageBoxButtons ConvertButtons(MessageBoxButtons buttons)
        {
            return buttons switch
            {
                MessageBoxButtons.OK => ModernMessageBoxButtons.OK,
                MessageBoxButtons.OKCancel => ModernMessageBoxButtons.OKCancel,
                MessageBoxButtons.YesNo => ModernMessageBoxButtons.YesNo,
                MessageBoxButtons.YesNoCancel => ModernMessageBoxButtons.YesNoCancel,
                _ => ModernMessageBoxButtons.OK
            };
        }
    }
}
