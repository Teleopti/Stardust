using System;
using System.Reflection;
using Stardust.Node.Interfaces;

namespace Stardust.Node.API
{
	public class NodeConfiguration : INodeConfiguration
	{
		public NodeConfiguration(Uri baseAddress,
		                         Uri managerLocation,
		                         Assembly handlerAssembly,
		                         string nodeName,
		                         double pingToManagerIdleDelaySeconds,
		                         double pingToManagerRunningDelaySeconds)
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

			PingToManagerIdleDelay = pingToManagerIdleDelaySeconds;
			PingToManagerRunningDelay = pingToManagerRunningDelaySeconds;
		}

		public double PingToManagerIdleDelay { get; set; }
		public double PingToManagerRunningDelay { get; set; }
		public Uri BaseAddress { get; private set; }
		public Uri ManagerLocation { get; private set; }
		public string NodeName { get; private set; }
		public Assembly HandlerAssembly { get; private set; }
	}
}