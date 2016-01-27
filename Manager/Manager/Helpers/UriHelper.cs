using System;

namespace Stardust.Manager.Helpers
{
    public static class UriHelper
    {
        public static Uri CreateCorrectUri(string host,
                                           string api)
        {
            Uri uriToReturn;

            if (!host.EndsWith("/") && !api.StartsWith("/"))
            {
                uriToReturn = new Uri(host + "/" + api);
            }
            else
            {
                uriToReturn = new Uri(host + api);
            }

            return uriToReturn;
        }
    }
}