using System.Threading.Tasks;

namespace UploadedLoader.AdBypasser
{
    public interface IAdBypasser
    {
        int Priority { get; }

        bool CanBypass(string adUrl);

        Task<string> BypassAd(string adUrl);
    }
}