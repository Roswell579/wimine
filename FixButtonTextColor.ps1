# Script pour ajouter  à tous les boutons
# Version corrigée - Janvier 2025

Write-Host "?? Ajout de  à tous les boutons..." -ForegroundColor Cyan

$files = @(
    "Forms\FilonEditForm.Designer.cs",
    "Forms\ImportPanel.cs",
    "Forms\SettingsPanel.cs",
    "Forms\ContactsPanel.cs",
    "Forms\TechniquesEditPanel.cs",
    "Forms\MineralsPanel.cs"
)

$totalModified = 0

foreach ($file in $files) {
    $fullPath = Join-Path $PSScriptRoot $file
    
    if (Test-Path $fullPath) {
        Write-Host "`n?? Traitement de $file..." -ForegroundColor Yellow
        
        $content = Get-Content $fullPath -Raw -Encoding UTF8
        $originalContent = $content
        
        # Remplacer ForeColor = Color.White par  dans les Button
        $content = $content -replace '(new Button\s*\{[^}]*?)ForeColor\s*=\s*Color\.White,', '$1,'
        
        # Ajouter  si absent dans les Button
        $content = $content -replace '(new Button\s*\{[^}]*?BackColor\s*=\s*Color\.FromArgb\([^)]+\),)(\s+(?!ForeColor))', '$1`n                ,$2'
        
        # Ajouter FlatStyle si absent
        $content = $content -replace '(ForeColor\s*=\s*Color\.Black,)(\s+(?!FlatStyle)Font)', '$1`n                FlatStyle = FlatStyle.Flat,$2'
        
        # Ajouter Cursor si absent  
        $content = $content -replace '(Font\s*=\s*new\s+Font\([^)]+\))(\s+\})', '$1,`n                Cursor = Cursors.Hand$2'
        
        if ($content -ne $originalContent) {
            Set-Content $fullPath $content -NoNewline -Encoding UTF8
            $totalModified++
            Write-Host "? $file modifié" -ForegroundColor Green
        } else {
            Write-Host "??  $file - aucun changement nécessaire" -ForegroundColor Gray
        }
    } else {
        Write-Host "? $file introuvable" -ForegroundColor Red
    }
}

Write-Host "`n? Traitement terminé!" -ForegroundColor Green
Write-Host "?? $totalModified fichier(s) modifié(s)" -ForegroundColor Cyan

Write-Host "`n?? Compilation..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n? Compilation réussie! Tous les textes des boutons sont maintenant en noir." -ForegroundColor Green
} else {
    Write-Host "`n? Erreur de compilation. Restaurez avec: git restore Forms/" -ForegroundColor Red
}
