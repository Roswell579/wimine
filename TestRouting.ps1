# Script de test du service de routing
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "    TEST DU SERVICE DE ROUTING" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $projectDir

# Sauvegarder le Program.cs original
Write-Host "?? Sauvegarde du Program.cs..." -ForegroundColor Gray
Copy-Item "Program.cs" "Program.cs.backup" -Force

try {
    # Activer les tests console
    Write-Host "?? Activation des tests console..." -ForegroundColor Gray
    $content = Get-Content "Program.cs" -Raw
    $content = $content -replace '// TestRouteServiceConsole\(\)\.Wait\(\);', 'TestRouteServiceConsole().Wait();'
    $content = $content -replace '// return;(\s+// To customize)', 'return;$1'
    $content | Set-Content "Program.cs"

    Write-Host ""
    Write-Host "?? Lancement des tests..." -ForegroundColor Green
    Write-Host "???????????????????????????????????????" -ForegroundColor Gray
    Write-Host ""

    # Lancer les tests
    dotnet run --project wmine.csproj

    Write-Host ""
    Write-Host "???????????????????????????????????????" -ForegroundColor Gray
}
finally {
    # Restaurer le Program.cs original
    Write-Host ""
    Write-Host "??  Restauration du Program.cs..." -ForegroundColor Gray
    Move-Item "Program.cs.backup" "Program.cs" -Force
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "    Tests terminés ?" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Appuyez sur une touche pour continuer..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
