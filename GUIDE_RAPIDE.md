# ? GUIDE RAPIDE - 3 MODIFICATIONS PRIORITAIRES

**Temps total estimé: 10 minutes**

---

## ?? MODIFICATION 1: Rotation de la carte (5 minutes)

### étape 1.1: Ajouter les champs dans Form1.cs
Recherchez dans Form1.cs la ligne contenant `private FilonDataService _dataService;`

Ajoutez JUSTE APRéS cette ligne:
```csharp
private bool _isRotating = false;
private Point _rotationStartPoint;
private float _startBearing = 0f;
```

### étape 1.2: Ajouter les méthodes dans Form1.cs  
é LA FIN du fichier Form1.cs (avant la derniére accolade `}`), ajoutez:
```csharp
private void GMapControl_MouseDown(object sender, MouseEventArgs e)
{
    if (e.Button == MouseButtons.Left && (ModifierKeys & Keys.Control) == Keys.Control)
    {
        _isRotating = true;
        _rotationStartPoint = e.Location;
        _startBearing = gMapControl.Bearing;
        gMapControl.CanDragMap = false;
        Cursor = Cursors.SizeAll;
    }
}

private void GMapControl_MouseMove(object sender, MouseEventArgs e)
{
    if (_isRotating)
    {
        int deltaX = e.Location.X - _rotationStartPoint.X;
        float rotation = deltaX * 0.5f;
        gMapControl.Bearing = _startBearing + rotation;
    }
}

private void GMapControl_MouseUp(object sender, MouseEventArgs e)
{
    if (_isRotating)
    {
        _isRotating = false;
        gMapControl.CanDragMap = true;
        Cursor = Cursors.Default;
    }
}
```

### étape 1.3: Activer les événements dans Form1.Designer.cs
Recherchez dans Form1.Designer.cs la ligne:
```csharp
gMapControl.Zoom = 10D
```

JUSTE APRéS les `};` qui suivent, ajoutez:
```csharp
gMapControl.MouseDown += GMapControl_MouseDown;
gMapControl.MouseMove += GMapControl_MouseMove;
gMapControl.MouseUp += GMapControl_MouseUp;
```

### étape 1.4: Ajouter les boutons de rotation dans Form1.Designer.cs
Recherchez dans Form1.Designer.cs:
```csharp
panelMap.Controls.Add(_floatingMapSelector);
_floatingMapSelector.BringToFront();
```

JUSTE APRéS ces 2 lignes, ajoutez:
```csharp
// Bouton rotation 90é
var btnRotateMap = new Button
{
    Text = "??",
    Width = 50,
    Height = 50,
    Location = new Point(20, 220),
    BackColor = Color.FromArgb(156, 39, 176),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat,
    Font = new Font("Segoe UI", 16),
    Cursor = Cursors.Hand,
    Anchor = AnchorStyles.Top | AnchorStyles.Left
};
btnRotateMap.FlatAppearance.BorderSize = 0;
btnRotateMap.Click += (s, e) => { gMapControl.Bearing = (gMapControl.Bearing + 90) % 360; };
panelMap.Controls.Add(btnRotateMap);
btnRotateMap.BringToFront();

// Bouton reset orientation
var btnResetOrientation = new Button
{
    Text = "??",
    Width = 50,
    Height = 50,
    Location = new Point(80, 220),
    BackColor = Color.FromArgb(33, 150, 243),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat,
    Font = new Font("Segoe UI", 16),
    Cursor = Cursors.Hand,
    Anchor = AnchorStyles.Top | AnchorStyles.Left
};
btnResetOrientation.FlatAppearance.BorderSize = 0;
btnResetOrientation.Click += (s, e) => { gMapControl.Bearing = 0f; };
panelMap.Controls.Add(btnResetOrientation);
btnResetOrientation.BringToFront();
```

---

## ?? MODIFICATION 2: Double-clic sur liste filons (3 minutes)

### Dans Form1.cs - Méthode BtnViewFiches_Click
Recherchez dans Form1.cs:
```csharp
listView.Dock = DockStyle.Fill;
```

JUSTE APRéS cette ligne, ajoutez:
```csharp
// Double-clic pour voir sur la carte
listView.DoubleClick += (s, e) =>
{
    if (listView.SelectedItems.Count > 0 && listView.SelectedItems[0].Tag is Filon filon)
    {
        fichesForm.Close();
        if (filon.Latitude.HasValue && filon.Longitude.HasValue)
        {
            gMapControl.Position = new GMap.NET.PointLatLng(filon.Latitude.Value, filon.Longitude.Value);
            gMapControl.Zoom = 14;
            mainTabControl.SelectedTab = tabPageMap;
        }
    }
};
```

---

## ?? MODIFICATION 3: Enlever glass du FloatingMapSelector (2 minutes)

### Dans UI/FloatingMapSelector.cs
Recherchez la méthode `LoadMapTypes()` ou lé oé les boutons sont créés.

Remplacez TOUTES les occurrences de:
```csharp
new TransparentGlassButton
```

par:
```csharp
new Button
```

ET remplacez:
```csharp
BaseColor = Color.FromArgb(...)
Transparency = 220
```

par:
```csharp
BackColor = Color.FromArgb(...),
FlatStyle = FlatStyle.Flat
```

Puis ajoutez APRéS chaque création de bouton:
```csharp
btn.FlatAppearance.BorderSize = 0;
```

---

## ? TEST FINAL

```powershell
cd "C:\Users\Roswell\Desktop\WitoJordanMineLocalisateur\wmine"
dotnet clean
dotnet build
dotnet run
```

### Vérifier:
1. ? Sur la carte: maintenir Ctrl + glisser souris = rotation
2. ? Clic sur ?? = rotation 90é
3. ? Clic sur ?? = reset orientation (zoom reste inchangé!)
4. ? Liste filons: double-clic sur un item = voir sur carte
5. ? Boutons sélecteur de carte sans effet glass

---

## ?? EN CAS DE PROBLéME

Si éa ne compile pas, supprimez les modifications et recommencez étape par étape.

Le fichier `MODIFICATIONS_A_APPLIQUER.md` contient plus de détails si nécessaire.
