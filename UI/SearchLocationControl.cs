using wmine.Services;

namespace wmine.UI
{
    /// <summary>
    /// Contréle de recherche géographique avec autocomplétion
    /// </summary>
    public class SearchLocationControl : Panel
    {
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnHistory;
        private ListBox lstResults;
        private readonly GeocodingService _geocodingService;
        private System.Windows.Forms.Timer _searchTimer;
        private static readonly List<string> _searchHistory = new List<string>();
        private const int MAX_HISTORY = 10;

        public event EventHandler<GeocodingResult>? LocationSelected;

        public SearchLocationControl()
        {
            _geocodingService = new GeocodingService();
            LoadSearchHistory();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(350, 300);
            this.BackColor = Color.FromArgb(200, 30, 35, 45);
            this.Visible = false;

            // Zone de texte
            txtSearch = new TextBox
            {
                Location = new Point(10, 10),
                Width = 240,
                Height = 30,
                Font = new Font("Segoe UI Emoji", 11),
                BackColor = Color.FromArgb(45, 50, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.KeyDown += TxtSearch_KeyDown;

            // Bouton recherche
            btnSearch = new Button
            {
                Text = "??",
                Location = new Point(260, 10),
                Width = 40,
                Height = 30,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += BtnSearch_Click;

            // Bouton historique
            btnHistory = new Button
            {
                Text = "??",
                Location = new Point(310, 10),
                Width = 30,
                Height = 30,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnHistory.FlatAppearance.BorderSize = 0;
            btnHistory.Click += BtnHistory_Click;

            var tooltip = new ToolTip();
            tooltip.SetToolTip(btnHistory, "Afficher l'historique des recherches");

            // Liste de résultats
            lstResults = new ListBox
            {
                Location = new Point(10, 50),
                Width = 330,
                Height = 240,
                BackColor = Color.FromArgb(35, 40, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 9),
                BorderStyle = BorderStyle.FixedSingle
            };
            lstResults.DoubleClick += LstResults_DoubleClick;
            lstResults.KeyDown += LstResults_KeyDown;

            // Timer pour éviter trop de requétes
            _searchTimer = new System.Windows.Forms.Timer
            {
                Interval = 800 // 800ms aprés la derniére frappe
            };
            _searchTimer.Tick += SearchTimer_Tick;

            this.Controls.Add(txtSearch);
            this.Controls.Add(btnSearch);
            this.Controls.Add(btnHistory);
            this.Controls.Add(lstResults);
        }

        private void BtnHistory_Click(object? sender, EventArgs e)
        {
            lstResults.Items.Clear();

            if (_searchHistory.Count == 0)
            {
                lstResults.Items.Add("Aucun historique");
            }
            else
            {
                lstResults.Items.Add("Recherches récentes:");
                foreach (var query in _searchHistory.Take(MAX_HISTORY))
                {
                    lstResults.Items.Add($"  é {query}");
                }
                lstResults.Items.Add("");
                lstResults.Items.Add("Double-cliquez pour rechercher");
            }
        }

        private void LoadSearchHistory()
        {
            try
            {
                var historyPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "wmine", "search_history.txt");

                if (File.Exists(historyPath))
                {
                    _searchHistory.Clear();
                    _searchHistory.AddRange(File.ReadAllLines(historyPath).Take(MAX_HISTORY));
                }
            }
            catch { }
        }

        private void SaveSearchHistory()
        {
            try
            {
                var historyPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "wmine", "search_history.txt");

                Directory.CreateDirectory(Path.GetDirectoryName(historyPath)!);
                File.WriteAllLines(historyPath, _searchHistory.Take(MAX_HISTORY));
            }
            catch { }
        }

        private void AddToHistory(string query)
        {
            query = query.Trim();
            if (string.IsNullOrWhiteSpace(query))
                return;

            // Supprimer si déjé présent
            _searchHistory.Remove(query);

            // Ajouter en premier
            _searchHistory.Insert(0, query);

            // Limiter é MAX_HISTORY
            while (_searchHistory.Count > MAX_HISTORY)
            {
                _searchHistory.RemoveAt(_searchHistory.Count - 1);
            }

            SaveSearchHistory();
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                PerformSearch();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Visible = false;
            }
            else if (e.KeyCode == Keys.Down && lstResults.Items.Count > 0)
            {
                lstResults.Focus();
                lstResults.SelectedIndex = 0;
            }
        }

        private void LstResults_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && lstResults.SelectedItem is GeocodingResult result)
            {
                SelectLocation(result);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Visible = false;
            }
        }

        private async void SearchTimer_Tick(object? sender, EventArgs e)
        {
            _searchTimer.Stop();
            await PerformSearchAsync();
        }

        private async void BtnSearch_Click(object? sender, EventArgs e)
        {
            await PerformSearchAsync();
        }

        private async Task PerformSearchAsync()
        {
            var query = txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(query))
                return;

            try
            {
                btnSearch.Enabled = false;
                btnSearch.Text = "?";
                lstResults.Items.Clear();
                lstResults.Items.Add("Recherche en cours...");

                var results = await _geocodingService.SearchPlaceAsync(query);

                lstResults.Items.Clear();

                if (results.Count == 0)
                {
                    lstResults.Items.Add("Aucun résultat trouvé.");
                }
                else
                {
                    // Ajouter é l'historique si résultats trouvés
                    AddToHistory(query);

                    foreach (var result in results)
                    {
                        lstResults.Items.Add(result);
                    }
                }
            }
            catch (Exception ex)
            {
                lstResults.Items.Clear();
                lstResults.Items.Add($"Erreur: {ex.Message}");
            }
            finally
            {
                btnSearch.Text = "??";
                btnSearch.Enabled = true;
            }
        }

        private void PerformSearch()
        {
            _ = PerformSearchAsync();
        }

        private void LstResults_DoubleClick(object? sender, EventArgs e)
        {
            if (lstResults.SelectedItem is GeocodingResult result)
            {
                SelectLocation(result);
            }
            else if (lstResults.SelectedItem is string historyItem && historyItem.StartsWith("  é "))
            {
                // Relancer la recherche depuis l'historique
                var query = historyItem.Replace("  é ", "").Trim();
                txtSearch.Text = query;
                _ = PerformSearchAsync();
            }
        }

        private void SelectLocation(GeocodingResult result)
        {
            LocationSelected?.Invoke(this, result);
            this.Visible = false;
        }

        public void Show(Point location)
        {
            this.Location = location;
            this.Visible = true;
            this.BringToFront();
            txtSearch.Focus();
            txtSearch.SelectAll();
        }

        public new void Hide()
        {
            this.Visible = false;
        }
    }
}

