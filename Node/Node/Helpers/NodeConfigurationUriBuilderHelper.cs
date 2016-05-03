using Stardust.Node.Extensions;
using Stardust.Node.Workers;

namespace Stardust.Node.Helpers
{
	public class NodeConfigurationUriBuilderHelper
	{
		public NodeConfigurationUriBuilderHelper(NodeConfiguration configuration)
		{
			configuration.ThrowArgumentNullException();

			ManagerUriBuilder = new ManagerUriBuilderHelper(configuration.ManagerLocation);
		}

		public ManagerUriBuilderHelper ManagerUriBuilder { get; set; }
	}
}