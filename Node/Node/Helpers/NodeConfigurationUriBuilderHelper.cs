namespace Stardust.Node.Helpers
{
	public class NodeConfigurationUriBuilderHelper
	{
		public NodeConfigurationUriBuilderHelper(NodeConfiguration configuration)
		{
			ManagerUriBuilder = new ManagerUriBuilderHelper(configuration.ManagerLocation);
		}

		public ManagerUriBuilderHelper ManagerUriBuilder { get; set; }
	}
}