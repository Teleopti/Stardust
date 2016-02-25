using System;
using System.Reflection;

namespace Stardust.Node.Interfaces
{
	public interface INodeConfiguration
	{
		double PingToManagerIdleDelay { get; set; }

		double PingToManagerRunningDelay { get; set; }

		Uri BaseAddress { get; }
		Assembly HandlerAssembly { get; }
		Uri ManagerLocation { get; }
		string NodeName { get; }
	}
}