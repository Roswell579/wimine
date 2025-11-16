# Script d'installation automatique des données Tesseract OCR
# Exécuter avec : powershell -ExecutionPolicy Bypass -File install-ocr-data.ps1

Write-Host "?? Vérification des données Tesseract OCR..." -ForegroundColor Cyan

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$tessDataFolder = Join-Path $projectRoot "tessdata"
$frDataFile = Join-Path $tessDataFolder "fra.traineddata"
$downloadUrl = "https://github.com/tesseract-ocr/tessdata/raw/main/fra.traineddata"

# Créer le dossier tessdata s'il n'existe pas
if (-not (Test-Path $tessDataFolder)) {
    Write-Host "?? Création du dossier tessdata..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $tessDataFolder -Force | Out-Null
}

# Vérifier si le fichier existe déjà
if (Test-Path $frDataFile) {
    $fileSize = (Get-Item $frDataFile).Length
    Write-Host "? fra.traineddata trouvé ($([math]::Round($fileSize/1MB, 2)) MB)" -ForegroundColor Green
    Write-Host "?? Emplacement: $frDataFile" -ForegroundColor Gray
} else {
    Write-Host "? fra.traineddata manquant" -ForegroundColor Red
    Write-Host "?? Téléchargement depuis GitHub..." -ForegroundColor Yellow
    
    try {
        # Télécharger le fichier
        $ProgressPreference = 'SilentlyContinue'
        Invoke-WebRequest -Uri $downloadUrl -OutFile $frDataFile -UseBasicParsing
        
        $fileSize = (Get-Item $frDataFile).Length
        Write-Host "? Téléchargement réussi ($([math]::Round($fileSize/1MB, 2)) MB)" -ForegroundColor Green
        Write-Host "?? Installé dans: $frDataFile" -ForegroundColor Gray
    }
    catch {
        Write-Host "? Erreur lors du téléchargement: $_" -ForegroundColor Red
        Write-Host "?? Téléchargez manuellement depuis:" -ForegroundColor Yellow
        Write-Host "   $downloadUrl" -ForegroundColor Cyan
        Write-Host "?? Et placez-le dans:" -ForegroundColor Yellow
        Write-Host "   $tessDataFolder" -ForegroundColor Cyan
        exit 1
    }
}

# Vérifier les dossiers de sortie
$debugFolder = Join-Path $projectRoot "bin\Debug\net8.0\tessdata"
$releaseFolder = Join-Path $projectRoot "bin\Release\net8.0\tessdata"

foreach ($outputFolder in @($debugFolder, $releaseFolder)) {
    if (Test-Path $outputFolder) {
        $outputFile = Join-Path $outputFolder "fra.traineddata"
        if (-not (Test-Path $outputFile)) {
            Write-Host "?? Copie vers: $outputFolder" -ForegroundColor Yellow
            Copy-Item $frDataFile $outputFile -Force
        } else {
            Write-Host "? Déjà présent dans: $outputFolder" -ForegroundColor Green
        }
    }
}

Write-Host ""
Write-Host "?? Installation terminée avec succès !" -ForegroundColor Green
Write-Host "?? Vous pouvez maintenant lancer l'application." -ForegroundColor Cyan
Write-Host ""

# Attendre une touche avant de fermer
Write-Host "Appuyez sur une touche pour continuer..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
