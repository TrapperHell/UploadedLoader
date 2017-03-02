using System;
using System.Net.Http;
using System.Threading.Tasks;
using UploadedLoader.Http;

namespace UploadedLoader.Debridder
{
    [Obsolete("This implementation was used in ~November 2016.", true)]
    public class LiveDebridV2 : IDebrid
    {
        public async Task<UrlExtractionResult> GetDirectDownloadUrl(string url)
        {
            var link = WebLogic.Decode(url);

            var requestHandler = HttpRequestHandler.NewWithCookies();
            requestHandler.Referrer = $"http://livedebrid.com/generate?link={link}";
            requestHandler.Method = HttpMethod.Post;

            try
            {
                using (var response = await new Http.HttpClient().GetResponse($"http://livedebrid.com/download?link={link}", requestHandler))
                {
                    var http = await response.Content.ReadAsStringAsync();

                    var ix = http.IndexOf("http://adf.ly");

                    if (ix >= 0)
                    {
                        var downloadUrl = http.Substring(ix, http.IndexOf("'", ix) - ix);
                        return await AdBypasser.AdBypasserService.BypassAd(downloadUrl);
                    }
                    else if (http.Contains("You only can download "))
                        return ResponseTypes.LimitReached;
                    else
                        return ResponseTypes.UnknownError;
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
        }



        /// <summary>
        /// Direct copy of Javascript logic found on target website
        /// </summary>
        private static class WebLogic
        {
            private static Random _randomizer = new Random();

            internal static string Decode(string value)
            {
                // Direct copy from LiveDebrid JS implementation
                var res = value.Replace("http://ul.to/", "");
                var pes = res.Replace("http://uploaded.net/file/", "");
                var kes = pes.Substring(0, 8);

                var ml = Convert.ToInt64(_randomizer.NextDouble() * 10000000000000);
                var strml = ml.ToString().PadLeft(13, '0');
                var m1 = strml.Substring(0, 2);
                var m2 = strml.Substring(2, 2);
                var m3 = strml.Substring(4, 2);
                var m4 = strml.Substring(6, 2);
                var mkes1 = kes.Substring(0, 2);
                var mkes2 = kes.Substring(2, 2);
                var mkes3 = kes.Substring(4, 2);
                var mkes4 = kes.Substring(6, 2);

                return strml + mkes1 + m1 + mkes2 + m2 + mkes3 + m3 + mkes4 + m4;
            }
        }
    }
}