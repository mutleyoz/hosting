using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace TestFixture
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
