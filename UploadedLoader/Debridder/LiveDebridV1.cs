using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UploadedLoader.Http;

namespace UploadedLoader.Debridder
{
    [Obsolete("This implementation was used in ~July 2016.", true)]
    public class LiveDebridV1 : IDebrid
    {
        /*
         * The actual way of doing this from the website would be as follows:
         * 
         * POST /solve link=*LINK-HERE*
         * POST /code
         * POST /check
         * POST /agree
         * POST /generate checkbox=check
         * POST /download
         * 
         * The Referer must be updated every time to match the previous page.
        */

        public async Task<UrlExtractionResult> GetDirectDownloadUrl(string url)
        {
            url = WebUtility.UrlEncode(url);

            var requestHandler = HttpRequestHandler.NewWithCookies();
            requestHandler.Referrer = "http://livedebrid.com/";

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, "http://livedebrid.com/solve"))
                {
                    request.Content = new StringContent($"link={url}", Encoding.UTF8, "application/x-www-form-urlencoded");

                    // By visiting the below URL, we get a PHPSESSID cookie which will be needed to download the file
                    using (var response = await new Http.HttpClient().GetResponse(url, requestHandler))
                    { }

                    requestHandler.Referrer = "http://livedebrid.com/generate";
                }

                // We can skip the rest of the pages, and go directly to the download page.
                using (var request = new HttpRequestMessage(HttpMethod.Post, "http://livedebrid.com/download"))
                {
                    request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/x-www-form-urlencoded");

                    using (var response = await new Http.HttpClient().GetResponse(url, requestHandler))
                    {
                        var http = await response.Content.ReadAsStringAsync();

                        var linkStartIndex = http.IndexOf("http://dl.livedebrid.com/");
                        if (linkStartIndex >= 0)
                        {
                            var downloadUrl = http.Substring(linkStartIndex, http.IndexOf("'", linkStartIndex) - linkStartIndex);
                            return await AdBypasser.AdBypasserService.BypassAd(downloadUrl);
                        }
                        else if (http.Contains("You only can download "))
                            return ResponseTypes.LimitReached;
                        else
                            return ResponseTypes.UnknownError;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}