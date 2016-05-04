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
			ValidateParameters();
		}

		public double PingToManagerSeconds { get; private set; }
		public Uri BaseAddress { get; private set; }
		public Uri ManagerLocation { get; private set; }
		public string NodeName { get; private set; }
		public Assembly HandlerAssembly { get; private set; }

		private void ValidateParameters()
		{
			if (BaseAddress == null)
			{
				throw new ArgumentNullException("baseAddress");
			}
			if (ManagerLocation == null)
			{
				throw new ArgumentNullException("managerLocation");
			}
			if (HandlerAssembly == null)
			{
				throw new ArgumentNullException("handlerAssembly");
			}
			if (string.IsNullOrEmpty(NodeName))
			{
				throw new ArgumentNullException("nodeName");
			}
			if (PingToManagerSeconds <= 0)
			{
				throw new ArgumentNullException("pingToManagerSeconds");
			}
		}
	}
}