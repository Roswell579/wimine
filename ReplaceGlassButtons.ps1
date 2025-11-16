# Script pour remplacer tous les TransparentGlassButton par Button standard
# Et enlever l'effet glass partout

$files = @(
    "Forms\FilonEditForm.Designer.cs",
    "Forms\GalleryForm.cs",
    "Forms\ImportPanel.cs",
    "Forms\PinDialog.cs",
    "Forms\SettingsPanel.cs"
)

foreach ($file in $files) {
    $path = Join-Path $PSScriptRoot $file
    if (Test-Path $path) {
        Write-Host "Processing: $file" -ForegroundColor Cyan
        
        $content = Get-Content $path -Raw
        
        # Remplacer la déclaration du type
        $content = $content -replace 'private TransparentGlassButton\s+', 'private Button '
        $content = $content -replace 'new TransparentGlassButton\s*\{', 'new Button {'
        $content = $content -replace 'new TransparentGlassButton\s*\(', 'new Button('
        $content = $content -replace 'var\s+(\w+)\s*=\s*new\s+TransparentGlassButton', 'var $1 = new Button'
        
        # Remplacer les propriétés spécifiques TransparentGlassButton par équivalent Button
        $content = $content -replace 'BaseColor\s*=\s*Color\.FromArgb\((\d+),\s*(\d+),\s*(\d+)\)', 'BackColor = Color.FromArgb($1, $2, $3), FlatStyle = FlatStyle.Flat'
        $content = $content -replace 'BaseColor\s*=\s*Color\.FromArgb\((\d+),\s*(\d+),\s*(\d+),\s*(\d+)\)', 'BackColor = Color.FromArgb($1, $2, $3, $4), FlatStyle = FlatStyle.Flat'
        
        # Supprimer la propriété Transparency (pas nécessaire pour Button)
        $content = $content -replace ',?\s*Transparency\s*=\s*\d+', ''
        
        Set-Content $path $content -Encoding UTF8
        Write-Host "  ? Updated" -ForegroundColor Green
    } else {
        Write-Host "  ? File not found: $path" -ForegroundColor Red
    }
}

Write-Host "`n? All files processed!" -ForegroundColor Green
Write-Host "Note: Vous devrez peut-être ajouter FlatAppearance.BorderSize = 0 manuellement" -ForegroundColor Yellow
