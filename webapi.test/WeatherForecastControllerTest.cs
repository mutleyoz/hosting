using Fixture;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using webapi.Controllers;
using Xunit;

namespace webapi.test
{
    public class WeatherForecastControllerTest
    {
        TestFixture _host;

        public WeatherForecastControllerTest()
        {
            _host = new TestFixture();
        }

        [Fact]
        public async Task WeaterForecastController_ReturnsSuccess_WhenQuoteServiceReturnsOKAsync()
        {
            _host.AddService("QuoteApi:uri", (context) =>
            {
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync("[{ \"q\": \"Weaseling out of things is important to learn; it’s what separates us from the animals… except the weasel.\",\"a\": \"Homer Simpson\",\"h\":\"\"}]");

                return Task.CompletedTask;
            });

            var (webhost, uri) = _host.Build(typeof(webapi.Program).Assembly);
            _host.Start();

            uri += "/weatherforecast";

            HttpClient _httpClient = new HttpClient();
            var responseStream = await _httpClient.GetStreamAsync(uri);
            var forecasts = await JsonSerializer.DeserializeAsync<List<WeatherForecast>>(responseStream);

            Assert.Equal("Weaseling out of things is important to learn; it’s what separates us from the animals… except the weasel.", forecasts?.First()?.Quote);
        }
    }
}
