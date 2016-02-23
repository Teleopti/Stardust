using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Helpers
{
	public class NodeConfigurationUriBuilderHelper
	{
		public NodeConfigurationUriBuilderHelper(INodeConfiguration configuration)
		{
			configuration.ThrowArgumentNullException();

			ManagerUriBuilder = new ManagerUriBuilderHelper(configuration.ManagerLocation);

			NodeUriBuilder = new NodeUriBuilderHelper(configuration.BaseAddress);
		}

		public ManagerUriBuilderHelper ManagerUriBuilder { get; set; }

		public NodeUriBuilderHelper NodeUriBuilder { get; set; }
	}
}