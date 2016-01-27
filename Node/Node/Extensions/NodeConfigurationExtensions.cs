using System;
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

        public static string CreateWhoIAm(this INodeConfiguration nodeConfiguration, string machineName)
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


        public static Uri GenerateUri(this INodeConfiguration nodeConfiguration,
            string extraResourceInformation = null)
        {
            nodeConfiguration.ThrowArgumentNullException();
            nodeConfiguration.ManagerLocation.ThrowArgumentExceptionWhenNull();

            if (extraResourceInformation != null)
            {
                return new Uri(nodeConfiguration.ManagerLocation + extraResourceInformation);
            }

            return null;
        }
    }
}