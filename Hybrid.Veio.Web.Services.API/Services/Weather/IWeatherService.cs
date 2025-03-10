using Hybrid.Veio.Web.Services.API.Models.Weather;

namespace Hybrid.Veio.Web.Services.API.Services.Weather;

/// <summary>
/// Interfaccia per il servizio di previsioni meteorologiche
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Ottiene le previsioni del tempo correnti per una località specifica
    /// </summary>
    /// <param name="location">Nome della città o coordinate (lat,lon)</param>
    /// <returns>Dati meteo correnti</returns>
    Task<WeatherForecastDto> GetCurrentWeatherAsync(string location);
    
    /// <summary>
    /// Ottiene le previsioni del tempo per i giorni specificati
    /// </summary>
    /// <param name="request">Richiesta contenente località e parametri aggiuntivi</param>
    /// <returns>Previsioni meteo dettagliate</returns>
    Task<WeatherForecastDto> GetForecastAsync(WeatherRequest request);
    
    /// <summary>
    /// Cerca località in base al testo inserito
    /// </summary>
    /// <param name="query">Testo di ricerca</param>
    /// <returns>Lista di località corrispondenti</returns>
    Task<IEnumerable<string>> SearchLocationsAsync(string query);
}