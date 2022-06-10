using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;

namespace TestFixture
{
    internal class ClientProxy
    {
        public string ConfigurationKey { get; set; }

        public Uri Uri { get; set; }

        public RequestDelegate RequestDelegate { get; set; }

        public HttpMessageHandler Handler { get; set; }

        public Type ProxyInterface { get; set; }

        public Type ProxyImplementation { get; set; }
    }

    public class HttpTestFixture : IDisposable
    {
        private List<ClientProxy> _proxies = new List<ClientProxy>();

        public HttpClient Client { get; set; }

        /// <summary>
        /// Configure web server for unit under test, update appsettings entries to replace endpoints with proxy web servers. Add HttpClient for each proxy.
        /// </summary>
        /// <param name="assembly"></param>
        public HttpTestFixture Build(Assembly assembly)
        {
            var _config = new ConfigurationBuilder()
                    .AddJsonFile("hostsettings.json", optional: true)
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

            var _uri = FetchNextAvailableUrl();

            var builder = WebHost.CreateDefaultBuilder()
                            .CaptureStartupErrors(true)
                            .UseUrls(_uri.ToString())
                            .UseConfiguration(_config)
                            .UseStartup(assembly.FullName)
                            .ConfigureTestServices(services => { _proxies.ForEach(proxy => AddHttpClient(services, proxy)); });

            // Substitute appSettings endpoint settings for proxy web servers
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var proxies = _proxies.Select(p => new KeyValuePair<string, string>(p.ConfigurationKey, p.Uri.ToString())).ToList();
                config.AddInMemoryCollection(proxies);
            });

            Client = new TestServer(builder).CreateClient();

            return this;
        }

        /// <summary>
        /// Configure web server for proxy with response delegate and add to proxy collection.
        /// </summary>
        /// <typeparam name="IProxy"></typeparam>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="configurationEntry"></param>
        /// <param name="requestDelegate"></param>
        public HttpTestFixture AddProxy<IProxy, TProxy>(string configurationEntry, RequestDelegate requestDelegate) where TProxy : class, IProxy where IProxy : class
        {
            var _config = new ConfigurationBuilder().AddJsonFile("hostsettings.json", optional: true).Build();
            var _uri = FetchNextAvailableUrl();

            var builder = WebHost.CreateDefaultBuilder()
                            .CaptureStartupErrors(true)
                            .UseUrls(_uri.ToString())
                            .UseConfiguration(_config)
                            .Configure(app =>
                            {
                                app.Run(context => requestDelegate.Invoke(context));
                            });

            _proxies.Add(new ClientProxy
            {
                ConfigurationKey = configurationEntry,
                RequestDelegate = requestDelegate,
                ProxyInterface = typeof(IProxy),
                ProxyImplementation = typeof(TProxy),
                Uri = _uri,
                Handler = new TestServer(builder).CreateHandler()
            });

            return this;
        }

        public void Dispose()
        {
            Client?.Dispose();

            _proxies?.Clear();
        }

        /// <summary>
        /// Invoke AddHttpClient with the types registered in client proxy and attach message handler
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="clientProxy"></param>
        private void AddHttpClient(IServiceCollection serviceCollection, ClientProxy clientProxy)
        {
            var addHttpClientMethod = GetAddHttpClientMethod();
            var genericMethod = addHttpClientMethod.MakeGenericMethod(clientProxy.ProxyInterface, clientProxy.ProxyImplementation);
            var builder = (IHttpClientBuilder)genericMethod.Invoke(serviceCollection, new object[] { serviceCollection });
            builder?.ConfigurePrimaryHttpMessageHandler(_ => clientProxy.Handler);
        }

        private Uri FetchNextAvailableUrl() => new Uri($"http://localhost:{FetchNextAvailablePort()}");

        private int FetchNextAvailablePort()
        {
            using (var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.IPv6Loopback, 0));
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }

        private MethodInfo GetAddHttpClientMethod()
        {
            var assembly = Assembly.Load("Microsoft.Extensions.Http");

            return assembly.GetTypes()
                            .SelectMany(t => t.GetMethods())
                            .Where(m => m.Name == "AddHttpClient"
                                && m.GetGenericArguments().Count() == 2
                                && m.GetParameters().Count() == 1
                                && m.GetParameters()[0].ParameterType == typeof(IServiceCollection))
                            .First();
        }
    }
}
