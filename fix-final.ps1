# Correction finale de TOUS les opérateurs manquants
# Ce script recherche les double-espaces qui remplacent les opérateurs

cd $PSScriptRoot

Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  CORRECTION FINALE DES OPÉRATEURS" -ForegroundColor White
Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

$files = @(
    "Forms\MineralEditForm.cs",
    "Forms\MineralsPanel.cs",
    "Forms\OcrImportForm.cs",
    "Forms\PinDialog.cs"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "Correction de $file..." -ForegroundColor Yellow
        $content = Get-Content $file -Raw
        
        # Pattern 1: ToString()  "" -> ToString() ?? ""
        $content = $content -replace 'ToString\(\)\s\s"', 'ToString() ?? "'
        
        # Pattern 2: variable  autre -> variable ?? autre  
        $content = $content -replace '(\w+)\s\s(new \w+)', '$1 ?? $2'
        $content = $content -replace '(\w+)\s\s(throw )', '$1 ?? $2'
        $content = $content -replace '(\w+)\s\s(EventArgs)', '$1 ?? $2'
        $content = $content -replace '(\w+)\s\s(")', '$1 ?? $2'
        
        # Pattern 3: condition  value : autre -> condition ? value : autre
        $content = $content -replace '(\w+)\s\s([^:]+)\s*:\s*', '$1 ? $2 : '
        $content = $content -replace '(_\w+)\s\s(")', '$1 ? $2'
        $content = $content -replace '(\w+)\s\s(Color\.)', '$1 ? $2'
        $content = $content -replace '> 1\s\s"s"', '> 1 ? "s"'
        
        # Pattern 4: result.Property  "default" -> result.Property ?? "default"
        $content = $content -replace '(result\.\w+)\s\s(")', '$1 ?? $2'
        
        Set-Content $file $content -NoNewline
        Write-Host "  ? $file corrigé" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  CORRECTIONS TERMINÉES" -ForegroundColor Green
Write-Host "???????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "Exécutez maintenant:" -ForegroundColor Yellow
Write-Host "  dotnet build" -ForegroundColor White
