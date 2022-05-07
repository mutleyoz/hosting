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
            var (webhost, _) = host.BuildHost(typeof(webapi.Program).Assembly);

            webhost.RunAsync();               

            Console.WriteLine("Started host");
            Console.Read();
        }
    }
}
