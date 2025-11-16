# ?? PLAN DE REFONTE WMINE - EN COURS

## ? éTAPE 1 : TERMINéE - Onglet Import OCR créé

### Ce qui a été fait :
- ? Création du fichier `Forms/ImportPanel.cs` avec interface compléte d'import
- ? Ajout de l'onglet "?? Import OCR" dans Form1.Designer.cs
- ? Onglet "Minéraux du Var" vidé (carte supprimée)
- ? Compilation réussie (0 erreurs)

### Structure actuelle des onglets :
```
1. ??? Les minéraux du Var  ? VIDE (message d'attente)
2. ?? Import OCR           ? NOUVEAU !
3. ?? Techniques
4. ?? Contacts
5. ?? Paramétres
```

---

## ?? éTAPES RESTANTES (é IMPLéMENTER)

### éTAPE 2 : Supprimer boutons Import OCR existants
**Fichiers é modifier :**
- `Forms/FilonEditForm.Designer.cs` : Supprimer btnImportOcr
- Ajuster les positions des autres boutons (Export KML, Enregistrer, Annuler)

**Action requise :**
```csharp
// SUPPRIMER ces lignes dans FilonEditForm.Designer.cs :
var btnImportOcr = new TransparentGlassButton { ... };
btnImportOcr.Click += BtnImportOcr_Click;
panelButtons.Controls.Add(btnImportOcr);

// REPOSITIONNER :
btnExportKml.Location = new Point(10, 15);  // Au début
btnSave.Location = new Point(180, 15);
btnCancel.Location = new Point(350, 15);
```

---

### éTAPE 3 : Tous les boutons de FilonEditForm en transparent
**Status actuel :** déjé fait dans les modifications précédentes ?
- Tous les boutons utilisent déjé `TransparentGlassButton`
- Aucune action supplémentaire requise

---

### éTAPE 4 : Boutons transparents dans "Voir Fiche"
**Fichier é modifier :** `Form1.cs` (méthode `BtnViewFiches_Click`)

**Boutons é convertir :**
```csharp
// REMPLACER Button par TransparentGlassButton :
- btnVoirFiche (vert, gros bouton)
- btnExportAll (violet)
- btnClose (rouge)
- btnFermer (rouge dans la fiche compléte)
- btnEditer (bleu)
- btnLocaliser (vert)
- btnExporter (violet)
```

---

### éTAPE 5 : Systéme de galerie photo avec PIN ??
**Nouveaux fichiers é créer :**

#### `Models/PinProtection.cs`
```csharp
public class PinProtection
{
    public string EncryptedPin { get; set; }
    public bool IsLocked { get; set; }
    
    public static string EncryptPin(string pin) { ... }
    public bool ValidatePin(string inputPin) { ... }
}
```

#### `Forms/GalleryForm.cs`
```csharp
public class GalleryForm : Form
{
    private List<string> _photoPaths;
    private int _currentIndex;
    private PinProtection _pinProtection;
    
    // Fonctionnalités :
    // - Affichage diaporama
    // - Zoom/Rotation
    // - Premiére photo verrouillée avec PIN 4 chiffres
    // - Navigation suivant/précédent
    // - Suppression (avec confirmation)
}
```

#### `Forms/PinDialog.cs`
```csharp
public class PinDialog : Form
{
    // Dialogue pour saisir/définir le PIN
    // 4 TextBox pour les 4 chiffres
    // Validation automatique
    // 3 tentatives maximum
}
```

---

### éTAPE 6 : développer onglet Contacts ??
**Fichier é créer :** `Forms/ContactsPanel.cs`

**Champs préremplis :**
```csharp
public class ContactsPanel : Panel
{
    // Sections :
    // 1. Associations miniéres
    //    - ASEPAM (Association d'étude du Patrimoine Minier)
    //    - CPIE (Centre Permanent d'Initiatives pour l'Environnement)
    
    // 2. Musées
    //    - Musée de la Mine du Cap Garonne
    //    - Musée des Gueules Rouges (Tourves)
    
    // 3. Services publics
    //    - BRGM (Bureau de Recherches Géologiques et Miniéres)
    //    - DREAL PACA
    
    // 4. Experts locaux
    //    - Géologues
    //    - Spéléologues
    
    // 5. Secours
    //    - SDIS 83 (Pompiers)
    //    - PGHM (Peloton de Gendarmerie de Haute Montagne)
}
```

**Exemple de contact pré rempli :**
```
?? Musée de la Mine du Cap Garonne
   ?? contact@capgaronne.com
   ?? 04 94 05 50 20
   ?? www.capgaronne.com
   ?? Av. Général de Gaulle, 83320 Le Pradet
   
?? Visite guidée des galeries de cuivre
   Ouvert : Avril-Septembre, 14h-18h
```

---

### éTAPE 7 : Systéme de thémes ??
**Fichiers é créer :**

#### `Models/AppTheme.cs`
```csharp
public enum ThemeType
{
    Dark,      // Théme actuel
    Light,     // Théme clair
    Blue,      // Théme bleu
    Green,     // Théme vert
    Mineral    // Théme minéral (couleurs des minéraux)
}

public class AppTheme
{
    public ThemeType Type { get; set; }
    public Color BackgroundPrimary { get; set; }
    public Color BackgroundSecondary { get; set; }
    public Color TextPrimary { get; set; }
    public Color TextSecondary { get; set; }
    public Color AccentColor { get; set; }
    public Color ButtonBaseColor { get; set; }
    
    public static AppTheme GetTheme(ThemeType type) { ... }
}
```

#### `Services/ThemeService.cs`
```csharp
public class ThemeService
{
    public AppTheme CurrentTheme { get; private set; }
    
    public void ApplyTheme(Form form, AppTheme theme)
    {
        // Parcourir récursivement tous les contréles
        // Appliquer les couleurs du théme
    }
    
    public void SaveThemePreference(ThemeType type)
    {
        // Sauvegarder dans settings.json
    }
    
    public ThemeType LoadThemePreference()
    {
        // Charger depuis settings.json
    }
}
```

**Thémes disponibles :**
1. **Dark** (actuel) : Noir/gris
2. **Light** : Blanc/gris clair
3. **Blue** : Bleu marine
4. **Green** : Vert minéral
5. **Mineral** : Couleurs des minéraux (cuivre, or, etc.)

---

### éTAPE 8 : Onglet Paramétres ??
**Fichier é créer :** `Forms/SettingsPanel.cs`

**Sections de réglages :**

#### 1. Apparence
```csharp
- ComboBox "Théme" : Dark, Light, Blue, Green, Mineral
- CheckBox "Animations" : Activer/désactiver FadeIn
- Slider "Opacité" : 180-255
```

#### 2. Carte
```csharp
- ComboBox "Type de carte par défaut"
- CheckBox "Afficher légende"
- CheckBox "Afficher échelle"
- Slider "Zoom par défaut" : 8-15
```

#### 3. Import/Export
```csharp
- TextBox "Dossier d'import par défaut"
- TextBox "Dossier d'export par défaut"
- CheckBox "Validation automatique coordonnées"
- CheckBox "Conversion auto Lambert ? GPS"
```

#### 4. Sécurité
```csharp
- CheckBox "PIN pour premiére photo"
- Button "Changer le PIN"
- CheckBox "Confirmation avant suppression"
- CheckBox "Sauvegarde automatique"
```

#### 5. Données
```csharp
- Button "Sauvegarder toutes les données"
- Button "Restaurer depuis sauvegarde"
- Button "Exporter tout en CSV"
- Button "Nettoyer cache OCR"
- Label "Nombre de filons" : XX
- Label "Espace utilisé" : XX MB
```

#### 6. é propos
```csharp
- Label "WMine v1.0"
- Label "é 2025"
- Button "Voir licence"
- Button "Vérifier mises é jour"
- Button "Documentation"
```

---

## ?? ORDRE D'IMPLéMENTATION RECOMMANdé

1. ? **FAIT** : Onglet Import OCR
2. ? **SIMPLE** : Supprimer boutons Import OCR (5 min)
3. ? **SIMPLE** : Vérifier boutons transparents Fiche (10 min)
4. ? **MOYEN** : développer Contacts (30 min)
5. ? **MOYEN** : développer Paramétres (45 min)
6. ? **COMPLEXE** : Systéme de thémes (1h)
7. ? **COMPLEXE** : Galerie photo + PIN (1h30)

**Temps total estimé : 4h**

---

## ?? COMMANDES POUR CONTINUER

### Pour supprimer les boutons Import OCR :
```bash
# éditer FilonEditForm.Designer.cs
# Chercher "btnImportOcr" et supprimer toutes les occurrences
# Repositionner les 3 boutons restants
```

### Pour tester l'onglet Import OCR :
```bash
dotnet run
# Cliquer sur l'onglet "?? Import OCR"
# Sélectionner des fichiers
# Tester les imports
```

---

## ?? PROGRESSION GLOBALE

```
[????????????????????] 35% complété

? Onglet Import OCR créé
? Onglet Minéraux vidé
? Compilation réussie
? 7 étapes restantes
```

---

## ?? PROCHAINE ACTION

**Exécuter les étapes 2-4 (boutons transparents) - RAPIDE**

Puis compiler et tester avant de passer aux étapes complexes (Galerie PIN, Thémes, Paramétres).

---

**Date** : 08/01/2025
**Status** : ? En cours (étape 1/8 terminée)
**Compilation** : ? Réussie (0 erreurs)
