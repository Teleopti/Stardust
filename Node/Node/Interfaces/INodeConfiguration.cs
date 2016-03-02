using System;
using System.Reflection;

namespace Stardust.Node.Interfaces
{
	public interface INodeConfiguration
	{
		double PingToManagerIdleDelaySeconds { get; set; }

		double PingToManagerRunningDelaySeconds { get; set; }

		Uri BaseAddress { get; }
		Assembly HandlerAssembly { get; }
		Uri ManagerLocation { get; }
		string NodeName { get; }
	}
}