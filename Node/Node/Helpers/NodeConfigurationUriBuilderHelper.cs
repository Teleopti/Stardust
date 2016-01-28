using Stardust.Node.Interfaces;

namespace Stardust.Node.Helpers
{
    public class NodeConfigurationUriBuilderHelper
    {
        public ManagerUriBuilderHelper ManagerUriBuilder { get; set; }

        public NodeUriBuilderHelper NodeUriBuilder { get; set; }

        public NodeConfigurationUriBuilderHelper(INodeConfiguration configuration)
        {
            ManagerUriBuilder = new ManagerUriBuilderHelper(configuration.ManagerLocation);

            NodeUriBuilder = new NodeUriBuilderHelper(configuration.BaseAddress);
        }
    }
}