using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Web.TestApplicationsCommon;

namespace Teleopti.Ccc.Web.Loadtest
{
	class Program
	{
		private static object Lock = new object();
		static void Main(string[] args)
		{
			RunReportsTest();
		}

		private static void RunReportsTest()
		{
			var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
			var businessUnitName = ConfigurationManager.AppSettings["BusinessUnitName"];
			double totalElapsedMilliseconds = 0;

			Task.WaitAll(UserData.TestUsers.Select(user => Task.Run(() =>
			{
				using (var trafficSimulator = new SimulateReportsTraffic())
				{
					trafficSimulator.Start(baseUrl, businessUnitName, user.Username, user.Password);
					var timeTaken = trafficSimulator.GoToReportsController();
					lock (Lock)
					{
						totalElapsedMilliseconds += timeTaken.TotalMilliseconds;
					}
				}
			})).ToArray());
			Console.WriteLine("Total milliseconds spend loading reports: {0}", totalElapsedMilliseconds);
			Console.WriteLine("Average: {0}", totalElapsedMilliseconds / UserData.TestUsers.Count);
			Console.ReadKey();
		}
	}
}
