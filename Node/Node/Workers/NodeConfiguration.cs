using System;
using System.Reflection;

namespace Stardust.Node.Workers
{
	public class NodeConfiguration
	{
		public NodeConfiguration(Uri baseAddress,
		                         Uri managerLocation,
		                         Assembly handlerAssembly,
		                         string nodeName,
		                         double pingToManagerSeconds)
		{
			if (baseAddress == null)
			{
				throw new ArgumentNullException("baseAddress");
			}

			if (managerLocation == null)
			{
				throw new ArgumentNullException("managerLocation");
			}

			if (handlerAssembly == null)
			{
				throw new ArgumentNullException("handlerAssembly");
			}

			if (string.IsNullOrEmpty(nodeName))
			{
				throw new ArgumentNullException("nodeName");
			}

			BaseAddress = baseAddress;
			ManagerLocation = managerLocation;
			HandlerAssembly = handlerAssembly;
			NodeName = nodeName;

			PingToManagerSeconds = pingToManagerSeconds;
		}

		public double PingToManagerSeconds { get; set; }
		public Uri BaseAddress { get; private set; }
		public Uri ManagerLocation { get; private set; }
		public string NodeName { get; private set; }
		public Assembly HandlerAssembly { get; private set; }
	}
}