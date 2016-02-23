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
			UriBuilder.Path = UriTemplateBuilder.Path;

			UriBuilder.Path += path;

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