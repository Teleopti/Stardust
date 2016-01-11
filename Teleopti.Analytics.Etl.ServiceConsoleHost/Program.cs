using System;
using Teleopti.Analytics.Etl.ServiceLogic;


namespace Teleopti.Analytics.Etl.ServiceConsoleHost
{
	class Program
	{
		static void Main(string[] args)
		{
			var service = new EtlService();
			service.Start(() => {});

			Console.WriteLine("ETL Service is now running, press Enter to stop...");
			Console.ReadLine();

			service.Dispose();

			Console.WriteLine("ETL Service is stopped, press Enter to close...");
			Console.ReadLine();
		}
	}
}
