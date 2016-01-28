using System;
using Stardust.Manager.Constants;

namespace Stardust.Manager.Helpers
{
    public class NodeUriBuilderHelper
    {
        private UriBuilder UriBuilder { get; set; }

        private Uri LocationUri { get; set; }

        public NodeUriBuilderHelper(Uri locationUri) : this(locationUri.ToString())
        {
        }

        public NodeUriBuilderHelper(string locationUri)
        {
            LocationUri = new Uri(locationUri);

            UriBuilder = new UriBuilder
            {
                Host = LocationUri.Host,
                Port = LocationUri.Port,
                Scheme = LocationUri.Scheme
            };
        }

        public Uri GetIsAliveTemplateUri()
        {
            return CreateUri(NodeRouteConstants.IsAlive);
        }

        public Uri GetJobTemplateUri()
        {
            return CreateUri(NodeRouteConstants.Job);
        }

        public Uri GetCancelJobTemplateUri()
        {
            return CreateUri(NodeRouteConstants.CancelJob);
        }

        public Uri GetCancelJobUri(Guid guid)
        {
            var path = NodeRouteConstants.CancelJob.Replace(NodeRouteConstants.JobIdOptionalParameter,
                                                            guid.ToString());

            return CreateUri(path);
        }

        public Uri CreateUri(string path)
        {
            UriBuilder.Path = path;

            return UriBuilder.Uri;
        }

        public string GetHostName()
        {
            return UriBuilder.Host;
        }

        public int GetPort()
        {
            return UriBuilder.Port;
        }

        public string GetScheme()
        {
            return UriBuilder.Scheme;
        }

        public Uri GetLocationUri()
        {
            return UriBuilder.Uri;
        }
    }
}