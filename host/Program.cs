using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using webapi;
using System.Linq;

namespace hosting
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //var host = new Fixture.TestFixture();
            //host.AddService("QuoteApi:uri", (context) =>
            //{
            //    context.Response.WriteAsync("{q: \"Weaseling out of things is important to learn; it’s what separates us from the animals… except the weasel.\",a: \"Homer Simpson\"}");

            //    return Task.CompletedTask;
            //});
            //var (webhost, url) = host.Build(typeof(webapi.Program).Assembly);

            //host.Start();

            var _host = new Fixture.TestFixture();
            _host.AddService("QuoteApi:uri", (context) =>
            {
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync("[{ \"q\": \"Weaseling out of things is important to learn; it’s what separates us from the animals… except the weasel.\",\"a\": \"Homer Simpson\",\"h\":\"\"}]");

                return Task.CompletedTask;
            });
            var (webhost, uri) = _host.Build(typeof(webapi.Program).Assembly);
            _host.Start();

            uri = uri + "/weatherforecast";

            HttpClient _httpClient = new HttpClient();
            var responseStream = await _httpClient.GetStreamAsync(uri);
            var forecasts = await JsonSerializer.DeserializeAsync<List<WeatherForecast>>(responseStream);
            
            Console.WriteLine("Started host");
            Console.Read();
        }
    }
}
