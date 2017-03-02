using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UploadedLoader.AdBypasser
{
    public static class AdBypasserService
    {
        private static List<IAdBypasser> _adBypassers;

        static AdBypasserService()
        {
            // Get an instance of all non-obsolete, non-abstract classes implementing IAdBypasser...
            _adBypassers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(IAdBypasser))
                    && t.IsClass && !t.IsAbstract
                    && t.GetCustomAttribute<ObsoleteAttribute>() == null)
                .Select(t => (IAdBypasser)Activator.CreateInstance(t))
                .ToList();
        }

        public static async Task<string> BypassAd(string adUrl)
        {
            foreach (var adBypasser in _adBypassers.Where(ab => ab.CanBypass(adUrl)).OrderByDescending(ab => ab.Priority))
            {
                var url = await adBypasser.BypassAd(adUrl);

                if (url != null)
                    return url;
            }

            throw new InvalidOperationException($"Unable to bypass ad: {adUrl}");
        }
    }
}