using System;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Properties;

namespace Manager.Integration.Test.Helpers
{
    public class ManagerUriBuilder
    {
        private UriBuilder _uriBuilder;

        private Uri _managerLocationUri;

        public ManagerUriBuilder()
        {
            _managerLocationUri = new Uri(Settings.Default.ManagerLocationUri);

            _uriBuilder = new UriBuilder
            {
                Host = _managerLocationUri.Host,
                Port = _managerLocationUri.Port,
                Scheme = _managerLocationUri.Scheme
            };
        }

        public Uri GetLocationUri()
        {
            return _uriBuilder.Uri;
        }

        public Uri GetStartJobUri()
        {
            return CreateUri(ManagerRouteConstants.Job);
        }

        public Uri GetJobHistoryUri(Guid guid)
        {
            string path = string.Format(ManagerRouteConstants.GetJobHistory.Replace("{jobId}",
                                                                                    "{0}"),
                                        guid);

            return CreateUri(path);
        }


        public Uri GetCancelJobUri(Guid guid)
        {
            string path = string.Format(ManagerRouteConstants.CancelJob.Replace("{jobId}",
                                                                                "{0}"),
                                        guid);

            return CreateUri(path);
        }

        public Uri CreateUri(string path)
        {
            _uriBuilder.Path = path;

            return _uriBuilder.Uri;
        }
    }
}