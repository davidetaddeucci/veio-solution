using Hybrid.Veio.Web.Services.API.Models.Weather;

namespace Hybrid.Veio.Web.Services.API.Services.Weather;

/// <summary>
/// Estensione dell'interfaccia del servizio meteo con nuove funzionalità
/// </summary>
public interface IWeatherServiceExtended : IWeatherService
{
    /// <summary>
    /// Ottiene le previsioni meteo per un'area geografica rettangolare
    /// </summary>
    /// <param name="request">Richiesta con parametri dell'area e della previsione</param>
    /// <returns>Previsioni aggregate per l'area</returns>
    Task<WeatherAreaForecastDto> GetAreaForecastAsync(WeatherAreaRequest request);
    
    /// <summary>
    /// Ottiene dati meteorologici storici per una località
    /// </summary>
    /// <param name="location">Località o coordinate</param>
    /// <param name="startDate">Data di inizio</param>
    /// <param name="endDate">Data di fine</param>
    /// <returns>Dati storici per il periodo specificato</returns>
    Task<WeatherHistoryDto> GetHistoricalDataAsync(string location, DateTime startDate, DateTime endDate);
}