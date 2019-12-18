using System;
using Manager.IntegrationTest.Constants;
using Manager.IntegrationTest.Properties;

namespace Manager.IntegrationTest.Helpers
{
	public class ManagerUriBuilder
	{
		private readonly UriBuilder _uriBuilder;

		private readonly UriBuilder _uriTemplateBuilder;

		public ManagerUriBuilder()
		{
			var managerLocationUri = new Uri(Settings.Default.ManagerLocationUri);

			_uriBuilder = new UriBuilder(managerLocationUri);
			_uriTemplateBuilder = new UriBuilder(managerLocationUri);
		}

		public Uri GetPingUri()
		{
			return CreateUri(ManagerRouteConstants.Ping);
		}

		public Uri GetAddToJobQueueUri()
		{
			return CreateUri(ManagerRouteConstants.Job);
		}

		public Uri GetJobHistoryUri(Guid guid)
		{
			var path = ManagerRouteConstants.GetJobHistory.Replace(ManagerRouteConstants.JobIdOptionalParameter,
			                                                       guid.ToString());

			return CreateUri(path);
		}

		public Uri GetNodesUri()
		{
			return CreateUri(ManagerRouteConstants.Nodes);
		}


		public Uri GetCancelJobUri(Guid guid)
		{
			var path = ManagerRouteConstants.CancelJob.Replace(ManagerRouteConstants.JobIdOptionalParameter,
			                                                   guid.ToString());

			return CreateUri(path);
		}

		public Uri CreateUri(string path)
		{
			_uriBuilder.Path = _uriTemplateBuilder.Path;

			_uriBuilder.Path += path;

			return _uriBuilder.Uri;
		}
	}
}