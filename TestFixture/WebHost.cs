using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace Fixture
{
    public class ClientProxy
    {
        public string ConfigurationKey { get; set; }

        public string Url { get; set; }

        public RequestDelegate RequestDelegate { get; set; }

        public IWebHost WebHost { get; set; }
    }

    public class TestFixture : IDisposable
    {
        public List<ClientProxy> Proxies = new List<ClientProxy>();

        public IWebHost _webhost {get;set;}

        public (IWebHost, string) Build(Assembly assembly, IConfigurationRoot config = null)
        {
            var _config = config ?? new ConfigurationBuilder().AddJsonFile("hostsettings.json", optional: true).Build();
            var _url = FetchNextAvailableUrl();

            var builder = WebHost.CreateDefaultBuilder()
                            .CaptureStartupErrors(true)
                            .UseUrls(_url)
                            .UseKestrel( options => {})
                            .UseConfiguration(_config)
                            .UseStartup(assembly.FullName);

            builder.ConfigureAppConfiguration((context, config) =>
            {
                var proxies = Proxies.Select(p => new KeyValuePair<string, string>(p.ConfigurationKey, p.Url)).ToList();                                   
                config.AddInMemoryCollection(proxies);
            });

            _webhost = builder.Build();
            Console.WriteLine($"Configured api under test for {_url}");
            return (_webhost, _url);
        }

        public TestFixture AddDependencyUrl(string configurationEntry, RequestDelegate requestDelegate)
        {
            Proxies.Add(new ClientProxy {  ConfigurationKey = configurationEntry, 
                                           RequestDelegate = requestDelegate,
                                           Url = FetchNextAvailableUrl() 
                                        });
            return this;
        }

        public void Start()
        {
            foreach(var proxy in Proxies)
            {
                proxy.WebHost = WebHost.Start(proxy.Url, proxy.RequestDelegate);
                Console.WriteLine($"Started listening to '{proxy.Url}' for {proxy.ConfigurationKey}");
            }
            _webhost.StartAsync();
        }

        private string FetchNextAvailableUrl() => $"http://*:{FetchNextAvailablePort()}";

        private int FetchNextAvailablePort()
        {
            using( var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.IPv6Loopback, 0));
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }

        public void Dispose()
        {
            foreach(var proxy in Proxies)
            {
                Task.FromResult(proxy.WebHost.StopAsync());
                proxy?.WebHost.Dispose();
            }

            Task.FromResult(_webhost.StopAsync());
            _webhost?.Dispose();
        }
    }
}
