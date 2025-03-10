using System.Text.Json.Serialization;

namespace Hybrid.Veio.Web.Services.API.Models.Weather;

/// <summary>
/// Richiesta per previsioni meteo su un'area geografica rettangolare
/// </summary>
public class WeatherAreaRequest
{
    /// <summary>
    /// Latitudine dell'angolo superiore destro dell'area rettangolare
    /// </summary>
    public double LatitudeTopRight { get; set; }
    
    /// <summary>
    /// Longitudine dell'angolo superiore destro dell'area rettangolare
    /// </summary>
    public double LongitudeTopRight { get; set; }
    
    /// <summary>
    /// Latitudine dell'angolo inferiore sinistro dell'area rettangolare
    /// </summary>
    public double LatitudeBottomLeft { get; set; }
    
    /// <summary>
    /// Longitudine dell'angolo inferiore sinistro dell'area rettangolare
    /// </summary>
    public double LongitudeBottomLeft { get; set; }
    
    /// <summary>
    /// Numero di giorni di previsione (default: 3, max: 14)
    /// </summary>
    public int Days { get; set; } = 3;
    
    /// <summary>
    /// Numero di punti di campionamento nell'area (default: 5)
    /// </summary>
    public int SamplingPoints { get; set; } = 5;
    
    /// <summary>
    /// Includere dati sulla qualità dell'aria
    /// </summary>
    public bool AirQuality { get; set; } = false;
    
    /// <summary>
    /// Includere avvisi meteo
    /// </summary>
    public bool Alerts { get; set; } = false;
}

/// <summary>
/// Rappresenta un punto geografico per il campionamento
/// </summary>
public class GeoPoint
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO per la previsione meteo su un'area geografica
/// </summary>
public class WeatherAreaForecastDto
{
    /// <summary>
    /// Area geografica coperta dalla previsione
    /// </summary>
    public GeoArea Area { get; set; } = new();
    
    /// <summary>
    /// Data e ora dell'ultimo aggiornamento
    /// </summary>
    public DateTime LastUpdated { get; set; }
    
    /// <summary>
    /// Previsioni giornaliere per l'area
    /// </summary>
    public List<AreaDailyForecastDto> DailyForecasts { get; set; } = new();
    
    /// <summary>
    /// Punti di campionamento utilizzati per la previsione
    /// </summary>
    public List<GeoPoint> SamplingPoints { get; set; } = new();
}

/// <summary>
/// Rappresenta l'area geografica rettangolare
/// </summary>
public class GeoArea
{
    /// <summary>
    /// Latitudine dell'angolo superiore destro
    /// </summary>
    public double LatitudeTopRight { get; set; }
    
    /// <summary>
    /// Longitudine dell'angolo superiore destro
    /// </summary>
    public double LongitudeTopRight { get; set; }
    
    /// <summary>
    /// Latitudine dell'angolo inferiore sinistro
    /// </summary>
    public double LatitudeBottomLeft { get; set; }
    
    /// <summary>
    /// Longitudine dell'angolo inferiore sinistro
    /// </summary>
    public double LongitudeBottomLeft { get; set; }
    
    /// <summary>
    /// Centro dell'area (calcolato)
    /// </summary>
    public GeoPoint Center => new()
    {
        Latitude = (LatitudeTopRight + LatitudeBottomLeft) / 2,
        Longitude = (LongitudeTopRight + LongitudeBottomLeft) / 2
    };
    
    /// <summary>
    /// Larghezza approssimativa dell'area in chilometri
    /// </summary>
    public double WidthKm { get; set; }
    
    /// <summary>
    /// Altezza approssimativa dell'area in chilometri
    /// </summary>
    public double HeightKm { get; set; }
}

/// <summary>
/// DTO per la previsione giornaliera su un'area
/// </summary>
public class AreaDailyForecastDto
{
    /// <summary>
    /// Data della previsione
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Temperatura massima media nell'area (°C)
    /// </summary>
    public double MaxTempC { get; set; }
    
    /// <summary>
    /// Temperatura minima media nell'area (°C)
    /// </summary>
    public double MinTempC { get; set; }
    
    /// <summary>
    /// Temperatura media nell'area (°C)
    /// </summary>
    public double AvgTempC { get; set; }
    
    /// <summary>
    /// Probabilità media di precipitazioni nell'area (%)
    /// </summary>
    public double ChanceOfRain { get; set; }
    
    /// <summary>
    /// Condizioni meteorologiche prevalenti nell'area
    /// </summary>
    public string PredominantCondition { get; set; } = string.Empty;
    
    /// <summary>
    /// Icona delle condizioni meteorologiche prevalenti
    /// </summary>
    public string ConditionIcon { get; set; } = string.Empty;
    
    /// <summary>
    /// Indice di affidabilità della previsione (0-100%)
    /// Calcolato in base a vari fattori
    /// </summary>
    public double ReliabilityScore { get; set; }
    
    /// <summary>
    /// Variabilità delle previsioni nell'area (0-100%)
    /// Un valore alto indica condizioni molto diverse all'interno dell'area
    /// </summary>
    public double VariabilityScore { get; set; }
    
    /// <summary>
    /// Condizioni meteo nei vari punti di campionamento
    /// </summary>
    public List<string> ConditionsInArea { get; set; } = new();
}

/// <summary>
/// Richiesta per dati meteo storici
/// </summary>
public class WeatherHistoryRequest
{
    /// <summary>
    /// Località o coordinate
    /// </summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// Data di inizio (non oltre 1 anno fa per i piani standard)
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Data di fine (non può essere nel futuro)
    /// </summary>
    public DateTime EndDate { get; set; }
}

/// <summary>
/// DTO per i dati meteo storici
/// </summary>
public class WeatherHistoryDto
{
    /// <summary>
    /// Località dei dati storici
    /// </summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// Paese
    /// </summary>
    public string Country { get; set; } = string.Empty;
    
    /// <summary>
    /// Data di inizio periodo
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Data di fine periodo
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Dati giornalieri nel periodo
    /// </summary>
    public List<DailyHistoryDto> DailyData { get; set; } = new();
}

/// <summary>
/// DTO per i dati storici di un singolo giorno
/// </summary>
public class DailyHistoryDto
{
    /// <summary>
    /// Data 
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Temperatura massima (°C)
    /// </summary>
    public double MaxTempC { get; set; }
    
    /// <summary>
    /// Temperatura minima (°C)
    /// </summary>
    public double MinTempC { get; set; }
    
    /// <summary>
    /// Temperatura media (°C)
    /// </summary>
    public double AvgTempC { get; set; }
    
    /// <summary>
    /// Precipitazioni totali (mm)
    /// </summary>
    public double TotalPrecipitationMm { get; set; }
    
    /// <summary>
    /// Umidità media (%)
    /// </summary>
    public double AvgHumidity { get; set; }
    
    /// <summary>
    /// Condizione meteorologica prevalente
    /// </summary>
    public string Condition { get; set; } = string.Empty;
    
    /// <summary>
    /// Icona della condizione
    /// </summary>
    public string ConditionIcon { get; set; } = string.Empty;
    
    /// <summary>
    /// Velocità media del vento (km/h)
    /// </summary>
    public double AvgWindKph { get; set; }
    
    /// <summary>
    /// Direzione prevalente del vento
    /// </summary>
    public string WindDirection { get; set; } = string.Empty;
}