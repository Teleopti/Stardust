using System;
using Manager.Integration.Test.Constants;
using Manager.Integration.Test.Properties;

namespace Manager.Integration.Test.Helpers
{
	public class ManagerUriBuilder
	{
		private readonly Uri _managerLocationUri;
		private readonly UriBuilder _uriBuilder;

		private readonly UriBuilder _uriTemplateBuilder;

		public ManagerUriBuilder()
		{
			_managerLocationUri = new Uri(Settings.Default.ManagerLocationUri);

			_uriBuilder = new UriBuilder(_managerLocationUri);

			_uriTemplateBuilder = new UriBuilder(_managerLocationUri);
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
			var path = ManagerRouteConstants.GetJobHistory.Replace(ManagerRouteConstants.JobIdOptionalParameter,
			                                                       guid.ToString());

			return CreateUri(path);
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