using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace UploadedLoader.Http
{
    public class HttpClient : IHttpClient
    {
        public Task<HttpResponseMessage> GetResponse(string url)
        {
            var requestHandler = HttpRequestHandler.New();

            return GetResponse(url, requestHandler);
        }

        public async Task<HttpResponseMessage> GetResponse(string url, HttpRequestHandler requestHandler)
        {
            using (var request = new HttpRequestMessage(requestHandler.Method, url))
            using (var client = new System.Net.Http.HttpClient(requestHandler))
            {
                if (!string.IsNullOrWhiteSpace(requestHandler.Referrer))
                    client.DefaultRequestHeaders.Referrer = new Uri(requestHandler.Referrer);

                if (!string.IsNullOrWhiteSpace(requestHandler.UserAgent))
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", requestHandler.UserAgent);

                if (requestHandler.Accept != null && requestHandler.Accept.Any())
                {
                    foreach (var accept in requestHandler.Accept)
                    {
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(accept));
                    }
                }

                return await client.SendAsync(request, requestHandler.CompletionOption);
            }
        }
    }
}