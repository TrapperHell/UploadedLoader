using System;

namespace UploadedLoader
{
    public class UrlExtractionResult
    {
        private Exception error;

        public string ResponseText { get; set; }

        public ResponseTypes ResponseType { get; set; }

        public string ResolvedUrl { get; set; }

        public Exception Error
        {
            get { return this.error; }
            set
            {
                if (ResponseType == ResponseTypes.OK && value != null)
                    ResponseType = ResponseTypes.UnknownError;

                this.error = value;
            }
        }



        public UrlExtractionResult()
        { }

        public UrlExtractionResult(Exception ex)
        {
            Error = ex;
        }

        public UrlExtractionResult(ResponseTypes responseType, Exception ex)
        {
            ResponseType = responseType;
            Error = ex;
        }

        public UrlExtractionResult(string resolvedUrl)
        {
            ResolvedUrl = resolvedUrl;
        }



        public static implicit operator UrlExtractionResult(Exception ex)
        {
            return new UrlExtractionResult(ex);
        }

        public static implicit operator UrlExtractionResult(ResponseTypes responseType)
        {
            return new UrlExtractionResult(responseType, null);
        }

        public static implicit operator UrlExtractionResult(string resolvedUrl)
        {
            return new UrlExtractionResult(resolvedUrl);
        }
    }

    public class AdUrlExtractionResult : UrlExtractionResult
    {
        public string AdLink { get; set; }



        public AdUrlExtractionResult()
        { }

        public AdUrlExtractionResult(string resolvedUrl, string adLink)
            : base(resolvedUrl)
        {
            AdLink = adLink;
        }
    }
}