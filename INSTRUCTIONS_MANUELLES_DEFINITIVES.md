# ? SOLUTION déFINITIVE - PAS DE SCRIPT

## ?? INSTRUCTIONS MANUELLES (5 MINUTES)

### étape 1 : Ouvrez Visual Studio

### étape 2 : Rechercher/Remplacer

1. **Appuyez sur** : `Ctrl+H` (Rechercher et remplacer)

2. **Cochez** : "Respecter la casse"

3. **Dans "Rechercher"**, tapez **EXACTEMENT** :
```
ForeColor = Color.White,
```

4. **Dans "Remplacer par"**, tapez **EXACTEMENT** :
```

```

5. **Cliquez sur** : "Remplacer tout"

### étape 3 : Ajoutez FlatStyle et BorderSize

Pour chaque fichier Forms/*.cs, ajoutez manuellement aprés chaque `BackColor` :

**AVANT** :
```csharp
new Button
{
    Text = "Action",
    BackColor = Color.FromArgb(0, 150, 136),
    ForeColor = Color.White,
    Font = new Font("Segoe UI", 10, FontStyle.Bold)
};
```

**APRéS** :
```csharp
new Button
{
    Text = "Action",
    BackColor = Color.FromArgb(0, 150, 136),
    
    FlatStyle = FlatStyle.Flat,
    Font = new Font("Segoe UI", 10, FontStyle.Bold),
    Cursor = Cursors.Hand
};
nomDuBouton.FlatAppearance.BorderSize = 0;
```

### étape 4 : Compilez

`Ctrl+Shift+B`

---

## ?? FICHIERS é MODIFIER (6 fichiers)

1. ? Form1.Designer.cs (déJé FAIT - ne touchez pas)
2. Forms/FilonEditForm.Designer.cs
3. Forms/ImportPanel.cs
4. Forms/SettingsPanel.cs
5. Forms/ContactsPanel.cs
6. Forms/TechniquesEditPanel.cs

---

## ?? IMPORTANT - ENCODAGE

Quand Visual Studio demande l'encodage :

**CHOISISSEZ** : "Unicode (UTF-8 avec signature) - Page de codes 65001"

---

## ? RéSULTAT FINAL

- Tous les textes des boutons en NOIR
- Style plat (FlatStyle.Flat)
- Pas de bordures
- Curseur main au survol

---

**Temps estimé** : 5-10 minutes de modification manuelle, mais GARANTI de fonctionner.
