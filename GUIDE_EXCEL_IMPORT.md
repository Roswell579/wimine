# Guide d'utilisation - Import Excel

## ?? Format Excel supporté

Votre fichier Excel (.xlsx ou .xls) doit contenir **au minimum 3 colonnes** :

### Format avec en-téte (recommandé) :

| Nom | Lambert X | Lambert Y |
|-----|-----------|-----------|
| Mine du Cap Garonne | 971045 | 3144260 |
| Mine de l'Argentiére | 985420 | 3162580 |
| Filon de la Madeleine | 991234 | 3145678 |

### Variantes d'en-tétes acceptées :

**Pour la colonne Nom :**
- `Nom`, `Filon`, `Mine`, `Site`

**Pour Lambert X :**
- `Lambert X`, `X`, `Est`, `Easting`

**Pour Lambert Y :**
- `Lambert Y`, `Y`, `Nord`, `Northing`

### Format sans en-téte :

Si votre fichier n'a pas d'en-téte, les colonnes seront détectées automatiquement :
- Colonne 1 = Nom
- Colonne 2 = Lambert X
- Colonne 3 = Lambert Y

## ?? Exemple de fichier Excel

Créez un fichier Excel avec ce contenu :

```
Nom du Filon              Lambert X    Lambert Y
Mine du Cap Garonne       971045       3144260
Mine de l'Argentiére      985420       3162580
Filon de la Madeleine     991234       3145678
Mine des Bormettes        978900       3156700
Filon Saint-Pierre        982500       3158900
Mine de l'Esterel         995600       3142300
Filon des Maures          988750       3151200
Mine de la Colle          976800       3159400
Filon du Pradet           972300       3143800
Mine des Salettes         993400       3147900
```

## ? Régles de validation

1. **Lambert X** : doit étre entre 850 000 et 1 150 000
2. **Lambert Y** : doit étre entre 2 950 000 et 3 250 000
3. **Nom** : ne peut pas étre vide
4. **Coordonnées** : sont converties automatiquement en GPS (WGS84)

## ?? Import mixte (Excel + OCR)

Vous pouvez sélectionner **é la fois** :
- Des fichiers Excel (.xlsx, .xls)
- Des images scannées (.jpg, .png, .tiff, .bmp)

Le systéme traitera automatiquement :
- Les fichiers Excel ? lecture directe
- Les images ? reconnaissance OCR

## ?? Utilisation

1. **Cliquez** sur "?? Sélectionner des fichiers (Excel ou Images)"
2. **Choisissez** vos fichiers (Excel et/ou images)
3. **Cliquez** sur "?? Lancer l'import"
4. **Vérifiez** les résultats dans le tableau
5. **Cochez** les filons é importer
6. **Cliquez** sur "? Importer les filons sélectionnés"

## ?? Conseils

- **Préparez vos données** dans Excel avant l'import
- **Vérifiez les coordonnées** Lambert avant d'importer
- **Testez** avec quelques lignes d'abord
- **Sauvegardez** votre base avant un import massif
- **Utilisez Excel** pour des données propres et structurées
- **Utilisez OCR** pour des documents papier scannés

## ?? Résolution de problémes

### Erreur : "Colonnes non détectées"

**Solution :**
1. Assurez-vous d'avoir des en-tétes de colonnes
2. Nommez vos colonnes : `Nom`, `Lambert X`, `Lambert Y`
3. Ou utilisez le format sans en-téte (colonnes 1, 2, 3)

### Erreur : "Coordonnées hors zone géographique"

**Solution :**
- Vérifiez que vous utilisez Lambert III Zone Sud
- Les valeurs doivent étre en métres, pas en degrés
- Zone couverte : Var et Alpes-Maritimes

### Certaines lignes ne sont pas importées

**Solution :**
- Vérifiez les coordonnées (format numérique)
- Assurez-vous que le nom n'est pas vide
- Consultez la colonne "Statut" pour voir l'erreur

## ?? Support

En cas de probléme, consultez le fichier `GUIDE_OCR.md` pour plus de détails.
