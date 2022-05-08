using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace webapi.Controllers
{
    public class Quote
    {
        [JsonPropertyName("a")]
        public string Author { get; set; }

        [JsonPropertyName("q")]
        public string Text { get; set; }
    }


    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _httpClient = new HttpClient();
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            var rng = new Random();
            List<Quote> quotes;

            var uri = _config["QuoteApi:uri"];
            var response = await _httpClient.GetAsync(uri);
            quotes = response.IsSuccessStatusCode ? await JsonSerializer.DeserializeAsync<List<Quote>>(await response.Content.ReadAsStreamAsync()) : new List<Quote> { };
            
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Quote = quotes?.FirstOrDefault()?.Text,
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
            
        }
    }
}
