using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace hosting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new TestFixture.TestFixtureWebHost();
            var (webhost, url) = host.StartHost(typeof(webapi.Program).Assembly);
            Console.WriteLine($"Now listening on {url}");

            webhost.Run();

            Console.WriteLine("Started host");
            Console.Read();

           // CreateHostBuilder(args).Build().Run();

        }

        //public static IWebHostBuilder CreateHostBuilder(string[] args)
        //{

        //    var config = new ConfigurationBuilder()
        //                    .AddJsonFile("hostsettings.json", optional: true)
        //                    .AddCommandLine(args)                            
        //                    .Build();

        //    return WebHost.CreateDefaultBuilder(args)
        //                    .CaptureStartupErrors(true)
        //                    .UseUrls("http://*:5000;https://*:5001")
        //                    .UseConfiguration(config)
        //                    .UseKestrel( options => {})
        //                    .UseStartup(typeof(webapi.Program).Assembly.FullName);
        //}
    }
}
