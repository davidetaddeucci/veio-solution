using System.Text.Json;
using Hybrid.Veio.Web.Services.API.Models.Weather;
using Microsoft.Extensions.Options;

namespace Hybrid.Veio.Web.Services.API.Services.Weather;

/// <summary>
/// Implementazione estesa del servizio meteo
/// </summary>
public class WeatherServiceExtended : WeatherService, IWeatherServiceExtended
{
    public WeatherServiceExtended(
        HttpClient httpClient,
        IOptions<WeatherApiOptions> options,
        ILogger<WeatherService> logger)
        : base(httpClient, options, logger)
    {
    }
    
    /// <inheritdoc />
    public async Task<WeatherAreaForecastDto> GetAreaForecastAsync(WeatherAreaRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
            
        // Verifica validità delle coordinate
        if (!IsValidCoordinates(request.LatitudeTopRight, request.LongitudeTopRight) ||
            !IsValidCoordinates(request.LatitudeBottomLeft, request.LongitudeBottomLeft))
        {
            throw new ArgumentException("Le coordinate specificate non sono valide");
        }
        
        // Limita il numero di giorni (WeatherAPI supporta max 14-16 giorni a seconda del piano)
        if (request.Days < 1 || request.Days > 14)
            request.Days = Math.Clamp(request.Days, 1, 14);
            
        // Limita il numero di punti di campionamento per non sovraccaricare l'API
        if (request.SamplingPoints < 1 || request.SamplingPoints > 25)
            request.SamplingPoints = Math.Clamp(request.SamplingPoints, 1, 25);
        
        try
        {
            // Genera punti di campionamento nell'area
            var samplingPoints = GenerateSamplingPoints(
                request.LatitudeTopRight, request.LongitudeTopRight,
                request.LatitudeBottomLeft, request.LongitudeBottomLeft,
                request.SamplingPoints);
            
            // Ottieni previsioni per ogni punto
            var forecasts = new List<WeatherForecastDto>();
            foreach (var point in samplingPoints)
            {
                var pointRequest = new WeatherRequest
                {
                    Location = $"{point.Latitude},{point.Longitude}",
                    Days = request.Days,
                    AirQuality = request.AirQuality,
                    Alerts = request.Alerts
                };
                
                var forecast = await GetForecastAsync(pointRequest);
                forecasts.Add(forecast);
            }
            
            // Calcola area geografica e relative dimensioni
            var area = new GeoArea
            {
                LatitudeTopRight = request.LatitudeTopRight,
                LongitudeTopRight = request.LongitudeTopRight,
                LatitudeBottomLeft = request.LatitudeBottomLeft,
                LongitudeBottomLeft = request.LongitudeBottomLeft
            };
            
            // Calcola dimensioni approssimative dell'area in km
            area.WidthKm = CalculateDistanceKm(
                area.LatitudeBottomLeft, area.LongitudeBottomLeft,
                area.LatitudeBottomLeft, area.LongitudeTopRight);
                
            area.HeightKm = CalculateDistanceKm(
                area.LatitudeBottomLeft, area.LongitudeBottomLeft,
                area.LatitudeTopRight, area.LongitudeBottomLeft);
            
            // Aggrega le previsioni per l'area
            return AggregateForecasts(forecasts, area, samplingPoints);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Errore durante la richiesta delle previsioni meteo per l'area");
            throw new ApplicationException("Impossibile ottenere le previsioni meteo per l'area specificata", ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Errore durante l'elaborazione delle previsioni meteo per l'area");
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<WeatherHistoryDto> GetHistoricalDataAsync(string location, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("La località è obbligatoria", nameof(location));
            
        if (startDate > endDate)
            throw new ArgumentException("La data di inizio deve essere precedente o uguale alla data di fine");
            
        if (endDate > DateTime.Today)
            throw new ArgumentException("La data di fine non può essere nel futuro");
            
        // Limita il range per non sovraccaricare l'API
        var maxDays = 30; // Imposta un limite ragionevole
        if ((endDate - startDate).TotalDays > maxDays)
            throw new ArgumentException($"L'intervallo di date richiesto è troppo grande. Massimo {maxDays} giorni.");
        
        try
        {
            var results = new List<DailyHistoryDto>();
            var currentDate = startDate;
            string locationName = location;
            string countryName = string.Empty;
            
            // Poiché l'API potrebbe richiedere un piano a pagamento per i dati storici, implementiamo
            // una fallback che utilizza l'API di previsioni per ottenere dati passati recenti

            // Verifica se la data richiesta è molto recente (ultimi 7 giorni)
            bool isRecent = (DateTime.Today - startDate).TotalDays <= 7;

            if (isRecent)
            {
                // Se la data è recente, proviamo a utilizzare le previsioni normali
                // che potrebbero includere dati per i giorni passati recenti
                try
                {
                    Logger.LogInformation("Tentativo di utilizzare l'API forecast per dati storici recenti");
                    var weatherRequest = new WeatherRequest
                    {
                        Location = location,
                        Days = 7, // Massimo disponibile senza piano premium
                        AirQuality = false,
                        Alerts = false
                    };

                    var forecast = await GetForecastAsync(weatherRequest);
                    locationName = forecast.Location;
                    countryName = forecast.Country;

                    // Filtra solo i giorni richiesti
                    foreach (var dailyForecast in forecast.DailyForecasts)
                    {
                        if (dailyForecast.Date.Date >= startDate.Date && dailyForecast.Date.Date <= endDate.Date)
                        {
                            results.Add(new DailyHistoryDto
                            {
                                Date = dailyForecast.Date,
                                MaxTempC = dailyForecast.MaxTemp,
                                MinTempC = dailyForecast.MinTemp,
                                AvgTempC = (dailyForecast.MaxTemp + dailyForecast.MinTemp) / 2,
                                Condition = dailyForecast.Condition,
                                ConditionIcon = dailyForecast.ConditionIcon,
                                TotalPrecipitationMm = 0, // Non disponibile nelle previsioni base
                                AvgHumidity = 0, // Non disponibile nelle previsioni base
                                AvgWindKph = 0, // Non disponibile nelle previsioni base
                                WindDirection = ""
                            });
                        }
                    }

                    if (results.Count > 0)
                    {
                        Logger.LogInformation("Ottenuti {Count} giorni di dati storici dall'API forecast", results.Count);
                        return new WeatherHistoryDto
                        {
                            Location = locationName,
                            Country = countryName,
                            StartDate = startDate,
                            EndDate = endDate,
                            DailyData = results
                        };
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Fallito il tentativo di utilizzare l'API forecast per dati storici");
                    // Procediamo con il tentativo di utilizzare l'API history
                }
            }

            // Se siamo arrivati qui, o la data non è recente o il tentativo con forecast è fallito
            // Proviamo ad usare l'API history (che potrebbe richiedere un piano premium)

            // Per l'API history, utilizziamo sia dt che end_dt come richiesto dalla documentazione
            var formattedStartDate = startDate.ToString("yyyy-MM-dd");
            var formattedEndDate = endDate.ToString("yyyy-MM-dd");
            
            // Costruisci la richiesta con entrambi i parametri di data
            var requestUri = $"history.json?key={ApiKey}" +
                          $"&q={Uri.EscapeDataString(location)}" +
                          $"&dt={formattedStartDate}" +
                          $"&end_dt={formattedEndDate}";
            
            Logger.LogInformation("Richiesta API history: {RequestUri}", requestUri);
            
            var response = await HttpClient.GetAsync(requestUri);
            
            // Leggi la risposta e logga gli eventuali errori
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError("Errore API history: {StatusCode}, Risposta: {Response}", 
                    response.StatusCode, responseContent);
                throw new HttpRequestException($"Errore API WeatherAPI.com: {response.StatusCode}. {responseContent}");
            }
            
            response.EnsureSuccessStatusCode();
            
            Logger.LogInformation("Risposta API history: {Response}", responseContent);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var historyData = JsonSerializer.Deserialize<WeatherForecastResponse>(responseContent, options);
            
            if (historyData?.Forecast?.ForecastDays == null || !historyData.Forecast.ForecastDays.Any())
            {
                throw new InvalidOperationException("Nessun dato storico disponibile per le date specificate");
            }
            
            foreach (var day in historyData.Forecast.ForecastDays)
            {
                results.Add(new DailyHistoryDto
                {
                    Date = DateTime.Parse(day.Date),
                    MaxTempC = day.Day.MaxTempC,
                    MinTempC = day.Day.MinTempC,
                    AvgTempC = day.Day.AvgTempC,
                    TotalPrecipitationMm = day.Day.TotalPrecipMm,
                    AvgHumidity = day.Day.AvgHumidity,
                    Condition = day.Day.Condition.Text,
                    ConditionIcon = FixIconUrl(day.Day.Condition.Icon),
                    AvgWindKph = day.Day.MaxwindKph,
                    WindDirection = GetPredominantWindDirection(day.Hours)
                });
            }
            
            if (historyData.Location != null)
            {
                locationName = historyData.Location.Name;
                countryName = historyData.Location.Country;
            }
            
            return new WeatherHistoryDto
            {
                Location = locationName,
                Country = countryName,
                StartDate = startDate,
                EndDate = endDate,
                DailyData = results
            };
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Errore durante la richiesta dei dati storici per {Location}", location);
            throw new ApplicationException($"Impossibile ottenere i dati storici per {location}", ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Errore durante l'elaborazione dei dati storici per {Location}", location);
            throw;
        }
    }
    
    #region Helper Methods
    
    /// <summary>
    /// Genera punti di campionamento all'interno di un'area rettangolare
    /// </summary>
    private List<GeoPoint> GenerateSamplingPoints(
        double latTopRight, double lonTopRight,
        double latBottomLeft, double lonBottomLeft,
        int numPoints)
    {
        var points = new List<GeoPoint>();
        
        // Aggiungi sempre i quattro angoli e il centro
        if (numPoints >= 5)
        {
            // Angolo in alto a destra
            points.Add(new GeoPoint 
            { 
                Latitude = latTopRight, 
                Longitude = lonTopRight,
                Name = "Angolo NE" 
            });
            
            // Angolo in alto a sinistra
            points.Add(new GeoPoint 
            { 
                Latitude = latTopRight, 
                Longitude = lonBottomLeft,
                Name = "Angolo NO" 
            });
            
            // Angolo in basso a sinistra
            points.Add(new GeoPoint 
            { 
                Latitude = latBottomLeft, 
                Longitude = lonBottomLeft,
                Name = "Angolo SO" 
            });
            
            // Angolo in basso a destra
            points.Add(new GeoPoint 
            { 
                Latitude = latBottomLeft, 
                Longitude = lonTopRight,
                Name = "Angolo SE" 
            });
            
            // Centro
            points.Add(new GeoPoint 
            { 
                Latitude = (latTopRight + latBottomLeft) / 2, 
                Longitude = (lonTopRight + lonBottomLeft) / 2,
                Name = "Centro" 
            });
            
            numPoints -= 5; // Abbiamo già aggiunto 5 punti
        }
        
        // Se richiesti più punti, distribuiscili in modo uniforme
        if (numPoints > 0)
        {
            // Calcola quante righe e colonne usare
            int rows = (int)Math.Ceiling(Math.Sqrt(numPoints));
            int cols = (int)Math.Ceiling((double)numPoints / rows);
            
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (points.Count >= 5 + numPoints)
                        break;
                        
                    double latRatio = (r + 1.0) / (rows + 1);
                    double lonRatio = (c + 1.0) / (cols + 1);
                    
                    double lat = latBottomLeft + (latTopRight - latBottomLeft) * latRatio;
                    double lon = lonBottomLeft + (lonTopRight - lonBottomLeft) * lonRatio;
                    
                    points.Add(new GeoPoint 
                    { 
                        Latitude = lat, 
                        Longitude = lon,
                        Name = $"Punto {r+1}-{c+1}" 
                    });
                }
            }
        }
        
        return points;
    }
    
    /// <summary>
    /// Aggrega le previsioni dai diversi punti di campionamento
    /// </summary>
    private WeatherAreaForecastDto AggregateForecasts(
        List<WeatherForecastDto> forecasts, 
        GeoArea area,
        List<GeoPoint> samplingPoints)
    {
        var result = new WeatherAreaForecastDto
        {
            Area = area,
            LastUpdated = forecasts.Any() ? forecasts.Max(f => f.LastUpdated) : DateTime.Now,
            SamplingPoints = samplingPoints
        };
        
        // Se non ci sono previsioni, restituisci un oggetto vuoto
        if (!forecasts.Any() || !forecasts.First().DailyForecasts.Any())
            return result;
            
        // Determina il numero di giorni disponibili
        int days = forecasts.Min(f => f.DailyForecasts.Count);
        
        // Aggrega le previsioni giorno per giorno
        for (int day = 0; day < days; day++)
        {
            // Estrai le previsioni per questo giorno da tutti i punti
            var dailyForecasts = forecasts.Select(f => f.DailyForecasts[day]).ToList();
            
            // Calcola l'affidabilità in base alla distanza temporale
            double reliabilityScore = CalculateReliabilityScore(dailyForecasts[0].Date, dailyForecasts);
            
            // Calcola le condizioni prevalenti
            var conditions = dailyForecasts.Select(df => df.Condition).ToList();
            var predominantCondition = GetPredominantCondition(conditions);
            
            // Calcola l'indice di variabilità (quanto sono diverse le previsioni nell'area)
            double variabilityScore = CalculateVariabilityScore(dailyForecasts);
            
            // Crea l'aggregazione per questo giorno
            var aggregatedDay = new AreaDailyForecastDto
            {
                Date = dailyForecasts[0].Date,
                MaxTempC = dailyForecasts.Average(df => df.MaxTemp),
                MinTempC = dailyForecasts.Average(df => df.MinTemp),
                AvgTempC = dailyForecasts.Average(df => (df.MaxTemp + df.MinTemp) / 2),
                ChanceOfRain = dailyForecasts.Average(df => df.ChanceOfRain),
                PredominantCondition = predominantCondition.condition,
                ConditionIcon = predominantCondition.icon,
                ReliabilityScore = reliabilityScore,
                VariabilityScore = variabilityScore,
                ConditionsInArea = conditions.Distinct().ToList()
            };
            
            result.DailyForecasts.Add(aggregatedDay);
        }
        
        return result;
    }
    
    /// <summary>
    /// Calcola la condizione meteorologica prevalente tra più punti
    /// </summary>
    private (string condition, string icon) GetPredominantCondition(List<string> conditions)
    {
        // Raggruppa per condizione e conta occorrenze
        var grouped = conditions
            .GroupBy(c => c)
            .Select(g => new { Condition = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToList();
            
        // Prendi la condizione più frequente
        var predominant = grouped.First();
        
        // Trova l'icona corrispondente (possiamo assumere che sia la stessa per la stessa condizione)
        // In una implementazione reale, dovresti mappare le condizioni alle icone o passare sia condizione che icona
        return (predominant.Condition, string.Empty); // Qui andrebbe aggiunta l'icona
    }
    
    /// <summary>
    /// Calcola l'affidabilità delle previsioni in base a vari fattori
    /// </summary>
    private double CalculateReliabilityScore(DateTime forecastDate, List<DailyForecastDto> forecasts)
    {
        // Base: l'affidabilità diminuisce con la distanza temporale
        int daysInFuture = (int)(forecastDate - DateTime.Today).TotalDays;
        
        // Valori empirici: ~90% per oggi, ~80% per 3 giorni, ~70% per 7 giorni, ~50% per 14 giorni
        double baseReliability = Math.Max(95 - (daysInFuture * 3.5), 30);
        
        // L'affidabilità diminuisce in proporzione alla variabilità delle previsioni
        double tempVariability = CalculateTemperatureVariability(forecasts);
        double conditionVariability = CalculateConditionVariability(forecasts);
        
        // Riduci l'affidabilità in base alla variabilità
        double reliabilityReduction = (tempVariability * 0.5) + (conditionVariability * 0.5);
        
        // Limita la riduzione massima al 30%
        reliabilityReduction = Math.Min(reliabilityReduction, 30);
        
        return Math.Round(baseReliability - reliabilityReduction, 1);
    }
    
    /// <summary>
    /// Calcola la variabilità delle temperature tra i punti
    /// </summary>
    private double CalculateTemperatureVariability(List<DailyForecastDto> forecasts)
    {
        // Varianza nelle temperature max
        double maxTempVariance = CalculateVariance(forecasts.Select(f => f.MaxTemp));
        
        // Varianza nelle temperature min
        double minTempVariance = CalculateVariance(forecasts.Select(f => f.MinTemp));
        
        // Normalizza su scala 0-100
        double normalizedVariance = (maxTempVariance + minTempVariance) * 5;
        
        return Math.Min(normalizedVariance, 100);
    }
    
    /// <summary>
    /// Calcola la variabilità delle condizioni meteorologiche tra i punti
    /// </summary>
    private double CalculateConditionVariability(List<DailyForecastDto> forecasts)
    {
        // Conta quante condizioni diverse ci sono
        int uniqueConditions = forecasts.Select(f => f.Condition).Distinct().Count();
        
        // Se c'è una sola condizione, variabilità 0
        // Se ogni punto ha una condizione diversa, variabilità 100
        return Math.Min((uniqueConditions - 1) * 100.0 / forecasts.Count, 100);
    }
    
    /// <summary>
    /// Calcola l'indice di variabilità complessivo per un giorno
    /// </summary>
    private double CalculateVariabilityScore(List<DailyForecastDto> forecasts)
    {
        // Media delle variabilità delle temperature e delle condizioni
        double tempVar = CalculateTemperatureVariability(forecasts);
        double condVar = CalculateConditionVariability(forecasts);
        
        return Math.Round((tempVar + condVar) / 2, 1);
    }
    
    /// <summary>
    /// Calcola la varianza di una sequenza di numeri
    /// </summary>
    private double CalculateVariance(IEnumerable<double> values)
    {
        double average = values.Average();
        double sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();
        return sumOfSquaresOfDifferences / values.Count();
    }
    
    /// <summary>
    /// Calcola la distanza approssimativa in km tra due punti geografici
    /// Utilizza la formula dell'emisenoverso (Haversine)
    /// </summary>
    private double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusKm = 6371.0;
        
        // Converti in radianti
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);
        
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                   
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return EarthRadiusKm * c;
    }
    
    /// <summary>
    /// Converte gradi in radianti
    /// </summary>
    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    
    /// <summary>
    /// Verifica se le coordinate sono valide
    /// </summary>
    private bool IsValidCoordinates(double latitude, double longitude)
    {
        return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
    }
    
    /// <summary>
    /// Determina la direzione prevalente del vento dalle rilevazioni orarie
    /// </summary>
    private string GetPredominantWindDirection(List<Hour> hours)
    {
        // Assumiamo che questo campo esista nelle ore
        var directions = hours.Select(h => h.WindDir).ToList();
        
        // Semplice logica: prendi la direzione più frequente
        return directions
            .GroupBy(d => d)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "N/A";
    }
    
    #endregion
}