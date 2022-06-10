using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace webapi.Proxies
{
    public interface IQuoteProxy
    {
        Task<HttpResponseMessage> FetchQuotes();
    }

    public class QuoteProxy : IQuoteProxy
    {
        private HttpClient _httpClient;
        private IConfiguration _config;

        public QuoteProxy(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _config = configuration;
        }

        public async Task<HttpResponseMessage> FetchQuotes()
        {
            if (_config is null) throw new NullReferenceException(nameof(_config));

            return await _httpClient.GetAsync(_config["QuoteApi:uri"]);
        }
    }
}
