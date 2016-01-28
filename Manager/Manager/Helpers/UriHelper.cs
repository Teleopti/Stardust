using System;

namespace Stardust.Manager.Helpers
{
    public static class UriHelper
    {
        public static Uri CreateCorrectUri(string host,
                                           string path)
        {
            UriBuilder uriBuilder = new UriBuilder
            {
                Host = host,
                Path = path
            };

            return uriBuilder.Uri;
        }
    }
}