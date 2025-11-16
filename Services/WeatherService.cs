using System.Net.Http;
using System.Text.Json;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service de m�t�o utilisant l'API Open-Meteo (gratuite, sans cl� API)
    /// Documentation: https://open-meteo.com/en/docs
    /// </summary>
    public class WeatherService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private const string OPEN_METEO_API = "https://api.open-meteo.com/v1/forecast";

        // Cache pour �viter les requ�tes trop fr�quentes
        private static readonly Dictionary<string, (DateTime timestamp, WeatherInfo? weather)> _cache
            = new Dictionary<string, (DateTime, WeatherInfo?)>();
        private static readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        static WeatherService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WMine-FilonLocator/1.0");
        }

        /// <summary>
        /// R�cup�re la m�t�o actuelle pour des coordonn�es GPS donn�es
        /// </summary>
        /// <param name="latitude">Latitude GPS</param>
        /// <param name="longitude">Longitude GPS</param>
        /// <returns>Informations m�t�o ou null en cas d'erreur</returns>
        public async Task<WeatherInfo?> GetCurrentWeatherAsync(decimal latitude, decimal longitude)
        {
            var cacheKey = $"{latitude:F4}_{longitude:F4}";

            // V�rifier le cache
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                if (DateTime.Now - cached.timestamp < _cacheExpiration)
                {
                    return cached.weather;
                }
                else
                {
                    _cache.Remove(cacheKey);
                }
            }

            try
            {
                var url = $"{OPEN_METEO_API}?latitude={latitude}&longitude={longitude}" +
                          $"&current_weather=true" +
                          $"&hourly=temperature_2m,relativehumidity_2m,apparent_temperature,windspeed_10m" +
                          $"&timezone=Europe/Paris";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<OpenMeteoResponse>(json);

                if (data?.current_weather == null)
                    return null;

                var weather = new WeatherInfo
                {
                    Temperature = (decimal)data.current_weather.temperature,
                    FeelsLike = (decimal)(data.hourly?.apparent_temperature?.FirstOrDefault() ?? data.current_weather.temperature),
                    Humidity = data.hourly?.relativehumidity_2m?.FirstOrDefault() ?? 0,
                    WindSpeed = (decimal)data.current_weather.windspeed,
                    WindDirection = data.current_weather.winddirection,
                    WeatherCode = data.current_weather.weathercode,
                    Conditions = GetConditionsFromCode(data.current_weather.weathercode),
                    Icon = GetWeatherEmoji(data.current_weather.weathercode),
                    Timestamp = DateTime.Now
                };

                // Mettre en cache
                _cache[cacheKey] = (DateTime.Now, weather);

                return weather;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur m�t�o: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Convertit le code m�t�o WMO en description fran�aise
        /// </summary>
        private string GetConditionsFromCode(int code)
        {
            return code switch
            {
                0 => "Ciel d�gag�",
                1 => "Principalement d�gag�",
                2 => "Partiellement nuageux",
                3 => "Couvert",
                45 => "Brouillard",
                48 => "Brouillard givrant",
                51 => "Bruine l�g�re",
                53 => "Bruine mod�r�e",
                55 => "Bruine dense",
                56 => "Bruine vergla�ante l�g�re",
                57 => "Bruine vergla�ante dense",
                61 => "Pluie faible",
                63 => "Pluie mod�r�e",
                65 => "Pluie forte",
                66 => "Pluie vergla�ante l�g�re",
                67 => "Pluie vergla�ante forte",
                71 => "Neige faible",
                73 => "Neige mod�r�e",
                75 => "Neige forte",
                77 => "Grains de neige",
                80 => "Averses faibles",
                81 => "Averses mod�r�es",
                82 => "Averses violentes",
                85 => "Averses de neige faibles",
                86 => "Averses de neige fortes",
                95 => "Orage",
                96 => "Orage avec gr�le l�g�re",
                99 => "Orage avec gr�le forte",
                _ => "Conditions inconnues"
            };
        }

        /// <summary>
        /// Retourne l'emoji correspondant au code m�t�o
        /// </summary>
        private string GetWeatherEmoji(int code)
        {
            return code switch
            {
                0 => "??",           // Ciel d�gag�
                1 or 2 => "???",     // Partiellement nuageux
                3 => "??",           // Couvert
                45 or 48 => "???",   // Brouillard
                51 or 53 or 55 or 56 or 57 => "???", // Bruine
                61 or 63 or 65 => "???", // Pluie
                66 or 67 => "???",   // Pluie vergla�ante
                71 or 73 or 75 or 77 => "???", // Neige
                80 or 81 or 82 => "???", // Averses
                85 or 86 => "???",   // Averses de neige
                95 or 96 or 99 => "??", // Orage
                _ => "???"
            };
        }

        /// <summary>
        /// Nettoie le cache (optionnel)
        /// </summary>
        public static void ClearCache()
        {
            _cache.Clear();
        }

        #region Mod�les de r�ponse Open-Meteo

        private class OpenMeteoResponse
        {
            public CurrentWeather? current_weather { get; set; }
            public HourlyData? hourly { get; set; }
        }

        private class CurrentWeather
        {
            public double temperature { get; set; }
            public double windspeed { get; set; }
            public int winddirection { get; set; }
            public int weathercode { get; set; }
            public string? time { get; set; }
        }

        private class HourlyData
        {
            public List<double>? temperature_2m { get; set; }
            public List<int>? relativehumidity_2m { get; set; }
            public List<double>? apparent_temperature { get; set; }
            public List<double>? windspeed_10m { get; set; }
        }

        #endregion
    }
}
