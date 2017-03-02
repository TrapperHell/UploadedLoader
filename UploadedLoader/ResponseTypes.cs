namespace UploadedLoader
{
    public enum ResponseTypes
    {
        OK = 0,
        UnknownError = 1,
        ConnectionError = 1 << 1,
        NotFound = 1 << 2,
        ServerError = 1 << 3,
        LimitReached = 1 << 4
    }
}