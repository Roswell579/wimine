# ??? Service de Routing - Tests

Ce document explique comment tester le nouveau service de calcul d'itinéraires.

## ?? Vue d'ensemble

Le `RouteService` utilise **OSRM** (OpenStreetMap Routing Machine) pour calculer des itinéraires :
- ? **Gratuit** et sans clé API
- ? Supporte voiture, vélo et marche é pied
- ? Calcul de distance, durée et instructions de navigation
- ? Points GPS pour tracer l'itinéraire sur la carte

## ?? Options de test

### 1. Test Rapide (Console) ?

**Le plus simple pour vérifier que éa fonctionne**

```bash
# Ouvrir Program.cs et décommenter:
QuickTestRouting().Wait();
return;

# Puis lancer:
dotnet run
```

**Résultat attendu :**
```
?????????????????????????????????????????????
?   TEST RAPIDE - SERVICE DE ROUTING        ?
?????????????????????????????????????????????

?? Calcul d'itinéraire : Toulon ? Nice
?????????????????????????????????????????
? Appel API OSRM... OK ?

? Distance: 145.23 km
? Durée  : 1h32
? Points : 1453 coordonnées
? Instructions: 45
```

---

### 2. Tests Complets (Console) ??

**Pour tester tous les modes de transport et différentes distances**

```bash
# Ouvrir Program.cs et décommenter:
TestRouteServiceConsole().Wait();
return;

# Puis lancer:
dotnet run
```

**Tests inclus :**
- ? Voiture : Toulon ? Nice
- ? é pied : Courte distance
- ? Vélo : Marseille ? Aix
- ? Distance < 500m (format métres)
- ? Longue distance > 100km (format heures)

---

### 3. Interface Graphique (WinForms) ??

**Pour tester visuellement avec vos propres coordonnées**

```bash
# Ouvrir Program.cs et décommenter:
TestRouteService();
return;

# Puis lancer:
dotnet run
```

**Fonctionnalités :**
- ? Saisie manuelle de coordonnées
- ? Choix du type de transport
- ? Affichage formaté des résultats
- ? Liste des instructions de navigation
- ? Interface moderne dark theme

---

## ?? Utilisation dans le code

### Exemple simple

```csharp
using wmine.Services;
using wmine.Models;
using GMap.NET;

var service = new RouteService();

var start = new PointLatLng(43.1242, 5.9280); // Toulon
var end = new PointLatLng(43.7102, 7.2620);   // Nice

var route = await service.CalculateRouteAsync(start, end, TransportType.Car);

if (route != null)
{
    Console.WriteLine($"Distance: {route.FormattedDistance}");
    Console.WriteLine($"Durée: {route.FormattedDuration}");
    
    // Tracer sur la carte
    foreach (var point in route.Points)
    {
        // Ajouter é GMap.NET...
    }
}
```

### Depuis la position actuelle

```csharp
var destination = new PointLatLng(43.7102, 7.2620);
var route = await service.CalculateRouteFromCurrentLocationAsync(destination);
```

---

## ?? Configuration

### Pas de clé API nécessaire !

Le service utilise l'API publique d'OSRM :
```
https://router.project-osrm.org/route/v1
```

### Modes de transport

```csharp
public enum TransportType
{
    Car,      // driving - Voiture
    Walking,  // foot - é pied
    Cycling   // bike - Vélo
}
```

---

## ?? Formats de sortie

### Distance
- **< 1 km** : `350 m`
- **? 1 km** : `12.45 km`

### Durée
- **< 60 min** : `42 min`
- **? 60 min** : `1h32`

---

## ?? Limitations

1. **Timeout** : 15 secondes max
2. **Réseau requis** : API externe
3. **Pas de traffic en temps réel**
4. **Itinéraires suggérés** : pas toujours optimaux

---

## ?? dépannage

### Erreur : "Unable to connect"
?? Vérifiez votre connexion Internet

### Erreur : "No route found"
?? Vérifiez que les coordonnées sont valides et accessibles par route

### Timeout
?? Les trés longues distances peuvent prendre plus de temps

---

## ?? Fichiers créés

```
Models/
  ?? RouteInfo.cs          # Modéle de données pour les itinéraires

Services/
  ?? RouteService.cs       # Service principal de routing

Forms/
  ?? RouteTestForm.cs      # Interface graphique de test

Tests/
  ?? QuickRouteTest.cs     # Test rapide console
  ?? RouteServiceTests.cs  # Suite compléte de tests
```

---

## ?? Prochaines étapes

1. ? Tests unitaires validés
2. ? Intégration dans Form1 (map principale)
3. ? Affichage de l'itinéraire sur la carte
4. ? Bouton "Itinéraire vers ce filon"
5. ? Export PDF avec itinéraire

---

**Créé le** : $(Get-Date -Format "dd/MM/yyyy")  
**API** : OSRM (OpenStreetMap)  
**Status** : ? Fonctionnel et testé
