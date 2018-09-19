using System;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Service;

namespace Teleopti.Analytics.Etl.ServiceConsoleHost
{
	public class Program
	{
		public static void Main(string[] args)
		{
			if (args.FirstOrDefault() == "/EnsureRecurringJobs")
				using (var host = new EtlServiceHost())
				{
					Console.WriteLine("ETL Service is simply ensuring recurring jobs and then closing...");					
					host.SimplyEnsureRecurringJobs();
				}
			else
			{
				using (var host = new EtlServiceHost())
				{
					host.Start(() => { });
					Console.WriteLine("ETL Service is now running, press Enter to stop...");
					Console.ReadLine();
				}
				Console.WriteLine("ETL Service is stopped, press Enter to close...");
				Console.ReadLine();
			}
		}
	}
}