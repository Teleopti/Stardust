using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Rhino.ServiceBus.Hosting;

namespace Teleopti.Ccc.Sdk.ServiceBus.ConsoleHost
{
	class Program
	{
		private static RemoteAppDomainHost _host;

		static void Main(string[] args)
		{
			Console.WriteLine("Starting Service Bus... (be ready for your head to explode)");
			_host = new RemoteAppDomainHost(typeof(RequestBootStrapper))
				.Configuration("Teleopti.Ccc.Sdk.ServiceBus.ConsoleHost.exe.config");
			_host.Start();
			Thread.Sleep(10000);
			Console.WriteLine("Started!");
			while (true)
			{
				Console.Write("[C|Q]>");
				var key = Console.ReadKey();
				Console.WriteLine();
				if (key.Key == ConsoleKey.C)
				{
					Console.Clear();
					continue;
				}
				if (key.Key == ConsoleKey.Q)
					break;
			}
			Console.WriteLine("Closing...");
			_host.Close();
			Console.WriteLine("Bye bye...");
		}
	}
}
