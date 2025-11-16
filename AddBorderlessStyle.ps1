# Script pour ajouter FlatStyle et BorderSize = 0 à tous les boutons
# Date: Janvier 2025

Write-Host "?? Ajout de FlatStyle et BorderSize = 0 à tous les boutons..." -ForegroundColor Cyan

$files = Get-ChildItem -Path "Forms" -Filter "*.cs" -Recurse | Where-Object { $_.Name -notmatch "\.Designer\.cs$" -or $_.Name -eq "FilonEditForm.Designer.cs" }

$totalModified = 0

foreach ($file in $files) {
    Write-Host "`n?? Traitement de $($file.Name)..." -ForegroundColor Yellow
    
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $modified = $false
    
    # Pattern pour trouver les déclarations de boutons qui n'ont pas déjà FlatStyle
    $buttonPattern = '((?:var|private)\s+\w+\s*=\s*new\s+Button\s*\{[^}]*?)(Font\s*=\s*new\s+Font[^}]*?\})'
    
    $content = [regex]::Replace($content, $buttonPattern, {
        param($match)
        $declaration = $match.Groups[1].Value
        $fontPart = $match.Groups[2].Value
        
        # Vérifier si FlatStyle existe déjà
        if ($declaration -notmatch 'FlatStyle\s*=') {
            # Ajouter FlatStyle et Cursor avant Font
            $result = $declaration + "`n                FlatStyle = FlatStyle.Flat,`n                Cursor = Cursors.Hand,`n                " + $fontPart
            $script:modified = $true
            return $result
        }
        return $match.Value
    })
    
    # Ajouter .FlatAppearance.BorderSize = 0 après chaque déclaration de Button
    $buttonEndPattern = '(\w+)\s*=\s*new\s+Button\s*\{[^}]*?\};'
    
    $content = [regex]::Replace($content, $buttonEndPattern, {
        param($match)
        $buttonName = $match.Groups[1].Value
        $fullMatch = $match.Value
        
        # Vérifier si BorderSize n'existe pas déjà juste après
        $position = $match.Index + $match.Length
        $nextLines = $content.Substring($position, [Math]::Min(100, $content.Length - $position))
        
        if ($nextLines -notmatch "$buttonName\.FlatAppearance\.BorderSize") {
            $result = $fullMatch + "`n            $buttonName.FlatAppearance.BorderSize = 0;"
            $script:modified = $true
            return $result
        }
        return $fullMatch
    })
    
    if ($modified) {
        Set-Content $file.FullName $content -NoNewline -Encoding UTF8
        $totalModified++
        Write-Host "? $($file.Name) modifié" -ForegroundColor Green
    } else {
        Write-Host "??  $($file.Name) - aucun changement nécessaire" -ForegroundColor Gray
    }
}

Write-Host "`n? Traitement terminé!" -ForegroundColor Green
Write-Host "?? $totalModified fichier(s) modifié(s)" -ForegroundColor Cyan

Write-Host "`n?? Compilation..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n? Compilation réussie! Tous les boutons ont maintenant FlatStyle et BorderSize = 0" -ForegroundColor Green
} else {
    Write-Host "`n? Erreur de compilation. Vérifiez les modifications." -ForegroundColor Red
}
