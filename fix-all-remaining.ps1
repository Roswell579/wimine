# Script de correction FINALE - Tous les opérateurs manquants
# Corrige TOUS les fichiers restants

cd $PSScriptRoot

$files = @(
    "Forms\SettingsPanel.cs",
    "MarkerVisualizationForm.cs",
    "Models\TechniqueDocument.cs",
    "Services\ContactsDataService.cs",
    "Services\CoordinateConverter.cs",
    "Services\CsvExportService.cs",
    "Services\EmailService.cs",
    "Services\ExcelImportService.cs"
)

Write-Host "??????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  CORRECTION FINALE DE TOUS LES FICHIERS" -ForegroundColor White
Write-Host "??????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "Correction de $file..." -ForegroundColor Yellow
        
        $content = Get-Content $file -Raw -Encoding UTF8
        $changed = $false
        
        # RÈGLE 1: Remplacer "  " (deux espaces) par " ?? " avant quotes, new, throw, EventArgs
        if ($content -match '\w+\s\s"') {
            $content = $content -replace '(\w+)\s\s"', '$1 ?? "'
            $changed = $true
        }
        if ($content -match '\w+\s\snew\s') {
            $content = $content -replace '(\w+)\s\snew\s', '$1 ?? new '
            $changed = $true
        }
        if ($content -match '\w+\s\sthrow\s') {
            $content = $content -replace '(\w+)\s\sthrow\s', '$1 ?? throw '
            $changed = $true
        }
        if ($content -match '\w+\s\sEventArgs') {
            $content = $content -replace '(\w+)\s\sEventArgs', '$1 ?? EventArgs'
            $changed = $true
        }
        if ($content -match '\w+\s\s0\s*;') {
            $content = $content -replace '(\w+)\s\s0', '$1 ?? 0'
            $changed = $true
        }
        if ($content -match '\w+\s\s1\s*;') {
            $content = $content -replace '(\w+)\s\s1', '$1 ?? 1'
            $changed = $true
        }
        
        # RÈGLE 2: Remplacer condition  value : autre par condition ? value : autre
        if ($content -match '\w+\s\s[^:]+\s*:') {
            # Plus complexe - patterns spécifiques
            $content = $content -replace '(hover)\s\s(\d+\.?\d*f?)\s*:', '$1 ? $2 :'
            $content = $content -replace '(HasPdf)\s\s"\s*([^"]+)"\s*:', '$1 ? "$2" :'
            $content = $content -replace '(isValid)\s\s"([^"]+)"\s*:', '$1 ? "$2" :'
            $content = $content -replace '(\.Checked)\s\s"([^"]+)"\s*:', '$1 ? "$2" :'
            $content = $content -replace '(\.HasValue)\s\s\$', '$1 ? $'
            $content = $content -replace '(\.Any\(\))\s\s([^\s:]+)\s*:', '$1 ? $2 :'
            $changed = $true
        }
        
        if ($changed) {
            Set-Content $file $content -Encoding UTF8 -NoNewline
            Write-Host "  ? $file corrigé" -ForegroundColor Green
        } else {
            Write-Host "  ? $file déjà correct" -ForegroundColor Gray
        }
    } else {
        Write-Host "  ? $file introuvable" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "??????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  TERMINÉ !" -ForegroundColor Green
Write-Host "??????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "Exécutez: dotnet build" -ForegroundColor Yellow
