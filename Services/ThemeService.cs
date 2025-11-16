using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service de gestion des thémes d'apparence
    /// </summary>
    public class ThemeService
    {
        private AppTheme _currentTheme;
        private readonly string _settingsPath;
        
        public AppTheme CurrentTheme => _currentTheme;
        
        public ThemeService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "wmine");
            
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);
            
            _settingsPath = Path.Combine(appDataPath, "settings.json");
            
            // Charger le théme sauvegardé ou utiliser Dark par défaut
            var savedType = LoadThemePreference();
            _currentTheme = AppTheme.GetTheme(savedType);
        }
        
        /// <summary>
        /// Applique un théme é un formulaire et tous ses contréles
        /// </summary>
        public void ApplyTheme(Form form, AppTheme theme)
        {
            _currentTheme = theme;
            
            // Appliquer au formulaire
            form.BackColor = theme.BackgroundPrimary;
            form.ForeColor = theme.TextPrimary;
            
            // Appliquer récursivement é tous les contréles
            ApplyThemeToControls(form.Controls, theme);
            
            // Sauvegarder la préférence
            SaveThemePreference(theme.Type);
            
            // Rafraéchir l'affichage
            form.Refresh();
        }
        
        private void ApplyThemeToControls(Control.ControlCollection controls, AppTheme theme)
        {
            foreach (Control control in controls)
            {
                // Panels et conteneurs
                if (control is Panel panel)
                {
                    if (panel.BackColor == Color.FromArgb(25, 25, 35) || 
                        panel.BackColor == Color.FromArgb(245, 245, 245))
                        panel.BackColor = theme.BackgroundPrimary;
                    else if (panel.BackColor == Color.FromArgb(30, 35, 45) || 
                             panel.BackColor == Color.White)
                        panel.BackColor = theme.BackgroundSecondary;
                    else if (panel.BackColor == Color.FromArgb(40, 45, 55) ||
                             panel.BackColor == Color.FromArgb(235, 235, 235))
                        panel.BackColor = theme.BackgroundTertiary;
                    
                    panel.ForeColor = theme.TextPrimary;
                }
                
                // Labels
                else if (control is Label label)
                {
                    if (label.ForeColor == Color.White || 
                        label.ForeColor == Color.FromArgb(33, 33, 33))
                        label.ForeColor = theme.TextPrimary;
                    else if (label.ForeColor == Color.FromArgb(180, 180, 180) ||
                             label.ForeColor == Color.FromArgb(100, 100, 100))
                        label.ForeColor = theme.TextSecondary;
                    
                    // Couleur d'accent pour les titres
                    if (label.Font.Size >= 18 && label.Font.Bold)
                    {
                        if (label.ForeColor == Color.FromArgb(0, 150, 136))
                            label.ForeColor = theme.AccentColor;
                    }
                    
                    // Backgrounds
                    if (label.BackColor == Color.FromArgb(40, 45, 55))
                        label.BackColor = theme.BackgroundTertiary;
                }
                
                // GroupBox
                else if (control is GroupBox groupBox)
                {
                    groupBox.BackColor = theme.BackgroundSecondary;
                    groupBox.ForeColor = theme.AccentColor;
                }
                
                // TextBox
                else if (control is TextBox textBox)
                {
                    textBox.BackColor = theme.BackgroundTertiary;
                    textBox.ForeColor = theme.TextPrimary;
                }
                
                // ComboBox
                else if (control is ComboBox comboBox)
                {
                    comboBox.BackColor = theme.BackgroundTertiary;
                    comboBox.ForeColor = theme.TextPrimary;
                }
                
                // ListBox
                else if (control is ListBox listBox)
                {
                    listBox.BackColor = theme.BackgroundTertiary;
                    listBox.ForeColor = theme.TextPrimary;
                }
                
                // ListView
                else if (control is ListView listView)
                {
                    listView.BackColor = theme.BackgroundSecondary;
                    listView.ForeColor = theme.TextPrimary;
                }
                
                // DataGridView
                else if (control is DataGridView dataGrid)
                {
                    dataGrid.BackgroundColor = theme.BackgroundSecondary;
                    dataGrid.DefaultCellStyle.BackColor = theme.BackgroundSecondary;
                    dataGrid.DefaultCellStyle.ForeColor = theme.TextPrimary;
                    dataGrid.AlternatingRowsDefaultCellStyle.BackColor = theme.BackgroundTertiary;
                }
                
                // TabControl
                else if (control is TabControl tabControl)
                {
                    tabControl.BackColor = theme.BackgroundPrimary;
                    tabControl.ForeColor = theme.TextPrimary;
                    
                    foreach (TabPage tabPage in tabControl.TabPages)
                    {
                        tabPage.BackColor = theme.BackgroundPrimary;
                        tabPage.ForeColor = theme.TextPrimary;
                    }
                }
                
                // LinkLabel
                else if (control is LinkLabel linkLabel)
                {
                    linkLabel.LinkColor = theme.AccentColor;
                    linkLabel.VisitedLinkColor = theme.AccentColor;
                }
                
                // Appliquer récursivement aux enfants
                if (control.HasChildren)
                {
                    ApplyThemeToControls(control.Controls, theme);
                }
            }
        }
        
        /// <summary>
        /// Sauvegarde la préférence de théme
        /// </summary>
        public void SaveThemePreference(ThemeType type)
        {
            try
            {
                var settings = new AppSettings
                {
                    ThemeType = type,
                    LastModified = DateTime.Now
                };
                
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur sauvegarde théme: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Charge la préférence de théme sauvegardée
        /// </summary>
        public ThemeType LoadThemePreference()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    
                    if (settings != null)
                        return settings.ThemeType;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur chargement théme: {ex.Message}");
            }
            
            return ThemeType.Dark; // Théme par défaut
        }
        
        /// <summary>
        /// Obtient tous les thémes disponibles
        /// </summary>
        public AppTheme[] GetAvailableThemes()
        {
            return new[]
            {
                AppTheme.GetTheme(ThemeType.Dark),
                AppTheme.GetTheme(ThemeType.Light),
                AppTheme.GetTheme(ThemeType.Blue),
                AppTheme.GetTheme(ThemeType.Green),
                AppTheme.GetTheme(ThemeType.Mineral)
            };
        }
    }
    
    /// <summary>
    /// Paramétres de l'application
    /// </summary>
    internal class AppSettings
    {
        public ThemeType ThemeType { get; set; }
        public DateTime LastModified { get; set; }
        public bool EnableAnimations { get; set; } = true;
        public int WindowOpacity { get; set; } = 220;
        public bool EnablePinProtection { get; set; } = false;
        public bool ConfirmDelete { get; set; } = true;
        public bool AutoSave { get; set; } = true;
    }
}
