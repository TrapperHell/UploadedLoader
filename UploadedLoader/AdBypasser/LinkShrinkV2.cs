using System.Threading.Tasks;
using UploadedLoader.Http;

namespace UploadedLoader.AdBypasser
{
    public class LinkShrinkV2 : LinkShrinkV1
    {
        public override int Priority
        {
            get { return 1; }
        }

        public override async Task<string> BypassAd(string adUrl)
        {
            string code = null;

            var requestHandler = HttpRequestHandler.NewWithCookies();

            using (var response = await new HttpClient().GetResponse(adUrl, requestHandler))
            {
                var respContent = await response.Content.ReadAsStringAsync();

                var text = "document.getElementById(\"btd\").href = revC(\"";
                var ix = respContent.IndexOf(text) + text.Length;

                code = respContent.Substring(ix, respContent.IndexOf('"', ix) - ix);
                code = WebLogic.Decode(code);
            }

            requestHandler.AllowRedirects = false;
            requestHandler.Referrer = adUrl;
            // This magic cookie has to be present...
            requestHandler.Cookies.Add(new System.Net.Cookie("s32", "1", "/", DomainName));

            using (var response = await new HttpClient().GetResponse($"https://{DomainName}/{code}", requestHandler))
            {
                if (response.Headers.Location != null)
                    return response.Headers.Location.ToString();
            }

            return null;
        }



        /// <summary>
        /// Adaptation of Javascript logic found on target website
        /// </summary>
        private static class WebLogic
        {
            private static string _keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

            internal static string Encode(string value)
            {
                var encode = string.Empty;
                char n, r, i;
                int x = 0;

                value = Utf8Encode(value);

                while (x < value.Length)
                {
                    n = value[x++];
                    r = value[x++];
                    i = value[x++];

                    encode += _keyStr[n >> 2]; // s
                    encode += _keyStr[(n & 3) << 4 | r >> 4]; // o

                    encode += _keyStr[char.IsDigit(r) ? ((r & 15) << 2 | i >> 6) : 64]; // u
                    encode += _keyStr[char.IsDigit(r) && char.IsDigit(i) ? i & 63 : 64]; // a
                }

                return encode;
            }

            internal static string Decode(string value)
            {
                var decode = string.Empty;
                int s, o, u, a, x = 0;

                while (x < value.Length)
                {
                    s = _keyStr.IndexOf(value[x++]);
                    o = _keyStr.IndexOf(value[x++]);
                    u = _keyStr.IndexOf(value[x++]);
                    a = _keyStr.IndexOf(value[x++]);

                    decode += (char)(s << 2 | o >> 4); // n

                    if (u != 64)
                        decode += (char)((o & 15) << 4 | u >> 2); // r

                    if (a != 64)
                        decode += (char)((u & 3) << 6 | a); // i
                }

                return Utf8Decode(decode);
            }

            private static string Utf8Encode(string e)
            {
                e = e.Replace("rn", "n");

                var t = string.Empty;

                for (var n = 0; n < e.Length; n++)
                {
                    var r = e[n];

                    if (r < 128)
                        t += r;
                    else if (r > 127 && r < 2048)
                    {
                        t += (char)(r >> 6 | 192);
                        t += (char)(r & 63 | 128);
                    }
                    else
                    {
                        t += (char)(r >> 12 | 224);
                        t += (char)(r >> 6 & 63 | 128);
                        t += (char)(r & 63 | 128);
                    }
                }

                return e;
            }

            private static string Utf8Decode(string e)
            {
                var t = string.Empty;
                int n = 0, r, c1, c2;

                while (n < e.Length)
                {
                    r = e[n];

                    if (r < 128)
                    {
                        t += (char)r;
                        n++;
                    }
                    else if (r > 191 && r < 224)
                    {
                        c2 = e[n + 1];
                        t += (char)((r & 31) << 6 | c2 & 63);
                        n++;
                    }
                    else
                    {
                        c2 = e[n + 1];
                        c1 = e[n + 2];
                        t += (char)((r & 15) << 12 | (c2 & 63) << 6 | c1 & 63);
                        n += 3;
                    }
                }

                return t;
            }
        }
    }
}