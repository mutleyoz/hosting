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
using webapi.Proxies;

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
        private readonly IQuoteProxy _quotesProxy;
        private readonly IConfiguration _config;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IQuoteProxy quoteProxy)
        {
            _logger = logger;
            _quotesProxy = quoteProxy;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            var rng = new Random();
            List<Quote> quotes;

            var response = await _quotesProxy.FetchQuote();

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
