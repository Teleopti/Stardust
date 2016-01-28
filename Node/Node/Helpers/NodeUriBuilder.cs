using System;

namespace Stardust.Node.Helpers
{
    public class NodeUriBuilder
    {
        public NodeUriBuilder(string locationUri)
        {
            LocationUri = new Uri(locationUri);

            UriBuilder = new UriBuilder
            {
                Host = LocationUri.Host,
                Port = LocationUri.Port,
                Scheme = LocationUri.Scheme
            };
        }

        public Uri CreateUri(string path)
        {
            UriBuilder.Path = path;

            return UriBuilder.Uri;
        }

        private UriBuilder UriBuilder { get; set; }

        private Uri LocationUri { get; set; }

        public NodeUriBuilder(Uri locationUri) : this(locationUri.ToString())
        {
        }
    }
}