namespace Hybrid.Veio.Web.Services.API.Services.Weather;

/// <summary>
/// Opzioni di configurazione per il servizio WeatherAPI
/// </summary>
public class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";
    
    /// <summary>
    /// Chiave API per WeatherAPI.com
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Timeout in secondi per le richieste HTTP (default: 30 secondi)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Numero massimo di tentativi per le richieste HTTP (default: 3)
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}