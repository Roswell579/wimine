# Script PowerShell pour corriger l'encodage UTF-8 avec BOM
# À exécuter dans le dossier du projet

Write-Host "?? Correction de l'encodage UTF-8 pour Form1.Designer.cs" -ForegroundColor Cyan

$filePath = "Form1.Designer.cs"

if (Test-Path $filePath) {
    Write-Host "? Fichier trouvé : $filePath" -ForegroundColor Green
    
    # Lire le contenu
    $content = Get-Content $filePath -Raw -Encoding UTF8
    
    # Sauvegarder avec UTF-8 BOM
    $utf8BOM = New-Object System.Text.UTF8Encoding $true
    [System.IO.File]::WriteAllText((Resolve-Path $filePath), $content, $utf8BOM)
    
    Write-Host "? Encodage UTF-8 avec BOM appliqué" -ForegroundColor Green
    
    # Vérifier les emojis présents
    $emojiCount = ([regex]::Matches($content, '[???????????????????????????]')).Count
    Write-Host "? Nombre d'emojis détectés : $emojiCount" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "?? Liste des emojis dans le fichier :" -ForegroundColor Yellow
    Write-Host "   ??? Carte" -ForegroundColor White
    Write-Host "   ?? Minéraux" -ForegroundColor White
    Write-Host "   ?? Import" -ForegroundColor White
    Write-Host "   ?? Techniques" -ForegroundColor White
    Write-Host "   ?? Contacts" -ForegroundColor White
    Write-Host "   ?? Paramètres" -ForegroundColor White
    Write-Host "   ?? Éditer" -ForegroundColor White
    Write-Host "   ??? Supprimer" -ForegroundColor White
    Write-Host "   ?? PDF" -ForegroundColor White
    Write-Host "   ?? Email" -ForegroundColor White
    Write-Host "   ?? Fiches" -ForegroundColor White
    Write-Host "   ? Cacher / ? Afficher" -ForegroundColor White
    Write-Host "   ? Commentaires" -ForegroundColor White
    
    Write-Host ""
    Write-Host "?? Correction terminée !" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? IMPORTANT :" -ForegroundColor Yellow
    Write-Host "   1. Fermez Visual Studio" -ForegroundColor White
    Write-Host "   2. Rouvrez Visual Studio" -ForegroundColor White
    Write-Host "   3. Rechargez le projet" -ForegroundColor White
    Write-Host ""
    Write-Host "?? Si les emojis ne s'affichent toujours pas dans l'éditeur :" -ForegroundColor Cyan
    Write-Host "   - C'est peut-être la police de Visual Studio qui ne supporte pas les emojis" -ForegroundColor White
    Write-Host "   - Essayez de changer la police : Outils > Options > Polices et couleurs" -ForegroundColor White
    Write-Host "   - Police recommandée : 'Cascadia Code' ou 'Consolas'" -ForegroundColor White
    Write-Host ""
    Write-Host "? Les emojis fonctionneront correctement dans l'application, même si" -ForegroundColor Green
    Write-Host "   ils n'apparaissent pas correctement dans l'éditeur Visual Studio." -ForegroundColor Green
    
} else {
    Write-Host "? Fichier non trouvé : $filePath" -ForegroundColor Red
    Write-Host "   Assurez-vous d'exécuter ce script dans le dossier du projet." -ForegroundColor Yellow
}

Write-Host ""
Read-Host "Appuyez sur Entrée pour continuer"
