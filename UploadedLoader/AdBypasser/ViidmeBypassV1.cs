using System;
using System.Linq;
using System.Threading.Tasks;
using UploadedLoader.Http;

namespace UploadedLoader.AdBypasser
{
    public class ViidmeBypassV1 : IAdBypasser
    {
        protected readonly string[] DomainNames = new string[] { "viid.me", "sh.st" };

        public virtual int Priority
        {
            get { return 1; }
        }

        public virtual bool CanBypass(string adUrl)
        {
            return adUrl != null && DomainNames.Any(dn => adUrl.ToLowerInvariant().Contains(dn));
        }

        public virtual async Task<string> BypassAd(string adUrl)
        {
            string url = null;

            var requestHandler = HttpRequestHandler.New();
            // Any UserAgent string seems to work...
            requestHandler.UserAgent = "Mozilla / 5.0";

            using (var response = await new HttpClient().GetResponse(adUrl, requestHandler))
            {
                var http = await response.Content.ReadAsStringAsync();

                var callbackUrl = "callbackUrl: \"";
                var ix = http.IndexOf(callbackUrl) + callbackUrl.Length;
                callbackUrl = http.Substring(ix, http.IndexOf("\",", ix) - ix);

                var sessionId = "sessionId: \"";
                ix = http.IndexOf(sessionId) + sessionId.Length;
                sessionId = http.Substring(ix, http.IndexOf("\",", ix) - ix);

                var time = Math.Round(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);

                url = $"{(adUrl.Substring(0, adUrl.LastIndexOf("/")))}{callbackUrl}?adSessionId={sessionId}&adbd=1&callback=reqwest_{time}";
            }

            // Unfortunately we have to wait a bit...
            await Task.Delay(5000);

            requestHandler.Referrer = adUrl;

            using (var locationResponse = await new HttpClient().GetResponse(url, requestHandler))
            {
                var json = await locationResponse.Content.ReadAsStringAsync();

                // Error-prone but lightweight JSON "parsing"
                var destinationUrl = "{\"destinationUrl\":\"";
                var ix = json.IndexOf(destinationUrl) + destinationUrl.Length;
                destinationUrl = json.Substring(ix, json.IndexOf("\",", ix) - ix).Replace("\\/", "/");

                return destinationUrl;
            }
        }
    }
}