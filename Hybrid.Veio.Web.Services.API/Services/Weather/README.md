# Servizi Meteo WeatherAPI

Questa cartella contiene l'implementazione dei servizi meteo che utilizzano WeatherAPI.com come provider di dati meteorologici.

## Funzionalità

- **Previsioni meteo correnti** per località specifiche
- **Previsioni meteo future** per i prossimi giorni (fino a 14 giorni)
- **Previsioni per aree geografiche** definite tramite coordinate
- **Dati storici meteo** con sistema di fallback intelligente
- **Dati astronomici** (alba, tramonto, fasi lunari, ecc.)
- **Calcolo dell'affidabilità** delle previsioni
- **Ricerca di località** tramite testo

## Configurazione

Per utilizzare i servizi, aggiungi le seguenti impostazioni nel file `appsettings.json`:

```json
"WeatherApi": {
  "ApiKey": "YOUR_WEATHERAPI_KEY_HERE",
  "TimeoutSeconds": 30,
  "MaxRetries": 3
}
```

## Endpoint API

### Previsioni base (`/api/weather`)

- `GET /api/weather/current?location={località}` - Previsioni meteo correnti
- `GET /api/weather/forecast?location={località}&days={giorni}` - Previsioni meteo per i prossimi giorni
- `GET /api/weather/locations?query={testo}` - Ricerca località

### Funzionalità avanzate (`/api/weatherextended`)

- `GET /api/weatherextended/area-forecast` - Previsioni meteo per un'area geografica
- `POST /api/weatherextended/area-forecast` - Previsioni meteo per un'area geografica (post)
- `GET /api/weatherextended/history` - Dati storici meteorologici

## Note

- L'accesso ai dati storici oltre i 7 giorni potrebbe richiedere un piano a pagamento su WeatherAPI.com
- Le previsioni oltre i 14 giorni non sono supportate
- Il servizio include resilienza HTTP con policy di retry per errori transitori