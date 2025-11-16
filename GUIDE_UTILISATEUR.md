# ?? GUIDE D'UTILISATION - APPLICATION WMINE

## ?? LANCEMENT DE L'APPLICATION

### Méthode 1 : Visual Studio
1. Ouvrez `wmine.sln` dans Visual Studio
2. Appuyez sur **F5** (ou cliquez sur le bouton ? "wmine")
3. L'application démarre automatiquement

### Méthode 2 : Exécutable
```
Naviguez vers : wmine/bin/Debug/net8.0/
Double-cliquez sur : wmine.exe
```

---

## ?? LES 4 ONGLETS PRINCIPAUX

Au lancement, vous verrez **4 onglets en haut de l'application** :

```
???????????????????????????????????????????????????????????????????
?  ??? Les minéraux du Var  ?  ?? Techniques  ?  ?? Contacts  ?  ?? Paramétres  ?
???????????????????????????????????????????????????????????????????
```

### ??? ONGLET 1 : Les minéraux du Var
**C'est l'onglet principal avec la carte interactive**

#### En haut de la fenétre :
```
??????????????????????????????????????????????????????????????????
?  [+ Nouveau] [éditer] [Supprimer] [Export PDF] [Partager]    ?
?                                                  [?? Liste]    ?
?  Sélectionner: [____?]        Filtre: [____?]                 ?
??????????????????????????????????????????????????????????????????
```

#### Au centre :
- **Carte interactive** avec les filons représentés par des cristaux colorés
- **Contréles flottants** :
  - ??? Sélecteur de carte (haut gauche)
  - ?? Position GPS (bas droite)
  - ?? échelle (bas gauche)
  - ? Bouton cacher/afficher panel (haut centre)

#### Actions disponibles :
- **Click gauche** sur un marqueur ? Sélectionne le filon
- **Double-click** sur un marqueur ? Ouvre l'édition
- **Click droit** sur la carte ? Menu contextuel
- **Click droit** sur un marqueur ? Menu filon

---

### ?? ONGLET 2 : Techniques d'extraction miniére
**Section de contenu éducatif (é remplir)**

Structure actuelle :
```
?? Techniques d'extraction miniére

Contenu é développer...
```

*Cette section est préte pour recevoir du contenu sur les techniques historiques et modernes d'extraction miniére.*

---

### ?? ONGLET 3 : Contacts
**Annuaire de contacts et ressources**

Structure actuelle :
```
?? Contacts et Ressources

Contenu é développer...
```

*Cette section est préte pour recevoir les contacts d'associations, musées, experts, etc.*

---

### ?? ONGLET 4 : Paramétres
**Configuration de l'application**

Structure actuelle :
```
?? Configuration de l'application

Contenu é développer...
```

*Cette section est préte pour les paramétres de carte, préférences d'affichage, gestion des données, etc.*

---

## ?? éDITION D'UN FILON

Quand vous ouvrez l'édition d'un filon, vous verrez **2 colonnes** :

### COLONNE GAUCHE
```
???????????????????????????????
? Nom du filon:               ?
? [___________________]       ?
?                             ?
? Matiére principale:         ?
? ? Bauxite                   ?
? ? Cuivre                    ?
? ? Fer                       ?
? ...                         ?
?                             ?
? Matiéres secondaires:       ?
? ? Bauxite                   ?
? ? Cuivre                    ?
? ...                         ?
?                             ?
? ?? Coordonnées              ?
? Lambert X: [____] Y: [____] ?
?          [??? ? GPS]         ?
? Latitude:  [____]           ?
? Longitude: [____]           ?
???????????????????????????????
```

### COLONNE DROITE
```
???????????????????????????????
? ?? état du filon            ?
? ? Actif                     ?
? ? Abandonné                 ?
? ? Inondé                    ?
? ? éboulé                    ?
? ? Foudroyé                  ?
? ? Danger mortel             ?
?                             ?
? ?? Ancrage (année): [____]  ?
?                             ?
? ?? Photo et Documentation   ?
? [Image preview]             ?
? [?? Choisir]  [??? Suppr.]  ?
? [?? Choisir PDF]            ?
? [??? Galerie]  ? NOUVEAU     ?
?                             ?
? ?? Notes                    ?
? [? Afficher Notes]          ?
?                             ?
? ?????????????????????????  ?
? ?[?? Import] [??? Export]?  ?
? ?         [??] [?]      ?  ?
? ?????????????????????????  ?
???????????????????????????????
```

### ?? NOUVEAUX BOUTONS

#### ??? Galerie Photo
- **Emplacement** : Dans la section "Photo et Documentation"
- **Couleur** : Violet
- **Fonction** : Ouvrir la galerie de photos du filon
- **Status** : Bouton visible, fonctionnalité é venir

#### ??? Export KML
- **Emplacement** : Barre de boutons en bas, entre "Import OCR" et "Enregistrer"
- **Couleur** : Vert
- **Fonction** : Exporter le filon au format KML pour Google Earth
- **Status** : ? Totalement fonctionnel

---

## ??? UTILISATION DE L'EXPORT KML

### étape par étape :

1. **Ouvrez l'édition d'un filon** qui posséde des coordonnées GPS
2. **Vérifiez** que Latitude et Longitude sont renseignées
3. **Cliquez** sur le bouton **"??? Export KML"** (vert, en bas)
4. **Choisissez** l'emplacement et le nom du fichier
5. **Cliquez** sur "Enregistrer"
6. **Résultat** : Message "? Export KML réussi !"

### Utilisation du fichier KML :

#### Avec Google Earth Desktop
1. Ouvrez Google Earth
2. Fichier ? Ouvrir
3. Sélectionnez votre fichier `.kml`
4. Le filon s'affiche avec son pin et ses informations

#### Avec Google Earth Web
1. Allez sur https://earth.google.com/web/
2. Icéne menu (?) ? Projets
3. Importer un fichier KML
4. Sélectionnez votre fichier `.kml`

---

## ?? APERéU DES COULEURS

### Par type de minéral (marqueurs sur la carte) :
```
?? Bauxite      - Rouge
?? Cuivre       - Orange
?? Fer          - Marron
?? Or           - Or/Jaune
? Argent       - Argent
? Plomb        - Gris foncé
?? Zinc         - Violet
?? Chrome       - Vert
?? Manganése    - Bleu
?? Fluorine     - Orange clair
```

### Boutons TransparentGlassButton :
```
?? Vert         - Actions positives (Enregistrer, Nouveau, Export KML)
?? Bleu         - édition
?? Rouge        - Suppression/Annulation
?? Orange       - Import OCR
?? Violet       - Galerie/Média
? Gris         - Actions neutres
```

---

## ?? IMPORT MASSIF (OCR & EXCEL)

### Bouton "?? Import OCR"
**Emplacement** : Barre de boutons en bas de FilonEditForm

**Fonctionnalités** :
1. Import depuis fichiers **Excel** (.xlsx, .xls)
2. Import depuis **images scannées** (JPG, PNG, TIFF, BMP) avec OCR
3. Import **mixte** (Excel + images en méme temps)

**Utilisation** :
1. Cliquez sur "?? Import OCR"
2. Fenétre d'import s'ouvre
3. Cliquez sur "?? Sélectionner des fichiers"
4. Choisissez vos fichiers (Excel et/ou images)
5. Cliquez sur "?? Lancer l'analyse"
6. Vérifiez les résultats dans le tableau
7. Cochez les filons é importer
8. Cliquez sur "? Importer les filons sélectionnés"

**Consultez** `Documentation/GUIDE_OCR.md` pour plus de détails.

---

## ?? RACCOURCIS CLAVIER

```
F5              - Lancer l'application (Visual Studio)
Ctrl + S        - Enregistrer (dans les formulaires)
échap           - Fermer/Annuler
Entrée          - Valider (dans les formulaires)
Click droit     - Menu contextuel
Double-click    - édition rapide
```

---

## ?? LISTE DES FILONS (Bouton "?? Liste")

Cliquez sur **"?? Liste (X)"** pour voir :
- Tableau de tous les filons
- Colonnes : Nom, Minéral, Latitude, Longitude, Lambert X/Y, Statut
- **Double-click** sur une ligne ? Localise sur la carte
- **Bouton vert "VOIR FICHE"** ? Affiche la fiche compléte

---

## ?? ZONES DE SéCURITé

### ?? Ancrage (année)
**Zone é risque** - Affiché en **ROUGE**

Si vous saisissez une année d'ancrage :
- Tooltip de sécurité s'affiche
- Avertissement sur les risques de galeries anciennes
- **TOUJOURS vérifier la solidité avant d'entrer !**

### ?? Statut "Danger mortel"
Cocher ce statut affiche un **avertissement critique**.

---

## ?? CONSEILS D'UTILISATION

### Pour débuter :
1. ? Explorez l'**onglet 1** (carte)
2. ? Créez votre premier filon avec **"+ Nouveau"**
3. ? Testez le mode **"Placer un pin sur la carte"**
4. ? Utilisez les **filtres** par minéral
5. ? Testez l'**export KML** avec Google Earth

### Pour un usage avancé :
1. ? Importez en masse avec **Import OCR**
2. ? Organisez vos données avec **filtres avancés**
3. ? Exportez des **PDF** pour documentation
4. ? Partagez par **email**
5. ? Consultez la **liste compléte** réguliérement

---

## ?? EN CAS DE PROBLéME

### L'application ne démarre pas
```powershell
# Recompilez :
cd wmine
dotnet clean
dotnet build
```

### Les onglets ne s'affichent pas
**Vérifiez** :
1. La compilation est réussie (0 erreurs)
2. Vous utilisez bien la version corrigée de `Form1.Designer.cs`
3. Le fichier `ETAT_FINAL_CORRIGES.md` confirme que tout est OK

### Le bouton Export KML n'apparaét pas
**Vérifiez** :
1. Vous étes bien dans l'**édition d'un filon** (pas dans Form1)
2. Le bouton est **entre Import OCR et Enregistrer**
3. Couleur : **Vert**, Texte : **"??? Export KML"**

---

## ?? DOCUMENTATION COMPLéTE

Consultez le dossier `Documentation/` pour :
- `GUIDE_OCR.md` - Import OCR détaillé
- `GUIDE_EXCEL_IMPORT.md` - Format Excel
- `GUIDE_EXPORT_CSV.md` - Export CSV
- `AIDE_RAPIDE.md` - démarrage rapide
- Et 17 autres guides...

---

## ? CHECKLIST DE VéRIFICATION

Avant de commencer, vérifiez que :
- [ ] L'application compile sans erreur
- [ ] Les 4 onglets sont visibles en haut
- [ ] La carte s'affiche dans l'onglet 1
- [ ] Le bouton "Export KML" est visible en édition de filon
- [ ] Le bouton "Galerie" est visible dans la section Médias
- [ ] Tous les boutons sont en TransparentGlassButton

**Si tout est ?, vous étes prét é utiliser WMine !** ??????

---

**Bonne exploration minérale ! ??????**
