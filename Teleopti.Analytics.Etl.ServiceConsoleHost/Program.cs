using System;
using Teleopti.Analytics.Etl.Common.Service;

namespace Teleopti.Analytics.Etl.ServiceConsoleHost
{
	class Program
	{
		static void Main(string[] args)
		{
			var host = new EtlServiceHost();
			host.Start(() => {});

			Console.WriteLine("ETL Service is now running, press Enter to stop...");
			Console.ReadLine();

			host.Dispose();

			Console.WriteLine("ETL Service is stopped, press Enter to close...");
			Console.ReadLine();
		}
	}
}
