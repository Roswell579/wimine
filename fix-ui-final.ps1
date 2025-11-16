# SCRIPT FINAL - Corrections UI complètes

Write-Host "?? CORRECTIONS UI - VERSION FINALE" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Gray

$files = @(
    "Forms\ImportPanel.cs",
    "Forms\SettingsPanel.cs", 
    "Forms\ContactsPanel.cs",
    "Forms\TechniquesEditPanel.cs",
    "Forms\MineralsPanel.cs",
    "Forms\GalleryForm.cs",
    "Forms\RouteDialog.cs",
    "Forms\FilonEditForm.cs",
    "Forms\FilonEditForm.Designer.cs",
    "Form1.cs",
    "Form1.Designer.cs"
)

$totalChanges = 0

foreach ($file in $files) {
    if (!(Test-Path $file)) {
        Write-Host "??  $file - N'existe pas" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "`n?? $file" -ForegroundColor White
    
    $content = [System.IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)
    $originalContent = $content
    
    # 1. BOUTONS : ForeColor en BLANC
    $before = ([regex]::Matches($content, 'new Button.*?ForeColor = Color\.Black')).Count
    $content = $content -replace '(new Button[^}]*?)ForeColor = Color\.Black', '$1ForeColor = Color.White'
    $after = ([regex]::Matches($content, 'new Button.*?ForeColor = Color\.White')).Count
    if ($before -gt 0) {
        Write-Host "  ? Boutons ForeColor: $before ? Color.White" -ForegroundColor Green
        $totalChanges += $before
    }
    
    # 2. BOUTONS : Ajouter FlatStyle si manquant
    if ($content -match 'new Button\s*\{[^}]*?BackColor[^}]*?\}' -and 
        $content -notmatch 'FlatStyle = FlatStyle\.Flat') {
        # Compter combien de boutons n'ont pas FlatStyle
        $matches = [regex]::Matches($content, 'new Button\s*\{[^}]*?BackColor[^}]*?\}')
        foreach ($match in $matches) {
            if ($match.Value -notmatch 'FlatStyle') {
                $oldBlock = $match.Value
                $newBlock = $oldBlock -replace '(\};)', ', FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand$1'
                $content = $content.Replace($oldBlock, $newBlock)
            }
        }
    }
    
    # 3. BOUTONS : BorderSize = 0 après déclaration
    $lines = $content -split "`n"
    $newLines = @()
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $newLines += $lines[$i]
        # Si ligne contient btn<Name>.Click et ligne suivante ne contient pas BorderSize
        if ($lines[$i] -match '(btn\w+)\.Click \+=' -and 
            $i + 1 -lt $lines.Count -and 
            $lines[$i + 1] -notmatch 'FlatAppearance\.BorderSize') {
            $btnName = $matches[1]
            $indent = '            '
            if ($lines[$i] -match '^(\s+)') { $indent = $matches[1] }
            $newLines += "$indent$btnName.FlatAppearance.BorderSize = 0;"
        }
    }
    $content = $newLines -join "`n"
    
    # 4. LABELS/DESCRIPTIFS : ForeColor en BLANC (sauf boutons)
    # Seulement si c'est un Label ET ForeColor existe ET n'est pas déjà White
    $labelsBefore = ([regex]::Matches($content, 'new Label[^}]*?ForeColor = Color\.(Black|Gray|LightGray)')).Count
    $content = $content -replace '(new Label[^}]*?)ForeColor = Color\.(Black|Gray|LightGray)', '$1ForeColor = Color.White'
    if ($labelsBefore -gt 0) {
        Write-Host "  ? Labels ForeColor: $labelsBefore ? Color.White" -ForegroundColor Green
        $totalChanges += $labelsBefore
    }
    
    # 5. MENUS CONTEXTUELS : ForeColor en BLANC
    $menusBefore = ([regex]::Matches($content, 'contextMenu\.ForeColor = Color\.Black')).Count
    $content = $content -replace 'contextMenu\.ForeColor = Color\.Black', 'contextMenu.ForeColor = Color.White'
    if ($menusBefore -gt 0) {
        Write-Host "  ? Menus ForeColor: $menusBefore ? Color.White" -ForegroundColor Green
        $totalChanges += $menusBefore
    }
    
    # 6. ONGLETS : Enlever bordures
    $content = $content -replace 'e\.Graphics\.DrawRectangle\([^)]+\bselectedTabRect\b[^)]*\);', '// Bordure enlevée'
    $content = $content -replace 'e\.Graphics\.DrawRectangle\([^)]+\btabRect\b[^)]*\);', '// Bordure enlevée'
    
    # Sauvegarder si changements
    if ($content -ne $originalContent) {
        [System.IO.File]::WriteAllText($file, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  ?? Sauvegardé" -ForegroundColor Cyan
    } else {
        Write-Host "  ? Aucun changement" -ForegroundColor Gray
    }
}

Write-Host "`n" + ("=" * 60) -ForegroundColor Gray
Write-Host "? TOTAL: $totalChanges corrections appliquées" -ForegroundColor Green
Write-Host "`n?? Compilation..." -ForegroundColor Cyan

dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n?? BUILD RÉUSSIE!" -ForegroundColor Green
    Write-Host "? Toutes les corrections sont appliquées" -ForegroundColor Green
} else {
    Write-Host "`n? Erreurs de build (voir ci-dessus)" -ForegroundColor Red
}
