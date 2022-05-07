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

namespace hosting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new Fixture.TestFixture();
            host.AddDependencyUrl("QuoteApi:uri", (context) =>
            {
                context.Response.WriteAsync("Stub for quote api");
                return Task.CompletedTask;
            });
            var (webhost, url) = host.Build(typeof(webapi.Program).Assembly);

            host.Start();

            Console.WriteLine("Started host");
            Console.Read();
        }
    }
}
