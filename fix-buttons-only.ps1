# Script CORRECT pour fixer UNIQUEMENT les boutons (pas les labels/textbox)

$projectPath = "C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine"

# Liste TOUS les fichiers .cs
$files = Get-ChildItem -Path "$projectPath" -Recurse -Include "*.cs" -File

$totalFixed = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    $fileFixed = 0
    
    # Pattern pour trouver les blocs "new Button"
    $pattern = '(new Button\s*\{[^}]+\})'
    $matches = [regex]::Matches($content, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    
    foreach ($match in $matches) {
        $buttonBlock = $match.Value
        $originalBlock = $buttonBlock
        
        # Vérifier si c'est bien un Button et pas autre chose
        if ($buttonBlock -notmatch 'new Button') {
            continue
        }
        
        # Changer ForeColor = Color.White en ForeColor = Color.Black (UNIQUEMENT dans les boutons)
        if ($buttonBlock -match 'ForeColor\s*=\s*Color\.White') {
            $buttonBlock = $buttonBlock -replace 'ForeColor\s*=\s*Color\.White', 'ForeColor = Color.Black'
            $fileFixed++
        }
        
        # Ajouter ForeColor = Color.Black si absent
        if ($buttonBlock -notmatch 'ForeColor\s*=') {
            # Ajouter après BackColor si existe
            if ($buttonBlock -match '(BackColor\s*=\s*[^,]+,)') {
                $buttonBlock = $buttonBlock -replace '(BackColor\s*=\s*[^,]+,)', "`$1`n                ForeColor = Color.Black,"
                $fileFixed++
            }
        }
        
        # Ajouter FlatStyle = FlatStyle.Flat si absent
        if ($buttonBlock -notmatch 'FlatStyle\s*=\s*FlatStyle\.Flat') {
            $buttonBlock = $buttonBlock -replace '(\s+\})', ",`n                FlatStyle = FlatStyle.Flat`$1"
            $fileFixed++
        }
        
        # Remplacer dans le contenu
        if ($originalBlock -ne $buttonBlock) {
            $content = $content.Replace($originalBlock, $buttonBlock)
        }
    }
    
    # Maintenant ajouter .FlatAppearance.BorderSize = 0 après chaque déclaration de bouton
    # Pattern: var btnXXX = new Button { ... };
    # Ajouter après le };
    $pattern2 = '(var\s+\w+\s*=\s*new Button\s*\{[^}]+\};)'
    $content = [regex]::Replace($content, $pattern2, {
        param($m)
        $block = $m.Value
        $varName = [regex]::Match($block, 'var\s+(\w+)').Groups[1].Value
        
        # Vérifier si .FlatAppearance.BorderSize = 0 existe déjà juste après
        $nextLines = $content.Substring($m.Index + $m.Length, [Math]::Min(200, $content.Length - $m.Index - $m.Length))
        if ($nextLines -notmatch "$varName\.FlatAppearance\.BorderSize\s*=\s*0") {
            return "$block`n            $varName.FlatAppearance.BorderSize = 0;"
        }
        return $block
    }, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    
    # Sauvegarder si changements
    if ($content -ne $originalContent) {
        Set-Content $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "? $($file.Name): $fileFixed bouton(s) corrigé(s)" -ForegroundColor Green
        $totalFixed += $fileFixed
    }
}

Write-Host "`n?? TOTAL: $totalFixed bouton(s) corrigé(s)" -ForegroundColor Cyan
Write-Host "?? Compilation..." -ForegroundColor Yellow

cd $projectPath
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n? BUILD RÉUSSIE!" -ForegroundColor Green
} else {
    Write-Host "`n? Erreurs (voir ci-dessus)" -ForegroundColor Red
}
