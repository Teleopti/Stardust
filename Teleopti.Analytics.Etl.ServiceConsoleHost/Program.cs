﻿using System;
using Teleopti.Analytics.Etl.ServiceLogic;


namespace Teleopti.Analytics.Etl.ServiceConsoleHost
{
	class Program
	{
		static void Main(string[] args)
		{
			var host = new EtlJobStarter(new EtlConfigReader());
			host.Start();

			Console.WriteLine("ETL Service is now running, press Enter to stop...");
			Console.ReadLine();

			host.Dispose();

			Console.WriteLine("ETL Service is stopped, press Enter to close...");
			Console.ReadLine();
		}
	}
}
