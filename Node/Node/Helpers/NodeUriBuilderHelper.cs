using System;
using Stardust.Node.Constants;

namespace Stardust.Node.Helpers
{
    public class NodeUriBuilderHelper
    {
        public NodeUriBuilderHelper(string locationUri)
        {
            LocationUri = new Uri(locationUri);

            if (string.IsNullOrEmpty(LocationUri.Scheme))
            {
                throw new ArgumentNullException();
            }

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

        public Uri CreateUri(string template,
                             Guid guid)
        {
            UriBuilder.Path = template.Replace(NodeRouteConstants.JobIdOptionalParameter,
                                               guid.ToString());

            return UriBuilder.Uri;
        }


        private UriBuilder UriBuilder { get; set; }

        private Uri LocationUri { get; set; }

        public NodeUriBuilderHelper(Uri locationUri) : this(locationUri.ToString())
        {
        }
    }
}