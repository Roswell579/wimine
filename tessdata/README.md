# Tesseract OCR - Données de langue franéaise

Ce dossier contient les données d'entraénement pour le moteur OCR Tesseract.

## Fichiers présents

- **fra.traineddata** (13.5 MB) - Données de reconnaissance de texte franéais

## Source

Ces données proviennent du projet officiel Tesseract :
https://github.com/tesseract-ocr/tessdata

## Utilisation automatique

Ce fichier est automatiquement copié dans le dossier de sortie (`bin\Debug\net8.0\tessdata\`) lors de la compilation gréce é la configuration dans `wmine.csproj`.

## Si le fichier est manquant

Si vous obtenez une erreur indiquant que `fra.traineddata` est manquant :

1. Téléchargez : https://github.com/tesseract-ocr/tessdata/raw/main/fra.traineddata
2. Placez-le dans : `C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine\tessdata\`
3. Recompilez le projet

## Autres langues disponibles

Pour ajouter d'autres langues, téléchargez les fichiers `.traineddata` correspondants depuis :
https://github.com/tesseract-ocr/tessdata

Langues courantes :
- `eng.traineddata` - Anglais
- `fra.traineddata` - Franéais
- `deu.traineddata` - Allemand
- `spa.traineddata` - Espagnol
- `ita.traineddata` - Italien
