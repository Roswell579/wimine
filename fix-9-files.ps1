# CORRECTION SIMPLE ET DIRECTE - 9 FICHIERS
cd "C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine"

Write-Host "CORRECTION DES 9 FICHIERS RESTANTS..." -ForegroundColor Yellow

# 1. MarkerVisualizationForm.cs
(Get-Content "MarkerVisualizationForm.cs" -Raw) `
    -replace 'hover 1\.1f :', 'hover ? 1.1f :' `
    -replace 'hover 1\.2f :', 'hover ? 1.2f :' `
    | Set-Content "MarkerVisualizationForm.cs" -NoNewline
Write-Host "? MarkerVisualizationForm.cs" -ForegroundColor Green

# 2. ContactsDataService.cs
(Get-Content "Services\ContactsDataService.cs" -Raw) `
    -replace 'json\) new List', 'json) ?? new List' `
    -replace '\.Any\(\) _contacts', '.Any() ? _contacts' `
    | Set-Content "Services\ContactsDataService.cs" -NoNewline
Write-Host "? ContactsDataService.cs" -ForegroundColor Green

# 3. CoordinateConverter.cs
(Get-Content "Services\CoordinateConverter.cs" -Raw) `
    -replace 'isValid "', 'isValid ? "' `
    | Set-Content "Services\CoordinateConverter.cs" -NoNewline
Write-Host "? CoordinateConverter.cs" -ForegroundColor Green

# 4. CsvExportService.cs
(Get-Content "Services\CsvExportService.cs" -Raw) `
    -replace 'ToString\("F6"\) ""', 'ToString("F6") ?? ""' `
    -replace 'ToString\("F2"\) ""', 'ToString("F2") ?? ""' `
    | Set-Content "Services\CsvExportService.cs" -NoNewline
Write-Host "? CsvExportService.cs" -ForegroundColor Green

# 5. EmailService.cs
(Get-Content "Services\EmailService.cs" -Raw) `
    -replace '\.HasValue \$', '.HasValue ? $' `
    | Set-Content "Services\EmailService.cs" -NoNewline
Write-Host "? EmailService.cs" -ForegroundColor Green

# 6. ExcelImportService.cs
(Get-Content "Services\ExcelImportService.cs" -Raw) `
    -replace 'RowNumber\(\) firstDataRow', 'RowNumber() ?? firstDataRow' `
    -replace 'RowNumber\(\) 1\)', 'RowNumber() ?? 1)' `
    -replace 'IsNullOrWhiteSpace\(nom\) \$', 'IsNullOrWhiteSpace(nom) ? $' `
    | Set-Content "Services\ExcelImportService.cs" -NoNewline
Write-Host "? ExcelImportService.cs" -ForegroundColor Green

# 7. FilonDataService.cs
(Get-Content "Services\FilonDataService.cs" -Raw) `
    -replace 'json\) new List', 'json) ?? new List' `
    | Set-Content "Services\FilonDataService.cs" -NoNewline
Write-Host "? FilonDataService.cs" -ForegroundColor Green

# 8. MapBoundsRestrictor.cs
(Get-Content "Services\MapBoundsRestrictor.cs" -Raw) `
    -replace 'mapControl throw', 'mapControl ?? throw' `
    | Set-Content "Services\MapBoundsRestrictor.cs" -NoNewline
Write-Host "? MapBoundsRestrictor.cs" -ForegroundColor Green

# 9. MineralDataService.cs
(Get-Content "Services\MineralDataService.cs" -Raw) `
    -replace 'MineralType\.Améthyste', 'MineralType.Amethyste' `
    -replace 'MineralType\.Disthène', 'MineralType.Disthene' `
    | Set-Content "Services\MineralDataService.cs" -NoNewline
Write-Host "? MineralDataService.cs" -ForegroundColor Green

Write-Host ""
Write-Host "????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  TOUS LES FICHIERS CORRIGÉS !" -ForegroundColor Green
Write-Host "????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "Compilation..." -ForegroundColor Yellow
dotnet build wmine.csproj --nologo
