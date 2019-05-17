using System;
using Stardust.Node.Constants;

namespace Stardust.Core.Node.Extensions
{
	public static class NodeConfigurationExtensions
	{
		public static string CreateWhoIAm(this NodeConfiguration nodeConfiguration,
		                                  string machineName)
		{
			var nodeName = "Missing Node Name";
			var machine = "Missing Machine Name";

			if (!string.IsNullOrEmpty(nodeConfiguration?.NodeName))
			{
				nodeName = nodeConfiguration.NodeName;
			}

			if (!string.IsNullOrEmpty(machineName))
			{
				machine = machineName;
			}

			return "[" + nodeName.ToUpper() + ", " + machine.ToUpper() + "]";
		}


		public static Uri GetManagerNodeHasBeenInitializedUri(this NodeConfiguration nodeConfiguration)
		{
			return CreateUri(nodeConfiguration.ManagerLocation, ManagerRouteConstants.NodeHasBeenInitialized);
		}

		public static Uri GetManagerNodeHeartbeatUri(this NodeConfiguration nodeConfiguration)
		{
			return CreateUri(nodeConfiguration.ManagerLocation, ManagerRouteConstants.Heartbeat);
		}

		public static Uri GetManagerJobHasBeenCanceledTemplateUri(this NodeConfiguration nodeConfiguration)
		{
			return CreateUri(nodeConfiguration.ManagerLocation, ManagerRouteConstants.JobHasBeenCanceled);
		}

		public static Uri GetManagerJobHasFailedTemplatedUri(this NodeConfiguration nodeConfiguration)
		{
			return CreateUri(nodeConfiguration.ManagerLocation, ManagerRouteConstants.JobFailed);
		}

		public static Uri GetManagerJobDoneTemplateUri(this NodeConfiguration nodeConfiguration)
		{
			return CreateUri(nodeConfiguration.ManagerLocation, ManagerRouteConstants.JobDone);
		}

		private static Uri CreateUri(Uri location, string path)
		{
			var uriBuilder = new UriBuilder(location);
			uriBuilder.Path += path;

			return uriBuilder.Uri;
		}
	}
}