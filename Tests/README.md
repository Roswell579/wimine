# Tests du Service de Routing

Ce dossier contient les tests pour le `RouteService`.

## ?? Fichiers

### `QuickRouteTest.cs`
Test rapide et simple pour valider le fonctionnement de base.

**Scénario testé :**
- Toulon ? Nice en voiture

**Durée :** ~3-5 secondes

**Usage :**
```csharp
await Tests.QuickRouteTest.Run();
```

---

### `RouteServiceTests.cs`
Suite compléte de tests couvrant différents scénarios.

**Scénarios testés :**
1. **Voiture** - Toulon ? Nice (~145 km)
2. **é pied** - Courte distance (~1 km)
3. **Vélo** - Marseille ? Aix (~28 km)
4. **Distance courte** - < 500m (vérif format métres)
5. **Longue distance** - Toulon ? Lyon (~350 km, vérif format heures)

**Durée :** ~20-30 secondes

**Usage :**
```csharp
await Tests.RouteServiceTests.RunAllTests();
```

---

## ?? Lancement des tests

### Depuis Program.cs

**Test rapide :**
```csharp
QuickTestRouting().Wait();
return;
```

**Tests complets :**
```csharp
TestRouteServiceConsole().Wait();
return;
```

### Depuis un autre fichier
```csharp
using wmine.Tests;

// Test rapide
await QuickRouteTest.Run();

// Tests complets
await RouteServiceTests.RunAllTests();

// Test depuis position actuelle
await RouteServiceTests.TestFromCurrentLocation();
```

---

## ? Résultats attendus

Tous les tests devraient afficher `?` pour :
- Distance calculée
- Durée calculée
- Points GPS extraits
- Instructions de navigation

---

## ?? En cas d'échec

### Erreur réseau
?? Vérifier la connexion Internet

### Timeout
?? L'API OSRM peut étre temporairement lente

### "No route found"
?? Vérifier que les coordonnées sont valides

---

## ?? Couverture

Les tests couvrent :
- ? Les 3 modes de transport
- ? Différentes distances (courte, moyenne, longue)
- ? Formatage automatique (m/km, min/h)
- ? Gestion des erreurs
- ? Extraction des points GPS
- ? Instructions de navigation
