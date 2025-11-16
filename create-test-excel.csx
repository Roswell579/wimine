using ClosedXML.Excel;

// Script PowerShell pour créer un fichier Excel de test
// Exécuter avec : dotnet script create-test-excel.csx

var workbook = new XLWorkbook();
var worksheet = workbook.Worksheets.Add("Filons");

// En-têtes
worksheet.Cell(1, 1).Value = "Nom du Filon";
worksheet.Cell(1, 2).Value = "Lambert X";
worksheet.Cell(1, 3).Value = "Lambert Y";

// Style des en-têtes
var headerRange = worksheet.Range(1, 1, 1, 3);
headerRange.Style.Font.Bold = true;
headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

// Données
var data = new[]
{
    new { Nom = "Mine du Cap Garonne", X = 971045, Y = 3144260 },
    new { Nom = "Mine de l'Argentière", X = 985420, Y = 3162580 },
    new { Nom = "Filon de la Madeleine", X = 991234, Y = 3145678 },
    new { Nom = "Mine des Bormettes", X = 978900, Y = 3156700 },
    new { Nom = "Filon Saint-Pierre", X = 982500, Y = 3158900 },
    new { Nom = "Mine de l'Esterel", X = 995600, Y = 3142300 },
    new { Nom = "Filon des Maures", X = 988750, Y = 3151200 },
    new { Nom = "Mine de la Colle", X = 976800, Y = 3159400 },
    new { Nom = "Filon du Pradet", X = 972300, Y = 3143800 },
    new { Nom = "Mine des Salettes", X = 993400, Y = 3147900 }
};

int row = 2;
foreach (var item in data)
{
    worksheet.Cell(row, 1).Value = item.Nom;
    worksheet.Cell(row, 2).Value = item.X;
    worksheet.Cell(row, 3).Value = item.Y;
    row++;
}

// Ajuster les largeurs
worksheet.Column(1).Width = 30;
worksheet.Column(2).Width = 15;
worksheet.Column(3).Width = 15;

// Sauvegarder
workbook.SaveAs("test-import-filons.xlsx");
Console.WriteLine("? Fichier Excel créé : test-import-filons.xlsx");
