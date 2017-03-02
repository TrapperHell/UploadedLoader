using System.Net.Http;
using System.Threading.Tasks;
using UploadedLoader.Http;

namespace UploadedLoader.AdBypasser
{
    public class ViidmeBypassV2 : ViidmeBypassV1
    {
        public override int Priority
        {
            get { return 2; }
        }

        public override async Task<string> BypassAd(string adUrl)
        {
            var requestHandler = HttpRequestHandler.New();
            requestHandler.AllowRedirects = false;
            requestHandler.Method = HttpMethod.Head;

            using (var response = await new Http.HttpClient().GetResponse(adUrl, requestHandler))
            {
                // How nice of them... If no User-Agent is provided, the location is returned immediately
                return response.Headers.Location.ToString();
            }
        }
    }
}