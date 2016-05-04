using System;
using Stardust.Node.Helpers;

namespace Stardust.Node.Extensions
{
	public static class NodeConfigurationExtensions
	{

		public static void ValidateParameters(this NodeConfiguration nodeConfiguration)
		{
			if (nodeConfiguration == null)
			{
				throw new ArgumentNullException("nodeConfiguration");
			}
			if (nodeConfiguration.BaseAddress == null)
			{
				throw new ArgumentNullException("baseAddress");
			}

			if (nodeConfiguration.ManagerLocation == null)
			{
				throw new ArgumentNullException("managerLocation");
			}

			if (nodeConfiguration.HandlerAssembly == null)
			{
				throw new ArgumentNullException("handlerAssembly");
			}

			if (string.IsNullOrEmpty(nodeConfiguration.NodeName))
			{
				throw new ArgumentNullException("nodeName");
			}
		}

		public static string CreateWhoIAm(this NodeConfiguration nodeConfiguration,
		                                  string machineName)
		{
			var nodeName = "Missing Node Name";
			var machine = "Missing Machine Name";

			if (nodeConfiguration != null && !string.IsNullOrEmpty(nodeConfiguration.NodeName))
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

			var nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

			var ret =
				nodeConfigurationUriBuilder.ManagerUriBuilder.GetNodeHasBeenInitializedTemplateUri();

			return ret;
		}

		public static Uri GetManagerNodeHeartbeatUri(this NodeConfiguration nodeConfiguration)
		{

			var nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

			return nodeConfigurationUriBuilder.ManagerUriBuilder.GetHeartbeatTemplateUri();
		}

		public static Uri GetManagerJobHasBeenCanceledTemplateUri(this NodeConfiguration nodeConfiguration)
		{
			var nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

			return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobHasBeenCanceledTemplateUri();
		}

		public static Uri GetManagerJobHasFaileTemplatedUri(this NodeConfiguration nodeConfiguration)
		{
			var nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

			return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobHasFailedTemplateUri();
		}

		public static Uri GetManagerJobDoneTemplateUri(this NodeConfiguration nodeConfiguration)
		{
			var nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

			return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobDoneTemplateUri();
		}

	}
}