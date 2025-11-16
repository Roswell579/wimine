# Script PowerShell pour créer un fichier Excel de test
# Exécuter avec : powershell -ExecutionPolicy Bypass -File create-test-excel.ps1

Write-Host "?? Création d'un fichier Excel de test..." -ForegroundColor Cyan

$excelPath = Join-Path $PSScriptRoot "test-import-filons.xlsx"

# Créer l'objet Excel
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false

try {
    # Créer un nouveau classeur
    $workbook = $excel.Workbooks.Add()
    $worksheet = $workbook.Worksheets.Item(1)
    $worksheet.Name = "Filons"

    # En-têtes
    $worksheet.Cells.Item(1, 1) = "Nom du Filon"
    $worksheet.Cells.Item(1, 2) = "Lambert X"
    $worksheet.Cells.Item(1, 3) = "Lambert Y"

    # Style des en-têtes
    $headerRange = $worksheet.Range("A1:C1")
    $headerRange.Font.Bold = $true
    $headerRange.Interior.ColorIndex = 15  # Gris clair
    $headerRange.Borders.Weight = 2

    # Données
    $data = @(
        @("Mine du Cap Garonne", 971045, 3144260),
        @("Mine de l'Argentière", 985420, 3162580),
        @("Filon de la Madeleine", 991234, 3145678),
        @("Mine des Bormettes", 978900, 3156700),
        @("Filon Saint-Pierre", 982500, 3158900),
        @("Mine de l'Esterel", 995600, 3142300),
        @("Filon des Maures", 988750, 3151200),
        @("Mine de la Colle", 976800, 3159400),
        @("Filon du Pradet", 972300, 3143800),
        @("Mine des Salettes", 993400, 3147900)
    )

    $row = 2
    foreach ($item in $data) {
        $worksheet.Cells.Item($row, 1) = $item[0]
        $worksheet.Cells.Item($row, 2) = $item[1]
        $worksheet.Cells.Item($row, 3) = $item[2]
        $row++
    }

    # Ajuster les largeurs de colonnes
    $worksheet.Columns.Item(1).ColumnWidth = 30
    $worksheet.Columns.Item(2).ColumnWidth = 15
    $worksheet.Columns.Item(3).ColumnWidth = 15

    # Sauvegarder
    $workbook.SaveAs($excelPath)
    $workbook.Close($false)
    
    Write-Host "? Fichier Excel créé avec succès !" -ForegroundColor Green
    Write-Host "?? Emplacement : $excelPath" -ForegroundColor Gray
    Write-Host ""
    Write-Host "?? Contenu :" -ForegroundColor Yellow
    Write-Host "  • 10 filons de test" -ForegroundColor White
    Write-Host "  • Colonnes : Nom, Lambert X, Lambert Y" -ForegroundColor White
    Write-Host "  • Coordonnées valides pour la région Var/Alpes-Maritimes" -ForegroundColor White
    Write-Host ""
    Write-Host "?? Utilisez ce fichier pour tester l'import Excel !" -ForegroundColor Cyan
}
catch {
    Write-Host "? Erreur lors de la création du fichier : $_" -ForegroundColor Red
}
finally {
    # Fermer Excel
    $excel.Quit()
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
    Remove-Variable excel
}

Write-Host ""
Write-Host "Appuyez sur une touche pour continuer..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
