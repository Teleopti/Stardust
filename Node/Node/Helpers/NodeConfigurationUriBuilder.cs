using Stardust.Node.Interfaces;

namespace Stardust.Node.Helpers
{
    public class NodeConfigurationUriBuilder
    {
        public ManagerUriBuilder ManagerUriBuilder { get; set; }

        public NodeUriBuilder NodeUriBuilder { get; set; }

        public NodeConfigurationUriBuilder(INodeConfiguration configuration)
        {
            ManagerUriBuilder = new ManagerUriBuilder(configuration.ManagerLocation);

            NodeUriBuilder = new NodeUriBuilder(configuration.BaseAddress);
        }
    }
}