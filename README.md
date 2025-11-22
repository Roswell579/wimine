<<<<<<< HEAD
﻿# ?? WMine Localisateur - Application de gestion de filons miniers

## ?? Description

**WMine Localisateur** est une application Windows de gestion et localisation de filons miniers avec cartographie interactive. Elle permet de cataloguer, localiser, documenter et partager des informations sur les gisements miniers.

---

## ? Fonctionnalités principales

### ??? Cartographie interactive
- Carte OpenStreetMap avec markers personnalisés (cristaux hexagonaux)
- Positionnement GPS précis des filons
- Zoom, déplacement, centrage automatique
- Conversion coordonnées Lambert 3 ? GPS (WGS84)

### ?? Gestion des filons
- **CRUD complet** : Créer, Lire, Modifier, Supprimer
- **Fiches détaillées** : Nom, coordonnées, minéral, statut, notes
- **Photos** : Association de dossiers photos par filon
- **Documentation PDF** : Lien vers documentation technique
- **Filtrage** : Par type de minéral

### ?? Import automatique ? NOUVEAU
- **Import Excel** : .xlsx, .xls (détection automatique des colonnes)
- **Import OCR** : Scan de documents papier (reconnaissance texte)
- **Import mixte** : Excel + Images simultanément
- **Validation** : Coordonnées Lambert automatiquement vérifiées
- **Conversion** : Lambert 3 ? GPS automatique

### ?? Export et partage
- **Export PDF** : Fiche compléte avec toutes les informations
- **Partage email** : Envoi direct avec PDF joint
- **Liste récapitulative** : Tableau de tous les filons

### ?? Interface moderne
- Design sombre professionnel
- Boutons en verre translucides avec effets
- Couleurs par type de minéral
- Animations et transitions fluides

---

## ?? Installation

### Prérequis
- **Windows 10/11**
- **.NET 8.0 Runtime**
- **Connexion Internet** (pour les tuiles de carte)

### Installation rapide
1. Clonez le repository
2. Installez les données OCR :
   ```powershell
   .\install-ocr-data.ps1
   ```
3. Compilez le projet :
   ```bash
   dotnet build
   ```
4. Lancez l'application :
   ```bash
   dotnet run
   ```

---

## ?? Packages utilisés

| Package | Version | Usage |
|---------|---------|-------|
| GMap.NET.Core | 2.1.7 | Cartographie |
| GMap.NET.WinForms | 2.1.7 | Contréles carte |
| QuestPDF | 2024.12.3 | Export PDF |
| Newtonsoft.Json | 13.0.3 | Sérialisation données |
| MaterialSkin.2 | 2.3.1 | Interface moderne |
| Tesseract | 5.2.0 | OCR (reconnaissance texte) |
| ClosedXML | 0.104.2 | Import Excel |

---

## ?? Documentation

### Guides utilisateur
- **[GUIDE_IMPORT_COMPLET.md](GUIDE_IMPORT_COMPLET.md)** - Guide principal import OCR & Excel
- **[GUIDE_OCR.md](GUIDE_OCR.md)** - détails OCR et résolution de problémes
- **[GUIDE_EXCEL_IMPORT.md](GUIDE_EXCEL_IMPORT.md)** - Format Excel et bonnes pratiques

### Documentation technique
- **[INSTALLATION_OCR.md](INSTALLATION_OCR.md)** - Installation Tesseract OCR
- **[JOURNAL_DEVELOPPEMENT.md](JOURNAL_DEVELOPPEMENT.md)** - Historique du projet
- **[ETAPE_OCR_OK.md](ETAPE_OCR_OK.md)** - Validation étape OCR
- **[README_ETAPE_OCR.md](README_ETAPE_OCR.md)** - Récapitulatif étape OCR

---

## ?? Utilisation

### Créer un nouveau filon

**Option 1 : Placement sur carte**
1. Cliquez sur "**+ Nouveau**"
2. Choisissez "**?? Placer un pin sur la carte**"
3. Cliquez sur la carte é l'emplacement désiré
4. Remplissez le formulaire
5. Sauvegardez

**Option 2 : Saisie directe**
1. Cliquez sur "**+ Nouveau**"
2. Choisissez "**?? Créer directement**"
3. Remplissez le formulaire (avec ou sans coordonnées)
4. Sauvegardez

### Importer des filons en masse ? NOUVEAU

**Depuis Excel** :
1. Préparez votre fichier Excel avec colonnes : `Nom`, `Lambert X`, `Lambert Y`
2. Cliquez sur "**?? Import OCR**"
3. Sélectionnez votre fichier .xlsx
4. Cliquez sur "**?? Lancer l'import Excel**"
5. Vérifiez et importez

**Depuis document papier (OCR)** :
1. Scannez votre liste de filons (300+ DPI)
2. Cliquez sur "**?? Import OCR**"
3. Sélectionnez vos images
4. Cliquez sur "**?? Lancer l'analyse OCR**"
5. Vérifiez et importez

**Import mixte** :
1. Sélectionnez **é la fois** des fichiers Excel ET des images
2. Le systéme traite automatiquement chaque type
3. Résultats combinés dans un seul tableau

### éditer un filon
1. Double-cliquez sur le marker de la carte OU
2. Sélectionnez dans la liste et cliquez "**?? éditer**"
3. Modifiez les informations
4. Sauvegardez

### Afficher une fiche compléte
1. Cliquez sur "**?? Fiche (X)**"
2. Sélectionnez un filon dans la liste
3. Cliquez sur "**?? VOIR FICHE**"
4. Consultez toutes les informations détaillées

### Exporter en PDF
1. Sélectionnez un filon
2. Cliquez sur "**?? Export PDF**"
3. Choisissez l'emplacement
4. Le PDF est généré avec toutes les informations

---

## ??? Structure des données

### Format de stockage
Les filons sont sauvegardés en JSON dans :
```
%LOCALAPPDATA%\WMine\filons.json
```

### Structure d'un filon
```json
{
  "Id": "guid",
  "Nom": "Mine du Cap Garonne",
  "MatierePrincipale": "Cuivre",
  "Latitude": 43.123456,
  "Longitude": 6.234567,
  "LambertX": 971045.0,
  "LambertY": 3144260.0,
  "Statut": "Exploite|Epuise",
  "Notes": "Description détaillée...",
  "PhotoPath": "C:\\Photos\\Cap",
  "DocumentationPath": "C:\\Docs\\cap.pdf",
  "DateCreation": "2025-11-07T00:00:00",
  "DateModification": "2025-11-07T00:00:00"
}
```

---

## ?? Types de minéraux supportés

| Minéral | Couleur | Code RGB |
|---------|---------|----------|
| Fer | Rouille | 184, 134, 11 |
| Cuivre | Cuivre | 255, 153, 51 |
| Plomb | Gris foncé | 70, 70, 70 |
| Zinc | Gris clair | 169, 169, 169 |
| Or | Or | 255, 215, 0 |
| Argent | Argent | 192, 192, 192 |
| étain | Gris métal | 119, 136, 153 |
| Tungsténe | Gris foncé | 49, 49, 49 |
| Manganése | Violet | 138, 43, 226 |
| Bauxite | Rouge | 205, 92, 92 |
| Autre | Bleu | 100, 181, 246 |

---

## ??? Systémes de coordonnées

L'application supporte deux systémes :

### GPS (WGS84)
- Latitude : degrés décimaux (-90 é +90)
- Longitude : degrés décimaux (-180 é +180)
- Usage : Affichage carte, GPS mobile

### Lambert 3 Zone Sud (NTF)
- X (Est) : métres (850 000 é 1 150 000)
- Y (Nord) : métres (2 950 000 é 3 250 000)
- Zone couverte : Var et Alpes-Maritimes
- Usage : Cartes IGN, topographie

**Conversion automatique** : L'application convertit automatiquement entre les deux systémes.

---

## ?? Formats d'import supportés

### Excel (.xlsx, .xls)
```
| Nom du Filon | Lambert X | Lambert Y |
|--------------|-----------|-----------|
| Mine du Cap  | 971045    | 3144260   |
```

### CSV (via Excel)
```csv
Nom du Filon,Lambert X,Lambert Y
Mine du Cap,971045,3144260
```

### Images OCR (JPG, PNG, TIFF, BMP)
```
Mine du Cap    971045  3144260
Mine de l'Aigle  985420  3162580
```

---

## ?? Fichiers de test

### Test Excel
Utilisez `test-import-filons.csv` :
- 10 filons de test
- Coordonnées réelles du Var
- Prét é importer

### Test OCR
Utilisez `test-ocr-filons.txt` :
- 10 filons formatés
- é scanner en image
- Testez la reconnaissance texte

---

## ??? développement

### Architecture
```
wmine/
??? Forms/          # Interfaces utilisateur
??? Services/       # Logique métier
??? Models/         # Modéles de données
??? UI/             # Composants UI personnalisés
??? Utils/          # Utilitaires (conversions, etc.)
??? tessdata/       # Données OCR
```

### Technologies
- **.NET 8.0** - Framework principal
- **WinForms** - Interface graphique
- **GMap.NET** - Cartographie
- **Tesseract** - OCR
- **ClosedXML** - Excel
- **QuestPDF** - Export PDF

### Compilation
```bash
dotnet build
dotnet run
```

### Tests
```bash
dotnet test
```

---

## ?? Résolution de problémes

### "Les données OCR franéaises sont manquantes"
```powershell
# Exécutez le script d'installation
.\install-ocr-data.ps1
```

### "Colonnes Excel non détectées"
Assurez-vous d'avoir des en-tétes : `Nom`, `Lambert X`, `Lambert Y`

### "Coordonnées hors zone géographique"
Vérifiez que vous utilisez Lambert 3 Zone Sud (Var/Alpes-Maritimes)

### Plus de détails
Consultez les guides dans `/Documentation`

---

## ?? Changelog

### Version actuelle (Post-OCR OK)
- ? Import OCR (Tesseract)
- ? Import Excel (ClosedXML)
- ? Import mixte
- ? Validation coordonnées automatique
- ? Conversion Lambert ? GPS

### Version précédente
- Gestion manuelle CRUD
- Carte interactive
- Export PDF
- Partage email
- Fiches détaillées

---

## ?? Contribution

### Idées d'amélioration
- Export KML/KMZ (Google Earth)
- Export Shapefile (SIG)
- Module photos géolocalisées
- Synchronisation cloud
- Application mobile compagnon

### Contact
Pour toute suggestion ou bug, consultez `JOURNAL_DEVELOPPEMENT.md`

---

## ?? Licence

Projet personnel - Tous droits réservés

---

## ?? Crédits

- **GMap.NET** - Cartographie
- **Tesseract** - OCR open-source
- **ClosedXML** - Lecture Excel
- **QuestPDF** - Export PDF
- **GitHub Copilot** - Assistance développement

---

## ?? Support

### Documentation compléte
- `GUIDE_IMPORT_COMPLET.md` - Guide principal
- `GUIDE_OCR.md` - OCR détaillé
- `GUIDE_EXCEL_IMPORT.md` - Excel détaillé
- `INSTALLATION_OCR.md` - Installation

### Scripts d'aide
- `install-ocr-data.ps1` - Installation OCR
- `create-test-excel.ps1` - Fichier test

---

**WMine Localisateur** - Gérez vos filons miniers simplement et efficacement ! ?????

**Version** : Post-OCR OK ?  
**Derniére mise é jour** : 07/11/2025
=======
# wimine
sig mine var france
>>>>>>> origin/master
