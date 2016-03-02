using System;
using System.Reflection;
using Stardust.Node.Interfaces;

namespace NodeTest.Fakes.InvokeHandlers
{
	public class NodeConfigurationFake : INodeConfiguration
	{
		public NodeConfigurationFake(Uri baseAddress,
		                             Uri managerLocation,
		                             Assembly handlerAssembly,
		                             string nodeName,
		                             double pingToManagerIdleDelay,
		                             double pingToManagerRunningDelay)

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

			PingToManagerIdleDelaySeconds = PingToManagerIdleDelaySeconds;
			PingToManagerRunningDelaySeconds = PingToManagerRunningDelaySeconds;
		}

		public double PingToManagerIdleDelaySeconds { get; set; }
		public double PingToManagerRunningDelaySeconds { get; set; }
		public Uri BaseAddress { get; private set; }
		public Assembly HandlerAssembly { get; private set; }
		public Uri ManagerLocation { get; private set; }
		public string NodeName { get; private set; }
	}
}