using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace UploadedLoader.Http
{
    public class HttpRequestHandler
    {
        private HttpRequestHandler()
        {
            Accept = new List<string>();
        }

        public List<string> Accept { get; set; }

        public bool AllowRedirects { get; set; } = true;

        public HttpCompletionOption CompletionOption { get; set; } = HttpCompletionOption.ResponseContentRead;

        public CookieContainer Cookies { get; set; }

        public HttpMethod Method { get; set; } = HttpMethod.Get;

        public IWebProxy Proxy { get; set; }

        public string Referrer { get; set; }

        public string UserAgent { get; set; }



        public static HttpRequestHandler New()
        {
            return new HttpRequestHandler();
        }

        public static HttpRequestHandler NewWithCookies()
        {
            return new HttpRequestHandler() { Cookies = new CookieContainer() };
        }

        public static implicit operator HttpClientHandler(HttpRequestHandler request)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                Proxy = request.Proxy,
                UseProxy = request.Proxy != null,

                AllowAutoRedirect = request.AllowRedirects,
                UseCookies = true,
                UseDefaultCredentials = false
            };

            if (request.Cookies != null)
                handler.CookieContainer = request.Cookies;

            return handler;
        }
    }
}