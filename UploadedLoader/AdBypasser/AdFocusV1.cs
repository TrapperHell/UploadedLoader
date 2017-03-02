using System.Threading.Tasks;
using UploadedLoader.Http;

namespace UploadedLoader.AdBypasser
{
    public class AdFocusV1 : IAdBypasser
    {
        protected const string DomainName = "adfoc.us";

        public int Priority
        {
            get { return 1; }
        }

        public bool CanBypass(string adUrl)
        {
            return adUrl != null && adUrl.ToLowerInvariant().Contains(DomainName);
        }

        public async Task<string> BypassAd(string adUrl)
        {
            var requestHandler = HttpRequestHandler.New();
            requestHandler.Accept.Add("text/html");

            using (var response = await new HttpClient().GetResponse(adUrl, requestHandler))
            {
                var http = await response.Content.ReadAsStringAsync();

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(http);

                return doc.DocumentNode.SelectSingleNode("//span[@id='showSkip']/a").GetAttributeValue("href", string.Empty);
            }
        }
    }
}