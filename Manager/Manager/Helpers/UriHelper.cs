using System;

namespace Stardust.Manager.Helpers
{
    public static class UriHelper
    {
        public static Uri CreateCorrectUri(string host,
                                           string path)
        {
            UriBuilder uriBuilder = new UriBuilder();

            uriBuilder.Host = host;
            uriBuilder.Path = path;

            return uriBuilder.Uri;
        }
    }
}