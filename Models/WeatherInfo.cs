namespace wmine.Models
{
    /// <summary>
    /// Informations m�t�orologiques pour un lieu donn�
    /// </summary>
    public class WeatherInfo
    {
        /// <summary>
        /// Temp�rature en degr�s Celsius
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// Temp�rature ressentie en degr�s Celsius
        /// </summary>
        public decimal FeelsLike { get; set; }

        /// <summary>
        /// Humidit� relative en pourcentage (0-100)
        /// </summary>
        public int Humidity { get; set; }

        /// <summary>
        /// Vitesse du vent en km/h
        /// </summary>
        public decimal WindSpeed { get; set; }

        /// <summary>
        /// Direction du vent en degr�s (0-360)
        /// </summary>
        public int WindDirection { get; set; }

        /// <summary>
        /// Description textuelle des conditions m�t�o (fran�ais)
        /// </summary>
        public string Conditions { get; set; } = string.Empty;

        /// <summary>
        /// Emoji repr�sentant les conditions m�t�o
        /// </summary>
        public string Icon { get; set; } = "???";

        /// <summary>
        /// Date et heure de la derni�re mise � jour
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Code m�t�o WMO (World Meteorological Organization)
        /// </summary>
        public int WeatherCode { get; set; }

        /// <summary>
        /// Retourne une description format�e de la m�t�o
        /// </summary>
        public string GetFormattedDescription()
        {
            return $"{Icon} {Conditions}\n" +
                   $"??? {Temperature:F1}�C (ressenti {FeelsLike:F1}�C)\n" +
                   $"?? Humidit�: {Humidity}%\n" +
                   $"?? Vent: {WindSpeed:F1} km/h";
        }

        /// <summary>
        /// Retourne une description courte de la m�t�o
        /// </summary>
        public string GetShortDescription()
        {
            return $"{Icon} {Temperature:F1}�C - {Conditions}";
        }
    }
}
