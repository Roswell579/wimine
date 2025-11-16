# Script pour corriger les 6 points précis

$projectPath = "C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine"
cd $projectPath

Write-Host "?? Corrections UI en cours..." -ForegroundColor Cyan

# 1. ? Déjà fait - SettingsPanel descriptifs en blanc

# 2. ? Déjà fait - ImportPanel bordures

# 3. FilonEditForm - Texte en blanc (pas les boutons)
Write-Host "3?? FilonEditForm - Texte en blanc..." -ForegroundColor Yellow
$content = Get-Content "Forms\FilonEditForm.cs" -Raw -Encoding UTF8
# Trouver les Labels et mettre ForeColor = Color.White
$content = $content -replace '(new Label\s*\{[^}]*?)ForeColor = Color\.Black', '$1ForeColor = Color.White'
Set-Content "Forms\FilonEditForm.cs" -Value $content -Encoding UTF8 -NoNewline

# 4. Form1.Designer.cs - Bouton Liste en blanc
Write-Host "4?? Bouton Liste Filons - Texte en blanc..." -ForegroundColor Yellow
$content = Get-Content "Form1.Designer.cs" -Raw -Encoding UTF8
$content = $content -replace '(btnViewFiches[^}]*?)ForeColor = Color\.Black', '$1ForeColor = Color.White'
Set-Content "Form1.Designer.cs" -Value $content -Encoding UTF8 -NoNewline

# 5. FilonEditForm.Designer.cs - Texte labels en blanc
Write-Host "5?? Edition fiche - Labels en blanc..." -ForegroundColor Yellow
$content = Get-Content "Forms\FilonEditForm.Designer.cs" -Raw -Encoding UTF8
# Tous les labels en blanc (pas les boutons)
$content = $content -replace '(lbl\w+\.ForeColor = )Color\.Black', '$1Color.White'
$content = $content -replace '(grp\w+\.ForeColor = )Color\.Black', '$1Color.White'
Set-Content "Forms\FilonEditForm.Designer.cs" -Value $content -Encoding UTF8 -NoNewline

# 6. Panneau rétractable (FloatingMapSelector) - Boutons en noir
Write-Host "6?? Panneau rétractable - Boutons en noir..." -ForegroundColor Yellow
$content = Get-Content "UI\FloatingMapSelector.cs" -Raw -Encoding UTF8
# Boutons du panneau en noir
$content = $content -replace '(new Button\s*\{[^}]*?)ForeColor = Color\.White', '$1ForeColor = Color.Black'
Set-Content "UI\FloatingMapSelector.cs" -Value $content -Encoding UTF8 -NoNewline

Write-Host "`n? Corrections terminées!" -ForegroundColor Green
Write-Host "?? Compilation..." -ForegroundColor Cyan

dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n?? BUILD RÉUSSIE!" -ForegroundColor Green
} else {
    Write-Host "`n? Erreurs de build" -ForegroundColor Red
}
