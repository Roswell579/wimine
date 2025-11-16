# Script automatique pour mettre TOUS les textes de boutons en NOIR

$projectPath = "C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine"
$files = @(
    "$projectPath\Forms\SettingsPanel.cs",
    "$projectPath\Forms\RouteDialog.cs",
    "$projectPath\Forms\ImportPanel.cs",
    "$projectPath\Forms\FilonEditForm.cs",
    "$projectPath\Forms\FilonEditForm.Designer.cs",
    "$projectPath\Form1.cs"
)

$count = 0

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "?? Traitement: $([System.IO.Path]::GetFileName($file))" -ForegroundColor Cyan
        
        # Lire le contenu
        $content = Get-Content $file -Raw -Encoding UTF8
        
        # Compter les remplacements
        $matches = [regex]::Matches($content, 'ForeColor = Color\.White')
        $fileCount = $matches.Count
        
        if ($fileCount -gt 0) {
            # Remplacer ForeColor = Color.White par ForeColor = Color.Black
            $content = $content -replace 'ForeColor = Color\.White', 'ForeColor = Color.Black'
            
            # Sauvegarder
            Set-Content $file -Value $content -Encoding UTF8 -NoNewline
            
            Write-Host "   ? $fileCount remplacement(s)" -ForegroundColor Green
            $count += $fileCount
        } else {
            Write-Host "   ? Aucun remplacement nécessaire" -ForegroundColor Gray
        }
    } else {
        Write-Host "   ??  Fichier introuvable" -ForegroundColor Yellow
    }
}

Write-Host "`n?? TERMINÉ! $count texte(s) de bouton(s) passé(s) en NOIR" -ForegroundColor Green
Write-Host "`n?? Compilation..." -ForegroundColor Cyan

cd $projectPath
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n? BUILD RÉUSSIE !" -ForegroundColor Green
} else {
    Write-Host "`n? Erreurs de build (vérifiez les détails ci-dessus)" -ForegroundColor Red
}
