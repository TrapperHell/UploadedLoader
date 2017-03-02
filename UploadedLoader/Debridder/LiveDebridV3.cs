using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UploadedLoader.Http;

namespace UploadedLoader.Debridder
{
    [Obsolete("This implementation was used in ~December 2016.", true)]
    public class LiveDebridV3 : IDebrid
    {
        public async Task<UrlExtractionResult> GetDirectDownloadUrl(string url)
        {
            var link = Regex.Match(url, @"(?:uploaded.net\/file|ul.to)\/(?<id>[\da-z]{8})(?:\/)?").Groups["id"].Value;

            // Step #1 - Submit Link
            url = await ExtractNextUrl($"http://livedebrid.com/luxboxit?link={link}", HttpMethod.Post, "a href='", "'");

            // Steps #2 - #3 - Solve, Code, Code-Copy, Check, Agree, Generate, Download, File
            for (int i = 0; i < 7; i++)
            {
                url = await ExtractNextUrl(url, HttpMethod.Get, "document.getElementById('linkadf').href = '", "'");
            }

            return url;
        }

        protected async Task<string> ExtractNextUrl(string url, HttpMethod method, string htmlSearchStart, string htmlSearchEnd = null)
        {
            string respContent;

            var requestHandler = HttpRequestHandler.New();
            requestHandler.Method = method;

            using (var response = await new Http.HttpClient().GetResponse(url, requestHandler))
            {
                respContent = await response.Content.ReadAsStringAsync();

                var ix = respContent.IndexOf(htmlSearchStart) + htmlSearchStart.Length;

                if (ix < htmlSearchStart.Length)
                    throw new InvalidOperationException("Unable to extract url");

                var adLink = respContent.Substring(ix, respContent.IndexOf(htmlSearchEnd, ix) - ix);
                return await AdBypasser.AdBypasserService.BypassAd(adLink);
            }
        }
    }
}