# ??? PANNEAU D'OUTILS FLOTTANT - GUIDE D'UTILISATION

## ?? **LOCALISATION**

Le panneau d'outils est situé **à gauche de la carte**, sous le widget météo.

```
???????????????????????????????
? Carte                       ?
?                             ?
? ??? [Sélecteur carte]      ?
? ???                         ?
?                             ?
? ?? ?? ?? [Boutons rotation] ?
?                             ?
? ???????????????????         ?
? ? ??? Météo       ?         ?
? ???????????????????         ?
?                             ?
? ???????????????????         ?
? ? ??? OUTILS       ? ?? ICI ?
? ?                 ?         ?
? ? ?? Mesurer      ?         ?
? ? ?? Photos GPS   ?         ?
? ? ?? Filons Proches?        ?
? ? ?? Tracer Zone  ?         ?
? ? ??? Export KMZ   ?         ?
? ? ? Aide         ?         ?
? ???????????????????         ?
???????????????????????????????
```

---

## ??? **LES 6 OUTILS DISPONIBLES**

### 1. ?? **Mesurer Distance**

**À quoi ça sert ?**
- Mesurer la distance entre 2 points sur la carte
- Obtenir le cap/azimut (direction)
- Estimer le temps de trajet (pied/vélo/voiture)

**Comment l'utiliser ?**
1. Cliquez sur le bouton **?? Mesurer Distance**
2. Le bouton devient **rouge** (mode actif)
3. Cliquez sur le **premier point** de la carte
4. Cliquez sur le **deuxième point**
5. Une fenêtre affiche :
   - Distance exacte (ex: "1.5 km")
   - Direction cardinale (ex: "Nord-Est (45°)")
   - Temps estimés :
     - ?? À pied: 18 min
     - ?? À vélo: 6 min
     - ?? En voiture: 2 min

**Exemple :**
```
?? Distance: 1.5 km
?? Direction: Nord-Est (45°)

?? Temps estimé:
  ?? À pied: 18 min
  ?? À vélo: 6 min
  ?? En voiture: 2 min
```

---

### 2. ?? **Import Photos GPS**

**À quoi ça sert ?**
- Importer des photos prises avec un smartphone/appareil GPS
- Associer automatiquement les photos aux filons proches
- Copier les photos dans les dossiers des filons

**Comment l'utiliser ?**
1. Cliquez sur **?? Import Photos GPS**
2. Sélectionnez un dossier contenant vos photos
3. L'application analyse les données EXIF GPS
4. Les photos sont automatiquement associées aux filons dans un rayon de 500m
5. Un résumé s'affiche :
   ```
   ?? Import de Photos avec GPS
   ???????????????????????????????

   Total photos analysées: 25
     ? Avec GPS: 18
     ? Sans GPS: 7

   ?? Association aux filons:
     ? Matchées: 12
     ?? Non matchées: 6
   ```

**Prérequis :**
- Photos avec géolocalisation activée (EXIF GPS)
- Filons existants avec coordonnées GPS

---

### 3. ?? **Filons Proches**

**À quoi ça sert ?**
- Trouver tous les filons dans un rayon autour d'un point
- Voir les distances exactes
- Planifier une sortie terrain

**Comment l'utiliser ?**
1. **Centrez la carte** sur le point de départ (ex: votre position)
2. Cliquez sur **?? Filons Proches**
3. Entrez le rayon de recherche (ex: **5 km**)
4. Cliquez **Rechercher**
5. Une liste s'affiche avec tous les filons proches triés par distance

**Exemple de résultat :**
```
?? 10 filon(s) trouvé(s) dans 5 km:

?? Mine du Cap Garonne - 850 m
?? Filon des Maures - 1.2 km
?? Carrière de bauxite - 2.5 km
?? Filon de cuivre - 3.8 km
... et 6 autres
```

---

### 4. ?? **Tracer Zone** ?? (En développement)

**À quoi ça servira ?**
- Dessiner des polygones sur la carte
- Délimiter des zones de recherche
- Calculer la surface d'une zone
- Trouver tous les filons dans la zone

**Fonctionnalités prévues :**
- Clic pour placer des points
- Fermeture automatique du polygone
- Calcul de surface (km²)
- Liste des filons dans la zone
- Sauvegarde des zones

---

### 5. ??? **Export KMZ** ?? (En développement)

**À quoi ça servira ?**
- Exporter tous vos filons avec photos
- Format KMZ (KML compressé avec images)
- Compatible Google Earth
- Partage facile

**Contenu du fichier KMZ :**
- Tous les filons avec coordonnées
- Photos de chaque filon incluses
- Descriptions complètes
- Icônes colorées par type de minéral

---

### 6. ? **Aide**

**À quoi ça sert ?**
- Afficher l'aide rapide des outils
- Rappel des fonctionnalités
- Astuces d'utilisation

**Contenu de l'aide :**
- Description de chaque outil
- Mode d'emploi rapide
- Astuce : "Maintenez Ctrl + Glisser pour pivoter la carte !"

---

## ?? **APPARENCE DU PANNEAU**

### Couleurs des boutons :
- ?? **Bleu** : Mesurer Distance
- ?? **Violet** : Import Photos GPS
- ?? **Orange** : Filons Proches
- ?? **Vert** : Tracer Zone
- ?? **Turquoise** : Export KMZ
- ? **Gris** : Aide

### État actif :
Quand un outil est actif (ex: mode mesure), le bouton devient **rouge** avec l'icône ??

---

## ?? **ASTUCES D'UTILISATION**

### ?? Workflow recommandé

#### Pour une sortie terrain :
1. Ouvrez **?? Filons Proches** avec rayon 5 km
2. Notez les filons intéressants
3. Utilisez **?? Itinéraire** (bouton carte) pour y aller
4. Sur place, prenez des photos avec GPS activé
5. De retour, utilisez **?? Import Photos GPS**

#### Pour mesurer des distances :
1. **?? Mesurer Distance** entre 2 filons
2. Utilisez la direction donnée pour planifier l'accès
3. Les temps estimés vous aident à préparer l'équipement

#### Pour organiser vos données :
1. **?? Import Photos GPS** après chaque sortie
2. Vérifiez l'association automatique
3. Ajustez manuellement si nécessaire

---

## ?? **FONCTIONNALITÉS TECHNIQUES**

### Services utilisés :

#### MeasurementService
```csharp
- CalculateDistance() : Formule Haversine
- CalculateBearing() : Cap/azimut
- FindFilonsInRadius() : Recherche spatiale
- EstimateTravelTime() : Temps trajet
```

#### PhotoGeotagService
```csharp
- ExtractGPS() : Lecture EXIF
- ImportPhotosFromFolderAsync() : Import batch
- AutoMatchPhotosToFilons() : Association intelligente
```

---

## ?? **STATISTIQUES D'UTILISATION**

Le panneau d'outils vous permet de :
- ? Mesurer des distances en 2 clics
- ? Importer 100+ photos en quelques secondes
- ? Trouver des filons dans un rayon de 0.1 à 50 km
- ?? Tracer des zones personnalisées (bientôt)
- ?? Exporter en KMZ avec photos (bientôt)

---

## ?? **AIDE ET SUPPORT**

### Problèmes fréquents

**"Mode mesure ne se désactive pas"**
- Cliquez sur le 2ème point de la carte
- OU recliquez sur le bouton ??

**"Photos non associées"**
- Vérifiez que les photos ont des données GPS (EXIF)
- Réduisez le rayon de recherche (< 500m recommandé)
- Vérifiez que des filons existent dans la zone

**"Aucun filon proche trouvé"**
- Augmentez le rayon de recherche
- Vérifiez que vous êtes dans la région du Var
- Créez d'abord des filons !

---

## ?? **PROCHAINES MISES À JOUR**

### Fonctionnalités en cours :
1. **?? Tracer Zone** - Polygones personnalisés
2. **??? Export KMZ** - Export avec photos
3. **?? QR Codes** - Partage rapide
4. **?? Alertes** - Notifications proximité

### Améliorations prévues :
- Mode mesure multi-points (chemin complet)
- Import photos par glisser-déposer
- Historique des mesures
- Export rapport PDF avec carte

---

## ? **CHECKLIST RAPIDE**

### Avant une sortie terrain :
- [ ] Vérifier les filons proches (rayon 5-10 km)
- [ ] Noter les coordonnées GPS
- [ ] Mesurer les distances d'accès
- [ ] Activer GPS sur appareil photo

### Après une sortie terrain :
- [ ] Importer les photos GPS
- [ ] Vérifier associations automatiques
- [ ] Ajouter notes si nécessaire
- [ ] Sauvegarder (auto-save actif)

---

**Date de création :** 09/01/2025  
**Version :** 1.0  
**Auteur :** WMine Development Team

**Pour plus d'aide, cliquez sur le bouton ? Aide dans le panneau !** ??
