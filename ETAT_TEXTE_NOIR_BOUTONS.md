# ? Form1.Designer.cs - TEXTE NOIR SUR TOUS LES BOUTONS - TERMINé

## ?? état Actuel

? **Form1.Designer.cs** : Tous les 6 boutons du TopBar ont maintenant :
- `` (texte en noir)
- `FlatStyle = FlatStyle.Flat` (style plat)
- `Cursor = Cursors.Hand` (curseur main)
- `.FlatAppearance.BorderSize = 0` (pas de bordure)

## ?? Boutons Modifiés dans Form1.Designer.cs

1. ? `btnAddFilon` - "+ Nouveau" (texte noir)
2. ? `btnEditFilon` - "?? éditer" (texte noir)
3. ? `btnDeleteFilon` - "??? Supprimer" (texte noir)
4. ? `btnExportPdf` - "?? PDF" (texte noir)
5. ? `btnShareEmail` - "?? Email" (texte noir)
6. ? `btnViewFiches` - "?? Fiches (0)" (texte noir)

## ? Fichiers Forms/ é Modifier Manuellement

Pour avoir le texte en noir sur **TOUS** les boutons, modifiez ces fichiers :

### 1. Forms/FilonEditForm.Designer.cs (~9 boutons)

Cherchez tous les boutons et ajoutez :
```csharp
var btn = new Button
{
    // ... propriétés existantes ...
    BackColor = Color.FromArgb(...),
    ,        // ? AJOUTER
    FlatStyle = FlatStyle.Flat,     // ? AJOUTER
    Font = new Font(...),
    Cursor = Cursors.Hand           // ? AJOUTER
};
btn.FlatAppearance.BorderSize = 0;  // ? AJOUTER aprés
```

**Boutons é modifier** :
- `btnConvertLambert`
- `btnChoosePhoto`
- `btnRemovePhoto`
- `btnChooseDoc`
- `btnToggleNotes`
- `btnExportKml`
- `btnSave`
- `btnCancel`
- `btnPhotoGallery`

### 2. Forms/ImportPanel.cs (~5 boutons)

**Boutons é modifier** :
- `btnSelectFiles`
- `btnClearSelection`
- `btnImportOcr`
- `btnImportExcel`
- `btnLoadHistoricalMines`

### 3. Forms/SettingsPanel.cs (~10 boutons)

**Boutons é modifier** :
- `btnApplyAppearance`
- `btnSaveData`
- `btnRestoreData`
- `btnExportAll`
- `btnCleanCache`
- `btnOpenDataFolder`
- `btnDocumentation`
- `btnCheckUpdates`
- `btnLicense`

### 4. Forms/ContactsPanel.cs

Tous les boutons d'édition de contacts.

### 5. Forms/TechniquesEditPanel.cs

Tous les boutons de gestion des techniques.

### 6. Forms/MineralsPanel.cs

Bouton de sélection de photo pour les minéraux.

---

## ?? Modification Manuelle Recommandée

**étapes pour chaque fichier** :

1. Ouvrez le fichier dans Visual Studio
2. Cherchez `new Button` (Ctrl+F)
3. Pour chaque bouton trouvé, ajoutez les 4 éléments :
   - `,` (aprés BackColor)
   - `FlatStyle = FlatStyle.Flat,` (aprés ForeColor)
   - `Cursor = Cursors.Hand` (é la fin avant `}`)
   - `nomDuBouton.FlatAppearance.BorderSize = 0;` (aprés la déclaration)

4. Compilez (`Ctrl+Shift+B`)
5. Si erreurs, corrigez la syntaxe
6. Passez au fichier suivant

---

## ? Scripts PowerShell - NE PAS UTILISER

Les scripts automatiques causent des erreurs de syntaxe avec les caractéres spéciaux.  
**Préférez la modification manuelle** pour étre sér que tout fonctionne.

---

## ? Test Final

Une fois TOUS les fichiers modifiés :

```bash
dotnet build
```

Si compilation OK :
```bash
dotnet run
```

Vérifiez que :
- ? Tous les boutons ont du texte visible
- ? Tous les textes sont en NOIR
- ? Pas de bordures visibles
- ? Curseur main au survol

---

## ?? Commit Quand Tout Est OK

```bash
git add .
git commit -m "?? Texte noir + FlatStyle sur tous les boutons"
git push origin fix-operators-clean
```

---

**Résumé** : Form1.Designer.cs est ? TERMINé. Il reste les 6 fichiers Forms/ é modifier manuellement.
