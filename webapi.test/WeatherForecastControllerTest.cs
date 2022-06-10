using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TestFixture;
using webapi.Proxies;
using Xunit;

namespace webapi.test
{
    public class WeatherForecastControllerTest
    {
        [Fact]
        public async Task WeatherForecastController_ReturnsSuccess_WhenQuoteServiceReturnsOKAsync()
        {
            using (var testFixture = new HttpTestFixture())
            {
                // Arrange
                testFixture
                    .AddProxy<IQuoteProxy, QuoteProxy>("QuoteApi:uri", (context) =>
                    {
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        context.Response.ContentType = "application/json";
                        context.Response.WriteAsync("[{ \"q\": \"Weaseling out of things is important to learn; it’s what separates us from the animals… except the weasel.\",\"a\": \"Homer Simpson\",\"h\":\"\"}]");

                        return Task.CompletedTask;
                    })
                   .Build(typeof(Program).Assembly);

                //Act
                var responseStream = await testFixture.Client.GetStreamAsync("/weatherforecast");
                var forecasts = await JsonSerializer.DeserializeAsync<List<WeatherForecast>>(responseStream);

                //Assert
                Assert.Equal("Weaseling out of things is important to learn; it’s what separates us from the animals… except the weasel.", forecasts?.First()?.Quote);
            }
        }

        [Fact]
        public async Task WeatherForecastController_ReturnsEmptyQuote_WhenServiceUnavailable()
        {
            /*
            try
            {
                // Arrange
                TestFixture.AddProxy("QuoteApi:uri", (context) =>
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return Task.CompletedTask;
                });

                TestFixture.Build(typeof(Program).Assembly);
                await TestFixture.StartAsync();

                //Act
                var responseStream = await TestFixture.Client.GetStreamAsync("/weatherforecast");
                var forecasts = await JsonSerializer.DeserializeAsync<List<WeatherForecast>>(responseStream);

                //Assert
                Assert.True(String.IsNullOrEmpty( forecasts?.First()?.Quote));
            }
            finally
            {
                await TestFixture.StopAsync();
            }
            */
        }
    }
}
