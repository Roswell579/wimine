# Script de correction automatique des opérateurs manquants

$projectPath = "C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine"

# Liste des patterns à corriger (sans ?? et ?)
$fixes = @(
    # Opérateur ?? (null-coalescing) - Patterns communs
    @{ Pattern = '(\w+)\s+new\s+(\w+)'; Replacement = '$1 ?? new $2'; Desc = "?? avant new" },
    @{ Pattern = '(\))\s+"([^"]+)"\s*;'; Replacement = '$1 ?? "$2";'; Desc = "?? avant string" },
    @{ Pattern = 'ToString\(\)\s+"'; Replacement = 'ToString() ?? "'; Desc = "?? après ToString()" },
    
    # Opérateur ? (ternaire) - Patterns communs  
    @{ Pattern = '=\s+(\w+)\s+(\w+)\s+:'; Replacement = '= $1 ? $2 :'; Desc = "? dans assignation" },
    @{ Pattern = '\((\w+)\s+"([^"]+)"\s+:'; Replacement = '($1 ? "$2" :'; Desc = "? dans condition" }
)

Write-Host " Correction automatique des opérateurs manquants..." -ForegroundColor Cyan
Write-Host ""

$allFiles = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse | 
    Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }

$totalFiles = $allFiles.Count
$currentFile = 0
$totalChanges = 0

foreach ($file in $allFiles) {
    $currentFile++
    $relativePath = $file.FullName.Replace($projectPath, "").TrimStart('\')
    
    Write-Progress -Activity "Correction en cours..." -Status "$currentFile/$totalFiles : $relativePath" `
        -PercentComplete (($currentFile / $totalFiles) * 100)
    
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    $fileChanges = 0
    
    foreach ($fix in $fixes) {
        $matches = [regex]::Matches($content, $fix.Pattern)
        if ($matches.Count -gt 0) {
            $content = $content -replace $fix.Pattern, $fix.Replacement
            $fileChanges += $matches.Count
        }
    }
    
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host " $relativePath" -ForegroundColor Green
        Write-Host "    $fileChanges changement(s)" -ForegroundColor Gray
        $totalChanges += $fileChanges
    }
}

Write-Progress -Activity "Correction en cours..." -Completed

Write-Host ""
Write-Host "??????????????????????????????????????????" -ForegroundColor Cyan
Write-Host " Correction terminée !" -ForegroundColor Green
Write-Host "??????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "Fichiers traités : $totalFiles"
Write-Host "Changements total : $totalChanges"
Write-Host ""
Write-Host " Exécutez maintenant : dotnet build" -ForegroundColor Yellow
