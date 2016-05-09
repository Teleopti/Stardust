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
		static void Main(string[] args)
		{
			//RunReportsTest();
			RunMyTimeScheduleTest();
		}

		private static void RunMyTimeScheduleTest()
		{
			var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
			var businessUnitName = ConfigurationManager.AppSettings["BusinessUnitName"];
			var users = UserData.TestUsers;
			var myTimeScheduleTest = new LoadMyTimeScheduleTest(baseUrl, businessUnitName);
			var result=new List<List<MyTimeScheduleTimingData>>();
			var times = 1000;
			for (int i = 0; i < times; i++)
			{
				var runAsync = myTimeScheduleTest.RunAsync(users, 100);
				result.Add(runAsync.Result);
			}

			var loginTotalElapsedMilliseconds = result.Sum(x => x.Sum(a=>a.Logon));
			var totalElapsedMilliseconds = result.Sum(x => x.Sum(a=>a.Schedule));
			var totalErrors = result.Sum(x => x.Sum(a=>a.Errors));
			Console.WriteLine("Total users: {0}", users.Count*times - totalErrors);
			Console.WriteLine("Total spend logging in: {0} ms", loginTotalElapsedMilliseconds);
			Console.WriteLine("Total erorrs: {0}", totalErrors);
			Console.WriteLine("Average/login: {0} ms", loginTotalElapsedMilliseconds / (users.Count * times - totalErrors));
			Console.WriteLine("Total spend loading mytime schedule: {0} ms", totalElapsedMilliseconds);
			Console.WriteLine("Average/schedule: {0} ms", totalElapsedMilliseconds / (users.Count * times - totalErrors));
			Console.ReadKey();
		}

		private static void RunReportsTest()
		{
			var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
			var businessUnitName = ConfigurationManager.AppSettings["BusinessUnitName"];
			var users = UserData.GenerateTestUsers(100);
			var reportsTest = new LoadReportsTest(baseUrl, businessUnitName);
			var result = reportsTest.RunAsync(users, 100).Result;
			var loginTotalElapsedMilliseconds = result.Sum(x => x.Logon);
			var totalElapsedMilliseconds = result.Sum(x => x.Reports);
			Console.WriteLine("Total users: {0}", users.Count);
			Console.WriteLine("Total spend logging in: {0} ms", loginTotalElapsedMilliseconds);
			Console.WriteLine("Average/login: {0} ms", loginTotalElapsedMilliseconds / users.Count);
			Console.WriteLine("Total spend loading reports: {0} ms", totalElapsedMilliseconds);
			Console.WriteLine("Average/report: {0} ms", totalElapsedMilliseconds / users.Count);
			Console.ReadKey();
		}
	}

	
}
