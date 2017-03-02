using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UploadedLoader.Http;

namespace UploadedLoader.Debridder
{
    public class DebridNetV1 : IDebrid
    {
        private readonly IHttpClient _httpClient;
        private readonly IWebProxy _webProxy;

        public DebridNetV1(IHttpClient httpClient, IWebProxy webProxy)
        {
            _httpClient = httpClient;
            _webProxy = webProxy;
        }

        /// <summary>
        /// Extracts the Uploaded.net file Id.
        /// </summary>
        /// <param name="uploadedUrl">
        /// The Uploaded.net url.
        /// </param>
        /// <returns>
        /// Returns the Uploaded.net file Id.
        /// </returns>
        protected virtual string GetLinkId(string uploadedUrl)
        {
            return Regex.Match(uploadedUrl, @"(?:uploaded.net\/file|ul.to)\/(?<id>[\da-z]{8})(?:\/)?").Groups["id"].Value;
        }

        public async Task<UrlExtractionResult> GetDirectDownloadUrl(string url)
        {
            var fileId = GetLinkId(url);

            var requestHandler = HttpRequestHandler.NewWithCookies();
            /*
             * The website now checks that the browser is Google Chrome - however the check only sees that 'Chrome' is present,
             * regardless that this UserAgent string is forged.
            */
            requestHandler.UserAgent = "Chrome";
            requestHandler.Referrer = null;
            requestHandler.Proxy = _webProxy;
            // Initial request is technically done through a form submission (ie. POST), but GET seems to work just as well...
            requestHandler.Method = HttpMethod.Get;

            url = $"http://debridnet.com/luxboxit?link={fileId}";
            AdUrlExtractionResult adInfo = null;

            /*
             * Steps are in the following order:
             * 
             * Step #1 - Submit Link
             * Step #2 - Solve
             * Step #3 - Code
             * Step #4 - Check
             * Step #5 - Agree (Sometimes this step is skipped)
             * Step #6 - Generate
             * step #7 - Download
             * 
             * Each step directs to a paid URL shortener. Every step - except for the first - has the shortened URL
             * within an anchor with the name 'linkadf'.
            */

            for (int i = 0; i < 7 && !url.ToLower().StartsWith("http://debridnet.com/?file="); i++)
            {
                adInfo = await ExtractNextUrl(url, i == 0 ? null : "linkadf", requestHandler);

                if (adInfo.ResponseType != ResponseTypes.OK)
                    return adInfo;

                url = adInfo.ResolvedUrl;
                requestHandler.Referrer = adInfo.AdLink;
            }

            return adInfo;
        }

        protected async Task<AdUrlExtractionResult> ExtractNextUrl(string url, string anchorId, HttpRequestHandler requestHandler)
        {
            string respContent;

            try
            {
                using (var response = await _httpClient.GetResponse(url, requestHandler))
                {
                    respContent = await response.Content.ReadAsStringAsync();
                }

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(respContent);

                var adLink = doc.DocumentNode.SelectSingleNode(anchorId == null ? "//a" : $"//a[@id = '{anchorId}']")?.GetAttributeValue("href", null);

                if (adLink == null && anchorId != null)
                {
                    // Sometimes the anchor ref isn't set in the DOM, but through Javascript
                    var jsText = $"document.getElementById('{anchorId}').action = '";

                    var ix = respContent.IndexOf(jsText) + jsText.Length;

                    if (ix < jsText.Length)
                    {
                        // If no Javascript reference can be found, check for an error message...
                        var responseText = doc.DocumentNode.SelectNodes("//div[@class='col_33']/h2")?.FirstOrDefault()?.NextSibling?.InnerText?.Trim('\r', '\n', ' ');

                        // Sometimes an empty response is returned. In this case the request should be retried...
                        if (string.IsNullOrWhiteSpace(responseText))
                            return await ExtractNextUrl(url, anchorId, requestHandler);

                        return new AdUrlExtractionResult() { ResponseType = GetResponseType(responseText), ResponseText = responseText };
                    }

                    adLink = respContent.Substring(ix, respContent.IndexOf("'", ix) - ix);
                }
                else if (adLink == null)
                    throw new InvalidOperationException("Unable to extract url of element without Id.");

                url = await AdBypasser.AdBypasserService.BypassAd(adLink);

                return new AdUrlExtractionResult(url, adLink);
            }
            catch (HttpRequestException hrex)
            {
                return new AdUrlExtractionResult() { ResponseType = ResponseTypes.ConnectionError, Error = hrex };
            }
            catch (Exception ex)
            {
                return new AdUrlExtractionResult() { Error = ex };
            }
        }

        private ResponseTypes GetResponseType(string responseText)
        {
            switch (responseText)
            {
                // Only one response seems to be returned - regardless if it's the server's or user's account limit
                case "Sorry, The bandwidth of this account has reached his limit temporary.":
                    return ResponseTypes.LimitReached;
                default:
                    return ResponseTypes.UnknownError;
            }
        }
    }
}