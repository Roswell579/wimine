@echo off
echo ========================================
echo    TEST DU SERVICE DE ROUTING
echo ========================================
echo.
echo Modification du Program.cs pour activer le test...
echo.

cd /d "%~dp0"

REM Créer une sauvegarde temporaire
copy /Y Program.cs Program.cs.backup >nul

REM Activer le test dans Program.cs
powershell -Command "(Get-Content Program.cs) -replace '// TestRouteService\(\);', 'TestRouteService();' -replace '// return;', 'return;' | Set-Content Program.cs"

echo Compilation et lancement du test...
dotnet run

echo.
echo Restauration du Program.cs original...
move /Y Program.cs.backup Program.cs >nul

echo.
echo ========================================
echo    Test termine!
echo ========================================
pause
