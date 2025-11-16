# Script ULTRA SIMPLE - Texte noir sur tous les boutons
# Encodage UTF-8 garanti - Janvier 2025

Write-Host "?? Mise à jour de la couleur du texte des boutons..." -ForegroundColor Cyan

$files = Get-ChildItem -Path "Forms" -Filter "*.cs" -Recurse

$totalModified = 0

foreach ($file in $files) {
    try {
        # Lire avec UTF-8
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        
        # Simple remplacement
        $newContent = $content -replace 'ForeColor\s*=\s*Color\.White,', ''
        
        # Vérifier si changement
        if ($content -ne $newContent) {
            # Écrire avec UTF-8 + BOM
            $utf8WithBom = New-Object System.Text.UTF8Encoding $true
            [System.IO.File]::WriteAllText($file.FullName, $newContent, $utf8WithBom)
            
            $totalModified++
            Write-Host "? $($file.Name)" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "? Erreur sur $($file.Name): $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n? Terminé! $totalModified fichier(s) modifié(s)" -ForegroundColor Green
Write-Host "`n?? Compilation..." -ForegroundColor Yellow

dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n? SUCCÈS! Tous les textes des boutons sont en noir." -ForegroundColor Green
} else {
    Write-Host "`n?? Erreurs de compilation. Vérifiez les messages ci-dessus." -ForegroundColor Yellow
}
