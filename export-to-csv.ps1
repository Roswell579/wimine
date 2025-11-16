# Script d'export des filons en CSV
# Exécuter avec : powershell -ExecutionPolicy Bypass -File export-to-csv.ps1

Write-Host "?? Export des filons en CSV" -ForegroundColor Cyan
Write-Host "?????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host ""

# Chemin du fichier JSON
$jsonPath = Join-Path $env:LOCALAPPDATA "WMine\filons.json"

if (-not (Test-Path $jsonPath)) {
    Write-Host "? Fichier filons.json introuvable !" -ForegroundColor Red
    Write-Host "?? Chemin recherché : $jsonPath" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Appuyez sur une touche pour quitter..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Write-Host "? Fichier JSON trouvé" -ForegroundColor Green
Write-Host "?? $jsonPath" -ForegroundColor Gray
Write-Host ""

# Lire et parser le JSON
try {
    $jsonContent = Get-Content -Path $jsonPath -Raw -Encoding UTF8
    $filons = $jsonContent | ConvertFrom-Json
    
    if ($null -eq $filons -or $filons.Count -eq 0) {
        Write-Host "??  Aucun filon trouvé dans le fichier JSON" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Appuyez sur une touche pour quitter..." -ForegroundColor Gray
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        exit 0
    }
    
    Write-Host "?? Nombre de filons : $($filons.Count)" -ForegroundColor Cyan
    Write-Host ""
}
catch {
    Write-Host "? Erreur lors de la lecture du fichier JSON : $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Appuyez sur une touche pour quitter..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# Demander le nom du fichier CSV de sortie
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$defaultName = "Filons_Export_$timestamp.csv"
$desktopPath = [Environment]::GetFolderPath("Desktop")
$outputPath = Join-Path $desktopPath $defaultName

Write-Host "?? Fichier de sortie :" -ForegroundColor Cyan
Write-Host "   $outputPath" -ForegroundColor White
Write-Host ""

$response = Read-Host "Appuyez sur ENTRÉE pour utiliser ce nom, ou tapez un autre chemin"
if (-not [string]::IsNullOrWhiteSpace($response)) {
    $outputPath = $response
}

# Créer le CSV
try {
    Write-Host ""
    Write-Host "? Génération du fichier CSV..." -ForegroundColor Yellow
    
    # Créer un tableau d'objets pour l'export
    $csvData = @()
    
    foreach ($filon in $filons) {
        # Gérer le statut (combinaison de flags)
        $statuts = @()
        if ($filon.Statut) {
            $statusValue = [int]$filon.Statut
            if ($statusValue -band 1) { $statuts += "Exploité" }
            if ($statusValue -band 2) { $statuts += "Épuisé" }
            if ($statusValue -band 4) { $statuts += "Abandonné" }
            if ($statusValue -band 8) { $statuts += "Actif" }
            if ($statusValue -band 16) { $statuts += "En Exploration" }
            if ($statusValue -band 32) { $statuts += "Fermé" }
            if ($statusValue -band 64) { $statuts += "Dangereux" }
        }
        $statutStr = if ($statuts.Count -gt 0) { $statuts -join " | " } else { "Aucun" }
        
        # Mapper le type de minéral
        $mineralMap = @{
            0 = "Fer"
            1 = "Cuivre"
            2 = "Plomb"
            3 = "Zinc"
            4 = "Or"
            5 = "Argent"
            6 = "Étain"
            7 = "Tungstène"
            8 = "Manganèse"
            9 = "Bauxite"
            10 = "Autre"
        }
        $mineralName = if ($mineralMap.ContainsKey([int]$filon.MatierePrincipale)) {
            $mineralMap[[int]$filon.MatierePrincipale]
        } else {
            "Inconnu"
        }
        
        # Créer l'objet pour cette ligne
        $csvData += [PSCustomObject]@{
            "Nom" = $filon.Nom ?? ""
            "Minéral Principal" = $mineralName
            "Latitude" = if ($filon.Latitude) { [math]::Round($filon.Latitude, 6) } else { "" }
            "Longitude" = if ($filon.Longitude) { [math]::Round($filon.Longitude, 6) } else { "" }
            "Lambert X" = if ($filon.LambertX) { [math]::Round($filon.LambertX, 2) } else { "" }
            "Lambert Y" = if ($filon.LambertY) { [math]::Round($filon.LambertY, 2) } else { "" }
            "Statut" = $statutStr
            "Notes" = ($filon.Notes ?? "") -replace "`r`n", " " -replace "`n", " " -replace "`r", " "
            "Date Création" = if ($filon.DateCreation) { 
                try { 
                    [DateTime]::Parse($filon.DateCreation).ToString("yyyy-MM-dd HH:mm:ss") 
                } catch { 
                    $filon.DateCreation 
                } 
            } else { "" }
            "Date Modification" = if ($filon.DateModification) { 
                try { 
                    [DateTime]::Parse($filon.DateModification).ToString("yyyy-MM-dd HH:mm:ss") 
                } catch { 
                    $filon.DateModification 
                } 
            } else { "" }
            "Chemin Photos" = $filon.PhotoPath ?? ""
            "Chemin Documentation" = $filon.DocumentationPath ?? ""
        }
    }
    
    # Exporter en CSV avec encodage UTF-8 (support des accents)
    $csvData | Export-Csv -Path $outputPath -NoTypeInformation -Encoding UTF8
    
    Write-Host ""
    Write-Host "?????????????????????????????????????????" -ForegroundColor Green
    Write-Host "? Export réussi !" -ForegroundColor Green
    Write-Host "?????????????????????????????????????????" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Statistiques :" -ForegroundColor Cyan
    Write-Host "   • Filons exportés : $($filons.Count)" -ForegroundColor White
    Write-Host "   • Taille du fichier : $([math]::Round((Get-Item $outputPath).Length / 1KB, 2)) KB" -ForegroundColor White
    Write-Host ""
    Write-Host "?? Fichier créé :" -ForegroundColor Cyan
    Write-Host "   $outputPath" -ForegroundColor White
    Write-Host ""
    
    # Proposer d'ouvrir le fichier
    $openFile = Read-Host "Voulez-vous ouvrir le fichier CSV maintenant ? (O/N)"
    if ($openFile -eq "O" -or $openFile -eq "o") {
        Start-Process $outputPath
    }
    
    # Proposer d'ouvrir le dossier
    $openFolder = Read-Host "Voulez-vous ouvrir le dossier contenant le fichier ? (O/N)"
    if ($openFolder -eq "O" -or $openFolder -eq "o") {
        Start-Process (Split-Path $outputPath)
    }
}
catch {
    Write-Host ""
    Write-Host "? Erreur lors de la génération du CSV : $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Appuyez sur une touche pour quitter..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Write-Host ""
Write-Host "?? Export terminé avec succès !" -ForegroundColor Green
Write-Host ""
Write-Host "Appuyez sur une touche pour quitter..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
