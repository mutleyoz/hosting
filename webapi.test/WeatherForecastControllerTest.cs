using HttpFixture;
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
        [Fact]
        public async Task WeatherForecastController_ReturnsSuccess_WhenQuoteServiceReturnsOKAsync()
        {
            try
            {
                // Arrange
                TestFixture.AddService("QuoteApi:uri", (context) =>
                {
                    context.Response.ContentType = "application/json";
                    context.Response.WriteAsync("[{ \"q\": \"Weaseling out of things is important to learn; it’s what separates us from the animals… except the weasel.\",\"a\": \"Homer Simpson\",\"h\":\"\"}]");

                    return Task.CompletedTask;
                });

                TestFixture.Build(typeof(Program).Assembly);
                TestFixture.Start();

                //Act
                var responseStream = await TestFixture.Client.GetStreamAsync("/weatherforecast");
                var forecasts = await JsonSerializer.DeserializeAsync<List<WeatherForecast>>(responseStream);

                //Assert
                Assert.Equal("Weaseling out of things is important to learn; it’s what separates us from the animals… except the weasel.", forecasts?.First()?.Quote);
            }
            finally
            {
                TestFixture.Stop();
            }
        }

        [Fact]
        public async Task WeatherForecastController_ReturnsEmptyQuote_WhenServiceUnavailable()
        {
            try
            {
                // Arrange
                TestFixture.AddService("QuoteApi:uri", (context) =>
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return Task.CompletedTask;
                });

                TestFixture.Build(typeof(Program).Assembly);
                TestFixture.Start();

                //Act
                var responseStream = await TestFixture.Client.GetStreamAsync("/weatherforecast");
                var forecasts = await JsonSerializer.DeserializeAsync<List<WeatherForecast>>(responseStream);

                //Assert
                Assert.Equal(String.Empty, forecasts?.First()?.Quote);
            }
            finally
            {
                TestFixture.Stop();
            }
        }
    }
}
