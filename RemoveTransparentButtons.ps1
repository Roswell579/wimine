# Script pour remplacer TransparentGlassButton par Button standard
# Date: Janvier 2025

Write-Host "?? Remplacement de TransparentGlassButton par Button standard..." -ForegroundColor Cyan

$files = @(
    "Forms\FilonEditForm.Designer.cs",
    "Forms\ImportPanel.cs",
    "Forms\SettingsPanel.cs",
    "Forms\ContactsPanel.cs",
    "Forms\TechniquesEditPanel.cs"
)

$replacements = 0

foreach ($file in $files) {
    $fullPath = Join-Path $PSScriptRoot $file
    
    if (Test-Path $fullPath) {
        Write-Host "?? Traitement de $file..." -ForegroundColor Yellow
        
        $content = Get-Content $fullPath -Raw
        $originalContent = $content
        
        # Remplacer TransparentGlassButton par Button
        $content = $content -replace 'new TransparentGlassButton', 'new Button'
        $content = $content -replace 'TransparentGlassButton\s+(\w+)\s*=', 'Button $1 ='
        $content = $content -replace 'private\s+TransparentGlassButton', 'private Button'
        
        # Remplacer les propriétés TransparentGlassButton par propriétés Button standard
        $content = $content -replace 'BaseColor\s*=\s*Color\.FromArgb\(([^)]+)\)', 'BackColor = Color.FromArgb($1)'
        $content = $content -replace 'Transparency\s*=\s*\d+,?\s*\n', ''
        
        # Ajouter FlatStyle si nécessaire
        $content = $content -replace '(new Button\s*\{[^}]*?)(\})', '$1    FlatStyle = FlatStyle.Flat,$2'
        
        if ($content -ne $originalContent) {
            Set-Content $fullPath $content -NoNewline
            $replacements++
            Write-Host "? $file modifié" -ForegroundColor Green
        } else {
            Write-Host "??  $file - aucun changement nécessaire" -ForegroundColor Gray
        }
    } else {
        Write-Host "? $file introuvable" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "? Remplacement terminé!" -ForegroundColor Green
Write-Host "?? $replacements fichier(s) modifié(s)" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Compilation..." -ForegroundColor Yellow
dotnet build

Write-Host ""
Write-Host "? Terminé! Les boutons transparents ont été remplacés par des boutons standards." -ForegroundColor Green
