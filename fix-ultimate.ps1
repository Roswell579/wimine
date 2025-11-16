# SCRIPT ULTIME DE CORRECTION - TOUS LES FICHIERS
# Corrige automatiquement TOUS les opérateurs man

quants

$projectPath = "C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine"
cd $projectPath

Write-Host "???????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  CORRECTION ULTIME - TOUS LES FICHIERS" -ForegroundColor White
Write-Host "???????????????????????????????????????????????" -ForegroundColor Cyan

# Liste des fichiers critiques avec erreurs connues
$criticalFixes = @{
    "MarkerVisualizationForm.cs" = @(
        @{ Pattern = '(hover)\s+(\d+\.?\d*f?)\s*:'; Replace = '$1 ? $2 :' }
    )
    "Services\ContactsDataService.cs" = @(
        @{ Pattern = '(json)\)\s+new\s'; Replace = '$1) ?? new ' }
        @{ Pattern = '(\.Any\(\))\s+(_contacts)'; Replace = '$1 ? $2' }
    )
    "Services\CoordinateConverter.cs" = @(
        @{ Pattern = '(isValid)\s+"'; Replace = '$1 ? "' }
    )
    "Services\EmailService.cs" = @(
        @{ Pattern = '(\.HasValue)\s+\$'; Replace = '$1 ? $' }
    )
    "Services\ExcelImportService.cs" = @(
        @{ Pattern = '(RowNumber\(\))\s+(\w+);'; Replace = '$1 ?? $2;' }
        @{ Pattern = '(IsNullOrWhiteSpace\([^)]+\))\s+\$'; Replace = '$1 ? $' }
    )
    "Services\FilonDataService.cs" = @(
        @{ Pattern = '(json)\)\s+new\s'; Replace = '$1) ?? new ' }
    )
    "Services\MapBoundsRestrictor.cs" = @(
        @{ Pattern = '(mapControl)\s+throw'; Replace = '$1 ?? throw' }
    )
    "Services\MineralDataService.cs" = @(
        @{ Pattern = 'MineralType\.Améthyste'; Replace = 'MineralType.Amethyste' }
        @{ Pattern = 'MineralType\.Disthène'; Replace = 'MineralType.Disthene' }
    )
}

$totalFixed = 0

foreach ($file in $criticalFixes.Keys) {
    $filePath = Join-Path $projectPath $file
    
    if (-not (Test-Path $filePath)) {
        Write-Host "? $file introuvable" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "Correction de $file..." -ForegroundColor Yellow
    $content = Get-Content $filePath -Raw -Encoding UTF8
    $changed = $false
    
    foreach ($fix in $criticalFixes[$file]) {
        if ($content -match $fix.Pattern) {
            $content = $content -replace $fix.Pattern, $fix.Replace
            $changed = $true
        }
    }
    
    if ($changed) {
        Set-Content $filePath $content -Encoding UTF8 -NoNewline
        Write-Host "  ? $file corrigé" -ForegroundColor Green
        $totalFixed++
    }
}

Write-Host ""
Write-Host "???????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  Fichiers corrigés: $totalFixed" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? COMPILATION..." -ForegroundColor Yellow
dotnet build wmine.csproj --nologo
