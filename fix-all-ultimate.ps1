# SCRIPT ULTIME - CORRECTION AUTOMATIQUE COMPLÈTE
# Corrige TOUS les opérateurs manquants dans TOUS les fichiers .cs du projet

$projectPath = "C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine"
cd $projectPath

Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  CORRECTION AUTOMATIQUE COMPLÈTE - TOUS LES FICHIERS" -ForegroundColor White
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Fonction de correction universelle
function Fix-CSharpOperators {
    param([string]$Content)
    
    # RÈGLE 1: variable  "text" -> variable ?? "text"
    $Content = $Content -replace '(\w+)\s{2,}"', '$1 ?? "'
    
    # RÈGLE 2: variable  new -> variable ?? new
    $Content = $Content -replace '(\w+)\s{2,}new\s', '$1 ?? new '
    
    # RÈGLE 3: variable  throw -> variable ?? throw
    $Content = $Content -replace '(\w+)\s{2,}throw', '$1 ?? throw'
    
    # RÈGLE 4: variable  EventArgs -> variable ?? EventArgs
    $Content = $Content -replace '(\w+\s+e)\s{2,}EventArgs', '$1 ?? EventArgs'
    
    # RÈGLE 5: variable  0 -> variable ?? 0
    $Content = $Content -replace '(\w+)\s{2,}0([;,)])', '$1 ?? 0$2'
    
    # RÈGLE 6: variable  1 -> variable ?? 1
    $Content = $Content -replace '(\w+)\s{2,}1([;,)])', '$1 ?? 1$2'
    
    # RÈGLE 7: ?.Method()  "text" -> ?.Method() ?? "text"
    $Content = $Content -replace '(\?\.\w+\([^)]*\))\s{2,}"', '$1 ?? "'
    
    # RÈGLE 8: condition  value : autre -> condition ? value : autre
    # Pattern: (mot ou expression) suivi de 2+ espaces suivi de valeur suivi de :
    $Content = $Content -replace '(\w+\s*>\s*\d+)\s{2,}([^:\s]+)\s*:', '$1 ? $2 :'
    $Content = $Content -replace '(\w+\s*<\s*\d+)\s{2,}([^:\s]+)\s*:', '$1 ? $2 :'
    $Content = $Content -replace '(\w+\s*==\s*\w+)\s{2,}([^:\s]+)\s*:', '$1 ? $2 :'
    $Content = $Content -replace '(\w+\s*!=\s*null)\s{2,}([^:\s]+)\s*:', '$1 ? $2 :'
    $Content = $Content -replace '(\.HasValue)\s{2,}', '$1 ? '
    $Content = $Content -replace '(\.Any\(\))\s{2,}([^:\s]+)\s*:', '$1 ? $2 :'
    $Content = $Content -replace '(\.HasFlag\([^)]+\))\s{2,}', '$1 ? '
    $Content = $Content -replace '(\.Contains\([^)]+\))\s{2,}', '$1 ? '
    $Content = $Content -replace '(\.HasCoordinates\(\))\s{2,}"', '$1 ? "'
    $Content = $Content -replace '(\.HasPin\(\))\s{2,}"', '$1 ? "'
    
    # RÈGLE 9: Cas spéciaux multi-lignes
    # TotalCount > 0\n    (expression)\n    : 0
    $Content = $Content -replace '(TotalCount > 0)\s+\(', '$1 ? ('
    
    # RÈGLE 10: Remplacer les ?? devenus doubles
    $Content = $Content -replace '\?\?\s+\?\?', '??'
    
    return $Content
}

# Obtenir TOUS les fichiers .cs (sauf bin/obj)
$files = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse | 
    Where-Object { 
        $_.FullName -notmatch '\\obj\\' -and 
        $_.FullName -notmatch '\\bin\\' -and
        $_.FullName -notmatch '\.bak$'
    }

$totalFiles = $files.Count
$fixedFiles = 0
$unchanged = 0

Write-Host "?? Fichiers trouvés: $totalFiles" -ForegroundColor Yellow
Write-Host ""

foreach ($file in $files) {
    $relativePath = $file.FullName.Replace("$projectPath\", "")
    
    try {
        $originalContent = Get-Content $file.FullName -Raw -Encoding UTF8 -ErrorAction Stop
        $fixedContent = Fix-CSharpOperators -Content $originalContent
        
        if ($originalContent -ne $fixedContent) {
            # Créer une sauvegarde
            $backupPath = "$($file.FullName).bak"
            if (-not (Test-Path $backupPath)) {
                Copy-Item $file.FullName $backupPath -Force
            }
            
            # Écrire le fichier corrigé
            Set-Content $file.FullName $fixedContent -Encoding UTF8 -NoNewline
            Write-Host "? $relativePath" -ForegroundColor Green
            $fixedFiles++
        } else {
            Write-Host "? $relativePath (déjà correct)" -ForegroundColor Gray
            $unchanged++
        }
    }
    catch {
        Write-Host "? $relativePath - ERREUR: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  CORRECTION TERMINÉE" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "Fichiers corrigés:    $fixedFiles" -ForegroundColor Green
Write-Host "Fichiers inchangés:   $unchanged" -ForegroundColor Gray
Write-Host "Total traité:         $totalFiles" -ForegroundColor White
Write-Host ""
Write-Host "?? COMPILATION EN COURS..." -ForegroundColor Yellow
Write-Host ""

# Compiler le projet
$buildOutput = dotnet build wmine.csproj --nologo 2>&1
$buildSuccess = $LASTEXITCODE -eq 0

if ($buildSuccess) {
    Write-Host ""
    Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host "  ??? BUILD RÉUSSI ???" -ForegroundColor White
    Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? PROJET PRÊT À L'EXÉCUTION!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Lancez votre application avec:" -ForegroundColor Yellow
    Write-Host "  dotnet run" -ForegroundColor White
    Write-Host "ou ouvrez wmine.sln dans Visual Studio" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Red
    Write-Host "  ? ERREURS RESTANTES" -ForegroundColor Yellow
    Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Red
    Write-Host ""
    
    # Compter les erreurs
    $errorLines = $buildOutput | Select-String "error CS"
    $errorCount = ($errorLines | Measure-Object).Count
    
    Write-Host "Total erreurs: $errorCount" -ForegroundColor Red
    Write-Host ""
    
    if ($errorCount -le 20) {
        Write-Host "Erreurs détaillées:" -ForegroundColor Yellow
        $errorLines | Select-Object -First 20 | ForEach-Object {
            Write-Host $_ -ForegroundColor Red
        }
    } else {
        Write-Host "Première erreurs (sur $errorCount):" -ForegroundColor Yellow
        $errorLines | Select-Object -First 10 | ForEach-Object {
            Write-Host $_ -ForegroundColor Red
        }
    }
    
    Write-Host ""
    Write-Host "?? Vérifiez les erreurs et relancez le script si nécessaire" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Script terminé." -ForegroundColor Cyan
Write-Host ""
