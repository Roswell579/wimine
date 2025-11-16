# ?? Installation du systéme OCR

## ? étape 1 : Installer le package Tesseract

Ouvrez le **Terminal** dans Visual Studio et exécutez :

```bash
dotnet add package Tesseract --version 5.2.0
```

## ?? étape 2 : Télécharger les données de langue franéaise

1. **Téléchargez** le fichier de données OCR franéaises :
   - URL : https://github.com/tesseract-ocr/tessdata/raw/main/fra.traineddata

2. **Créez le dossier** dans votre projet :
   ```
   C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine\bin\Debug\net8.0\tessdata\
   ```

3. **Placez** le fichier `fra.traineddata` dans ce dossier

## ?? étape 3 : Recompiler le projet

Aprés avoir installé le package Tesseract :

```bash
dotnet build
```

## ?? Utilisation

Le bouton **"?? Import OCR"** apparaétra automatiquement dans votre interface principale.

### Comment utiliser l'import OCR :

1. **Scannez** votre liste papier de filons (format JPG, PNG, etc.)
2. Cliquez sur **"?? Import OCR"**
3. Sélectionnez les images scannées
4. Cliquez sur **"?? Lancer l'analyse OCR"**
5. Vérifiez les résultats détectés
6. Cochez les filons é importer
7. Cliquez sur **"? Importer"**

### Format attendu dans les documents scannés :

L'OCR détecte automatiquement ces formats :

```
Nom du filon    971045  3144260
Mine du Cap     X=971045 Y=3144260
Cap Lambert: 971045, 3144260
```

Les coordonnées Lambert X et Y doivent étre :
- **X** : 6-7 chiffres (environ 850000 é 1150000)
- **Y** : 6-7 chiffres (environ 2950000 é 3250000)

## ? En cas de probléme

### Si le bouton OCR n'apparaét pas :
- Vérifiez que le package Tesseract est bien installé
- Recompilez le projet (`Ctrl+Shift+B`)
- Relancez l'application (`F5`)

### Si l'OCR ne détecte rien :
- Assurez-vous que le fichier `fra.traineddata` est bien dans le dossier `tessdata`
- Vérifiez la qualité du scan (résolution minimale 300 DPI recommandée)
- Assurez-vous que le texte est bien lisible

## ?? Position du bouton

Le bouton **"?? Import OCR"** sera automatiquement placé **en dessous** du bouton "?? Fiche (X)" dans la colonne de gauche de l'interface principale.

Couleur : **Orange** ?? pour le distinguer des autres actions.
