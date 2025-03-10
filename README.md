# Hybrid Veio Solution

## Descrizione
Hybrid Veio Solution è una piattaforma API completa per la gestione di vari servizi, inclusi dati meteo, autenticazione e molto altro. L'architettura è progettata per essere modulare, scalabile e facilmente estendibile.

## Struttura della Soluzione

La soluzione è composta dai seguenti progetti:

- **Hybrid.Veio.Web.Services.API**: API RESTful principale che espone i vari servizi
- **Hybrid.Veio.Names**: Libreria di supporto con definizioni condivise

### Servizi Principali

#### Servizio Meteo (WeatherAPI)
Integrazione completa con WeatherAPI.com per fornire dati meteorologici dettagliati:
- Previsioni meteo attuali e future
- Previsioni per aree geografiche definite da coordinate
- Dati storici meteo
- Dati astronomici (alba, tramonto, fasi lunari)

#### Autenticazione e Autorizzazione
Sistema di autenticazione basato su JWT (JSON Web Token):
- Registrazione utenti
- Login sicuro
- Gestione dei ruoli

## Prerequisiti

- .NET 9.0 SDK
- SQL Server
- Account WeatherAPI.com (per i servizi meteo)

## Configurazione

### Database
Modificare la stringa di connessione nel file `appsettings.json` del progetto Hybrid.Veio.Web.Services.API:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TUO_SERVER;Database=Hybrid.Veio.DB;User ID=TUO_USER;Password=TUA_PASSWORD;TrustServerCertificate=True"
}
```

### API Meteo
Per utilizzare i servizi meteo, è necessario ottenere una chiave API da WeatherAPI.com e configurarla nel file `appsettings.json`:

```json
"WeatherApi": {
  "ApiKey": "TUA_CHIAVE_API_QUI",
  "TimeoutSeconds": 30,
  "MaxRetries": 3
}
```

### JWT
Configurare le impostazioni JWT per l'autenticazione:

```json
"Jwt": {
  "Key": "your-256-bit-secret-key-which-is-at-least-32-characters-long",
  "Issuer": "your-issuer",
  "Audience": "your-audience"
}
```

## Avvio del Progetto

```bash
# Posizionarsi nella directory del progetto API
cd Hybrid.Veio.Web.Services.API

# Eseguire l'API
dotnet run
```

L'API sarà disponibile all'indirizzo: `https://localhost:7233/`

La documentazione Swagger è accessibile all'URL root: `https://localhost:7233/`

## Endpoint API

### Autenticazione
- `POST /api/auth/register` - Registrazione utente
- `POST /api/auth/login` - Login utente

### Servizio Meteo
- `GET /api/weather/current` - Previsioni meteo correnti
- `GET /api/weather/forecast` - Previsioni meteo future
- `GET /api/weather/locations` - Ricerca località
- `GET /api/weatherextended/area-forecast` - Previsioni per area geografica
- `GET /api/weatherextended/history` - Dati meteo storici

## Documentazione Dettagliata

Per ulteriori dettagli sui singoli servizi, consultare i README specifici:

- [Servizio Meteo](./Hybrid.Veio.Web.Services.API/Services/Weather/README.md)

## Licenza

Proprietaria © Hybrid Veio. Tutti i diritti riservati.