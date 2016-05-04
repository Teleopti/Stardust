using System;
using System.Reflection;

namespace Stardust.Node
{
	public class NodeConfiguration
	{
		public NodeConfiguration(Uri baseAddress,
		                         Uri managerLocation,
		                         Assembly handlerAssembly,
		                         string nodeName,
		                         double pingToManagerSeconds)
		{
			BaseAddress = baseAddress;
			ManagerLocation = managerLocation;
			HandlerAssembly = handlerAssembly;
			NodeName = nodeName;

			PingToManagerSeconds = pingToManagerSeconds;
		}

		public double PingToManagerSeconds { get; private set; }
		public Uri BaseAddress { get; private set; }
		public Uri ManagerLocation { get; private set; }
		public string NodeName { get; private set; }
		public Assembly HandlerAssembly { get; private set; }
	}
}