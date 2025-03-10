using System.Text.Json.Serialization;

namespace Hybrid.Veio.Web.Services.API.Models.Weather;

public class WeatherForecastResponse
{
    [JsonPropertyName("location")]
    public Location Location { get; set; } = new();
    
    [JsonPropertyName("current")]
    public Current Current { get; set; } = new();
    
    [JsonPropertyName("forecast")]
    public Forecast Forecast { get; set; } = new();
    
    [JsonPropertyName("astronomy")]
    public AstronomyInfo Astronomy { get; set; } = new();
}

public class AstronomyInfo
{
    [JsonPropertyName("astro")]
    public Astro Astro { get; set; } = new();
}

public class Location
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;
    
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
    
    [JsonPropertyName("lat")]
    public double Lat { get; set; }
    
    [JsonPropertyName("lon")]
    public double Lon { get; set; }
    
    [JsonPropertyName("localtime")]
    public string Localtime { get; set; } = string.Empty;
}

public class Current
{
    [JsonPropertyName("last_updated")]
    public string LastUpdated { get; set; } = string.Empty;
    
    [JsonPropertyName("temp_c")]
    public double TemperatureC { get; set; }
    
    [JsonPropertyName("temp_f")]
    public double TemperatureF { get; set; }
    
    [JsonPropertyName("condition")]
    public Condition Condition { get; set; } = new();
    
    [JsonPropertyName("wind_kph")]
    public double WindKph { get; set; }
    
    [JsonPropertyName("wind_dir")]
    public string WindDirection { get; set; } = string.Empty;
    
    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
    
    [JsonPropertyName("feelslike_c")]
    public double FeelsLikeC { get; set; }
    
    [JsonPropertyName("uv")]
    public double UvIndex { get; set; }
}

public class Condition
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
    
    [JsonPropertyName("code")]
    public int Code { get; set; }
}

public class Forecast
{
    [JsonPropertyName("forecastday")]
    public List<ForecastDay> ForecastDays { get; set; } = new();
}

public class ForecastDay
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
    
    [JsonPropertyName("date_epoch")]
    public long DateEpoch { get; set; }
    
    [JsonPropertyName("day")]
    public Day Day { get; set; } = new();
    
    [JsonPropertyName("astro")]
    public Astro Astro { get; set; } = new();
    
    [JsonPropertyName("hour")]
    public List<Hour> Hours { get; set; } = new();
}

public class Day
{
    [JsonPropertyName("maxtemp_c")]
    public double MaxTempC { get; set; }
    
    [JsonPropertyName("mintemp_c")]
    public double MinTempC { get; set; }
    
    [JsonPropertyName("avgtemp_c")]
    public double AvgTempC { get; set; }
    
    [JsonPropertyName("daily_chance_of_rain")]
    public int ChanceOfRain { get; set; }
    
    [JsonPropertyName("totalprecip_mm")]
    public double TotalPrecipMm { get; set; }

    [JsonPropertyName("avghumidity")]
    public double AvgHumidity { get; set; }

    [JsonPropertyName("maxwind_kph")]
    public double MaxwindKph { get; set; }
    
    [JsonPropertyName("condition")]
    public Condition Condition { get; set; } = new();
}

public class Astro
{
    [JsonPropertyName("sunrise")]
    public string Sunrise { get; set; } = string.Empty;
    
    [JsonPropertyName("sunset")]
    public string Sunset { get; set; } = string.Empty;
    
    [JsonPropertyName("moonrise")]
    public string Moonrise { get; set; } = string.Empty;
    
    [JsonPropertyName("moonset")]
    public string Moonset { get; set; } = string.Empty;
    
    [JsonPropertyName("moon_phase")]
    public string MoonPhase { get; set; } = string.Empty;
    
    [JsonPropertyName("moon_illumination")]
    public int MoonIllumination { get; set; }
    
    [JsonPropertyName("is_moon_up")]
    public int IsMoonUp { get; set; }
    
    [JsonPropertyName("is_sun_up")]
    public int IsSunUp { get; set; }
}

public class Hour
{
    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;
    
    [JsonPropertyName("temp_c")]
    public double TempC { get; set; }
    
    [JsonPropertyName("chance_of_rain")]
    public int ChanceOfRain { get; set; }

    [JsonPropertyName("wind_dir")]
    public string WindDir { get; set; } = string.Empty;
    
    [JsonPropertyName("condition")]
    public Condition Condition { get; set; } = new();
}

// Modello per le richieste di previsioni meteo
public class WeatherRequest
{
    public string Location { get; set; } = string.Empty;
    public int Days { get; set; } = 3;
    public bool AirQuality { get; set; } = false;
    public bool Alerts { get; set; } = false;
}

// DTO per la risposta semplificata delle previsioni meteo
public class WeatherForecastDto
{
    public string Location { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public double CurrentTemperature { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string ConditionIcon { get; set; } = string.Empty;
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string WindDirection { get; set; } = string.Empty;
    public List<DailyForecastDto> DailyForecasts { get; set; } = new();
}

public class DailyForecastDto
{
    public DateTime Date { get; set; }
    public double MaxTemp { get; set; }
    public double MinTemp { get; set; }
    public double AvgTemp { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string ConditionIcon { get; set; } = string.Empty;
    public int ChanceOfRain { get; set; }
    
    // Informazioni astronomiche
    public string Sunrise { get; set; } = string.Empty;
    public string Sunset { get; set; } = string.Empty;
    public string Moonrise { get; set; } = string.Empty;
    public string Moonset { get; set; } = string.Empty;
    public string MoonPhase { get; set; } = string.Empty;
    public int MoonIllumination { get; set; }
    public bool IsMoonUp { get; set; }
    public bool IsSunUp { get; set; }
}