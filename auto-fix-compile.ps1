# SCRIPT DE RÉPARATION COMPLÈTE AUTOMATIQUE
# Corrige TOUS les opérateurs manquants dans TOUS les fichiers du projet

$ErrorActionPreference = "Stop"
$projectPath = "C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine"
cd $projectPath

Write-Host "????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?  RÉPARATION AUTOMATIQUE COMPLÈTE - WMINE    ?" -ForegroundColor White
Write-Host "????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Fonction de correction
function Fix-Operators {
    param([string]$FilePath)
    
    if (-not (Test-Path $FilePath)) { return $false }
    
    $content = Get-Content $FilePath -Raw -Encoding UTF8
    $original = $content
    
    # TOUS LES PATTERNS POSSIBLES
    # Pattern 1: variable  autre -> variable ?? autre
    $content = $content -replace '(\$?\w+)\s{2,}"', '$1 ?? "'
    $content = $content -replace '(\$?\w+)\s{2,}new\s', '$1 ?? new '
    $content = $content -replace '(\$?\w+)\s{2,}throw\s', '$1 ?? throw '
    $content = $content -replace '(\$?\w+)\s{2,}EventArgs', '$1 ?? EventArgs'
    $content = $content -replace '(\$?\w+)\s{2,}(\d+);', '$1 ?? $2;'
    $content = $content -replace '(json)\s{2,}(new\s)', '$1 ?? $2'
    
    # Pattern 2: ToString()  "" -> ToString() ?? ""
    $content = $content -replace '(ToString\([^)]*\))\s{2,}"', '$1 ?? "'
    $content = $content -replace '(\?\.\w+)\s{2,}"', '$1 ?? "'
    $content = $content -replace '(RowNumber\(\))\s{2,}(\w)', '$1 ?? $2'
    $content = $content -replace '(\.ErrorMessage)\s{2,}"', '$1 ?? "'
    
    # Pattern 3: condition  value : autre -> condition ? value : autre
    $content = $content -replace '(hover)\s{2,}(\d+\.?\d*f?)\s*:', '$1 ? $2 :'
    $content = $content -replace '(HasPdf)\s{2,}"([^"]*?)"\s*:', '$1 ? "$2" :'
    $content = $content -replace '(isValid)\s{2,}"([^"]*?)"\s*:', '$1 ? "$2" :'
    $content = $content -replace '(\.Checked)\s{2,}"([^"]*?)"\s*:', '$1 ? "$2" :'
    $content = $content -replace '(\.HasValue)\s{2,}\$', '$1 ? $'
    $content = $content -replace '(\.Any\(\))\s{2,}([^\s:]+)\s*:', '$1 ? $2 :'
    $content = $content -replace '(\?\.\w+)\s{2,}(\d)', '$1 ?? $2'
    
    if ($content -ne $original) {
        Set-Content $FilePath $content -Encoding UTF8 -NoNewline
        return $true
    }
    return $false
}

# Obtenir TOUS les fichiers .cs
$allFiles = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse | 
    Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }

Write-Host "?? Fichiers trouvés: $($allFiles.Count)" -ForegroundColor Yellow
Write-Host ""

$fixed = 0
$total = $allFiles.Count

foreach ($file in $allFiles) {
    $relativePath = $file.FullName.Replace($projectPath, "").TrimStart('\')
    Write-Progress -Activity "Correction en cours" -Status $relativePath -PercentComplete (($fixed / $total) * 100)
    
    if (Fix-Operators -FilePath $file.FullName) {
        Write-Host "? $relativePath" -ForegroundColor Green
        $fixed++
    }
}

Write-Progress -Activity "Correction en cours" -Completed

Write-Host ""
Write-Host "????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?  CORRECTION TERMINÉE                         ?" -ForegroundColor Green
Write-Host "????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "Fichiers corrigés: $fixed / $total" -ForegroundColor White
Write-Host ""
Write-Host "?? Compilation en cours..." -ForegroundColor Yellow
Write-Host ""

# Compiler
$buildResult = dotnet build wmine.csproj 2>&1
$buildSuccess = $LASTEXITCODE -eq 0

if ($buildSuccess) {
    Write-Host ""
    Write-Host "????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host "?  ??? COMPILATION RÉUSSIE ???                ?" -ForegroundColor White
    Write-Host "????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Projet prêt à l'exécution!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "????????????????????????????????????????????????" -ForegroundColor Red
    Write-Host "?  ? ERREURS RESTANTES                        ?" -ForegroundColor Yellow
    Write-Host "????????????????????????????????????????????????" -ForegroundColor Red
    Write-Host ""
    
    # Afficher uniquement les erreurs uniques
    $errors = $buildResult | Select-String "error CS" | Select-Object -First 10 -Unique
    $errors | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    
    Write-Host ""
    Write-Host "Total erreurs: " -NoNewline -ForegroundColor Yellow
    $errorCount = ($buildResult | Select-String "error CS").Count
    Write-Host $errorCount -ForegroundColor Red
}

Write-Host ""
Write-Host "Script terminé." -ForegroundColor Cyan
