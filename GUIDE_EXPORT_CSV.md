# ?? Export CSV - Guide d'utilisation

## ? C'EST PRéT !

L'export CSV est maintenant disponible via un **script PowerShell autonome**.

---

## ?? Utilisation (3 clics)

### **Méthode 1 : Double-clic** (Recommandé)

1. **Ouvrez** l'explorateur de fichiers
2. **Allez** dans le dossier du projet : `C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine\`
3. **Double-cliquez** sur `export-to-csv.ps1`
4. Suivez les instructions é l'écran
5. **C'est tout !** Le fichier CSV est créé sur votre bureau ??

---

### **Méthode 2 : Ligne de commande**

```powershell
# Depuis le dossier du projet
cd "C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine"
.\export-to-csv.ps1
```

Ou depuis n'importe oé :

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine\export-to-csv.ps1"
```

---

## ?? Ce que fait le script

1. ? Lit `filons.json` depuis `%LOCALAPPDATA%\WMine\`
2. ? Convertit tous les filons en format CSV
3. ? Crée le fichier sur votre **Bureau** avec horodatage
4. ? Gére les accents franéais (encodage UTF-8)
5. ? Propose d'ouvrir le fichier automatiquement

---

## ?? Format CSV généré

Le fichier CSV contient **12 colonnes** :

| Colonne | Exemple |
|---------|---------|
| Nom | Mine du Cap Garonne |
| Minéral Principal | Cuivre |
| Latitude | 43.123456 |
| Longitude | 6.234567 |
| Lambert X | 971045.00 |
| Lambert Y | 3144260.00 |
| Statut | Exploité \| Actif |
| Notes | Description détaillée... |
| Date Création | 2025-11-07 12:34:56 |
| Date Modification | 2025-11-07 14:22:10 |
| Chemin Photos | C:\Photos\Cap |
| Chemin Documentation | C:\Docs\cap.pdf |

---

## ?? Exemples d'utilisation

### **Ouvrir dans Excel**

1. Double-cliquez sur le fichier `.csv` généré
2. Excel s'ouvre automatiquement
3. Toutes vos données sont lé ! ?

### **Importer dans Google Sheets**

1. Allez sur Google Sheets
2. Fichier ? Importer
3. Sélectionnez votre fichier CSV
4. Cliquez sur "Importer"

### **Analyser avec Python/R**

```python
import pandas as pd

# Lire le CSV
df = pd.read_csv('Filons_Export_20251107_123456.csv')

# Analyser les données
print(df.head())
print(df.describe())
```

---

## ?? Nom du fichier

Le fichier est créé avec un **horodatage automatique** :

```
Filons_Export_20251107_143022.csv
                ?        ?
            Date (AAAAMMJJ) Heure (HHMMSS)
```

**Exemple** : Export le 7 nov 2025 é 14h30:22  
? `Filons_Export_20251107_143022.csv`

Vous pouvez aussi **choisir un autre nom** pendant l'exécution du script.

---

## ?? Personnalisation

### Changer le dossier de destination

éditez `export-to-csv.ps1` ligne 54 :

```powershell
# Par défaut : Bureau
$desktopPath = [Environment]::GetFolderPath("Desktop")

# Pour exporter ailleurs :
$desktopPath = "C:\MesExports"
```

### Ajouter/Retirer des colonnes

éditez la section `[PSCustomObject]@{ ... }` (lignes 84-98) pour modifier les colonnes exportées.

---

## ?? En cas de probléme

### "Impossible d'exécuter car les scripts sont désactivés"

**Solution** :
```powershell
# Autoriser les scripts pour cette session
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
.\export-to-csv.ps1
```

Ou faites un clic droit ? **"Exécuter avec PowerShell"**

### "Fichier filons.json introuvable"

**Chemin attendu** :
```
C:\Users\Roswell\AppData\Local\WMine\filons.json
```

Vérifiez que vous avez bien des filons dans l'application.

### "Aucun filon trouvé"

L'application WMine n'a pas encore de filons enregistrés.  
Ajoutez quelques filons dans l'application d'abord.

---

## ?? Statistiques affichées

Le script affiche :
- ? **Nombre de filons exportés**
- ? **Taille du fichier CSV**
- ? **Chemin complet du fichier**

---

## ?? Avantages de cette solution

? **Aucune modification de l'interface** - Pas de risque de bugs  
? **Script autonome** - Fonctionne indépendamment de l'app  
? **Rapide** - Export instantané  
? **Flexible** - Personnalisable facilement  
? **Compatible** - Excel, Google Sheets, Python, R, etc.  
? **Encodage UTF-8** - Support parfait des accents franéais  
? **Horodatage** - Pas de risque d'écraser un ancien export  

---

## ?? Fréquence d'utilisation

**Quand utiliser ce script ?**

- ?? **Analyse de données** : Statistiques, graphiques
- ?? **Partage** : Envoyer la liste é quelqu'un
- ?? **Sauvegarde** : Backup externe de vos données
- ?? **Migration** : Vers un autre systéme
- ?? **Reporting** : Créer des rapports Excel

**Combien de fois ?**

- é la demande, quand vous en avez besoin
- Recommandé : 1 fois par semaine pour backup
- Le script est **instantané** (< 1 seconde)

---

## ?? Support

En cas de probléme :
1. Vérifiez que `filons.json` existe dans `%LOCALAPPDATA%\WMine\`
2. Relancez PowerShell en **mode administrateur**
3. Consultez les messages d'erreur affichés

---

## ? RéSUMé : Comment exporter en CSV

```
1. Double-clic sur export-to-csv.ps1
2. Appuyez sur ENTRéE
3. Votre fichier CSV est sur le bureau ! ??
```

**C'est aussi simple que éa !** ??

---

**Script créé le** : 07/11/2025  
**Compatible avec** : Windows 10/11 + PowerShell 5.1+  
**Testé avec** : WMine Localisateur (Post-OCR OK)
