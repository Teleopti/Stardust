using System;
using Stardust.Node.Constants;
using Stardust.Node.Extensions;

namespace Stardust.Node.Helpers
{
    public class ManagerUriBuilderHelper
    {
        private UriBuilder UriBuilder { get; set; }

        private UriBuilder UriTemplateBuilder { get; set; }

        public ManagerUriBuilderHelper(Uri locationUri) : this(locationUri.ToString())
        {
        }

        public ManagerUriBuilderHelper(string location)
        {
            location.ThrowArgumentNullExceptionIfNullOrEmpty();

            UriBuilder = new UriBuilder(location);

            UriTemplateBuilder = new UriBuilder(location);

            UriBuilder.Scheme.ThrowArgumentNullExceptionIfNullOrEmpty();
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

        public Uri GetJobProgressTemplateUri()
        {
            return CreateUri(ManagerRouteConstants.JobProgress);
        }

        public Uri GetHeartbeatTemplateUri()
        {
            return CreateUri(ManagerRouteConstants.Heartbeat);
        }

        public Uri GetNodeHasBeenInitializedTemplateUri()
        {
            return CreateUri(ManagerRouteConstants.NodeHasBeenInitialized);
        }

        public Uri GetJobHasFailedTemplateUri()
        {
            return CreateUri(ManagerRouteConstants.JobFailed);
        }

        public Uri GetJobHasFailedUri(Guid guid)
        {
            string path = ManagerRouteConstants.JobFailed.Replace(ManagerRouteConstants.JobIdOptionalParameter,
                                                                  guid.ToString());

            return CreateUri(path);
        }

        public Uri GetJobHasBeenCanceledTemplateUri()
        {
            return CreateUri(ManagerRouteConstants.JobHasBeenCanceled);
        }

        public Uri GetJobHasBeenCanceledUri(Guid guid)
        {
            string path = ManagerRouteConstants.JobHasBeenCanceled.Replace(ManagerRouteConstants.JobIdOptionalParameter,
                                                                           guid.ToString());

            return CreateUri(path);
        }

        public Uri GetJobDoneTemplateUri()
        {
            return CreateUri(ManagerRouteConstants.JobDone);
        }

        public Uri GetJobDoneUri(Guid guid)
        {
            string path = ManagerRouteConstants.JobDone.Replace(ManagerRouteConstants.JobIdOptionalParameter,
                                                                guid.ToString());

            return CreateUri(path);
        }

        public Uri CreateUri(string path)
        {
            UriBuilder.Path = UriTemplateBuilder.Path;

            UriBuilder.Path += path;

            return UriBuilder.Uri;
        }
    }
}