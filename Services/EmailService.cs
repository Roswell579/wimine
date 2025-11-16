using System.Diagnostics;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service d'envoi de fiches par email
    /// </summary>
    public class EmailService
    {
        private readonly PdfExportService _pdfExportService;

        public EmailService(PdfExportService pdfExportService)
        {
            _pdfExportService = pdfExportService;
        }

        /// <summary>
        /// Partage un filon par email en générant un PDF temporaire
        /// </summary>
        public void ShareFilonByEmail(Filon filon, string recipientEmail = "")
        {
            try
            {
                // Créer un PDF temporaire
                string tempPath = Path.Combine(Path.GetTempPath(), $"Filon_{filon.Nom}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
                _pdfExportService.ExportFilonToPdf(filon, tempPath);

                // Créer le mailto avec piéce jointe
                string subject = Uri.EscapeDataString($"Fiche Filon - {filon.Nom}");
                string body = Uri.EscapeDataString(
                    $"Bonjour,\n\n" +
                    $"Veuillez trouver ci-joint la fiche du filon '{filon.Nom}'.\n\n" +
                    $"Matiére principale: {MineralColors.GetDisplayName(filon.MatierePrincipale)}\n" +
                    $"Coordonnées: {(filon.Latitude.HasValue ? $"{filon.Latitude:F6}éN, {filon.Longitude:F6}éE" : "Non renseignées")}\n\n" +
                    $"Cordialement,\n" +
                    $"WMine Localisateur"
                );

                // Ouvrir le client email par défaut
                // Note: Windows n'autorise pas l'ajout automatique de piéces jointes via mailto pour des raisons de sécurité
                // On ouvre juste l'email avec le sujet et le corps, l'utilisateur devra ajouter la piéce jointe manuellement
                string mailtoUrl = $"mailto:{recipientEmail}?subject={subject}&body={body}";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = mailtoUrl,
                    UseShellExecute = true
                };

                Process.Start(psi);

                // Informer l'utilisateur oé se trouve le PDF
                MessageBox.Show(
                    $"Le client email a été ouvert.\n\n" +
                    $"Le PDF a été généré é l'emplacement suivant:\n{tempPath}\n\n" +
                    $"Veuillez l'ajouter manuellement comme piéce jointe é votre email.",
                    "Partage par email",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // Ouvrir le dossier contenant le PDF
                Process.Start("explorer.exe", $"/select,\"{tempPath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de l'envoi de l'email: {ex.Message}",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Partage plusieurs filons par email
        /// </summary>
        public void ShareMultipleFilonsByEmail(List<Filon> filons, string recipientEmail = "")
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), $"Filons_{DateTime.Now:yyyyMMddHHmmss}.pdf");
                _pdfExportService.ExportMultipleFilonsToPdf(filons, tempPath);

                string subject = Uri.EscapeDataString($"Liste de {filons.Count} filon(s)");
                string body = Uri.EscapeDataString(
                    $"Bonjour,\n\n" +
                    $"Veuillez trouver ci-joint une liste de {filons.Count} filon(s).\n\n" +
                    $"Cordialement,\n" +
                    $"WMine Localisateur"
                );

                string mailtoUrl = $"mailto:{recipientEmail}?subject={subject}&body={body}";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = mailtoUrl,
                    UseShellExecute = true
                };

                Process.Start(psi);

                MessageBox.Show(
                    $"Le client email a été ouvert.\n\n" +
                    $"Le PDF a été généré é l'emplacement suivant:\n{tempPath}\n\n" +
                    $"Veuillez l'ajouter manuellement comme piéce jointe é votre email.",
                    "Partage par email",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                Process.Start("explorer.exe", $"/select,\"{tempPath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de l'envoi de l'email: {ex.Message}",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
