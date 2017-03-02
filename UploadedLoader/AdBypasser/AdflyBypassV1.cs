using System;
using System.Text;
using System.Threading.Tasks;
using UploadedLoader.Http;

namespace UploadedLoader.AdBypasser
{
    public class AdflyBypassV1 : IAdBypasser
    {
        protected const string DomainName = "adf.ly";

        public int Priority
        {
            get { return 1; }
        }

        public bool CanBypass(string adUrl)
        {
            return adUrl != null && adUrl.ToLowerInvariant().Contains(DomainName);
        }

        /// <summary>
        /// Retrieved from: https://github.com/jkehler/unshortenit
        /// </summary>
        public async Task<string> BypassAd(string adUrl)
        {
            using (var response = await new HttpClient().GetResponse(adUrl))
            {
                var http = await response.Content.ReadAsStringAsync();

                var ix = http.IndexOf("var ysmm = '") + 12;
                var ysmm = http.Substring(ix, http.IndexOf("';", ix) - ix);

                var leftAppend = "";
                var rightPrepand = "";

                for (int i = 0; i < ysmm.Length; i++)
                {
                    if (i % 2 == 0)
                        leftAppend += ysmm[i];
                    else
                        rightPrepand = ysmm[i] + rightPrepand;
                }

                var key = leftAppend + rightPrepand;
                key = Encoding.UTF8.GetString(Convert.FromBase64String(key));

                return key.Substring(2);
            }
        }
    }
}