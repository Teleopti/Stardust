using System;
using Stardust.Manager.Constants;

namespace Stardust.Manager.Helpers
{
	public class NodeUriBuilderHelper
	{
		public NodeUriBuilderHelper(Uri locationUri) : this(locationUri.ToString())
		{
		}

		public NodeUriBuilderHelper(string locationUri)
		{
			if (string.IsNullOrEmpty(locationUri))
			{
				throw new UriFormatException();
			}
			LocationUri = new Uri(locationUri);

			if (string.IsNullOrEmpty(LocationUri.Scheme))
			{
				throw new ArgumentNullException();
			}
			UriTemplateBuilder = new UriBuilder(LocationUri);
			UriBuilder = new UriBuilder(LocationUri);
		}

		private UriBuilder UriBuilder { get; set; }

		private Uri LocationUri { get; set; }

		private UriBuilder UriTemplateBuilder { get; set; }



		public Uri GetIsIdleTemplateUri()
		{
			return CreateUri(NodeRouteConstants.IsIdle);
		}

		public Uri GetJobTemplateUri()
		{
			return CreateUri(NodeRouteConstants.Job);
		}

		public Uri GetPingTemplateUri()
		{
			return CreateUri(NodeRouteConstants.IsAlive);
		}

		public Uri GetUpdateJobUri(Guid jobId)
		{
			var path = NodeRouteConstants.UpdateJobByJobId.Replace(NodeRouteConstants.JobIdOptionalParameter,
															jobId.ToString());
			return CreateUri(path);
		}

		public Uri GetCancelJobUri(Guid jobId)
		{
			var path = NodeRouteConstants.CancelJobByJobId.Replace(NodeRouteConstants.JobIdOptionalParameter,
			                                                jobId.ToString());
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