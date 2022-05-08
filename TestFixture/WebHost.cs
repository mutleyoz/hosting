using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;

namespace HttpFixture
{
    internal class ClientProxy
    {
        public string ConfigurationKey { get; set; }

        public Uri Uri { get; set; }

        public RequestDelegate RequestDelegate { get; set; }

        public IWebHost WebHost { get; set; }
    }

    public static class TestFixture
    {
        private static List<ClientProxy> Proxies = new List<ClientProxy>();

        private static IWebHost _webhost {get; set;}

        public static HttpClient Client { get; set; }

        public static void Build(Assembly assembly, IConfigurationRoot config = null)
        {
            var _config = config ?? new ConfigurationBuilder().AddJsonFile("hostsettings.json", optional: true).Build();
            var _uri = FetchNextAvailableUrl();

            var builder = WebHost.CreateDefaultBuilder()
                            .CaptureStartupErrors(true)
                            .UseUrls(_uri.ToString())
                            .UseKestrel( options => {})
                            .UseConfiguration(_config)
                            .UseStartup(assembly.FullName);

            builder.ConfigureAppConfiguration((context, config) =>
            {
                var proxies = Proxies.Select(p => new KeyValuePair<string, string>(p.ConfigurationKey, p.Uri.ToString())).ToList();                                   
                config.AddInMemoryCollection(proxies);
            });

            _webhost = builder.Build();
            Client = new HttpClient { BaseAddress = _uri };
        }

        public static void AddService(string configurationEntry, RequestDelegate requestDelegate)
        {
            Proxies.Add(new ClientProxy {  ConfigurationKey = configurationEntry, 
                                           RequestDelegate = requestDelegate,
                                           Uri = FetchNextAvailableUrl() 
                                        });
        }

        public static void Start()
        {
            Proxies.ForEach(p => p.WebHost = WebHost.Start(p.Uri.ToString(), p.RequestDelegate));
            _webhost.StartAsync();
        }

        public static void Stop()
        {
            Client.Dispose();

            Proxies.ForEach(p =>
            {
                p.WebHost.StopAsync();
                p?.WebHost.Dispose();
            });

            _webhost.StopAsync();
            _webhost?.Dispose();
        }
        private static Uri FetchNextAvailableUrl() => new Uri($"http://localhost:{FetchNextAvailablePort()}");

        private static int FetchNextAvailablePort()
        {
            using (var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.IPv6Loopback, 0));
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }
    }
}
