using System.Net.Http;
using System.Threading.Tasks;

namespace UploadedLoader.Http
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetResponse(string url);

        Task<HttpResponseMessage> GetResponse(string url, HttpRequestHandler requestHandler);
    }
}