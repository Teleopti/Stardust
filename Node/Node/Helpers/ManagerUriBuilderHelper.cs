using System;
using Stardust.Node.Constants;
using Stardust.Node.Extensions;

namespace Stardust.Node.Helpers
{
	public class ManagerUriBuilderHelper
	{
		public ManagerUriBuilderHelper(Uri locationUri) : this(locationUri.ToString())
		{
		}

		public ManagerUriBuilderHelper(string location)
		{
			UriBuilder = new UriBuilder(location);
			UriTemplateBuilder = new UriBuilder(location);
		}

		private UriBuilder UriBuilder { get; set; }

		private UriBuilder UriTemplateBuilder { get; set; }


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

		public Uri GetJobHasBeenCanceledTemplateUri()
		{
			return CreateUri(ManagerRouteConstants.JobHasBeenCanceled);
		}

		public Uri GetJobDoneTemplateUri()
		{
			return CreateUri(ManagerRouteConstants.JobDone);
		}

		public Uri CreateUri(string path)
		{
			UriBuilder.Path = UriTemplateBuilder.Path;

			UriBuilder.Path += path;

			return UriBuilder.Uri;
		}
	}
}