# Script de correction automatique COMPLETE des opérateurs manquants
# Ce script corrige TOUS les opérateurs ?? et ? manquants dans le projet

$ErrorActionPreference = "Stop"
$projectPath = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host ""
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  CORRECTION AUTOMATIQUE DES OPERATEURS MANQUANTS" -ForegroundColor White
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Fonction pour corriger un fichier
function Fix-CSharpFile {
    param (
        [string]$FilePath
    )
    
    $content = Get-Content $FilePath -Raw -Encoding UTF8
    $originalContent = $content
    $changes = 0
    
    # Pattern 1: variable = autre new Type() -> variable ?? new Type()
    $pattern1 = '(\$?\w+)\s*=\s*(\w+)\s+new\s+'
    if ($content -match $pattern1) {
        $content = $content -replace $pattern1, '$1 = $2 ?? new '
        $changes++
    }
    
    # Pattern 2: variable = condition autre : valeur -> variable = condition ? autre : valeur
    $pattern2 = '=\s*([^=\s]+)\s+([^?\s:]+)\s*:\s*'
    $matches = [regex]::Matches($content, $pattern2)
    foreach ($match in $matches) {
        $original = $match.Value
        $fixed = $original -replace '\s+([^?\s:]+)\s*:\s*', ' ? $1 : '
        $content = $content.Replace($original, $fixed)
        $changes++
    }
    
    # Pattern 3: variable = autre "string" : "string" -> variable ?? "string"
    $pattern3 = '(\w+)\s+"([^"]+)"\s*:'
    if ($content -match $pattern3) {
        $content = $content -replace $pattern3, '$1 ?? "$2" :'
        $changes++
    }
    
    # Pattern 4: ToString() "string" -> ToString() ?? "string"
    $pattern4 = 'ToString\(\)\s+"([^"]+)"'
    if ($content -match $pattern4) {
        $content = $content -replace $pattern4, 'ToString() ?? "$1"'
        $changes++
    }
    
    # Pattern 5: condition "string" : "string" (opérateur ternaire)
    $pattern5 = '\(([^)]+)\)\s+"([^"]+)"\s*:\s*"([^"]+)"'
    if ($content -match $pattern5) {
        $content = $content -replace $pattern5, '($1) ? "$2" : "$3"'
        $changes++
    }
    
    # Pattern 6: SubItems.Add(value "default") -> SubItems.Add(value ?? "default")
    $pattern6 = 'Add\(([^)]+)\s+"([^"]+)"\)'
    if ($content -match $pattern6) {
        $content = $content -replace $pattern6, 'Add($1 ?? "$2")'
        $changes++
    }
    
    # Pattern 7: e = e EventArgs.Empty -> e ?? EventArgs.Empty
    $pattern7 = '(\w+)\s*=\s*\1\s+EventArgs\.Empty'
    if ($content -match $pattern7) {
        $content = $content -replace $pattern7, '$1 = $1 ?? EventArgs.Empty'
        $changes++
    }
    
    # Pattern 8: var x = condition value1 : value2 (ternaire simple)
    $pattern8 = 'var\s+(\w+)\s*=\s*([^=]+?)\s+(\w+)\s*:\s*(\w+);'
    if ($content -match $pattern8) {
        $content = $content -replace $pattern8, 'var $1 = $2 ? $3 : $4;'
        $changes++
    }
    
    if ($content -ne $originalContent) {
        Set-Content -Path $FilePath -Value $content -Encoding UTF8 -NoNewline
        return $changes
    }
    
    return 0
}

# Trouver tous les fichiers .cs
$files = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse | 
    Where-Object { 
        $_.FullName -notmatch '\\obj\\' -and 
        $_.FullName -notmatch '\\bin\\' -and
        $_.FullName -notmatch '\\.vs\\' 
    }

$totalFiles = $files.Count
$currentFile = 0
$filesFixed = 0
$totalChanges = 0

Write-Host "Fichiers à traiter : $totalFiles" -ForegroundColor Yellow
Write-Host ""

foreach ($file in $files) {
    $currentFile++
    $relativePath = $file.FullName.Replace($projectPath, "").TrimStart('\')
    
    Write-Progress -Activity "Correction en cours..." `
        -Status "[$currentFile/$totalFiles] $relativePath" `
        -PercentComplete (($currentFile / $totalFiles) * 100)
    
    $changes = Fix-CSharpFile -FilePath $file.FullName
    
    if ($changes -gt 0) {
        $filesFixed++
        $totalChanges += $changes
        Write-Host "? " -ForegroundColor Green -NoNewline
        Write-Host "$relativePath " -ForegroundColor White -NoNewline
        Write-Host "($changes correction(s))" -ForegroundColor Gray
    }
}

Write-Progress -Activity "Correction en cours..." -Completed

Write-Host ""
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  CORRECTION TERMINÉE !" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "Fichiers traités    : $totalFiles" -ForegroundColor White
Write-Host "Fichiers corrigés   : $filesFixed" -ForegroundColor Green
Write-Host "Corrections totales : $totalChanges" -ForegroundColor Yellow
Write-Host ""
Write-Host "Prochaine étape : " -ForegroundColor Cyan -NoNewline
Write-Host "dotnet build" -ForegroundColor White
Write-Host ""
