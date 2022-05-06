using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Fixture
{
    public class TestFixtureWebHost
    {
        public (IWebHost, string) StartHost(Assembly assembly, IConfigurationRoot config = null)
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
                var overrides = new List<KeyValuePair<string, string>>();
                overrides.Add(new KeyValuePair<string, string>("QuoteApi:uri", _url));

                config.AddInMemoryCollection(overrides);
            });

            return (builder.Build(), _url);
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
    }
}
