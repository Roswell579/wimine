using System.Reflection;

namespace wmine.Forms
{
    public partial class Form1 : Form
    {
        public event EventHandler? FilonsRefreshRequested;

        public void RefreshFilonsList()
        {
            if (IsDisposed) return;
            try
            {
                FilonsRefreshRequested?.Invoke(this, EventArgs.Empty);
                foreach (var ctrl in GetAllControls(this))
                {
                    if (ctrl is MineralsPanel)
                    {
                        var mi = ctrl.GetType().GetMethod("LoadMinerals", BindingFlags.Instance | BindingFlags.NonPublic);
                        mi?.Invoke(ctrl, null);
                    }
                    var reload =
                        ctrl.GetType().GetMethod("Reload", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
                        ctrl.GetType().GetMethod("RefreshList", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
                        ctrl.GetType().GetMethod("LoadData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    reload?.Invoke(ctrl, null);
                }
                Invalidate(true);
                Update();
            }
            catch { }
        }

        private IEnumerable<Control> GetAllControls(Control root)
        {
            foreach (Control c in root.Controls)
            {
                yield return c;
                foreach (var child in GetAllControls(c))
                    yield return child;
            }
        }

        private async void BtnIa_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Images|*.jpg;*.jpeg;*.png" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var bytes = File.ReadAllBytes(ofd.FileName);
                using var iaForm = new MineralAiForm();
                iaForm.Show();

                var mime = Path.GetExtension(ofd.FileName).ToLowerInvariant() == ".png" ? "image/png" : "image/jpeg";
                var preds = await iaForm.ClassifyAsync(bytes, mime);
                iaForm.Close();

                var msg = string.Join(Environment.NewLine, preds.Select(p => $"{p.Label} - {p.Prob * 100f:F1}%"));
                MessageBox.Show($"Résultats IA:\n\n{msg}", "Classification IA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur IA: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
