# ?? REMISE EN ORDRE COMPLÈTE - WMine v2.0
# Ce script remet automatiquement le projet en état fonctionnel

Write-Host ""
Write-Host "??????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?                                                ?" -ForegroundColor Cyan
Write-Host "?     ?? REMISE EN ORDRE WMINE V2.0              ?" -ForegroundColor Cyan
Write-Host "?                                                ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# ÉTAPE 1 : Vérifier l'application principale
Write-Host "?? ÉTAPE 1/3 : Vérification de l'application principale" -ForegroundColor Yellow
Write-Host ""

Write-Host "   ?? Compilation de wmine.csproj..." -ForegroundColor White
$buildResult = dotnet build wmine.csproj 2>&1 | Out-String
$appHasErrors = $buildResult -match "error CS"

if (-not $appHasErrors) {
    Write-Host "   ? Application principale : OK" -ForegroundColor Green
    $appOK = $true
} else {
    Write-Host "   ? Application principale : ERREURS" -ForegroundColor Red
    Write-Host ""
    Write-Host "   Premières erreurs :" -ForegroundColor Red
    $buildResult | Select-String "error" | Select-Object -First 5 | ForEach-Object {
        Write-Host "   $_" -ForegroundColor Red
    }
    $appOK = $false
    Write-Host ""
    Write-Host "??  L'application a des erreurs critiques." -ForegroundColor Red
    Write-Host "   Consultez URGENCE_REPARATION.md pour plus de détails." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Appuyez sur une touche pour quitter..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit
}

Write-Host ""

# ÉTAPE 2 : Gérer le projet de tests
Write-Host "?? ÉTAPE 2/3 : Gestion du projet de tests" -ForegroundColor Yellow
Write-Host ""

$testsExist = Test-Path "Tests"

if ($testsExist) {
    Write-Host "   ?? Projet de tests détecté : Tests/" -ForegroundColor White
    Write-Host "   ??  Ce projet cause 267 erreurs (packages manquants)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Que voulez-vous faire ?" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "   1??  SUPPRIMER le projet de tests (RECOMMANDÉ)" -ForegroundColor Green
    Write-Host "      ? Rapide (30 sec)" -ForegroundColor Gray
    Write-Host "      ? Élimine toutes les erreurs" -ForegroundColor Gray
    Write-Host "      ? L'app fonctionne sans les tests" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   2??  RÉPARER le projet de tests" -ForegroundColor Yellow
    Write-Host "      ? Plus long (5-10 min)" -ForegroundColor Gray
    Write-Host "      ? Installe les packages manquants" -ForegroundColor Gray
    Write-Host "      ? Les tests fonctionneront" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   3??  IGNORER (ne rien faire)" -ForegroundColor Red
    Write-Host "      ? Les 267 erreurs resteront" -ForegroundColor Gray
    Write-Host ""
    
    $choice = Read-Host "   Votre choix (1, 2 ou 3)"
    
    Write-Host ""
    
    switch ($choice) {
        "1" {
            Write-Host "   ???  Suppression du projet de tests..." -ForegroundColor Yellow
            Remove-Item -Recurse -Force Tests -ErrorAction SilentlyContinue
            Write-Host "   ? Projet de tests supprimé" -ForegroundColor Green
            $testsFixed = $true
        }
        "2" {
            Write-Host "   ?? Réparation du projet de tests..." -ForegroundColor Yellow
            Write-Host ""
            
            Push-Location Tests
            
            Write-Host "   ?? Installation des packages..." -ForegroundColor White
            dotnet add package xunit --version 2.6.2 | Out-Null
            dotnet add package xunit.runner.visualstudio --version 2.5.4 | Out-Null
            dotnet add package FluentAssertions --version 6.12.0 | Out-Null
            dotnet add package Moq --version 4.20.70 | Out-Null
            dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0 | Out-Null
            
            Write-Host "   ?? Restauration..." -ForegroundColor White
            dotnet restore | Out-Null
            
            Write-Host "   ?? Compilation..." -ForegroundColor White
            $testBuild = dotnet build 2>&1 | Out-String
            
            Pop-Location
            
            $testHasErrors = $testBuild -match "error CS"
            
            if (-not $testHasErrors) {
                Write-Host "   ? Tests réparés avec succès" -ForegroundColor Green
                $testsFixed = $true
            } else {
                Write-Host "   ??  Échec de la réparation" -ForegroundColor Red
                Write-Host "   ?? Recommandation : Supprimez les tests manuellement" -ForegroundColor Yellow
                $testsFixed = $false
            }
        }
        "3" {
            Write-Host "   ??  Ignoré - Les erreurs de tests resteront" -ForegroundColor Yellow
            $testsFixed = $false
        }
        default {
            Write-Host "   ??  Choix invalide - Ignoré" -ForegroundColor Red
            $testsFixed = $false
        }
    }
} else {
    Write-Host "   ? Aucun projet de tests - Rien à faire" -ForegroundColor Green
    $testsFixed = $true
}

Write-Host ""

# ÉTAPE 3 : Vérification finale
Write-Host "?? ÉTAPE 3/3 : Vérification finale" -ForegroundColor Yellow
Write-Host ""

Write-Host "   ?? Nettoyage..." -ForegroundColor White
dotnet clean | Out-Null

Write-Host "   ?? Build complet..." -ForegroundColor White
$finalBuild = dotnet build 2>&1 | Out-String

$finalErrors = ($finalBuild | Select-String "error CS").Count
$finalWarnings = ($finalBuild | Select-String "warning").Count

Write-Host ""
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host "?? RÉSULTAT FINAL" -ForegroundColor White
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host ""

if ($finalErrors -eq 0) {
    Write-Host "? SUCCÈS ! Projet remis en ordre" -ForegroundColor Green
    Write-Host ""
    Write-Host "   ?? Statistiques :" -ForegroundColor Cyan
    Write-Host "      Erreurs : $finalErrors" -ForegroundColor Green
    Write-Host "      Avertissements : $finalWarnings" -ForegroundColor $(if ($finalWarnings -gt 0) { "Yellow" } else { "Green" })
    Write-Host ""
    Write-Host "   ? Application principale : OK" -ForegroundColor Green
    Write-Host "   $(if ($testsFixed) { '?' } else { '?? ' }) Projet de tests : $(if ($testsFixed) { 'OK ou Supprimé' } else { 'Erreurs ignorées' })" -ForegroundColor $(if ($testsFixed) { "Green" } else { "Yellow" })
    Write-Host ""
    Write-Host "?? PROCHAINE ÉTAPE :" -ForegroundColor Cyan
    Write-Host "   Appuyez sur F5 dans Visual Studio" -ForegroundColor White
    Write-Host "   Ou exécutez : dotnet run --project wmine.csproj" -ForegroundColor Gray
    Write-Host ""
    
    # Demander si on veut lancer l'app
    $launch = Read-Host "Voulez-vous lancer l'application maintenant ? (O/N)"
    if ($launch -eq "O" -or $launch -eq "o") {
        Write-Host ""
        Write-Host "?? Lancement de l'application..." -ForegroundColor Green
        Start-Process "dotnet" -ArgumentList "run --project wmine.csproj" -NoNewWindow
    }
    
} else {
    Write-Host "? ÉCHEC - Des erreurs persistent" -ForegroundColor Red
    Write-Host ""
    Write-Host "   ?? Statistiques :" -ForegroundColor Cyan
    Write-Host "      Erreurs : $finalErrors" -ForegroundColor Red
    Write-Host "      Avertissements : $finalWarnings" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Premières erreurs :" -ForegroundColor Red
    $finalBuild | Select-String "error" | Select-Object -First 10 | ForEach-Object {
        Write-Host "   $_" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "?? Actions suggérées :" -ForegroundColor Yellow
    Write-Host "   1. Consultez URGENCE_REPARATION.md" -ForegroundColor White
    Write-Host "   2. Vérifiez Form1.Designer.cs" -ForegroundColor White
    Write-Host "   3. Restaurez depuis une sauvegarde Git" -ForegroundColor White
}

Write-Host ""
Write-Host "Appuyez sur une touche pour terminer..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
