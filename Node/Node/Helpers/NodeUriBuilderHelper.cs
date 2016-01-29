using System;
using Stardust.Node.Constants;
using Stardust.Node.Extensions;

namespace Stardust.Node.Helpers
{
    public class NodeUriBuilderHelper
    {
        public NodeUriBuilderHelper(string location)
        {
            location.ThrowArgumentNullExceptionIfNullOrEmpty();

            UriBuilder = new UriBuilder(location);

            UriBuilder.Scheme.ThrowArgumentNullExceptionIfNullOrEmpty();
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

        public NodeUriBuilderHelper(Uri locationUri) : this(locationUri.ToString())
        {
        }
    }
}