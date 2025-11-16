# MODIFICATIONS DEMANdéES PAR L'UTILISATEUR

## 1. ENLEVER TOUS LES EFFETS GLASS

### Fichiers é modifier (remplacer TransparentGlassButton par Button):
- Forms/FilonEditForm.Designer.cs
- Forms/GalleryForm.cs
- Forms/ImportPanel.cs
- Forms/PinDialog.cs
- Forms/SettingsPanel.cs
- UI/FloatingMapSelector.cs

### Pattern de remplacement:
```csharp
// AVANT:
private TransparentGlassButton btnSave;
btnSave = new TransparentGlassButton
{
    Text = "Enregistrer",
    BaseColor = Color.FromArgb(0, 150, 136),
    Transparency = 220,
    ...
};

// APRéS:
private Button btnSave;
btnSave = new Button
{
    Text = "Enregistrer",
    BackColor = Color.FromArgb(0, 150, 136),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat,
    ...
};
btnSave.FlatAppearance.BorderSize = 0;
```

## 2. DOUBLE-CLIC SUR LISTE FILONS

### Dans Form1.cs - Méthode BtnViewFiches_Click
Ajouter aprés la création du ListView:

```csharp
// Double-clic pour "Voir sur la carte"
listView.DoubleClick += (s, e) =>
{
    if (listView.SelectedItems.Count > 0 && listView.SelectedItems[0].Tag is Filon filon)
    {
        // Fermer le formulaire de liste
        fichesForm.Close();
        
        // Centrer sur le filon
        if (filon.Latitude.HasValue && filon.Longitude.HasValue)
        {
            gMapControl.Position = new PointLatLng(filon.Latitude.Value, filon.Longitude.Value);
            gMapControl.Zoom = 14;
            
            // Sélectionner dans le combo
            var comboItem = cmbSelectFilon.Items.Cast<FilonComboItem>()
                .FirstOrDefault(i => i.Filon?.Id == filon.Id);
            if (comboItem != null)
            {
                cmbSelectFilon.SelectedItem = comboItem;
            }
            
            // Aller é l'onglet carte
            mainTabControl.SelectedTab = tabPageMap;
        }
    }
};

// OU alternativement : Double-clic pour "éditer"
// listView.DoubleClick += (s, e) =>
// {
//     if (listView.SelectedItems.Count > 0 && listView.SelectedItems[0].Tag is Filon filon)
//     {
//         fichesForm.Close();
//         EditFilon(filon);
//     }
// };
```

## 3. ROTATION DE LA CARTE + BOUTONS

### A. Activer la rotation dans Form1.Designer.cs
Dans InitializeComponent(), aprés la création de gMapControl:

```csharp
// Activer la rotation de la carte
gMapControl.Bearing = 0f;  // déjé présent
gMapControl.CanDragMap = true;  // déjé présent

// AJOUTER ces événements pour permettre rotation avec Ctrl+Drag
gMapControl.MouseDown += GMapControl_MouseDown;
gMapControl.MouseMove += GMapControl_MouseMove;
gMapControl.MouseUp += GMapControl_MouseUp;
```

### B. Ajouter ces champs privés dans Form1.cs:
```csharp
private bool _isRotating = false;
private Point _rotationStartPoint;
private float _startBearing = 0f;
```

### C. Ajouter ces méthodes dans Form1.cs:
```csharp
private void GMapControl_MouseDown(object sender, MouseEventArgs e)
{
    if (e.Button == MouseButtons.Left && (ModifierKeys & Keys.Control) == Keys.Control)
    {
        _isRotating = true;
        _rotationStartPoint = e.Location;
        _startBearing = gMapControl.Bearing;
        gMapControl.CanDragMap = false;  // désactiver le drag pendant la rotation
        Cursor = Cursors.SizeAll;
    }
}

private void GMapControl_MouseMove(object sender, MouseEventArgs e)
{
    if (_isRotating)
    {
        // Calculer l'angle de rotation basé sur le mouvement horizontal
        int deltaX = e.Location.X - _rotationStartPoint.X;
        float rotation = deltaX * 0.5f;  // Facteur de sensibilité
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

### D. Enlever effet glass du FloatingMapSelector
Dans UI/FloatingMapSelector.cs, remplacer les boutons:

```csharp
// Dans LoadMapTypes() ou InitializeComponent():
foreach (var mapType in mapTypes)
{
    var btn = new Button  // Au lieu de TransparentGlassButton
    {
        Text = icon + " " + displayName,
        Width = 200,
        Height = 40,
        Location = new Point(10, yOffset),
        BackColor = Color.FromArgb(40, 45, 55),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 10),
        Cursor = Cursors.Hand,
        Tag = mapType
    };
    btn.FlatAppearance.BorderSize = 0;
    btn.Click += (s, e) => OnMapTypeChanged(mapType);
    
    // Effet hover
    btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(60, 65, 75);
    btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(40, 45, 55);
    
    this.Controls.Add(btn);
    yOffset += 45;
}
```

### E. Créer les boutons de rotation dans Form1.Designer.cs
Ajouter aprés la création de _floatingMapSelector:

```csharp
// Bouton de rotation de carte
var btnRotateMap = new Button
{
    Text = "??",
    Width = 50,
    Height = 50,
    Location = new Point(20, 220),  // Sous le sélecteur de carte
    BackColor = Color.FromArgb(156, 39, 176),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat,
    Font = new Font("Segoe UI", 16),
    Cursor = Cursors.Hand,
    Anchor = AnchorStyles.Top | AnchorStyles.Left
};
btnRotateMap.FlatAppearance.BorderSize = 0;
btnRotateMap.Click += (s, e) =>
{
    // Rotation de 90é dans le sens horaire
    gMapControl.Bearing = (gMapControl.Bearing + 90) % 360;
};
panelMap.Controls.Add(btnRotateMap);
btnRotateMap.BringToFront();

// Bouton reset orientation (PAS de reset zoom!)
var btnResetOrientation = new Button
{
    Text = "??",
    Width = 50,
    Height = 50,
    Location = new Point(80, 220),  // é cété du bouton rotation
    BackColor = Color.FromArgb(33, 150, 243),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat,
    Font = new Font("Segoe UI", 16),
    Cursor = Cursors.Hand,
    Anchor = AnchorStyles.Top | AnchorStyles.Left
};
btnResetOrientation.FlatAppearance.BorderSize = 0;
btnResetOrientation.Click += (s, e) =>
{
    // Reset seulement l'orientation, PAS le zoom
    gMapControl.Bearing = 0f;
};
panelMap.Controls.Add(btnResetOrientation);
btnResetOrientation.BringToFront();

// Tooltips
var tooltipMap = new ToolTip();
tooltipMap.SetToolTip(btnRotateMap, "Rotation 90é (ou Ctrl+Glisser sur la carte)");
tooltipMap.SetToolTip(btnResetOrientation, "Réinitialiser l'orientation Nord");
```

## ORDRE D'APPLICATION RECOMMANdé

1. ? Activer rotation carte (3.A, 3.B, 3.C) - 5 minutes
2. ? Ajouter boutons rotation (3.E) - 3 minutes
3. ? Double-clic liste filons (2) - 2 minutes
4. ? Enlever glass FloatingMapSelector (3.D) - 2 minutes
5. ? Enlever glass autres fichiers (1) - 15-20 minutes

## COMPILATION ET TEST

Aprés chaque étape:
```powershell
dotnet clean
dotnet build
dotnet run
```

Tester:
- Rotation: Ctrl+Drag sur la carte OU clic sur ??
- Reset orientation: Clic sur ?? (le zoom ne change pas!)
- Double-clic: Ouvrir liste filons, double-cliquer un item
