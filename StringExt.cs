using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
 
// helper local (extension)
static class StringExt
{
    public static string Truncate(this string s, int max) =>
        string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "...";
}
 
public class MeteoService
{
    private readonly HttpClient _httpClient;
 
    public MeteoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
 
    // Diagnostic amélioré : retourne plus d'informations pour comprendre pourquoi l'API
    // ne renvoie pas la météo. Conserve la signature existante.
    public async Task<(bool ok, string details)> PingAsync(CancellationToken ct = default)
    {
        var url = "https://api.open-meteo.com/v1/forecast?latitude=43.5&longitude=6.2&current_weather=true&timezone=UTC";
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            // Ajout d'un User-Agent explicite — parfois utile si le serveur rejette les clients sans UA.
            req.Headers.UserAgent.ParseAdd("wmine-dev/1.0 (+https://example.local)");
 
            using var resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            sw.Stop();
            var code = (int)resp.StatusCode;
 
            // Récupérer le corps (toujours) pour diagnostiquer
            string body;
            try
            {
                body = await resp.Content.ReadAsStringAsync(ct);
            }
            catch
            {
                body = "<impossible de lire le corps de la réponse>";
            }
 
            // Si non succès, fournir le body pour debug
            if (!resp.IsSuccessStatusCode)
            {
                var headers = HeadersToString(resp);
                return (false, $"HTTP {code} en {sw.ElapsedMilliseconds} ms. Headers: {headers} Réponse: {body.Truncate(1000)}");
            }
 
            // Succès : vérifier présence du champ attendu et parser la température
            bool hasCurrent = body?.IndexOf("current_weather", StringComparison.OrdinalIgnoreCase) >= 0;
            double? temp = null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("current_weather", out var cur) && cur.ValueKind == JsonValueKind.Object)
                {
                    if (cur.TryGetProperty("temperature", out var t) && t.ValueKind == JsonValueKind.Number)
                        temp = t.GetDouble();
                    else if (cur.TryGetProperty("temp", out var t2) && t2.ValueKind == JsonValueKind.Number)
                        temp = t2.GetDouble();
                }
            }
            catch (JsonException)
            {
                // ignore parsing error — on retournera le body pour diagnostic
            }
 
            if (!hasCurrent)
            {
                return (false, $"HTTP {code} en {sw.ElapsedMilliseconds} ms. Payload ne contient pas 'current_weather'. Réponse: {body.Truncate(1000)}");
            }
 
            var details = new StringBuilder();
            details.Append($"HTTP {code} en {sw.ElapsedMilliseconds} ms. PayloadOk=true");
            if (temp.HasValue)
                details.Append($". Température={temp:0.0} °C");
            else
                details.Append(". Température non trouvée dans 'current_weather'.");
 
            return (true, details.ToString());
        }
        catch (TaskCanceledException)
        {
            sw.Stop();
            return (false, $"Timeout après {sw.ElapsedMilliseconds} ms");
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            return (false, $"Erreur réseau ({ex.GetType().Name}): {ex.Message}");
        }
        catch (Exception ex)
        {
            sw.Stop();
            return (false, $"Erreur inattendue ({ex.GetType().Name}): {ex.Message}");
        }
    }
 
    private static string HeadersToString(HttpResponseMessage resp)
    {
        try
        {
            var sb = new StringBuilder();
            foreach (var h in resp.Headers)
                sb.Append($"{h.Key}=[{string.Join(",", h.Value)}]; ");
            foreach (var h in resp.Content.Headers)
                sb.Append($"{h.Key}=[{string.Join(",", h.Value)}]; ");
            return sb.ToString().Truncate(500);
        }
        catch
        {
            return "<impossible de récupérer les headers>";
        }
    }
}