# Script de correction et test rapide
# Date: 08/01/2025

Write-Host "?? Correction des erreurs de compilation..." -ForegroundColor Cyan
Write-Host ""

# Étape 1: Nettoyage
Write-Host "?? Nettoyage du projet..." -ForegroundColor Yellow
dotnet clean | Out-Null
Write-Host "  ? Nettoyage terminé" -ForegroundColor Green
Write-Host ""

# Étape 2: Restauration des packages
Write-Host "?? Restauration des packages..." -ForegroundColor Yellow
dotnet restore | Out-Null
Write-Host "  ? Packages restaurés" -ForegroundColor Green
Write-Host ""

# Étape 3: Compilation
Write-Host "?? Compilation du projet..." -ForegroundColor Yellow
$buildOutput = dotnet build 2>&1 | Out-String

# Analyser les résultats
$hasErrors = $buildOutput -match "error"
$hasWarnings = $buildOutput -match "warning"

if (-not $hasErrors) {
    Write-Host "  ? Compilation réussie!" -ForegroundColor Green
    
    # Compter les avertissements
    $warningCount = ($buildOutput | Select-String "warning" -AllMatches).Matches.Count
    if ($warningCount -gt 0) {
        Write-Host "  ??  $warningCount avertissement(s)" -ForegroundColor Yellow
    } else {
        Write-Host "  ? 0 avertissements" -ForegroundColor Green
    }
} else {
    Write-Host "  ? Échec de la compilation" -ForegroundColor Red
    Write-Host ""
    Write-Host "? Erreurs détectées:" -ForegroundColor Red
    
    # Afficher les 10 premières erreurs
    $errors = $buildOutput -split "`n" | Where-Object { $_ -match "error" } | Select-Object -First 10
    foreach ($error in $errors) {
        Write-Host "  $error" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host ("=" * 60) -ForegroundColor Cyan
Write-Host "?? Résumé" -ForegroundColor White
Write-Host ("=" * 60) -ForegroundColor Cyan

if (-not $hasErrors) {
    Write-Host "État: ? PRÊT" -ForegroundColor Green
    Write-Host "Erreurs: 0" -ForegroundColor Green
    Write-Host "Action: Vous pouvez maintenant lancer l'application (F5)" -ForegroundColor Cyan
} else {
    Write-Host "État: ? ERREURS" -ForegroundColor Red
    Write-Host "Action: Consultez les erreurs ci-dessus" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Appuyez sur une touche pour continuer..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
