using Hybrid.Veio.Web.Services.API.Data;
using Hybrid.Veio.Web.Services.API.Services.Weather;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;
using System.Text;
using Syncfusion.Blazor;
using Syncfusion.Licensing;


var builder = WebApplication.CreateBuilder(args);


// Registra AppDbContext con la connessione SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurazione di WeatherAPI
builder.Services.Configure<WeatherApiOptions>(
    builder.Configuration.GetSection(WeatherApiOptions.SectionName));

// Registra i servizi WeatherAPI
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IWeatherServiceExtended, WeatherServiceExtended>();

// Registra HttpClient senza policy di resilienza (per evitare errori di compilazione)
builder.Services.AddHttpClient<IWeatherServiceExtended, WeatherServiceExtended>((provider, client) => {
    var options = provider.GetRequiredService<IOptions<WeatherApiOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

// Configurazione di Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hybrid.Veio.API", Version = "v1" });
    
    // Aggiungi documentazione XML per gli endpoint meteo
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add authentication and policies
builder.Services.AddAuthorization(options =>
    options.AddPolicy("AdminPolicy", policy => policy.RequireClaim("Role", "Admin")));

var app = builder.Build();
// Middleware per Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hybrid.Veio.API v1");
    c.RoutePrefix = string.Empty; // Swagger come pagina predefinita
});


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
