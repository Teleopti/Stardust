using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Stardust.Node
{
	public class NodeConfigurationService
	{
		private readonly ConcurrentDictionary<int, NodeConfiguration> _configurationsPerPort = new ConcurrentDictionary<int, NodeConfiguration>();

		public void AddConfiguration(int port, NodeConfiguration nodeConfiguration)
		{
			_configurationsPerPort.TryAdd(port,nodeConfiguration);
		}

		public NodeConfiguration GetConfigurationForPort(int port)
		{
			return _configurationsPerPort[port];
		}
	}
}
