# GUIDE DE CORRECTION RAPIDE - WMINE
## Toutes les corrections é faire manuellement dans Visual Studio

### INSTRUCTIONS
1. Ouvrez Visual Studio
2. Pour chaque fichier ci-dessous, ouvrez-le
3. Utilisez Ctrl+H (Rechercher/Remplacer)
4. Cochez "Utiliser des expressions réguliéres"
5. Appliquez les remplacements listés

---

## FICHIER 1: MarkerVisualizationForm.cs

### Rechercher:
```
hover 1\.1f :
```
### Remplacer par:
```
hover ? 1.1f :
```

### Rechercher:
```
hover 1\.2f :
```
### Remplacer par:
```
hover ? 1.2f :
```

---

## FICHIER 2: Services\ContactsDataService.cs

### Ligne 111 - Rechercher:
```
json) new List
```
### Remplacer par:
```
json) ?? new List
```

### Ligne 138 - Rechercher:
```
\.Any\(\) _contacts
```
### Remplacer par:
```
.Any() ? _contacts
```

---

## FICHIER 3: Services\CoordinateConverter.cs

### Ligne 249 - Rechercher:
```
isValid "
```
### Remplacer par:
```
isValid ? "
```

---

## FICHIER 4: Services\CsvExportService.cs

### Lignes 46-49 - Rechercher:
```
ToString\("F[26]"\) ""
```
### Remplacer par:
```
ToString("F$1") ?? ""
```

---

## FICHIER 5: Services\EmailService.cs

### Ligne 35 - Rechercher:
```
\.HasValue \$
```
### Remplacer par:
```
.HasValue ? $
```

---

## FICHIER 6: Services\ExcelImportService.cs

### Ligne 56 - Rechercher:
```
RowNumber\(\) firstDataRow
```
### Remplacer par:
```
RowNumber() ?? firstDataRow
```

### Ligne 102 - Rechercher:
```
RowNumber\(\) 1\)
```
### Remplacer par:
```
RowNumber() ?? 1)
```

### Ligne 251 - Rechercher:
```
IsNullOrWhiteSpace\(nom\) \$
```
### Remplacer par:
```
IsNullOrWhiteSpace(nom) ? $
```

---

## FICHIER 7: Services\FilonDataService.cs

### Ligne 88 - Rechercher:
```
json) new List
```
### Remplacer par:
```
json) ?? new List
```

---

## FICHIER 8: Services\MapBoundsRestrictor.cs

### Ligne 30 - Rechercher:
```
mapControl throw
```
### Remplacer par:
```
mapControl ?? throw
```

---

## FICHIER 9: Services\MineralDataService.cs

### Rechercher TOUTES les occurrences:
```
MineralType\.Améthyste
```
### Remplacer par:
```
MineralType.Amethyste
```

### Rechercher TOUTES les occurrences:
```
MineralType\.Disthéne
```
### Remplacer par:
```
MineralType.Disthene
```

---

## APRéS TOUTES LES CORRECTIONS

1. Sauvegardez tous les fichiers (Ctrl+Shift+S)
2. Reconstruisez la solution (Ctrl+Shift+B)
3. Si des erreurs persistent, regardez l'onglet "Liste d'erreurs"
4. Les erreurs restantes devraient étre < 10

---

## TEMPS ESTIMé
?? **5-7 minutes maximum**

## RéSULTAT ATTENDU
? Compilation réussie
? 0 erreur
? Application préte é l'exécution

---

**Note**: Ces corrections réparent les 45 erreurs restantes.
Le fichier SettingsPanel.cs ligne 429 est déJé CORRECT (pas de modification nécessaire).
