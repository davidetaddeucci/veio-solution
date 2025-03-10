using System.Text.Json;
using Hybrid.Veio.Web.Services.API.Models.Weather;
using Microsoft.Extensions.Options;

namespace Hybrid.Veio.Web.Services.API.Services.Weather;

/// <summary>
/// Implementazione del servizio per le previsioni meteorologiche che utilizza WeatherAPI.com
/// </summary>
public class WeatherService : IWeatherService
{
    protected readonly HttpClient HttpClient;
    protected readonly WeatherApiOptions Options;
    protected readonly ILogger<WeatherService> Logger;

    protected string ApiKey => Options.ApiKey;

    public WeatherService(
        HttpClient httpClient,
        IOptions<WeatherApiOptions> options,
        ILogger<WeatherService> logger)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Configura l'URL base per l'API esterna
        HttpClient.BaseAddress = new Uri("https://api.weatherapi.com/v1/");
    }

    /// <inheritdoc />
    public async Task<WeatherForecastDto> GetCurrentWeatherAsync(string location)
    {
        try
        {
            // Costruisci l'URL con i parametri necessari
            var requestUri = $"current.json?key={ApiKey}&q={Uri.EscapeDataString(location)}&aqi=no";
            
            Logger.LogInformation("Richiesta API: {RequestUri}", requestUri);
            
            // Effettua la richiesta HTTP
            var response = await HttpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            
            // Leggi il contenuto della risposta come stringa per il log
            var contentString = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("Risposta API: {Response}", contentString);
            
            // Deserializza la risposta
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var weatherData = JsonSerializer.Deserialize<WeatherForecastResponse>(contentString, options);
            
            if (weatherData == null)
                throw new JsonException("Impossibile deserializzare i dati meteo");
                
            Logger.LogInformation("WeatherData deserializzato: Location={Location}, Temp={Temp}", 
                weatherData.Location.Name, 
                weatherData.Current.TemperatureC);
            
            // Converti i dati nel DTO
            var result = MapToWeatherForecastDto(weatherData);
            
            Logger.LogInformation("DTO mappato: Location={Location}, Temp={Temp}", 
                result.Location, 
                result.CurrentTemperature);
                
            return result;
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Errore durante la richiesta delle previsioni meteo correnti per {Location}", location);
            throw new ApplicationException($"Impossibile ottenere le previsioni meteo per {location}", ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Errore durante l'elaborazione delle previsioni meteo per {Location}", location);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<WeatherForecastDto> GetForecastAsync(WeatherRequest request)
    {
        try
        {
            // Controlla i parametri di input
            if (string.IsNullOrWhiteSpace(request.Location))
                throw new ArgumentException("La località è obbligatoria", nameof(request.Location));
            
            if (request.Days <= 0 || request.Days > 10)
                request.Days = 3; // Usa un valore predefinito se fuori intervallo
            
            // Costruisci l'URL con i parametri necessari
            var requestUri = $"forecast.json?key={ApiKey}" +
                            $"&q={Uri.EscapeDataString(request.Location)}" +
                            $"&days={request.Days}" +
                            $"&aqi={(request.AirQuality ? "yes" : "no")}" +
                            $"&alerts={(request.Alerts ? "yes" : "no")}" +
                            $"&astronomy=yes"; // Includi dati astronomici
            
            Logger.LogInformation("Richiesta API: {RequestUri}", requestUri);
            
            // Effettua la richiesta HTTP
            var response = await HttpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            
            // Leggi il contenuto della risposta come stringa per il log
            var contentString = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("Risposta API: {Response}", contentString);
            
            // Deserializza la risposta
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var weatherData = JsonSerializer.Deserialize<WeatherForecastResponse>(contentString, options);
            
            if (weatherData == null)
                throw new JsonException("Impossibile deserializzare i dati meteo");
            
            Logger.LogInformation("WeatherData deserializzato: Location={Location}, Temp={Temp}, ForecastDays={Count}", 
                weatherData.Location.Name, 
                weatherData.Current.TemperatureC,
                weatherData.Forecast?.ForecastDays?.Count ?? 0);
            
            // Converti i dati nel DTO
            var result = MapToWeatherForecastDto(weatherData);
            
            Logger.LogInformation("DTO mappato: Location={Location}, Temp={Temp}, ForecastDays={Count}", 
                result.Location, 
                result.CurrentTemperature,
                result.DailyForecasts?.Count ?? 0);
                
            return result;
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Errore durante la richiesta delle previsioni meteo per {Location}", request.Location);
            throw new ApplicationException($"Impossibile ottenere le previsioni meteo per {request.Location}", ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Errore durante l'elaborazione delle previsioni meteo per {Location}", request.Location);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> SearchLocationsAsync(string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
                return Enumerable.Empty<string>();
            
            // Costruisci l'URL per la ricerca delle località
            var requestUri = $"search.json?key={ApiKey}&q={Uri.EscapeDataString(query)}";
            
            // Effettua la richiesta HTTP
            var response = await HttpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            
            // Deserializza la risposta come array di location
            var contentString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var locations = JsonSerializer.Deserialize<List<Location>>(contentString, options);
            
            if (locations == null)
                return Enumerable.Empty<string>();
            
            // Restituisci i nomi delle località
            return locations.Select(l => $"{l.Name}, {l.Region}, {l.Country}");
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Errore durante la ricerca delle località per la query {Query}", query);
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Errore durante l'elaborazione della ricerca località per {Query}", query);
            return Enumerable.Empty<string>();
        }
    }
    
    #region Helper Methods
    
    protected WeatherForecastDto MapToWeatherForecastDto(WeatherForecastResponse response)
    {
        var result = new WeatherForecastDto
        {
            Location = response.Location.Name,
            Country = response.Location.Country,
            LastUpdated = ParseDateTime(response.Current.LastUpdated),
            CurrentTemperature = response.Current.TemperatureC,
            Condition = response.Current.Condition.Text,
            ConditionIcon = FixIconUrl(response.Current.Condition.Icon),
            Humidity = response.Current.Humidity,
            WindSpeed = response.Current.WindKph,
            WindDirection = response.Current.WindDirection
        };
        
        // Aggiungi le previsioni giornaliere se disponibili
        if (response.Forecast?.ForecastDays != null)
        {
            result.DailyForecasts = response.Forecast.ForecastDays.Select(fd => {
                var dto = new DailyForecastDto
                {
                    Date = DateTime.Parse(fd.Date),
                    MaxTemp = fd.Day.MaxTempC,
                    MinTemp = fd.Day.MinTempC,
                    AvgTemp = fd.Day.AvgTempC,
                    Condition = fd.Day.Condition.Text,
                    ConditionIcon = FixIconUrl(fd.Day.Condition.Icon),
                    ChanceOfRain = fd.Day.ChanceOfRain,
                    
                    // Dati astronomici base dalla previsione
                    Sunrise = fd.Astro.Sunrise,
                    Sunset = fd.Astro.Sunset,
                    Moonrise = fd.Astro.Moonrise,
                    Moonset = fd.Astro.Moonset,
                    MoonPhase = fd.Astro.MoonPhase,
                    MoonIllumination = fd.Astro.MoonIllumination,
                    IsMoonUp = fd.Astro.IsMoonUp == 1,
                    IsSunUp = fd.Astro.IsSunUp == 1
                };
                
                return dto;
            }).ToList();
        }
        
        return result;
    }
    
    protected DateTime ParseDateTime(string dateTimeStr)
    {
        return DateTime.TryParse(dateTimeStr, out var result) 
            ? result 
            : DateTime.Now;
    }
    
    protected string FixIconUrl(string iconUrl)
    {
        // WeatherAPI restituisce URL relativi per le icone, quindi aggiungiamo l'URL di base se necessario
        if (string.IsNullOrEmpty(iconUrl))
            return string.Empty;
            
        return iconUrl.StartsWith("//") 
            ? $"https:{iconUrl}" 
            : iconUrl;
    }
    
    #endregion
}