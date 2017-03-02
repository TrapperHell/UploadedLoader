using System.Threading.Tasks;

namespace UploadedLoader.Debridder
{
    public interface IDebrid
    {
        Task<UrlExtractionResult> GetDirectDownloadUrl(string url);
    }
}