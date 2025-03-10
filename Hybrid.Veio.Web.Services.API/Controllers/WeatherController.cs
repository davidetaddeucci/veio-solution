using Hybrid.Veio.Web.Services.API.Models.Weather;
using Hybrid.Veio.Web.Services.API.Services.Weather;
using Microsoft.AspNetCore.Mvc;

namespace Hybrid.Veio.Web.Services.API.Controllers;

/// <summary>
/// Controller per le operazioni relative alle previsioni meteo
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(
        IWeatherService weatherService,
        ILogger<WeatherController> logger)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ottiene le previsioni meteo correnti per una località specifica
    /// </summary>
    /// <param name="location">Nome della città o coordinate (lat,lon)</param>
    /// <returns>Dati meteo correnti</returns>
    [HttpGet("current")]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentWeather([FromQuery] string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            return BadRequest("Il parametro 'location' è obbligatorio");

        try
        {
            var result = await _weatherService.GetCurrentWeatherAsync(location);
            return Ok(result);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Località non trovata o errore nell'API: {Location}", location);
            return NotFound($"Impossibile trovare dati meteo per la località: {location}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero dei dati meteo per {Location}", location);
            return StatusCode(StatusCodes.Status500InternalServerError, "Si è verificato un errore durante l'elaborazione della richiesta");
        }
    }

    /// <summary>
    /// Ottiene le previsioni meteo per i giorni specificati
    /// </summary>
    /// <param name="request">Richiesta contenente località e parametri aggiuntivi</param>
    /// <returns>Previsioni meteo dettagliate</returns>
    [HttpPost("forecast")]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetForecast([FromBody] WeatherRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Location))
            return BadRequest("La richiesta è invalida o il parametro 'location' è mancante");

        try
        {
            var result = await _weatherService.GetForecastAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Località non trovata o errore nell'API: {Location}", request.Location);
            return NotFound($"Impossibile trovare dati meteo per la località: {request.Location}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero delle previsioni meteo per {Location}", request.Location);
            return StatusCode(StatusCodes.Status500InternalServerError, "Si è verificato un errore durante l'elaborazione della richiesta");
        }
    }

    /// <summary>
    /// Ottiene le previsioni meteo per i giorni specificati tramite GET
    /// </summary>
    /// <param name="location">Nome della città o coordinate</param>
    /// <param name="days">Numero di giorni di previsione (1-10)</param>
    /// <returns>Previsioni meteo dettagliate</returns>
    [HttpGet("forecast")]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetForecastGet(
        [FromQuery] string location,
        [FromQuery] int days = 3,
        [FromQuery] bool airQuality = false,
        [FromQuery] bool alerts = false)
    {
        if (string.IsNullOrWhiteSpace(location))
            return BadRequest("Il parametro 'location' è obbligatorio");

        try
        {
            var request = new WeatherRequest
            {
                Location = location,
                Days = days,
                AirQuality = airQuality,
                Alerts = alerts
            };
            
            var result = await _weatherService.GetForecastAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Località non trovata o errore nell'API: {Location}", location);
            return NotFound($"Impossibile trovare dati meteo per la località: {location}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero delle previsioni meteo per {Location}", location);
            return StatusCode(StatusCodes.Status500InternalServerError, "Si è verificato un errore durante l'elaborazione della richiesta");
        }
    }

    /// <summary>
    /// Cerca località in base al testo inserito
    /// </summary>
    /// <param name="query">Testo di ricerca</param>
    /// <returns>Lista di località corrispondenti</returns>
    [HttpGet("locations")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchLocations([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
            return BadRequest("La query di ricerca deve contenere almeno 3 caratteri");

        try
        {
            var locations = await _weatherService.SearchLocationsAsync(query);
            return Ok(locations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la ricerca delle località per la query {Query}", query);
            return StatusCode(StatusCodes.Status500InternalServerError, "Si è verificato un errore durante l'elaborazione della richiesta");
        }
    }

    /// <summary>
    /// API di esempio per compatibilità - restituisce dati meteo casuali
    /// </summary>
    [HttpGet("legacy")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = WeatherForecast.Summaries[Random.Shared.Next(WeatherForecast.Summaries.Length)]
        })
        .ToArray();
    }
}