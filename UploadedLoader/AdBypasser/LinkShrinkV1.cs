using System;
using System.Threading.Tasks;
using UploadedLoader.Http;

namespace UploadedLoader.AdBypasser
{
    [Obsolete]
    public class LinkShrinkV1 : IAdBypasser
    {
        protected const string DomainName = "linkshrink.net";

        public virtual int Priority
        {
            get { return 1; }
        }

        public virtual bool CanBypass(string adUrl)
        {
            return adUrl != null && adUrl.ToLowerInvariant().Contains(DomainName);
        }

        public virtual async Task<string> BypassAd(string adUrl)
        {
            string url = null;

            var requestHandler = HttpRequestHandler.NewWithCookies();

            using (var response = await new HttpClient().GetResponse(adUrl, requestHandler))
            {
                var respContent = await response.Content.ReadAsStringAsync();

                var ix = respContent.IndexOf("g.href = \"") + 10;
                url = respContent.Substring(ix, respContent.IndexOf("\"", ix + 1) - ix);

                // Navigating directly to the resolved code URL redirects to HTTPS alternative, so we can skip that...
                url = url.Replace("http:", "https:");
            }

            requestHandler.AllowRedirects = false;
            requestHandler.Referrer = adUrl;

            using (var response = await new HttpClient().GetResponse(url, requestHandler))
            {
                if (response.Headers.Location != null)
                    return response.Headers.Location.ToString();
            }

            return null;
        }
    }
}