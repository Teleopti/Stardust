using System.Collections.Generic;

namespace Stardust.Core.Node
{
	public class NodeConfigurationService
	{
		private readonly Dictionary<int, NodeConfiguration> _configurationsPerPort = new Dictionary<int, NodeConfiguration>();

		public void AddConfiguration(int port, NodeConfiguration nodeConfiguration)
		{
			_configurationsPerPort.Add(port,nodeConfiguration);
		}

		public NodeConfiguration GetConfigurationForPort(int port)
		{
			return _configurationsPerPort[port];
		}
	}
}
