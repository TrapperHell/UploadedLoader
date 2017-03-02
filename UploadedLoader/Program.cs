using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TinyIoC;
using UploadedLoader.Debridder;
using UploadedLoader.Http;

namespace UploadedLoader
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Uploaded.net Loader >>>>>>";

            using (var iocContainer = TinyIoCContainer.Current)
            {
                // Want to use a proxy? Cheeky... Note: Proxy is only considered by IDebrid implementation
                iocContainer.Register<IWebProxy>((IWebProxy)null);

                iocContainer.Register<IHttpClient, HttpClient>();
                iocContainer.Register<IDebrid, DebridNetV1>();

                Console.WriteLine("UploadedLoader is a url-shortener ad-bypass proof-of-concept.");
                Console.WriteLine("\t...using Debridnet.com to download from Uploaded.net");
                Console.WriteLine();
                Console.Write("Uploaded.net URL: ");

                string url = Console.ReadLine();

                if (UploadedStatusChecker.CheckStatus(url).Result)
                {
                    var directDownloadUrl = iocContainer.Resolve<IDebrid>().GetDirectDownloadUrl(url).Result;

                    if (directDownloadUrl.ResponseType == ResponseTypes.OK)
                        DownloadFile(directDownloadUrl.ResolvedUrl).Wait();
                    else
                    {
                        var exceptionBuilder = new StringBuilder();
                        GetExceptionInnerMessages(directDownloadUrl.Error, ref exceptionBuilder);

                        Console.WriteLine($"Unable to download file. [{directDownloadUrl.ResponseType}]: {directDownloadUrl.ResponseText ?? exceptionBuilder.ToString()}");
                    }
                }
                else
                    Console.WriteLine("Invalid URL or file is no longer available.");
            }

            Console.ReadLine();
        }



        private static async Task DownloadFile(string directDownloadUrl)
        {
            object syncLock = new object();

            using (var wc = new WebClient())
            {
                var percentage = 0;
                wc.DownloadProgressChanged += (s, e) =>
                {
                    if (e.ProgressPercentage <= percentage)
                        return;

                    lock (syncLock)
                    {
                        percentage = e.ProgressPercentage;

                        Console.CursorLeft = 0;
                        Console.Write($"Downloading... {e.ProgressPercentage}% complete\t\t");
                    }
                };

                wc.DownloadDataCompleted += (s, e) =>
                {
                    Console.CursorLeft = 0;

                    if (!e.Cancelled && e.Error == null)
                    {
                        var fileName = GetFileName(wc.ResponseHeaders["Content-Disposition"]);

                        using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            fs.Write(e.Result, 0, e.Result.Length);
                        }

                        Console.Write("Download complete!\t\t\t");
                    }
                    else
                        Console.Write($"Download {(e.Cancelled ? "cancelled" : $"failed - {e.Error?.Message}")}\t\t");
                };

                await wc.DownloadDataTaskAsync(directDownloadUrl);
            }
        }

        /// <summary>
        /// Concatenates the provided exception any all inner exceptions within it.
        /// </summary>
        /// <param name="ex">
        /// The exception whose message is to be retrieved.
        /// </param>
        /// <param name="sb">
        /// A StringBuilder containing all concatenated messages.
        /// </param>
        /// <param name="separator">
        /// The separator between each inner exception message.
        /// </param>
        private static void GetExceptionInnerMessages(Exception ex, ref StringBuilder sb, string separator = " -> ")
        {
            if (ex == null)
                return;

            sb.Append(ex.Message);

            if (ex.InnerException != null)
            {
                sb.Append(separator);
                GetExceptionInnerMessages(ex.InnerException, ref sb, separator);
            }
        }

        /// <summary>
        /// Gets the file name from a Content-Disposition header (or provided default). If the name
        /// already exists, a unique file name is attempted.
        /// </summary>
        /// <param name="contentDispositionHeader">
        /// The Content-Disposition header from which to retrieve the file name.
        /// </param>
        /// <param name="defaultName">
        /// A default fallback file name to be used if header does not contain name.
        /// </param>
        /// <returns>
        /// Returns the Content-Disposition header or fallback filename.
        /// </returns>
        private static string GetFileName(string contentDispositionHeader, string defaultName = "File.dat")
        {
            var ix = contentDispositionHeader?.IndexOf("filename=") + 9;

            contentDispositionHeader = (!ix.HasValue || ix.Value < 9) ? defaultName : contentDispositionHeader.Substring(ix.Value);
            var fileName = Path.GetFileNameWithoutExtension(contentDispositionHeader);
            var extension = Path.GetExtension(contentDispositionHeader);

            ix = 0;

            do
            {
                var name = (ix == 0 ? $"{fileName}{extension}" : $"{fileName} - Copy {ix}{extension}");

                if (!File.Exists(name) || ++ix >= 1000)
                    return name;

            } while (true);
        }
    }
}