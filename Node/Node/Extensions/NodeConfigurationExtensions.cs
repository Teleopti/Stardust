using System;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Extensions
{
    public static class NodeConfigurationExtensions
    {
        public static void ThrowArgumentNullException(this INodeConfiguration nodeConfiguration)
        {
            if (nodeConfiguration == null)
            {
                throw new ArgumentNullException("nodeConfiguration");
            }
        }

        public static string CreateWhoIAm(this INodeConfiguration nodeConfiguration,
                                          string machineName)
        {
            var nodeName = "Missing Node Name";
            var machine = "Missing Machine Name";

            if (nodeConfiguration != null && !nodeConfiguration.NodeName.IsNullOrEmpty())
            {
                nodeName = nodeConfiguration.NodeName;
            }

            if (!machineName.IsNullOrEmpty())
            {
                machine = machineName;
            }

            return "[" + nodeName.ToUpper() + ", " + machine.ToUpper() + "]";
        }

        public static Uri GetManagerJobDoneUri(this INodeConfiguration nodeConfiguration,
                                               Guid guid)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilderHelper nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobDoneUri(guid);
        }

        public static Uri GetManagerNodeHasBeenInitializedUri(this INodeConfiguration nodeConfiguration)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilderHelper nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetNodeHasBeenInitializedTemplateUri();
        }

        public static Uri GetManagerNodeHeartbeatUri(this INodeConfiguration nodeConfiguration)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilderHelper nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetHeartbeatTemplateUri();
        }

        public static Uri GetManagerJobHasBeenCanceledTemplateUri(this INodeConfiguration nodeConfiguration)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilderHelper nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobHasBeenCanceledTemplateUri();
        }

        public static Uri GetManagerJobHasBeenCanceledUri(this INodeConfiguration nodeConfiguration,
                                                          Guid guid)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilderHelper nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobHasBeenCanceledUri(guid);
        }

        public static Uri GetManagerJobHasFaileTemplatedUri(this INodeConfiguration nodeConfiguration)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilderHelper nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobHasFailedTemplateUri();
        }

        public static Uri GetManagerJobDoneTemplateUri(this INodeConfiguration nodeConfiguration)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilderHelper nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobDoneTemplateUri();
        }

        public static Uri GetManagerLocationUri(this INodeConfiguration nodeConfiguration)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilderHelper nodeConfigurationUriBuilder = new NodeConfigurationUriBuilderHelper(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetLocationUri();
        }
    }
}