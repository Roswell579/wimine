using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace wmine.Forms
{
    public sealed class MineralAiForm : Form
    {
        private readonly WebView2 _web;
        private readonly Label _statusLabel;
        private readonly Button _retryButton;
        private TaskCompletionSource<List<(string Label, float Prob)>>? _tcs;
        private TaskCompletionSource<bool>? _modelReadyTcs;
        private bool _modelReady = false;

        public MineralAiForm()
        {
            Text = "IA Minéraux";
            Width = 900;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(30, 30, 40);

            _statusLabel = new Label
            {
                Text = "⏳ Chargement du modèle IA...",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(50, 50, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            _retryButton = new Button
            {
                Text = "🔄 Réessayer le chargement",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Visible = false,
                Cursor = Cursors.Hand
            };
            _retryButton.FlatAppearance.BorderSize = 0;
            _retryButton.Click += async (s, e) =>
            {
                _retryButton.Visible = false;
                _modelReady = false;
                await EnsureReadyAsync();
            };

            _web = new WebView2 { Dock = DockStyle.Fill };
            Controls.Add(_web);
            Controls.Add(_statusLabel);
            Controls.Add(_retryButton);
            _statusLabel.BringToFront();
            _retryButton.BringToFront();

            // Précharger le modèle dès l'ouverture
            this.Shown += async (s, e) =>
            {
                try
                {
                    await EnsureReadyAsync();
                }
                catch (TimeoutException)
                {
                    _retryButton.Visible = true;
                    _statusLabel.Text = "❌ Timeout: Connexion internet lente ou CDN inaccessible";
                }
                catch (Exception ex)
                {
                    _retryButton.Visible = true;
                    _statusLabel.Text = $"❌ Erreur: {ex.Message}";
                }
            };
        }

        private async Task EnsureReadyAsync()
        {
            if (_web.CoreWebView2 != null && _modelReady) return;

            try
            {
                if (_web.CoreWebView2 == null)
                {
                    _statusLabel.Text = "⏳ Initialisation WebView2...";
                    await _web.EnsureCoreWebView2Async();

                    // Activer la console pour debug
                    _web.CoreWebView2.Settings.AreDevToolsEnabled = true;

                    _web.CoreWebView2.WebMessageReceived += WebMessageReceived;

                    var script = """
                        (function(){
                          const html = `<html><head>
                            <meta charset='utf-8'/>
                            <style>
                              body{font-family:'Segoe UI',Arial;color:#eee;background:#1e1e25;margin:20px;padding:20px}
                              h3{color:#4CAF50;margin-bottom:10px}
                              p{color:#aaa;font-size:14px}
                              #status{color:#4CAF50;font-weight:bold;margin-top:20px;font-size:16px}
                              .spinner{display:inline-block;width:20px;height:20px;border:3px solid rgba(255,255,255,.3);border-radius:50%;border-top-color:#4CAF50;animation:spin 1s ease-in-out infinite}
                              @keyframes spin{to{transform:rotate(360deg)}}
                            </style>
                          </head>
                          <body>
                            <h3>🤖 MobileNet - Classification d'images</h3>
                            <p>📡 Tentative de chargement depuis plusieurs CDN...</p>
                            <p>✅ Aucune clé API requise</p>
                            <div id='status'><span class='spinner'></span> Chargement du modèle...</div>
                            <p id='debug' style='color:#888;font-size:12px;margin-top:10px'></p>
                            <img id='img' style='max-width:100%;display:none'/>
                            <script>
                              function updateDebug(msg) {
                                const d = document.getElementById('debug');
                                d.innerHTML += '<br>' + new Date().toLocaleTimeString() + ': ' + msg;
                                console.log(msg);
                              }
                              
                              // Liste des CDN à essayer
                              const cdnOptions = [
                                {
                                  name: 'UNPKG',
                                  tf: 'https://unpkg.com/@tensorflow/tfjs@4.13.0/dist/tf.min.js',
                                  mobilenet: 'https://unpkg.com/@tensorflow-models/mobilenet@2.1.0'
                                },
                                {
                                  name: 'jsDelivr',
                                  tf: 'https://cdn.jsdelivr.net/npm/@tensorflow/tfjs@4.13.0/dist/tf.min.js',
                                  mobilenet: 'https://cdn.jsdelivr.net/npm/@tensorflow-models/mobilenet@2.1.0'
                                },
                                {
                                  name: 'cdnjs',
                                  tf: 'https://cdnjs.cloudflare.com/ajax/libs/tensorflow/4.13.0/tf.min.js',
                                  mobilenet: 'https://cdn.jsdelivr.net/npm/@tensorflow-models/mobilenet@2.1.0'
                                }
                              ];
                              
                              let currentCdnIndex = 0;
                              let modelReady = false;
                              let modelPromise = null;
                              
                              async function loadScriptDynamic(src) {
                                return new Promise((resolve, reject) => {
                                  const script = document.createElement('script');
                                  script.src = src;
                                  script.onload = resolve;
                                  script.onerror = reject;
                                  document.head.appendChild(script);
                                });
                              }
                              
                              async function tryLoadModel() {
                                if (currentCdnIndex >= cdnOptions.length) {
                                  updateDebug('❌ Tous les CDN ont échoué');
                                  document.getElementById('status').innerHTML = '❌ Impossible de charger le modèle depuis aucun CDN';
                                  chrome.webview.postMessage(JSON.stringify({type:'error', message:'Tous les CDN ont échoué'}));
                                  return;
                                }
                                
                                const cdn = cdnOptions[currentCdnIndex];
                                updateDebug(`🔄 Tentative avec ${cdn.name}...`);
                                document.getElementById('status').innerHTML = `<span class="spinner"></span> Tentative ${currentCdnIndex + 1}/${cdnOptions.length}: ${cdn.name}...`;
                                
                                try {
                                  // Charger TensorFlow.js
                                  await loadScriptDynamic(cdn.tf);
                                  updateDebug(`✅ TensorFlow.js chargé depuis ${cdn.name}`);
                                  
                                  // Charger MobileNet
                                  await loadScriptDynamic(cdn.mobilenet);
                                  updateDebug(`✅ MobileNet chargé depuis ${cdn.name}`);
                                  
                                  // Initialiser le modèle
                                  updateDebug('📥 Téléchargement des poids du modèle...');
                                  modelPromise = mobilenet.load({version:2, alpha:1.0});
                                  await modelPromise;
                                  
                                  modelReady = true;
                                  updateDebug(`✅ Modèle prêt ! (CDN: ${cdn.name})`);
                                  document.getElementById('status').innerHTML = `✅ Modèle prêt ! (${cdn.name})`;
                                  chrome.webview.postMessage(JSON.stringify({type:'ready'}));
                                  
                                } catch(err) {
                                  updateDebug(`❌ Échec avec ${cdn.name}: ${err.message}`);
                                  currentCdnIndex++;
                                  setTimeout(() => tryLoadModel(), 1000);
                                }
                              }
                              
                              // Démarrer le chargement
                              tryLoadModel();
                              
                              async function classifyDataUrl(dataUrl){
                                try {
                                  if (!modelReady) {
                                    throw new Error('Le modèle n\'est pas encore chargé. Veuillez patienter.');
                                  }
                                  updateDebug('Début de l\'analyse...');
                                  document.getElementById('status').innerHTML = '<span class="spinner"></span> Analyse en cours...';
                                  const img = document.getElementById('img');
                                  img.src = dataUrl;
                                  await new Promise(r=>img.onload=r);
                                  const model = await modelPromise;
                                  updateDebug('Classification en cours...');
                                  const preds = await model.classify(img, 5);
                                  updateDebug('✅ Classification terminée !');
                                  document.getElementById('status').innerHTML = '✅ Classification terminée !';
                                  chrome.webview.postMessage(JSON.stringify({type:'result', data:preds}));
                                } catch(err) {
                                  updateDebug('❌ Erreur: ' + err.message);
                                  document.getElementById('status').innerHTML = '❌ Erreur: ' + err.message;
                                  chrome.webview.postMessage(JSON.stringify({type:'error', message:err.message}));
                                }
                              }
                              
                              chrome.webview.addEventListener('message', e => classifyDataUrl(e.data));
                            <\/script>
                          </body></html>`;
                          document.open();document.write(html);document.close();
                        })();
                        """;

                    await _web.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);

                    _web.CoreWebView2.NavigateToString("<html><body style='background:#1e1e25;color:#eee'>⏳ Initialisation IA…</body></html>");
                }

                if (!_modelReady)
                {
                    _statusLabel.Text = "⏳ Téléchargement du modèle MobileNet (~20 MB, peut prendre 1-2 min)...";
                    _modelReadyTcs = new TaskCompletionSource<bool>();

                    // Attendre que le modèle soit prêt avec un timeout de 120 secondes
                    var timeoutTask = Task.Delay(120000);
                    var completedTask = await Task.WhenAny(_modelReadyTcs.Task, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        _statusLabel.Text = "❌ Timeout: Le modèle n'a pas pu se charger";
                        throw new TimeoutException("Le modèle IA n'a pas pu se charger dans les 120 secondes. Vérifiez votre connexion internet (le modèle fait ~20 MB).");
                    }

                    _modelReady = true;
                    _statusLabel.Text = "✅ Modèle IA prêt !";
                }
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"❌ Erreur: {ex.Message}";
                throw;
            }
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var json = e.TryGetWebMessageAsString();
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Vérifier le type de message
                if (root.TryGetProperty("type", out var typeElement))
                {
                    var msgType = typeElement.GetString();

                    if (msgType == "ready")
                    {
                        _statusLabel.Text = "✅ Modèle IA chargé avec succès";
                        _modelReady = true;
                        _modelReadyTcs?.TrySetResult(true);
                        return;
                    }

                    if (msgType == "error")
                    {
                        var errorMsg = root.GetProperty("message").GetString() ?? "Erreur inconnue";
                        _modelReadyTcs?.TrySetException(new Exception($"Erreur IA: {errorMsg}"));
                        _tcs?.TrySetException(new Exception($"Erreur IA: {errorMsg}"));
                        _statusLabel.Text = $"❌ Erreur: {errorMsg}";
                        return;
                    }

                    if (msgType == "result" && _tcs != null)
                    {
                        var list = new List<(string Label, float Prob)>();
                        foreach (var item in root.GetProperty("data").EnumerateArray())
                        {
                            list.Add((
                                item.GetProperty("className").GetString() ?? "inconnu",
                                item.GetProperty("probability").GetSingle()
                            ));
                        }
                        _tcs.TrySetResult(list);
                        _statusLabel.Text = $"✅ {list.Count} résultat(s) trouvé(s)";
                    }
                }
            }
            catch (Exception ex)
            {
                _modelReadyTcs?.TrySetException(ex);
                _tcs?.TrySetException(ex);
                _statusLabel.Text = $"❌ Erreur de traitement: {ex.Message}";
            }
        }

        public async Task<List<(string Label, float Prob)>> ClassifyAsync(byte[] imageBytes, string mime = "image/jpeg")
        {
            // Attendre que le modèle soit prêt
            await EnsureReadyAsync();

            if (!_modelReady)
            {
                throw new InvalidOperationException("Le modèle IA n'est pas prêt. Cliquez sur 'Réessayer le chargement'.");
            }

            _statusLabel.Text = "⏳ Analyse de l'image...";
            _tcs = new TaskCompletionSource<List<(string Label, float Prob)>>();

            var base64 = Convert.ToBase64String(imageBytes);
            _web.CoreWebView2.PostWebMessageAsString($"data:{mime};base64,{base64}");

            // Timeout de 30 secondes pour l'analyse (le modèle est déjà chargé)
            var timeoutTask = Task.Delay(30000);
            var completedTask = await Task.WhenAny(_tcs.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                _statusLabel.Text = "❌ Timeout de l'analyse";
                throw new TimeoutException("L'analyse de l'image a pris trop de temps (>30s).");
            }

            return await _tcs.Task;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _web?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
