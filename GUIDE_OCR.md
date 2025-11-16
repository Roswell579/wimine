# ?? Import OCR - Guide complet

## ? Installation et configuration

### 1. Vérifier l'installation des données OCR

Les données OCR franéaises sont maintenant installées et configurées automatiquement.

**Emplacement des fichiers :**
- Source : `wmine/tessdata/fra.traineddata` (13.5 MB)
- Sortie Debug : `wmine/bin/Debug/net8.0/tessdata/fra.traineddata`
- Sortie Release : `wmine/bin/Release/net8.0/tessdata/fra.traineddata`

### 2. Réinstaller manuellement si nécessaire

Si vous obtenez toujours une erreur de fichiers manquants :

```powershell
# Exécutez ce script dans le dossier du projet
.\install-ocr-data.ps1
```

Ou manuellement :
1. Téléchargez : https://github.com/tesseract-ocr/tessdata/raw/main/fra.traineddata
2. Placez dans : `wmine/tessdata/fra.traineddata`
3. Recompilez : `dotnet build`

## ?? Utilisation du bouton OCR

### étape 1 : Localiser le bouton

Le bouton **"?? Import OCR"** (orange ??) se trouve dans la colonne de gauche, en dessous du bouton "?? Fiche (X)".

### étape 2 : Préparer vos documents

#### Format attendu sur le document papier :

```
Nom du Filon              Lambert X   Lambert Y
?????????????????????????????????????????????????
Mine du Cap Garonne       971045      3144260
Mine de l'Argentiére      985420      3162580
Filon de la Madeleine     991234      3145678
Mine des Bormettes        978900      3156700
```

**Variantes acceptées :**

```
Mine du Cap    971045  3144260
Mine de l'Aigle   X=985420 Y=3162580
Filon Nord Lambert: 991234, 3145678
```

#### Conseils pour un bon scan :

- **Résolution** : 300 DPI minimum (600 DPI idéal)
- **Format** : JPG, PNG, TIFF, BMP
- **Contraste** : Texte noir sur fond blanc
- **Orientation** : Document droit (pas incliné)
- **Qualité** : Image nette, pas floue
- **éclairage** : Uniforme, sans ombres
- **Police** : éviter les polices manuscrites ou fantaisistes

### étape 3 : Utiliser l'interface OCR

1. **Cliquez** sur "?? Import OCR"
2. **Cliquez** sur "?? Sélectionner des images scannées"
3. **Choisissez** une ou plusieurs images (Ctrl+clic pour multi-sélection)
4. **Cliquez** sur "?? Lancer l'analyse OCR"
5. **Attendez** l'analyse (quelques secondes par page)
6. **Vérifiez** les résultats dans le tableau :
   - ? Vert = Filon détecté avec succés
   - ? Rouge = Ligne non reconnue ou format invalide
7. **Cochez** les filons que vous voulez importer
8. **Cliquez** sur "? Importer les filons sélectionnés"
9. **Confirmez** l'import

### étape 4 : Vérifier les résultats

Aprés l'import :
- Les filons apparaissent dans la liste principale
- Les coordonnées GPS sont automatiquement calculées depuis Lambert
- Les notes contiennent la source OCR et la ligne originale

## ?? Tableau des résultats OCR

Le tableau affiche 8 colonnes :

| Colonne | Description | Exemple |
|---------|-------------|---------|
| ? | Case é cocher | ?? |
| Nom du Filon | Nom détecté | "Mine du Cap" |
| Lambert X | Coordonnée X (métres) | 971045.00 |
| Lambert Y | Coordonnée Y (métres) | 3144260.00 |
| Latitude | GPS WGS84 (degrés) | 43.123456 |
| Longitude | GPS WGS84 (degrés) | 6.234567 |
| Statut | Validation | "? Valide" ou "? Format non reconnu" |
| Source | Fichier d'origine | "scan_page1.jpg" |

## ?? Validation des coordonnées

Le systéme valide automatiquement les coordonnées Lambert pour la région Var/Alpes-Maritimes :

- **Lambert X** : 850 000 é 1 150 000 métres
- **Lambert Y** : 2 950 000 é 3 250 000 métres

Si les coordonnées sont hors de ces limites, la ligne est marquée comme invalide.

## ?? Résolution des problémes

### Erreur : "Les données OCR franéaises sont manquantes"

**Solution 1 :** Exécutez le script d'installation
```powershell
.\install-ocr-data.ps1
```

**Solution 2 :** Téléchargement manuel
1. Allez sur : https://github.com/tesseract-ocr/tessdata/raw/main/fra.traineddata
2. Téléchargez le fichier (13.5 MB)
3. Placez-le dans : `wmine/tessdata/fra.traineddata`
4. Relancez l'application

### Erreur : "Aucun filon détecté"

**Causes possibles :**
- Qualité de scan insuffisante
- Format de document non standard
- Coordonnées hors limites géographiques
- Police de caractéres non reconnue

**Solutions :**
1. Améliorez la qualité du scan (300+ DPI)
2. Assurez-vous du format : `Nom X Y` sur chaque ligne
3. Vérifiez que les coordonnées sont Lambert 3 Zone Sud
4. Utilisez une police standard (Arial, Times, Courier)

### L'OCR détecte mal certains caractéres

**Problémes courants :**
- `0` (zéro) confondu avec `O` (lettre O)
- `1` (un) confondu avec `l` (L minuscule) ou `I` (i majuscule)
- `5` confondu avec `S`

**Solutions :**
- Augmentez la résolution du scan
- Utilisez une police plus nette
- Vérifiez manuellement les résultats avant import
- éditez les coordonnées aprés import si nécessaire

### Le bouton OCR n'apparaét pas

**Vérifications :**
1. Recompilez le projet : `dotnet build`
2. Relancez l'application : `F5`
3. Vérifiez dans la console de débogage (Output Window)
4. Cherchez le message : `? Bouton OCR ajouté é la position (X, Y)`

Si toujours absent, ajoutez temporairement ce code dans `Form1_Load` :
```csharp
var ocrButton = this.Controls.Find("btnImportOcr", true).FirstOrDefault();
MessageBox.Show(ocrButton != null ? 
    $"? Bouton trouvé é ({ocrButton.Left}, {ocrButton.Top})" : 
    "? Bouton non trouvé");
```

## ?? Exemple de document test

Créez ce document dans Word pour tester :

```
LISTE DES FILONS - DEPARTEMENT DU VAR

Mine du Cap Garonne          971045  3144260
Mine de l'Argentiére         985420  3162580
Filon de la Madeleine        991234  3145678
Mine des Bormettes           978900  3156700
Filon Saint-Pierre           982500  3158900
Mine de l'Esterel            995600  3142300
```

**Instructions :**
1. Tapez ce texte en Arial 14pt
2. Enregistrez en PDF ou capturez l'écran
3. Testez l'import OCR

## ?? Performances

| Métrique | Valeur |
|----------|--------|
| Temps d'analyse | ~2-5 secondes par page A4 |
| Précision attendue | 90-95% pour un bon scan |
| Taille de fichier max | Illimitée (mais traitement plus long) |
| Formats supportés | JPG, PNG, TIFF, BMP |
| Résolution recommandée | 300-600 DPI |

## ?? Ressources

- **Documentation Tesseract** : https://github.com/tesseract-ocr/tesseract
- **Données de langue** : https://github.com/tesseract-ocr/tessdata
- **Wiki OCR** : https://en.wikipedia.org/wiki/Optical_character_recognition

## ?? Support

En cas de probléme persistant :
1. Vérifiez le fichier `INSTALLATION_OCR.md`
2. Exécutez le script `install-ocr-data.ps1`
3. Consultez les logs dans la Console de débogage
4. Testez avec un document simple (voir exemple ci-dessus)
