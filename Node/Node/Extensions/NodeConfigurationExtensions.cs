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

            NodeConfigurationUriBuilder nodeConfigurationUriBuilder = new NodeConfigurationUriBuilder(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobDoneUri(guid);
        }

        public static Uri GetManagerNodeHasBeenInitializedUri(this INodeConfiguration nodeConfiguration)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilder nodeConfigurationUriBuilder = new NodeConfigurationUriBuilder(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetNodeHasBeenInitializedUri();
        }

        public static Uri GetManagerNodeHeartbeatUri(this INodeConfiguration nodeConfiguration)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilder nodeConfigurationUriBuilder = new NodeConfigurationUriBuilder(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetHeartbeatUri();
        }

        public static Uri GetManagerJobHasBeenCanceledUri(this INodeConfiguration nodeConfiguration)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilder nodeConfigurationUriBuilder = new NodeConfigurationUriBuilder(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobHasBeenCanceledUri();
        }

        public static Uri GetManagerJobHasBeenCanceledUri(this INodeConfiguration nodeConfiguration,
                                                          Guid guid)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilder nodeConfigurationUriBuilder = new NodeConfigurationUriBuilder(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobHasBeenCanceledUri(guid);
        }

        public static Uri GetManagerJobHasFailedUri(this INodeConfiguration nodeConfiguration)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilder nodeConfigurationUriBuilder = new NodeConfigurationUriBuilder(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobHasFailedUri();
        }

        public static Uri GetManagerJobDoneUri(this INodeConfiguration nodeConfiguration)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilder nodeConfigurationUriBuilder = new NodeConfigurationUriBuilder(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetJobDoneUri();
        }

        public static Uri GetManagerLocationUri(this INodeConfiguration nodeConfiguration)
        {
            nodeConfiguration.ThrowArgumentNullException();

            NodeConfigurationUriBuilder nodeConfigurationUriBuilder = new NodeConfigurationUriBuilder(nodeConfiguration);

            return nodeConfigurationUriBuilder.ManagerUriBuilder.GetLocationUri();
        }
    }
}