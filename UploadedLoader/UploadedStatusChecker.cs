using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UploadedLoader
{
    public static class UploadedStatusChecker
    {
        public static async Task<bool> CheckStatus(string url)
        {
            if (!string.IsNullOrWhiteSpace(url) && Regex.IsMatch(url, @"(?:uploaded.net\/file|ul.to)\/(?:[\da-z]{8})(?:\/)?"))
            {
                using (var client = new HttpClient())
                {
                    var message = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url),
                        HttpCompletionOption.ResponseHeadersRead);

                    return message.StatusCode == HttpStatusCode.OK;
                }
            }
            else
                return false;
        }
    }
}