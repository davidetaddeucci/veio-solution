using Hybrid.Veio.Web.Services.API.Models.Weather;
using Hybrid.Veio.Web.Services.API.Services.Weather;
using Microsoft.AspNetCore.Mvc;

namespace Hybrid.Veio.Web.Services.API.Controllers;

/// <summary>
/// Controller esteso per le operazioni relative alle previsioni meteo
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherExtendedController : ControllerBase
{
    private readonly IWeatherServiceExtended _weatherService;
    private readonly ILogger<WeatherExtendedController> _logger;

    public WeatherExtendedController(
        IWeatherServiceExtended weatherService,
        ILogger<WeatherExtendedController> logger)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ottiene le previsioni meteo per un'area geografica definita da coordinate
    /// </summary>
    /// <param name="request">Richiesta con i dettagli dell'area e le opzioni di previsione</param>
    /// <returns>Previsioni meteo aggregate per l'area specificata</returns>
    [HttpPost("area-forecast")]
    [ProducesResponseType(typeof(WeatherAreaForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAreaForecast([FromBody] WeatherAreaRequest request)
    {
        if (request == null)
            return BadRequest("La richiesta è invalida");
            
        // Verifica che le coordinate formino un rettangolo valido
        if (!IsValidRectangle(request.LatitudeTopRight, request.LongitudeTopRight, 
                             request.LatitudeBottomLeft, request.LongitudeBottomLeft))
        {
            return BadRequest("Le coordinate non formano un rettangolo valido. " +
                              "Assicurati che LatitudeTopRight > LatitudeBottomLeft e " +
                              "LongitudeTopRight > LongitudeBottomLeft");
        }
        
        try
        {
            var result = await _weatherService.GetAreaForecastAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Errore durante il recupero delle previsioni per l'area");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero delle previsioni per l'area");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "Si è verificato un errore durante l'elaborazione della richiesta");
        }
    }
    
    /// <summary>
    /// Ottiene le previsioni meteo per un'area geografica definita da coordinate tramite GET
    /// </summary>
    /// <param name="latTopRight">Latitudine dell'angolo superiore destro</param>
    /// <param name="lonTopRight">Longitudine dell'angolo superiore destro</param>
    /// <param name="latBottomLeft">Latitudine dell'angolo inferiore sinistro</param>
    /// <param name="lonBottomLeft">Longitudine dell'angolo inferiore sinistro</param>
    /// <param name="days">Giorni di previsione (1-14)</param>
    /// <param name="samplingPoints">Punti di campionamento (1-25)</param>
    /// <returns>Previsioni meteo aggregate per l'area specificata</returns>
    [HttpGet("area-forecast")]
    [ProducesResponseType(typeof(WeatherAreaForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAreaForecastGet(
        [FromQuery] double latTopRight,
        [FromQuery] double lonTopRight,
        [FromQuery] double latBottomLeft,
        [FromQuery] double lonBottomLeft,
        [FromQuery] int days = 3,
        [FromQuery] int samplingPoints = 5,
        [FromQuery] bool airQuality = false,
        [FromQuery] bool alerts = false)
    {
        // Verifica che le coordinate formino un rettangolo valido
        if (!IsValidRectangle(latTopRight, lonTopRight, latBottomLeft, lonBottomLeft))
        {
            return BadRequest("Le coordinate non formano un rettangolo valido. " +
                              "Assicurati che latTopRight > latBottomLeft e " +
                              "lonTopRight > lonBottomLeft");
        }
        
        try
        {
            var request = new WeatherAreaRequest
            {
                LatitudeTopRight = latTopRight,
                LongitudeTopRight = lonTopRight,
                LatitudeBottomLeft = latBottomLeft,
                LongitudeBottomLeft = lonBottomLeft,
                Days = days,
                SamplingPoints = samplingPoints,
                AirQuality = airQuality,
                Alerts = alerts
            };
            
            var result = await _weatherService.GetAreaForecastAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Errore durante il recupero delle previsioni per l'area");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero delle previsioni per l'area");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "Si è verificato un errore durante l'elaborazione della richiesta");
        }
    }

    /// <summary>
    /// Ottiene dati meteorologici storici per una località in un intervallo di date
    /// </summary>
    /// <param name="location">Nome della città o coordinate</param>
    /// <param name="startDate">Data di inizio (formato yyyy-MM-dd)</param>
    /// <param name="endDate">Data di fine (formato yyyy-MM-dd)</param>
    /// <returns>Dati storici per il periodo specificato</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(WeatherHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHistoricalData(
        [FromQuery] string location,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(location))
            return BadRequest("Il parametro 'location' è obbligatorio");
            
        if (startDate > endDate)
            return BadRequest("La data di inizio deve essere precedente o uguale alla data di fine");
            
        if (endDate > DateTime.Today)
            return BadRequest("La data di fine non può essere nel futuro");
        
        try
        {
            var result = await _weatherService.GetHistoricalDataAsync(location, startDate, endDate);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Località non trovata o errore nell'API: {Location}", location);
            return NotFound($"Impossibile trovare dati storici per la località: {location}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero dei dati storici per {Location}", location);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "Si è verificato un errore durante l'elaborazione della richiesta");
        }
    }
    
    #region Helper Methods
    
    /// <summary>
    /// Verifica che le coordinate formino un rettangolo valido
    /// </summary>
    private bool IsValidRectangle(double latTopRight, double lonTopRight, 
                                 double latBottomLeft, double lonBottomLeft)
    {
        // Verifica che le coordinate siano valide
        if (!IsValidCoordinates(latTopRight, lonTopRight) || 
            !IsValidCoordinates(latBottomLeft, lonBottomLeft))
            return false;
            
        // In un rettangolo su una mappa, l'angolo superiore destro deve essere "sopra" e "a destra"
        // rispetto all'angolo inferiore sinistro
        return latTopRight >= latBottomLeft && lonTopRight >= lonBottomLeft;
    }
    
    /// <summary>
    /// Verifica che le coordinate siano valide
    /// </summary>
    private bool IsValidCoordinates(double latitude, double longitude)
    {
        return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
    }
    
    #endregion
}