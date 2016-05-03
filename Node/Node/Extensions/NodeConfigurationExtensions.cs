﻿using System;
using Stardust.Node.Helpers;
using Stardust.Node.Workers;

namespace Stardust.Node.Extensions
{
	public static class NodeConfigurationExtensions
	{
		public static void ThrowArgumentNullException(this NodeConfiguration nodeConfiguration)
		{
			if (nodeConfiguration == null)
			{
				throw new ArgumentNullException("nodeConfiguration");
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
			nodeConfiguration.ThrowArgumentNullException();

			var nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

			var ret =
				nodeConfigurationUriBuilder.ManagerUriBuilder.GetNodeHasBeenInitializedTemplateUri();

			return ret;
		}

		public static Uri GetManagerNodeHeartbeatUri(this NodeConfiguration nodeConfiguration)
		{
			nodeConfiguration.ThrowArgumentNullException();

			var nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

			return nodeConfigurationUriBuilder.ManagerUriBuilder.GetHeartbeatTemplateUri();
		}

		public static Uri GetManagerJobHasBeenCanceledTemplateUri(this NodeConfiguration nodeConfiguration)
		{
			nodeConfiguration.ThrowArgumentNullException();

			var nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

			return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobHasBeenCanceledTemplateUri();
		}

		public static Uri GetManagerJobHasFaileTemplatedUri(this NodeConfiguration nodeConfiguration)
		{
			nodeConfiguration.ThrowArgumentNullException();

			var nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

			return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobHasFailedTemplateUri();
		}

		public static Uri GetManagerJobDoneTemplateUri(this NodeConfiguration nodeConfiguration)
		{
			nodeConfiguration.ThrowArgumentNullException();

			var nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

			return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobDoneTemplateUri();
		}

	}
}