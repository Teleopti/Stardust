using System;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Ccc.Sdk.ServiceBus.ConsoleHost
{
	class Program
	{
		static void Main(string[] args)
		{
			var host = new ServiceBusRunner(i => { }, new WfmInstallationEnvironment());
			host.Start();

			Console.WriteLine("Service bus is now running, press Enter to stop...");
			Console.ReadLine();

			host.Stop();

			Console.WriteLine("Service bus is stopped, press Enter to close...");
			Console.ReadLine();
		}
	}
}
